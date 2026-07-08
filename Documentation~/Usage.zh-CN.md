# Sigil AI — 使用文档

[English](Usage.md) | [简体中文](Usage.zh-CN.md)

> NPC 行为 **配套包** `com.likeon.gas.ai`。依赖 Sigil 核心 `com.likeon.gas`（标签）与
> `com.likeon.narrative`（事件桥）；移动用内置 NavMesh。命名空间 `Likeon.GAS.AI`。
>
> 设计立场：AI 框架（goal / activity / schedule / 打分）是建在 GameplayTag 状态总线之上的纯逻辑。
> 时间驱动 goal、goal 按分数竞争、赢家去跑。是把 Narrative Pro / Narrative Arsenal（设计 by
> Narrative Tools）的 AI/Activities 系统重新设计到 Unity。

## 目录

1. [安装](#1-安装)
2. [一天时钟](#2-一天时钟)
3. [Goal 与打分](#3-goal-与打分)
4. [作息表与多 goal 竞争](#4-作息表与多-goal-竞争)
5. [Activity 与选择器](#5-activity-与选择器)
6. [移动（MoveToActivity / INpcAgent）](#6-移动movetoactivity--inpcagent)
7. [narrative 桥（AddGoalToNpc）](#7-narrative-桥addgoaltonpc)
8. [大脑组件](#8-大脑组件)

---

## 1. 安装

依次装 `com.likeon.gas`、`com.likeon.narrative`、`com.likeon.gas.ai`（Package Manager →
*Add package from disk…*）。跑测试：在 `Packages/manifest.json` 的 `"testables"` 里加 `"com.likeon.gas.ai"`。

## 2. 一天时钟

`TimeOfDayClock` 是 UE 0–2400 制的纯逻辑时钟（800 = 8:00、1200 = 正午、2100 = 21:00）。
宿主推进它（现实秒 → 游戏时间），作息/打分读它。

```csharp
var clock = new TimeOfDayClock(initialTimeOfDay: 800f); // 08:00
clock.Advance(400f);         // +4「小时」→ 1200（正午）
float now = clock.TimeOfDay;  // 1200，落在 [0,2400)
int day = clock.Day;          // 已过天数（AccumulatedTime / 2400）
```

`ITimeOfDay` 暴露 `TimeOfDay`（到 2400 回绕）与 `AccumulatedTime`（不回绕）。

## 3. Goal 与打分

goal 是"意图"。`MoveGoal`（M-AI-1 的具体 goal）带一个目标点。

```csharp
var goal = new MoveGoal(destination, score: 300f)
{
    GoalTag  = GameplayTag.RequestTag("Goal.Move.Eat"),
    GoalKey  = someKey,   // 可选去重键（默认用实例自身）
    GoalLifetime = 0f,    // <= 0 = 永不过期
};
```

- `GetScore()` — 默认返回 `DefaultScore`；可 override 做情境打分（如"饿了吃饭分更高"）。
- `PassesTagGate(ownerTags)` — owner 命中任一 `BlockTags` → false；缺任一 `RequireTags` → false；否则 true。
- `IsExpired(currentAccumulatedTime)` — 自 goal 的 `CreationTime` 起过了 `Lifetime` 即 true。

## 4. 作息表与多 goal 竞争

`ScheduleEntry` 是一个提供 goal 的时间窗；`ActivitySchedule` 持一天的条目。窗口可**重叠**（多 goal
就是这样并存竞争的），也可**跨午夜**（`StartTime > EndTime`）。

```csharp
var schedule = new ActivitySchedule(new[]
{
    new ScheduleEntry { StartTime=700,  EndTime=900,  Score=150, GoalTag=GameplayTag.RequestTag("Goal.Move.Smithy"), Destination=smithy },
    new ScheduleEntry { StartTime=900,  EndTime=1800, Score=100, GoalTag=GameplayTag.RequestTag("Goal.Move.Stall"),  Destination=market },
    new ScheduleEntry { StartTime=1200, EndTime=1300, Score=300, GoalTag=GameplayTag.RequestTag("Goal.Move.Eat"),    Destination=canteen }, // 与摆摊重叠、分更高
    new ScheduleEntry { StartTime=2200, EndTime=600,  Score=200, GoalTag=GameplayTag.RequestTag("Goal.Move.Sleep"),  Destination=bed },     // 跨午夜
});
```

`ScheduleGoalDriver.Sync(timeOfDay, accumulatedTime, pool)` 在窗口打开时发 goal、关闭时撤 goal（幂等）。
大脑组件每 tick 调它。

## 5. Activity 与选择器

`NpcActivity` 声明 `SupportedGoalType` 并给 goal 打分。`ScoreActivity` 返回该 activity 最优 goal 的分
（负分 goal 进 invalid 列表待清、0 分忽略、正分竞争）。`ActivitySelector.Select` 选全局最优
`(activity, goal)`：

```csharp
var selector = new ActivitySelector(new NpcActivity[] { new MoveToActivity() });
SelectionResult r = selector.Select(pool, scoringContext, current, currentGoal);
// r.Kind：None（无可跑）/ NoChange（最优即当前）/ Switch（跑 r.Activity + r.Goal）
```

打分中暴露的过期 goal 会从池中清除。想自定义效用就 override activity 的 `ScoreGoalItem`。

## 6. 移动（MoveToActivity / INpcAgent）

`MoveToActivity` 支持 `MoveGoal`。进入时调 `agent.MoveTo(goal.Destination)`；`Tick` 在
`agent.ReachedDestination` 后返回 true。activity 只通过 `INpcAgent`
（`MoveTo` / `StopMoving` / `ReachedDestination` / `Position`）与身体打交道，从不硬依赖 `NavMeshAgent`。

## 7. narrative 桥（AddGoalToNpc）

`AddGoalToNpc : NarrativeEvent` 让任务/对话给 NPC 塞 goal。它的 `Execute` 从
`NarrativeContext.Target` 取 `NpcActivityComponent` 并调 `AddGoal`。给它高分即可打断作息。
依赖单向（`gas.ai → narrative`）。

## 8. 大脑组件

`NpcActivityComponent` 是 NPC "大脑"——加到任意同时带 `NavMeshAgent` 的 GameObject 上（零基类）。
它每帧推进时钟、同步作息 goal、tick 当前 activity、重选。

```csharp
var brain = npc.AddComponent<NpcActivityComponent>();
brain.SetSchedule(schedule);            // 程序化（或在 Inspector 拖一个 NpcActivityScheduleAsset）
brain.AddGoal(new MoveGoal(pos, 999f)); // 塞一个临时 goal（narrative 桥也走这个）
```

Inspector：`dayLengthSeconds`（一游戏日 = 多少现实秒）、`startTimeOfDay`、`arriveDistance`、
`ownerTags`。组件实现 `IActivityScoringContext`（当前时间 + owner 标签）与 `INpcAgent`（NavMesh 移动）。

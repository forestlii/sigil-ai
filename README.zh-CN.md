# Sigil AI — NPC 行为 / Activities 配套包

[English](README.md) | [简体中文](README.zh-CN.md)

[Sigil](https://github.com/forestlii/sigil-gas)（`com.likeon.gas`）的 **NPC 行为 AI 配套包**。一个"一天时钟"驱动
**作息表（schedule）**，作息表把**目标（goal）**交给 NPC，再由**效用 AI（utility AI）**的选择器给这些 goal
打分、跑分最高的那个——于是 NPC 能早 8 点在铁砧打铁、中午逛集市、晚 9 点睡觉；任务和对话还能塞进
高优先级 goal 打断作息。

基于组件、GameplayTag 驱动，面向 Unity 6——把大脑组件加到任意 GameObject 即可，无需继承基类。

- **依赖：** `com.likeon.gas`（标签）+ `com.likeon.narrative`（事件桥）。移动用内置 NavMesh。
- **命名空间：** `Likeon.GAS.AI`
- **引擎：** Unity 6（6000.4）
- **许可：** MIT
- **状态：** 早期（**M-AI-1**）——效用 AI 核心 + NavMesh 移动 + narrative 桥。行为树、感知、goal 生成器、AI 存档为后续里程碑。

## 安装

本包依赖 Sigil 核心与 narrative 包，三者一起装：

1. 先装 `com.likeon.gas`（Sigil 核心）。
2. 再装 `com.likeon.narrative`。
3. 最后装 `com.likeon.gas.ai`（本包）。

（Package Manager → *Add package from disk…* → 各自的 `package.json`。）

### 运行测试

包内 `Tests/` 自带 EditMode 测试。在工程 `Packages/manifest.json` 的 `"testables"` 里加上本包，
再打开 **Window → General → Test Runner** 即可运行：

```json
"testables": [ "com.likeon.gas.ai" ]
```

## 功能

- **TimeOfDayClock** — UE 式 0–2400 制的纯逻辑时钟（800 = 8:00、1200 = 正午、2100 = 21:00）；推进、跨午夜回绕、天数计。
- **NpcGoal / MoveGoal** — goal 是"意图"，带可 override 的**分数**、标签**门禁**（block / require）、**去重键**、可选**存活期**。
- **ActivitySchedule + ScheduleGoalDriver** — 一天的定时条目；**窗口可重叠**，随时间进出窗口发/撤 goal——这正是**多 goal 竞争**的来源。
- **NpcActivity + ActivitySelector** — 效用打分：每个 activity 给它支持的 goal 打分（activity 分 = 其最优 goal 的分），全局分最高的 `(activity, goal)` 去跑；无效/过期 goal 清除；选中项不变则不重启。
- **MoveToActivity** — 经 **`INpcAgent`** 抽象把 NPC 移动到 goal 的目标点（NavMesh 直连，M-AI-1 不经行为树）。
- **NpcActivityComponent** — NPC"大脑"MonoBehaviour，串起 时钟 + 作息 + goal 池 + 选择器 + NavMesh。加到任意带 `NavMeshAgent` 的 GameObject 上即可——**零基类**，与生态其余部分一致。
- **AddGoalToNpc** — 一个 `NarrativeEvent`，让任务 / 对话能给 NPC 塞 goal。依赖**单向**（`gas.ai → narrative`）；narrative 完全不知 AI 存在。

设计立场：AI *框架*（goal / activity / schedule / 打分）只依赖 GameplayTag 核心；NavMesh 与 narrative 桥是
M-AI-1 里仅有的具体耦合。行为树、感知、战斗 BT 节点留到后续里程碑，让核心保持轻量。

## 示例

一个运行时 demo（`GasAiDemo`）——按 Play 自动搭场景（烘 NavMesh、生成一个按作息走位的 NPC：
打铁 → 摆摊 → 中午"吃饭"压过摆摊 → 跨午夜睡觉）——目前在开发用的 sample 工程里。
待 API 稳定后会做成包内 `Samples~`。

## 许可

[MIT](LICENSE.md) — 免费用于任何用途（含商用），保留版权声明即可。

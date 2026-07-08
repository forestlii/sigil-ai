# Sigil AI — Usage Guide

[English](Usage.md) | [简体中文](Usage.zh-CN.md)

> The NPC-behavior **companion package** `com.likeon.gas.ai`. Depends on the Sigil core
> `com.likeon.gas` (tags) and `com.likeon.narrative` (event bridge); movement uses the built-in NavMesh.
> Namespace is `Likeon.GAS.AI`.
>
> Design stance: the AI framework (goal / activity / schedule / scoring) is pure logic on top of the
> GameplayTag state bus. Time drives goals, goals compete by score, the winner runs — a component-based
> NPC behavior-AI framework built for Unity.

## Contents

1. [Install](#1-install)
2. [Time-of-day clock](#2-time-of-day-clock)
3. [Goals & scoring](#3-goals--scoring)
4. [Schedules & concurrent goals](#4-schedules--concurrent-goals)
5. [Activities & the selector](#5-activities--the-selector)
6. [Movement (MoveToActivity / INpcAgent)](#6-movement-movetoactivity--inpcagent)
7. [Narrative bridge (AddGoalToNpc)](#7-narrative-bridge-addgoaltonpc)
8. [The brain component](#8-the-brain-component)

---

## 1. Install

Install `com.likeon.gas`, `com.likeon.narrative`, then `com.likeon.gas.ai` (Package Manager →
*Add package from disk…*). To run the tests, add `"com.likeon.gas.ai"` to `"testables"` in your
`Packages/manifest.json`.

## 2. Time-of-day clock

`TimeOfDayClock` is a pure-logic clock on the UE 0–2400 scale (800 = 8:00, 1200 = noon, 2100 = 21:00).
The host advances it (real seconds → game time); schedule/scoring read it.

```csharp
var clock = new TimeOfDayClock(initialTimeOfDay: 800f); // 08:00
clock.Advance(400f);        // +4 "hours" → 1200 (noon)
float now = clock.TimeOfDay; // 1200, in [0,2400)
int day = clock.Day;         // whole days elapsed (AccumulatedTime / 2400)
```

`ITimeOfDay` exposes `TimeOfDay` (wraps at 2400) and `AccumulatedTime` (never wraps).

## 3. Goals & scoring

A goal is an intent. `MoveGoal` (the M-AI-1 concrete goal) carries a destination.

```csharp
var goal = new MoveGoal(destination, score: 300f)
{
    GoalTag  = GameplayTag.RequestTag("Goal.Move.Eat"),
    GoalKey  = someKey,   // optional dedup key (defaults to the instance)
    GoalLifetime = 0f,    // <= 0 = never expires
};
```

- `GetScore()` — defaults to `DefaultScore`; override for situational scoring (e.g. "eat scores higher when hungry").
- `PassesTagGate(ownerTags)` — owner has any `BlockTags` → false; missing any `RequireTags` → false; else true.
- `IsExpired(currentAccumulatedTime)` — true once `Lifetime` elapsed since the goal's `CreationTime`.

## 4. Schedules & concurrent goals

A `ScheduleEntry` is a timed window that provides a goal; `ActivitySchedule` holds a day's worth.
Windows may **overlap** (that's how multiple goals coexist and compete) and may cross midnight
(`StartTime > EndTime`).

```csharp
var schedule = new ActivitySchedule(new[]
{
    new ScheduleEntry { StartTime=700,  EndTime=900,  Score=150, GoalTag=GameplayTag.RequestTag("Goal.Move.Smithy"), Destination=smithy },
    new ScheduleEntry { StartTime=900,  EndTime=1800, Score=100, GoalTag=GameplayTag.RequestTag("Goal.Move.Stall"),  Destination=market },
    new ScheduleEntry { StartTime=1200, EndTime=1300, Score=300, GoalTag=GameplayTag.RequestTag("Goal.Move.Eat"),    Destination=canteen }, // overlaps stall, higher score
    new ScheduleEntry { StartTime=2200, EndTime=600,  Score=200, GoalTag=GameplayTag.RequestTag("Goal.Move.Sleep"),  Destination=bed },     // crosses midnight
});
```

`ScheduleGoalDriver.Sync(timeOfDay, accumulatedTime, pool)` adds goals as their windows open and removes
them as they close (idempotent). The brain component calls it every tick.

## 5. Activities & the selector

An `NpcActivity` declares a `SupportedGoalType` and scores goals. `ScoreActivity` returns the best
goal's score for that activity (negative-scored goals go to an invalid list; zero-scored are ignored;
positive compete). `ActivitySelector.Select` picks the global best `(activity, goal)`:

```csharp
var selector = new ActivitySelector(new NpcActivity[] { new MoveToActivity() });
SelectionResult r = selector.Select(pool, scoringContext, current, currentGoal);
// r.Kind: None (nothing runnable) / NoChange (best == current) / Switch (run r.Activity on r.Goal)
```

Expired goals surfaced during scoring are pruned from the pool. Override `ScoreGoalItem` on an activity
for custom utility.

## 6. Movement (MoveToActivity / INpcAgent)

`MoveToActivity` supports `MoveGoal`. On enter it calls `agent.MoveTo(goal.Destination)`; `Tick` returns
true once `agent.ReachedDestination`. The activity talks to the body only through `INpcAgent`
(`MoveTo` / `StopMoving` / `ReachedDestination` / `Position`), so it never hard-depends on `NavMeshAgent`.

## 7. Narrative bridge (AddGoalToNpc)

`AddGoalToNpc : NarrativeEvent` lets quests/dialogue push a goal to an NPC. Its `Execute` reads the
`NpcActivityComponent` off `NarrativeContext.Target` and calls `AddGoal`. Give it a high score to interrupt
the routine. Dependency is one-way (`gas.ai → narrative`).

## 8. The brain component

`NpcActivityComponent` is the NPC "brain" — add it to any GameObject that also has a `NavMeshAgent`
(zero base class). Each frame it advances the clock, syncs schedule goals, ticks the current activity,
and re-selects.

```csharp
var brain = npc.AddComponent<NpcActivityComponent>();
brain.SetSchedule(schedule);        // programmatic (or assign an NpcActivityScheduleAsset in the Inspector)
brain.AddGoal(new MoveGoal(pos, 999f)); // push an ad-hoc goal (also what the narrative bridge uses)
```

Inspector: `dayLengthSeconds` (real seconds per in-game day), `startTimeOfDay`, `arriveDistance`,
`ownerTags`. The component implements `IActivityScoringContext` (current time + owner tags) and
`INpcAgent` (NavMesh-backed movement).

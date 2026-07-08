# Sigil AI — NPC Behavior / Activities Companion

[English](README.md) | [简体中文](README.zh-CN.md)

A **companion package** for [Sigil](https://github.com/forestlii/sigil-gas) (`com.likeon.gas`) that provides
**NPC behavior AI**. A time-of-day clock drives **schedules**, schedules hand **goals** to NPCs, and a
**utility-AI** selector scores the goals and runs the best one — so an NPC forges at the blacksmith at 8am,
browses the market at noon, and sleeps at 9pm, while quests and dialogue can inject high-priority goals that
interrupt the routine.

Component-based and GameplayTag-driven, built for Unity 6 — add the brain to any GameObject, no base
class to inherit.

- **Depends on:** `com.likeon.gas` (tags) + `com.likeon.narrative` (event bridge). Movement uses the built-in NavMesh.
- **Namespace:** `Likeon.GAS.AI`
- **Engine:** Unity 6 (6000.4)
- **License:** MIT
- **Status:** early (**M-AI-1**) — utility-AI core + NavMesh movement + narrative bridge. Behavior trees, sensing, goal generators and AI save are later milestones.

## Install

This package depends on the Sigil core and the narrative package. Install all three:

1. Add `com.likeon.gas` (Sigil core) first.
2. Add `com.likeon.narrative`.
3. Add `com.likeon.gas.ai` (this package).

(Package Manager → *Add package from disk…* → each `package.json`.)

### Running tests

The package ships with EditMode tests under `Tests/`. Add the package to `"testables"` in your project's
`Packages/manifest.json`, then open **Window → General → Test Runner**:

```json
"testables": [ "com.likeon.gas.ai" ]
```

## Features

- **TimeOfDayClock** — a pure-logic clock on a 0–2400 scale (800 = 8:00, 1200 = noon, 2100 = 21:00); advance, midnight wrap, day count.
- **NpcGoal / MoveGoal** — a goal is an intent with an overridable **score**, tag **gates** (block / require), a **dedup key**, and an optional **lifetime**.
- **ActivitySchedule + ScheduleGoalDriver** — a day's timed entries; **overlapping windows** add/remove goals as time passes, which is where **multiple goals compete**.
- **NpcActivity + ActivitySelector** — utility scoring: each activity scores its supported goals (activity score = its best goal's score), the global-best `(activity, goal)` runs; invalid/expired goals are pruned; no restart when the pick is unchanged.
- **MoveToActivity** — moves the NPC to the goal's destination through the **`INpcAgent`** abstraction (NavMesh direct, no behavior tree in M-AI-1).
- **NpcActivityComponent** — the NPC "brain" MonoBehaviour wiring clock + schedule + goal pool + selector + NavMesh. Add it to any GameObject with a `NavMeshAgent` — **zero base class**, like the rest of the ecosystem.
- **AddGoalToNpc** — a `NarrativeEvent` so quests / dialogue can push a goal to an NPC. Dependency is **one-way** (`gas.ai → narrative`); narrative never knows about AI.

Design stance: the AI *framework* (goal / activity / schedule / scoring) depends only on the GameplayTag core;
NavMesh and the narrative bridge are the only concrete couplings in M-AI-1. Behavior trees, sensing and combat
BT nodes are deferred to later milestones so the core stays light.

## Samples

A runtime demo (`GasAiDemo`) — builds a scene on Play (bakes a NavMesh, spawns one NPC on a day schedule:
forge → stall → lunch outscores stall → sleep across midnight) — lives in the development sample project.
A packaged `Samples~` will ship as the API stabilizes.

## License

[MIT](LICENSE.md) — free for any use including commercial, just keep the copyright notice.

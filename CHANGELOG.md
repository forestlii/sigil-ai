[English](CHANGELOG.md) | [简体中文](CHANGELOG.zh-CN.md)

# Changelog

All notable changes to this package are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
While the public API is unstable it stays in the `0.y.z` range and may break without a major bump.

## [Unreleased]

### Added
- Package scaffolding: `com.likeon.gas.ai` (depends on `com.likeon.gas` and `com.likeon.narrative`),
  `Likeon.GAS.AI.Runtime` / `.Tests.EditMode` assembly definitions.
- **M-AI-1 utility-AI core**: time-of-day clock (0–2400 scale), `NpcGoal`/`MoveGoal` with
  overridable scoring, tag gates (block/require), dedup keys and lifetime; `ActivitySchedule`
  with overlapping windows and `ScheduleGoalDriver` (add/remove goals as windows open/close);
  `NpcActivity` scoring + `ActivitySelector` (pick the best activity+goal, prune expired, no-restart);
  `MoveToActivity` driving NavMesh via the `INpcAgent` abstraction; `NpcActivityComponent` brain;
  `NpcActivityScheduleAsset`; and an `AddGoalToNpc` narrative-event bridge. 55 EditMode tests.

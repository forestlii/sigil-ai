[English](CHANGELOG.md) | [简体中文](CHANGELOG.zh-CN.md)

# 更新日志

本包的所有重要变更记录于此。
格式遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本遵循 [语义化版本](https://semver.org/lang/zh-CN/)。
公共 API 尚未稳定，停留在 `0.y.z` 区间，可能在不升 major 的情况下破坏性变更。

## [未发布]

### 新增
- 包脚手架：`com.likeon.gas.ai`（依赖 `com.likeon.gas` 与 `com.likeon.narrative`），
  `Likeon.GAS.AI.Runtime` / `.Tests.EditMode` 程序集定义。
- **M-AI-1 效用 AI 核心**：0–2400 制时钟；`NpcGoal`/`MoveGoal`（可 override 打分、Block/Require 标签门禁、
  去重键、存活期）；`ActivitySchedule`（窗口可重叠）+ `ScheduleGoalDriver`（进窗口发/离窗口撤 goal）；
  `NpcActivity` 打分 + `ActivitySelector`（选最优 activity+goal、清过期、同项不重启）；`MoveToActivity`
  经 `INpcAgent` 抽象驱动 NavMesh；`NpcActivityComponent` 大脑；`NpcActivityScheduleAsset`；
  以及 `AddGoalToNpc` narrative 事件桥。55 个 EditMode 测试。

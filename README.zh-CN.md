# Sigil — GAS AI（NPC 行为 · Activities）for Unity

[English](README.md) | [简体中文](README.zh-CN.md)

面向 Unity RPG 的 **NPC 行为 AI**。一个"一天时钟"驱动**作息表（schedule）**，作息表把**目标（goal）**
交给 NPC，再由**效用 AI（utility AI）**的活动选择器给这些 goal 打分、跑分最高的那个 —— 于是 NPC
能早 8 点在铁砧打铁、中午逛集市、晚 9 点睡觉；任务和对话还能塞进高优先级 goal 打断作息。

这是把 **Narrative Pro / Narrative Arsenal**（原设计 by Narrative Tools）的 AI / Activities 系统
**重新设计**（不是逐行翻译）到 Unity 的、基于组件的 C# 实现。

- **引擎：** Unity 6（6000.4）
- **依赖：** `com.likeon.gas`（GameplayTag）。移动用内置 NavMesh。
- **搭桥：** `com.likeon.narrative`（任务/对话可给 NPC 下 goal）。
- **归属：** 朋友原设计（Narrative Tools）的 C# 重写。AI 辅助实现、本人把关 —— 不包装成自研设计。

> **状态：** 早期脚手架。里程碑 **M-AI-1** = 时钟 + 作息表 + goal + 效用打分 + NavMesh `MoveToActivity`
> + narrative 桥。行为树、感知、goal 生成器、AI 存档为后续里程碑。详见工作区 `MD/spec/M-AI-1.md`。

## 许可

MIT —— 见 [LICENSE.md](LICENSE.md)。Copyright (c) 2026 Likeon。

# Sigil — GAS AI (NPC Behavior · Activities) for Unity

[English](README.md) | [简体中文](README.zh-CN.md)

**NPC behavior AI** for Unity RPGs. A time-of-day clock drives **schedules**, schedules hand
**goals** to NPCs, and a **utility-AI** activity selector scores the goals and runs the best one —
so an NPC can forge at the blacksmith at 8am, browse the market at noon, and sleep at 9pm, while
quests and dialogue can inject high-priority goals that interrupt the routine.

This is a component-based C# **re-design** (not a line-by-line port) of the AI / Activities system
from **Narrative Pro / Narrative Arsenal** (original design by Narrative Tools), rebuilt for Unity.

- **Engine:** Unity 6 (6000.4)
- **Depends on:** `com.likeon.gas` (GameplayTags). Movement uses the built-in NavMesh.
- **Bridges to:** `com.likeon.narrative` (quests / dialogue can add goals to NPCs).
- **Attribution:** a C# rewrite of a friend's design (Narrative Tools). AI-assisted implementation,
  human-reviewed — not presented as original design.

> **Status:** early scaffolding. Milestone **M-AI-1** = clock + schedule + goal + utility scoring +
> NavMesh `MoveToActivity` + narrative bridge. Behavior trees, sensing, goal generators and AI save
> are later milestones. See `MD/spec/M-AI-1.md` in the workspace.

## License

MIT — see [LICENSE.md](LICENSE.md). Copyright (c) 2026 Likeon.

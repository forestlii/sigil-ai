// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using Likeon.GAS;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// 打分时需要的 NPC 上下文：当前时间（判 goal 过期）+ owner 身上的标签（判门禁）。
    /// 抽象成接口，让打分逻辑不硬依赖具体 NPC 组件（对齐"组件 + 接口"红线）。
    /// </summary>
    public interface IActivityScoringContext
    {
        /// <summary>当前累计时间（2400 制）。喂给 <see cref="NpcGoal.IsExpired"/>。</summary>
        float CurrentTime { get; }

        /// <summary>owner 当前拥有的标签。喂给 <see cref="NpcGoal.PassesTagGate"/>。</summary>
        GameplayTagContainer OwnerTags { get; }
    }
}

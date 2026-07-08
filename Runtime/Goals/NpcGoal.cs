// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;
using Likeon.GAS;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// NPC 的一个"意图"。打分相关语义：一个可 override 的分数、
    /// 标签门禁（Block/Require）、去重键（GoalKey）、可选存活期（Lifetime）。
    /// 抽象基类——具体 goal（如 <see cref="MoveGoal"/>）携带自己的目标数据。
    /// （数据对象用继承，不触及"NPC 组件不强制继承基类"那条红线。）
    /// </summary>
    [Serializable]
    public abstract class NpcGoal
    {
        /// <summary>意图标签（分类 / 调试用），如 "Goal.Move"。对齐生态标签驱动。</summary>
        public GameplayTag GoalTag;

        /// <summary>基础分。若不 override <see cref="GetScore"/> 即用此分。</summary>
        public float DefaultScore;

        /// <summary>owner 命中其中任一 → 本 goal 记 0（不被采纳）。</summary>
        public GameplayTagContainer BlockTags = new GameplayTagContainer();

        /// <summary>owner 须全部含有 → 否则记 0。</summary>
        public GameplayTagContainer RequireTags = new GameplayTagContainer();

        /// <summary>去重键：同键 goal 不重复添加。null 时以自身实例为键。</summary>
        public object GoalKey;

        /// <summary>存活期（2400 制时长）。&lt;= 0 表示永不过期。</summary>
        public float GoalLifetime;

        /// <summary>创建时的累计时间（由 component 添加 goal 时盖，2400 制累计时间）。</summary>
        public float CreationTime;

        private static readonly GameplayTagContainer EmptyTags = new GameplayTagContainer();

        /// <summary>本 goal 的分数。默认 = DefaultScore；子类可 override。</summary>
        public virtual float GetScore() => DefaultScore;

        /// <summary>去重键：GoalKey ?? this。</summary>
        public object GetKey() => GoalKey ?? this;

        /// <summary>标签门禁：命中任一 Block → false；缺任一 Require → false；否则 true。</summary>
        public bool PassesTagGate(GameplayTagContainer ownerTags)
        {
            ownerTags ??= EmptyTags;
            if (ownerTags.HasAny(BlockTags)) return false;
            if (!ownerTags.HasAll(RequireTags)) return false;
            return true;
        }

        /// <summary>是否已过期。Lifetime &lt;= 0 → 永不过期；否则 (当前累计时间 - CreationTime) &gt;= Lifetime。</summary>
        public bool IsExpired(float currentAccumulatedTime)
        {
            if (GoalLifetime <= 0f) return false;
            return (currentAccumulatedTime - CreationTime) >= GoalLifetime;
        }
    }
}

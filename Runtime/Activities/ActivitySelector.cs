// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System.Collections.Generic;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// 效用 AI 的选择器。忠实移植 UE <c>UNPCActivityComponent::PerformActivitySelection</c>：
    /// 遍历所有 activity，各自对本类型 goal 打分，选出全局分最高（且 &gt; 0）的 (activity, bestGoal)；
    /// 打分中标记的无效 goal（过期）从池中清除；若最优即当前在跑项则不重启。
    /// 纯逻辑——"何时重选"（发/撤 goal、activity 结束时）由上层组件事件驱动，本类不含定时器。
    /// </summary>
    public sealed class ActivitySelector
    {
        private readonly List<NpcActivity> _activities;

        public ActivitySelector(IEnumerable<NpcActivity> activities)
        {
            _activities = activities != null ? new List<NpcActivity>(activities) : new List<NpcActivity>();
        }

        public IReadOnlyList<NpcActivity> Activities => _activities;

        /// <summary>
        /// 依当前 goal 池选出该跑的 activity。会顺带把过期（负分）goal 从池中清除。
        /// </summary>
        /// <param name="current">当前正在跑的 activity（无则 null）。</param>
        /// <param name="currentGoal">当前正在跑的 goal（无则 null）。</param>
        public SelectionResult Select(NpcGoalPool pool, IActivityScoringContext ctx,
                                      NpcActivity current, NpcGoal currentGoal)
        {
            NpcActivity bestActivity = null;
            NpcGoal bestGoal = null;
            float bestScore = 0f;
            List<NpcGoal> allInvalid = null;

            for (int i = 0; i < _activities.Count; i++)
            {
                var act = _activities[i];
                if (act == null) continue;

                var typedGoals = pool.GetGoalsOfType(act.SupportedGoalType);
                float score = act.ScoreActivity(typedGoals, ctx, out var bg, out var invalid);

                if (invalid != null && invalid.Count > 0)
                    (allInvalid ??= new List<NpcGoal>()).AddRange(invalid);

                if (score > 0f && score > bestScore) // 全局最高（且 > 0）
                {
                    bestScore = score;
                    bestActivity = act;
                    bestGoal = bg;
                }
            }

            // 清除无效（过期）goal
            if (allInvalid != null)
                for (int i = 0; i < allInvalid.Count; i++)
                    pool.Remove(allInvalid[i]);

            if (bestActivity == null)
                return SelectionResult.None;

            // 最优即当前在跑项 → 不重启
            if (ReferenceEquals(bestActivity, current) && ReferenceEquals(bestGoal, currentGoal))
                return SelectionResult.NoChange(bestActivity, bestGoal);

            return SelectionResult.Switch(bestActivity, bestGoal);
        }
    }
}

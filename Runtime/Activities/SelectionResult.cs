// Copyright (c) 2026 Likeon. Licensed under the MIT License.

namespace Likeon.GAS.AI
{
    /// <summary>选择的结果类别。</summary>
    public enum SelectionKind
    {
        /// <summary>没有可跑的 activity（无正分 goal）。</summary>
        None,
        /// <summary>最优仍是当前正在跑的 (activity, goal)，不重启。</summary>
        NoChange,
        /// <summary>切换到新的 (activity, goal)。</summary>
        Switch,
    }

    /// <summary>一次 <see cref="ActivitySelector.Select"/> 的结果。</summary>
    public readonly struct SelectionResult
    {
        public readonly SelectionKind Kind;
        public readonly NpcActivity Activity;
        public readonly NpcGoal Goal;

        private SelectionResult(SelectionKind kind, NpcActivity activity, NpcGoal goal)
        {
            Kind = kind;
            Activity = activity;
            Goal = goal;
        }

        public static readonly SelectionResult None = new SelectionResult(SelectionKind.None, null, null);
        public static SelectionResult NoChange(NpcActivity a, NpcGoal g) => new SelectionResult(SelectionKind.NoChange, a, g);
        public static SelectionResult Switch(NpcActivity a, NpcGoal g) => new SelectionResult(SelectionKind.Switch, a, g);
    }
}

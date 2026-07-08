// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// NPC 当前持有的 goal 集合。按 <see cref="NpcGoal.GetKey"/> 去重
    /// （同键不重复添加），支持按类型筛选（喂给对应 activity 打分）。纯逻辑，可直接测。
    /// </summary>
    public sealed class NpcGoalPool
    {
        private readonly List<NpcGoal> _goals = new List<NpcGoal>();

        public IReadOnlyList<NpcGoal> Goals => _goals;
        public int Count => _goals.Count;

        /// <summary>加入一个 goal；若已存在同 <see cref="NpcGoal.GetKey"/> 的 goal 则不加、返回 false。</summary>
        public bool Add(NpcGoal goal)
        {
            if (goal == null) return false;
            var key = goal.GetKey();
            for (int i = 0; i < _goals.Count; i++)
                if (Equals(_goals[i].GetKey(), key)) return false; // 同键去重
            _goals.Add(goal);
            return true;
        }

        /// <summary>移除一个 goal。</summary>
        public bool Remove(NpcGoal goal) => _goals.Remove(goal);

        /// <summary>返回属于 <paramref name="goalType"/>（含子类）的 goal 子集。</summary>
        public List<NpcGoal> GetGoalsOfType(Type goalType)
        {
            var result = new List<NpcGoal>();
            if (goalType == null) return result;
            for (int i = 0; i < _goals.Count; i++)
                if (goalType.IsInstanceOfType(_goals[i])) result.Add(_goals[i]);
            return result;
        }
    }
}

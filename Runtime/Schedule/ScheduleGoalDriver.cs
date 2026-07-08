// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System.Collections.Generic;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// 把作息表变成"实时的 goal 增删"：进入某条目的窗口 → 造 goal 塞进池；离开窗口 → 从池撤掉。
    /// 忠实移植 UE <c>UScheduledBehavior_AddNPCGoal</c> 到 StartTime <c>ProvideGoal</c>、到 EndTime 撤销。
    /// 纯逻辑（持每条目"是否已发"的状态），可直接测；由组件每 tick 调 <see cref="Sync"/>。
    /// </summary>
    public sealed class ScheduleGoalDriver
    {
        private readonly ActivitySchedule _schedule;
        // entry 下标 -> 已为它发出的 goal（用于离开窗口时精确撤销）
        private readonly Dictionary<int, NpcGoal> _active = new Dictionary<int, NpcGoal>();

        public ScheduleGoalDriver(ActivitySchedule schedule)
        {
            _schedule = schedule;
        }

        /// <summary>
        /// 依当前时刻同步 goal 池：新激活的条目发 goal（盖 CreationTime=accumulatedTime），失活的撤掉。幂等。
        /// </summary>
        public void Sync(float timeOfDay, float accumulatedTime, NpcGoalPool pool)
        {
            if (pool == null) return;
            var entries = _schedule.Entries;

            for (int i = 0; i < entries.Count; i++)
            {
                bool active = entries[i].IsActiveAt(timeOfDay);
                bool has = _active.ContainsKey(i);

                if (active && !has)
                {
                    var goal = entries[i].CreateGoal();
                    goal.CreationTime = accumulatedTime;
                    if (pool.Add(goal))
                        _active[i] = goal;
                }
                else if (!active && has)
                {
                    pool.Remove(_active[i]);
                    _active.Remove(i);
                }
            }
        }
    }
}

// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System.Collections.Generic;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// 一天的作息表 = 一组 <see cref="ScheduleEntry"/>（纯逻辑，零 UnityEngine 运行时依赖，可直接测）。
    /// 对应 UE <c>UNPCActivitySchedule</c>（DataAsset）。这里只做纯逻辑；ScriptableObject 数据包装
    /// 留在组件/接线层（ScriptableObject 无法脱离 Unity 实例化）。
    /// 关键：<see cref="GetActiveEntries"/> 在窗口重叠时返回多条 → 这是"多 goal 并存竞争"的来源。
    /// </summary>
    public sealed class ActivitySchedule
    {
        private readonly List<ScheduleEntry> _entries;

        public ActivitySchedule(IEnumerable<ScheduleEntry> entries)
        {
            _entries = entries != null ? new List<ScheduleEntry>(entries) : new List<ScheduleEntry>();
        }

        public IReadOnlyList<ScheduleEntry> Entries => _entries;

        /// <summary>返回当前时刻所有激活的条目（可多条 → 窗口重叠时多 goal 并存）。</summary>
        public List<ScheduleEntry> GetActiveEntries(float timeOfDay)
        {
            var result = new List<ScheduleEntry>();
            for (int i = 0; i < _entries.Count; i++)
                if (_entries[i].IsActiveAt(timeOfDay))
                    result.Add(_entries[i]);
            return result;
        }
    }
}

// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// 作息表数据资产（一天的定时条目）。对应 UE <c>UNPCActivitySchedule</c> DataAsset。
    /// 纯数据容器——运行时 <see cref="BuildSchedule"/> 出纯逻辑 <see cref="ActivitySchedule"/> 供组件用。
    /// </summary>
    [CreateAssetMenu(menuName = "Likeon/GAS AI/NPC Activity Schedule", fileName = "NpcActivitySchedule")]
    public sealed class NpcActivityScheduleAsset : ScriptableObject
    {
        [SerializeField] private List<ScheduleEntry> entries = new List<ScheduleEntry>();

        public IReadOnlyList<ScheduleEntry> Entries => entries;

        /// <summary>构造纯逻辑作息表（拷贝当前条目）。</summary>
        public ActivitySchedule BuildSchedule() => new ActivitySchedule(entries);
    }
}

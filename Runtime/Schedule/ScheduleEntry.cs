// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;
using UnityEngine;
using Likeon.GAS;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// 作息表里的一条定时行为：
    /// 有起止时间窗口，窗口内向 NPC 提供一个 goal（<see cref="CreateGoal"/>），窗口外撤销。
    /// 时间用 0–2400 制式（800=8:00）。窗口可跨午夜（Start &gt; End，如 2200–0600 睡觉）。
    /// M-AI-1 生成 <see cref="MoveGoal"/>：带目标点 + 分数 + 意图标签。
    /// </summary>
    [Serializable]
    public struct ScheduleEntry
    {
        [Tooltip("窗口起点，0–2400。")] public float StartTime;
        [Tooltip("窗口终点，0–2400。")] public float EndTime;
        [Tooltip("该 goal 的基础分（打分竞争用）。")] public float Score;
        [Tooltip("意图标签，如 Goal.Move.Blacksmith。")] public GameplayTag GoalTag;
        [Tooltip("MoveGoal 的目标点。")] public Vector3 Destination;

        /// <summary>
        /// 给定当前一天中的时刻，本窗口是否激活。
        /// 正常窗口 Start&lt;End：<c>[Start, End)</c>（含起、不含止）；
        /// 跨午夜 Start&gt;End：<c>[Start, 2400) ∪ [0, End)</c>；
        /// Start==End：视为全天激活。
        /// </summary>
        public bool IsActiveAt(float timeOfDay)
        {
            if (StartTime == EndTime) return true;               // 全天
            if (StartTime < EndTime)                             // 正常窗口 [Start, End)
                return timeOfDay >= StartTime && timeOfDay < EndTime;
            return timeOfDay >= StartTime || timeOfDay < EndTime; // 跨午夜 [Start,2400)∪[0,End)
        }

        /// <summary>依本条目造一个 MoveGoal（目标点/分数/标签）。</summary>
        public MoveGoal CreateGoal() => new MoveGoal(Destination, Score) { GoalTag = GoalTag };
    }
}

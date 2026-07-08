// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;
using UnityEngine;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// "移动到某点"的 goal（M-AI-1 的具体 goal，供 MoveToActivity 消费）。
    /// </summary>
    [Serializable]
    public sealed class MoveGoal : NpcGoal
    {
        /// <summary>目标点（世界坐标）。</summary>
        public Vector3 Destination;

        public MoveGoal() { }

        public MoveGoal(Vector3 destination, float score)
        {
            Destination = destination;
            DefaultScore = score;
        }
    }
}

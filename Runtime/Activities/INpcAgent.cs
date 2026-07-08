// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using UnityEngine;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// NPC 身体能力的抽象。让 activity 通过它驱动移动，而**不硬依赖 <c>NavMeshAgent</c>**
    /// （守"组件 + 接口、不硬依赖具体实现"红线）。M-AI-1 由 NpcActivityComponent 用 NavMesh 实现。
    /// </summary>
    public interface INpcAgent
    {
        /// <summary>NPC 当前位置。</summary>
        Vector3 Position { get; }

        /// <summary>命令 NPC 前往目标点（NavMesh 直连时 = SetDestination）。</summary>
        void MoveTo(Vector3 destination);

        /// <summary>停止移动。</summary>
        void StopMoving();

        /// <summary>是否已到达当前目标点（供 activity 判完成）。</summary>
        bool ReachedDestination { get; }
    }
}

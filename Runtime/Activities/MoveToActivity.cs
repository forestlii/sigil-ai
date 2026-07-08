// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// M-AI-1 的具体 activity：把 NPC 移动到 <see cref="MoveGoal.Destination"/>（经 <see cref="INpcAgent"/>，
    /// NavMesh 直连、不经行为树）。到达即完成。
    /// </summary>
    public sealed class MoveToActivity : NpcActivity
    {
        public override Type SupportedGoalType => typeof(MoveGoal);

        public override void OnEnter(INpcAgent agent, NpcGoal goal)
        {
            if (agent != null && goal is MoveGoal move)
                agent.MoveTo(move.Destination);
        }

        public override bool Tick(INpcAgent agent)
        {
            return agent != null && agent.ReachedDestination;
        }

        public override void OnExit(INpcAgent agent)
        {
            agent?.StopMoving();
        }
    }
}

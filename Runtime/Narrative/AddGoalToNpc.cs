// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using UnityEngine;
using Likeon.Narrative;
using Likeon.GAS;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// narrative 桥：挂在对话/任务节点上的事件，触发时给目标 NPC 塞一个"去某点"的 goal，
    /// 让剧情能引导 / 打断 NPC 作息（如任务触发"去村口迎接玩家"）。
    /// 依赖方向：<b>gas.ai → narrative 单向</b>，narrative 不知 ai 存在。
    /// 目标 NPC 取自 <see cref="NarrativeContext.Target"/>（该节点作用于谁）。
    /// </summary>
    [System.Serializable]
    public sealed class AddGoalToNpc : NarrativeEvent
    {
        [Tooltip("目标点（世界坐标）。")]
        [SerializeField] private Vector3 destination;

        [Tooltip("goal 分数。要打断作息就调到高于当前作息 goal 的分。")]
        [SerializeField] private float score = 1000f;

        [Tooltip("意图标签，如 Goal.Move.Story。")]
        [SerializeField] private string goalTag = "Goal.Move.Story";

        [Tooltip("存活期（0–2400 制）。<= 0 永不过期（需 NPC 到达或被撤才消）。")]
        [SerializeField] private float lifetime = 0f;

        public AddGoalToNpc() { }

        public override void Execute(NarrativeContext context)
        {
            var target = context?.Target;
            if (target == null) return;

            var brain = target.GetComponent<NpcActivityComponent>();
            if (brain == null) return;

            var goal = new MoveGoal(destination, score)
            {
                GoalTag = GameplayTag.RequestTag(goalTag),
                GoalLifetime = lifetime,
            };
            brain.AddGoal(goal);
        }

        public override string GetDisplayText() => $"Add NPC goal → {destination} (score {score})";
    }
}

// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// 一个 activity = 支持某类 goal + 对其打分 + （后续任务）执行。忠实移植 UE <c>UNPCActivity</c>：
    /// <see cref="ScoreActivity"/> 遍历本类型 goal，取"最优 goal"的分作为 activity 的分；
    /// 负分 goal 视为无效（作废待删），0 分（门禁不过 / 过期以外的不可用）忽略，正分参与竞争。
    /// 执行（RunActivity）在具体子类（如 MoveToActivity，任务6），本基类只管纯打分逻辑。
    /// </summary>
    public abstract class NpcActivity
    {
        private static readonly List<NpcGoal> EmptyInvalid = new List<NpcGoal>();

        /// <summary>本 activity 支持的 goal 类型。对应 UE SupportedGoalType。</summary>
        public abstract Type SupportedGoalType { get; }

        /// <summary>
        /// 给单个 goal 打分。默认：过期 → -1（作废）；门禁不过 → 0（不采纳）；否则 goal.GetScore()。
        /// 子类可 override 提供情境分（如体力低时 SitOnCouch 抬分）。对应 UE ScoreGoalItem。
        /// </summary>
        public virtual float ScoreGoalItem(NpcGoal goal, IActivityScoringContext ctx)
        {
            if (goal == null) return 0f;
            if (goal.IsExpired(ctx.CurrentTime)) return -1f;        // 作废（待删）
            if (!goal.PassesTagGate(ctx.OwnerTags)) return 0f;      // 门禁不过 → 不采纳
            return goal.GetScore();
        }

        /// <summary>
        /// 给本 activity 打分 = 其支持类型下"最优 goal"的分，并输出该 bestGoal 与需清除的 invalidGoals。
        /// 对应 UE ScoreActivity。空/无正分 → 返回 0、bestGoal=null。
        /// </summary>
        public float ScoreActivity(IReadOnlyList<NpcGoal> goals, IActivityScoringContext ctx,
                                   out NpcGoal bestGoal, out List<NpcGoal> invalidGoals)
        {
            bestGoal = null;
            invalidGoals = null;
            float best = 0f;

            if (goals != null)
            {
                for (int i = 0; i < goals.Count; i++)
                {
                    var g = goals[i];
                    if (g == null) continue;

                    float s = ScoreGoalItem(g, ctx);
                    if (s < 0f)
                        (invalidGoals ??= new List<NpcGoal>()).Add(g);   // 负分 → 作废
                    else if (s > 0f && s > best)                          // 正分且更高 → 竞争（0 分忽略）
                    {
                        best = s;
                        bestGoal = g;
                    }
                }
            }

            invalidGoals ??= EmptyInvalid;
            return best;
        }

        // ── 执行钩子（任务6）：具体 activity override 来驱动 INpcAgent。基类默认空实现，不破坏纯打分测试 ──

        /// <summary>被选中开始执行时调用（对应 UE RunActivity 起点）。</summary>
        public virtual void OnEnter(INpcAgent agent, NpcGoal goal) { }

        /// <summary>每 tick 推进；返回 true 表示 activity 已完成（目标达成）。</summary>
        public virtual bool Tick(INpcAgent agent) => false;

        /// <summary>结束（完成或被打断）时调用，做清理。</summary>
        public virtual void OnExit(INpcAgent agent) { }
    }
}

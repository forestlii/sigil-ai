// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;
using NUnit.Framework;
using UnityEngine;
using Likeon.GAS;

namespace Likeon.GAS.AI.Tests
{
    /// <summary>
    /// activity 打分测试：activity 分 = 最优 goal 分；负分作废、0 分忽略、正分竞争；门禁/过期；override。
    /// </summary>
    public class NpcActivityTests
    {
        private const float Eps = 1e-4f;

        // 只支持 MoveGoal 的最简 activity（不涉及执行）。
        private sealed class TestMoveActivity : NpcActivity
        {
            public override Type SupportedGoalType => typeof(MoveGoal);
        }

        // 测 override：给分数统一 +1000。
        private sealed class BoostActivity : NpcActivity
        {
            public override Type SupportedGoalType => typeof(MoveGoal);
            public override float ScoreGoalItem(NpcGoal goal, IActivityScoringContext ctx)
                => goal.GetScore() + 1000f;
        }

        private sealed class StubContext : IActivityScoringContext
        {
            public float CurrentTime { get; set; }
            public GameplayTagContainer OwnerTags { get; set; } = new GameplayTagContainer();
        }

        private static MoveGoal Goal(float score, float lifetime = 0f, float creation = 0f)
            => new MoveGoal(Vector3.zero, score) { GoalLifetime = lifetime, CreationTime = creation };

        private static GameplayTagContainer Tags(params string[] names)
        {
            var c = new GameplayTagContainer();
            foreach (var n in names) c.AddTag(GameplayTag.RequestTag(n));
            return c;
        }

        [Test]
        public void ScoreActivity_空goals_返回0且无best()
        {
            var act = new TestMoveActivity();
            float s = act.ScoreActivity(new NpcGoal[0], new StubContext(), out var best, out var invalid);
            Assert.AreEqual(0f, s, Eps);
            Assert.IsNull(best);
            Assert.IsEmpty(invalid);
        }

        [Test]
        public void ScoreActivity_多goal_选最高分()
        {
            var act = new TestMoveActivity();
            var lo = Goal(100f);
            var hi = Goal(300f);
            var mid = Goal(200f);
            float s = act.ScoreActivity(new NpcGoal[] { lo, hi, mid }, new StubContext(), out var best, out _);
            Assert.AreEqual(300f, s, Eps);
            Assert.AreSame(hi, best);
        }

        [Test]
        public void ScoreActivity_门禁不过的goal_记0被忽略_不选不作废()
        {
            var act = new TestMoveActivity();
            var blocked = Goal(500f);
            blocked.BlockTags = Tags("State.Sleeping"); // owner 睡着 → 0 分
            var ok = Goal(100f);
            var ctx = new StubContext { OwnerTags = Tags("State.Sleeping") };
            float s = act.ScoreActivity(new NpcGoal[] { blocked, ok }, ctx, out var best, out var invalid);
            Assert.AreEqual(100f, s, Eps);
            Assert.AreSame(ok, best);
            Assert.IsEmpty(invalid, "门禁不过是 0 分忽略，不该进 invalid");
        }

        [Test]
        public void ScoreActivity_过期goal_负分进invalid_不选()
        {
            var act = new TestMoveActivity();
            var expired = Goal(500f, lifetime: 100f, creation: 800f); // 到 1000 已过期
            var ok = Goal(100f);
            var ctx = new StubContext { CurrentTime = 1000f };
            float s = act.ScoreActivity(new NpcGoal[] { expired, ok }, ctx, out var best, out var invalid);
            Assert.AreEqual(100f, s, Eps);
            Assert.AreSame(ok, best);
            Assert.Contains(expired, invalid);
        }

        [Test]
        public void ScoreGoalItem_默认_过期负分_门禁0_否则GetScore()
        {
            var act = new TestMoveActivity();
            var ctx = new StubContext { CurrentTime = 1000f, OwnerTags = Tags("State.Awake") };

            Assert.AreEqual(150f, act.ScoreGoalItem(Goal(150f), ctx), Eps);

            var expired = Goal(150f, lifetime: 100f, creation: 800f);
            Assert.Less(act.ScoreGoalItem(expired, ctx), 0f);

            var blocked = Goal(150f);
            blocked.BlockTags = Tags("State.Awake");
            Assert.AreEqual(0f, act.ScoreGoalItem(blocked, ctx), Eps);
        }

        [Test]
        public void ScoreActivity_override的ScoreGoalItem生效()
        {
            var act = new BoostActivity();
            float s = act.ScoreActivity(new NpcGoal[] { Goal(50f) }, new StubContext(), out var best, out _);
            Assert.AreEqual(1050f, s, Eps);
            Assert.IsNotNull(best);
        }
    }
}

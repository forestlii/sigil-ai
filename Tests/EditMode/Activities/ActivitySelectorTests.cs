// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;
using NUnit.Framework;
using UnityEngine;
using Likeon.GAS;

namespace Likeon.GAS.AI.Tests
{
    /// <summary>
    /// goal 池（去重/筛类型）+ 选择器（跨 activity 选全局最高分、清过期、同项不重启）测试。
    /// M-AI-1 效用 AI 的高潮：中午"吃饭"分高压过"摆摊"。
    /// </summary>
    public class ActivitySelectorTests
    {
        // 第二种 goal 类型，用于测 SupportedGoalType 筛选。
        private sealed class RestGoal : NpcGoal { }

        private sealed class MoveActivity : NpcActivity
        {
            public override Type SupportedGoalType => typeof(MoveGoal);
        }

        private sealed class RestActivity : NpcActivity
        {
            public override Type SupportedGoalType => typeof(RestGoal);
        }

        private sealed class StubContext : IActivityScoringContext
        {
            public float CurrentTime { get; set; }
            public GameplayTagContainer OwnerTags { get; set; } = new GameplayTagContainer();
        }

        private static MoveGoal Move(float score, float lifetime = 0f, float creation = 0f)
            => new MoveGoal(Vector3.zero, score) { GoalLifetime = lifetime, CreationTime = creation };

        // ── NpcGoalPool ──

        [Test]
        public void Pool_加入不同goal_都进()
        {
            var pool = new NpcGoalPool();
            Assert.IsTrue(pool.Add(Move(100f)));
            Assert.IsTrue(pool.Add(Move(200f)));
            Assert.AreEqual(2, pool.Count);
        }

        [Test]
        public void Pool_同GoalKey_去重不加()
        {
            var key = new object();
            var pool = new NpcGoalPool();
            Assert.IsTrue(pool.Add(new MoveGoal { GoalKey = key }));
            Assert.IsFalse(pool.Add(new MoveGoal { GoalKey = key }), "同键应被拒");
            Assert.AreEqual(1, pool.Count);
        }

        [Test]
        public void Pool_Remove生效()
        {
            var pool = new NpcGoalPool();
            var g = Move(100f);
            pool.Add(g);
            Assert.IsTrue(pool.Remove(g));
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void Pool_GetGoalsOfType_只返回该类型()
        {
            var pool = new NpcGoalPool();
            pool.Add(Move(100f));
            pool.Add(new RestGoal { DefaultScore = 50f });
            Assert.AreEqual(1, pool.GetGoalsOfType(typeof(MoveGoal)).Count);
            Assert.AreEqual(1, pool.GetGoalsOfType(typeof(RestGoal)).Count);
        }

        // ── ActivitySelector ──

        [Test]
        public void Select_单goal_切换到它()
        {
            var move = new MoveActivity();
            var sel = new ActivitySelector(new NpcActivity[] { move });
            var pool = new NpcGoalPool();
            var g = Move(100f);
            pool.Add(g);

            var r = sel.Select(pool, new StubContext(), null, null);
            Assert.AreEqual(SelectionKind.Switch, r.Kind);
            Assert.AreSame(move, r.Activity);
            Assert.AreSame(g, r.Goal);
        }

        [Test]
        public void Select_多goal竞争_选最高分_吃饭压过摆摊()
        {
            var move = new MoveActivity();
            var sel = new ActivitySelector(new NpcActivity[] { move });
            var pool = new NpcGoalPool();
            var stall = Move(100f); // 摆摊
            var eat = Move(300f);   // 吃饭（分高）
            pool.Add(stall);
            pool.Add(eat);

            var r = sel.Select(pool, new StubContext(), null, null);
            Assert.AreEqual(SelectionKind.Switch, r.Kind);
            Assert.AreSame(eat, r.Goal);
        }

        [Test]
        public void Select_空池_None()
        {
            var sel = new ActivitySelector(new NpcActivity[] { new MoveActivity() });
            var r = sel.Select(new NpcGoalPool(), new StubContext(), null, null);
            Assert.AreEqual(SelectionKind.None, r.Kind);
        }

        [Test]
        public void Select_过期goal_被清出池且不选()
        {
            var sel = new ActivitySelector(new NpcActivity[] { new MoveActivity() });
            var pool = new NpcGoalPool();
            var expired = Move(500f, lifetime: 100f, creation: 800f); // 到 1000 过期
            pool.Add(expired);

            var r = sel.Select(pool, new StubContext { CurrentTime = 1000f }, null, null);
            Assert.AreEqual(SelectionKind.None, r.Kind);
            Assert.AreEqual(0, pool.Count, "过期 goal 应被清出池");
        }

        [Test]
        public void Select_当前在跑即最优_NoChange不重启()
        {
            var move = new MoveActivity();
            var sel = new ActivitySelector(new NpcActivity[] { move });
            var pool = new NpcGoalPool();
            var g = Move(100f);
            pool.Add(g);

            var r = sel.Select(pool, new StubContext(), move, g); // 当前正跑 move/g
            Assert.AreEqual(SelectionKind.NoChange, r.Kind);
        }

        [Test]
        public void Select_出现更高分goal_从当前切换()
        {
            var move = new MoveActivity();
            var sel = new ActivitySelector(new NpcActivity[] { move });
            var pool = new NpcGoalPool();
            var low = Move(100f);
            var high = Move(400f);
            pool.Add(low);
            pool.Add(high);

            var r = sel.Select(pool, new StubContext(), move, low); // 当前跑 low
            Assert.AreEqual(SelectionKind.Switch, r.Kind);
            Assert.AreSame(high, r.Goal);
        }

        [Test]
        public void Select_跨activity_选全局最高分()
        {
            var move = new MoveActivity();
            var rest = new RestActivity();
            var sel = new ActivitySelector(new NpcActivity[] { move, rest });
            var pool = new NpcGoalPool();
            pool.Add(Move(100f));                              // MoveGoal 100
            var bestRest = new RestGoal { DefaultScore = 250f }; // RestGoal 250
            pool.Add(bestRest);

            var r = sel.Select(pool, new StubContext(), null, null);
            Assert.AreEqual(SelectionKind.Switch, r.Kind);
            Assert.AreSame(rest, r.Activity);
            Assert.AreSame(bestRest, r.Goal);
        }
    }
}

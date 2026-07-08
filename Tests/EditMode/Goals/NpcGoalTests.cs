// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using NUnit.Framework;
using UnityEngine;
using Likeon.GAS;

namespace Likeon.GAS.AI.Tests
{
    /// <summary>
    /// NpcGoal 打分相关语义测试：分数 / override / 去重键 / 标签门禁 / 存活期。
    /// </summary>
    public class NpcGoalTests
    {
        private const float Eps = 1e-4f;

        // 用于测 override 生效的子类：饥饿时分数抬高。
        private sealed class HungryGoal : NpcGoal
        {
            public override float GetScore() => DefaultScore + 500f;
        }

        private static GameplayTagContainer Container(params string[] names)
        {
            var c = new GameplayTagContainer();
            foreach (var n in names) c.AddTag(GameplayTag.RequestTag(n));
            return c;
        }

        [Test]
        public void GetScore_默认返回DefaultScore()
        {
            var goal = new MoveGoal(Vector3.zero, 120f);
            Assert.AreEqual(120f, goal.GetScore(), Eps);
        }

        [Test]
        public void GetScore_子类override生效()
        {
            var goal = new HungryGoal { DefaultScore = 100f };
            Assert.AreEqual(600f, goal.GetScore(), Eps);
        }

        [Test]
        public void GetKey_未设GoalKey时返回自身()
        {
            var goal = new MoveGoal();
            Assert.AreSame(goal, goal.GetKey());
        }

        [Test]
        public void GetKey_设了GoalKey时返回它()
        {
            var key = new object();
            var goal = new MoveGoal { GoalKey = key };
            Assert.AreSame(key, goal.GetKey());
        }

        [Test]
        public void PassesTagGate_无门禁_通过()
        {
            var goal = new MoveGoal();
            Assert.IsTrue(goal.PassesTagGate(Container("State.Idle")));
        }

        [Test]
        public void PassesTagGate_owner命中BlockTags_不通过()
        {
            var goal = new MoveGoal { BlockTags = Container("State.Sleeping") };
            Assert.IsFalse(goal.PassesTagGate(Container("State.Sleeping")));
        }

        [Test]
        public void PassesTagGate_owner缺RequireTags_不通过()
        {
            var goal = new MoveGoal { RequireTags = Container("State.Awake") };
            Assert.IsFalse(goal.PassesTagGate(Container("State.Idle")));
        }

        [Test]
        public void PassesTagGate_owner满足RequireTags且无Block_通过()
        {
            var goal = new MoveGoal { RequireTags = Container("State.Awake") };
            Assert.IsTrue(goal.PassesTagGate(Container("State.Awake", "State.Idle")));
        }

        [Test]
        public void IsExpired_Lifetime非正_永不过期()
        {
            var goal = new MoveGoal { GoalLifetime = 0f, CreationTime = 100f };
            Assert.IsFalse(goal.IsExpired(999999f));
        }

        [Test]
        public void IsExpired_未到期_false()
        {
            var goal = new MoveGoal { GoalLifetime = 400f, CreationTime = 800f };
            Assert.IsFalse(goal.IsExpired(1000f)); // 已过 200 < 400
        }

        [Test]
        public void IsExpired_到期_true()
        {
            var goal = new MoveGoal { GoalLifetime = 400f, CreationTime = 800f };
            Assert.IsTrue(goal.IsExpired(1200f)); // 已过 400 >= 400
        }
    }
}

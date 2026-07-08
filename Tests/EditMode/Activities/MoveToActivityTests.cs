// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using NUnit.Framework;
using UnityEngine;

namespace Likeon.GAS.AI.Tests
{
    /// <summary>
    /// MoveToActivity 执行钩子测试（用 stub agent，不碰 NavMesh）：OnEnter 走向目标点、Tick 到达判完成、OnExit 停。
    /// </summary>
    public class MoveToActivityTests
    {
        private sealed class StubAgent : INpcAgent
        {
            public Vector3 Position { get; set; }
            public Vector3 LastDestination;
            public int MoveToCalls;
            public bool StopCalled;
            public bool ReachedDestination { get; set; }

            public void MoveTo(Vector3 destination) { LastDestination = destination; MoveToCalls++; }
            public void StopMoving() { StopCalled = true; }
        }

        [Test]
        public void SupportedGoalType_是MoveGoal()
        {
            Assert.AreEqual(typeof(MoveGoal), new MoveToActivity().SupportedGoalType);
        }

        [Test]
        public void OnEnter_命令agent走向goal的目标点()
        {
            var act = new MoveToActivity();
            var agent = new StubAgent();
            act.OnEnter(agent, new MoveGoal(new Vector3(1f, 2f, 3f), 100f));
            Assert.AreEqual(1, agent.MoveToCalls);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), agent.LastDestination);
        }

        [Test]
        public void Tick_未到达_返回false()
        {
            var act = new MoveToActivity();
            Assert.IsFalse(act.Tick(new StubAgent { ReachedDestination = false }));
        }

        [Test]
        public void Tick_已到达_返回true表示完成()
        {
            var act = new MoveToActivity();
            Assert.IsTrue(act.Tick(new StubAgent { ReachedDestination = true }));
        }

        [Test]
        public void OnExit_停止移动()
        {
            var act = new MoveToActivity();
            var agent = new StubAgent();
            act.OnExit(agent);
            Assert.IsTrue(agent.StopCalled);
        }
    }
}

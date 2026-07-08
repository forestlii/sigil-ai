// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using NUnit.Framework;
using UnityEngine;
using Likeon.GAS;

namespace Likeon.GAS.AI.Tests
{
    /// <summary>
    /// 作息驱动 goal 增删测试：进窗口发、离窗口撤、幂等、盖 CreationTime、窗口重叠发多个、目标点/分数带过来。
    /// </summary>
    public class ScheduleGoalDriverTests
    {
        private const float Eps = 1e-4f;

        private static ScheduleEntry Entry(float start, float end, float score = 100f, string tag = "Goal.Move")
            => new ScheduleEntry
            {
                StartTime = start, EndTime = end, Score = score,
                GoalTag = GameplayTag.RequestTag(tag),
                Destination = new Vector3(start, 0f, end),
            };

        [Test]
        public void 进入窗口_发goal进池()
        {
            var driver = new ScheduleGoalDriver(new ActivitySchedule(new[] { Entry(900f, 1800f) }));
            var pool = new NpcGoalPool();
            driver.Sync(1000f, 1000f, pool);
            Assert.AreEqual(1, pool.Count);
        }

        [Test]
        public void 窗口外_不发()
        {
            var driver = new ScheduleGoalDriver(new ActivitySchedule(new[] { Entry(900f, 1800f) }));
            var pool = new NpcGoalPool();
            driver.Sync(800f, 800f, pool);
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void 已发_再Sync不重发_幂等()
        {
            var driver = new ScheduleGoalDriver(new ActivitySchedule(new[] { Entry(900f, 1800f) }));
            var pool = new NpcGoalPool();
            driver.Sync(1000f, 1000f, pool);
            driver.Sync(1100f, 1100f, pool);
            Assert.AreEqual(1, pool.Count);
        }

        [Test]
        public void 离开窗口_撤出池()
        {
            var driver = new ScheduleGoalDriver(new ActivitySchedule(new[] { Entry(900f, 1800f) }));
            var pool = new NpcGoalPool();
            driver.Sync(1000f, 1000f, pool); // 发
            driver.Sync(2000f, 2000f, pool); // 离开 → 撤
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void 发goal时盖CreationTime为accumulatedTime()
        {
            var driver = new ScheduleGoalDriver(new ActivitySchedule(new[] { Entry(900f, 1800f) }));
            var pool = new NpcGoalPool();
            driver.Sync(1000f, 1234f, pool);
            Assert.AreEqual(1234f, pool.Goals[0].CreationTime, Eps);
        }

        [Test]
        public void 窗口重叠_发多个goal()
        {
            var sched = new ActivitySchedule(new[]
            {
                Entry(900f, 1800f, tag: "Goal.Move.Stall"),
                Entry(1200f, 1300f, tag: "Goal.Move.Eat"),
            });
            var driver = new ScheduleGoalDriver(sched);
            var pool = new NpcGoalPool();
            driver.Sync(1230f, 1230f, pool);
            Assert.AreEqual(2, pool.Count);
        }

        [Test]
        public void goal携带entry的目标点与分数()
        {
            var sched = new ActivitySchedule(new[] { Entry(800f, 1200f, score: 150f, tag: "Goal.Move.Bs") });
            var driver = new ScheduleGoalDriver(sched);
            var pool = new NpcGoalPool();
            driver.Sync(900f, 900f, pool);
            var g = (MoveGoal)pool.Goals[0];
            Assert.AreEqual(150f, g.DefaultScore, Eps);
            Assert.AreEqual(new Vector3(800f, 0f, 1200f), g.Destination);
        }
    }
}

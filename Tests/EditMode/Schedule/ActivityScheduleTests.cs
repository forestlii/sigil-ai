// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Likeon.GAS;

namespace Likeon.GAS.AI.Tests
{
    /// <summary>
    /// 作息表定时逻辑测试（M-AI-1 自动测主体）：窗口边界、跨午夜、全天、**窗口重叠→多 goal 并存**、造 goal。
    /// 时刻用 0–2400 制：800=8:00、1200=正午、1800=18:00、2200=22:00。
    /// </summary>
    public class ActivityScheduleTests
    {
        private const float Eps = 1e-4f;

        private static ScheduleEntry Entry(float start, float end, float score = 100f, string tag = "Goal.Move")
        {
            return new ScheduleEntry
            {
                StartTime = start,
                EndTime = end,
                Score = score,
                GoalTag = GameplayTag.RequestTag(tag),
                Destination = new Vector3(start, 0f, end),
            };
        }

        // ── 单窗口 IsActiveAt ──

        [Test]
        public void IsActiveAt_正常窗口内_激活()
        {
            Assert.IsTrue(Entry(900f, 1800f).IsActiveAt(1200f));
        }

        [Test]
        public void IsActiveAt_含起点_不含终点()
        {
            var e = Entry(900f, 1800f);
            Assert.IsTrue(e.IsActiveAt(900f), "起点应含");
            Assert.IsFalse(e.IsActiveAt(1800f), "终点应不含");
        }

        [Test]
        public void IsActiveAt_窗口外_不激活()
        {
            Assert.IsFalse(Entry(900f, 1800f).IsActiveAt(800f));
        }

        [Test]
        public void IsActiveAt_跨午夜窗口_两端都激活中间不激活()
        {
            var sleep = Entry(2200f, 600f); // 22:00 → 次日 06:00
            Assert.IsTrue(sleep.IsActiveAt(2300f), "午夜前");
            Assert.IsTrue(sleep.IsActiveAt(500f), "午夜后");
            Assert.IsFalse(sleep.IsActiveAt(1200f), "白天不激活");
        }

        [Test]
        public void IsActiveAt_StartEqualsEnd_全天激活()
        {
            var allDay = Entry(1000f, 1000f);
            Assert.IsTrue(allDay.IsActiveAt(0f));
            Assert.IsTrue(allDay.IsActiveAt(1000f));
            Assert.IsTrue(allDay.IsActiveAt(2399f));
        }

        // ── 多条目 + 窗口重叠（竞争来源）──

        [Test]
        public void GetActiveEntries_无重叠_只返回当前窗口()
        {
            var sched = new ActivitySchedule(new[]
            {
                Entry(900f, 1800f, tag: "Goal.Move.Stall"),   // 摆摊
                Entry(1200f, 1300f, tag: "Goal.Move.Eat"),    // 吃饭
            });
            var active = sched.GetActiveEntries(1000f); // 只在摆摊窗口
            Assert.AreEqual(1, active.Count);
            Assert.AreEqual("Goal.Move.Stall", active[0].GoalTag.TagName);
        }

        [Test]
        public void GetActiveEntries_窗口重叠_返回多条()
        {
            var sched = new ActivitySchedule(new[]
            {
                Entry(900f, 1800f, tag: "Goal.Move.Stall"),   // 09-18 摆摊
                Entry(1200f, 1300f, tag: "Goal.Move.Eat"),    // 12-13 吃饭（重叠）
            });
            var active = sched.GetActiveEntries(1230f); // 正午 12:30，两窗口都激活
            Assert.AreEqual(2, active.Count);
            CollectionAssert.AreEquivalent(
                new[] { "Goal.Move.Stall", "Goal.Move.Eat" },
                active.Select(e => e.GoalTag.TagName).ToArray());
        }

        [Test]
        public void GetActiveEntries_都不激活_返回空()
        {
            var sched = new ActivitySchedule(new[] { Entry(900f, 1800f) });
            Assert.IsEmpty(sched.GetActiveEntries(2000f));
        }

        // ── 造 goal ──

        [Test]
        public void CreateGoal_携带目标点分数与标签()
        {
            var e = Entry(800f, 1200f, score: 150f, tag: "Goal.Move.Blacksmith");
            var goal = e.CreateGoal();
            Assert.IsNotNull(goal);
            Assert.AreEqual(150f, goal.DefaultScore, Eps);
            Assert.AreEqual(150f, goal.GetScore(), Eps);
            Assert.AreEqual("Goal.Move.Blacksmith", goal.GoalTag.TagName);
            Assert.AreEqual(new Vector3(800f, 0f, 1200f), goal.Destination);
        }
    }
}

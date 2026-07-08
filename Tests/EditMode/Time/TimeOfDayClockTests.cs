// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;
using NUnit.Framework;

namespace Likeon.GAS.AI.Tests
{
    /// <summary>
    /// 时钟推进逻辑（纯逻辑）测试。制式 = 0–2400 四位时钟：800=8:00、1200=正午、2100=21:00。
    /// </summary>
    public class TimeOfDayClockTests
    {
        private const float Eps = 1e-4f;

        [Test]
        public void 构造_初始时刻即为当前TimeOfDay与AccumulatedTime()
        {
            var clock = new TimeOfDayClock(800f); // 8:00
            Assert.AreEqual(800f, clock.TimeOfDay, Eps);
            Assert.AreEqual(800f, clock.AccumulatedTime, Eps);
        }

        [Test]
        public void Advance_同日内累加TimeOfDay()
        {
            var clock = new TimeOfDayClock(800f); // 8:00
            clock.Advance(400f);                  // +4h → 1200 正午
            Assert.AreEqual(1200f, clock.TimeOfDay, Eps);
        }

        [Test]
        public void Advance_跨午夜_TimeOfDay回绕到次日()
        {
            var clock = new TimeOfDayClock(2300f); // 23:00
            clock.Advance(200f);                   // +2h → 次日 01:00
            Assert.AreEqual(100f, clock.TimeOfDay, Eps);
        }

        [Test]
        public void Advance_AccumulatedTime不回绕_并记录天数()
        {
            var clock = new TimeOfDayClock(2300f);
            clock.Advance(200f); // 总累计 2500
            Assert.AreEqual(2500f, clock.AccumulatedTime, Eps);
            Assert.AreEqual(1, clock.Day);
        }

        [Test]
        public void Advance_多日累加_天数正确()
        {
            var clock = new TimeOfDayClock(0f);
            clock.Advance(2400f * 3f + 1200f); // 3 天又半天
            Assert.AreEqual(1200f, clock.TimeOfDay, Eps);
            Assert.AreEqual(3, clock.Day);
        }

        [Test]
        public void Advance_负增量_抛异常()
        {
            var clock = new TimeOfDayClock();
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(-1f));
        }
    }
}

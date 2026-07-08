// Copyright (c) 2026 Likeon. Licensed under the MIT License.

namespace Likeon.GAS.AI
{
    /// <summary>
    /// 一天中的时间。时钟制式：
    /// <see cref="TimeOfDay"/> 为 0–2400 的"四位时钟"（800 = 8:00、1200 = 正午、2100 = 21:00），
    /// 2400 = 一整天；<see cref="AccumulatedTime"/> 为不回绕的总累计时间。
    /// 供 schedule / activity 层读取当前时刻，具体推进由实现（如 <see cref="TimeOfDayClock"/>）负责。
    /// </summary>
    public interface ITimeOfDay
    {
        /// <summary>当前一天中的时间，落在 [0, 2400)。</summary>
        float TimeOfDay { get; }

        /// <summary>总累计时间，2400 = 一天，不回绕。</summary>
        float AccumulatedTime { get; }
    }
}

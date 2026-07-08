// Copyright (c) 2026 Likeon. Licensed under the MIT License.

using System;

namespace Likeon.GAS.AI
{
    /// <summary>
    /// 纯逻辑时钟（POCO，零 UnityEngine 依赖，可在 EditMode 或 dotnet 直接测）。
    /// 时间推进逻辑：累加
    /// <see cref="AccumulatedTime"/>，当前时刻 <see cref="TimeOfDay"/> = Fmod(AccumulatedTime, 2400)。
    /// 现实秒 → 游戏时间的换算由上层组件负责，本类只吃 2400 制的时间增量。
    /// </summary>
    public sealed class TimeOfDayClock : ITimeOfDay
    {
        /// <summary>一天的时间长度（2400 制）。</summary>
        public const float DayLength = 2400f;

        /// <summary>总累计时间，2400 = 一天，不回绕。</summary>
        public float AccumulatedTime { get; private set; }

        /// <summary>当前一天中的时间，[0, 2400)。= Fmod(AccumulatedTime, 2400)，负数也归一到该区间。</summary>
        public float TimeOfDay
        {
            get
            {
                float t = AccumulatedTime % DayLength;
                return t < 0f ? t + DayLength : t;
            }
        }

        /// <summary>第几天（从 0 起），= floor(AccumulatedTime / 2400)。</summary>
        public int Day => (int)Math.Floor(AccumulatedTime / DayLength);

        public TimeOfDayClock(float initialTimeOfDay = 0f)
        {
            AccumulatedTime = initialTimeOfDay;
        }

        /// <summary>
        /// 推进游戏时间（2400 制的增量，须 &gt;= 0），累加到 AccumulatedTime。
        /// </summary>
        public void Advance(float deltaGameUnits)
        {
            if (deltaGameUnits < 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(deltaGameUnits), "时间只能前进，delta 不能为负。");

            AccumulatedTime += deltaGameUnits;
        }
    }
}

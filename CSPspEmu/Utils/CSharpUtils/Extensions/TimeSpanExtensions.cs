using System;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static long GetTotalMicroseconds(this TimeSpan timeSpan)
        {
            return timeSpan.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
        }
    }
}
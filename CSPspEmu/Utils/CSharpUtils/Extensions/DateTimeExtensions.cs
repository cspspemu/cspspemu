using System;

namespace CSharpUtils.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetTotalNanoseconds(this DateTime dateTime)
        {
            return dateTime.Ticks * 10;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetTotalMicroseconds(this DateTime dateTime)
        {
            return dateTime.Ticks / 10;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="microseconds"></param>
        /// <returns></returns>
        public static DateTime FromMicroseconds(long microseconds)
        {
            return new DateTime(microseconds * 10);
        }
    }
}
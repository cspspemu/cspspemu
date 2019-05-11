using System;
using System.Threading;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class ThreadUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="until"></param>
        public static void SleepUntilUtc(DateTime until)
        {
            var duration = until - DateTime.UtcNow;
            if (duration.TotalSeconds < 0) return;
            Thread.Sleep(duration);
        }
    }
}
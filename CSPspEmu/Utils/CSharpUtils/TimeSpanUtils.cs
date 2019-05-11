using System;
using System.Timers;
using CSPspEmu.Utils;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public static class TimeSpanUtils
    {
        public static TimeSpan FromMicroseconds(long microseconds) => microseconds.Microseconds();

        /// <summary>
        /// </summary>
        /// <param name="description"></param>
        /// <param name="action"></param>
        /// <param name="loopAction"></param>
        public static void InfiniteLoopDetector(string description, Action action, Action loopAction = null)
        {
            using (var timer = new Timer(4.0 * 1000))
            {
                bool[] cancel = {false};
                timer.Elapsed += (sender, e) =>
                {
                    if (cancel[0]) return;
                    Console.WriteLine("InfiniteLoop Detected! : {0} : {1}", description, e.SignalTime);
                    loopAction?.Invoke();
                };
                timer.AutoReset = false;
                timer.Start();
                try
                {
                    action();
                }
                finally
                {
                    cancel[0] = true;
                    timer.Enabled = false;
                    timer.Stop();
                }
            }
        }
    }
}
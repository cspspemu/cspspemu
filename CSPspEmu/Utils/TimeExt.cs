using System;

namespace CSPspEmu.Utils
{
    static public class TimeExt
    {
        static public TimeSpan Microseconds(this long value) => TimeSpan.FromMilliseconds((double) value / (double) 1000.0);
        static public TimeSpan Milliseconds(this long value) => TimeSpan.FromMilliseconds(value);
        static public TimeSpan Seconds(this long value) => TimeSpan.FromSeconds(value);

        static public TimeSpan Microseconds(this int value) => TimeSpan.FromMilliseconds((double) value / (double) 1000.0);
        static public TimeSpan Milliseconds(this int value) => TimeSpan.FromMilliseconds(value);
        static public TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);

        static public TimeSpan Microseconds(this double value) => TimeSpan.FromMilliseconds((double) value / (double) 1000.0);
        static public TimeSpan Milliseconds(this double value) => TimeSpan.FromMilliseconds(value);
        static public TimeSpan Seconds(this double value) => TimeSpan.FromSeconds(value);
        
    }
}
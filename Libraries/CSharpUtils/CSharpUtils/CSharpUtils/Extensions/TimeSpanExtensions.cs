using System;

public static class TimeSpanExtensions
{
    public static long GetTotalMicroseconds(this TimeSpan TimeSpan)
    {
        return (long) TimeSpan.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
    }
}
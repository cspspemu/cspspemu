using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils
{
    public class TimeSampler
    {
        readonly Dictionary<DateTime, double> _samples = new Dictionary<DateTime, double>();

        public static TimeSpan Measure(Action action)
        {
            var start = DateTime.UtcNow;
            action();
            var end = DateTime.UtcNow;
            return end - start;
        }

        public void AddAt(DateTime dateTime, double sample)
        {
            _samples[dateTime] = sample;
        }

        public void AddNow(double sample)
        {
            AddAt(DateTime.UtcNow, sample);
        }

        public double GetInterpolatedSampleAt(DateTime time)
        {
            throw(new NotImplementedException());
        }

        public DateTime GetNearestDateTimeAt(DateTime time)
        {
            return _samples
                    .OrderBy(item => (time - item.Key).Duration())
                    .First()
                    .Key
                ;
        }

        /*
        public DateTime GetLowerDateTimeAt(DateTime Time)
        {
            return Samples
                .Where(Item => Item.Key <= Time)
                .OrderByDescending(Item => Item.Key)
                .First()
                .Key
            ;
        }

        public DateTime GetUpperDateTimeAt(DateTime Time)
        {
            return Samples
                .Where(Item => Item.Key >= Time)
                .OrderBy(Item => Item.Key)
                .First()
                .Key
            ;
        }*/

        public double GetSampleAt(DateTime time)
        {
            return _samples[time];
        }

        public double GetIncrementPerSecond(TimeSpan timeSpan, DateTime dateTimeNow)
        {
            //var Time1 = GetLowerDateTimeAt(DateTimeNow - TimeSpan);
            //var Time2 = GetLowerDateTimeAt(DateTimeNow);
            var time1 = GetNearestDateTimeAt(dateTimeNow - timeSpan);
            var time2 = GetNearestDateTimeAt(dateTimeNow);

            var sample1 = GetSampleAt(time1);
            var sample2 = GetSampleAt(time2);

            return (sample2 - sample1) / (time2 - time1).TotalSeconds;
        }

        public double GetIncrementPerSecond(TimeSpan timeSpan)
        {
            return GetIncrementPerSecond(timeSpan, DateTime.UtcNow);
        }

        public static double Difference(double a, double b)
        {
            return b - a;
        }

        public static double Interpolate(double a, double b, double step)
        {
            return Difference(b, a) * step + a;
        }
    }
}
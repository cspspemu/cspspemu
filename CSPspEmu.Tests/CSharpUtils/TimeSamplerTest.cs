using System;
using CSharpUtils;
using Xunit;

namespace CSharpUtilsTests
{
    
    public class TimeSamplerTest
    {
        [Fact]
        public void GetLastIncrementTest()
        {
            var TimeSampler = new TimeSampler();
            var DateTimeStart = DateTime.UtcNow;
            var DateTime0 = DateTimeStart;
            var DateTime1 = DateTimeStart + TimeSpan.FromMilliseconds(20);
            var DateTime2 = DateTimeStart + TimeSpan.FromMilliseconds(1100);
            TimeSampler.AddAt(DateTime0, 100);
            TimeSampler.AddAt(DateTime1, 200);
            TimeSampler.AddAt(DateTime2, 400);
            Assert.Equal(DateTime0, TimeSampler.GetNearestDateTimeAt(DateTimeStart + TimeSpan.FromMilliseconds(5)));
            Console.WriteLine(TimeSampler.GetNearestDateTimeAt(DateTimeStart + TimeSpan.FromMilliseconds(5)));
            Console.WriteLine(TimeSampler.GetNearestDateTimeAt(DateTimeStart + TimeSpan.FromMilliseconds(15)));
            Console.WriteLine(TimeSampler.GetNearestDateTimeAt(DateTimeStart + TimeSpan.FromMilliseconds(1000)));
            Console.WriteLine(TimeSampler.GetNearestDateTimeAt(DateTimeStart + TimeSpan.FromMilliseconds(2000)));
            var Increment = TimeSampler.GetIncrementPerSecond(TimeSpan.FromMilliseconds(300), DateTimeStart);

            Console.WriteLine(Increment);
        }
    }
}
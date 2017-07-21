using System;
using CSharpUtils;
using Xunit;


namespace CSharpUtilsTests
{
    public class DateTimeRangeTest
    {
        [Fact]
        public void EqualSamePrecisionHourTest()
        {
            var d1 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 34, 10), DateTimeRange.PrecisionType.Hours);
            var d2 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 1, 4), DateTimeRange.PrecisionType.Hours);
            Assert.Equal(d1, d2);
            Assert.Equal(d2, d1);
        }

        [Fact]
        public void EqualDifferentPrecisionHourMinuteTest()
        {
            var d1 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 34, 10), DateTimeRange.PrecisionType.Minutes);
            var d2 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 1, 4), DateTimeRange.PrecisionType.Hours);
            Assert.Equal(d1, d2);
            Assert.Equal(d2, d1);
        }

        [Fact]
        public void NotEqualSamePrecisionHourTest()
        {
            var d1 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 34, 10), DateTimeRange.PrecisionType.Hours);
            var d2 = new DateTimeRange(new DateTime(2011, 4, 4, 11, 20, 10), DateTimeRange.PrecisionType.Hours);
            Assert.NotEqual(d1, d2);
            Assert.NotEqual(d2, d1);
        }
    }
}
using System;
using CSharpUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class DateTimeRangeTest
    {
        [TestMethod]
        public void EqualSamePrecisionHourTest()
        {
            var d1 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 34, 10), DateTimeRange.PrecisionType.Hours);
            var d2 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 1, 4), DateTimeRange.PrecisionType.Hours);
            Assert.AreEqual(d1, d2);
            Assert.AreEqual(d2, d1);
        }

        [TestMethod]
        public void EqualDifferentPrecisionHourMinuteTest()
        {
            var d1 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 34, 10), DateTimeRange.PrecisionType.Minutes);
            var d2 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 1, 4), DateTimeRange.PrecisionType.Hours);
            Assert.AreEqual(d1, d2);
            Assert.AreEqual(d2, d1);
        }

        [TestMethod]
        public void NotEqualSamePrecisionHourTest()
        {
            var d1 = new DateTimeRange(new DateTime(2011, 4, 4, 10, 34, 10), DateTimeRange.PrecisionType.Hours);
            var d2 = new DateTimeRange(new DateTime(2011, 4, 4, 11, 20, 10), DateTimeRange.PrecisionType.Hours);
            Assert.AreNotEqual(d1, d2);
            Assert.AreNotEqual(d2, d1);
        }
    }
}
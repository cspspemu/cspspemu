using System;
using System.Collections.Generic;
using CSharpUtils.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class ListExtensionsTest
    {
        [TestMethod]
        public void LowerBoundTest()
        {
            var Items = new List<Int32>(new int[] {10, 20, 30, 40, 50, 60, 70, 80, 90, 100});
            Assert.AreEqual("70,60,50,40,30,20,10", Items.LowerBound(75).ToStringArray());
            Assert.AreEqual("70,60,50,40,30,20,10", Items.LowerBound(70, true).ToStringArray());
            Assert.AreEqual("60,50,40,30,20,10", Items.LowerBound(70, false).ToStringArray());

            Assert.AreEqual("70,80,90,100", Items.UpperBound(65).ToStringArray());
            Assert.AreEqual("70,80,90,100", Items.UpperBound(70, true).ToStringArray());
            Assert.AreEqual("80,90,100", Items.UpperBound(70, false).ToStringArray());
        }

        [TestMethod]
        public void LowerBoundEmptyTest()
        {
            var Items0 = new List<Int32>(new int[] { });
            Assert.AreEqual("", Items0.LowerBound(75).ToStringArray());
            Assert.AreEqual("", Items0.UpperBound(65).ToStringArray());
        }

        [TestMethod]
        public void LowerBoundOneTest()
        {
            var Items1 = new List<Int32>(new int[] {1});
            Assert.AreEqual("1", Items1.LowerBound(75).ToStringArray());
            Assert.AreEqual("", Items1.UpperBound(65).ToStringArray());
        }
    }
}
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils;

namespace CSharpUtilsTests
{
    [TestClass]
    public class MathUtilsTest
    {
        [TestMethod]
        public void TestNextAligned()
        {
            Assert.AreEqual(0x014000, (int) MathUtils.NextAligned((long) 0x013810, (long) 0x800));
        }
    }
}
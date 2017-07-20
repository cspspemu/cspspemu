using NUnit.Framework;
using CSharpUtils;

namespace CSharpUtilsTests
{
    [TestFixture]
    public class MathUtilsTest
    {
        [Test]
        public void TestNextAligned()
        {
            Assert.AreEqual(0x014000, (int) MathUtils.NextAligned((long) 0x013810, (long) 0x800));
        }
    }
}
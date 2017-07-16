using System;
using NUnit.Framework;
using CSharpUtils.Ext.Compression.Lz;

namespace Tests.CSharpUtils.Ext.Compression
{
    [TestFixture]
    public class RleMatcherTest
    {
        [Test]
        public void TestMatchEmpty()
        {
            var RleMatcher = new RleMatcher(new byte[] { });
            Assert.AreEqual(0, RleMatcher.Length);
            RleMatcher.Skip();
            Assert.AreEqual(0, RleMatcher.Length);
        }

        [Test]
        public void TestMatch3()
        {
            var RleMatcher = new RleMatcher(new byte[] {1, 1, 1});
            Assert.AreEqual(1, RleMatcher.Byte);
            Assert.AreEqual(3, RleMatcher.Length);
            RleMatcher.Skip();
            Assert.AreEqual(1, RleMatcher.Byte);
            Assert.AreEqual(2, RleMatcher.Length);
            RleMatcher.Skip();
            Assert.AreEqual(1, RleMatcher.Byte);
            Assert.AreEqual(1, RleMatcher.Length);
            RleMatcher.Skip();
            Assert.AreEqual(1, RleMatcher.Byte);
            Assert.AreEqual(0, RleMatcher.Length);
            RleMatcher.Skip();
            Assert.AreEqual(1, RleMatcher.Byte);
            Assert.AreEqual(0, RleMatcher.Length);
        }
    }
}
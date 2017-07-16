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
            var rleMatcher = new RleMatcher(new byte[] { });
            Assert.AreEqual(0, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.AreEqual(0, rleMatcher.Length);
        }

        [Test]
        public void TestMatch3()
        {
            byte bb = 77;
            var rleMatcher = new RleMatcher(new[] {bb, bb, bb});
            Assert.AreEqual(bb, rleMatcher.Byte);
            Assert.AreEqual(3, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.AreEqual(bb, rleMatcher.Byte);
            Assert.AreEqual(2, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.AreEqual(bb, rleMatcher.Byte);
            Assert.AreEqual(1, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.AreEqual(bb, rleMatcher.Byte);
            Assert.AreEqual(0, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.AreEqual(bb, rleMatcher.Byte);
            Assert.AreEqual(0, rleMatcher.Length);
        }
    }
}
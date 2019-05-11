
using CSharpUtils.Ext.Compression.Lz;
using Xunit;

namespace Tests.CSharpUtils.Ext.Compression
{
    
    public class RleMatcherTest
    {
        [Fact]
        public void TestMatchEmpty()
        {
            var rleMatcher = new RleMatcher(new byte[] { });
            Assert.Equal(0, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.Equal(0, rleMatcher.Length);
        }

        [Fact]
        public void TestMatch3()
        {
            byte bb = 77;
            var rleMatcher = new RleMatcher(new[] {bb, bb, bb});
            Assert.Equal(bb, rleMatcher.Byte);
            Assert.Equal(3, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.Equal(bb, rleMatcher.Byte);
            Assert.Equal(2, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.Equal(bb, rleMatcher.Byte);
            Assert.Equal(1, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.Equal(bb, rleMatcher.Byte);
            Assert.Equal(0, rleMatcher.Length);
            rleMatcher.Skip();
            Assert.Equal(bb, rleMatcher.Byte);
            Assert.Equal(0, rleMatcher.Length);
        }
    }
}
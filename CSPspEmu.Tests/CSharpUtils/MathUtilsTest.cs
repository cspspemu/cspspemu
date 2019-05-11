
using CSharpUtils;
using Xunit;

namespace CSharpUtilsTests
{
    
    public class MathUtilsTest
    {
        [Fact]
        public void TestNextAligned()
        {
            Assert.Equal(0x014000, (int) MathUtils.NextAligned((long) 0x013810, (long) 0x800));
        }
    }
}
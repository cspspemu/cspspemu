using CSPspEmu.Utils;
using Xunit;

namespace CSPspEmu.Tests
{
    public class IntExtTest
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal(0, 0.0.Interpolate(0, 100));
            Assert.Equal(50, 0.5.Interpolate(0, 100));
            Assert.Equal(100, 1.0.Interpolate(0, 100));
            
            Assert.Equal(50, 0.0.Interpolate(50, 100));
            Assert.Equal(75, 0.5.Interpolate(50, 100));
            Assert.Equal(100, 1.0.Interpolate(50, 100));
            
            Assert.Equal(100, 0.0.Interpolate(100, 50));
            Assert.Equal(75, 0.5.Interpolate(100, 50));
            Assert.Equal(50, 1.0.Interpolate(100, 50));

        }
    }
}
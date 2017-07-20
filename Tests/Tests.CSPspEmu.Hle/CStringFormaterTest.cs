
using CSPspEmu.Utils;
using Xunit;

namespace CSPspEmu.Core.Tests.Hle
{
    
    public class CStringFormaterTest
    {
        [Fact]
        public void TestSprintf()
        {
            var Expected = "Hello 0x00000001, 'World     '!";
            var Actual = CStringFormater.Sprintf("Hello 0x%08X, '%-10s'!", 1, "World");

            Assert.Equal(Expected, Actual);
        }
    }
}
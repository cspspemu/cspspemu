using CSPspEmu.Utils;
using Xunit;

namespace Tests.CSPspEmu.Hle
{
    
    public class CStringFormaterTest
    {
        [Fact]
        public void TestSprintf()
        {
            var expected = "Hello 0x00000001, 'World     '!";
            var actual = CStringFormater.Sprintf("Hello 0x%08X, '%-10s'!", 1, "World");

            Assert.Equal(expected, actual);
        }
    }
}
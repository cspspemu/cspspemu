using NUnit.Framework;
using CSPspEmu.Utils;

namespace CSPspEmu.Core.Tests.Hle
{
    [TestFixture]
    public class CStringFormaterTest
    {
        [Test]
        public void TestSprintf()
        {
            var Expected = "Hello 0x00000001, 'World     '!";
            var Actual = CStringFormater.Sprintf("Hello 0x%08X, '%-10s'!", 1, "World");

            Assert.AreEqual(Expected, Actual);
        }
    }
}
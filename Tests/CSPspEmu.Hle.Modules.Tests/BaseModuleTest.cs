using NUnit.Framework;

namespace CSPspEmu.Hle.Modules.Tests
{
    [TestFixture]
    public class BaseModuleTest
    {
        [SetUp]
        public void SetUp()
        {
            TestHleUtils.CreateInjectContext(this);
        }
    }
}
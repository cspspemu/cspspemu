using SafeILGenerator.Utils;
using NUnit.Framework;

namespace SafeILGenerator.Tests.Util
{
    [TestFixture]
    public class ILFieldInfoTest
    {
        public int Test;

        [Test]
        public void TestGetFieldInfo()
        {
            Assert.AreEqual(
                typeof(ILFieldInfoTest).GetField("Test"),
                IlFieldInfo.GetFieldInfo(() => Test)
            );
        }
    }
}
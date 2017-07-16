using NUnit.Framework;

namespace CSharpUtilsTests
{
    [TestFixture]
    public class TypeUtilsTest
    {
        public class MyType
        {
        }

        public class MyExtendedType1 : MyType
        {
        }

        public class MyExtendedType2 : MyType
        {
        }

        [Test]
        public void GetTypesExtendingTest()
        {
            /*
            Assert.Equals(
                "",
                TypeUtils.GetTypesExtending(typeof(MyType)).ToStringArray()
            );
            */
            Assert.Inconclusive();
        }
    }
}
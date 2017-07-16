using CSharpUtils.Extensions;
using NUnit.Framework;

namespace CSharpUtilsTests
{
    [TestFixture]
    public class StructExtensionsTest
    {
        public struct TestStruct
        {
            public int Field1;
            public int Field2;
            public int Field3;
            public string Field4;
        }

        [Test]
        public void ToStringDefaultTest()
        {
            Assert.AreEqual(
                "TestStruct(Field1=1,Field2=2,Field3=3,Field4=\"Hello World!\")",
                new TestStruct()
                {
                    Field1 = 1,
                    Field2 = 2,
                    Field3 = 3,
                    Field4 = "Hello World!",
                }.ToStringDefault()
            );
        }

        [Test]
        public void ToStringDefaultTestArray()
        {
            Assert.AreEqual(
                "[1, 2, 3, 4]",
                new int[] {1, 2, 3, 4}.ToStringDefault()
            );
        }

        [Test]
        public void ToStringDefaultTestString()
        {
            Assert.AreEqual(
                "\"Hello \\\"\\' World\"",
                "Hello \"' World".ToStringDefault()
            );
        }
    }
}
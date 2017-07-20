using CSharpUtils.Extensions;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class StructExtensionsTest
    {
        public struct TestStruct
        {
            public int Field1;
            public int Field2;
            public int Field3;
            public string Field4;
        }

        [Fact]
        public void ToStringDefaultTest()
        {
            Assert.Equal(
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

        [Fact]
        public void ToStringDefaultTestArray()
        {
            Assert.Equal(
                "[1, 2, 3, 4]",
                new int[] {1, 2, 3, 4}.ToStringDefault()
            );
        }

        [Fact]
        public void ToStringDefaultTestString()
        {
            Assert.Equal(
                "\"Hello \\\"\\' World\"",
                "Hello \"' World".ToStringDefault()
            );
        }
    }
}
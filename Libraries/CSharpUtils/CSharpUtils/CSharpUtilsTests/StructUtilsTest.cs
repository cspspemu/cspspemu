using CSharpUtils;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class StructUtilsTest
    {
        struct TestShorts
        {
            public short A, B, C;

            public override string ToString()
            {
                return $"TestShorts(0x{A:X4}, 0x{B:X4}, 0x{C:X4})";
            }
        }

        [Fact]
        public void BytesToStructArrayTest()
        {
            var Data = new byte[]
            {
                0x01, 0x00, 0x02, 0x00, 0x03, 0x00,
                0x01, 0x01, 0x02, 0x01, 0x03, 0x01,
                0x01, 0x02, 0x02, 0x02, 0x03, 0x02,
            };
            var TestShorts = StructUtils.BytesToStructArray<TestShorts>(Data);
            Assert.Equal("TestShorts(0x0001, 0x0002, 0x0003)", TestShorts[0].ToString());
            Assert.Equal("TestShorts(0x0101, 0x0102, 0x0103)", TestShorts[1].ToString());
            Assert.Equal("TestShorts(0x0201, 0x0202, 0x0203)", TestShorts[2].ToString());
        }
    }
}
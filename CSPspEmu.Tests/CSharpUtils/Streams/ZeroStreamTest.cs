using CSharpUtils.Extensions;
using CSharpUtils.Streams;
using Xunit;


namespace CSharpUtilsTests.Streams
{
    /// <summary>
    /// 
    /// </summary>
    
    public class ZeroStreamTest
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void TestRead()
        {
            var Stream = new ZeroStream(7, 0x11);
            byte[] Read1, Read2;
            Read1 = Stream.ReadBytesUpTo(3);
            Assert.Equal(new byte[] {0x11, 0x11, 0x11}, Read1);
            Read2 = Stream.ReadBytesUpTo(7);
            Assert.Equal(new byte[] {0x11, 0x11, 0x11, 0x11}, Read2);
        }
    }
}
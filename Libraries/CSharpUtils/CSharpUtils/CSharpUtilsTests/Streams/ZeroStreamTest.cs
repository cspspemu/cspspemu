using CSharpUtils.Extensions;
using CSharpUtils.Streams;
using NUnit.Framework;

namespace CSharpUtilsTests.Streams
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class ZeroStreamTest
    {
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestRead()
        {
            var Stream = new ZeroStream(7, 0x11);
            byte[] Read1, Read2;
            Read1 = Stream.ReadBytesUpTo(3);
            CollectionAssert.AreEqual(new byte[] {0x11, 0x11, 0x11}, Read1);
            Read2 = Stream.ReadBytesUpTo(7);
            CollectionAssert.AreEqual(new byte[] {0x11, 0x11, 0x11, 0x11}, Read2);
        }
    }
}
using System.IO;
using System.Text;

using CSharpUtils.Streams;
using Xunit;

namespace CSharpUtilsTests.Streams
{
    
    public class StreamChunkerTest
    {
        [Fact]
        public void TestMethod1()
        {
            var InputStream = new MemoryStream(Encoding.UTF8.GetBytes("A-//-BCD::E"));
            MemoryStream TempStream;

            StreamChunker test = new StreamChunker(InputStream);

            TempStream = new MemoryStream();
            test.CopyUpToSequence(TempStream, Encoding.UTF8.GetBytes("-//-"));
            Assert.Equal("A", Encoding.UTF8.GetString(TempStream.ToArray()));

            TempStream = new MemoryStream();
            test.CopyUpToSequence(TempStream, Encoding.UTF8.GetBytes("::"));
            Assert.Equal("BCD", Encoding.UTF8.GetString(TempStream.ToArray()));

            TempStream = new MemoryStream();
            test.CopyUpToSequence(TempStream, Encoding.UTF8.GetBytes("**"));
            Assert.Equal("E", Encoding.UTF8.GetString(TempStream.ToArray()));
        }
    }
}
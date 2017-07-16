using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils.Streams;

namespace CSharpUtilsTests.Streams
{
    [TestClass]
    public class StreamChunkerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var InputStream = new MemoryStream(Encoding.UTF8.GetBytes("A-//-BCD::E"));
            MemoryStream TempStream;

            StreamChunker test = new StreamChunker(InputStream);

            TempStream = new MemoryStream();
            test.CopyUpToSequence(TempStream, Encoding.UTF8.GetBytes("-//-"));
            Assert.AreEqual("A", Encoding.UTF8.GetString(TempStream.ToArray()));

            TempStream = new MemoryStream();
            test.CopyUpToSequence(TempStream, Encoding.UTF8.GetBytes("::"));
            Assert.AreEqual("BCD", Encoding.UTF8.GetString(TempStream.ToArray()));

            TempStream = new MemoryStream();
            test.CopyUpToSequence(TempStream, Encoding.UTF8.GetBytes("**"));
            Assert.AreEqual("E", Encoding.UTF8.GetString(TempStream.ToArray()));
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils.Streams;

namespace CSharpUtilsTests.Streams
{
    [TestClass]
    public class StreamChunker2Test
    {
        protected string BList(params string[] strings)
        {
            return String.Join(",", strings);
        }

        protected string ChunkStr(string Array, string ValueToFind)
        {
            return String.Join(",", StreamChunker2.SplitInChunks(
                new MemoryStream(Encoding.ASCII.GetBytes(Array)),
                Encoding.ASCII.GetBytes(ValueToFind)
            ).Select((Item) => Encoding.ASCII.GetString(Item)).ToArray());
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(BList("A", "A"), ChunkStr("A*******A", "*******"));
            Assert.AreEqual(BList("AA", "A"), ChunkStr("AA*******A", "*******"));
            Assert.AreEqual(BList("AAA", "A"), ChunkStr("AAA*******A", "*******"));
            Assert.AreEqual(BList("AAAA", "A"), ChunkStr("AAAA*******A", "*******"));
            Assert.AreEqual(BList("AAAAA", "A"), ChunkStr("AAAAA*******A", "*******"));
            Assert.AreEqual(BList("AAAAAA", "A"), ChunkStr("AAAAAA*******A", "*******"));
            Assert.AreEqual(BList("AAAAAAA", "A"), ChunkStr("AAAAAAA*******A", "*******"));
            Assert.AreEqual(BList("AAAAAAAA", "A"), ChunkStr("AAAAAAAA*******A", "*******"));
        }
    }
}
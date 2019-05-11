using System.IO;
using System.Text;
using System.Linq;

using CSharpUtils.Streams;
using Xunit;

namespace CSharpUtilsTests.Streams
{
    
    public class StreamChunker2Test
    {
        protected string BList(params string[] strings)
        {
            return string.Join(",", strings);
        }

        protected string ChunkStr(string Array, string ValueToFind)
        {
            return string.Join(",", StreamChunker2.SplitInChunks(
                new MemoryStream(Encoding.ASCII.GetBytes(Array)),
                Encoding.ASCII.GetBytes(ValueToFind)
            ).Select((Item) => Encoding.ASCII.GetString(Item)).ToArray());
        }

        [Fact]
        public void TestMethod1()
        {
            Assert.Equal(BList("A", "A"), ChunkStr("A*******A", "*******"));
            Assert.Equal(BList("AA", "A"), ChunkStr("AA*******A", "*******"));
            Assert.Equal(BList("AAA", "A"), ChunkStr("AAA*******A", "*******"));
            Assert.Equal(BList("AAAA", "A"), ChunkStr("AAAA*******A", "*******"));
            Assert.Equal(BList("AAAAA", "A"), ChunkStr("AAAAA*******A", "*******"));
            Assert.Equal(BList("AAAAAA", "A"), ChunkStr("AAAAAA*******A", "*******"));
            Assert.Equal(BList("AAAAAAA", "A"), ChunkStr("AAAAAAA*******A", "*******"));
            Assert.Equal(BList("AAAAAAAA", "A"), ChunkStr("AAAAAAAA*******A", "*******"));
        }
    }
}
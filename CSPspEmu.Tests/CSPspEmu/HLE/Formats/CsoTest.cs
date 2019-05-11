using System.IO;
using System.Linq;
using CSPspEmu.Hle.Formats;
using Xunit;

namespace Tests.CSPspEmu.Hle.Formats
{
    
    public class CsoTest
    {
        [Fact(Skip = "file not found")]
        public void ReadSectorDecompressedTest()
        {
            var cso = new Cso(File.OpenRead("../../../TestInput/cube.cso"));
            var isoBytes = File.ReadAllBytes("../../../TestInput/cube.iso");
            int expectedNumberOfBlocks = 229;
            int expectedBlockSize = 2048;
            Assert.Equal(expectedNumberOfBlocks, cso.NumberOfBlocks);
            Assert.Equal(expectedBlockSize, cso.BlockSize);
            for (uint block = 0; block < expectedNumberOfBlocks; block++)
            {
                var decompressedBlockData = cso.ReadBlocksDecompressed(block, 1)[0];
                Assert.Equal(
                    isoBytes.Skip((int) (expectedBlockSize * block)).Take(expectedBlockSize).ToArray(),
                    decompressedBlockData.ToArray()
                );
            }
        }
    }
}
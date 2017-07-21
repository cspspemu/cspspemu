using CSPspEmu.Hle.Formats;

using System.IO;
using System.Linq;
using Xunit;

namespace CSPspEmu.Core.Tests
{
    
    public class CsoTest
    {
        [Fact(Skip = "file not found")]
        public void ReadSectorDecompressedTest()
        {
            var Cso = new Cso(File.OpenRead("../../../TestInput/cube.cso"));
            var IsoBytes = File.ReadAllBytes("../../../TestInput/cube.iso");
            int ExpectedNumberOfBlocks = 229;
            int ExpectedBlockSize = 2048;
            Assert.Equal(ExpectedNumberOfBlocks, Cso.NumberOfBlocks);
            Assert.Equal(ExpectedBlockSize, Cso.BlockSize);
            for (uint Block = 0; Block < ExpectedNumberOfBlocks; Block++)
            {
                var DecompressedBlockData = Cso.ReadBlocksDecompressed(Block, 1)[0];
                Assert.Equal(
                    IsoBytes.Skip((int) (ExpectedBlockSize * Block)).Take(ExpectedBlockSize).ToArray(),
                    DecompressedBlockData.ToArray()
                );
            }
        }
    }
}
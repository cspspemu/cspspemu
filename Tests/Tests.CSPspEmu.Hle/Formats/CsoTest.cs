using CSPspEmu.Hle.Formats;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace CSPspEmu.Core.Tests
{
    [TestFixture]
    public class CsoTest
    {
        [Test]
        public void ReadSectorDecompressedTest()
        {
            var Cso = new Cso(File.OpenRead("../../../TestInput/cube.cso"));
            var IsoBytes = File.ReadAllBytes("../../../TestInput/cube.iso");
            int ExpectedNumberOfBlocks = 229;
            int ExpectedBlockSize = 2048;
            Assert.AreEqual(ExpectedNumberOfBlocks, Cso.NumberOfBlocks);
            Assert.AreEqual(ExpectedBlockSize, Cso.BlockSize);
            for (uint Block = 0; Block < ExpectedNumberOfBlocks; Block++)
            {
                var DecompressedBlockData = Cso.ReadBlocksDecompressed(Block, 1)[0];
                CollectionAssert.AreEqual(
                    IsoBytes.Skip((int) (ExpectedBlockSize * Block)).Take(ExpectedBlockSize).ToArray(),
                    DecompressedBlockData.ToArray()
                );
            }
        }
    }
}
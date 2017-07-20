using NUnit.Framework;
using CSPspEmu.Hle.Modules._unknownPrx;
using System.IO;

namespace CSPspEmu.Hle.Modules.Tests
{
    [TestFixture]
    public unsafe class sceDefltTest : BaseModuleTest
    {
        [Inject] sceDeflt sceDeflt = null;

        [Test]
        public void TestMethod1()
        {
            var inflated = File.ReadAllBytes("../../../TestInput/sample.inflated");
            var deflated = File.ReadAllBytes("../../../TestInput/sample.deflated");
            var buffer = new byte[inflated.Length];
            uint crc32;
            fixed (byte* buffer_ptr = buffer)
            fixed (byte* deflated_ptr = deflated)
            {
                sceDeflt.sceZlibDecompress(buffer_ptr, buffer.Length, deflated_ptr, &crc32);
            }

            CollectionAssert.AreEqual(inflated, buffer);

            //Assert.AreEqual((uint)0x3496193C, (uint)crc32);
            Assert.Inconclusive("Missing CRC check! We should check this is correct.");

            File.WriteAllBytes("../../../TestOutput/sample.inflated.again", buffer);
        }
    }
}
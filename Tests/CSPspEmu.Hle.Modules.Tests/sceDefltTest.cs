using System;
using NUnit.Framework;
using CSPspEmu.Hle.Modules._unknownPrx;
using System.IO;
using System.Net;
using System.Reflection;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Modules.Tests
{
    [TestFixture]
    public unsafe class sceDefltTest : BaseModuleTest
    {
        [Inject] sceDeflt sceDeflt = null;

        [Test]
        public void TestMethod1()
        {
            var inflated = ReadResourceBytes("sample.inflated");
            var deflated = ReadResourceBytes("sample.deflated");
            var buffer = new byte[inflated.Length];
            uint crc32;
            fixed (byte* bufferPtr = buffer)
            fixed (byte* deflatedPtr = deflated)
            {
                sceDeflt.sceZlibDecompress(bufferPtr, buffer.Length, deflatedPtr, &crc32);
            }

            CollectionAssert.AreEqual(inflated, buffer);

            //Assert.AreEqual((uint)0x3496193C, (uint)crc32);
            Assert.Inconclusive("Missing CRC check! We should check this is correct.");

            File.WriteAllBytes("../../../TestOutput/sample.inflated.again", buffer);
        }
    }
}
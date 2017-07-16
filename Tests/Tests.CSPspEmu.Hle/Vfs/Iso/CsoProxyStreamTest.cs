using CSPspEmu.Hle.Vfs.Iso;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using CSPspEmu.Hle.Formats;
using System.IO;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Tests
{
    [TestClass]
    public class CsoProxyStreamTest
    {
        [TestMethod]
        public void ReadTest()
        {
            var Cso = new Cso(File.OpenRead("../../../TestInput/cube.cso"));
            var IsoBytes = File.ReadAllBytes("../../../TestInput/cube.iso");
            var CsoStream = new CompressedIsoProxyStream(Cso);
            Assert.AreEqual(0x72800, CsoStream.Length);
            var Data = new byte[2048];
            var Data2 = new byte[3000];
            Assert.AreEqual(Data.Length, CsoStream.Read(Data, 0, Data.Length));
            Assert.AreEqual(Data.Length, CsoStream.Read(Data, 0, Data.Length));
            Assert.AreEqual(3000, CsoStream.Read(Data2, 0, 3000));

            CsoStream.Position = 0x72800 - Data.Length;
            Assert.AreEqual(Data.Length, CsoStream.Read(Data, 0, Data.Length));

            CsoStream.Position = 0x72800 - 10;
            Assert.AreEqual(10, CsoStream.Read(Data, 0, 10));

            CsoStream.Position = 0x72800 - 10;
            Assert.AreEqual(10, CsoStream.Read(Data, 0, 100));

            CollectionAssert.AreEqual(
                IsoBytes,
                CsoStream.ReadAll(true)
            );

            CsoStream.Position = 0x10 * 2048 - 100;
            CollectionAssert.AreEqual(
                IsoBytes.Slice(0x10 * 2048 - 100, 300),
                CsoStream.ReadBytes(300)
            );
        }
    }
}
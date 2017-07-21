using CSPspEmu.Hle.Vfs.Iso;

using CSPspEmu.Hle.Formats;
using System.IO;
using CSharpUtils.Extensions;
using Xunit;

namespace CSPspEmu.Core.Tests
{
    
    public class CsoProxyStreamTest
    {
        [Fact(Skip = "file not found")]
        public void ReadTest()
        {
            var Cso = new Cso(File.OpenRead("../../../TestInput/cube.cso"));
            var IsoBytes = File.ReadAllBytes("../../../TestInput/cube.iso");
            var CsoStream = new CompressedIsoProxyStream(Cso);
            Assert.Equal(0x72800, CsoStream.Length);
            var Data = new byte[2048];
            var Data2 = new byte[3000];
            Assert.Equal(Data.Length, CsoStream.Read(Data, 0, Data.Length));
            Assert.Equal(Data.Length, CsoStream.Read(Data, 0, Data.Length));
            Assert.Equal(3000, CsoStream.Read(Data2, 0, 3000));

            CsoStream.Position = 0x72800 - Data.Length;
            Assert.Equal(Data.Length, CsoStream.Read(Data, 0, Data.Length));

            CsoStream.Position = 0x72800 - 10;
            Assert.Equal(10, CsoStream.Read(Data, 0, 10));

            CsoStream.Position = 0x72800 - 10;
            Assert.Equal(10, CsoStream.Read(Data, 0, 100));

            Assert.Equal(
                IsoBytes,
                CsoStream.ReadAll(true)
            );

            CsoStream.Position = 0x10 * 2048 - 100;
            Assert.Equal(
                IsoBytes.Slice(0x10 * 2048 - 100, 300),
                CsoStream.ReadBytes(300)
            );
        }
    }
}
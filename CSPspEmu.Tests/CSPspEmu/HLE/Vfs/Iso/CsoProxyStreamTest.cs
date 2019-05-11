using System.IO;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Vfs.Iso;
using Xunit;

namespace Tests.CSPspEmu.Hle.Vfs.Iso
{
    public class CsoProxyStreamTest
    {
        [Fact(Skip = "file not found")]
        public void ReadTest()
        {
            var cso = new Cso(File.OpenRead("../../../TestInput/cube.cso"));
            var isoBytes = File.ReadAllBytes("../../../TestInput/cube.iso");
            var csoStream = new CompressedIsoProxyStream(cso);
            Assert.Equal(0x72800, csoStream.Length);
            var data = new byte[2048];
            var data2 = new byte[3000];
            Assert.Equal(data.Length, csoStream.Read(data, 0, data.Length));
            Assert.Equal(data.Length, csoStream.Read(data, 0, data.Length));
            Assert.Equal(3000, csoStream.Read(data2, 0, 3000));

            csoStream.Position = 0x72800 - data.Length;
            Assert.Equal(data.Length, csoStream.Read(data, 0, data.Length));

            csoStream.Position = 0x72800 - 10;
            Assert.Equal(10, csoStream.Read(data, 0, 10));

            csoStream.Position = 0x72800 - 10;
            Assert.Equal(10, csoStream.Read(data, 0, 100));

            Assert.Equal(
                isoBytes,
                csoStream.ReadAll()
            );

            csoStream.Position = 0x10 * 2048 - 100;
            Assert.Equal(
                isoBytes.Slice(0x10 * 2048 - 100, 300),
                csoStream.ReadBytes(300)
            );
        }
    }
}
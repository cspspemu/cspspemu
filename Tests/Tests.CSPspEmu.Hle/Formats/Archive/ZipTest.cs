using System;

using CSPspEmu.Hle.Formats.Archive;
using CSPspEmu.Resources;
using CSharpUtils.Extensions;
using Xunit;

namespace CSPspEmu.Core.Tests.Hle.Formats.Archive
{
    
    public class ZipTest
    {
        [Fact(Skip = "file not found")]
        public void TestUncompressedZip()
        {
            var Zip = new ZipArchive();
            Zip.Load("../../../TestInput/UncompressedZip.zip");
            foreach (var Entry in Zip)
            {
                Console.Error.WriteLine(Entry);
            }
        }

        [Fact(Skip = "file not found")]
        public void TestCompressedZip()
        {
            var Zip = new ZipArchive();
            Zip.Load("../../../TestInput/UncompressedZip.zip");
            var ExpectedString =
                "ffmpeg -i test.pmf -vcodec copy -an test.h264\nffmpeg -i test.pmf -acodec copy -vn test.ac3p";
            var ResultString = Zip["demux.bat"].OpenUncompressedStream().ReadAllContentsAsString(fromStart: false);
            Assert.Equal(ExpectedString, ResultString);
        }

        [Fact]
        public void TestZip2()
        {
            var Zip = new ZipArchive();
            Zip.Load(ResourceArchive.GetFlash0ZipFileStream());
            foreach (var Entry in Zip)
            {
                Console.Error.WriteLine(Entry);
            }
        }
    }
}
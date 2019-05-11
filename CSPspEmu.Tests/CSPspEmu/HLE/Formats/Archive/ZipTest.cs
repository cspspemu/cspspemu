using System;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Formats.Archive;
using CSPspEmu.Resources;
using Xunit;

namespace Tests.CSPspEmu.Hle.Formats.Archive
{
    
    public class ZipTest
    {
        [Fact(Skip = "file not found")]
        public void TestUncompressedZip()
        {
            var zip = new ZipArchive();
            zip.Load("../../../TestInput/UncompressedZip.zip");
            foreach (var entry in zip)
            {
                Console.Error.WriteLine(entry);
            }
        }

        [Fact(Skip = "file not found")]
        public void TestCompressedZip()
        {
            var zip = new ZipArchive();
            zip.Load("../../../TestInput/UncompressedZip.zip");
            var expectedString =
                "ffmpeg -i test.pmf -vcodec copy -an test.h264\nffmpeg -i test.pmf -acodec copy -vn test.ac3p";
            var resultString = zip["demux.bat"].OpenUncompressedStream().ReadAllContentsAsString(fromStart: false);
            Assert.Equal(expectedString, resultString);
        }

        [Fact]
        public void TestZip2()
        {
            var zip = new ZipArchive();
            zip.Load(ResourceArchive.GetFlash0ZipFileStream());
            foreach (var entry in zip)
            {
                Console.Error.WriteLine(entry);
            }
        }
    }
}
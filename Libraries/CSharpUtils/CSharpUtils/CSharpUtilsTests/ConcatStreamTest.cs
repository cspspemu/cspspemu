using System.IO;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;
using Xunit;


namespace CSharpUtilsTests
{
    public class ConcatStreamTest
    {
        Stream Stream1;
        Stream Stream2;
        Stream Stream;

        public ConcatStreamTest()
        {
            Stream1 = new MemoryStream(new byte[] {1, 2, 3});
            Stream2 = new MemoryStream(new byte[] {4, 5, 6, 7, 8});
            Stream = new ConcatStream(Stream1, Stream2);
        }

        [Fact]
        public void LengthTest()
        {
            Assert.Equal(8, Stream.Length);
        }

        [Fact]
        public void ReadAllTest()
        {
            Assert.Equal("0102030405060708", Stream.ReadAll().ToHexString());
        }

        [Fact]
        public void ReadExactChunksTest()
        {
            Assert.Equal("010203", Stream.ReadBytes(3).ToHexString());
            Assert.Equal(false, Stream.Eof());
            Assert.Equal("0405060708", Stream.ReadBytes(5).ToHexString());
            Assert.Equal(true, Stream.Eof());
        }

        [Fact]
        public void SeekTest()
        {
            Assert.Equal(6, Stream.Seek(-2, SeekOrigin.End));
            Assert.Equal("0708", Stream.ReadBytes(2).ToHexString());
            Assert.Equal(true, Stream.Eof());
            Assert.Equal(2, Stream.Seek(2, SeekOrigin.Begin));
            Assert.Equal("030405", Stream.ReadBytes(3).ToHexString());
        }
    }
}
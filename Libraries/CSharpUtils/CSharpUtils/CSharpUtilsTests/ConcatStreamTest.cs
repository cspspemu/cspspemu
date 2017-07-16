using System.IO;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;
using NUnit.Framework;

namespace CSharpUtilsTests
{
    [TestFixture]
    public class ConcatStreamTest
    {
        Stream Stream1;
        Stream Stream2;
        Stream Stream;

        [SetUp]
        public void MyTestInitialize()
        {
            Stream1 = new MemoryStream(new byte[] {1, 2, 3});
            Stream2 = new MemoryStream(new byte[] {4, 5, 6, 7, 8});
            Stream = new ConcatStream(Stream1, Stream2);
        }

        [Test]
        public void LengthTest()
        {
            Assert.AreEqual(8, Stream.Length);
        }

        [Test]
        public void ReadAllTest()
        {
            Assert.AreEqual("0102030405060708", Stream.ReadAll().ToHexString());
        }

        [Test]
        public void ReadExactChunksTest()
        {
            Assert.AreEqual("010203", Stream.ReadBytes(3).ToHexString());
            Assert.AreEqual(false, Stream.Eof());
            Assert.AreEqual("0405060708", Stream.ReadBytes(5).ToHexString());
            Assert.AreEqual(true, Stream.Eof());
        }

        [Test]
        public void SeekTest()
        {
            Assert.AreEqual(6, Stream.Seek(-2, SeekOrigin.End));
            Assert.AreEqual("0708", Stream.ReadBytes(2).ToHexString());
            Assert.AreEqual(true, Stream.Eof());
            Assert.AreEqual(2, Stream.Seek(2, SeekOrigin.Begin));
            Assert.AreEqual("030405", Stream.ReadBytes(3).ToHexString());
        }
    }
}
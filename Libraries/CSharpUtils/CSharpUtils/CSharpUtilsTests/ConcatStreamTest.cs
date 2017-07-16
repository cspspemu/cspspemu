using System.IO;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class ConcatStreamTest
    {
        Stream Stream1;
        Stream Stream2;
        Stream Stream;

        [TestInitialize]
        public void MyTestInitialize()
        {
            this.Stream1 = new MemoryStream(new byte[] {1, 2, 3});
            this.Stream2 = new MemoryStream(new byte[] {4, 5, 6, 7, 8});
            this.Stream = new ConcatStream(Stream1, Stream2);
        }

        [TestMethod]
        public void LengthTest()
        {
            Assert.AreEqual(8, this.Stream.Length);
        }

        [TestMethod]
        public void ReadAllTest()
        {
            Assert.AreEqual("0102030405060708", this.Stream.ReadAll().ToHexString());
        }

        [TestMethod]
        public void ReadExactChunksTest()
        {
            Assert.AreEqual("010203", this.Stream.ReadBytes(3).ToHexString());
            Assert.AreEqual(false, this.Stream.Eof());
            Assert.AreEqual("0405060708", this.Stream.ReadBytes(5).ToHexString());
            Assert.AreEqual(true, this.Stream.Eof());
        }

        [TestMethod]
        public void SeekTest()
        {
            Assert.AreEqual(6, this.Stream.Seek(-2, SeekOrigin.End));
            Assert.AreEqual("0708", this.Stream.ReadBytes(2).ToHexString());
            Assert.AreEqual(true, this.Stream.Eof());
            Assert.AreEqual(2, this.Stream.Seek(2, SeekOrigin.Begin));
            Assert.AreEqual("030405", this.Stream.ReadBytes(3).ToHexString());
        }
    }
}
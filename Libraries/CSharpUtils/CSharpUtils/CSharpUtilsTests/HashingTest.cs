using System;
using CSharpUtils;
using System.IO;
using System.Linq;
using CSharpUtils.Extensions;
using NUnit.Framework;

namespace CSharpUtilsTests
{
    [TestFixture]
    public class HashingTest
    {
        [Test]
        public void TestSmallMd5()
        {
            var Stream = new MemoryStream(new[] {'H', 'e', 'l', 'l', 'o'}.Select(Item => (byte) Item).ToArray());
            Assert.AreEqual("8b1a9953c4611296a827abf8c47804d7", Hashing.GetMd5Hash(Stream));
        }

        [Test]
        public void TestBigMd5()
        {
            var Stream = new MemoryStream();
            Stream.WriteByteRepeated((byte) ' ', 5 * 1024 * 1024);
            Stream.Position = 0;
            Assert.AreEqual("faa372d5265b47ad82a1aaeea5443e34", Hashing.GetMd5Hash(Stream));
        }
    }
}
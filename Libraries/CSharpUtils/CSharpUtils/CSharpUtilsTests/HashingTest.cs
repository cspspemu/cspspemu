using CSharpUtils;
using System.IO;
using System.Linq;
using CSharpUtils.Extensions;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class HashingTest
    {
        [Fact]
        public void TestSmallMd5()
        {
            var Stream = new MemoryStream(new[] {'H', 'e', 'l', 'l', 'o'}.Select(Item => (byte) Item).ToArray());
            Assert.Equal("8b1a9953c4611296a827abf8c47804d7", Hashing.GetMd5Hash(Stream));
        }

        [Fact]
        public void TestBigMd5()
        {
            var Stream = new MemoryStream();
            Stream.WriteByteRepeated((byte) ' ', 5 * 1024 * 1024);
            Stream.Position = 0;
            Assert.Equal("faa372d5265b47ad82a1aaeea5443e34", Hashing.GetMd5Hash(Stream));
        }
    }
}
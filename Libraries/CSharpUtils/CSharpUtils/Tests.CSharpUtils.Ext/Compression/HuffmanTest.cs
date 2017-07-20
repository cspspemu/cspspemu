using System.IO;
using System.Text;
using CSharpUtils.Ext.Compression;

using CSharpUtils.Extensions;
using Xunit;

namespace CSharpUtilsTest
{
    public class HuffmanTest
    {
        [Fact]
        public void HuffmanCompressUncompressTest()
        {
            var ThisEncoding = Encoding.UTF8;

            var InputString = "Hola. Esto es una prueba para ver si funciona la compresión Huffman.";
            var InputBytes = ThisEncoding.GetBytes(InputString);
            var InputStream = new MemoryStream(InputBytes);

            var InputUsageTable = Huffman.CalculateUsageTable(InputBytes);
            var InputEncodingTable = Huffman.BuildTable(InputUsageTable);

            var CompressedStream = Huffman.Compress(InputStream, InputEncodingTable);

            var DecompressedStream = Huffman.Uncompress(CompressedStream, (uint) InputBytes.Length, InputEncodingTable);
            var DecompressedBytes = DecompressedStream.ReadAll();
            var DecompressedString = ThisEncoding.GetString(DecompressedBytes);

            Assert.Equal(InputString, DecompressedString);
        }
    }
}
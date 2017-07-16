using System.IO;
using System.Text;
using CSharpUtils.Ext.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils.Extensions;

namespace CSharpUtilsTest
{
    [TestClass]
    public class HuffmanTest
    {
        [TestMethod]
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

            Assert.AreEqual(InputString, DecompressedString);
        }
    }
}
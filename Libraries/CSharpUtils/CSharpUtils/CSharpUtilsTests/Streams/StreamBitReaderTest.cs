using CSharpUtils.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CSharpUtilsTests
{
	[TestClass]
	public class StreamBitReaderTest
	{
		[TestMethod]
		public void ReadBitsTest()
		{
			var Stream = new MemoryStream(new byte[] { 0x00, 0x00, 0x01, 0xBA, 0x44, 0x00, 0x05, 0x3D, 0x1D, 0x11, 0x01, 0x86, 0xA3, 0xF8, 0x00, 0x00 });
			var StreamBitReader = new StreamBitReader(Stream);
			//Assert.AreEqual((uint)0, (uint)StreamBitReader.ReadBits(8));
			Assert.AreEqual((uint)0x000001BA, (uint)StreamBitReader.ReadBits(32));
			Assert.AreEqual((uint)0, (uint)StreamBitReader.ReadBits(2));
		}
	}
}

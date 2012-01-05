using CSPspEmu.Hle.Formats.video;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class MpegPsDemuxerTest
	{
		[TestMethod]
		public void GetNextPacketAndSyncTest()
		{
			var MpegPsDemuxer = new MpegPsDemuxer(File.OpenRead("../../../TestInput/test.pmf").SliceWithLength(0x800));
			Assert.AreEqual((uint)0x1BA, (uint)MpegPsDemuxer.GetNextPacketAndSync());
			Assert.AreEqual((uint)0x1BB, (uint)MpegPsDemuxer.GetNextPacketAndSync());
			Assert.AreEqual((uint)0x1BF, (uint)MpegPsDemuxer.GetNextPacketAndSync());
			Assert.AreEqual((uint)0x1E0, (uint)MpegPsDemuxer.GetNextPacketAndSync());
			Assert.AreEqual((uint)0x109, (uint)MpegPsDemuxer.GetNextPacketAndSync());
			Assert.AreEqual((uint)0x127, (uint)MpegPsDemuxer.GetNextPacketAndSync());
			Assert.AreEqual((uint)0x128, (uint)MpegPsDemuxer.GetNextPacketAndSync());
			Assert.AreEqual((uint)0x106, (uint)MpegPsDemuxer.GetNextPacketAndSync());
		}

		[TestMethod]
		public void ReadPacketizedElementaryStreamHeaderTest()
		{
			MpegPsDemuxer.Packet Packet;
			var MpegPsDemuxer = new MpegPsDemuxer(File.OpenRead("../../../TestInput/test.pmf").SliceWithLength(0x800));
			Packet = MpegPsDemuxer.ReadPacketizedElementaryStreamHeader();
			Console.WriteLine("0x{0:X}", Packet.Type);
			Packet = MpegPsDemuxer.ReadPacketizedElementaryStreamHeader();
			Console.WriteLine("0x{0:X}", Packet.Type);
			Packet = MpegPsDemuxer.ReadPacketizedElementaryStreamHeader();
			Console.WriteLine("0x{0:X}", Packet.Type);
			Packet = MpegPsDemuxer.ReadPacketizedElementaryStreamHeader();
			Console.WriteLine("0x{0:X}", Packet.Type);
			Packet = MpegPsDemuxer.ReadPacketizedElementaryStreamHeader();
			Console.WriteLine("0x{0:X}", Packet.Type);
		}
	}
}

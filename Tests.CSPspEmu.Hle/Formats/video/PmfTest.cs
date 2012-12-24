using cscodec;
using cscodec.h264.player;
using CSharpUtils;
using CSPspEmu.Hle.Formats.video;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;

namespace CSPspEmu.Core.Tests
{
	[TestFixture]
	public class PmfTest
	{
		[Test]
		public void LoadTest()
		{
			var Pmf = new Pmf();
			//Pmf.Load(File.OpenRead("../../../TestInput/test.pmf"));
			Pmf.Load(File.OpenRead("../../../TestInput/sample.pmf"));
			var Sample5Reference = new Bitmap(Image.FromFile("../../../TestInput/sample_5.png"));
			//Console.WriteLine(Pmf.InfoHeader.ToStringDefault());
			Assert.AreEqual(144, Pmf.InfoHeader.Width);
			Assert.AreEqual(80, Pmf.InfoHeader.Height);
			var MpegPs = Pmf.GetMpegPs();
			int PmfWidth = Pmf.InfoHeader.Width;
			int PmfHeight = Pmf.InfoHeader.Height;

			var OutStream = new MemoryStream();

			try
			{
				while (MpegPs.HasMorePackets)
				{
					var Packet = MpegPs.ReadPacketizedElementaryStreamHeader();
					if (Packet.Type == MpegPsDemuxer.ChunkType.ST_Video1)
					{
						var Info = MpegPs.ParsePacketizedStream(Packet.Stream);
						Info.Stream.CopyTo(OutStream);
						//Console.WriteLine(Info.dts);
						//Info
					}
				}
			}
			catch (EndOfStreamException)
			{
			}

			//return;

			var FrameDecoder = new FrameDecoder(OutStream.Slice());
			int n = 0;
			while (FrameDecoder.HasMorePackets)
			{
				var Frame = FrameDecoder.DecodeFrame();
				//Console.WriteLine(Frame.imageWidth);
				//Console.WriteLine(Frame.imageWidthWOEdge);
				//Console.WriteLine(Frame.imageHeight);
				//Console.WriteLine(Frame.imageHeightWOEdge);
				if (n == 5)
				{
					var Sample5Output = FrameUtils.imageFromFrameWithoutEdges(Frame, PmfWidth, PmfHeight);
					//Image.Save(@"c:\temp\frame_" + (n) + ".png");
					var CompareResult = BitmapUtils.CompareBitmaps(Sample5Reference, Sample5Output);
					Assert.IsTrue(CompareResult.Equal);
				}
				n++;
			}
			
		}
	}
}

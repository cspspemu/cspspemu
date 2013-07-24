using cscodec;
using cscodec.h264.player;
using CSharpUtils;
using CSPspEmu.Hle.Formats.audio.At3;
using CSPspEmu.Hle.Formats.video;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.IO;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	unsafe public class PmfTest
	{
		[TestMethod]
		public void LoadTest()
		{
			var Pmf = new Pmf();
			//Pmf.Load(File.OpenRead("../../../TestInput/test.pmf"));
			//Pmf.Load(File.OpenRead("../../../TestInput/sample.pmf"));
			Pmf.Load(File.OpenRead("c:/isos/psp/opening.pmf"));
			//Console.WriteLine(Pmf.InfoHeader.ToStringDefault());
			
			//Assert.AreEqual(0x800, Pmf.Header.StreamOffset);
			//Assert.AreEqual(137216, Pmf.Header.StreamSize);
			//Assert.AreEqual(144, Pmf.InfoHeader.Width);
			//Assert.AreEqual(80, Pmf.InfoHeader.Height);

			var MpegPs = Pmf.GetMpegPs();
			int PmfWidth = Pmf.InfoHeader.Width;
			int PmfHeight = Pmf.InfoHeader.Height;

			var VideoOutStream = new MemoryStream();
			var AudioOutStream = new MemoryStream();

			try
			{
				while (MpegPs.HasMorePackets)
				{
					var Packet = MpegPs.ReadPacketizedElementaryStreamHeader();
					var Info = MpegPs.ParsePacketizedStream(Packet.Stream);

					if (Packet.Type == MpegPsDemuxer.ChunkType.ST_Video1)
					{
						Info.Stream.CopyToFast(VideoOutStream);
						//Console.WriteLine(Info.dts);
						//Info
					}
					if (Packet.Type == MpegPsDemuxer.ChunkType.ST_Private1)
					{
						Info.Stream.CopyToFast(AudioOutStream);
					}
					//Console.WriteLine(Packet.Type);
				}
			}
			catch (EndOfStreamException)
			{
			}

			//return;
			//
			//VideoOutStream.Slice().CopyToFile(@"c:/isos/psp/opening.h264");
			//AudioOutStream.Slice().CopyToFile(@"c:/isos/psp/opening.at3");
			//
			//var AudioDecoder = new At3pFrameDecoder(AudioOutStream.Slice());
			//if (AudioDecoder.HasMorePackets)
			//{
			//	var AudioFrame = AudioDecoder.DecodeFrame();
			//	File.WriteAllBytes(@"c:\isos\psp\samples.raw", AudioFrame.ToByteArray());
			//	//Console.WriteLine(AudioFrame);
			//}
			//
			//return;

			//var Data = AudioOutStream2.ReadBytes((int)2048);
			//int BlockSize = Data.Length;
			//short[] buf;
			//int channels;
			//fixed (byte* DataPtr = Data)
			//{
			//	int Decoded = MaiAT3PlusFrameDecoder.decodeFrame(DataPtr, BlockSize, out channels, out buf);
			//	Console.WriteLine("[1] : {0} : {1} : {2}", channels, BlockSize, Decoded);
			//}


			//return;

			var Sample5Reference = new Bitmap(Image.FromFile("../../../TestInput/sample_5.png"));

			var FrameDecoder = new H264FrameDecoder(VideoOutStream.Slice());
			int n = 0;
			while (FrameDecoder.HasMorePackets)
			{
				var Frame = FrameDecoder.DecodeFrame();
				//Console.WriteLine(Frame.imageWidth);
				//Console.WriteLine(Frame.imageWidthWOEdge);
				//Console.WriteLine(Frame.imageHeight);
				//Console.WriteLine(Frame.imageHeightWOEdge);
				if (n == 70)
				{
					var Sample5Output = FrameUtils.imageFromFrameWithoutEdges(Frame, PmfWidth, PmfHeight);
					Sample5Output.Save(@"c:\isos\psp\frame_" + (n) + ".png");
					var CompareResult = BitmapUtils.CompareBitmaps(Sample5Reference, Sample5Output);
					//Assert.IsTrue(CompareResult.Equal);
					break;
				}
				n++;
			}
			
		}
	}
}

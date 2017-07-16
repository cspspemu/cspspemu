using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cscodec.h264.player;
using System.IO;
using cscodec;
using System.Net;

namespace Tests.cscodec.h264
{
	[TestClass]
	public class Test_h264_conformance
	{
		const string BaseUrl = "http://fate-suite.libav.org/fate-suite/";

		[TestMethod]
		public void aud_mw_e()
		{
			this.TestItem("h264-conformance-aud_mw_e", "h264-conformance/AUD_MW_E.264");
		}

		[TestMethod]
		public void ba1_sony_d()
		{
			this.TestItem("h264-conformance-ba1_sony_d", "h264-conformance/BA1_Sony_D.jsv");
		}

		private void TestItem(string fixtureName, string videoName)
		{
			var RemotePath = BaseUrl + videoName;
			var LocalPath = Directory.GetCurrentDirectory() + "/../../Cache/" + videoName;
			try { Directory.CreateDirectory(Path.GetDirectoryName(LocalPath)); } catch { }
			if (!File.Exists(LocalPath)) new WebClient().DownloadFile(RemotePath, LocalPath);

			var FrameDecoder = new FrameDecoder(File.OpenRead(LocalPath));
			var Index = 0;
			while (FrameDecoder.HasMorePackets)
			{
				//var Packet = FrameDecoder._ReadPacket();
				//Console.WriteLine("{0}: {1}", Index, FrameCrc.GetFrameLine(Packet));
				var Frame = FrameDecoder.DecodeFrame();
				var Image = FrameUtils.imageFromFrame(Frame);
				Console.WriteLine("{0}: {1}, {2}, {3}", Index, Frame.pkt_dts, Frame.pkt_pts, Frame.imageWidthWOEdge * Frame.imageHeightWOEdge);
				Index++;
			}
		}
	}
}

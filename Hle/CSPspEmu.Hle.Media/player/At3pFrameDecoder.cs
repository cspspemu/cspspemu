using CSPspEmu.Hle.Formats.audio.At3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cscodec.h264.player
{
	unsafe public sealed class At3pFrameDecoder : FrameDecoder<short[]>
	{
		private MaiAT3PlusFrameDecoder MaiAT3PlusFrameDecoder;

		public At3pFrameDecoder(Stream stream)
			: base(stream)
		{
		}

		protected override void Close()
		{
		}

		protected override void InitProtected()
		{
			this.MaiAT3PlusFrameDecoder = new MaiAT3PlusFrameDecoder();
		}

		protected override short[] DecodeFrameFromPacket(av.AVPacket avpkt, out int len)
		{
			Console.WriteLine("{0}", avpkt.data_offset);
			File.WriteAllBytes(@"c:\isos\psp\samples.raw2", avpkt.data_base);
			fixed (byte* data = &avpkt.data_base[avpkt.data_offset])
			{
				short[] samples;
				int channels = 0;
				int ret = this.MaiAT3PlusFrameDecoder.decodeFrame(data, avpkt.size, out channels, out samples);
				len = ret;
				Console.WriteLine("{0}, {1}, {2}", channels, ret, samples.Length);
				return samples;
			}
		}
	}
}

using CSharpUtils.Endian;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Hle.Formats.audio.At3.Sample
{
	unsafe public class MiniPlayer
	{
		static public int Play(Stream Stream, Stream OutStream)
		{
			var strt = Stream.ReadString(3);
			Stream.Position = 0;

			ushort bztmp = 0;
			if (strt == "ea3")
			{
				//we get ea3 header
				Console.WriteLine("ea3 header\n");
				Stream.Position = 0x6;
				var tmp = Stream.ReadBytes(4);
				int skipbytes = 0;
				for (int a0 = 0; a0 < 4; a0++)
				{
					skipbytes <<= 7;
					skipbytes += tmp[a0] & 0x7F;
				}
				Stream.Skip(skipbytes);
			}
			
			if (strt == "RIF") //RIFF
			{
				//we get RIFF header
				Console.WriteLine("RIFF header\n");
				Stream.Position = 0x10;
				var fmt_size = Stream.ReadStruct<int>();
				var fmt = Stream.ReadStruct<ushort>();
				if (fmt != 0xFFFE)
				{
					Console.WriteLine("RIFF File fmt error\n");
					return -1;
				}
				Stream.Skip(0x28);
				bztmp = Stream.ReadStruct<ushort_be>();
				Stream.Skip(fmt_size - 0x2c);

				//search the data chunk
				for (int a0 = 0; a0 < 0x100; a0++)
				{
					if (Stream.ReadString(4) == "atad") break;
				}
				int tmpr = Stream.ReadStruct<int>();
			}
			else
			{
				//EA3 block that contains at3+ stream
				Console.WriteLine("EA3 header");
				Stream.Skip(0x22);

				Console.WriteLine("{0:X}", Stream.Position);
				bztmp = (ushort)Stream.ReadStruct<ushort_be>();
				Stream.Skip(0x3c);
			}
			int blocksz = bztmp & 0x3FF;
	
			var buf0 = new byte[0x3000];
			fixed (byte* buf0_ptr = buf0)
			{
				//calculate the frame block size here
				int block_size = blocksz * 8 + 8;

				Console.WriteLine("frame_block_size 0x{0:X}\n", block_size);

				//Console.ReadKey();

				//so we make a new at3+ frame decoder
				MaiAT3PlusFrameDecoder d2 = new MaiAT3PlusFrameDecoder();

				Stream.Read(buf0, 0, block_size);
				int chns = 0;
				short[] p_buf;
				int rs;
				//decode the first frame and get channel num
				//for (int n = 0; n < block_size; n++) Console.Write(buf0[n]);
				if ((rs = d2.decodeFrame(buf0_ptr, block_size, out chns, out p_buf)) != 0) Console.WriteLine("decode error {0}", rs);
				Console.WriteLine("channels: {0}\n", chns);
				if (chns > 2) Console.WriteLine("warning: waveout doesn't support {0} chns\n", chns);

				//just waveout
				//MaiWaveOutI *mwo0 = new MaiWaveOutI(chns, 44100, 16);

				//mwo0.play();
				while (!Stream.Eof())
				{
					Stream.Read(buf0, 0, block_size);

					//decode frame and get sample data
					if ((rs = d2.decodeFrame(buf0_ptr, block_size, out chns, out p_buf)) != 0) Console.WriteLine("decode error {0}", rs);
					//play it

					OutStream.WriteStructVector(p_buf, 0x800 * chns);

					//mwo0.enqueue((Mai_I8*)p_buf, 0x800 * chns * 2);
				}

				//while (mwo0.getRemainBufSize()) Mai_Sleep(1);
				return 0;
			}
		}
	}
}

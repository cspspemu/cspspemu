using CSPspEmu.Hle.Formats.audio.At3.SUB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Hle.Formats.audio.At3
{
	public sealed unsafe class MaiAT3PlusFrameDecoder
	{
		private MaiAT3PlusCoreDecoder[] cores = new MaiAT3PlusCoreDecoder[0x10];
		private short[] sample_buf = new short[0x8000];
		private short[] sample_buf_tmp = new short[0x8000];
		//int num_cores = 0;

		public MaiAT3PlusFrameDecoder()
		{
		}

		public int decodeFrame(byte* p_frame_data, int data_len, out int p_chns, out short[] pp_sample_buf)
		{
			int rs = 0;

			var mbr0 = new MaiBitReader(data_len + 0x10);
			mbr0.addData(p_frame_data, data_len);
			var Pad = stackalloc byte[0x10];
			mbr0.addData(Pad, 0x10);

			if (mbr0.getWithI32Buffer(1) != 0)
			{
				rs = -1;
			}


			int counter_substream = 0;
			int counter_chn = 0;
			while (rs == 0)
			{
				int substream_type = mbr0.getWithI32Buffer(2);
				uint joint_flag = 0;
				uint chns = 0;

				if (substream_type == 0)
				{
					joint_flag = 0;
					chns = 1;
				}
				else if (substream_type == 1)
				{
					joint_flag = 1;
					chns = 2;
				}
				else if (substream_type == 3)
				{
					break;
				}
				else
				{
					rs = -1;
				}
		
				if (cores[counter_substream] == null)
					cores[counter_substream] = new MaiAT3PlusCoreDecoder();

				if (0 != (rs = cores[counter_substream].parseStream(mbr0, chns, joint_flag)))
					break;

				if (0 != (rs = cores[counter_substream].decodeStream(chns)))
					break;

				for (int a0 = 0; a0 < chns; a0++)
					cores[counter_substream].getAudioSamplesI16((uint)a0, new ManagedPointer<short>(sample_buf_tmp, 0x800 * (counter_chn++)));

				counter_substream++;
			}

			for (int a0 = 0; a0 < 0x800; a0++)
			{
				for (int a1 = 0; a1 < counter_chn; a1++)
				{
					sample_buf[a0 * counter_chn + a1] = sample_buf_tmp[a1 * 0x800 + a0];
				}
			}
			mbr0.Dispose();

			p_chns = counter_chn;
			pp_sample_buf = sample_buf;

			return rs;
		}
	}
}

using cscodec.av;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cscodec
{
	public class FrameCrc
	{
		public static string GetFrameLine(AVPacket pkt)
		{
			//pkt.data
			uint crc = BitConverter.ToUInt32(new CRC32().ComputeHash(pkt.data_base.Select(Item => (byte)Item).ToArray(), pkt.data_offset, pkt.size), 0);

			//snprintf(buf, sizeof(buf), "%d, %10"PRId64", %10"PRId64", %8d, %8d, 0x%08x",
			//		 pkt->stream_index, pkt->dts, pkt->pts, pkt->duration, pkt->size, crc);
			//if (pkt->flags != AV_PKT_FLAG_KEY)
			//	av_strlcatf(buf, sizeof(buf), ", F=0x%0X", pkt->flags);
			//if (pkt->side_data_elems)
			//	av_strlcatf(buf, sizeof(buf), ", S=%d", pkt->side_data_elems);
			//av_strlcatf(buf, sizeof(buf), "\n");
			//avio_write(s->pb, buf, strlen(buf));
			//avio_flush(s->pb);
			return string.Format(
				"{0}, {1}, {2}, {3}, {4}, 0x{5:X8}",
				pkt.stream_index, pkt.dts, pkt.pts, pkt.duration, pkt.size, crc
			);
		}
	}
}

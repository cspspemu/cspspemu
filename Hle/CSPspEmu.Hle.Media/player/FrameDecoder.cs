using cscodec.av;
using cscodec.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cscodec.h264.player
{
	abstract public class FrameDecoder<TOut> : IDisposable
	{
		public const int FF_INPUT_BUFFER_PADDING_SIZE = 80;
		const int INBUF_SIZE = 65535;
		int frame, len;
		sbyte[] inbuf = new sbyte[INBUF_SIZE + FF_INPUT_BUFFER_PADDING_SIZE];
		byte[] inbuf_int = new byte[INBUF_SIZE + FF_INPUT_BUFFER_PADDING_SIZE];
		//char buf[1024];
		sbyte[] buf = new sbyte[1024];
		AVPacket avpkt = new AVPacket();
		Stream fin;
		bool hasMoreNAL;
		int dataPointer;
		int[] cacheRead = new int[3];

		public FrameDecoder(Stream Stream)
		{
			this.fin = Stream;
			Init();
		}

		abstract protected void InitProtected();
		abstract protected TOut DecodeFrameFromPacket(AVPacket avpkt, out int len);
		abstract protected void Close();

		private void Init()
		{
			avpkt.av_init_packet();

			// Find the mpeg1 video decoder
			InitProtected();

			// The codec gives us the frame size, in samples

			frame = 0;

			// avpkt must contain exactly 1 NAL Unit in order for decoder to decode correctly.
			// thus we must read until we get next NAL header before sending it to decoder.
			// Find 1st NAL
			cacheRead[0] = fin.ReadByte();
			cacheRead[1] = fin.ReadByte();
			cacheRead[2] = fin.ReadByte();

			while (!(cacheRead[0] == 0x00 && cacheRead[1] == 0x00 && cacheRead[2] == 0x01))
			{
				cacheRead[0] = cacheRead[1];
				cacheRead[1] = cacheRead[2];
				cacheRead[2] = fin.ReadByte();
				if (cacheRead[2] == -1) throw(new EndOfStreamException());
			} // while

			// 4 first bytes always indicate NAL header
			inbuf_int[0] = inbuf_int[1] = inbuf_int[2] = 0x00;
			inbuf_int[3] = 0x01;

			hasMoreNAL = true;
		}

		public bool HasMorePackets
		{
			get
			{
				return hasMoreNAL;
			}
		}

		public AVPacket _ReadPacket()
		{
			if (hasMoreNAL)
			{
				dataPointer = 4;
				// Find next NAL
				if ((cacheRead[0] = fin.ReadByte()) == -1) hasMoreNAL = false;
				if ((cacheRead[1] = fin.ReadByte()) == -1) hasMoreNAL = false;
				if ((cacheRead[2] = fin.ReadByte()) == -1) hasMoreNAL = false;
				while (!(cacheRead[0] == 0x00 && cacheRead[1] == 0x00 && cacheRead[2] == 0x01) && hasMoreNAL)
				{
					inbuf_int[dataPointer++] = (byte)cacheRead[0];
					cacheRead[0] = cacheRead[1];
					cacheRead[1] = cacheRead[2];
					cacheRead[2] = fin.ReadByte();
					if (fin.Position >= fin.Length) hasMoreNAL = false;
				} // while

				avpkt.size = dataPointer;
				avpkt.data_base = inbuf_int;
				avpkt.data_offset = 0;
				return avpkt;
			}
			else
			{
				throw (new EndOfStreamException());
			}
		}

		public TOut DecodeFrame()
		{
			while (hasMoreNAL)
			{
				//Console.WriteLine(avpkt.size);

				_ReadPacket();

				while (avpkt.size > 0)
				{
					var Result = DecodeFrameFromPacket(avpkt, out len);
					if (len >= 0)
					{
						avpkt.size -= len;
						avpkt.data_offset += len;
						frame++;
						if (Result != null) return Result;
					}
					else
					{
						break;
					}
				}
			} // while
			throw (new EndOfStreamException());
		}

		void IDisposable.Dispose()
		{
			Close();
		}
	}
}

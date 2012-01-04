using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats.video
{
	/// <summary>
	/// MPEG Program Stream
	/// 
	/// Glosary:
	/// - AU : Access Unit
	/// - TS : Transport Stream
	/// - PS : Program Stream
	/// - PSI: Program Specific Information
	/// - PAT: Program Association Table
	/// - PMT: Program Map Tables
	/// - PES: Packetized Elementary Stream
	/// - GOP: Group Of Pictures
	/// - PTS: Presentation TimeStamp
	/// - DTS: Decode TimeStamp
	/// - PID: Packet IDentifier
	/// - PCR: Program Clock Reference
	/// </summary>
	/// <see cref="http://en.wikipedia.org/wiki/MPEG_program_stream"/>
	/// <see cref="http://en.wikipedia.org/wiki/MPEG_transport_stream"/>
	public class MpegPsDemuxer : IDemuxer
	{
		public string Name { get { return "mpeg"; } }
		public string LongName { get { return "MPEG-PS format"; } }

		public enum ChunkType : uint
		{
			Start            = 0x000001BA,
			SystemHeader     = 0x000001BB,
			ST_PSMapTable    = 0x000001BC,
			ST_Private1      = 0x000001BD,
			ST_Padding       = 0x000001BE,
			ST_Private2      = 0x000001BF,
			ST_Audio1        = 0x000001C0,
			ST_Audio2        = 0x000001DF,
			ST_Video1        = 0x000001E0,
			ST_Video2        = 0x000001EF,
			ST_ECM           = 0x000001F0,
			ST_EMM           = 0x000001F1,
			ST_DSMCC         = 0x000001F2,
			ST_ISO_13522     = 0x000001F3,
			ST_ITUT_A        = 0x000001F4,
			ST_ITUT_B        = 0x000001F5,
			ST_ITUT_C        = 0x000001F6,
			ST_ITUT_D        = 0x000001F7,
			ST_ITUT_E        = 0x000001F8,
			ST_PSDirectory   = 0x000001FF,
		}

		protected Stream Stream;

		public MpegPsDemuxer(Stream Stream)
		{
			this.Stream = Stream;
		}

		public class Packet
		{
			public uint Type;
			public Stream Stream;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://en.wikipedia.org/wiki/Packetized_elementary_stream"/>
		public Packet ReadPacketizedElementaryStreamHeader()
		{
			while (!Stream.Eof())
			{
				uint StartCode = GetNextPacketAndSync();
				switch (StartCode)
				{
					// PACK_START_CODE
					case 0x000001BA:
						continue;
					// SYSTEM_HEADER_START_CODE
					case 0x000001BB:
						continue;
					// PADDING_STREAM
					case 0x000001BE:
						Stream.Skip(Read16());
						continue;
					// PRIVATE_STREAM_2
					case 0x000001BF:
						Stream.Skip(Read16());
						continue;
					/*
					// PROGRAM_STREAM_MAP
					case 0x1bc:
						break;
						*/
				}

				if (
					// Audio Stream
					(StartCode >= 0x1C0 && StartCode <= 0x1CF) ||
					// Video Stream
					(StartCode >= 0x1E0 && StartCode <= 0x1EF) ||
					// Private Stream (Atrac3+)
					StartCode == 0x1bd ||
					// ???
					StartCode == 0x1fd
				)
				{
					// Found packet.
					//Console.WriteLine("Position: 0x{0:X}", Stream.Position);
					ushort PacketSize = Read16();
					var PacketStream = Stream.ReadStream(PacketSize);
					return new Packet()
					{
						Type = StartCode,
						Stream = PacketStream,
					};
				}

				//throw(new NotImplementedException(String.Format("Invalid stream 0x{0:X}", StartCode)));
				Console.Error.WriteLine("Invalid stream 0x{0:X}", StartCode);
			}
			throw(new Exception("End of Stream"));
		}

		public ushort Read16()
		{
			byte Hi = (byte)Stream.ReadByte();
			byte Lo = (byte)Stream.ReadByte();
			return (ushort)(((ushort)Hi << 8) | (ushort)Lo);
		}

		public uint GetNextPacketAndSync()
		{
			uint Value = 0xFFFFFFFF;
			int Byte;
			while ((Byte = Stream.ReadByte()) != -1)
			{
				Value <<= 8;
				Value |= (byte)Byte;
				if ((Value & 0xFFFFFF00) == 0x00000100)
				{
					return Value;
				}
			}
			return 0xFFFFFFFF;
		}

		// From libavformat/mpeg.h
		/*
		#define PACK_START_CODE             ((unsigned int)0x000001ba)
		#define SYSTEM_HEADER_START_CODE    ((unsigned int)0x000001bb)
		#define SEQUENCE_END_CODE           ((unsigned int)0x000001b7)
		#define PACKET_START_CODE_MASK      ((unsigned int)0xffffff00)
		#define PACKET_START_CODE_PREFIX    ((unsigned int)0x00000100)
		#define ISO_11172_END_CODE          ((unsigned int)0x000001b9)

		// mpeg2
		#define PROGRAM_STREAM_MAP 0x1bc
		#define PRIVATE_STREAM_1   0x1bd
		#define PADDING_STREAM     0x1be
		#define PRIVATE_STREAM_2   0x1bf

		#define AUDIO_ID 0xc0
		#define VIDEO_ID 0xe0
		#define AC3_ID   0x80
		#define DTS_ID   0x8a
		#define LPCM_ID  0xa0
		#define SUB_ID   0x20

		#define STREAM_TYPE_VIDEO_MPEG1     0x01
		#define STREAM_TYPE_VIDEO_MPEG2     0x02
		#define STREAM_TYPE_AUDIO_MPEG1     0x03
		#define STREAM_TYPE_AUDIO_MPEG2     0x04
		#define STREAM_TYPE_PRIVATE_SECTION 0x05
		#define STREAM_TYPE_PRIVATE_DATA    0x06
		#define STREAM_TYPE_AUDIO_AAC       0x0f
		#define STREAM_TYPE_VIDEO_MPEG4     0x10
		#define STREAM_TYPE_VIDEO_H264      0x1b

		#define STREAM_TYPE_AUDIO_AC3       0x81
		#define STREAM_TYPE_AUDIO_DTS       0x8a

		static const int lpcm_freq_tab[4] = { 48000, 96000, 44100, 32000 };

		static inline int64_t ff_parse_pes_pts(const uint8_t *buf) {
			return (int64_t)(*buf & 0x0e) << 29 |
					(AV_RB16(buf+1) >> 1) << 15 |
					 AV_RB16(buf+3) >> 1;
		}
		*/

		public float Probe(string FileName, Stream ProbeStream)
		{
			throw new NotImplementedException();
		}
	}
}

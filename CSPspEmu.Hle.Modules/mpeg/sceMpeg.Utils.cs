using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Endian;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.mpeg
{
	unsafe public partial class sceMpeg
	{
		public SceMpegData* GetSceMpegData(SceMpeg* SceMpeg)
		{
			return SceMpeg->GetSceMpegData(PspMemory);
		}

		public enum StreamId : int
		{
			/// <summary>
			/// MPEG_AVC_STREAM
			/// </summary>
			Avc = 0,

			/// <summary>
			/// MPEG_ATRAC_STREAM
			/// </summary>
			Atrac = 1,

			/// <summary>
			/// MPEG_PCM_STREAM
			/// </summary>
			Pcm = 2,

			/// <summary>
			/// MPEG_DATA_STREAM
			/// </summary>
			Data = 3,

			/// <summary>
			/// MPEG_AUDIO_STREAM
			/// </summary>
			Audio = 15,
		}

		public struct SceMpeg
		{
			public PspPointer SceMpegData;

			public SceMpegData* GetSceMpegData(PspMemory PspMemory)
			{
				return (SceMpegData *)PspMemory.PspPointerToPointerSafe(SceMpegData);
			}
		}

		public struct SceMpegData
		{
			/// <summary>
			/// 
			/// </summary>
			public fixed byte MagicBytes[12];

			/// <summary>
			/// 
			/// </summary>
			public int Unknown1;

			/// <summary>
			/// 
			/// </summary>
			public PspPointer RingBufferAddress;

			/// <summary>
			/// 
			/// </summary>
			public PspPointer RingBufferAddressDataUpper;

			/// <summary>
			/// 
			/// </summary>
			public SceMpegAvcMode SceMpegAvcMode;

			//public fixed byte Data[0x10000];
		}

		public struct SceMpegStream
		{
		}

		public struct SceMpegAu
		{
			/// <summary>
			/// presentation timestamp MSB
			/// </summary>
			public uint_be PresentationTimestampBe;

			/// <summary>
			/// presentation timestamp LSB
			/// </summary>
			public uint_le PresentationTimestampLe;

			/// <summary>
			/// decode timestamp MSB
			/// </summary>
			public uint_be DecodeTimestampBe;

			/// <summary>
			/// decode timestamp LSB
			/// </summary>
			public uint_le DecodeTimestampLe;

			/// <summary>
			/// Es buffer handle
			/// </summary>
			public int EsBuffer;

			/// <summary>
			/// Au size
			/// </summary>
			public int AuSize;
		}

		public struct SceMpegRingbuffer
		{
			/// <summary>
			/// 00 - Packets
			/// </summary>
			public int Packets;

			/// <summary>
			/// 04 - PacketsRead
			/// </summary>
			public uint PacketsRead;

			/// <summary>
			/// 08 - packetsWritten
			/// </summary>
			public uint PacketsWritten;
			
			/// <summary>
			/// 0C - PacketsFree - Returned by sceMpegRingbufferAvailableSize
			/// </summary>
			public int PacketsFree;
			
			/// <summary>
			/// 10 - PacketSize
			/// </summary>
			public uint PacketSize;

			/// <summary>
			/// 14 - Data;
			/// </summary>
			public PspPointer Data;

			/// <summary>
			/// 18 - Callback
			/// </summary>
			//sceMpegRingbufferCB Callback;
			public uint Callback;

			/// <summary>
			/// 1C - CallbackParameter
			/// </summary>
			public PspPointer CallbackParameter;

			/// <summary>
			/// 20 - DataUpperBound
			/// </summary>
			public PspPointer DataUpperBound;
			
			/// <summary>
			/// 24 - SemaId
			/// </summary>
			public int SemaId;
			
			/// <summary>
			/// 28 - Pointer to SceMpeg
			/// </summary>
			public PspPointer SceMpeg;
		}

		public struct SceMpegAvcMode
		{
			/// <summary>
			/// unknown, set to -1
			/// </summary>
			public int Unknown;
			
			/// <summary>
			/// Decode pixelformat
			/// </summary>
			public GuPixelFormats PixelFormat;
		}
	}
}

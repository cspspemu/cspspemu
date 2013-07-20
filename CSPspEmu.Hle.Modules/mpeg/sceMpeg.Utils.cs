using System.Runtime.InteropServices;
using CSharpUtils.Endian;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Hle.Modules.mpeg
{
	public unsafe partial class sceMpeg
	{
		/// <summary>
		/// MPEG AVC elementary stream.
		/// MPEG packet size.
		/// </summary>
		protected const int MPEG_AVC_ES_SIZE = 2048;

		/// <summary>
		/// MPEG ATRAC elementary stream.
		/// </summary>
		protected const int MPEG_ATRAC_ES_SIZE = 2112;

		/// <summary>
		/// 
		/// </summary>
		protected const int RingBufferPacketSize = 0x800;

		/// <summary>
		/// 
		/// </summary>
		public const int MPEG_ATRAC_ES_OUTPUT_SIZE = 8192;

		public SceMpeg* GetSceMpegData(SceMpegPointer* SceMpeg)
		{
			return SceMpeg->GetSceMpeg(Memory);
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

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SceMpegPointer
		{
			public PspPointer SceMpeg;

			public SceMpeg* GetSceMpeg(PspMemory PspMemory)
			{
				return (SceMpeg *)PspMemory.PspPointerToPointerSafe(SceMpeg);
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10000)]
		public struct SceMpeg
		{
			/// <summary>
			/// 0000 - 
			/// </summary>
			public fixed byte MagicBytes[12];

			/// <summary>
			/// 000C - 
			/// </summary>
			public int Unknown1;

			/// <summary>
			/// 0010 - 
			/// </summary>
			public PspPointer RingBufferAddress;

			/// <summary>
			/// 0014 - 
			/// </summary>
			public PspPointer RingBufferAddressDataUpper;

			/// <summary>
			/// 
			/// </summary>
			public SceMpegAvcMode SceMpegAvcMode;

			/// <summary>
			/// 
			/// </summary>
			public int FrameWidth;

			/// <summary>
			/// 
			/// </summary>
			public int VideoFrameCount;

			/// <summary>
			/// 
			/// </summary>
			public int AudioFrameCount;

			/// <summary>
			/// 
			/// </summary>
			public int AvcFrameStatus;

			/// <summary>
			/// 
			/// </summary>
			public int StreamSize;

			//public fixed byte Data[0x10000];
		}

		/*
		public struct SceMpegStream
		{
		}
		*/

		/// <summary>
		/// Access Unit
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SceMpegAu
		{
			/// <summary>
			/// 0000 - presentation timestamp (PTS) MSB
			/// </summary>
			public uint_be PresentationTimestampBe;

			/// <summary>
			/// 0004 - presentation timestamp (PTS) LSB
			/// </summary>
			public uint_le PresentationTimestampLe;

			/// <summary>
			/// 0008 - decode timestamp (DTS) MSB
			/// </summary>
			public uint_be DecodeTimestampBe;

			/// <summary>
			/// 000C - decode timestamp (DTS) LSB
			/// </summary>
			public uint_le DecodeTimestampLe;

			/// <summary>
			/// 0010 - Es buffer handle
			/// </summary>
			public int EsBuffer;

			/// <summary>
			/// 0014 - Au size
			/// </summary>
			public int AuSize;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SceMpegRingbuffer
		{
			/// <summary>
			/// 00 - Packets
			/// </summary>
			public int Packets;

			/// <summary>
			/// 04 - PacketsRead
			/// </summary>
			public int PacketsRead;

			/// <summary>
			/// 08 - packetsWritten
			/// </summary>
			public int PacketsWritten;
			
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

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SceMpegAvcMode
		{
			/// <summary>
			/// 0000 - unknown, set to -1
			/// </summary>
			public int Mode;
			
			/// <summary>
			/// 0004 - Decode pixelformat
			/// </summary>
			public GuPixelFormats PixelFormat;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct AvcDecodeDetailStruct
		{
			/// <summary>
			/// 0000 - Stores the result.
			/// </summary>
			public int AvcDecodeResult;

			/// <summary>
			/// 0004 - Last decoded frame.
			/// </summary>
			public int VideoFrameCount;

			/// <summary>
			/// 0008 - Frame width.
			/// </summary>
			public int AvcDetailFrameWidth;

			/// <summary>
			/// 000C - Frame height.
			/// </summary>
			public int AvcDetailFrameHeight;

			/// <summary>
			/// 0010 - Frame crop rect (left).
			/// </summary>
			public int FrameCropRectLeft;

			/// <summary>
			/// 0014 - Frame crop rect (right).
			/// </summary>
			public int FrameCropRectRight;

			/// <summary>
			/// 0018 - Frame crop rect (top).
			/// </summary>
			public int FrameCropRectTop;

			/// <summary>
			/// 001C - Frame crop rect (bottom).
			/// </summary>
			public int FrameCropRectBottom;

			/// <summary>
			/// 0x20 - Status of the last decoded frame.
			/// </summary>
			public int AvcFrameStatus;
		}
	}
}

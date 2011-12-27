using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.mpeg
{
	unsafe public partial class sceMpeg
	{
		public struct SceMpeg
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
			/// 0C - PacketsFree
			/// </summary>
			public uint PacketsFree;
			
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
			public uint SemaId;
			
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

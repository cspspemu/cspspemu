using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.libatrac3plus
{
	unsafe public partial class sceAtrac3plus
	{
		public struct PspBufferInfo
		{
			//u8* pucWritePositionFirstBuf;
			public uint pucWritePositionFirstBufPointer;
			public uint uiWritableByteFirstBuf;
			public uint uiMinWriteByteFirstBuf;
			public uint uiReadPositionFirstBuf;

			//u8* pucWritePositionSecondBuf;
			public uint pucWritePositionSecondBufPointer;
			public uint uiWritableByteSecondBuf;
			public uint uiMinWriteByteSecondBuf;
			public uint uiReadPositionSecondBuf;
		}
	}
}

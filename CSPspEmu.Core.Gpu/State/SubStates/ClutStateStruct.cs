using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Display;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	unsafe public struct ClutStateStruct
	{
		public uint Address;
		public uint Shift;
		public uint Mask;
		public uint Start;
		public PspDisplay.PixelFormats Format;
		public byte* Data;
		/*
		public PixelFormats format;
		public ubyte[] data;

		public int colorEntrySize() { return PixelFormatSize(format, 1); }
		public int blocksSize(int num_blocks) { return PixelFormatSize(format, num_blocks * 8); }
		public string hash() { return cast(string)(cast(ubyte*)cast(void*)&this)[0..data.offsetof]; }
		public string toString() { return std.string.format("ClutState(addr=%08X, format=%d, shift=%d, mask=%d, start=%d)", address, format, shift, mask, start); }
		*/
	}
}

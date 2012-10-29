namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public unsafe struct ClutStateStruct
	{
		public uint Address;
		public int Shift;
		public int Mask;
		public int Start;
		public GuPixelFormats PixelFormat;
		public byte* Data;
		public int NumberOfColors;
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

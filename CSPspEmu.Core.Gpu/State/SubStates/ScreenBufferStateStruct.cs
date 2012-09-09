using System;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ScreenBufferStateStruct
	{
		public uint Width;
		public GuPixelFormats Format;

		public byte HighAddress;
		public uint LowAddress;

		public uint LoadAddress;
		public uint StoreAddress;

		public uint Address
		{
			get
			{
				return 0x04000000 | ((uint)HighAddress << 24) | LowAddress;
			}
		}

		//uint Width = 512;
		//PspDisplay.PixelFormats Format = PspDisplay.PixelFormats.RGBA_8888;
		/*
		union {
			uint _address;
			struct { mixin(bitfields!(
				uint, "lowAddress" , 24,
				uint, "highAddress", 8
			)); }
		}
		uint address(uint _address) { return this._address = _address; }
		uint address() { return (0x04_000000 | this._address); }
		uint addressEnd() { return address + width * 272 * pixelSize; }
		uint pixelSize() { return PixelFormatSizeMul[format]; }
		ubyte[] row(void* ptr, int row) {
			int rowsize = PixelFormatSize(format, width);
			return ((cast(ubyte *)ptr) + rowsize * row)[0..rowsize];
		}
		bool isAnyAddressInBuffer(uint[] ptrList) {
			foreach (ptr; ptrList) {
				if ((ptr >= address) && (ptr < addressEnd)) return true;
			}
			return false;
		}
		*/

		public int BytesPerPixel
		{
			get
			{
				switch (Format)
				{
					case GuPixelFormats.RGBA_5650:
					case GuPixelFormats.RGBA_5551:
					case GuPixelFormats.RGBA_4444: return 2;
					case GuPixelFormats.RGBA_8888: return 4;
					default: throw (new InvalidOperationException("ScreenBufferStateStruct.BytesPerPixel : Invalid Format : " + Format));
				}
			}
		}
	}
}

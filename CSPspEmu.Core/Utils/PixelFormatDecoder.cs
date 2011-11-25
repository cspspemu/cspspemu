using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Utils
{
	unsafe public class PixelFormatDecoder
	{
		public struct OutputPixel
		{
			//public byte R, G, B, A;
			public byte B, G, R, A;
		}

		public struct Dxt1Block { }
		public struct Dxt3Block { }
		public struct Dxt5Block { }

		static public void Decode(PixelFormats PixelFormat, void* Input, OutputPixel* Output, int PixelCount, int Width = 0, void* Palette = null, PixelFormats PaletteType = PixelFormats.NONE)
		{
			switch (PixelFormat)
			{
				case PixelFormats.RGBA_5650: Decode_RGBA_5650((ushort*)Input, Output, PixelCount, Width, Palette, PaletteType); break;
				case PixelFormats.RGBA_5551: Decode_RGBA_5551((ushort*)Input, Output, PixelCount, Width, Palette, PaletteType); break;
				case PixelFormats.RGBA_4444: Decode_RGBA_4444((ushort*)Input, Output, PixelCount, Width, Palette, PaletteType); break;
				case PixelFormats.RGBA_8888: Decode_RGBA_8888((uint *)Input, Output, PixelCount, Width, Palette, PaletteType); break;

				case PixelFormats.PALETTE_T4: Decode_PALETTE_T4((byte*)Input, Output, PixelCount, Width, Palette, PaletteType); break;
				case PixelFormats.PALETTE_T8: Decode_PALETTE_T8((byte *)Input, Output, PixelCount, Width, Palette, PaletteType); break;
				case PixelFormats.PALETTE_T16: Decode_PALETTE_T16((ushort *)Input, Output, PixelCount, Width, Palette, PaletteType); break;
				case PixelFormats.PALETTE_T32: Decode_PALETTE_T32((uint *)Input, Output, PixelCount, Width, Palette, PaletteType); break;

				case PixelFormats.COMPRESSED_DXT1: Decode_COMPRESSED_DXT1((Dxt1Block*)Input, Output, PixelCount, Width, Palette, PaletteType); break;
				case PixelFormats.COMPRESSED_DXT3: Decode_COMPRESSED_DXT3((Dxt3Block*)Input, Output, PixelCount, Width, Palette, PaletteType); break;
				case PixelFormats.COMPRESSED_DXT5: Decode_COMPRESSED_DXT5((Dxt5Block*)Input, Output, PixelCount, Width, Palette, PaletteType); break;
				default: throw(new InvalidCastException());
			}
		}

		static private unsafe void Decode_COMPRESSED_DXT5(Dxt5Block* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			throw new NotImplementedException();
		}

		static private unsafe void Decode_COMPRESSED_DXT3(Dxt3Block* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			throw new NotImplementedException();
		}

		static private unsafe void Decode_COMPRESSED_DXT1(Dxt1Block* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			throw new NotImplementedException();
		}

		static private unsafe void Decode_PALETTE_T32(uint* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			throw new NotImplementedException();
		}

		static private unsafe void Decode_PALETTE_T16(ushort* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			throw new NotImplementedException();
		}

		static private unsafe void Decode_PALETTE_T8(byte* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			throw new NotImplementedException();
		}

		static private unsafe void Decode_PALETTE_T4(byte* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			throw new NotImplementedException();
		}

		static private unsafe void Decode_RGBA_8888(uint* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			for (int n = 0; n < PixelCount; n++)
			{
				uint Value = Input[n];
				Output[n].R = (byte)((Value >> 0) & 0xFF);
				Output[n].G = (byte)((Value >> 8) & 0xFF);
				Output[n].B = (byte)((Value >> 16) & 0xFF);
				Output[n].A = (byte)((Value >> 24) & 0xFF);
			}
		}

		static private unsafe void Decode_RGBA_4444(ushort* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			throw new NotImplementedException();
		}

		static private unsafe void Decode_RGBA_5551(ushort* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			for (int n = 0; n < PixelCount; n++)
			{
				ushort Value = Input[n];
				Output[n].R = (byte)Value.ExtractUnsignedScale(0, 5, 255);
				Output[n].G = (byte)Value.ExtractUnsignedScale(5, 5, 255);
				Output[n].B = (byte)Value.ExtractUnsignedScale(10, 5, 255);
				Output[n].A = (byte)Value.ExtractUnsignedScale(16, 1, 255);
			}
		}

		static private unsafe void Decode_RGBA_5650(ushort* Input, OutputPixel* Output, int PixelCount, int Width, void* Palette, PixelFormats PaletteType)
		{
			throw(new NotImplementedException());
		}
	}
}

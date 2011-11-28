using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Utils
{
	unsafe public class PixelFormatDecoder
	{
		internal PixelFormatDecoder()
		{
		}

		public struct OutputPixel
		{
			public byte R, G, B, A;
			//public byte B, G, R, A;

			public override string ToString()
			{
				return String.Format("RGBA({0},{1},{2},{3})", R, G, B, A);
			}
		}

		public struct Dxt1Block { }
		public struct Dxt3Block { }
		public struct Dxt5Block { }

		static readonly double[] Sizes =
		{
			// Rgba
			2, 2, 2, 4,
			// Palette
			0.5f, 1, 2, 4,
			// Compressed
			1, 1, 1
		};

		static public int GetPixelsSize(PixelFormats PixelFormat, int PixelCount)
		{
			return (int)(Sizes[(int)PixelFormat] * PixelCount);
		}

		void* _Input;
		OutputPixel* Output;
		int PixelCount;
		int Width;
		void* Palette;
		PixelFormats PaletteType;
		int PaletteCount;
		int PaletteStart;
		int PaletteShift;
		int PaletteMask;

		static public void Decode(PixelFormats PixelFormat, void* Input, OutputPixel* Output, int PixelCount, int Width = 0, void* Palette = null, PixelFormats PaletteType = PixelFormats.NONE, int PaletteCount = 0, int PaletteStart = 0, int PaletteShift = 0, int PaletteMask = 0xFF)
		{
			var PixelFormatInt = (int)PixelFormat;
			var PixelFormatDecoder = new PixelFormatDecoder()
			{
				_Input = Input,
				Output = Output,
				PixelCount = PixelCount,
				Width = Width,
				Palette = Palette,
				PaletteType = PaletteType,
				PaletteCount = PaletteCount,
				PaletteStart = PaletteStart,
				PaletteShift = PaletteShift,
				PaletteMask = PaletteMask,
			};
			switch (PixelFormat)
			{
				case PixelFormats.RGBA_5650: PixelFormatDecoder.Decode_RGBA_5650(); break;
				case PixelFormats.RGBA_5551: PixelFormatDecoder.Decode_RGBA_5551(); break;
				case PixelFormats.RGBA_4444: PixelFormatDecoder.Decode_RGBA_4444(); break;
				case PixelFormats.RGBA_8888: PixelFormatDecoder.Decode_RGBA_8888(); break;
				case PixelFormats.PALETTE_T4: PixelFormatDecoder.Decode_PALETTE_T4(); break;
				case PixelFormats.PALETTE_T8: PixelFormatDecoder.Decode_PALETTE_T8(); break;
				case PixelFormats.PALETTE_T16: PixelFormatDecoder.Decode_PALETTE_T16(); break;
				case PixelFormats.PALETTE_T32: PixelFormatDecoder.Decode_PALETTE_T32(); break;
				case PixelFormats.COMPRESSED_DXT1: PixelFormatDecoder.Decode_COMPRESSED_DXT1(); break;
				case PixelFormats.COMPRESSED_DXT3: PixelFormatDecoder.Decode_COMPRESSED_DXT3(); break;
				case PixelFormats.COMPRESSED_DXT5: PixelFormatDecoder.Decode_COMPRESSED_DXT5(); break;
				default: throw(new InvalidOperationException());
			}
			//DecoderCallbackTable[PixelFormatInt](Input, Output, PixelCount, Width, Palette, PaletteType, PaletteCount, PaletteStart, PaletteShift, PaletteMask);
		}

		private unsafe void Decode_COMPRESSED_DXT5()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_COMPRESSED_DXT3()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_COMPRESSED_DXT1()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_PALETTE_T32()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_PALETTE_T16()
		{
			throw new NotImplementedException();
		}

		private unsafe void Decode_PALETTE_T8()
		{
			var Input = (byte*)_Input;

			if (Palette == null || PaletteType == PixelFormats.NONE) throw (new Exception("Palette required!"));
			OutputPixel[] PalettePixels;
			int PaletteSize = 256;
			PalettePixels = new OutputPixel[PaletteSize];
			var Translate = new int[PaletteSize];
			fixed (OutputPixel* PalettePixelsPtr = PalettePixels)
			{
				Decode(PaletteType, Palette, PalettePixelsPtr, PalettePixels.Length);
				//Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
			}
			for (int n = 0; n < PaletteSize; n++)
			{
				Translate[n] = ((PaletteStart + n) >> PaletteShift) & PaletteMask;
			}

			for (int n = 0; n < PixelCount; n++)
			{
				byte Value = Input[n];
				Output[n] = PalettePixels[Translate[(Value >> 0) & 0xFF]];
			}
		}

		private unsafe void Decode_PALETTE_T4()
		{
			var Input = (byte*)_Input;

			if (Palette == null || PaletteType == PixelFormats.NONE) throw(new Exception("Palette required!"));
			OutputPixel[] PalettePixels;
			int PaletteSize = 256;
			PalettePixels = new OutputPixel[PaletteSize];
			var Translate = new int[PaletteSize];
			fixed (OutputPixel* PalettePixelsPtr = PalettePixels)
			{
				Decode(PaletteType, Palette, PalettePixelsPtr, PalettePixels.Length);
				//Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
			}
			//Console.WriteLine(PalettePixels.Length);
			for (int n = 0; n < 16; n++)
			{
				Translate[n] = ((PaletteStart + n) >> PaletteShift) & PaletteMask;
				//Console.WriteLine(PalettePixels[Translate[n]]);
			}

			for (int n = 0; n < PixelCount / 2; n++)
			{
				byte Value = Input[n];
				Output[n * 2 + 0] = PalettePixels[Translate[(Value >> 0) & 0xF]];
				Output[n * 2 + 1] = PalettePixels[Translate[(Value >> 4) & 0xF]];
				/*
				if (Output[n * 2 + 0].A == 0)
				{
					Console.WriteLine("Transparent pixel!");
				}
				*/
			}
		}

		private unsafe void Decode_RGBA_8888()
		{
			var Input = (uint*)_Input;

			for (int n = 0; n < PixelCount; n++)
			{
				OutputPixel Value = *((OutputPixel*)&Input[n]);
				Output[n].R = Value.R;
				Output[n].G = Value.G;
				Output[n].B = Value.B;
				Output[n].A = Value.A;
			}
		}

		private unsafe void Decode_RGBA_4444()
		{
			var Input = (ushort*)_Input;

			for (int n = 0; n < PixelCount; n++)
			{
				ushort Value = Input[n];
				Output[n].R = (byte)Value.ExtractUnsignedScale(0, 4, 255);
				Output[n].G = (byte)Value.ExtractUnsignedScale(4, 4, 255);
				Output[n].B = (byte)Value.ExtractUnsignedScale(8, 4, 255);
				Output[n].A = (byte)Value.ExtractUnsignedScale(12, 4, 255);
			}

		}

		private unsafe void Decode_RGBA_5551()
		{
			var Input = (ushort*)_Input;

			for (int n = 0; n < PixelCount; n++)
			{
				ushort Value = Input[n];
				Output[n].R = (byte)Value.ExtractUnsignedScale(0, 5, 255);
				Output[n].G = (byte)Value.ExtractUnsignedScale(5, 5, 255);
				Output[n].B = (byte)Value.ExtractUnsignedScale(10, 5, 255);
				Output[n].A = (byte)Value.ExtractUnsignedScale(16, 1, 255);
			}
		}

		private unsafe void Decode_RGBA_5650()
		{
			throw(new NotImplementedException());
		}
	}
}

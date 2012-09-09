using System;
using CSharpUtils;

namespace CSPspEmu.Core.Utils
{
	unsafe sealed public class PixelFormatDecoder
	{
		internal PixelFormatDecoder()
		{
		}

		public struct Dxt1Block
		{
			public uint ColorLookup;
			public ushort Color0;
			public ushort Color1;
		}
		public struct Dxt3Block
		{
			public uint ColorLookup;
			public ushort Color0;
			public ushort Color1;
			public ulong Alpha;
		}

		public struct Dxt5Block
		{
			public uint ColorLookup;
			public ushort Color0;
			public ushort Color1;
			public ulong Alpha;
		}

		static readonly double[] Sizes =
		{
			// Rgba
			2, 2, 2, 4,
			// Palette
			0.5f, 1, 2, 4,
			// Compressed
			1, 1, 1
		};

		static public int GetPixelsBits(GuPixelFormats PixelFormat)
		{
			return (int)(Sizes[(int)PixelFormat] * 8);
		}

		static public int GetPixelsSize(GuPixelFormats PixelFormat, int PixelCount)
		{
			return (int)(Sizes[(int)PixelFormat] * PixelCount);
		}

		void* _Input;
		byte* InputByte;
		ushort* InputShort;
		uint* InputInt;
		OutputPixel* Output;
		int Width;
		int Height;
		void* Palette;
		GuPixelFormats PaletteType;
		int PaletteCount;
		int PaletteStart;
		int PaletteShift;
		int PaletteMask;
		int StrideWidth;

		static public ColorFormat ColorFormatFromPixelFormat(GuPixelFormats PixelFormat)
		{
			switch (PixelFormat)
			{
				case GuPixelFormats.RGBA_8888: return ColorFormats.RGBA_8888;
				case GuPixelFormats.RGBA_5551: return ColorFormats.RGBA_5551;
				case GuPixelFormats.RGBA_5650: return ColorFormats.RGBA_5650;
				case GuPixelFormats.RGBA_4444: return ColorFormats.RGBA_4444;
				default: throw(new NotImplementedException("Not implemented " + PixelFormat));
			}
		}

		/*
		static public uint EncodePixel(GuPixelFormats PixelFormat, OutputPixel Color)
		{
			return ColorFormatFromPixelFormat(PixelFormat).Encode(Color.R, Color.G, Color.B, Color.A);
		}

		static public OutputPixel DecodePixel(GuPixelFormats PixelFormat, uint Value)
		{
			throw new NotImplementedException();
		}
		*/

		static public void Decode(GuPixelFormats PixelFormat, void* Input, OutputPixel* Output, int Width, int Height, void* Palette = null, GuPixelFormats PaletteType = GuPixelFormats.NONE, int PaletteCount = 0, int PaletteStart = 0, int PaletteShift = 0, int PaletteMask = 0xFF, int StrideWidth = -1, bool IgnoreAlpha = false)
		{
			if (StrideWidth == -1) StrideWidth = GetPixelsSize(PixelFormat, Width);
			var PixelFormatInt = (int)PixelFormat;
			var PixelFormatDecoder = new PixelFormatDecoder()
			{
				_Input = Input,
				InputByte = (byte *)Input,
				InputShort = (ushort*)Input,
				InputInt = (uint*)Input,
				Output = Output,
				StrideWidth = StrideWidth,
				Width = Width,
				Height = Height,
				Palette = Palette,
				PaletteType = PaletteType,
				PaletteCount = PaletteCount,
				PaletteStart = PaletteStart,
				PaletteShift = PaletteShift,
				PaletteMask = PaletteMask,
			};
			//Console.WriteLine(PixelFormat);
			switch (PixelFormat)
			{
				case GuPixelFormats.RGBA_5650: PixelFormatDecoder.Decode_RGBA_5650(); break;
				case GuPixelFormats.RGBA_5551: PixelFormatDecoder.Decode_RGBA_5551(); break;
				case GuPixelFormats.RGBA_4444: PixelFormatDecoder.Decode_RGBA_4444(); break;
				case GuPixelFormats.RGBA_8888: PixelFormatDecoder.Decode_RGBA_8888(); break;
				case GuPixelFormats.PALETTE_T4: PixelFormatDecoder.Decode_PALETTE_T4(); break;
				case GuPixelFormats.PALETTE_T8: PixelFormatDecoder.Decode_PALETTE_T8(); break;
				case GuPixelFormats.PALETTE_T16: PixelFormatDecoder.Decode_PALETTE_T16(); break;
				case GuPixelFormats.PALETTE_T32: PixelFormatDecoder.Decode_PALETTE_T32(); break;
				case GuPixelFormats.COMPRESSED_DXT1: PixelFormatDecoder.Decode_COMPRESSED_DXT1(); break;
				case GuPixelFormats.COMPRESSED_DXT3: PixelFormatDecoder.Decode_COMPRESSED_DXT3(); break;
				case GuPixelFormats.COMPRESSED_DXT5: PixelFormatDecoder.Decode_COMPRESSED_DXT5(); break;
				default: throw(new InvalidOperationException());
			}
			if (IgnoreAlpha)
			{
				for (int y = 0, n = 0; y < Height; y++) for (int x = 0; x < Width; x++, n++) Output[n].A = 0xFF;
			}
			//DecoderCallbackTable[PixelFormatInt](Input, Output, PixelCount, Width, Palette, PaletteType, PaletteCount, PaletteStart, PaletteShift, PaletteMask);
		}

		private unsafe void _Decode_Unimplemented()
		{
			for (int y = 0, n = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++, n++)
				{
					OutputPixel OutputPixel;
					OutputPixel.R = 0xFF;
					OutputPixel.G = (byte)(((n & 1) == 0) ? 0xFF : 0x00);
					OutputPixel.B = 0x00;
					OutputPixel.A = 0xFF;
					Output[n] = OutputPixel;
				}
			}
		}

		private unsafe void Decode_COMPRESSED_DXT5()
		{
			//Console.Error.WriteLine("Not Implemented: Decode_COMPRESSED_DXT5");
			//throw new NotImplementedException();

			//_Decode_Unimplemented();

			var Colors = new OutputPixel[4];

			for (int y = 0, ni = 0; y < Height; y += 4)
			{
				for (int x = 0; x < Width; x += 4, ni++)
				{
					var Block = ((Dxt5Block*)InputByte)[ni];
					Colors[0] = Decode_RGBA_5650_Pixel(Block.Color0).Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
					Colors[1] = Decode_RGBA_5650_Pixel(Block.Color1).Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
					Colors[2] = OutputPixel.OperationPerComponent(Colors[0], Colors[1], (a, b) => { return (byte)(((a * 2) / 3) + ((b * 1) / 3)); });
					Colors[3] = OutputPixel.OperationPerComponent(Colors[0], Colors[1], (a, b) => { return (byte)(((a * 1) / 3) + ((b * 2) / 3)); });

					// Create Alpha Lookup
					var AlphaLookup = new byte[8];
					var Alphas = (ushort)(Block.Alpha >> 48);
					var Alpha0 = (byte)((Alphas >> 0) & 0xFF);
					var Alpha1 = (byte)((Alphas >> 8) & 0xFF);

					AlphaLookup[0] = Alpha0;
					AlphaLookup[1] = Alpha1;
					if (Alpha0 > Alpha1)
					{
						AlphaLookup[2] = (byte)((6 * Alpha0 + Alpha1) / 7);
						AlphaLookup[3] = (byte)((5 * Alpha0 + 2 * Alpha1) / 7);
						AlphaLookup[4] = (byte)((4 * Alpha0 + 3 * Alpha1) / 7);
						AlphaLookup[5] = (byte)((3 * Alpha0 + 4 * Alpha1) / 7);
						AlphaLookup[6] = (byte)((2 * Alpha0 + 5 * Alpha1) / 7);
						AlphaLookup[7] = (byte)((Alpha0 + 6 * Alpha1) / 7);
					}
					else
					{
						AlphaLookup[2] = (byte)((4 * Alpha0 + Alpha1) / 5);
						AlphaLookup[3] = (byte)((3 * Alpha0 + 2 * Alpha1) / 5);
						AlphaLookup[4] = (byte)((2 * Alpha0 + 3 * Alpha1) / 5);
						AlphaLookup[5] = (byte)((Alpha0 + 4 * Alpha1) / 5);
						AlphaLookup[6] = (byte)(0x00);
						AlphaLookup[7] = (byte)(0xFF);
					}

					for (int y2 = 0, no = 0; y2 < 4; y2++)
					{
						for (int x2 = 0; x2 < 4; x2++, no++)
						{
							var Alpha = AlphaLookup[((Block.Alpha >> (3 * no)) & 0x7)];
							var Color = ((Block.ColorLookup >> (2 * no)) & 0x3);

							int rx = (x + x2);
							int ry = (y + y2);
							int n = ry * Width + rx;

							Output[n] = Colors[Color];
							Output[n].A = Alpha;
						}
					}
				}
			}
		}

		/// <summary>
		/// DXT2 and DXT3 (collectively also known as Block Compression 2 or BC2) converts 16 input pixels
		/// (corresponding to a 4x4 pixel block) into 128 bits of output, consisting of 64 bits of alpha channel data
		/// (4 bits for each pixel) followed by 64 bits of color data, encoded the same way as DXT1 (with the exception
		/// that the 4 color version of the DXT1 algorithm is always used instead of deciding which version to use based
		/// on the relative values of  and ). In DXT2, the color data is interpreted as being premultiplied by alpha, in
		/// DXT3 it is interpreted as not having been premultiplied by alpha. Typically DXT2/3 are well suited to images
		/// with sharp alpha transitions, between translucent and opaque areas.
		/// </summary>
		private unsafe void Decode_COMPRESSED_DXT3()
		{
			var Colors = new OutputPixel[4];

			for (int y = 0, ni = 0; y < Height; y += 4)
			{
				for (int x = 0; x < Width; x += 4, ni++)
				{
					var Block = ((Dxt3Block*)InputByte)[ni];
					Colors[0] = Decode_RGBA_5650_Pixel(Block.Color0).Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
					Colors[1] = Decode_RGBA_5650_Pixel(Block.Color1).Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
					Colors[2] = OutputPixel.OperationPerComponent(Colors[0], Colors[1], (a, b) => { return (byte)(((a * 2) / 3) + ((b * 1) / 3)); });
					Colors[3] = OutputPixel.OperationPerComponent(Colors[0], Colors[1], (a, b) => { return (byte)(((a * 1) / 3) + ((b * 2) / 3)); });

					for (int y2 = 0, no = 0; y2 < 4; y2++)
					{
						for (int x2 = 0; x2 < 4; x2++, no++)
						{
							var Alpha = ((Block.Alpha >> (4 * no)) & 0xF);
							var Color = ((Block.ColorLookup >> (2 * no)) & 0x3);

							int rx = (x + x2);
							int ry = (y + y2);
							int n = ry * Width + rx;

							Output[n] = Colors[Color];
							Output[n].A = (byte)((Alpha * 0xFF) / 0xF);
						}
					}
				}
			}
		}

		private unsafe void Decode_COMPRESSED_DXT1()
		{
			var Colors = new OutputPixel[4];

			for (int y = 0, ni = 0; y < Height; y += 4)
			{
				for (int x = 0; x < Width; x += 4, ni++)
				{
					var Block = ((Dxt1Block*)InputByte)[ni];
		
					Colors[0] = Decode_RGBA_5650_Pixel(Block.Color0).Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));
					Colors[1] = Decode_RGBA_5650_Pixel(Block.Color1).Transform((R, G, B, A) => OutputPixel.FromRGBA(B, G, R, A));

					if (Block.Color0 > Block.Color1)
					{
						Colors[2] = OutputPixel.OperationPerComponent(Colors[0], Colors[1], (a, b) => { return (byte)(((a * 2) / 3) + ((b * 1) / 3)); });
						Colors[3] = OutputPixel.OperationPerComponent(Colors[0], Colors[1], (a, b) => { return (byte)(((a * 1) / 3) + ((b * 2) / 3)); });
					}
					else
					{
						Colors[2] = OutputPixel.OperationPerComponent(Colors[0], Colors[1], (a, b) => { return (byte)(((a * 1) / 2) + ((b * 1) / 2)); });
						Colors[3] = OutputPixel.FromRGBA(0, 0, 0, 0);
					}

					for (int y2 = 0, no = 0; y2 < 4; y2++)
					{
						for (int x2 = 0; x2 < 4; x2++, no++)
						{
							var Color = ((Block.ColorLookup >> (2 * no)) & 0x3);

							int rx = (x + x2);
							int ry = (y + y2);
							int n = ry * Width + rx;

							Output[n] = Colors[Color];
						}
					}
				}
			}
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

			if (Palette == null || PaletteType == GuPixelFormats.NONE) throw (new Exception("Palette required!"));
			OutputPixel[] PalettePixels;
			int PaletteSize = 256;
			PalettePixels = new OutputPixel[PaletteSize];
			var Translate = new int[PaletteSize];
			fixed (OutputPixel* PalettePixelsPtr = PalettePixels)
			{
				Decode(PaletteType, Palette, PalettePixelsPtr, PalettePixels.Length, 1);
				//Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
			}
			for (int n = 0; n < PaletteSize; n++)
			{
				Translate[n] = ((PaletteStart + n) >> PaletteShift) & PaletteMask;
			}

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (byte *)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					byte Value = InputRow[x];
					Output[n] = PalettePixels[Translate[(Value >> 0) & 0xFF]];
				}
			}
		}

		private unsafe void Decode_PALETTE_T4()
		{
			var Input = (byte*)_Input;

			if (Palette == null || PaletteType == GuPixelFormats.NONE) throw(new Exception("Palette required!"));
			OutputPixel[] PalettePixels;
			int PaletteSize = 256;
			PalettePixels = new OutputPixel[PaletteSize];
			var Translate = new int[PaletteSize];
			fixed (OutputPixel* PalettePixelsPtr = PalettePixels)
			{
				Decode(PaletteType, Palette, PalettePixelsPtr, PalettePixels.Length, 1);
				//Decode(PaletteType, Palette, PalettePixelsPtr, PaletteCount);
			}
			//Console.WriteLine(PalettePixels.Length);
			for (int n = 0; n < 16; n++)
			{
				Translate[n] = ((PaletteStart + n) >> PaletteShift) & PaletteMask;
				//Console.WriteLine(PalettePixels[Translate[n]]);
			}

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (byte*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width / 2; x++, n++)
				{
					byte Value = InputRow[x];
					Output[n * 2 + 0] = PalettePixels[Translate[(Value >> 0) & 0xF]];
					Output[n * 2 + 1] = PalettePixels[Translate[(Value >> 4) & 0xF]];
				}
			}
		}

		private unsafe void Decode_RGBA_8888()
		{
			var Input = (uint*)_Input;

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (uint*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					OutputPixel Value = *((OutputPixel*)&InputRow[x]);
					Output[n].R = Value.R;
					Output[n].G = Value.G;
					Output[n].B = Value.B;
					Output[n].A = Value.A;
				}
			}
		}

		private unsafe void Decode_RGBA_4444()
		{
			//throw(new NotImplementedException());
			var Input = (ushort*)_Input;

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (ushort*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					Output[n] = Decode_RGBA_4444_Pixel(InputRow[x]);
				}
			}

		}

		private unsafe void Decode_RGBA_5551()
		{
			var Input = (ushort*)_Input;

			//long CheckSum = 0;

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (ushort*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					Decode_RGBA_5551_Pixel(InputRow[x], out Output[n]);
					//CheckSum += (long)Output[n].CheckSum;
				}
			}

			/*
			try
			{
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				Console.Error.WriteLine("Decode_RGBA_5551 : {0}x{1} : {2}", Width, Height, CheckSum);
			}
			*/
		}

		private unsafe void Decode_RGBA_5650()
		{
			var Input = (ushort*)_Input;

			for (int y = 0, n = 0; y < Height; y++)
			{
				var InputRow = (ushort*)&InputByte[y * StrideWidth];
				for (int x = 0; x < Width; x++, n++)
				{
					Output[n] = Decode_RGBA_5650_Pixel(InputRow[x]);
				}
			}
		}

		static public unsafe OutputPixel Decode_RGBA_4444_Pixel(ushort Value)
		{
			return new OutputPixel()
			{
				R = (byte)Value.ExtractUnsignedScale(0, 4, 255),
				G = (byte)Value.ExtractUnsignedScale(4, 4, 255),
				B = (byte)Value.ExtractUnsignedScale(8, 4, 255),
				A = (byte)Value.ExtractUnsignedScale(12, 4, 255),
			};
		}

		static public unsafe void Decode_RGBA_5551_Pixel(ushort Value, out OutputPixel OutputPixel)
		{
#if true
			OutputPixel.R = (byte)(((Value >> 0) & 0x1F) * 255 / 0x1F);
			OutputPixel.G = (byte)(((Value >> 5) & 0x1F) * 255 / 0x1F);
			OutputPixel.B = (byte)(((Value >> 10) & 0x1F) * 255 / 0x1F);
			OutputPixel.A = (byte)(((Value >> 15) != 0) ? 255 : 0);
#else
			OutputPixel.R = (byte)Value.ExtractUnsignedScale(0, 5, 255);
			OutputPixel.G = (byte)Value.ExtractUnsignedScale(5, 5, 255);
			OutputPixel.B = (byte)Value.ExtractUnsignedScale(10, 5, 255);
			OutputPixel.A = (byte)Value.ExtractUnsignedScale(15, 1, 255);
#endif
		}

		static public unsafe OutputPixel Decode_RGBA_5650_Pixel(ushort Value)
		{
			return new OutputPixel()
			{
				R = (byte)Value.ExtractUnsignedScale(0, 5, 255),
				G = (byte)Value.ExtractUnsignedScale(5, 6, 255),
				B = (byte)Value.ExtractUnsignedScale(11, 5, 255),
				A = 0xFF,
			};
		}

		static public unsafe OutputPixel Decode_RGBA_8888_Pixel(uint Value)
		{
			return *(OutputPixel*)&Value;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="Output"></param>
		/// <param name="RowWidth">Width of the texture. In bytes? In pixels? Maybe bytes?</param>
		/// <param name="TextureHeight">Height of the texture</param>
		static public unsafe void Unswizzle(byte[] Input, byte[] Output, int RowWidth, int TextureHeight)
		{
			fixed (void* InputPtr = Input)
			fixed (void* OutputPtr = Output)
			{
				Unswizzle(InputPtr, OutputPtr, RowWidth, TextureHeight);
			}
		}

		static public unsafe void Unswizzle(void* Input, void* Output, int RowWidth, int TextureHeight)
		{
			int pitch = (RowWidth - 16) / 4;
			int bxc = RowWidth / 16;
			int byc = TextureHeight / 8;

			var src = (uint*)Input;
			var ydest = (byte*)Output;
			for (int by = 0; by < byc; by++)
			{
				var xdest = ydest;
				for (int bx = 0; bx < bxc; bx++)
				{
					var dest = (uint*)xdest;
					for (int n = 0; n < 8; n++, dest += pitch)
					{
						*(dest++) = *(src++);
						*(dest++) = *(src++);
						*(dest++) = *(src++);
						*(dest++) = *(src++);
					}
					xdest += 16;
				}
				ydest += RowWidth * 8;
			}
		}

		static public unsafe void UnswizzleInline(void* Data, int RowWidth, int TextureHeight)
		{
			var Temp = new byte[RowWidth * TextureHeight];
			fixed (void* TempPointer = Temp)
			{
				Unswizzle(Data, TempPointer, RowWidth, TextureHeight);
			}
			PointerUtils.Memcpy((byte*)Data, Temp, RowWidth * TextureHeight);
		}

		public static unsafe void UnswizzleInline(GuPixelFormats Format, void* Data, int Width, int Height)
		{
			UnswizzleInline(Data, GetPixelsSize(Format, Width), Height);
		}

		public static unsafe ulong Hash(GuPixelFormats PixelFormat, void* Input, int Width, int Height)
		{
			int TotalBytes = GetPixelsSize(PixelFormat, Width * Height);

			return Hashing.FastHash((byte*)Input, TotalBytes, (ulong)((int)PixelFormat * Width * Height));
		}
	}
}

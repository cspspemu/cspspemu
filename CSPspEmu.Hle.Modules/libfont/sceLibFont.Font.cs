using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Utils;

namespace CSPspEmu.Hle.Modules.libfont
{
	unsafe public partial class sceLibFont
	{
		/// <summary>
		/// Closes the specified font file.
		/// </summary>
		/// <param name="FontHandle">Handle of the font.</param>
		/// <returns>
		///		0 on success.
		/// </returns>
		[HlePspFunction(NID = 0x3AEA8CB6, FirmwareVersion = 150)]
		public int sceFontClose(FontHandle FontHandle)
		{
			Fonts.Remove(FontHandle);
			return 0;
		}

		/// <summary>
		/// Obtains the FontInfo of a FontHandle.
		/// </summary>
		/// <param name="FontHandle">Font Handle to get the information from.</param>
		/// <param name="FontInfoPointer">Pointer to a FontInfo structure that will hold the information.</param>
		/// <returns>
		///		0 on success
		/// </returns>
		[HlePspFunction(NID = 0x0DA7535E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceFontGetFontInfo(FontHandle FontHandle, FontInfo* FontInfoPointer)
		{
			var Font = Fonts.Get(FontHandle);
			*FontInfoPointer = Font.GetFontInfo();
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontHandle"></param>
		/// <param name="CharCode"></param>
		/// <param name="FontCharInfoPointer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xDCC80C2F, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceFontGetCharInfo(FontHandle FontHandle, ushort CharCode, ref FontCharInfo FontCharInfoPointer)
		{
			try
			{
				var Font = Fonts.Get(FontHandle);
				FontCharInfoPointer = Font.GetCharInfo(CharCode);
				Console.WriteLine("sceFontGetCharInfo({0}) : {1}", CharCode, FontCharInfoPointer);
				return 0;
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				return -1;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontHandle"></param>
		/// <param name="CharCode"></param>
		/// <param name="GlyphImagePointer"></param>
		[HlePspFunction(NID = 0x980F4895, FirmwareVersion = 150)]
		public int sceFontGetCharGlyphImage(FontHandle FontHandle, ushort CharCode, ref GlyphImage GlyphImagePointer)
		{
			var Font = Fonts.Get(FontHandle);
			var CharInfo = Font.GetCharInfo(CharCode);
			return sceFontGetCharGlyphImage_Clip(
				FontHandle, CharCode, ref GlyphImagePointer,
				//(int)CharInfo.BitmapLeft,
				//(int)CharInfo.BitmapTop,
				0,
				0,
				(int)CharInfo.BitmapWidth,
				(int)CharInfo.BitmapHeight
			);
		}

		public class FontBitmap
		{
			protected FontPixelFormat FontPixelFormat;
			public int Width;
			public int BitsPerPixel;
			public int Height;
			public int BytesPerLine;
			public byte* Address;

			public FontBitmap(byte* Address, FontPixelFormat FontPixelFormat, int Width, int Height, int BytesPerLine)
			{
				this.Address = Address;
				this.FontPixelFormat = FontPixelFormat;
				this.Width = Width;
				this.Height = Height;
				this.BytesPerLine = BytesPerLine;
				switch (FontPixelFormat)
				{
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_4: this.BitsPerPixel = 4; break;
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_4_REV: this.BitsPerPixel = 4; break;
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_8: this.BitsPerPixel = 8; break;
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_24: this.BitsPerPixel = 24; break;
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_32: this.BitsPerPixel = 32; break;
				}
			}

			private int GetOffset(int X, int Y)
			{
				return Y * BytesPerLine + (X * this.BitsPerPixel) / 8;
			}

			public void SetPixel(int X, int Y, OutputPixel Color)
			{
				if (X < 0 || Y < 0) return;
				if (X >= Width || Y >= Height) return;
				var Offset = GetOffset(X, Y);
				var WriteAddress = (byte *)(Address + Offset);

				//byte C = (byte)((Color.R + Color.G + Color.B) * 15 / 3 / 255);
				byte C = (byte)(Color.R * 15 / 255);

				switch (FontPixelFormat)
				{
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_4:
						*WriteAddress = (byte)((*WriteAddress & 0xF0) | ((C & 0xF) << 0));
					break;
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_4_REV:
						*WriteAddress = (byte)((*WriteAddress & 0x0F) | ((C & 0xF) << 4));
					break;
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_8:
						*WriteAddress = Color.A;
					break;
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_24:
						*(WriteAddress + 0) = Color.R;
						*(WriteAddress + 1) = Color.G;
						*(WriteAddress + 2) = Color.B;
						break;
					case sceLibFont.FontPixelFormat.PSP_FONT_PIXELFORMAT_32:
						*(WriteAddress + 0) = Color.R;
						*(WriteAddress + 1) = Color.G;
						*(WriteAddress + 2) = Color.B;
						*(WriteAddress + 3) = Color.A;
						break;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontHandle"></param>
		/// <param name="CharCode"></param>
		/// <param name="GlyphImage"></param>
		/// <param name="ClipX"></param>
		/// <param name="ClipY"></param>
		/// <param name="ClipWidth"></param>
		/// <param name="ClipHeight"></param>
		[HlePspFunction(NID = 0xCA1E6945, FirmwareVersion = 150)]
		public int sceFontGetCharGlyphImage_Clip(FontHandle FontHandle, ushort CharCode, ref GlyphImage GlyphImage, int ClipX, int ClipY, int ClipWidth, int ClipHeight)
		{
			try
			{
				var Font = Fonts.Get(FontHandle);
				var Glyph = Font.GetGlyph(CharCode);
				var CharInfo = Font.GetCharInfo(CharCode);
				var Face = Glyph.Face;
				var PixelFormat = GlyphImage.PixelFormat;
				var Buffer = PspMemory.PspAddressToPointerSafe(GlyphImage.Buffer);
				var BufferHeight = GlyphImage.BufferHeight;
				var BufferWidth = GlyphImage.BufferWidth;
				var BytesPerLine = GlyphImage.BytesPerLine;
				var Position = GlyphImage.Position;
				var GlyphBitmap = Face.GetBitmap();
				var OutputBitmap = new FontBitmap((byte*)Buffer, PixelFormat, (int)BufferWidth, (int)BufferHeight, BytesPerLine);

				Console.WriteLine(
					"sceFontGetCharGlyphImage_Clip({0}, ({1}, {2})-({3}, {4}) : {5}) : {6}",
					CharCode, ClipX, ClipY, ClipWidth, ClipHeight, PixelFormat, Position
				);

				ClipWidth = Math.Min(ClipWidth, BufferWidth - ClipX);
				ClipHeight = Math.Min(ClipHeight, BufferHeight - ClipY);

				ClipWidth = Math.Min(ClipWidth, GlyphBitmap.Width - ClipX);
				ClipHeight = Math.Min(ClipHeight, GlyphBitmap.Height - ClipY);

				try
				{
					for (int y = 0; y < ClipHeight; y++)
					{
						for (int x = 0; x < ClipWidth; x++)
						{
							//Console.WriteLine();
							var Pixel = GlyphBitmap.GetPixel(x + ClipX, y + ClipY);
							OutputBitmap.SetPixel(x + (int)Position.X, y + (int)Position.Y, new OutputPixel(Pixel));
							//Console.Write(Pixel.R > 0x7F ? "X" : ".");
							//OutputBitmap.SetPixel(x, y, new OutputPixel(Color.Red));
						}
						//Console.WriteLine("");
					}
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
				}

				//for (int n = 0; n < )
				//Console.Error.WriteLine("'{0}': {1}", (char)CharCode, Glyph);
				//throw (new NotImplementedException());
				return 0;
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				return -1;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontHandle"></param>
		/// <param name="CharCode"></param>
		/// <param name="CharRectPointer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x5C3E4A9E, FirmwareVersion = 150)]
		public int sceFontGetCharImageRect(FontHandle FontHandle, ushort CharCode, CharRect* CharRectPointer)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontHandle"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x02D7F94B, FirmwareVersion = 150)]
		public int sceFontFlush(FontHandle FontHandle)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x2F67356A, FirmwareVersion = 150)]
		public void sceFontCalcMemorySize()
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x48B06520, FirmwareVersion = 150)]
		public void sceFontGetShadowImageRect()
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x568BE516, FirmwareVersion = 150)]
		public void sceFontGetShadowGlyphImage()
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x5DCF6858, FirmwareVersion = 150)]
		public void sceFontGetShadowGlyphImage_Clip()
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xAA3DE7B5, FirmwareVersion = 150)]
		public void sceFontGetShadowInfo()
		{
			throw (new NotImplementedException());
		}
	}
}

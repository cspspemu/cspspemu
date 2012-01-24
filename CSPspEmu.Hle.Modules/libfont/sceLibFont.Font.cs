using System;
using System.Collections.Generic;
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
		public int sceFontGetCharInfo(FontHandle FontHandle, ushort CharCode, FontCharInfo* FontCharInfoPointer)
		{
			var Font = Fonts.Get(FontHandle);
			*FontCharInfoPointer = Font.GetCharInfo(CharCode);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontHandle"></param>
		/// <param name="CharCode"></param>
		/// <param name="GlyphImagePointer"></param>
		[HlePspFunction(NID = 0x980F4895, FirmwareVersion = 150)]
		public int sceFontGetCharGlyphImage(FontHandle FontHandle, ushort CharCode, GlyphImage* GlyphImagePointer)
		{
			var Font = Fonts.Get(FontHandle);
			var CharInfo = Font.GetCharInfo(CharCode);
			return sceFontGetCharGlyphImage_Clip(
				FontHandle, CharCode, GlyphImagePointer,
				(int)CharInfo.BitmapLeft,
				(int)CharInfo.BitmapTop,
				(int)CharInfo.BitmapWidth,
				(int)CharInfo.BitmapHeight
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontHandle"></param>
		/// <param name="CharCode"></param>
		/// <param name="GlyphImagePointer"></param>
		/// <param name="ClipX"></param>
		/// <param name="ClipY"></param>
		/// <param name="ClipWidth"></param>
		/// <param name="ClipHeight"></param>
		[HlePspFunction(NID = 0xCA1E6945, FirmwareVersion = 150)]
		public int sceFontGetCharGlyphImage_Clip(FontHandle FontHandle, ushort CharCode, GlyphImage* GlyphImagePointer, int ClipX, int ClipY, int ClipWidth, int ClipHeight)
		{
			var Font = Fonts.Get(FontHandle);
			var Glyph = Font.GetGlyph(CharCode);
			var Face = Glyph.Face;
			var PixelFormat = GlyphImagePointer->PixelFormat;
			var Buffer = PspMemory.PspAddressToPointerSafe(GlyphImagePointer->Buffer);
			var BufferHeight = GlyphImagePointer->BufferHeight;
			var BufferWidth = GlyphImagePointer->BufferWidth;
			var Position = GlyphImagePointer->Position;
			var GlyphBitmap = Face.GetBitmap();
			var OutputBitmap = new PspBitmap(PixelFormat, (int)BufferWidth, (int)BufferHeight, (byte*)Buffer);

			try
			{
				for (int y = 0; y < ClipHeight; y++)
				{
					for (int x = 0; x < ClipWidth; x++)
					{
						OutputBitmap.SetPixel(x, y, new OutputPixel(GlyphBitmap.GetPixel(x + ClipX, y + ClipY)));
					}
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

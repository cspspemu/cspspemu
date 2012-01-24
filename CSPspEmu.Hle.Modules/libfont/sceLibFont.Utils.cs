using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Formats.Font;

namespace CSPspEmu.Hle.Modules.libfont
{
	unsafe public partial class sceLibFont
	{
		public struct FontNewLibParams
		{
			/// <summary>
			/// 
			/// </summary>
			public uint UserDataAddr;

			/// <summary>
			/// 
			/// </summary>
			public uint NumberOfFonts;

			/// <summary>
			/// 
			/// </summary>
			public uint CacheDataAddr;

			// Driver callbacks.
			public uint AllocFuncAddr;
			public uint FreeFuncAddr;
			public uint OpenFuncAddr;
			public uint CloseFuncAddr;
			public uint ReadFuncAddr;
			public uint SeekFuncAddr;
			public uint ErrorFuncAddr;
			public uint IoFinishFuncAddr;
		}

		public class FontRegistryEntry
		{
			public FontStyle FontStyle;
			public int ExtraAttributes;

			public FontRegistryEntry(int HorizontalSize, int VerticalSize, int HorizontalResolution,
				int VerticalResolution, int ExtraAttributes, int Weight,
				FamilyEnum Family, StyleEnum StyleStyle, ushort StyleSub, LanguageEnum Language,
				ushort Region, ushort Country, String FileName,
				String Name, uint Expire, int ShadowOption)
			{
				this.ExtraAttributes = ExtraAttributes;
				this.ShadowOption = ShadowOption;
				this.FontStyle = new FontStyle()
				{
					Size = new HorizontalVerticalFloat() { Horizontal = HorizontalSize, Vertical = VerticalSize, },
					Resolution = new HorizontalVerticalFloat() { Horizontal = HorizontalResolution, Vertical = VerticalResolution, },
					Weight = Weight,
					Family = Family,
					StyleStyle = StyleStyle,
					StyleSub = StyleSub,
					Language = Language,
					Region = Region,
					Country = Country,
					FileName = FileName,
					Name = Name,
					Expire = Expire,
				};
			}

			public int ShadowOption { get; set; }
		}

		public struct FontInfo
		{
			#region Glyph metrics (in 26.6 signed fixed-point).
			public Fixed26_6 MaxGlyphWidthI;
			public Fixed26_6 MaxGlyphHeightI;
			public Fixed26_6 MaxGlyphAscenderI;
			public Fixed26_6 MaxGlyphDescenderI;
			public Fixed26_6 MaxGlyphLeftXI;
			public Fixed26_6 MaxGlyphBaseYI;
			public Fixed26_6 MinGlyphCenterXI;
			public Fixed26_6 MaxGlyphTopYI;
			public Fixed26_6 MaxGlyphAdvanceXI;
			public Fixed26_6 MaxGlyphAdvanceYI;
			#endregion

			#region Glyph metrics (replicated as float).
			public float MaxGlyphWidthF;
			public float MaxGlyphHeightF;
			public float MaxGlyphAscenderF;
			public float MaxGlyphDescenderF;
			public float MaxGlyphLeftXF;
			public float MaxGlyphBaseYF;
			public float MinGlyphCenterXF;
			public float MaxGlyphTopYF;
			public float MaxGlyphAdvanceXF;
			public float MaxGlyphAdvanceYF;
			#endregion

			#region Bitmap dimensions.
			/// <summary>
			/// 
			/// </summary>
			public ushort MaxGlyphWidth;

			/// <summary>
			/// 
			/// </summary>
			public ushort MaxGlyphHeight;
			
			/// <summary>
			/// Number of elements in the font's charmap.
			/// </summary>
			public uint CharMapLength;
			
			/// <summary>
			/// Number of elements in the font's shadow charmap.
			/// </summary>
			public uint ShadowMapLength;
    
			/// <summary>
			/// Font style (used by font comparison functions).
			/// </summary>
			public FontStyle FontStyle;
			#endregion

			/// <summary>
			/// Font's BPP. = 4
			/// </summary>
			public byte BPP;

			/// <summary>
			/// Padding.
			/// </summary>
			public fixed byte Pad[3];
		}

		/// <summary>
		/// Char's metrics:
		/// 
		///           Width / Horizontal Advance
		///           <---------->
		///      |           000 |
		///      |           000 |  Ascender
		///      |           000 |
		///      |     000   000 |
		///      | -----000--000-------- Baseline
		///      |        00000  |  Descender
		/// Height /
		/// Vertical Advance
		/// 
		/// The char's bearings represent the difference between the
		/// width and the horizontal advance and/or the difference
		/// between the height and the vertical advance.
		/// In our debug font, these measures are the same (block pixels),
		/// but in real PGF fonts they can vary (italic fonts, for example).
		/// </summary>
		public struct FontCharInfo
		{
			public uint BitmapWidth;
			public uint BitmapHeight;
			public int BitmapLeft;
			public int BitmapTop;

			// Glyph metrics (in 26.6 signed fixed-point).
			public Fixed26_6 Width;
			public Fixed26_6 Height;
			public Fixed26_6 Ascender;
			public Fixed26_6 Descender;
			public Fixed26_6 BearingHX;
			public Fixed26_6 BearingHY;
			public Fixed26_6 BearingVX;
			public Fixed26_6 BearingVY;
			public Fixed26_6 AdvanceH;
			public Fixed26_6 AdvanceV;
			public int Padding;
    
			//static assert(this.sizeof == 4 * 15);
		}

		public struct GlyphImage
		{
			/// <summary>
			/// 
			/// </summary>
			public GuPixelFormats PixelFormat;

			/// <summary>
			/// 
			/// </summary>
			public PointFixed26_6 Position;

			/// <summary>
			/// 
			/// </summary>
			public short BufferWidth;

			/// <summary>
			/// 
			/// </summary>
			public short BufferHeight;

			/// <summary>
			/// 
			/// </summary>
			public short BytesPerLine;

			/// <summary>
			/// 
			/// </summary>
			public short __Padding;
			
			/// <summary>
			/// 
			/// </summary>
			public PspPointer Buffer;
		}

		public struct CharRect
		{
			public short X;
			public short Y;

			// ?
			public short Width;
			public short Height;
		}
	}
}

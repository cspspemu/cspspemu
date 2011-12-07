using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		public struct FontStyle
		{
			public enum Family : ushort
			{
				FONT_FAMILY_SANS_SERIF = 1,
				FONT_FAMILY_SERIF      = 2,
			}
	
			public enum Style : ushort
			{
				FONT_STYLE_REGULAR     = 1,
				FONT_STYLE_ITALIC      = 2,
				FONT_STYLE_BOLD        = 5,
				FONT_STYLE_BOLD_ITALIC = 6,
				FONT_STYLE_DB          = 103, // Demi-Bold / semi-bold
			}
	
			public enum Language : ushort
			{
				FONT_LANGUAGE_JAPANESE = 1,
				FONT_LANGUAGE_LATIN    = 2,
				FONT_LANGUAGE_KOREAN   = 3,
			}

			public float    fontH;
			public float    fontV;
			public float    fontHRes;
			public float    fontVRes;
			public float    fontWeight;
			public Family   fontFamily;
			public Style    fontStyleStyle;
			// Check.
			public ushort   fontStyleSub;
			public Language fontLanguage;
			public ushort   fontRegion;
			public ushort   fontCountry;
			public fixed byte fontFileName[64];
			public fixed byte fontName[64];
			public uint     fontAttributes;
			public uint     fontExpire;
		}

		public struct FontInfo
		{
			// Glyph metrics (in 26.6 signed fixed-point).
			public uint maxGlyphWidthI;
			public uint maxGlyphHeightI;
			public uint maxGlyphAscenderI;
			public uint maxGlyphDescenderI;
			public uint maxGlyphLeftXI;
			public uint maxGlyphBaseYI;
			public uint minGlyphCenterXI;
			public uint maxGlyphTopYI;
			public uint maxGlyphAdvanceXI;
			public uint maxGlyphAdvanceYI;

			// Glyph metrics (replicated as float).
			public float maxGlyphWidthF;
			public float maxGlyphHeightF;
			public float maxGlyphAscenderF;
			public float maxGlyphDescenderF;
			public float maxGlyphLeftXF;
			public float maxGlyphBaseYF;
			public float minGlyphCenterXF;
			public float maxGlyphTopYF;
			public float maxGlyphAdvanceXF;
			public float maxGlyphAdvanceYF;
    
			// Bitmap dimensions.
			public short maxGlyphWidth;
			public short maxGlyphHeight;
			public uint  charMapLength;   // Number of elements in the font's charmap.
			public uint  shadowMapLength; // Number of elements in the font's shadow charmap.
    
			// Font style (used by font comparison functions).
			public FontStyle fontStyle;

			public byte BPP; // Font's BPP. = 4
			public fixed byte pad[3];
		}

		/*
		 * Char's metrics:
		 *
		 *           Width / Horizontal Advance
		 *           <---------->
		 *      |           000 |
		 *      |           000 |  Ascender
		 *      |           000 |
		 *      |     000   000 |
		 *      | -----000--000-------- Baseline
		 *      |        00000  |  Descender
		 * Height /
		 * Vertical Advance
		 *
		 * The char's bearings represent the difference between the
		 * width and the horizontal advance and/or the difference
		 * between the height and the vertical advance.
		 * In our debug font, these measures are the same (block pixels),
		 * but in real PGF fonts they can vary (italic fonts, for example).
		 */
		public struct FontCharInfo
		{
			public uint bitmapWidth;
			public uint bitmapHeight;
			public uint bitmapLeft;
			public uint bitmapTop;

			// Glyph metrics (in 26.6 signed fixed-point).
			public uint sfp26Width;
			public uint sfp26Height;
			public uint sfp26Ascender;
			public uint sfp26Descender;
			public uint sfp26BearingHX;
			public uint sfp26BearingHY;
			public uint sfp26BearingVX;
			public uint sfp26BearingVY;
			public uint sfp26AdvanceH;
			public uint sfp26AdvanceV;
			public uint padding;
    
			//static assert(this.sizeof == 4 * 15);
		}

	}
}

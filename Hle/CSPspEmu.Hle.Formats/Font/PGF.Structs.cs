using System;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Hle.Formats.Font
{
	public unsafe partial class PGF
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct HeaderStruct
		{
			/// <summary>
			/// = 0
			/// </summary>
			public ushort HeaderOffset;

			/// <summary>
			/// 392 =
			/// </summary>
			public ushort HeaderSize;

			/// <summary>
			/// "PGF0" 
			/// </summary>
			public fixed byte Magic[4];

			/// <summary>
			/// = 2;
			/// </summary>
			public uint Revision;

			/// <summary>
			/// = 6;
			/// </summary>
			public uint Version;

			/// <summary>
			/// 
			/// </summary>
			public int TableCharMapLength;

			/// <summary>
			/// 
			/// </summary>
			public int TableCharPointerLength;

			/// <summary>
			/// Number of bits per packedCharMap entry.
			/// </summary>
			public int TableCharMapBpe;

			/// <summary>
			/// Number of bits per packedCharPointerTable entry.
			/// </summary>
			public int TableCharPointerBpe;

			/// <summary>
			/// 
			/// </summary>
			public uint __unk1;

			/// <summary>
			/// 
			/// </summary>
			//public PointFixed26_6 Size;
			public Point32 Size;

			/// <summary>
			/// Resolution of a single character?
			/// </summary>
			public Point32 Resolution;

			/// <summary>
			/// 
			/// </summary>
			public byte __unk2;

			/// <summary>
			/// 
			/// </summary>
			public fixed byte RawFontName[64];

			/// <summary>
			/// 
			/// </summary>
			public String FontName { get { fixed (byte* Pointer = RawFontName) return PointerUtils.PtrToStringUtf8(Pointer); } }

			/// <summary>
			/// 
			/// </summary>
			public fixed byte RawFontType[64];

			/// <summary>
			/// 
			/// </summary>
			public String FontType { get { fixed (byte* Pointer = RawFontType) return PointerUtils.PtrToStringUtf8(Pointer); } }

			/// <summary>
			/// 
			/// </summary>
			public byte __unk3;

			/// <summary>
			/// 
			/// </summary>
			public ushort FirstGlyph;

			/// <summary>
			/// 
			/// </summary>
			public ushort LastGlyph;

			/// <summary>
			/// 
			/// </summary>
			public fixed byte __unk4[34];

			/// <summary>
			/// 
			/// </summary>
			public Fixed26_6 MaxLeftXAdjust;

			/// <summary>
			/// 
			/// </summary>
			public Fixed26_6 MaxBaseYAdjust;

			/// <summary>
			/// 
			/// </summary>
			public Fixed26_6 MinCenterXAdjust;

			/// <summary>
			/// 
			/// </summary>
			public Fixed26_6 MaxTopYAdjust;

			/// <summary>
			/// 
			/// </summary>
			public PointFixed26_6 MaxAdvance;

			/// <summary>
			/// 
			/// </summary>
			public PointFixed26_6 MaxSize;

			/// <summary>
			/// 
			/// </summary>
			public ushort MaxGlyphWidth;

			/// <summary>
			/// 
			/// </summary>
			public ushort MaxGlyphHeight;

			/// <summary>
			/// 
			/// </summary>
			public ushort __unk5;

			/// <summary>
			/// 
			/// </summary>
			public byte TableDimLength;

			/// <summary>
			/// 
			/// </summary>
			public byte TableXAdjustLength;

			/// <summary>
			/// 
			/// </summary>
			public byte TableYAdjustLength;

			/// <summary>
			/// 
			/// </summary>
			public byte TableAdvanceLength;

			/// <summary>
			/// 
			/// </summary>
			public fixed byte __unk6[102];

			/// <summary>
			/// 
			/// </summary>
			public int TableShadowMapLength;

			/// <summary>
			/// 
			/// </summary>
			public int TableShadowMapBpe;

			/// <summary>
			/// 
			/// </summary>
			public uint __unk7;

			/// <summary>
			/// 
			/// </summary>
			public Point32 ShadowScale;

			/// <summary>
			/// 
			/// </summary>
			public ulong __unk8;

			// Revision 3
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct HeaderRevision3Struct
		{
			/// <summary>
			/// 
			/// </summary>
			public uint TableCompCharMapBpe1;

			/// <summary>
			/// 
			/// </summary>
			public ushort TableCompCharMapLength1;

			/// <summary>
			/// 
			/// </summary>
			public ushort __unk1;

			/// <summary>
			/// 
			/// </summary>
			public uint TableCompCharMapBpe2;

			/// <summary>
			/// 
			/// </summary>
			public ushort TableCompCharMapLength2;

			/// <summary>
			/// 
			/// </summary>
			public fixed byte __unk2[6];
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct Point32
	{
		public int X;
		public int Y;

		public override string ToString()
		{
			return String.Format("Point32({0}; {1})", X, Y);
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 8)]
	public struct PointFixed26_6
	{
		public Fixed26_6 X;
		public Fixed26_6 Y;

		public override string ToString()
		{
			return String.Format("Point32({0}; {1})", X, Y);
		}
	}

	/// <summary>
	/// 26.6 signed fixed-point.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
	public struct Fixed26_6
	{
		private int RawValue;

		public float Value
		{
			get
			{
				return (float)((double)RawValue / Math.Pow(2, 6));
			}
			set
			{
				RawValue = (int)(((double)value) * Math.Pow(2, 6));
			}
		}

		public static implicit operator float(Fixed26_6 that)
		{
			return that.Value;
		}

		public static implicit operator Fixed26_6(float that)
		{
			return new Fixed26_6()
			{
				Value = that,
			};
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public struct MapUshort
	{
		public uint Src;
		public uint Dst;

		public override string ToString()
		{
			return String.Format("MapUshort({0}, {1})", Src, Dst);
		}
	}

	public struct MapUint
	{
		public uint Src;
		public uint Dst;

		public override string ToString()
		{
			return String.Format("MapUint({0}, {1})", Src, Dst);
		}
	}

	public struct MapInt
	{
		public int Src;
		public int Dst;

		public override string ToString()
		{
			return String.Format("MapUint({0}, {1})", Src, Dst);
		}
	}

	public enum FamilyEnum : ushort
	{
		FONT_FAMILY_SANS_SERIF = 1,
		FONT_FAMILY_SERIF = 2,
	}

	public enum StyleEnum : ushort
	{
		FONT_STYLE_REGULAR = 1,
		FONT_STYLE_ITALIC = 2,
		FONT_STYLE_BOLD = 5,
		FONT_STYLE_BOLD_ITALIC = 6,
		FONT_STYLE_DB = 103, // Demi-Bold / semi-bold
	}

	public enum LanguageEnum : ushort
	{
		FONT_LANGUAGE_JAPANESE = 1,
		FONT_LANGUAGE_LATIN = 2,
		FONT_LANGUAGE_KOREAN = 3,
	}

	public unsafe struct FontStyle
	{
		/// <summary>
		/// 
		/// </summary>
		public HorizontalVerticalFloat Size;

		/// <summary>
		/// 
		/// </summary>
		public HorizontalVerticalFloat Resolution;

		/// <summary>
		/// 
		/// </summary>
		public float Weight;

		/// <summary>
		/// 
		/// </summary>
		public FamilyEnum Family;

		/// <summary>
		/// 
		/// </summary>
		public StyleEnum StyleStyle;

		/// <summary>
		/// 
		/// </summary>
		public ushort StyleSub;

		/// <summary>
		/// 
		/// </summary>
		public LanguageEnum Language;

		/// <summary>
		/// 
		/// </summary>
		public ushort Region;

		/// <summary>
		/// 
		/// </summary>
		public ushort Country;

		/// <summary>
		/// 
		/// </summary>
		private fixed byte RawFileName[64];

		public string FileName
		{
			get { fixed (byte* Pointer = RawFileName) return PointerUtils.PtrToStringUtf8(Pointer); }
			set { fixed (byte* Pointer = RawFileName) PointerUtils.StoreStringOnPtr(value, Encoding.UTF8, Pointer, 64); }
		}

		/// <summary>
		/// 
		/// </summary>
		private fixed byte RawName[64];

		public string Name
		{
			get { fixed (byte* Pointer = RawName) return PointerUtils.PtrToStringUtf8(Pointer); }
			set { fixed (byte* Pointer = RawName) PointerUtils.StoreStringOnPtr(value, Encoding.UTF8, Pointer, 64); }
		}

		/// <summary>
		/// 
		/// </summary>
		public uint Attributes;

		/// <summary>
		/// 
		/// </summary>
		public uint Expire;

		public static float GetScoreCompare(FontStyle Left, FontStyle Right)
		{
			float Score = 0.0f;
			if (Left.Size == Right.Size) Score++;
			if (Left.Resolution == Right.Resolution) Score++;

			return Score;
		}
	}

	public struct HorizontalVerticalFloat
	{
		/// <summary>
		/// 
		/// </summary>
		public float Horizontal;

		/// <summary>
		/// 
		/// </summary>
		public float Vertical;

		public HorizontalVerticalFloat(float Horizontal, float Vertical)
		{
			this.Horizontal = Horizontal;
			this.Vertical = Vertical;
		}

		public static bool operator ==(HorizontalVerticalFloat Left, HorizontalVerticalFloat Right)
		{
			if (Left.Horizontal != Right.Horizontal) return false;
			if (Left.Vertical != Right.Vertical) return false;
			return true;
		}

		public static bool operator !=(HorizontalVerticalFloat Left, HorizontalVerticalFloat Right)
		{
			return !(Left == Right);
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() != typeof(HorizontalVerticalFloat)) return false;
			return (HorizontalVerticalFloat)obj == this;
		}

		public override int GetHashCode()
		{
			return Horizontal.GetHashCode() ^ Vertical.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("HV({0}, {1})", Horizontal, Vertical);
		}
	}
}

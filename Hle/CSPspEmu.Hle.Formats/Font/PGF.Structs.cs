using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Hle.Formats.Font
{
    public unsafe partial class Pgf
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
            public string FontName
            {
                get
                {
                    fixed (byte* pointer = RawFontName) return PointerUtils.PtrToStringUtf8(pointer);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public fixed byte RawFontType[64];

            /// <summary>
            /// 
            /// </summary>
            public string FontType
            {
                get
                {
                    fixed (byte* pointer = RawFontType) return PointerUtils.PtrToStringUtf8(pointer);
                }
            }

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
            public Fixed266 MaxLeftXAdjust;

            /// <summary>
            /// 
            /// </summary>
            public Fixed266 MaxBaseYAdjust;

            /// <summary>
            /// 
            /// </summary>
            public Fixed266 MinCenterXAdjust;

            /// <summary>
            /// 
            /// </summary>
            public Fixed266 MaxTopYAdjust;

            /// <summary>
            /// 
            /// </summary>
            public PointFixed266 MaxAdvance;

            /// <summary>
            /// 
            /// </summary>
            public PointFixed266 MaxSize;

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

        public override string ToString() => $"Point32({X}; {Y})";
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 8)]
    public struct PointFixed266
    {
        public Fixed266 X;
        public Fixed266 Y;

        public override string ToString() => $"Point32({X}; {Y})";
    }

    /// <summary>
    /// 26.6 signed fixed-point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public struct Fixed266
    {
        private int RawValue;

        public float Value
        {
            get => (float) (RawValue / Math.Pow(2, 6));
            set => RawValue = (int) (value * Math.Pow(2, 6));
        }

        public static implicit operator float(Fixed266 that) => that.Value;

        public static implicit operator Fixed266(float that) => new Fixed266()
        {
            Value = that,
        };

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public struct MapUshort
    {
        public uint Src;
        public uint Dst;

        public override string ToString()
        {
            return $"MapUshort({Src}, {Dst})";
        }
    }

    public struct MapUint
    {
        public uint Src;
        public uint Dst;

        public override string ToString()
        {
            return $"MapUint({Src}, {Dst})";
        }
    }

    public struct MapInt
    {
        public int Src;
        public int Dst;

        public override string ToString()
        {
            return $"MapUint({Src}, {Dst})";
        }
    }

    public enum FamilyEnum : ushort
    {
        FontFamilySansSerif = 1,
        FontFamilySerif = 2,
    }

    public enum StyleEnum : ushort
    {
        FontStyleRegular = 1,
        FontStyleItalic = 2,
        FontStyleBold = 5,
        FontStyleBoldItalic = 6,
        FontStyleDb = 103, // Demi-Bold / semi-bold
    }

    public enum LanguageEnum : ushort
    {
        FontLanguageJapanese = 1,
        FontLanguageLatin = 2,
        FontLanguageKorean = 3,
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
#pragma warning disable 649
        private fixed byte _rawFileName[64];
#pragma warning restore 649

        public string FileName
        {
            get
            {
                fixed (byte* pointer = _rawFileName) return PointerUtils.PtrToStringUtf8(pointer);
            }
            set
            {
                fixed (byte* pointer = _rawFileName) PointerUtils.StoreStringOnPtr(value, Encoding.UTF8, pointer, 64);
            }
        }

        /// <summary>
        /// 
        /// </summary>
#pragma warning disable 649
        private fixed byte _rawName[64];
#pragma warning restore 649

        public string Name
        {
            get
            {
                fixed (byte* pointer = _rawName) return PointerUtils.PtrToStringUtf8(pointer);
            }
            set
            {
                fixed (byte* pointer = _rawName) PointerUtils.StoreStringOnPtr(value, Encoding.UTF8, pointer, 64);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public uint Attributes;

        /// <summary>
        /// 
        /// </summary>
        public uint Expire;

        public static float GetScoreCompare(FontStyle left, FontStyle right)
        {
            float score = 0.0f;
            if (left.Size == right.Size) score++;
            if (left.Resolution == right.Resolution) score++;

            return score;
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

        public HorizontalVerticalFloat(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public static bool operator ==(HorizontalVerticalFloat left, HorizontalVerticalFloat right) => left.Horizontal == right.Horizontal && left.Vertical == right.Vertical;

        public static bool operator !=(HorizontalVerticalFloat left, HorizontalVerticalFloat right) => !(left == right);

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(HorizontalVerticalFloat)) return false;
            return (HorizontalVerticalFloat) obj == this;
        }

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => Horizontal.GetHashCode() ^ Vertical.GetHashCode();

        public override string ToString() => $"HV({Horizontal}, {Vertical})";
    }
}
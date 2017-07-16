using System;
using System.Runtime.InteropServices;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Formats.Font;

namespace CSPspEmu.Hle.Modules.libfont
{
    public unsafe partial class sceLibFont
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
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
                    Size = new HorizontalVerticalFloat() {Horizontal = HorizontalSize, Vertical = VerticalSize,},
                    Resolution =
                        new HorizontalVerticalFloat()
                        {
                            Horizontal = HorizontalResolution,
                            Vertical = VerticalResolution,
                        },
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

        /// <summary>
        /// Char's metrics: <para/>
        /// 
        ///           Width / Horizontal Advance  <para/>
        ///           &lt;----------&gt;          <para/>
        ///      |           000 |                <para/>
        ///      |           000 |  Ascender      <para/>
        ///      |           000 |                <para/>
        ///      |     000   000 |                <para/>
        ///      | -----000--000-------- Baseline <para/>
        ///      |        00000  |  Descender     <para/>
        /// Height /                              <para/>
        /// Vertical Advance                      <para/>
        /// <para/>
        /// The char's bearings represent the difference between the
        /// width and the horizontal advance and/or the difference
        /// between the height and the vertical advance.
        /// In our debug font, these measures are the same (block pixels),
        /// but in real PGF fonts they can vary (italic fonts, for example).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 60)]
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
            public int Unknown;

            //static assert(this.sizeof == 4 * 15);

            public override string ToString()
            {
                return String.Format(
                    "FontCharInfo[Bitmap(W={0},H={1},L={2},T={3}), " +
                    "Metrics(W={4}, H={5}, Ascender={6}, Descender={7}, BearingH={8}x{9}, BearingV={10}x{11}, Advance={12}x{13})",
                    BitmapWidth, BitmapHeight, BitmapLeft, BitmapTop,
                    Width, Height, Ascender, Descender, BearingHX, BearingHY, BearingVX, BearingVY, AdvanceH, AdvanceV
                );
            }
        }

        public enum FontPixelFormat : uint
        {
            /// <summary>
            /// 2 pixels packed in 1 byte (natural order)
            /// </summary>
            PSP_FONT_PIXELFORMAT_4 = 0,

            /// <summary>
            /// 2 pixels packed in 1 byte (reversed order)
            /// </summary>
            PSP_FONT_PIXELFORMAT_4_REV = 1,

            /// <summary>
            /// 1 pixel in 1 byte
            /// </summary>
            PSP_FONT_PIXELFORMAT_8 = 2,

            /// <summary>
            /// 1 pixel in 3 bytes (RGB)
            /// </summary>
            PSP_FONT_PIXELFORMAT_24 = 3,

            /// <summary>
            /// 1 pixel in 4 bytes (RGBA)
            /// </summary>
            PSP_FONT_PIXELFORMAT_32 = 4,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GlyphImage
        {
            /// <summary>
            /// 
            /// </summary>
            public FontPixelFormat PixelFormat;

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

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
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
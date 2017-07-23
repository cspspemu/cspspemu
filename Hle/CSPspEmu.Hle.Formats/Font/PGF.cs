using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats.Font
{
    public interface IPgf
    {
        IGlyph GetGlyph(char character, char alternativeCharacter = '?');
        FontInfo GetFontInfo();
        Size GetAdvance(uint index);
    }

    public class NativeFontIpgf : IPgf
    {
        public IGlyph GetGlyph(char character, char alternativeCharacter = '?') => throw new NotImplementedException();

        public FontInfo GetFontInfo()
        {
            return new FontInfo
            {
                MaxGlyphWidth = 0,
                MaxGlyphHeight = 0,

                MaxGlyphAscender = 0,
                MaxGlyphDescender = 0,
                MaxGlyphLeftX = 0,
                MaxGlyphBaseY = 0,
                MinGlyphCenterX = 0,
                MaxGlyphTopY = 0,
                MaxGlyphAdvanceX = 0,
                MaxGlyphAdvanceY = 0,

                FontStyle = new FontStyle
                {
                    Attributes = 0,
                    Country = 0,
                    Expire = 0,
                    Family = FamilyEnum.FontFamilySerif,
                    FileName = "test.pgf",
                    Name = "Arial",
                    Language = LanguageEnum.FontLanguageJapanese,
                    Region = 0,
                    Resolution = new HorizontalVerticalFloat(32, 32),
                    Size = new HorizontalVerticalFloat(32, 32),
                    StyleStyle = StyleEnum.FontStyleRegular,
                    StyleSub = 0,
                    Weight = 0,
                },
                BPP = 4,
            };
        }

        public Size GetAdvance(uint index) => new Size(16, 16);
    }

    public partial class Pgf : IPgf
    {
        protected IGlyph[] Glyphs;

        protected IGlyph _GetGlyph(int index) => Glyphs[index] ?? (Glyphs[index] = new Glyph(this, index));

        public Size GetAdvance(uint index) => new Size(AdvanceTable[index].Src, AdvanceTable[index].Dst);

        public IGlyph GetGlyph(char character, char alternativeCharacter = '?')
        {
            if (character >= 0 && character < CharMap.Length) return _GetGlyph(CharMap[character]);
            return _GetGlyph(CharMap[alternativeCharacter]);
        }

        public FontInfo GetFontInfo()
        {
            return new FontInfo
            {
                MaxGlyphWidth = Header.MaxGlyphWidth,
                MaxGlyphHeight = Header.MaxGlyphHeight,

                MaxGlyphAscender = Header.MaxBaseYAdjust,
                MaxGlyphDescender = Header.MaxBaseYAdjust - Header.MaxGlyphHeight,
                MaxGlyphLeftX = Header.MaxLeftXAdjust,
                MaxGlyphBaseY = Header.MaxBaseYAdjust,
                MinGlyphCenterX = Header.MinCenterXAdjust,
                MaxGlyphTopY = Header.MaxTopYAdjust,
                MaxGlyphAdvanceX = Header.MaxAdvance.X,
                MaxGlyphAdvanceY = Header.MaxAdvance.Y,

                FontStyle = FontStyle,
                BPP = 4,
            };
        }

        public HeaderStruct Header;
        public HeaderRevision3Struct HeaderExtraRevision3;

        public PointFixed266[] DimensionTable;
        public MapInt[] AdvanceTable;
        public MapInt[] XAdjustTable;
        public MapInt[] YAdjustTable;
        public byte[] PackedShadowCharMap;

        public MapUshort[] CharmapCompressionTable1;
        public MapUshort[] CharmapCompressionTable2;

        public byte[] PackedCharMap;
        public byte[] PackedCharPointerTable;

        public int[] CharMap;
        public Dictionary<int, int> ReverseCharMap;
        public int[] CharPointer;

        public Dictionary<int, int> ShadowCharMap;
        public Dictionary<int, int> ReverseShadowCharMap;

        public byte[] CharData;

        public int GetGlyphId(char Char)
        {
            if (Char < Header.FirstGlyph) return -1;
            if (Char > Header.LastGlyph) return -1;
            int glyphPos = Char - Header.FirstGlyph;
            //Console.WriteLine("Offset: {0}, Size: {1}", glyphPos * header.charMapBpe, header.charMapBpe);
            return (int) BitReader.ReadBitsAt(PackedCharMap, glyphPos * Header.TableCharMapBpe, Header.TableCharMapBpe);
        }

        protected static int BitsToBytesHighAligned(int bits) => ((bits + 31) & ~31) / 8;

        public Pgf Load(string fileName) => Load(new FileStream(fileName, FileMode.Open, FileAccess.Read));

        public Pgf Load(Stream fileStream)
        {
            Header = fileStream.ReadStruct<HeaderStruct>();

            if (Header.Revision >= 3)
            {
                HeaderExtraRevision3 = fileStream.ReadStruct<HeaderRevision3Struct>();
            }

            fileStream.ReadStructVector(out DimensionTable, Header.TableDimLength);
            fileStream.ReadStructVector(out XAdjustTable, Header.TableXAdjustLength);
            fileStream.ReadStructVector(out YAdjustTable, Header.TableYAdjustLength);
            fileStream.ReadStructVector(out AdvanceTable, Header.TableAdvanceLength);

            PackedShadowCharMap =
                fileStream.ReadBytes(BitsToBytesHighAligned(Header.TableShadowMapLength * Header.TableShadowMapBpe));

            if (Header.Revision == 3)
            {
                fileStream.ReadStructVector(out CharmapCompressionTable1, HeaderExtraRevision3.TableCompCharMapLength1);
                fileStream.ReadStructVector(out CharmapCompressionTable2, HeaderExtraRevision3.TableCompCharMapLength2);
            }

            PackedCharMap =
                fileStream.ReadBytes(BitsToBytesHighAligned(Header.TableCharMapLength * Header.TableCharMapBpe));
            PackedCharPointerTable =
                fileStream.ReadBytes(
                    BitsToBytesHighAligned(Header.TableCharPointerLength * Header.TableCharPointerBpe));

            /*
            int BytesLeft = (int)(FileStream.Length - FileStream.Position);
            charData = new byte[BytesLeft];
            FileStream.Read(charData, 0, BytesLeft);
            */

            CharData = fileStream.ReadBytes((int) (fileStream.Length - fileStream.Position));

            var numberOfCharacters = Header.TableCharPointerLength;

            CharMap = new int[Header.FirstGlyph + Header.LastGlyph + 1];
            CharPointer = new int[numberOfCharacters];
            Glyphs = new IGlyph[numberOfCharacters];
            ReverseCharMap = new Dictionary<int, int>();
            ShadowCharMap = new Dictionary<int, int>();
            ReverseShadowCharMap = new Dictionary<int, int>();

            foreach (var pair in BitReader.FixedBitReader(PackedShadowCharMap, Header.TableShadowMapBpe))
            {
                var unicodeIndex = (int) pair.Key + Header.FirstGlyph;
                var glyphIndex = (int) pair.Value;
                ShadowCharMap[unicodeIndex] = glyphIndex;
                ReverseShadowCharMap[glyphIndex] = unicodeIndex;
            }

            foreach (var pair in BitReader.FixedBitReader(PackedCharMap, Header.TableCharMapBpe))
            {
                var unicodeIndex = (int) pair.Key + Header.FirstGlyph;
                var glyphIndex = (int) pair.Value;
                CharMap[unicodeIndex] = glyphIndex;
                ReverseCharMap[glyphIndex] = unicodeIndex;
            }

            foreach (var pair in BitReader.FixedBitReader(PackedCharPointerTable, Header.TableCharPointerBpe))
            {
                CharPointer[pair.Key] = (int) pair.Value;
            }

            /*
            for (int n = 0; n < NumberOfCharacters; n++)
            {
                Glyphs[n] = new Glyph().Read(this, n);
            }
            */

            Console.WriteLine(Header.FontName);

            /*
            Console.WriteLine(this.header.fontName);
            for (int n = 0; n < 300; n++)
            {
                Console.WriteLine(GetGlyphId((char)n));
            }
            */

            return this;
        }

        public void Write(string fileName)
        {
            var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            fileStream.WriteStruct(Header);

            if (Header.Revision >= 3)
            {
                fileStream.WriteStruct(HeaderExtraRevision3);
            }

            fileStream.WriteStructVector(DimensionTable);
            fileStream.WriteStructVector(XAdjustTable);
            fileStream.WriteStructVector(YAdjustTable);
            fileStream.WriteStructVector(AdvanceTable);
            fileStream.WriteStructVector(PackedShadowCharMap);

            if (Header.Revision == 3)
            {
                fileStream.WriteStructVector(CharmapCompressionTable1);
                fileStream.WriteStructVector(CharmapCompressionTable2);
            }

            fileStream.WriteStructVector(PackedCharMap);
            fileStream.WriteStructVector(PackedCharPointerTable);

            fileStream.WriteBytes(CharData);
        }

        public FontStyle FontStyle => new FontStyle
        {
            Size = new HorizontalVerticalFloat(Header.Size.X, Header.Size.Y),
            Resolution = new HorizontalVerticalFloat(Header.Resolution.X, Header.Resolution.Y),
            Weight = 1.0f,
            Family = FamilyEnum.FontFamilySansSerif,
            StyleStyle = StyleEnum.FontStyleRegular,
            StyleSub = 0,
            Language = LanguageEnum.FontLanguageLatin,
            Region = 0,
            Country = 0,
            FileName = "dummy.pgf",
            Name = Header.FontName,
            Attributes = 0,
            Expire = 0,
        };
    }

    public interface IGlyph
    {
        IPgf Pgf { get; }
        int GlyphIndex { get; }
        GlyphSymbol Face { get; }
        GlyphSymbol Shadow { get; }
    }

    public class Glyph : IGlyph
    {
        protected Pgf Pgf;
        protected int GlyphIndex;
        public GlyphSymbol Pface;
        public GlyphSymbol Pshadow;

        public Glyph(Pgf pgf, int glyphIndex)
        {
            Pgf = pgf;
            GlyphIndex = glyphIndex;
        }

        public GlyphSymbol Face
        {
            get
            {
                if (Pface != null) return Pface;
                Pface = new GlyphSymbol();
                Pface.Read(Pgf, GlyphIndex);
                return Pface;
            }
        }

        public GlyphSymbol Shadow
        {
            get
            {
                if (Pshadow != null) return Pshadow;
                Pshadow = new GlyphSymbol(GlyphSymbol.GlyphFlags.FontPgfShadowglyph);
                Pshadow.Read(Pgf, GlyphIndex);
                return Pshadow;
            }
        }


        IPgf IGlyph.Pgf => Pgf;

        int IGlyph.GlyphIndex => GlyphIndex;

        GlyphSymbol IGlyph.Face => Face;

        GlyphSymbol IGlyph.Shadow => Shadow;
    }

    public class GlyphSymbol
    {
        [Flags]
        public enum GlyphFlags
        {
            FontPgfBmpHRows = 0x01,
            FontPgfBmpVRows = 0x02,
            FontPgfBmpOverlay = 0x03,
            FontPgfMetricFlag1 = 0x04,
            FontPgfMetricFlag2 = 0x08,
            FontPgfMetricFlag3 = 0x10,
            FontPgfCharglyph = 0x20,
            FontPgfShadowglyph = 0x40,
        }

        public char UnicodeChar;
        public int GlyphIndex;
        public uint Width;
        public uint Height;
        public int Left;
        public int Top;
        public uint DataByteOffset;
        public uint AdvanceIndex;
        public GlyphFlags Flags;
        public byte[] Data;
        readonly GlyphFlags _glyphType;

        public GlyphSymbol(GlyphFlags glyphType = GlyphFlags.FontPgfCharglyph) => _glyphType = glyphType;

        public override string ToString()
        {
            return
                $"PGF.Glyph(GlyphIndex={GlyphIndex}, Char='{UnicodeChar}', Width={Width}, Height={Height}, Left={Left}, Top={Top}, Flags={Flags})";
        }

        public GlyphSymbol Read(Pgf pgf, int glyphIndex)
        {
            var bitReader = new BitReader(pgf.CharData)
            {
                Position = pgf.CharPointer[glyphIndex] * 4 * 8
            };

            GlyphIndex = glyphIndex;
            UnicodeChar = (char) pgf.ReverseCharMap[glyphIndex];

            //int NextOffset = br.Position;

            //br.Position = NextOffset;
            var shadowOffset = bitReader.Position + (int) bitReader.ReadBits(14) * 8;
            if (_glyphType == GlyphFlags.FontPgfShadowglyph)
            {
                bitReader.Position = shadowOffset;
                bitReader.SkipBits(14);
            }

            Width = bitReader.ReadBits(7);
            Height = bitReader.ReadBits(7);
            Left = bitReader.ReadBitsSigned(7);
            Top = bitReader.ReadBitsSigned(7);
            Flags = (GlyphFlags) bitReader.ReadBits(6);

            if (Flags.HasFlag(GlyphFlags.FontPgfCharglyph))
            {
                bitReader.SkipBits(7);
                // ReSharper disable once UnusedVariable
                var shadowId = bitReader.ReadBits(9);
                bitReader.SkipBits(24);
                if (!Flags.HasFlag(GlyphFlags.FontPgfMetricFlag1)) bitReader.SkipBits(56);
                if (!Flags.HasFlag(GlyphFlags.FontPgfMetricFlag2)) bitReader.SkipBits(56);
                if (!Flags.HasFlag(GlyphFlags.FontPgfMetricFlag3)) bitReader.SkipBits(56);
                AdvanceIndex = bitReader.ReadBits(8);
            }

            DataByteOffset = (uint) (bitReader.Position / 8);

            uint pixelIndex = 0;
            var numberOfPixels = Width * Height;
            var bitmapHorizontalRows = (Flags & GlyphFlags.FontPgfBmpOverlay) == GlyphFlags.FontPgfBmpHRows;
            Data = new byte[numberOfPixels];
            uint value = 0;

            //Console.WriteLine(br.BitsLeft);

            while (pixelIndex < numberOfPixels)
            {
                var code = bitReader.ReadBits(4);

                int count;
                if (code < 8)
                {
                    value = bitReader.ReadBits(4);
                    count = (int) code + 1;
                }
                else
                {
                    count = 16 - (int) code;
                }

                for (var n = 0; n < count && pixelIndex < numberOfPixels; n++)
                {
                    if (code >= 8)
                    {
                        value = bitReader.ReadBits(4);
                    }

                    uint x;
                    uint y;
                    if (bitmapHorizontalRows)
                    {
                        x = pixelIndex % Width;
                        y = pixelIndex / Width;
                    }
                    else
                    {
                        x = pixelIndex / Height;
                        y = pixelIndex % Height;
                    }

                    Data[x + y * Width] = (byte) ((value << 0) | (value << 4));
                    pixelIndex++;
                }
            }

            /*
            for (int y = 0, n = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++, n++)
                {
                    Console.Write("{0:X1}", this.Data[n] & 0xF);
                    //String.Format
                }
                Console.WriteLine("");
            }

            */
            //Console.WriteLine(this);

            return this;
        }

        public Bitmap GetBitmap()
        {
            if (Width == 0 || Height == 0) return new Bitmap(1, 1);
            var bitmap = new Bitmap((int) Width, (int) Height);
            for (int y = 0, n = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++, n++)
                {
                    var c = Data[n];
                    //Bitmap.SetPixel(x, y, Color.FromArgb(Data[n], 0xFF, 0xFF, 0xFF));
                    bitmap.SetPixel(x, y, Color.FromArgb(0xFF, c, c, c));
                }
            }
            return bitmap;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FontInfo
    {
        private Fixed266 MaxGlyphWidthI;
        private Fixed266 MaxGlyphHeightI;
        private Fixed266 MaxGlyphAscenderI;
        private Fixed266 MaxGlyphDescenderI;
        private Fixed266 MaxGlyphLeftXI;
        private Fixed266 MaxGlyphBaseYI;
        private Fixed266 MinGlyphCenterXI;
        private Fixed266 MaxGlyphTopYI;
        private Fixed266 MaxGlyphAdvanceXI;
        private Fixed266 MaxGlyphAdvanceYI;

        private float MaxGlyphWidthF;
        private float MaxGlyphHeightF;
        private float MaxGlyphAscenderF;
        private float MaxGlyphDescenderF;
        private float MaxGlyphLeftXF;
        private float MaxGlyphBaseYF;
        private float MinGlyphCenterXF;
        private float MaxGlyphTopYF;
        private float MaxGlyphAdvanceXF;
        private float MaxGlyphAdvanceYF;

        public float MaxGlyphAscender
        {
            set => MaxGlyphAscenderI = MaxGlyphAscenderF = value;
            get => MaxGlyphAscenderF;
        }

        public float MaxGlyphDescender
        {
            set => MaxGlyphDescenderI = MaxGlyphDescenderF = value;
            get => MaxGlyphDescenderF;
        }

        public float MaxGlyphLeftX
        {
            set => MaxGlyphLeftXI = MaxGlyphLeftXF = value;
            get => MaxGlyphLeftXF;
        }

        public float MaxGlyphBaseY
        {
            set => MaxGlyphBaseYI = MaxGlyphBaseYF = value;
            get => MaxGlyphBaseYF;
        }

        public float MinGlyphCenterX
        {
            set => MinGlyphCenterXI = MinGlyphCenterXF = value;
            get => MinGlyphCenterXF;
        }

        public float MaxGlyphTopY
        {
            set => MaxGlyphTopYI = MaxGlyphTopYF = value;
            get => MaxGlyphTopYF;
        }

        public float MaxGlyphAdvanceX
        {
            set => MaxGlyphAdvanceXI = MaxGlyphAdvanceXF = value;
            get => MaxGlyphAdvanceXF;
        }

        public float MaxGlyphAdvanceY
        {
            set => MaxGlyphAdvanceYI = MaxGlyphAdvanceYF = value;
            get => MaxGlyphAdvanceYF;
        }

        public ushort MaxGlyphWidth
        {
            set => MaxGlyphWidthI = MaxGlyphWidthF = _MaxGlyphWidth = value;
            get => _MaxGlyphWidth;
        }

        public ushort MaxGlyphHeight
        {
            set => MaxGlyphHeightI = MaxGlyphHeightF = _MaxGlyphHeight = value;
            get => _MaxGlyphHeight;
        }

        #region Bitmap dimensions.

        /// <summary>
        /// 
        /// </summary>
        private ushort _MaxGlyphWidth;

        /// <summary>
        /// 
        /// </summary>
        private ushort _MaxGlyphHeight;

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
}
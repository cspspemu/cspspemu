using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace CSPspEmu.Hle.Formats.Font
{
    public partial class PGF
	{
		public class Glyph
		{
			protected PGF PGF;
			protected int GlyphIndex;
			protected GlyphSymbol _Face;
			protected GlyphSymbol _Shadow;

			public Glyph(PGF PGF, int GlyphIndex)
			{
				this.PGF = PGF;
				this.GlyphIndex = GlyphIndex;
			}

			public GlyphSymbol Face {
				get
				{
					if (_Face == null)
					{
						_Face = new GlyphSymbol(GlyphSymbol.GlyphFlags.FONT_PGF_CHARGLYPH);
						_Face.Read(PGF, GlyphIndex);
					}
					return _Face;
				}
			}

			public GlyphSymbol Shadow
			{
				get
				{
					if (_Shadow == null)
					{
						_Shadow = new GlyphSymbol(GlyphSymbol.GlyphFlags.FONT_PGF_SHADOWGLYPH);
						_Shadow.Read(PGF, GlyphIndex);
					}
					return _Shadow;
				}

			}

		}

		public class GlyphSymbol
		{
			public enum GlyphFlags : int
			{
				FONT_PGF_BMP_H_ROWS = 0x01,
				FONT_PGF_BMP_V_ROWS = 0x02,
				FONT_PGF_BMP_OVERLAY = 0x03,
				FONT_PGF_METRIC_FLAG1 = 0x04,
				FONT_PGF_METRIC_FLAG2 = 0x08,
				FONT_PGF_METRIC_FLAG3 = 0x10,
				FONT_PGF_CHARGLYPH = 0x20,
				FONT_PGF_SHADOWGLYPH = 0x40,
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
			GlyphFlags GlyphType;

			public GlyphSymbol(GlyphFlags GlyphType = GlyphFlags.FONT_PGF_CHARGLYPH)
			{
				this.GlyphType = GlyphType;
			}

			public override string ToString()
			{
				return String.Format(
					"PGF.Glyph(GlyphIndex={0}, Char='{1}', Width={2}, Height={3}, Left={4}, Top={5}, Flags={6})",
					GlyphIndex, UnicodeChar, Width, Height, Left, Top, Flags
				);
			}

			public GlyphSymbol Read(PGF PGF, int GlyphIndex)
			{
				var BitReader = new BitReader(PGF.CharData);
				BitReader.Position = PGF.CharPointer[GlyphIndex] * 4 * 8;

				this.GlyphIndex = GlyphIndex;
				this.UnicodeChar = (char)PGF.ReverseCharMap[GlyphIndex];

				//int NextOffset = br.Position;

				//br.Position = NextOffset;
				int ShadowOffset = (int)BitReader.Position + (int)BitReader.ReadBits(14) * 8;
				if (GlyphType == GlyphFlags.FONT_PGF_SHADOWGLYPH)
				{
					BitReader.Position = ShadowOffset;
					BitReader.SkipBits(14);
				}

				this.Width = BitReader.ReadBits(7);
				this.Height = BitReader.ReadBits(7);
				this.Left = BitReader.ReadBitsSigned(7);
				this.Top = BitReader.ReadBitsSigned(7);
				this.Flags = (GlyphFlags)BitReader.ReadBits(6);

				if (Flags.HasFlag(GlyphFlags.FONT_PGF_CHARGLYPH))
				{
					BitReader.SkipBits(7);
					var shadowId = BitReader.ReadBits(9);
					BitReader.SkipBits(24);
					if (!Flags.HasFlag(GlyphFlags.FONT_PGF_METRIC_FLAG1)) BitReader.SkipBits(56);
					if (!Flags.HasFlag(GlyphFlags.FONT_PGF_METRIC_FLAG2)) BitReader.SkipBits(56);
					if (!Flags.HasFlag(GlyphFlags.FONT_PGF_METRIC_FLAG3)) BitReader.SkipBits(56);
					this.AdvanceIndex = BitReader.ReadBits(8);
				}

				this.DataByteOffset = (uint)(BitReader.Position / 8);

				uint PixelIndex = 0;
				uint NumberOfPixels = Width * Height;
				bool BitmapHorizontalRows = (Flags & GlyphFlags.FONT_PGF_BMP_OVERLAY) == GlyphFlags.FONT_PGF_BMP_H_ROWS;
				this.Data = new byte[NumberOfPixels];
				int Count;
				uint Value = 0;
				uint x, y;

				//Console.WriteLine(br.BitsLeft);

				while (PixelIndex < NumberOfPixels)
				{
					uint Code = BitReader.ReadBits(4);

					if (Code < 8)
					{
						Value = BitReader.ReadBits(4);
						Count = (int)Code + 1;
					}
					else
					{
						Count = 16 - (int)Code;
					}

					for (int n = 0; (n < Count) && (PixelIndex < NumberOfPixels); n++)
					{
						if (Code >= 8)
						{
							Value = BitReader.ReadBits(4);
						}

						if (BitmapHorizontalRows)
						{
							x = PixelIndex % Width;
							y = PixelIndex / Width;
						}
						else
						{
							x = PixelIndex / Height;
							y = PixelIndex % Height;
						}

						this.Data[x + y * Width] = (byte)((Value << 0) | (Value << 4));
						PixelIndex++;
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
				Bitmap Bitmap = new Bitmap((int)Width, (int)Height);
				for (int y = 0, n = 0; y < Height; y++)
				{
					for (int x = 0; x < Width; x++, n++)
					{
						byte c = Data[n];
						//Bitmap.SetPixel(x, y, Color.FromArgb(Data[n], 0xFF, 0xFF, 0xFF));
						Bitmap.SetPixel(x, y, Color.FromArgb(0xFF, c, c, c));
					}
				}
				return Bitmap;
			}
		}

		protected Glyph[] Glyphs;

		protected Glyph _GetGlyph(int Index)
		{
			if (Glyphs[Index] == null) {
				Glyphs[Index] = new Glyph(this, Index);
			}
			return Glyphs[Index];
		}

		public Glyph GetGlyph(char character, char alternativeCharacter = '?')
		{
			if (character >= 0 && character < CharMap.Length)
			{
				return _GetGlyph(CharMap[character]);
			}
			return _GetGlyph(CharMap[alternativeCharacter]);
		}

		public HeaderStruct Header;
		public HeaderRevision3Struct HeaderExtraRevision3;

		public PointFixed26_6[] DimensionTable;
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

		public PGF()
		{
		}

		public int GetGlyphId(char Char)
		{
			if (Char < Header.FirstGlyph) return -1;
			if (Char > Header.LastGlyph) return -1;
			int glyphPos = (Char - Header.FirstGlyph);
			//Console.WriteLine("Offset: {0}, Size: {1}", glyphPos * header.charMapBpe, header.charMapBpe);
			return (int)BitReader.ReadBitsAt(PackedCharMap, glyphPos * Header.TableCharMapBpe, Header.TableCharMapBpe);
		}

		protected static int BitsToBytesHighAligned(int Bits)
		{
			//return MathUtils.NextHigherAligned(Bits, 8) / 8;
			return ((Bits + 31) & ~31) / 8;
		}

		public PGF Load(string FileName)
		{
			var FileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
			return Load(FileStream);
		}

		public PGF Load(Stream FileStream)
		{
			this.Header = FileStream.ReadStruct<HeaderStruct>();

			if (this.Header.Revision >= 3)
			{
				this.HeaderExtraRevision3 = FileStream.ReadStruct<HeaderRevision3Struct>();
			}

			FileStream.ReadStructVector(ref DimensionTable, Header.TableDimLength);
			FileStream.ReadStructVector(ref XAdjustTable, Header.TableXAdjustLength);
			FileStream.ReadStructVector(ref YAdjustTable, Header.TableYAdjustLength);
			FileStream.ReadStructVector(ref AdvanceTable, Header.TableAdvanceLength);

			PackedShadowCharMap = FileStream.ReadBytes(BitsToBytesHighAligned(Header.TableShadowMapLength * Header.TableShadowMapBpe));

			if (Header.Revision == 3)
			{
				FileStream.ReadStructVector(ref CharmapCompressionTable1, HeaderExtraRevision3.TableCompCharMapLength1);
				FileStream.ReadStructVector(ref CharmapCompressionTable2, HeaderExtraRevision3.TableCompCharMapLength2);
			}

			PackedCharMap = FileStream.ReadBytes(BitsToBytesHighAligned(Header.TableCharMapLength * Header.TableCharMapBpe));
			PackedCharPointerTable = FileStream.ReadBytes(BitsToBytesHighAligned(Header.TableCharPointerLength * Header.TableCharPointerBpe));

			/*
			int BytesLeft = (int)(FileStream.Length - FileStream.Position);
			charData = new byte[BytesLeft];
			FileStream.Read(charData, 0, BytesLeft);
			*/

			CharData = FileStream.ReadBytes((int)(FileStream.Length - FileStream.Position));

			var NumberOfCharacters = Header.TableCharPointerLength;

			CharMap = new int[Header.FirstGlyph + Header.LastGlyph + 1];
			CharPointer = new int[NumberOfCharacters];
			Glyphs = new Glyph[NumberOfCharacters];
			ReverseCharMap = new Dictionary<int, int>();
			ShadowCharMap = new Dictionary<int, int>();
			ReverseShadowCharMap = new Dictionary<int, int>();

			foreach (var Pair in BitReader.FixedBitReader(PackedShadowCharMap, Header.TableShadowMapBpe))
			{
				var UnicodeIndex = (int)Pair.Key + Header.FirstGlyph;
				var GlyphIndex = (int)Pair.Value;
				ShadowCharMap[UnicodeIndex] = GlyphIndex;
				ReverseShadowCharMap[GlyphIndex] = UnicodeIndex;
			}

			foreach (var Pair in BitReader.FixedBitReader(PackedCharMap, Header.TableCharMapBpe))
			{
				var UnicodeIndex = (int)Pair.Key + Header.FirstGlyph;
				var GlyphIndex = (int)Pair.Value;
				CharMap[UnicodeIndex] = GlyphIndex;
				ReverseCharMap[GlyphIndex] = UnicodeIndex;
			}

			foreach (var Pair in BitReader.FixedBitReader(PackedCharPointerTable, Header.TableCharPointerBpe))
			{
				CharPointer[Pair.Key] = (int)Pair.Value;
			}

			/*
			for (int n = 0; n < NumberOfCharacters; n++)
			{
				Glyphs[n] = new Glyph().Read(this, n);
			}
			*/

			Console.WriteLine(this.Header.FontName);

			/*
			Console.WriteLine(this.header.fontName);
			for (int n = 0; n < 300; n++)
			{
				Console.WriteLine(GetGlyphId((char)n));
			}
			*/

			return this;
		}

		public void Write(string FileName)
		{
			var FileStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write);
			FileStream.WriteStruct(this.Header);

			if (this.Header.Revision >= 3)
			{
				FileStream.WriteStruct(this.HeaderExtraRevision3);
			}

			FileStream.WriteStructVector(DimensionTable);
			FileStream.WriteStructVector(XAdjustTable);
			FileStream.WriteStructVector(YAdjustTable);
			FileStream.WriteStructVector(AdvanceTable);
			FileStream.WriteStructVector(PackedShadowCharMap);

			if (Header.Revision == 3)
			{
				FileStream.WriteStructVector(CharmapCompressionTable1);
				FileStream.WriteStructVector(CharmapCompressionTable2);
			}

			FileStream.WriteStructVector(PackedCharMap);
			FileStream.WriteStructVector(PackedCharPointerTable);

			FileStream.WriteBytes(CharData);
		}

		public FontStyle FontStyle
		{
			get
			{
				return new FontStyle()
				{
					Size = new HorizontalVerticalFloat(Header.Size.X, Header.Size.Y),
					Resolution = new HorizontalVerticalFloat(Header.Resolution.X, Header.Resolution.Y),
					Weight = 1.0f,
					Family = FamilyEnum.FONT_FAMILY_SANS_SERIF,
					StyleStyle = StyleEnum.FONT_STYLE_REGULAR,
					StyleSub = 0,
					Language = LanguageEnum.FONT_LANGUAGE_LATIN,
					Region = 0,
					Country = 0,
					FileName = "dummy.pgf",
					Name = Header.FontName,
					Attributes = 0,
					Expire = 0,
				};
			}
		}
	}
}

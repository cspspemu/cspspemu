using System;
using System.Collections.Generic;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Formats.Font;

namespace CSPspEmu.Hle.Modules.libfont
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010000)]
    public unsafe partial class sceLibFont : HleModuleHost
    {
        public const float PointDPI = 72.0f;

        //public enum FontLibraryHandle : int { }
        public class FontLibrary : IHleUidPoolClass, IDisposable
        {
            public FontNewLibParams Params;
            public float HorizontalResolution = 128.0f;
            public float VerticalResolution = 128.0f;
            public ushort AlternateCharCode;

            public List<FontRegistryEntry> FontRegistryList;

            public FontLibrary()
            {
                FontRegistryList = new List<FontRegistryEntry>()
                {
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_DB, 0, LanguageEnum.FONT_LANGUAGE_JAPANESE, 0, 1, "jpn0.pgf",
                        "FTT-NewRodin Pro DB", 0, 0),
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_REGULAR, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn0.pgf",
                        "FTT-NewRodin Pro Latin", 0, 0),
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SERIF,
                        StyleEnum.FONT_STYLE_REGULAR, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn1.pgf",
                        "FTT-Matisse Pro Latin", 0, 0),
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_ITALIC, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn2.pgf",
                        "FTT-NewRodin Pro Latin", 0, 0),
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SERIF,
                        StyleEnum.FONT_STYLE_ITALIC, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn3.pgf",
                        "FTT-Matisse Pro Latin", 0, 0),
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_BOLD, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn4.pgf",
                        "FTT-NewRodin Pro Latin", 0, 0),
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SERIF,
                        StyleEnum.FONT_STYLE_BOLD, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn5.pgf",
                        "FTT-Matisse Pro Latin", 0, 0),
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_BOLD_ITALIC, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn6.pgf",
                        "FTT-NewRodin Pro Latin", 0, 0),
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SERIF,
                        StyleEnum.FONT_STYLE_BOLD_ITALIC, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn7.pgf",
                        "FTT-Matisse Pro Latin", 0, 0),
                    new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_REGULAR, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn8.pgf",
                        "FTT-NewRodin Pro Latin", 0, 0),
                    new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SERIF,
                        StyleEnum.FONT_STYLE_REGULAR, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn9.pgf",
                        "FTT-Matisse Pro Latin", 0, 0),
                    new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_ITALIC, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn10.pgf",
                        "FTT-NewRodin Pro Latin", 0, 0),
                    new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SERIF,
                        StyleEnum.FONT_STYLE_ITALIC, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn11.pgf",
                        "FTT-Matisse Pro Latin", 0, 0),
                    new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_BOLD, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn12.pgf",
                        "FTT-NewRodin Pro Latin", 0, 0),
                    new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SERIF,
                        StyleEnum.FONT_STYLE_BOLD, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn13.pgf",
                        "FTT-Matisse Pro Latin", 0, 0),
                    new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_BOLD_ITALIC, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn14.pgf",
                        "FTT-NewRodin Pro Latin", 0, 0),
                    new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SERIF,
                        StyleEnum.FONT_STYLE_BOLD_ITALIC, 0, LanguageEnum.FONT_LANGUAGE_LATIN, 0, 1, "ltn15.pgf",
                        "FTT-Matisse Pro Latin", 0, 0),
                    new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FamilyEnum.FONT_FAMILY_SANS_SERIF,
                        StyleEnum.FONT_STYLE_REGULAR, 0, LanguageEnum.FONT_LANGUAGE_KOREAN, 0, 3, "kr0.pgf",
                        "AsiaNHH(512Johab)", 0, 0),
                };
            }

            public void Dispose()
            {
            }
        }

        //public enum FontHandle : int { }
        [HleUidPoolClass(NotFoundError = (SceKernelErrors) (-1))]
        public class Font : IHleUidPoolClass, IDisposable
        {
            public FontLibrary FontLibrary;

            public IPGF PGF;
            //public FontInfo FontInfo;

            public Font(FontLibrary FontLibrary, IPGF PGF)
            {
                this.FontLibrary = FontLibrary;
                this.PGF = PGF;
            }

            public void Dispose()
            {
            }

            public IGlyph GetGlyph(ushort CharCode)
            {
                return PGF.GetGlyph((char) CharCode, (char) FontLibrary.AlternateCharCode);
            }

            public FontCharInfo GetCharInfo(ushort CharCode)
            {
                var Glyph = GetGlyph(CharCode);
                var Face = Glyph.Face;

                var Advance = PGF.GetAdvance(Face.AdvanceIndex);

                var FontCharInfo = new sceLibFont.FontCharInfo()
                {
                    BitmapWidth = Face.Width,
                    BitmapHeight = Face.Height,
                    BitmapLeft = Face.Left,
                    BitmapTop = Face.Top,
                    Width = Face.Width,
                    Height = Face.Height,
                    Ascender = Face.Top,
                    Descender = Face.Height - Face.Top,
                    BearingHX = Face.Left,
                    BearingHY = Face.Top,
                    BearingVX = Face.Left,
                    BearingVY = Face.Top,
                    AdvanceH = Advance.Width,
                    AdvanceV = Advance.Height,
                    Unknown = 0,
                };

                return FontCharInfo;
            }

            internal FontInfo GetFontInfo()
            {
                return PGF.GetFontInfo();
            }
        }

        /// <summary>
        /// Creates a new font library.
        /// </summary>
        /// <param name="Params">Parameters of the new library.</param>
        /// <param name="ErrorCode">Pointer to store any error code.</param>
        /// <returns>FontLibraryHandle</returns>
        [HlePspFunction(NID = 0x67F17ED7, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public FontLibrary sceFontNewLib(FontNewLibParams* Params, uint* ErrorCode)
        {
            //if (Params != null) throw (new NotImplementedException("(Params != null)"));

            var FontLibrary = new FontLibrary()
            {
                Params = *Params,
            };

            *ErrorCode = 0;

            return FontLibrary;
        }

        /// <summary>
        /// Releases the font library.
        /// </summary>
        /// <param name="FontLibrary">Handle of the library.</param>
        /// <returns>
        ///		0 - success
        /// </returns>
        [HlePspFunction(NID = 0x574B6FBC, FirmwareVersion = 150)]
        public int sceFontDoneLib(FontLibrary FontLibrary)
        {
            FontLibrary.RemoveUid(InjectContext);
            return 0;
        }
    }
}
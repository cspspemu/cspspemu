using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Hle.Formats.Font;

namespace CSPspEmu.Hle.Modules.libfont
{
	unsafe public partial class sceLibFont
	{

		/// <summary>
		/// Opens a new font.
		/// </summary>
		/// <param name="FontLibraryHandle">Handle of the library.</param>
		/// <param name="Index">Index of the font.</param>
		/// <param name="Mode">Mode for opening the font.</param>
		/// <param name="ErrorCode">Pointer to store any error code.</param>
		/// <returns>
		///		FontHandle
		/// </returns>
		[HlePspFunction(NID = 0xA834319D, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public FontHandle sceFontOpen(FontLibraryHandle FontLibraryHandle, int Index, int Mode, uint* ErrorCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);

			var FontRegistry = FontLibrary.FontRegistryList[Index];
			var FontFileStream = HleState.HleIoManager.HleIoWrapper.Open("flash0:/font/" + FontRegistry.FontStyle.FileName, Vfs.HleIoFlags.Read, Vfs.SceMode.All);
			var PGF = new PGF().Load(FontFileStream);
			var Font = new Font(FontLibrary, PGF);
			*ErrorCode = 0;
			return Fonts.Create(Font);
		}

		/// <summary>
		/// Opens a new font from memory.
		/// </summary>
		/// <param name="FontLibraryHandle">Handle of the library.</param>
		/// <param name="MemoryFontAddress">Pointer to the font.</param>
		/// <param name="MemoryFontLength">Mode for opening the font.</param>
		/// <param name="ErrorCode">Pointer to store any error code.</param>
		/// <returns>
		///		FontHandle
		/// </returns>
		[HlePspFunction(NID = 0xBB8E7FE6, FirmwareVersion = 150)]
		public FontHandle sceFontOpenUserMemory(FontLibraryHandle FontLibraryHandle, byte* MemoryFontAddress, int MemoryFontLength, uint* ErrorCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			var MemoryFont = PointerUtils.PointerToByteArray(MemoryFontAddress, MemoryFontLength);
			var PGF = new PGF().Load(new MemoryStream(MemoryFont));
			var Font = new Font(FontLibrary, PGF);
			*ErrorCode = 0;
			return Fonts.Create(Font);
		}

		/// <summary>
		/// Opens a new font from a file.
		/// </summary>
		/// <param name="FontLibraryHandle">Handle of the library.</param>
		/// <param name="FileName">Path to the font file to open.</param>
		/// <param name="Mode">Mode for opening the font.</param>
		/// <param name="ErrorCode">Pointer to store any error code.</param>
		/// <returns>
		///		FontHandle
		/// </returns>
		[HlePspFunction(NID = 0x57FCB733, FirmwareVersion = 150)]
		public FontHandle sceFontOpenUserFile(FontLibraryHandle FontLibraryHandle, string FileName, int Mode, uint* ErrorCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			var FontFileStream = HleState.HleIoManager.HleIoWrapper.Open(FileName, Vfs.HleIoFlags.Read, (Vfs.SceMode)Mode);
			var PGF = new PGF().Load(FontFileStream);
			var Font = new Font(FontLibrary, PGF);
			*ErrorCode = 0;
			return Fonts.Create(Font);
		}

		/// <summary>
		/// Returns the number of available fonts.
		/// </summary>
		/// <param name="FontLibraryHandle">Handle of the library.</param>
		/// <param name="ErrorCode">Pointer to store any error code.</param>
		/// <returns>Number of fonts</returns>
		[HlePspFunction(NID = 0x27F6E642, FirmwareVersion = 150)]
		public int sceFontGetNumFontList(FontLibraryHandle FontLibraryHandle, uint* ErrorCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			*ErrorCode = 0;
			return FontLibrary.FontRegistryList.Count;
		}

		/// <summary>
		/// Retrieves all the font styles up to numFonts.
		/// </summary>
		/// <param name="FontLibraryHandle">Handle of the library.</param>
		/// <param name="FontStylesPointer">Pointer to store the font styles.</param>
		/// <param name="MaximumNumberOfFontsToGet">Number of fonts to write.</param>
		/// <returns>Number of fonts</returns>
		[HlePspFunction(NID = 0xBC75D85B, FirmwareVersion = 150)]
		public int sceFontGetFontList(FontLibraryHandle FontLibraryHandle, FontStyle* FontStylesPointer, int MaximumNumberOfFontsToGet)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			int NumberOfFontsReturned = Math.Min(MaximumNumberOfFontsToGet, FontLibrary.FontRegistryList.Count);
			for (int n = 0; n < NumberOfFontsReturned; n++)
			{
				FontStylesPointer[n] = FontLibrary.FontRegistryList[n].FontStyle;
			}
			return NumberOfFontsReturned;
		}

		/// <summary>
		/// Returns a font index that best matches the specified FontStyle.
		/// </summary>
		/// <param name="FontLibraryHandle">Handle of the library.</param>
		/// <param name="FontStylePointer">Family, style and </param>
		/// <param name="ErrorCode">Pointer to store any error code.</param>
		/// <returns>Font index</returns>
		[HlePspFunction(NID = 0x099EF33C, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceFontFindOptimumFont(FontLibraryHandle FontLibraryHandle, FontStyle* FontStylePointer, uint* ErrorCode)
		{
			return sceFontFindFont(FontLibraryHandle, FontStylePointer, ErrorCode);
		}

		/// <summary>
		/// Returns a font index that best matches the specified FontStyle.
		/// </summary>
		/// <param name="FontLibraryHandle">Handle of the library.</param>
		/// <param name="FontStylePointer">Family, style and language.</param>
		/// <param name="ErrorCode">Pointer to store any error code.</param>
		/// <returns>Font index</returns>
		[HlePspFunction(NID = 0x681E61A7, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceFontFindFont(FontLibraryHandle FontLibraryHandle, FontStyle* FontStylePointer, uint* ErrorCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			var FontRegistry = FontLibrary.FontRegistryList.OrderByDescending(Entry => FontStyle.GetScoreCompare(Entry.FontStyle, *FontStylePointer)).First();
			var FontIndex = FontLibrary.FontRegistryList.IndexOf(FontRegistry);
			*ErrorCode = 0;
			return FontIndex;
		}

		/// <summary>
		/// Obtains the FontInfo of a Font with its index.
		/// </summary>
		/// <param name="FontLibraryHandle">Handle of the library.</param>
		/// <param name="FontInfoPointer">Pointer to a FontInfo structure that will hold the information.</param>
		/// <param name="Unknown">???</param>
		/// <param name="FontIndex">Index of the font to get the information from.</param>
		/// <returns>
		///		0 on success
		/// </returns>
		[HlePspFunction(NID = 0x5333322D, FirmwareVersion = 150)]
		public int sceFontGetFontInfoByIndexNumber(FontLibraryHandle FontLibraryHandle, FontInfo* FontInfoPointer, int Unknown, int FontIndex)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontLibraryHandle"></param>
		/// <param name="CharCode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xEE232411, FirmwareVersion = 150)]
		public int sceFontSetAltCharacterCode(FontLibraryHandle FontLibraryHandle, ushort CharCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			FontLibrary.AlternateCharCode = CharCode;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontLibraryHandle"></param>
		/// <param name="HorizontalResolution"></param>
		/// <param name="VerticalResolution"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x48293280, FirmwareVersion = 150)]
		public int sceFontSetResolution(FontLibraryHandle FontLibraryHandle, float HorizontalResolution, float VerticalResolution)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			FontLibrary.HorizontalResolution = HorizontalResolution;
			FontLibrary.VerticalResolution = VerticalResolution;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontLibraryHandle"></param>
		/// <param name="FontPointsH"></param>
		/// <param name="ErrorCode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x472694CD, FirmwareVersion = 150)]
		public float sceFontPointToPixelH(FontLibraryHandle FontLibraryHandle, float FontPointsH, int* ErrorCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			*ErrorCode = 0;
			return FontPointsH * FontLibrary.HorizontalResolution / PointDPI;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontLibraryHandle"></param>
		/// <param name="FontPointsV"></param>
		/// <param name="ErrorCode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x3C4B7E82, FirmwareVersion = 150)]
		public float sceFontPointToPixelV(FontLibraryHandle FontLibraryHandle, float FontPointsV, int* ErrorCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			*ErrorCode = 0;
			return FontPointsV * FontLibrary.VerticalResolution / PointDPI;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontLibraryHandle"></param>
		/// <param name="FontPointsH"></param>
		/// <param name="ErrorCode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x74B21701, FirmwareVersion = 150)]
		public float sceFontPixelToPointH(FontLibraryHandle FontLibraryHandle, float FontPointsH, int* ErrorCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			*ErrorCode = 0;
			return FontPointsH * PointDPI / FontLibrary.HorizontalResolution;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FontLibraryHandle"></param>
		/// <param name="FontPointsH"></param>
		/// <param name="ErrorCode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF8F0752E, FirmwareVersion = 150)]
		public float sceFontPixelToPointV(FontLibraryHandle FontLibraryHandle, float FontPointsV, int* ErrorCode)
		{
			var FontLibrary = FontLibraries.Get(FontLibraryHandle);
			*ErrorCode = 0;
			return FontPointsV * PointDPI / FontLibrary.VerticalResolution;
		}
	}
}

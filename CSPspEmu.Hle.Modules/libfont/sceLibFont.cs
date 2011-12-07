using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.libfont
{
	unsafe public partial class sceLibFont : HleModuleHost
	{
		public enum FontLibraryHandle : int { }
		public enum FontHandle : int { }

		protected class FontLibrary
		{
			public FontNewLibParams Params;
		}

		HleUidPool<FontLibrary> FontLibraries = new HleUidPool<FontLibrary>();

		/// <summary>
		/// Creates a new font library.
		/// </summary>
		/// <param name="Params">Parameters of the new library.</param>
		/// <param name="errorCode">Pointer to store any error code.</param>
		/// <returns>FontLibraryHandle</returns>
		[HlePspFunction(NID = 0x67F17ED7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public FontLibraryHandle sceFontNewLib(FontNewLibParams* Params, uint* errorCode)
		{
			//if (Params != null) throw (new NotImplementedException("(Params != null)"));

			var FontLibrary = new FontLibrary()
			{
				Params = *Params,
			};

			return (FontLibraryHandle)FontLibraries.Create(FontLibrary);
		}

		/// <summary>
		/// Releases the font library.
		/// </summary>
		/// <param name="FontLibraryHandle">Handle of the library.</param>
		/// <returns>
		///		0 - success
		/// </returns>
		[HlePspFunction(NID = 0x574B6FBC, FirmwareVersion = 150)]
		public int sceFontDoneLib(FontLibraryHandle FontLibraryHandle)
		{
			FontLibraries.Remove((int)FontLibraryHandle);
			return 0;
		}

		/// <summary>
		/// Opens a new font.
		/// </summary>
		/// <param name="libHandle">Handle of the library.</param>
		/// <param name="index">Index of the font.</param>
		/// <param name="mode">Mode for opening the font.</param>
		/// <param name="errorCode">Pointer to store any error code.</param>
		/// <returns>
		///		FontHandle
		/// </returns>
        [HlePspFunction(NID = 0xA834319D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public FontHandle sceFontOpen(FontLibraryHandle libHandle, int index, int mode, uint* errorCode)
		{
			return (FontHandle)0;
			//throw (new NotImplementedException());
			/*
			unimplemented_notice();

			*errorCode = 0;
		
			return uniqueIdFactory.add(
				(new Font(uniqueIdFactory.get!FontLibrary(libHandle)))
					.setByIndex(index)
			);
			*/
		}

		/// <summary>
		/// Opens a new font from memory.
		/// </summary>
		/// <param name="libHandle">Handle of the library.</param>
		/// <param name="memoryFontAddr">Index of the font.</param>
		/// <param name="memoryFontLength">Mode for opening the font.</param>
		/// <param name="errorCode">Pointer to store any error code.</param>
		/// <returns>
		///		FontHandle
		/// </returns>
        [HlePspFunction(NID = 0xBB8E7FE6, FirmwareVersion = 150)]
		public FontHandle sceFontOpenUserMemory(FontLibraryHandle libHandle, void* memoryFontAddr, int memoryFontLength, uint* errorCode)
		{
			throw (new NotImplementedException());
			/*
			unimplemented_notice();

			*errorCode = 0;
		
			return uniqueIdFactory.add(
				(new Font(uniqueIdFactory.get!FontLibrary(libHandle)))
					.setByData((cast(ubyte *)memoryFontAddr)[0..memoryFontLength])
			);
			*/
		}

		/// <summary>
		/// Opens a new font from a file.
		/// </summary>
		/// <param name="libHandle">Handle of the library.</param>
		/// <param name="fileName">Path to the font file to open.</param>
		/// <param name="mode">Mode for opening the font.</param>
		/// <param name="errorCode">Pointer to store any error code.</param>
		/// <returns>
		///		FontHandle
		/// </returns>
        [HlePspFunction(NID = 0x57FCB733, FirmwareVersion = 150)]
		public FontHandle sceFontOpenUserFile(FontLibraryHandle libHandle, string fileName, int mode, uint* errorCode)
		{
			throw(new NotImplementedException());
			/*
			unimplemented_notice();
		
			*errorCode = 0;

			return uniqueIdFactory.add(
				(new Font(uniqueIdFactory.get!FontLibrary(libHandle)))
					.setByFileName(fileName)
			);
			*/
		}

		/// <summary>
		/// Closes the specified font file.
		/// </summary>
		/// <param name="fontHandle">Handle of the font.</param>
		/// <returns>
		///		0 on success.
		/// </returns>
        [HlePspFunction(NID = 0x3AEA8CB6, FirmwareVersion = 150)]
		public int sceFontClose(FontHandle fontHandle)
		{
			throw(new NotImplementedException());

			//return 0;
		}

		/// <summary>
		/// Returns the number of available fonts.
		/// </summary>
		/// <param name="libHandle">Handle of the library.</param>
		/// <param name="errorCode">Pointer to store any error code.</param>
		/// <returns>Number of fonts</returns>
		[HlePspFunction(NID = 0x27F6E642, FirmwareVersion = 150)]
		public int sceFontGetNumFontList(FontLibraryHandle libHandle, uint* errorCode)
		{
			throw(new NotImplementedException());
			/*
			unimplemented_notice();

			//throw(new NotImplementedException());
			FontLibrary fontLibrary = uniqueIdFactory.get!FontLibrary(libHandle);
			*errorCode = 0;

			return fontLibrary.fontNames.length;		
			*/
		}

		/// <summary>
		/// Retrieves all the font styles up to numFonts.
		/// </summary>
		/// <param name="libHandle">Handle of the library.</param>
		/// <param name="fontStyles">Pointer to store the font styles.</param>
		/// <param name="numFonts">Number of fonts to write.</param>
		/// <returns>Number of fonts</returns>
        [HlePspFunction(NID = 0xBC75D85B, FirmwareVersion = 150)]
		public int sceFontGetFontList(FontLibraryHandle libHandle, FontStyle* fontStyles, int numFonts)
		{
			throw (new NotImplementedException());
			/*
			unimplemented_notice();

    		FontLibrary fontLibrary = uniqueIdFactory.get!FontLibrary(libHandle);
    		fontStyles[0..numFonts] = fontLibrary.fontStyles[0..numFonts];
    		return 0;
			*/
		}

		/// <summary>
		/// Returns a font index that best matches the specified FontStyle.
		/// </summary>
		/// <param name="libHandle">Handle of the library.</param>
		/// <param name="fontStyle">Family, style and </param>
		/// <param name="errorCode">Pointer to store any error code.</param>
		/// <returns>Font index</returns>
		[HlePspFunction(NID = 0x099EF33C, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceFontFindOptimumFont(FontLibraryHandle libHandle, FontStyle* fontStyle, uint* errorCode)
		{
			return 0;
			//throw(new NotImplementedException());
			/*
			unimplemented_notice();

			*errorCode = 0;
			return 1;
			*/
		}

		/// <summary>
		/// Returns a font index that best matches the specified FontStyle.
		/// </summary>
		/// <param name="libHandle">Handle of the library.</param>
		/// <param name="fontStyle">Family, style and language.</param>
		/// <param name="errorCode">Pointer to store any error code.</param>
		/// <returns>Font index</returns>
        [HlePspFunction(NID = 0x681E61A7, FirmwareVersion = 150)]
		public int sceFontFindFont(FontLibraryHandle libHandle, FontStyle* fontStyle, uint* errorCode)
		{
			throw(new NotImplementedException());

			*errorCode = 0;
			return 0;
		}

		/// <summary>
		/// Obtains the FontInfo of a FontHandle.
		/// </summary>
		/// <param name="FontHandle">Font Handle to get the information from.</param>
		/// <param name="FontInfo">Pointer to a FontInfo structure that will hold the information.</param>
		/// <returns>
		///		0 on success
		/// </returns>
		[HlePspFunction(NID = 0x0DA7535E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceFontGetFontInfo(FontHandle FontHandle, FontInfo* FontInfo)
		{
			FontInfo[0] = new FontInfo()
			{
			};
			return 0;
		}

		/// <summary>
		/// Obtains the FontInfo of a Font with its index.
		/// </summary>
		/// <param name="LibraryHandle">Handle of the library.</param>
		/// <param name="FontInfo">Pointer to a FontInfo structure that will hold the information.</param>
		/// <param name="Unknown">???</param>
		/// <param name="FontIndex">Index of the font to get the information from.</param>
		/// <returns>
		///		0 on success
		/// </returns>
	    [HlePspFunction(NID = 0x5333322D, FirmwareVersion = 150)]
		public int sceFontGetFontInfoByIndexNumber(FontLibraryHandle LibraryHandle, FontInfo* FontInfo, int Unknown, int FontIndex)
		{
			throw(new NotImplementedException());
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fontHandle"></param>
		/// <param name="charCode"></param>
		/// <param name="fontCharInfo"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xDCC80C2F, FirmwareVersion = 150)]
		public int sceFontGetCharInfo(FontHandle fontHandle, uint charCode, FontCharInfo* fontCharInfo)
		{
    		throw(new NotImplementedException());
		}
    
    
		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x980F4895, FirmwareVersion = 150)]
		public void sceFontGetCharGlyphImage()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xCA1E6945, FirmwareVersion = 150)]
		public void sceFontGetCharGlyphImage_Clip()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xEE232411, FirmwareVersion = 150)]
		public void sceFontSetAltCharacterCode()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x5C3E4A9E, FirmwareVersion = 150)]
		public void sceFontGetCharImageRect()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x472694CD, FirmwareVersion = 150)]
		public void sceFontPointToPixelH()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x48293280, FirmwareVersion = 150)]
		public void sceFontSetResolution()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x3C4B7E82, FirmwareVersion = 150)]
		public void sceFontPointToPixelV()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x74B21701, FirmwareVersion = 150)]
		public void sceFontPixelToPointH()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0xF8F0752E, FirmwareVersion = 150)]
		public void sceFontPixelToPointV()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x2F67356A, FirmwareVersion = 150)]
		public void sceFontCalcMemorySize()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x48B06520, FirmwareVersion = 150)]
		public void sceFontGetShadowImageRect()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x568BE516, FirmwareVersion = 150)]
		public void sceFontGetShadowGlyphImage()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x5DCF6858, FirmwareVersion = 150)]
		public void sceFontGetShadowGlyphImage_Clip()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0xAA3DE7B5, FirmwareVersion = 150)]
		public void sceFontGetShadowInfo()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
        [HlePspFunction(NID = 0x02D7F94B, FirmwareVersion = 150)]
		public void sceFontFlush()
		{
			throw(new NotImplementedException());
		}
	}
}

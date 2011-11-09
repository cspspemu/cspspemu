using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public class TextureStateStruct
	{
		/*
		// Format of the texture data.
		public bool           swizzled;              /// Is texture swizzled?
		public PixelFormats   format;                /// Texture Data mode

		// Normal attributes
		public TextureFilter  filterMin, filterMag;  /// TextureFilter when drawing the texture scaled
		public WrapMode       wrapU, wrapV;          /// Wrap mode when specifying texture coordinates beyond texture size
		public UV             scale;                 /// 
		public UV             offset;                /// 

		// Effects
		public TextureEffect  effect;                /// 
		public TextureColorComponent colorComponent; ///
		public bool           fragment_2x;           /// ???

		// Mimaps
		struct MipmapState {
			uint address;                     /// Pointer 
			uint buffer_width;                ///
			uint width, height;               ///
		}
		public int            mipmapMaxLevel;        /// Levels of mipmaps
		public bool           mipmapShareClut;       /// Mipmaps share clut?
		public MipmapState[8] mipmaps;               /// MipmapState list

		public int mipmapRealWidth(int mipmap = 0) { return PixelFormatSize(format, mipmaps[mipmap].buffer_width); }
		public int mipmapTotalSize(int mipmap = 0) { return mipmapRealWidth(mipmap) * mipmaps[mipmap].height; }

		public string hash() { return cast(string)TA(this); }
		//string toString() { return std.string.format("TextureState(addr=%08X, size(%dx%d), bwidth=%d, format=%d, swizzled=%d)", address, width, height, buffer_width, format, swizzled); }

		public int address() { return mipmaps[0].address; }
		public int buffer_width() { return mipmaps[0].buffer_width; }
		public int width() { return mipmaps[0].width; }
		public int height() { return mipmaps[0].height; }
		public bool hasPalette() { return (format >= PixelFormats.GU_PSM_T4 && format <= PixelFormats.GU_PSM_T32); }
		public uint paletteRequiredComponents() { return hasPalette ? (1 << (4 + (format - PixelFormats.GU_PSM_T4))) : 0; }
		*/
	}
}

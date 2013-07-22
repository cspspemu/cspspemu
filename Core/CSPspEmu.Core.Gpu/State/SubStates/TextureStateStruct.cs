using CSPspEmu.Core.Types;
using System.Runtime.InteropServices;
namespace CSPspEmu.Core.Gpu.State.SubStates
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureStateStruct
	{
		/// <summary>
		/// Mimaps
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct MipmapState
		{
			/// <summary>
			/// Pointer 
			/// </summary>
			public uint Address;

			/// <summary>
			/// Data width of the image.
			/// With will be bigger. For example:
			/// Bufferwidth = 480, Width = 512, Height = 512
			/// Texture is 512x512 but there is data just for 480x512
			/// </summary>
			public ushort BufferWidth;

			/// <summary>
			/// Texture Width
			/// </summary>
			public ushort TextureWidth;

			/// <summary>
			/// Texture Height
			/// </summary>
			public ushort TextureHeight;
		}

		/// <summary>
		/// Is texture swizzled?
		/// </summary>
		public bool Swizzled;
	
		/// <summary>
		/// Mipmaps share clut?
		/// </summary>
		public bool MipmapShareClut;

		/// <summary>
		/// Levels of mipmaps
		/// </summary>
		public int MipmapMaxLevel;

		/// <summary>
		/// Format of the texture data.
		/// Texture Data mode
		/// </summary>
		public GuPixelFormats PixelFormat;

		/// <summary>
		/// MipmapState list
		/// </summary>
		public MipmapState Mipmap0;
		public MipmapState Mipmap1;
		public MipmapState Mipmap2;
		public MipmapState Mipmap3;
		public MipmapState Mipmap4;
		public MipmapState Mipmap5;
		public MipmapState Mipmap6;
		public MipmapState Mipmap7;

		/// <summary>
		/// TextureFilter when drawing the texture scaled
		/// </summary>
		public TextureFilter FilterMinification;

		/// <summary>
		/// TextureFilter when drawing the texture scaled
		/// </summary>
		public TextureFilter FilterMagnification;

		/// <summary>
		/// Wrap mode when specifying texture coordinates beyond texture size
		/// </summary>
		public WrapMode WrapU;

		/// <summary>
		/// Wrap mode when specifying texture coordinates beyond texture size
		/// </summary>
		public WrapMode WrapV;

		/// <summary>
		/// 
		/// </summary>
		public float ScaleU;

		/// <summary>
		/// 
		/// </summary>
		public float ScaleV;

		/// <summary>
		/// 
		/// </summary>
		public float OffsetU;

		/// <summary>
		/// 
		/// </summary>
		public float OffsetV;

		/// <summary>
		/// Effects:
		/// </summary>
		public bool Fragment2X;

		/// <summary>
		/// 
		/// </summary>
		public TextureEffect Effect;

		/// <summary>
		/// 
		/// </summary>
		public TextureColorComponent ColorComponent;

		/*
		public int mipmapRealWidth(int mipmap = 0) { return PixelFormatSize(format, mipmaps[mipmap].buffer_width); }
		public int mipmapTotalSize(int mipmap = 0) { return mipmapRealWidth(mipmap) * mipmaps[mipmap].height; }

		public string hash() { return cast(string)TA(this); }
		//string toString() { return std.string.format("TextureState(addr=%08X, size(%dx%d), bwidth=%d, format=%d, swizzled=%d)", address, width, height, buffer_width, format, swizzled); }

		public int address() { return mipmaps->address; }
		public int buffer_width() { return mipmaps->buffer_width; }
		public int width() { return mipmaps->width; }
		public int height() { return mipmaps->height; }
		public bool hasPalette() { return (format >= PixelFormats.GU_PSM_T4 && format <= PixelFormats.GU_PSM_T32); }
		public uint paletteRequiredComponents() { return hasPalette ? (1 << (4 + (format - PixelFormats.GU_PSM_T4))) : 0; }
		*/
	}
}

namespace CSPspEmu.Core
{
	public enum GuPixelFormats : uint
	{
		/// <summary>
		/// 0
		/// </summary>
		NONE = unchecked((uint)-1),

		/// <summary>
		/// PSP_DISPLAY_PIXEL_FORMAT_565
		/// GU_PSM_565
		/// </summary>
		RGBA_5650 = 0,

		/// <summary>
		/// PSP_DISPLAY_PIXEL_FORMAT_5551
		/// GU_PSM_5551
		/// </summary>
		RGBA_5551 = 1,

		/// <summary>
		/// PSP_DISPLAY_PIXEL_FORMAT_4444
		/// GU_PSM_4444
		/// </summary>
		RGBA_4444 = 2,

		/// <summary>
		/// PSP_DISPLAY_PIXEL_FORMAT_8888
		/// GU_PSM_8888
		/// </summary>
		RGBA_8888 = 3,

		/// <summary>
		/// GU_PSM_T4
		/// </summary>
		PALETTE_T4 = 4,

		/// <summary>
		/// GU_PSM_T8
		/// </summary>
		PALETTE_T8 = 5,

		/// <summary>
		/// GU_PSM_T16
		/// </summary>
		PALETTE_T16 = 6,

		/// <summary>
		/// GU_PSM_T32
		/// </summary>
		PALETTE_T32 = 7,

		/// <summary>
		/// GU_PSM_DXT1
		/// </summary>
		COMPRESSED_DXT1 = 8,

		/// <summary>
		/// GU_PSM_DXT3
		/// </summary>
		COMPRESSED_DXT3 = 9,

		/// <summary>
		/// GU_PSM_DXT5
		/// </summary>
		COMPRESSED_DXT5 = 10,
	}
}

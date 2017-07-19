namespace CSPspEmu.Core.Types
{
    public enum GuPixelFormats : uint
    {
        None = unchecked((uint) -1), // 0
        Rgba5650 = 0, // PSP_DISPLAY_PIXEL_FORMAT_565 - GU_PSM_565
        Rgba5551 = 1, // PSP_DISPLAY_PIXEL_FORMAT_5551 - GU_PSM_5551 
        Rgba4444 = 2, // PSP_DISPLAY_PIXEL_FORMAT_4444 - GU_PSM_4444
        Rgba8888 = 3, // PSP_DISPLAY_PIXEL_FORMAT_8888 - GU_PSM_8888 
        PaletteT4 = 4, // GU_PSM_T4
        PaletteT8 = 5, // GU_PSM_T8
        PaletteT16 = 6, // GU_PSM_T16
        PaletteT32 = 7, // GU_PSM_T32
        CompressedDxt1 = 8, // GU_PSM_DXT1
        CompressedDxt3 = 9, // GU_PSM_DXT3
        CompressedDxt5 = 10, // GU_PSM_DXT5
    }
}
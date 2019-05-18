using System;

namespace CSPspEmu.Core.Types
{
    // Format of the texture data. Texture Data mode.
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
    
    public static class GuPixelFormatsExt
    {
        public static int BytesPerPixel(this GuPixelFormats that, int numberOfPixels = 1)
        {
            switch (that)
            {
                case GuPixelFormats.None: return 0;
                case GuPixelFormats.Rgba5650:
                case GuPixelFormats.Rgba5551:
                case GuPixelFormats.Rgba4444: return 2 * numberOfPixels;
                case GuPixelFormats.Rgba8888: return 4 * numberOfPixels;
                case GuPixelFormats.PaletteT4: return (int) (0.5 * numberOfPixels);
                case GuPixelFormats.PaletteT8: return numberOfPixels;
                case GuPixelFormats.PaletteT16: return 2 * numberOfPixels;
                case GuPixelFormats.PaletteT32: return 4 * numberOfPixels;
                case GuPixelFormats.CompressedDxt1: return (int) ((0.5 * numberOfPixels) / 16);
                case GuPixelFormats.CompressedDxt3: return (1 * numberOfPixels) / 16;
                case GuPixelFormats.CompressedDxt5: return (1 * numberOfPixels) / 16;
                default: throw new InvalidOperationException($"ScreenBufferStateStruct.BytesPerPixel : Invalid Format : {that} : {numberOfPixels}");
            }
        }
    }
}
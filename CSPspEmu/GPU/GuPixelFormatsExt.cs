using System;
using CSPspEmu.Core.Types;

namespace CSPspEmu.GPU
{
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
                default: throw (new InvalidOperationException($"ScreenBufferStateStruct.BytesPerPixel : Invalid Format : {that} : {numberOfPixels}"));
            }
        }
    }
}
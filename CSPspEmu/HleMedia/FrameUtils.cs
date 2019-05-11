using cscodec.av;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace cscodec
{
    public unsafe class FrameUtils
    {
        public static Bitmap imageFromFrameWithoutEdges(AVFrame f)
        {
            return imageFromFrameWithoutEdges(f, f.imageWidthWOEdge, f.imageHeightWOEdge);
        }

        public static Bitmap imageFromFrameWithoutEdges(AVFrame f, int Width, int Height)
        {
            var XEdge = (f.imageWidth - f.imageWidthWOEdge) / 2;
            var YEdge = (f.imageHeight - f.imageHeightWOEdge) / 2;
            var Out = new Bitmap(Math.Min(Width, f.imageWidthWOEdge), Math.Min(Height, f.imageHeightWOEdge));
            var In = imageFromFrame(f);
            Graphics.FromImage(Out).DrawImage(In, new Point(-XEdge, -YEdge));
            return Out;
        }

        public static Bitmap imageFromFrame(AVFrame f)
        {
            Bitmap bi = new Bitmap(f.imageWidth, f.imageHeight, PixelFormat.Format32bppArgb);
            int[] rgb = new int[f.imageWidth * f.imageHeight];

            YUV2RGB(f, rgb);

            var BitmapData = bi.LockBits(new System.Drawing.Rectangle(0, 0, bi.Width, bi.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var Ptr = (int*) BitmapData.Scan0.ToPointer();
            for (int j = 0; j < f.imageHeight; j++)
            {
                int off = j * f.imageWidth;
                for (int i = 0; i < f.imageWidth; i++)
                {
                    Ptr[j * f.imageWidth + i] = rgb[off + i];
                }
            }
            bi.UnlockBits(BitmapData);

            return bi;
        }

        public static void YUV2RGB(AVFrame f, int[] rgb)
        {
            var luma = f.data_base[0];
            var cb = f.data_base[1];
            var cr = f.data_base[2];
            int stride = f.linesize[0];
            int strideChroma = f.linesize[1];

            fixed (int* rgbPtr = rgb)
            {
                for (int y = 0; y < f.imageHeight; y++)
                {
                    int lineOffLuma = y * stride;
                    int lineOffChroma = (y >> 1) * strideChroma;

                    for (int x = 0; x < f.imageWidth; x++)
                    {
                        int c = luma[lineOffLuma + x] - 16;
                        int d = cb[lineOffChroma + (x >> 1)] - 128;
                        int e = cr[lineOffChroma + (x >> 1)] - 128;

                        var c298 = 298 * c;

                        byte red = (byte) MathUtils.Clamp((c298 + 409 * e + 128) >> 8, 0, 255);
                        byte green = (byte) MathUtils.Clamp((c298 - 100 * d - 208 * e + 128) >> 8, 0, 255);
                        byte blue = (byte) MathUtils.Clamp((c298 + 516 * d + 128) >> 8, 0, 255);
                        byte alpha = 255;

                        rgbPtr[lineOffLuma + x] = (alpha << 24) | (red << 16) | (green << 8) | (blue << 0);
                    }
                }
            }
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Imaging;
using CSharpUtils.Drawing;
using CSharpUtils.Drawing.Extensions;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Utils.Utils
{
    public unsafe class PspBitmap
    {
        public GuPixelFormats GuPixelFormat;
        public int Width;
        public int BitsPerPixel;
        public int Height;
        public int BytesPerLine;
        public byte* Address;
        public ColorFormat ColorFormat;

        public int Area => Width * Height;

        public PspBitmap(GuPixelFormats pixelFormat, int width, int height, byte* address, int bytesPerLine = -1)
        {
            GuPixelFormat = pixelFormat;
            Width = width;
            Height = height;
            Address = address;
            BitsPerPixel = PixelFormatDecoder.GetPixelsBits(pixelFormat);
            ColorFormat = PixelFormatDecoder.ColorFormatFromPixelFormat(pixelFormat);
            if (bytesPerLine < 0)
            {
                BytesPerLine = PixelFormatDecoder.GetPixelsSize(GuPixelFormat, width);
            }
            else
            {
                BytesPerLine = bytesPerLine;
            }
        }

        public bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        private int GetOffset(int x, int y) => y * BytesPerLine + PixelFormatDecoder.GetPixelsSize(GuPixelFormat, x);

        public void SetPixel(int x, int y, OutputPixel color)
        {
            if (!IsValidPosition(x, y)) return;
            //var Position = PixelFormatDecoder.GetPixelsSize(GuPixelFormat, Y * Width + X);
            var position = GetOffset(x, y);
            var value = ColorFormat.Encode(color);
            switch (BitsPerPixel)
            {
                case 16:
                    *(ushort*) (Address + position) = (ushort) value;
                    break;
                case 32:
                    *(uint*) (Address + position) = value;
                    break;
                default: throw new NotImplementedException();
            }
        }

        public OutputPixel[] GetPixels()
        {
            var o = new OutputPixel[Width * Height];
            var length = Width * Height;
            var n = 0;
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    o[n++] = this.GetPixel(x, y);
                }
            }
            return o;
        }

        public OutputPixel GetPixel(int x, int y)
        {
            if (!IsValidPosition(x, y)) return new OutputPixel();
            uint value;
            var position = GetOffset(x, y);
            switch (BitsPerPixel)
            {
                case 16:
                    value = *(ushort*) (Address + position);
                    break;
                case 32:
                    value = *(uint*) (Address + position);
                    break;
                default: throw new NotImplementedException();
            }
            //var OutputPixel = default(OutputPixel);
            return ColorFormat.Decode(value);
        }

        public Bitmap ToBitmap()
        {
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (bitmapData) =>
            {
                var count = Width * Height;
                var output = (OutputPixel*) bitmapData.Scan0;
                PixelFormatDecoder.Decode(
                    GuPixelFormat,
                    Address,
                    output,
                    Width,
                    Height,
                    ignoreAlpha: true
                );

                for (var n = 0; n < count; n++)
                {
                    var color = output[n];
                    output[n].R = color.B;
                    output[n].G = color.G;
                    output[n].B = color.R;
                    output[n].A = 0xFF;
                }
            });
            return bitmap;
        }
    }
}
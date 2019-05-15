using System;

namespace CSPspEmu.Utils.Imaging
{
    public class Bitmap32
    {
        public readonly int Width;
        public readonly int Height;
        public readonly uint[] Data;
        public Span<uint> DataSpan => new Span<uint>(Data);

        public Bitmap32(int width, int height, uint[] data = null)
        {
            Width = width;
            Height = height;
            Data = data ?? new uint[width * height];
        }
    }
}
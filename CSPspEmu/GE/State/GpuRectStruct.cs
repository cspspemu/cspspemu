using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GpuRectStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public short Left;

        /// <summary>
        /// 
        /// </summary>
        public short Top;

        /// <summary>
        /// 
        /// </summary>
        public short Right;

        /// <summary>
        /// 
        /// </summary>
        public short Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        /// <summary>
        /// 
        /// </summary>
        public bool IsFull => Left <= 0 && Top <= 0 && Right >= 480 && Bottom >= 272;

        public GpuRectStruct(short left, short top, short right, short bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
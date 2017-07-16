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

        /// <summary>
        /// 
        /// </summary>
        public bool IsFull
        {
            get { return (Left <= 0 && Top <= 0) && (Right >= 480 && Bottom >= 272); }
        }
    }
}
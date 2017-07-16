using System.Runtime.InteropServices;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureTransferStateStruct
    {
        public enum TexelSizeEnum : ushort
        {
            BIT_16 = 0,
            BIT_32 = 1
        }
        //enum TexelSize { BIT_32 = 0, BIT_16 = 1 }

        public int BytesPerPixel
        {
            get { return (TexelSize == TexelSizeEnum.BIT_16) ? 2 : 4; }
        }

        public int SourceLineWidthInBytes
        {
            get { return (int) (SourceLineWidth * BytesPerPixel); }
        }

        public int DestinationLineWidthInBytes
        {
            get { return (int) (DestinationLineWidth * BytesPerPixel); }
        }

        public int WidthInBytes
        {
            get { return (int) (Width * BytesPerPixel); }
        }

        public PspPointer SourceAddress, DestinationAddress;
        public ushort SourceLineWidth, DestinationLineWidth;
        public ushort SourceX, SourceY, DestinationX, DestinationY;
        public ushort Width, Height;
        public TexelSizeEnum TexelSize;
    }
}
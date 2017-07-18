using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
    {
        public void OP_TRXSBP() => GpuState->TextureTransferState.SourceAddress.Low24 = Params24;

        public void OP_TRXSBW()
        {
            GpuState->TextureTransferState.SourceAddress.High8 = Params24 << 8;
            GpuState->TextureTransferState.SourceLineWidth = (ushort) Extract(0, 16);
            GpuState->TextureTransferState.SourceX = 0;
            GpuState->TextureTransferState.SourceY = 0;
        }

        public void OP_TRXSPOS()
        {
            GpuState->TextureTransferState.SourceX = (ushort) Extract(10 * 0, 10);
            GpuState->TextureTransferState.SourceY = (ushort) Extract(10 * 1, 10);
        }

        public void OP_TRXDBP() => GpuState->TextureTransferState.DestinationAddress.Low24 = Params24;

        public void OP_TRXDBW()
        {
            ref var textureTransferStateStruct = ref GpuState->TextureTransferState;
            textureTransferStateStruct.DestinationAddress.High8 = Params24 << 8;
            textureTransferStateStruct.DestinationLineWidth = (ushort) Extract(0, 16);
            textureTransferStateStruct.DestinationX = 0;
            textureTransferStateStruct.DestinationY = 0;
        }

        public void OP_TRXDPOS()
        {
            GpuState->TextureTransferState.DestinationX = (ushort) Extract(10 * 0, 10);
            GpuState->TextureTransferState.DestinationY = (ushort) Extract(10 * 1, 10);
        }

        public void OP_TRXSIZE()
        {
            GpuState->TextureTransferState.Width = (ushort) (Extract(10 * 0, 10) + 1);
            GpuState->TextureTransferState.Height = (ushort) (Extract(10 * 1, 10) + 1);
        }

        public void OP_TRXKICK()
        {
            GpuState->TextureTransferState.TexelSize = (TextureTransferStateStruct.TexelSizeEnum) Extract(0, 1);
            GpuDisplayList.GpuProcessor.GpuImpl.Transfer(GpuDisplayList.GpuStateStructPointer);
        }
    }
}
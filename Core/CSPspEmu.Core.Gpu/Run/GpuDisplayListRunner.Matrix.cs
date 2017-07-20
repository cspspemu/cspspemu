using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Run
{
    // ReSharper disable UnusedMember.Global
    public sealed unsafe partial class GpuDisplayListRunner
    {
        [GpuInstructionAttribute(GpuOpCodes.VMS)]
        public void OP_VMS() => GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Reset(Params24);
        [GpuInstructionAttribute(GpuOpCodes.VIEW)]
        public void OP_VIEW() => GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Write(Float1);

        [GpuInstructionAttribute(GpuOpCodes.WMS)]
        public void OP_WMS() => GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Reset(Params24);
        [GpuInstructionAttribute(GpuOpCodes.WORLD)]
        public void OP_WORLD() => GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Write(Float1);
        
        [GpuInstructionAttribute(GpuOpCodes.PMS)]
        public void OP_PMS() => GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Reset(Params24);
        
        [GpuInstructionAttribute(GpuOpCodes.PROJ)]
        public void OP_PROJ() => GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Write(Float1);
        
        private SkinningStateStruct* SkinningState => &GpuDisplayList.GpuStateStructPointer->SkinningState;

        [GpuInstructionAttribute(GpuOpCodes.BOFS)]
        public void OP_BOFS() => SkinningState->CurrentBoneIndex = (int) Params24;

        [GpuInstructionAttribute(GpuOpCodes.BONE)]
        public void OP_BONE()
        {
            var boneMatrices = &SkinningState->BoneMatrix0;
            boneMatrices[SkinningState->CurrentBoneIndex / 12]
                .WriteAt(SkinningState->CurrentBoneIndex % 12, Float1);
            SkinningState->CurrentBoneIndex++;
        }
    }
}
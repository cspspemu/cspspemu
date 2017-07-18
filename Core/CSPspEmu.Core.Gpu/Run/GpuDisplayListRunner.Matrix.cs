using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Run
{
    // ReSharper disable UnusedMember.Global
    public sealed unsafe partial class GpuDisplayListRunner
    {
        public void OP_VMS() => GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Reset(Params24);
        public void OP_VIEW() => GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Write(Float1);
        public void OP_WMS() => GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Reset(Params24);
        public void OP_WORLD() => GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Write(Float1);
        public void OP_PMS() => GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Reset(Params24);
        public void OP_PROJ() => GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Write(Float1);
        private SkinningStateStruct* SkinningState => &GpuDisplayList.GpuStateStructPointer->SkinningState;

        public void OP_BOFS() => SkinningState->CurrentBoneIndex = (int) Params24;

        public void OP_BONE()
        {
            var boneMatrices = &SkinningState->BoneMatrix0;
            boneMatrices[SkinningState->CurrentBoneIndex / 12]
                .WriteAt(SkinningState->CurrentBoneIndex % 12, Float1);
            SkinningState->CurrentBoneIndex++;
        }
    }
}
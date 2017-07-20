namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
    {
        [GpuInstructionAttribute(GpuOpCodes.PSUB)]
        public void OP_PSUB()
        {
            GpuState->PatchState.DivS = Param8(0);
            GpuState->PatchState.DivT = Param8(8);
        }

        [GpuOpCodesNotImplemented]
        // ReSharper disable once UnusedMember.Global
        [GpuInstructionAttribute(GpuOpCodes.PPRIM)]
        public void OP_PPRIM()
        {
            //gpu.state.patch.type = command.extract!(PatchPrimitiveType, 0);
        }

        [GpuOpCodesNotImplemented]
        // ReSharper disable once UnusedMember.Global
        [GpuInstructionAttribute(GpuOpCodes.SPLINE)]
        public void OP_SPLINE()
        {
            /*
            auto sp_ucount = command.extract!(uint,  0, 8); 
            auto sp_vcount = command.extract!(uint,  8, 8);
            auto sp_utype  = command.extract!(uint, 16, 2);
            auto sp_vtype  = command.extract!(uint, 18, 2);
            gpu.logWarning("OP_SPLINE(%d, %d, %d, %d)", sp_ucount, sp_vcount, sp_utype, sp_vtype);
            */
        }

        // ReSharper disable once UnusedMember.Global
        [GpuInstructionAttribute(GpuOpCodes.PFACE)]
        public void OP_PFACE() => GpuState->PatchCullingState.FaceFlag = (Params24 != 0);
    }
}
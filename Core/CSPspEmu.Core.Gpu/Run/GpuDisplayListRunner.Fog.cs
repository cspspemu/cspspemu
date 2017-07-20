namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
    {
        /**
         * Set current Fog
         *
         * @param near  - 
         * @param far   - 
         * @param color - 0x00RRGGBB
         **/
        // void sceGuFog(float near, float far, unsigned int color); // OP_FCOL + OP_FFAR + OP_FDIST

        [GpuInstructionAttribute(GpuOpCodes.FGE)]
        public void OP_FGE() => GpuState->FogState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.FCOL)]
        public void OP_FCOL() => GpuState->FogState.Color.SetRGB_A1(Params24);

        [GpuInstructionAttribute(GpuOpCodes.FFAR)]
        public void OP_FFAR() => GpuState->FogState.End = Float1;

        [GpuInstructionAttribute(GpuOpCodes.FDIST)]
        public void OP_FDIST() => GpuState->FogState.Dist = Float1;
    }
}
namespace CSPspEmu.Core.Gpu.Run
{
    // ReSharper disable UnusedMember.Global
    public sealed unsafe partial class GpuDisplayListRunner
    {
        private void _OP_MW(int index) => GpuState->MorphingState.MorphWeight[index] = Float1;

        [GpuInstructionAttribute(GpuOpCodes.MW0)]
        public void OP_MW0() => _OP_MW(0);

        [GpuInstructionAttribute(GpuOpCodes.MW1)]
        public void OP_MW1() => _OP_MW(1);

        [GpuInstructionAttribute(GpuOpCodes.MW2)]
        public void OP_MW2() => _OP_MW(2);

        [GpuInstructionAttribute(GpuOpCodes.MW3)]
        public void OP_MW3() => _OP_MW(3);

        [GpuInstructionAttribute(GpuOpCodes.MW4)]
        public void OP_MW4() => _OP_MW(4);

        [GpuInstructionAttribute(GpuOpCodes.MW5)]
        public void OP_MW5() => _OP_MW(5);

        [GpuInstructionAttribute(GpuOpCodes.MW6)]
        public void OP_MW6() => _OP_MW(6);

        [GpuInstructionAttribute(GpuOpCodes.MW7)]
        public void OP_MW7() => _OP_MW(7);
    }
}
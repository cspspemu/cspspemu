namespace CSPspEmu.Core.Gpu.Run
{
    // ReSharper disable UnusedMember.Global
    public sealed unsafe partial class GpuDisplayListRunner
    {
        private void _OP_MW(int index) => GpuState->MorphingState.MorphWeight[index] = Float1;
        public void OP_MW0() => _OP_MW(0);
        public void OP_MW1() => _OP_MW(1);
        public void OP_MW2() => _OP_MW(2);
        public void OP_MW3() => _OP_MW(3);
        public void OP_MW4() => _OP_MW(4);
        public void OP_MW5() => _OP_MW(5);
        public void OP_MW6() => _OP_MW(6);
        public void OP_MW7() => _OP_MW(7);
    }
}
namespace CSPspEmu.Core.Cpu.VFpu
{
    public struct VfpuRegisterInt
    {
        public uint Value;

        public static implicit operator int(VfpuRegisterInt value) => (int) value.Value;

        public static implicit operator VfpuRegisterInt(int value) => new VfpuRegisterInt {Value = (uint) value};

        public static implicit operator uint(VfpuRegisterInt value) => value.Value;

        public static implicit operator VfpuRegisterInt(uint value) => new VfpuRegisterInt {Value = value};
    }
}
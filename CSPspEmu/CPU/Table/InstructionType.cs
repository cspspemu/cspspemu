using System;

namespace CSPspEmu.Core.Cpu.Table
{
    [Flags]
    public enum InstructionType
    {
        None = 0x00,
        B = 1 << 0,
        Jump = 1 << 1,
        Jal = (1 << 2) | (1 << 3),
        Likely = 1 << 4,
        Psp = 1 << 8,
        Syscall = 1 << 16,
    }
}
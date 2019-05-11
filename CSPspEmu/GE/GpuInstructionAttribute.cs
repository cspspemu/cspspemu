using System;

namespace CSPspEmu.Core.Gpu
{
    public class GpuInstructionAttribute : Attribute
    {
        public GpuOpCodes Opcode;

        public GpuInstructionAttribute(GpuOpCodes opcode)
        {
            Opcode = opcode;
        }
    }
}
using CSharpUtils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Utils;

namespace CSPspEmu
{
    public unsafe class ArgumentReader : IArgumentReader
    {
        int GprPosition = 4;
        int FprPosition = 0;
        CpuThreadState CpuThreadState;

        public ArgumentReader(CpuThreadState CpuThreadState)
        {
            this.CpuThreadState = CpuThreadState;
        }

        public int LoadInteger()
        {
            try
            {
                return CpuThreadState.Gpr[GprPosition];
            }
            finally
            {
                GprPosition++;
            }
        }

        public long LoadLong()
        {
            try
            {
                var Low = CpuThreadState.Gpr[GprPosition + 0];
                var High = CpuThreadState.Gpr[GprPosition + 1];
                return (long) ((High << 32) | (Low << 0));
            }
            finally
            {
                GprPosition += 2;
            }
        }

        public string LoadString()
        {
            return PointerUtils.PtrToStringUtf8((byte*) CpuThreadState.GetMemoryPtr((uint) LoadInteger()));
        }

        public float LoadFloat()
        {
            try
            {
                return CpuThreadState.Fpr[FprPosition];
            }
            finally
            {
                FprPosition++;
            }
        }
    }
}
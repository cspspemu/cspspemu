using CSharpUtils;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle
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
				return CpuThreadState.GPR[GprPosition];
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
				var Low = CpuThreadState.GPR[GprPosition + 0];
				var High = CpuThreadState.GPR[GprPosition + 1];
				return (long)((High << 32) | (Low << 0));
			}
			finally
			{
				GprPosition += 2;
			}
		}

		public string LoadString()
		{
			return PointerUtils.PtrToStringUtf8((byte*)CpuThreadState.GetMemoryPtr((uint)LoadInteger()));
		}

		public float LoadFloat()
		{
			try
			{
				return CpuThreadState.FPR[FprPosition];
			}
			finally
			{
				FprPosition++;
			}
		}
	}
}

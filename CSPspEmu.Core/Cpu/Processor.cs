using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	unsafe sealed public class Processor
	{
		public uint *GPR_Ptr;
		public float* FPR_Ptr;
		public uint[] GPR = new uint[32];
		public float[] FPR = new float[32];

		public Processor()
		{
			fixed (uint* Ptr = &GPR[0])
			{
				GPR_Ptr = Ptr;
			}
			fixed (float* Ptr = &FPR[0])
			{
				FPR_Ptr = Ptr;
			}
		}

		public void Test()
		{
			GPR[0] = 0;
		}

		public uint LoadGPR(int R)
		{
			return GPR_Ptr[R];
		}

		public void SaveGPR(int R, uint V)
		{
			GPR_Ptr[R] = V;
		}

		static public void TestGPR(Processor Processor)
		{
			Processor.GPR_Ptr[1] = Processor.GPR_Ptr[2] + Processor.GPR_Ptr[2];
		}
	}
}

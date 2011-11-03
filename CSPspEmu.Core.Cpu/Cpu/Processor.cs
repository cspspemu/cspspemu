using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	unsafe sealed public class Processor
	{
		public uint PC;
		public uint nPC;

		public uint HI, LO;

		public bool BranchFlag;

		public uint *GPR_Ptr;
		public float* FPR_Ptr;
		readonly public int[] GPR = new int[32];
		readonly public float[] FPR = new float[32];

		public Processor()
		{
			fixed (int* Ptr = &GPR[0])
			{
				GPR_Ptr = (uint *)Ptr;
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

		static public void TestBranchFlag(Processor Processor)
		{
			Processor.BranchFlag = (Processor.GPR_Ptr[2] == Processor.GPR_Ptr[2]);
		}

		static public void TestGPR(Processor Processor)
		{
			Processor.GPR_Ptr[1] = Processor.GPR_Ptr[2] + Processor.GPR_Ptr[2];
		}

		Dictionary<int, Action<int, Processor>> RegisteredNativeSyscalls = new Dictionary<int, Action<int, Processor>>();

		public Processor RegisterNativeSyscall(int Code, Action Callback)
		{
			return RegisterNativeSyscall(Code, (_Code, _Processor) => Callback());
		}

		public Processor RegisterNativeSyscall(int Code, Action<int, Processor> Callback)
		{
			RegisteredNativeSyscalls[Code] = Callback;
			return this;
		}

		public void Syscall(int Code)
		{
			Action<int, Processor> Callback;
			if (RegisteredNativeSyscalls.TryGetValue(Code, out Callback))
			{
				Callback(Code, this);
			}
			else
			{
				Console.WriteLine("Undefined syscall: {0}", Code);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu;
using CSharpUtils.Threading;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu
{
	unsafe sealed public class CpuThreadState
	{
		public Processor Processor;
		public object ModuleObject;

		public int StepInstructionCount;
		public long TotalInstructionCount;

		public uint PC;
		//public uint nPC;

		public int LO, HI;

		public bool BranchFlag;

		public uint GPR0, GPR1, GPR2, GPR3, GPR4, GPR5, GPR6, GPR7, GPR8, GPR9, GPR10, GPR11, GPR12, GPR13, GPR14, GPR15, GPR16, GPR17, GPR18, GPR19, GPR20, GPR21, GPR22, GPR23, GPR24, GPR25, GPR26, GPR27, GPR28, GPR29, GPR30, GPR31;
		public float FPR0, FPR1, FPR2, FPR3, FPR4, FPR5, FPR6, FPR7, FPR8, FPR9, FPR10, FPR11, FPR12, FPR13, FPR14, FPR15, FPR16, FPR17, FPR18, FPR19, FPR20, FPR21, FPR22, FPR23, FPR24, FPR25, FPR26, FPR27, FPR28, FPR29, FPR30, FPR31;


		public struct FCR31
		{
			public enum TypeEnum : uint {
				Rint = 0,
				Cast = 1,
				Ceil = 2,
				Floor = 3,
			}
			public uint Value;

			// 0b_0000000_F_C_000000000000000000000_RM
			/*
				// 0b_0000000_1_1_000000000000000000000_11
				Type, "RM", 2,
				uint, "",   21,
				bool, "C" , 1,
				bool, "FS", 1,
				uint, "",   7
			*/

			public TypeEnum RM
			{
				get
				{
					return (TypeEnum)BitUtils.Extract(Value, 0, 2);
				}
				set
				{
					Value = BitUtils.Insert(Value, 0, 2, (uint)value);
				}
			}


			public bool CC {
				get
				{
					return (BitUtils.Extract(Value, 23, 1) != 0);
				}
				set
				{
					Value = BitUtils.Insert(Value, 23, 1, (uint)(value ? 1 : 0));
				}
			}

			public bool FS
			{
				get
				{
					return (BitUtils.Extract(Value, 24, 1) != 0);
				}
				set
				{
					Value = BitUtils.Insert(Value, 24, 1, (uint)(value ? 1 : 0));
				}
			}
		}

		public FCR31 Fcr31;

		// http://msdn.microsoft.com/en-us/library/ms253512(v=vs.80).aspx
		// http://logos.cs.uic.edu/366/notes/mips%20quick%20tutorial.htm

		/// <summary>
		/// Points to the middle of the 64K block of memory in the static data segment.
		/// </summary>
		public uint GP
		{
			get { return GPR28; }
			set { GPR28 = value; }
		}

		/// <summary>
		/// Points to last location on the stack.
		/// </summary>
		public uint SP
		{
			get { return GPR29; }
			set { GPR29 = value; }
		}

		/// <summary>
		/// saved value / frame pointer
		/// Preserved across procedure calls
		/// </summary>
		public uint FP
		{
			get { return GPR30; }
			set { GPR30 = value; }
		}

		/// <summary>
		/// Return Address
		/// </summary>
		public uint RA
		{
			get { return GPR31; }
			set { GPR31 = value; }
		}

		/*
		public struct FixedRegisters
		{
			public fixed uint GPR[32];
			public fixed float FPR[32];
		}

		public FixedRegisters Registers;
		*/

		public class GprList
		{
			public CpuThreadState Processor;

			public int this[int Index]
			{
				get
				{
					fixed (uint* PTR = &Processor.GPR0)
					{
						return (int)PTR[Index];
					}
				}
				set
				{
					fixed (uint* PTR = &Processor.GPR0)
					{
						PTR[Index] = (uint)value;
					}
				}
			}
		}

		public class FprList
		{
			public CpuThreadState Processor;

			public float this[int Index]
			{
				get
				{
					fixed (float* PTR = &Processor.FPR0)
					{
						return PTR[Index];
					}
				}
				set
				{
					fixed (float* PTR = &Processor.FPR0)
					{
						PTR[Index] = value;
					}
				}
			}
		}

		public class FprListInteger
		{
			public CpuThreadState Processor;

			public int this[int Index]
			{
				get
				{
					fixed (float* PTR = &Processor.FPR0)
					{
						return ((int *)PTR)[Index];
					}
				}
				set
				{
					fixed (float* PTR = &Processor.FPR0)
					{
						((int*)PTR)[Index] = value;
					}
				}
			}
		}

		//public uint *GPR_Ptr;
		//public float* FPR_Ptr;

		public GprList GPR;
		public FprList FPR;
		public FprListInteger FPR_I;
		//readonly public float* FPR;

		public void* GetMemoryPtr(uint Address)
		{
			var Pointer = Processor.Memory.PspAddressToPointer(Address);
			//Console.WriteLine("%08X".Sprintf((uint)Pointer));
			return Pointer;
		}

		public IEnumerable<int> GPRList(params int[] Indexes)
		{
			return Indexes.Select(Index => {
				fixed (uint *PTR = &GPR0)
				{
					return (int)PTR[Index];
				}
			});
		}

		public CpuThreadState(Processor Processor)
		{
			this.Processor = Processor;

			GPR = new GprList() { Processor = this };
			FPR = new FprList() { Processor = this };
			FPR_I = new FprListInteger() { Processor = this };

			for (int n = 0; n < 32; n++)
			{
				GPR[n] = 0;
				FPR[n] = 0;
			}
		}

		~CpuThreadState()
		{
			//Marshal.FreeHGlobal(new IntPtr(GPR));
			//Marshal.FreeHGlobal(new IntPtr(FPR));
		}

		/*
		public void Test()
		{
			GPR[0] = 0;
		}

		public uint LoadGPR(int R)
		{
			return (uint)GPR[R];
		}

		public void SaveGPR(int R, uint V)
		{
			GPR[R] = (int)V;
		}

		static public void TestBranchFlag(Processor Processor)
		{
			Processor.BranchFlag = (Processor.GPR[2] == Processor.GPR[2]);
		}

		static public void TestGPR(Processor Processor)
		{
			Processor.GPR[1] = Processor.GPR[2] + Processor.GPR[2];
		}
		*/

		public void Syscall(int Code)
		{
			Processor.Syscall(Code, this);
		}

		/*
		static public void TestMemset(CpuThreadState Processor)
		{
			*((byte*)Processor.GetMemoryPtr(0x04000000)) = 0x77;
		}
		*/

		public void Yield()
		{
			//Console.WriteLine(StepInstructionCount);
			GreenThread.Yield();
			//Console.WriteLine(StepInstructionCount);
		}

		public void Trace(uint PC)
		{
			Console.WriteLine("  Trace: {0:X}", PC);
		}

		public void BreakpointIfEnabled()
		{
		}

		public void DumpRegisters()
		{
			for (int n = 0; n < 32; n++)
			{
				if (n % 4 != 0) Console.Write(", ");
				Console.Write("r{0,2} : {1:X}", n, "0x%08X".Sprintf(GPR[n]));
				if (n % 4 == 3) Console.WriteLine();
			}
			Console.WriteLine();
		}
	}
}

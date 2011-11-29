using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu;
using CSharpUtils.Threading;
using CSharpUtils;
using System.IO;

namespace CSPspEmu.Core.Cpu
{
	unsafe sealed public class CpuThreadState
	{
		public CpuProcessor CpuProcessor;
		public object ModuleObject;

		public int StepInstructionCount;
		public long TotalInstructionCount;

		public uint PC;
		//public uint nPC;

		public int LO, HI;

		public uint IC;

		public bool BranchFlag;

		/// <summary>
		/// General Purporse Registers
		/// </summary>
		public uint GPR0, GPR1, GPR2, GPR3, GPR4, GPR5, GPR6, GPR7, GPR8, GPR9, GPR10, GPR11, GPR12, GPR13, GPR14, GPR15, GPR16, GPR17, GPR18, GPR19, GPR20, GPR21, GPR22, GPR23, GPR24, GPR25, GPR26, GPR27, GPR28, GPR29, GPR30, GPR31;

		/// <summary>
		/// Floating Point Registers
		/// </summary>
		public float FPR0, FPR1, FPR2, FPR3, FPR4, FPR5, FPR6, FPR7, FPR8, FPR9, FPR10, FPR11, FPR12, FPR13, FPR14, FPR15, FPR16, FPR17, FPR18, FPR19, FPR20, FPR21, FPR22, FPR23, FPR24, FPR25, FPR26, FPR27, FPR28, FPR29, FPR30, FPR31;

		/// <summary>
		/// Vfpu registers
		/// </summary>
		public float VFR0, VFR1, VFR2, VFR3, VFR4, VFR5, VFR6, VFR7, VFR8, VFR9, VFR10, VFR11, VFR12, VFR13, VFR14, VFR15, VFR16, VFR17, VFR18, VFR19, VFR20, VFR21, VFR22, VFR23, VFR24, VFR25, VFR26, VFR27, VFR28, VFR29, VFR30, VFR31, VFR32, VFR33, VFR34, VFR35, VFR36, VFR37, VFR38, VFR39, VFR40, VFR41, VFR42, VFR43, VFR44, VFR45, VFR46, VFR47, VFR48, VFR49, VFR50, VFR51, VFR52, VFR53, VFR54, VFR55, VFR56, VFR57, VFR58, VFR59, VFR60, VFR61, VFR62, VFR63, VFR64, VFR65, VFR66, VFR67, VFR68, VFR69, VFR70, VFR71, VFR72, VFR73, VFR74, VFR75, VFR76, VFR77, VFR78, VFR79, VFR80, VFR81, VFR82, VFR83, VFR84, VFR85, VFR86, VFR87, VFR88, VFR89, VFR90, VFR91, VFR92, VFR93, VFR94, VFR95, VFR96, VFR97, VFR98, VFR99, VFR100, VFR101, VFR102, VFR103, VFR104, VFR105, VFR106, VFR107, VFR108, VFR109, VFR110, VFR111, VFR112, VFR113, VFR114, VFR115, VFR116, VFR117, VFR118, VFR119, VFR120, VFR121, VFR122, VFR123, VFR124, VFR125, VFR126, VFR127;

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

		//public fixed uint PcStack[1024];
		//public uint PcStackOffset;

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
		/// Reserved for use by the interrupt/trap handler 
		/// </summary>
		public uint K0
		{
			get { return GPR26; }
			set { GPR26 = value; }
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
			var Pointer = CpuProcessor.Memory.PspAddressToPointer(Address);
			//Console.WriteLine("%08X".Sprintf((uint)Pointer));
			return Pointer;
		}

		public void* GetMemoryPtrSafe(uint Address)
		{
			return CpuProcessor.Memory.PspAddressToPointerSafe(Address);
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

		public CpuThreadState(CpuProcessor Processor)
		{
			this.CpuProcessor = Processor;

			GPR = new GprList() { Processor = this };
			FPR = new FprList() { Processor = this };
			FPR_I = new FprListInteger() { Processor = this };

			for (int n = 0; n < 32; n++)
			{
				GPR[n] = 0;
				FPR[n] = 0.0f;
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
			CpuProcessor.Syscall(Code, this);
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

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://msdn.microsoft.com/en-us/library/ms253512(v=vs.80).aspx"/>
		string[] RegisterMnemonicNames = new string[] {
			"zr", "at", "v0", "v1", "a0", "a1", "a2", "a3",
			"t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7",
			"s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7",
			"t8", "t9", "k0", "k1", "gp", "sp", "fp", "ra", 
		};

		public void DumpRegisters()
		{
			DumpRegisters(Console.Out);
		}

		public void DumpRegisters(TextWriter TextWriter)
		{
			for (int n = 0; n < 32; n++)
			{
				if (n % 4 != 0) TextWriter.Write(", ");
				TextWriter.Write("r{0,2}({1}) : {2}", n, RegisterMnemonicNames[n], "0x%08X".Sprintf(GPR[n]));
				if (n % 4 == 3) TextWriter.WriteLine();
			}

			TextWriter.WriteLine();
			for (int n = 0; n < 32; n++)
			{
				if (n % 4 != 0) TextWriter.Write(", ");
				TextWriter.Write("f{0,2} : {1}, {2}", n, "0x%08X".Sprintf(FPR_I[n]), FPR[n]);
				if (n % 4 == 3) TextWriter.WriteLine();
			}
			TextWriter.WriteLine();
		}

		public unsafe void CopyRegistersFrom(CpuThreadState that)
		{
			this.PC = that.PC;
			this.BranchFlag = that.BranchFlag;
			this.Fcr31 = that.Fcr31;
			this.IC = that.IC;
			this.LO = that.LO;
			this.HI = that.HI;
			fixed (float* ThisFPR = &this.FPR0)
			fixed (float* ThatFPR = &that.FPR0)
			fixed (uint* ThisGPR = &this.GPR0)
			fixed (uint* ThatGPR = &that.GPR0)
			{
				for (int n = 0; n < 32; n++)
				{
					ThisFPR[n] = ThatFPR[n];
					ThisGPR[n] = ThatGPR[n];
				}
			}

			fixed (float* ThisVFR = &this.VFR0)
			fixed (float* ThatVFR = &that.VFR0)
			{
				for (int n = 0; n < 128; n++)
				{
					ThisVFR[n] = ThatVFR[n];
				}
			}
		}
	}
}

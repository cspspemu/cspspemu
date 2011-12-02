using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emiter
{
	// http://forums.ps2dev.org/viewtopic.php?t=6929 
	// http://wiki.fx-world.org/doku.php?do=index
	// http://mrmrice.fx-world.org/vfpu.html
	// http://hitmen.c02.at/files/yapspd/psp_doc/chap4.html
	// pspgl_codegen.h
	// 
	/**
	 * Before you begin messing with the vfpu, you need to do one thing in your project:
	 * PSP_MAIN_THREAD_ATTR(PSP_THREAD_ATTR_VFPU);
	 * Almost all psp applications define this in the projects main c file. It sets a value that tells the psp how to handle your applications thread
	 * in case the kernel needs to switch to another thread and back to yours. You need to add PSP_THREAD_ATTR_VFPU to this so the psp's kernel will
	 * properly save/restore the vfpu state on thread switch, otherwise bad things might happen if another thread uses the vfpu and stomps on whatever was in there.
	 *
	 * Before diving into the more exciting bits, first you need to know how the VFPU registers are configured.
	 * The vfpu contains 128 32-bit floating point registers (same format as the float type in C).
	 * These registers can be accessed individually or in groups of 2, 3, 4, 9 or 16 in one instruction.
	 * They are organized as 8 blocks of registers, 16 per block.When you write code to access these registers, there is a naming convention you must use.
	 * 
	 * Every register name has 4 characters: Xbcr
	 * 
	 * X can be one of:
	 *   M - this identifies a matrix block of 4, 9 or 16 registers
	 *   E - this identifies a transposed matrix block of 4, 9 or 16 registers
	 *   C - this identifies a column of 2, 3 or 4 registers
	 *   R - this identifies a row of 2, 3, or 4 registers
	 *   S - this identifies a single register
	 *
	 * b can be one of:
	 *   0 - register block 0
	 *   1 - register block 1
	 *   2 - register block 2
	 *   3 - register block 3
	 *   4 - register block 4
	 *   5 - register block 5
	 *   6 - register block 6
	 *   7 - register block 7
	 *
	 * c can be one of:
	 *   0 - column 0
	 *   1 - column 1
	 *   2 - column 2
	 *   3 - column 3
	 *
	 * r can be one of:
	 *   0 - row 0
	 *   1 - row 1
	 *   2 - row 2
	 *   3 - row 3
	 *
	 * So for example, the register name S132 would be a single register in column 3, row 2 in register block 1.
	 * M500 would be a matrix of registers in register block 5.
	 *
	 * Almost every vfpu instruction will end with one of the following extensions:
	 *   .s - instruction works on a single register
	 *   .p - instruction works on a 2 register vector or 2x2 matrix
	 *   .t - instruction works on a 3 register vector or 3x3 matrix
	 *   .q - instruction works on a 4 register vector or 4x4 matrix
	 * 
	 * http://wiki.fx-world.org/doku.php?id=general:vfpu_registers
	 *
	 * This is something you need to know about how to transfer data in or out of the vfpu. First lets show the instructions used to load/store data from the vfpu:
	 *   lv.s (load 1 vfpu reg from unaligned memory)
	 *   lv.q (load 4 vfpu regs from 16 byte aligned memory)
	 *   sv.s (write 1 vfpu reg to unaligned memory)
	 *   sv.q (write 4 vfpu regs to 16 byte aligned memory)
	 *
	 * There are limitations with these instructions. You can only transfer to or from column or row registers in the vfpu.
	 *
	 * You can also load values into the vfpu from a MIPS register, this will work with all single registers:
	 *   mtv (move MIPS register to vfpu register)
	 *   mfv (move from vfpu register to MIPS register)
	 *
	 * There are 2 instructions, ulv.q and usv.q, that perform unaligned ran transfers to/from the vfpu. These have been found to be faulty so it is not recommended to use them.
	 *
	 * The vfpu performs a few trig functions, but they dont behave like the normal C functions we are used to.
	 * Normally we would pass in the angle in radians from -pi/2 to +pi/2, but the vfpu wants the input value in the range of -1 to 1.
	 *
	**/

	/**
	   The VFPU contains 32 registers (128bits each, 4x32bits).

	   VFPU Registers can get accessed as Matrices, Vectors or single words.
	   All registers are overlayed and enumerated in 3 digits (Matrix/Column/Row):

		M000 | C000   C010   C020   C030	M100 | C110   C110   C120   C130
		-----+--------------------------	-----+--------------------------
		R000 | S000   S010   S020   S030	R100 | S100   S110   S120   S130
		R001 | S001   S011   S021   S031	R101 | S101   S111   S121   S131
		R002 | S002   S012   S022   S032	R102 | S102   S112   S122   S132
		R003 | S003   S013   S023   S033	R103 | S103   S113   S123   S133

	  same for matrices starting at M200 - M700.
	  Subvectors can get addressed as singles/pairs/triplets/quads.
	  Submatrices can get addressed 2x2 pairs, 3x3 triplets or 4x4 quads.

	  So Q_C010 specifies the Quad Column starting at S010, T_C011 the triple Column starting at S011.
	*/
	unsafe sealed public partial class CpuEmiter
	{

		/// <summary>
		///  vcst.[s | p | t | q] vd, VFPU_CST
		///  vd = vfpu_constant[VFPU_CST], where VFPU_CST is one of:
		///    VFPU_HUGE      infinity
		///    VFPU_SQRT2     sqrt(2)
		///    VFPU_SQRT1_2   sqrt(1/2)
		///    VFPU_2_SQRTPI  2/sqrt(pi)
		///    VFPU_PI        pi
		///    VFPU_2_PI      2/pi
		///    VFPU_1_PI      1/pi
		///    VFPU_PI_4      pi/4
		///    VFPU_PI_2      pi/2
		///    VFPU_E         e
		///    VFPU_LOG2E     log2(e)
		///    VFPU_LOG10E    log10(e)
		///    VFPU_LN2       ln(2)
		///    VFPU_LN10      ln(10)
		///    VFPU_2PI       2*pi
		///    VFPU_PI_6      pi/6
		///    VFPU_LOG10TWO  log10(2)
		///    VFPU_LOG2TEN   log2(10)
		///    VFPU_SQRT3_2   sqrt(3)/2
		/// </summary>
		static readonly float[] VfpuConstants = new float[] {
			(float)0.0f,                        /// VFPU_ZERO     - 0
			(float)float.PositiveInfinity,      /// VFPU_HUGE     - infinity
			(float)(Math.Sqrt(2.0)),            /// VFPU_SQRT2    - sqrt(2)
			(float)(Math.Sqrt(1.0 / 2.0)),      /// VFPU_SQRT1_2  - sqrt(1 / 2)
			(float)(2.0 / Math.Sqrt(Math.PI)),  /// VFPU_2_SQRTPI - 2 / sqrt(pi)
			(float)(2.0 / Math.PI),             /// VFPU_2_PI     - 2 / pi
			(float)(1.0 / Math.PI),             /// VFPU_1_PI     - 1 / pi
			(float)(Math.PI / 4.0),             /// VFPU_PI_4     - pi / 4
			(float)(Math.PI / 2.0),             /// VFPU_PI_2     - pi / 2
			(float)(Math.PI),                   /// VFPU_PI       - pi
			(float)(Math.E),                    /// VFPU_E        - e
			(float)(Math.Log(Math.E, 2)),       /// VFPU_LOG2E    - log2(E) = log(E) / log(2)
			(float)(Math.Log10(Math.E)),        /// VFPU_LOG10E   - log10(E)
			(float)(Math.Log(2)),               /// VFPU_LN2      - ln(2)
			(float)(Math.Log(10)),              /// VFPU_LN10     - ln(10)
			(float)(2.0 * Math.PI),             /// VFPU_2PI      - 2 * pi
			(float)(Math.PI / 6.0),             /// VFPU_PI_6     - pi / 6
			(float)(Math.Log10(2.0)),           /// VFPU_LOG10TWO - log10(2)
			(float)(Math.Log(10.0, 2)),         /// VFPU_LOG2TEN  - log2(10) = log(10) / log(2)
			(float)(Math.Sqrt(3.0) / 2.0)       /// VFPU_SQRT3_2  - sqrt(3) / 2
		};


		private void _call_debug_vfpu()
		{
			MipsMethodEmiter.CallMethodWithCpuThreadStateAsFirstArgument(this.GetType(), "_debug_vfpu");
		}

		static public void _debug_vfpu(CpuThreadState CpuThreadState)
		{
			Console.Error.WriteLine("");
			Console.Error.WriteLine("VPU DEBUG:");
			fixed (float* FPR = &CpuThreadState.VFR0)
			{
				int Index = 0;
				for (int Matrix = 0; Matrix < 8; Matrix++)
				{
					Console.Error.WriteLine("Matrix {0}: ", Matrix);
					for (int Row = 0; Row < 4; Row++)
					{
						for (int Column = 0; Column < 4; Column++)
						{
							Console.Error.Write("{0},", FPR[Index]);
							Index++;
						}
						Console.Error.WriteLine("");
					}
					Console.Error.WriteLine("");
				}
			}
		}

		private void _load_memory_imm14_index(uint Index)
		{
			MipsMethodEmiter._getmemptr(() =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, Instruction.IMM14 * 4 + Index * 4);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
			});
		}

		public VfpuPrefix PrefixNone;
		public VfpuPrefix PrefixSource;
		public VfpuPrefix PrefixDestination;
		public VfpuPrefix PrefixTarget;

		private void CheckPrefixUsage(VfpuPrefix Prefix)
		{
			// Disable the prefix once it have been used.
			if (Prefix.Enabled)
			{
				if (Prefix.UsedCount > 0 && Prefix.UsedPC != PC)
				{
					throw (new InvalidOperationException(
						String.Format(
							"[0] Prefix not used or not applied to the next instruction Prefix={0}, PC=0x{1:X}",
							Prefix, PC
						)
					));
				}

				if (Prefix.UsedCount > 0 && Prefix.DeclaredPC != PC)
				{
					Prefix.Enabled = false;
				}

				/*
				// Additional consideration.
				if (PC != Prefix.DeclaredPC + 4)
				{
					if (Prefix.UsedCount == 0)
					{
						throw (new InvalidOperationException(
							String.Format(
								"[1] Prefix not used or not applied to the next instruction Prefix={0}, PC=0x{1:X}",
								Prefix, PC
							)
						));
					}
					Prefix.Enabled = false;
				}
				*/
			}
		}

		/*
		if (enabled && prefix.enabled) {
			foreach (i, ref value; dst) {
				// Constant.
				if (prefix.constant(i)) {
					final switch (prefix.index(i)) {
						case 0: value = prefix.absolute(i) ? (3.0f       ) : (0.0f); break;
						case 1: value = prefix.absolute(i) ? (1.0f / 3.0f) : (1.0f); break;
						case 2: value = prefix.absolute(i) ? (1.0f / 4.0f) : (2.0f); break;
						case 3: value = prefix.absolute(i) ? (1.0f / 6.0f) : (0.5f); break;
					}
				}
				// Value
				else {
					value = *src[prefix.index(i)];
				}
				
				if (prefix.absolute(i)) value = abs(value);
				if (prefix.negate  (i)) value = -value;
			}
			prefix.enabled = false;
		} else {
			foreach (n, ref value; dst) value = *src[n];
		}
		*/

		private void VfpuLoad_Register(uint Register, int Index, uint VectorSize, VfpuPrefix Prefix, bool Debug = false)
		{
			CheckPrefixUsage(Prefix);

			if (Prefix.Enabled)
			{
				Prefix.UsedPC = PC;
				Prefix.UsedCount++;

				// Constant.
				if (Prefix.SourceConstant(Index))
				{
					float Value = 0.0f;
					switch (Prefix.SourceIndex(Index))
					{
						case 0: Value = Prefix.SourceAbsolute(Index) ? (3.0f) : (0.0f); break;
						case 1: Value = Prefix.SourceAbsolute(Index) ? (1.0f / 3.0f) : (1.0f); break;
						case 2: Value = Prefix.SourceAbsolute(Index) ? (1.0f / 4.0f) : (2.0f); break;
						case 3: Value = Prefix.SourceAbsolute(Index) ? (1.0f / 6.0f) : (0.5f); break;
						default: throw(new InvalidOperationException());
					}
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_R4, Value);
				}
				// Value.
				else
				{
					_VfpuLoadVectorWithIndexPointer(Register, (uint)Index, VectorSize, Debug);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_R4);
				}

				if (Prefix.SourceAbsolute(Index))
				{
					//MipsMethodEmiter.ILGenerator.Emit(OpCodes);
					MipsMethodEmiter.CallMethod(typeof(MathFloat), "Abs");
				}
				if (Prefix.SourceNegate(Index))
				{
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Neg);
				}
			}
			else
			{
				_VfpuLoadVectorWithIndexPointer(Register, (uint)Index, VectorSize, Debug);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_R4);
			}
		}

		/*
		if (enabled && prefix.enabled) {
			foreach (i, value; src) {
				if (prefix.mask(i)) continue;

				switch (prefix.saturation(i)) {
					case 1: value = clamp!(float)(value,  0.0, 1.0); break;
					case 3: value = clamp!(float)(value, -1.0, 1.0); break;
					default: break;
				}

				*dst[i] = value;
			}
			prefix.enabled = false;
		} else {
			foreach (i, value; src) *dst[i] = value;
		}
		*/

		private void VfpuSave_Register(uint Register, int Index, uint VectorSize, VfpuPrefix Prefix, Action Action, bool Debug = false)
		{
			CheckPrefixUsage(Prefix);
			_VfpuLoadVectorWithIndexPointer(Register, (uint)Index, VectorSize, Debug);
			{
				Action();
				if (Prefix.Enabled)
				{
					if (!Prefix.DestinationMask(Index))
					{
						float Min = 0, Max = 0;
						bool DoClamp = false;
						switch (Prefix.DestinationSaturation(Index))
						{
							case 1: DoClamp = true; Min = 0.0f; Max = 1.0f; break;
							case 3: DoClamp = true; Min = -1.0f; Max = 1.0f; break;
							default: break;
						}
						if (DoClamp)
						{
							MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_R4, Min);
							MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_R4, Max);
							MipsMethodEmiter.CallMethod(typeof(MathFloat), "Clamp");
						}
					}
				}
			}
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Stind_R4);
		}

		// VfpuPrefix VfpuPrefix, 

		private void Load_VS(int Index, uint VectorSize, int RegisterOffset = 0, bool Debug = false)
		{
			VfpuLoad_Register((uint)(Instruction.VS + RegisterOffset), Index, VectorSize, PrefixSource, Debug);
		}

		private void Load_VT(int Index, uint VectorSize, int RegisterOffset = 0, bool Debug = false)
		{
			VfpuLoad_Register((uint)(Instruction.VT + RegisterOffset), Index, VectorSize, PrefixTarget, Debug);
		}

		private void Load_VD(int Index, uint VectorSize, int RegisterOffset = 0, bool Debug = false)
		{
			//Load_Register(Instruction.VD + RegisterOffset, Index, VectorSize, PrefixNone, Debug);
			VfpuLoad_Register((uint)(Instruction.VD + RegisterOffset), Index, VectorSize, PrefixDestination, Debug);
		}

		private void Save_VD(int Index, uint VectorSize, int RegisterOffset, Action Action, bool Debug = false)
		{
			VfpuSave_Register((uint)(Instruction.VD + RegisterOffset), Index, VectorSize, PrefixDestination, Action, Debug);
		}

		private void Save_VD(int Index, uint VectorSize, Action Action, bool Debug = false)
		{
			VfpuSave_Register((uint)(Instruction.VD), Index, VectorSize, PrefixDestination, Action, Debug);
		}

		private void Save_VT(int Index, uint VectorSize, int RegisterOffset, Action Action, bool Debug = false)
		{
			VfpuSave_Register((uint)(Instruction.VT + RegisterOffset), Index, VectorSize, PrefixTarget, Action, Debug);
		}

		private void Save_VT(int Index, uint VectorSize, Action Action, bool Debug = false)
		{
			VfpuSave_Register((uint)(Instruction.VT), Index, VectorSize, PrefixTarget, Action, Debug);
		}

		IEnumerable<int> XRange(int Start, int End)
		{
			for (int Value = Start; Value < End; Value++)
			{
				yield return Value;
			}
		}

		IEnumerable<int> XRange(uint Count)
		{
			return XRange(0, (int)Count);
		}


		public enum LineType
		{
			None = 0,
			Row = 1,
			Column = 2,
		}

		// S000 <-- S(Matrix)(Column)(Row)
		private void _VfpuLoadVectorWithIndexPointer(uint Register, uint Index, uint Size, bool Debug = false)
		{
			uint Line = BitUtils.Extract(Register, 0, 2); // 0-3
			uint Matrix = BitUtils.Extract(Register, 2, 3); // 0-7
			LineType LineType = LineType.None;
			uint Row = 0;
			uint Column = 0;

			if (Size == 1)
			{
				Column = Line;
				Row = (Register >> 5) & 3;
			}
			else
			{
				uint Offset = (Register & 64) >> (int)(3 + Size);
				LineType = ((Register & 32) == 0) ? LineType.Column : LineType.Row;

				if (LineType == LineType.Row)
				{
					Column = Offset + Index;
					Row = Line;
				}
				else
				{
					Column = Line;
					Row = Offset + Index;
				}
			}

			uint RegisterIndex = Matrix * 16 + Row * 4 + Column;

			if (Debug)
			{
				char C = 'S';
				if (LineType != LineType.None)
				{
					C = (LineType == LineType.Row) ? 'R' : 'C';
				}
				Console.Error.WriteLine(
					"_VfpuLoadVectorWithIndexPointer(R={0},I={1},S={2}): " +
					"{9}{3}{4}{5} Index({1}) :: Matrix={3}, Column={4}, Row={5}, Type={6}, " +
					"RegisterIndex={7}, Line={8}",
					Register, Index, Size,
					Matrix, Column, Row, LineType,
					RegisterIndex,
					Line, C
				);
			}

			//if (Reg == null) throw(new InvalidOperationException("Invalid Vfpu register"));
			MipsMethodEmiter.LoadFieldPtr(typeof(CpuThreadState).GetField("VFR" + RegisterIndex));
		}

		private void _VectorOperation_N_Registers(uint VectorSize, int InputCount, Action<uint> Action)
		{
			foreach (var Index in XRange(VectorSize))
			{
				Save_VD(Index, VectorSize, () =>
				{
					if (InputCount >= 1) Load_VS(Index, VectorSize);
					if (InputCount >= 2) Load_VT(Index, VectorSize);
					Action((uint)Index);
				});
			}
		}

		private void VectorOperationSaveVd(Action<uint, Action<int>> Action)
		{
			VectorOperationSaveVd(Instruction.ONE_TWO, Action);
		}

		private void VectorOperationSaveVd(uint VectorSize, Action<uint, Action<int>> Action)
		{
			foreach (var Index in XRange(VectorSize))
			{
				Action<int> Load = InputCount =>
				{
					if (InputCount >= 1) Load_VS(Index, VectorSize);
					if (InputCount >= 2) Load_VT(Index, VectorSize);
				};
				Save_VD(Index, VectorSize, () =>
				{
					Action((uint)Index, Load);
				});
			}
		}

		/*
		private void _VectorOperation0Registers(Action<uint> Action)
		{
			_VectorOperation_N_Registers(Instruction.ONE_TWO, 0, Action);
		}

		private void _VectorOperation1Registers(Action<uint> Action)
		{
			_VectorOperation_N_Registers(Instruction.ONE_TWO, 1, Action);
		}

		private void _VectorOperation2Registers(Action<uint> Action)
		{
			_VectorOperation_N_Registers(Instruction.ONE_TWO, 2, Action);
		}
		*/

		/*
		private void _VectorOperation0Registers(uint VectorSize, Action<uint> Action)
		{
			_VectorOperation_N_Registers(VectorSize, 0, Action);
		}

		private void _VectorOperation1Registers(uint VectorSize, Action<uint> Action)
		{
			_VectorOperation_N_Registers(VectorSize, 1, Action);
		}

		private void _VectorOperation2Registers(uint VectorSize, Action<uint> Action)
		{
			_VectorOperation_N_Registers(VectorSize, 2, Action);
		}
		*/
	}

	public struct VfpuPrefix
	{
		public uint DeclaredPC;
		public uint UsedPC;
		public uint Value;
		public bool Enabled;
		public int UsedCount;

		// swz(xyzw)
		public uint SourceIndex(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (0 + i * 2)) & 3;
			return BitUtils.Extract(Value, 0 + i * 2, 2);
		}

		// abs(xyzw)
		public bool SourceAbsolute(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (8 + i * 1)) & 1;
			return BitUtils.Extract(Value, 8 + i * 1, 1) != 0;
		}

		// cst(xyzw)
		public bool SourceConstant(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (12 + i * 1)) & 1;
			return BitUtils.Extract(Value, 12 + i * 1, 1) != 0;
		}

		// neg(xyzw)
		public bool SourceNegate(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (16 + i * 1)) & 1;
			return BitUtils.Extract(Value, 16 + i * 1, 1) != 0;
		}

		// sat(xyzw)
		public uint DestinationSaturation(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (0 + i * 2)) & 3;
			return BitUtils.Extract(Value, 0 + i * 2, 2);
		}

		// msk(xyzw)
		public bool DestinationMask(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (8 + i * 1)) & 1;
			return BitUtils.Extract(Value, 8 + i * 1, 1) != 0;
		}

		public void EnableAndSetValueAndPc(uint Value, uint PC)
		{
			this.Enabled = true;
			this.Value = Value;
			this.DeclaredPC = PC;
			this.UsedCount = 0;
		}

		public override string ToString()
		{
			return String.Format(
				"VfpuPrefix(Enabled={0}, UsedPC=0x{1:X}, DeclaredPC=0x{2:X})",
				Enabled, UsedPC, DeclaredPC
			);
		}
	}

}

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

		private void Load_Register(uint Register, uint Index, uint VectorSize, bool Debug)
		{
			_VfpuLoadVectorWithIndexPointer(Register, Index, VectorSize, Debug);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_R4);
		}

		private void Save_Register(uint Register, uint Index, uint VectorSize, Action Action, bool Debug)
		{
			_VfpuLoadVectorWithIndexPointer(Register, Index, VectorSize, Debug);
			Action();
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Stind_R4);
		}

		private void Load_VS(uint Index, uint VectorSize, bool Debug = false)
		{
			Load_Register(Instruction.VS, Index, VectorSize, Debug);
		}

		private void Load_VT(uint Index, uint VectorSize, bool Debug = false)
		{
			Load_Register(Instruction.VT, Index, VectorSize, Debug);
		}

		private void Save_VD(uint Index, uint VectorSize, Action Action, bool Debug = false)
		{
			Save_Register(Instruction.VD, Index, VectorSize, Action, Debug);
		}

		private void Save_VT(uint Index, uint VectorSize, Action Action, bool Debug = false)
		{
			Save_Register(Instruction.VT, Index, VectorSize, Action, Debug);
		}

		IEnumerable<uint> XRange(uint Start, uint End)
		{
			for (uint Value = Start; Value < End; Value++)
			{
				yield return Value;
			}
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

/*
		bool order  = void;

		if (row.length == 1) {
			offset = (vx >> 5) & 3;
			order  = false;
		} else {
			offset = (vx & 64) >> (3 + row.length);
			order = ((vx & 32) != 0);
		}
		
		if (order) foreach (n, ref value; row) value = &registers.VF_CELLS[matrix][offset + n][line];
		else       foreach (n, ref value; row) value = &registers.VF_CELLS[matrix][line][offset + n];
 */

			if (Debug)
			{
				char C = 'S';
				if (LineType != LineType.None)
				{
					C = (LineType == LineType.Row) ? 'R' : 'C';
				}
				Console.Error.WriteLine(
					"_VfpuLoadVectorWithIndexPointer(R={0},I={1},S={2}): " +
					"{9}{3}{4}{5} :: Matrix={3}, Column={4}, Row={5}, Type={6}, " +
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
			foreach (var Index in XRange(0, VectorSize))
			{
				Save_VD(Index, VectorSize, () =>
				{
					if (InputCount >= 1) Load_VS(Index, VectorSize);
					if (InputCount >= 2) Load_VT(Index, VectorSize);
					Action(Index);
				});
			}
		}

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
	}
}

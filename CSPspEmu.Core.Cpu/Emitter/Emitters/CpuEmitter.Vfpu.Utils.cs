using System;
using System.Collections.Generic;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emitter
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

		M000 | C000   C010   C020   C030	M100 | C100   C110   C120   C130
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
	public unsafe sealed partial class CpuEmitter
	{
		private void _call_debug_vfpu()
		{
			throw (new NotImplementedException("_call_debug_vfpu"));
			//MipsMethodEmitter.CallMethodWithCpuThreadStateAsFirstArgument(this.GetType(), "_debug_vfpu");
		}

		public static void _debug_vfpu(CpuThreadState CpuThreadState)
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
			throw (new NotImplementedException("_load_memory_imm14_index"));
			//MipsMethodEmitter._getmemptr(() =>
			//{
			//	MipsMethodEmitter.LoadGPR_Unsigned(RS);
			//	SafeILGenerator.Push((int)(Instruction.IMM14 * 4 + Index * 4));
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			//}, Safe: true, CanBeNull: false);
		}

		public VfpuPrefix PrefixNone = new VfpuPrefix();
		public VfpuPrefix PrefixSource = new VfpuPrefix();
		public VfpuPrefix PrefixTarget = new VfpuPrefix();
		public VfpuDestinationPrefix PrefixDestinationNone = new VfpuDestinationPrefix();
		public VfpuDestinationPrefix PrefixDestination = new VfpuDestinationPrefix();

		static private AstNodeExprLValue VFR(int Index)
		{
			return REG("VFR" + Index);
		}


		internal abstract class VfpuRuntimeRegister
		{
			protected uint PC;
			protected Instruction Instruction;
			protected VReg VReg;
			protected VType VType;
			protected int VectorSize;

			protected VfpuRuntimeRegister(CpuEmitter CpuEmitter, VReg VReg, VType VType, int VectorSize)
			{
				this.PC = CpuEmitter.PC;
				this.Instruction = CpuEmitter.Instruction;
				this.VReg = VReg;
				this.VType = VType;
				this.VectorSize = (VectorSize == 0) ? (int)Instruction.ONE_TWO : VectorSize;
			}

			private Type GetVTypeType()
			{
				switch (VType)
				{
					case CpuEmitter.VType.VFloat: return typeof(float);
					case CpuEmitter.VType.VInt: return typeof(int);
					case CpuEmitter.VType.VUInt: return typeof(uint);
					default: throw (new InvalidCastException("Invalid VType " + VType));
				}
			}

			protected AstNodeExprLValue _GetVRegRef(int RegIndex)
			{
				if (GetVTypeType() == typeof(float)) return VFR(RegIndex);
				return ast.Reinterpret(GetVTypeType(), VFR(RegIndex));
			}

			protected AstNodeExpr GetRegApplyPrefix(int[] Indices, int RegIndex, int PrefixIndex)
			{
				var Prefix = VReg.VfpuPrefix;
				Prefix.CheckPrefixUsage(PC);

				AstNodeExpr AstNodeExpr = _GetVRegRef(Indices[RegIndex]);

				if (Prefix.Enabled && Prefix.IsValidIndex(PrefixIndex))
				{
					// Constant.
					if (Prefix.SourceConstant(PrefixIndex))
					{
						float Value = 0.0f;
						switch (Prefix.SourceIndex(PrefixIndex))
						{
							case 0: Value = Prefix.SourceAbsolute(PrefixIndex) ? (3.0f) : (0.0f); break;
							case 1: Value = Prefix.SourceAbsolute(PrefixIndex) ? (1.0f / 3.0f) : (1.0f); break;
							case 2: Value = Prefix.SourceAbsolute(PrefixIndex) ? (1.0f / 4.0f) : (2.0f); break;
							case 3: Value = Prefix.SourceAbsolute(PrefixIndex) ? (1.0f / 6.0f) : (0.5f); break;
							default: throw (new InvalidOperationException("Invalid SourceIndex"));
						}

						AstNodeExpr = ast.Cast(GetVTypeType(), Value);
					}
					// Value.
					else
					{
						AstNodeExpr = _GetVRegRef(Indices[(int)Prefix.SourceIndex(PrefixIndex)]);
						if (Prefix.SourceAbsolute(PrefixIndex)) AstNodeExpr = ast.CallStatic((Func<float, float>)MathFloat.Abs, AstNodeExpr);
					}

					if (Prefix.SourceNegate(PrefixIndex)) AstNodeExpr = ast.Unary("-", AstNodeExpr);
				}

				return AstNodeExpr;
			}

			protected AstNodeStm SetRegApplyPrefix(int RegIndex, int PrefixIndex, AstNodeExpr AstNodeExpr)
			{
				var PrefixDestination = VReg.VfpuDestinationPrefix;
				PrefixDestination.CheckPrefixUsage(PC);

				if (PrefixDestination.Enabled && PrefixDestination.IsValidIndex(PrefixIndex))
				{
					if (!PrefixDestination.DestinationMask(PrefixIndex))
					{
						float Min = 0, Max = 0;
						bool DoClamp = false;
						switch (PrefixDestination.DestinationSaturation(PrefixIndex))
						{
							case 1: DoClamp = true; Min = 0.0f; Max = 1.0f; break;
							case 3: DoClamp = true; Min = -1.0f; Max = 1.0f; break;
							default: break;
						}

						if (DoClamp)
						{
							if (VType == CpuEmitter.VType.VFloat)
							{
								AstNodeExpr = ast.CallStatic((Func<float, float, float, float>)MathFloat.Clamp, AstNodeExpr, (float)Min, (float)Max);
							}
							else
							{
								AstNodeExpr = ast.Cast(GetVTypeType(), ast.CallStatic((Func<int, int, int, int>)MathFloat.ClampInt, AstNodeExpr, (int)Min, (int)Max));
							}
						}
					}
				}

				return ast.Assign(_GetVRegRef(RegIndex), AstNodeExpr);
			}

		}

		internal sealed class VfpuCell : VfpuRuntimeRegister
		{
			private int Index;

			public VfpuCell(CpuEmitter CpuEmitter, VReg VReg, VType VType)
				: base(CpuEmitter, VReg, VType, 1)
			{
				this.Index = VfpuUtils.GetIndexCell(VReg.Reg);
			}

			public AstNodeExpr Get()
			{
				return GetRegApplyPrefix(new int[] { this.Index }, 0, 0);
			}

			public AstNodeStm Set(AstNodeExpr Value)
			{
				return SetRegApplyPrefix(this.Index, 0, Value);
			}
		}

		internal sealed class VfpuVector : VfpuRuntimeRegister
		{
			private int[] Indices;

			public VfpuVector(CpuEmitter CpuEmitter, VReg VReg, VType VType, int VectorSize)
				: base(CpuEmitter, VReg, VType, VectorSize)
			{
				this.Indices = VfpuUtils.GetIndicesVector(this.VectorSize, VReg.Reg);
			}

			public AstNodeExpr this[int Index]
			{
				get { return Get(Index); }
			}

			public AstNodeExprLValue GetIndexRef(int Index)
			{
				return this._GetVRegRef(Indices[Index]);
			}

			private AstNodeExpr Get(int Index)
			{
				return GetRegApplyPrefix(this.Indices, Index, Index);
			}

			public AstNodeStm Set(int Index, AstNodeExpr Value)
			{
				return SetRegApplyPrefix(this.Indices[Index], Index, Value);
			}

			public AstNodeStm SetVector(Func<int, AstNodeExpr> Generator)
			{
				var Statements = new List<AstNodeStm>();
				for (int Index = 0; Index < this.VectorSize; Index++)
				{
					Statements.Add(SetRegApplyPrefix(this.Indices[Index], Index, Generator(Index)));
				}
				return ast.Statements(Statements);
			}
		}

		internal sealed class VfpuMatrix : VfpuRuntimeRegister
		{
			private int[,] Indices;

			public VfpuMatrix(CpuEmitter CpuEmitter, VReg VReg, VType VType, int VectorSize)
				: base(CpuEmitter, VReg, VType, VectorSize)
			{
				this.Indices = VfpuUtils.GetIndicesMatrix(this.VectorSize, VReg.Reg);
			}

			public AstNodeExpr this[int Column, int Row]
			{
				get { return Get(Column, Row); }
			}

			public AstNodeExprLValue GetIndexRef(int Column, int Row)
			{
				return this._GetVRegRef(Indices[Column, Row]);
			}

			private AstNodeExpr Get(int Column, int Row)
			{
				return _GetVRegRef(this.Indices[Column, Row]);
				//return GetRegApplyPrefix(this.Indices[Column, Row], -1);
			}

			public AstNodeStm Set(int Column, int Row, AstNodeExpr Value)
			{
				return SetRegApplyPrefix(this.Indices[Column, Row], -1, Value);
			}

			public AstNodeStm SetMatrix(Func<int, int, AstNodeExpr> Generator)
			{
				var Statements = new List<AstNodeStm>();
				for (int Row = 0; Row < VectorSize; Row++)
				for (int Column = 0; Column < VectorSize; Column++)
				{
					Statements.Add(Set(Column, Row, Generator(Column, Row)));
				}
				return ast.Statements(Statements);
			}
		}

		public class VReg
		{
			public VfpuRegisterInt Reg;
			public VfpuDestinationPrefix VfpuDestinationPrefix;
			public VfpuPrefix VfpuPrefix;
		}

		public enum VType
		{
			VFloat,
			VInt,
			VUInt,
		}

		private VType VInt { get { return VType.VInt; } }
		private VType VUInt { get { return VType.VUInt; } }
		private VType VFloat { get { return VType.VFloat; } }

		private VReg VD { get { return new VReg() { Reg = Instruction.VD, VfpuPrefix = PrefixNone, VfpuDestinationPrefix = PrefixDestination }; } }
		private VReg VS { get { return new VReg() { Reg = Instruction.VS, VfpuPrefix = PrefixSource, VfpuDestinationPrefix = PrefixDestinationNone }; } }
		private VReg VT { get { return new VReg() { Reg = Instruction.VT, VfpuPrefix = PrefixTarget, VfpuDestinationPrefix = PrefixDestinationNone }; } }
		private VReg VT5_1 { get { return new VReg() { Reg = Instruction.VT5_1, VfpuPrefix = PrefixNone, VfpuDestinationPrefix = PrefixDestinationNone }; } }
		private VReg VT5_2 { get { return new VReg() { Reg = Instruction.VT5_2, VfpuPrefix = PrefixNone, VfpuDestinationPrefix = PrefixDestinationNone }; } }

		private VReg VD_NoPrefix { get { return new VReg() { Reg = Instruction.VD, VfpuPrefix = PrefixNone, VfpuDestinationPrefix = PrefixDestinationNone }; } }
		private VReg VS_NoPrefix { get { return new VReg() { Reg = Instruction.VS, VfpuPrefix = PrefixNone, VfpuDestinationPrefix = PrefixDestinationNone }; } }
		private VReg VT_NoPrefix { get { return new VReg() { Reg = Instruction.VT, VfpuPrefix = PrefixNone, VfpuDestinationPrefix = PrefixDestinationNone }; } }

		private VfpuCell _Cell(VReg VReg, VType VType = VType.VFloat) { return new VfpuCell(this, VReg, VType); }
		private VfpuVector _Vector(VReg VReg, VType VType = VType.VFloat, int Size = 0) { return new VfpuVector(this, VReg, VType, Size); }
		private VfpuMatrix _Matrix(VReg VReg, VType VType = VType.VFloat, int Size = 0) { return new VfpuMatrix(this, VReg, VType, Size); }
	}
}

using System;
using CSharpUtils;
using SafeILGenerator;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		public enum VfpuControlRegistersEnum
		{
			/// <summary>
			/// Source prefix stack
			/// </summary>
			VFPU_PFXS = 128,

			/// <summary>
			/// Target prefix stack
			/// </summary>
			VFPU_PFXT = 129,

			/// <summary>
			/// Destination prefix stack
			/// </summary>
			VFPU_PFXD = 130,

			/// <summary>
			/// Condition information
			/// </summary>
			VFPU_CC = 131,

			/// <summary>
			/// VFPU internal information 4
			/// </summary>
			VFPU_INF4 = 132,

			/// <summary>
			/// Not used (reserved)
			/// </summary>
			VFPU_RSV5 = 133,

			/// <summary>
			/// Not used (reserved)
			/// </summary>
			VFPU_RSV6 = 134,

			/// <summary>
			/// VFPU revision information
			/// </summary>
			VFPU_REV  = 135,

			/// <summary>
			/// Pseudorandom number generator information 0
			/// </summary>
			VFPU_RCX0 = 136,

			/// <summary>
			/// Pseudorandom number generator information 1
			/// </summary>
			VFPU_RCX1 = 137,

			/// <summary>
			/// Pseudorandom number generator information 2
			/// </summary>
			VFPU_RCX2 = 138,

			/// <summary>
			/// Pseudorandom number generator information 3
			/// </summary>
			VFPU_RCX3 = 139,

			/// <summary>
			/// Pseudorandom number generator information 4
			/// </summary>
			VFPU_RCX4 = 140,

			/// <summary>
			/// Pseudorandom number generator information 5
			/// </summary>
			VFPU_RCX5 = 141,
	
			/// <summary>
			/// Pseudorandom number generator information 6
			/// </summary>
			VFPU_RCX6 = 142,

			/// <summary>
			/// Pseudorandom number generator information 7
			/// </summary>
			VFPU_RCX7 = 143,
		}

		// Vfpu DOT product
		// Vfpu SCaLe/ROTate
		/// <summary>
		/// ID("vdot",        VM("011001:001:vt:two:vs:one:vd"), "%zs, %yp, %xp", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public AstNodeStm vdot()
		{
			throw(new NotImplementedException());
			//uint VectorSize = Instruction.ONE_TWO;
			//if (VectorSize == 1)
			//{
			//	throw (new NotImplementedException(""));
			//}
			//_VfpuLoadVectorWithIndexPointer(Instruction.VD, 0, 1);
			//{
			//	SafeILGenerator.Push((float)0.0f);
			//	for (uint Index = 0; Index < VectorSize; Index++)
			//	{
			//		_VfpuLoadVectorWithIndexPointer(Instruction.VS, Index, VectorSize);
			//		SafeILGenerator.LoadIndirect<float>();
			//
			//		_VfpuLoadVectorWithIndexPointer(Instruction.VT, Index, VectorSize);
			//		SafeILGenerator.LoadIndirect<float>();
			//
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			//	}
			//}
			//SafeILGenerator.StoreIndirect<float>();
		}
		public AstNodeStm vscl()
		{
			throw(new NotImplementedException());
			//uint VectorSize = Instruction.ONE_TWO;
			//if (VectorSize == 1)
			//{
			//	throw (new NotImplementedException(""));
			//}
			//
			//foreach (var Index in XRange(VectorSize))
			//{
			//	Save_VD(Index, VectorSize, () =>
			//	{
			//		Load_VS(Index, VectorSize);
			//		Load_VT(0, 1);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//	});
			//}
		}

		/// <summary>
		/// Vector ROTate
		/// </summary>
		public AstNodeStm vrot()
		{
			var VectorSize = Instruction.ONE_TWO;
			uint imm5 = Instruction.IMM5;
			var CosIndex = BitUtils.Extract(imm5, 0, 2);
			var SinIndex = BitUtils.Extract(imm5, 2, 2);
			bool NegateSin = BitUtils.ExtractBool(imm5, 4);

			AstNodeExpr Sine = ast.CallStatic((Func<float, float>)MathFloat.SinV1, AstLoadVs(0));
			AstNodeExpr Cosine = ast.CallStatic((Func<float, float>)MathFloat.CosV1, AstLoadVs(0));
			if (NegateSin) Sine = -Sine;

			//Console.WriteLine("{0},{1},{2}", CosIndex, SinIndex, NegateSin);

			return AstVfpuStoreVd((Index) =>
			{
				if (Index == CosIndex)
				{
					return Cosine;
				}
				else if (Index == SinIndex)
				{
					return Sine;
				}
				else
				{
					return (SinIndex == CosIndex) ? Cosine : Sine;
				}
			});
		}

		// vzero: Vector ZERO
		// vone : Vector ONE
		public AstNodeStm vzero() { return AstVfpuStoreVd((Index) => 0f);  }
		public AstNodeStm vone() { return AstVfpuStoreVd((Index) => 1f); }

		// vmov  : Vector MOVe
		// vsgn  : Vector SiGN
		// *     : Vector Reverse SQuare root/COSine/Arc SINe/LOG2
		// @CHECK
		public AstNodeStm vmov() { return AstVfpuStoreVd((Index) => AstLoadVs(Index)); }
		public AstNodeStm vabs() { return AstVfpuStoreVd((Index) => ast.CallStatic((Func<float, float>)MathFloat.Abs, AstLoadVs(Index))); }
		public AstNodeStm vneg() { return AstVfpuStoreVd((Index) => -AstLoadVs(Index)); }
		public AstNodeStm vocp() { return AstVfpuStoreVd((Index) => 1f - AstLoadVs(Index)); }
		public AstNodeStm vsgn() { return AstVfpuStoreVd((Index) => ast.CallStatic((Func<float, float>)MathFloat.Sign, AstLoadVs(Index))); }
		public AstNodeStm vrcp() { return AstVfpuStoreVd((Index) => 1f / AstLoadVs(Index)); }

		private AstNodeStm _vfpu_call_single_method(Delegate Delegate)
		{
			throw(new NotImplementedException());
			//VectorOperationSaveVd((Index) =>
			//{
			//	Load_VS(Index);
			//	MipsMethodEmitter.CallMethod(Delegate);
			//});
		}

		// OP_V_INTERNAL_IN_N!(1, "1.0f / sqrt(v)");
		// vcst: Vfpu ConSTant
		public AstNodeStm vsqrt() { return _vfpu_call_single_method((Func<float, float>)MathFloat.Sqrt); }
		public AstNodeStm vrsq() { return _vfpu_call_single_method((Func<float, float>)MathFloat.RSqrt); }
		public AstNodeStm vsin() { return _vfpu_call_single_method((Func<float, float>)MathFloat.SinV1); }
		public AstNodeStm vcos() { return _vfpu_call_single_method((Func<float, float>)MathFloat.CosV1); }
		public AstNodeStm vexp2() { return _vfpu_call_single_method((Func<float, float>)MathFloat.Exp2); }
		public AstNodeStm vlog2() { return _vfpu_call_single_method((Func<float, float>)MathFloat.Log2); }
		public AstNodeStm vasin() { return _vfpu_call_single_method((Func<float, float>)MathFloat.AsinV1); }
		public AstNodeStm vnrcp() { throw (new NotImplementedException("")); }
		public AstNodeStm vnsin() { throw (new NotImplementedException("")); }
		public AstNodeStm vrexp2() { throw (new NotImplementedException("")); }
		public AstNodeStm vsat0() { return _vfpu_call_single_method((Func<float, float>)MathFloat.Vsat0); }
		public AstNodeStm vsat1() { return _vfpu_call_single_method((Func<float, float>)MathFloat.Vsat1); }
		public AstNodeStm vcst() { return AstVfpuStoreVd((Index) => ast.Immediate(VfpuUtils.GetVfpuConstantsValue((int)Instruction.IMM5))); }

		// -
		public AstNodeStm vhdp()
		{
			throw(new NotImplementedException());
			//var VectorSize = Instruction.ONE_TWO;
			//
			//VectorOperationSaveVd(1, (Index) =>
			//{
			//	SafeILGenerator.Push(0.0f);
			//	for (int n = 0; n < VectorSize - 1; n++)
			//	{
			//		Load_VS(n);
			//		Load_VT(n);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			//	}
			//
			//	Load_VT((int)(VectorSize - 1));
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			//});
		}

		public AstNodeStm vcrs_t()
		{
			throw (new NotImplementedException());
			//uint VectorSize = 3;
			//
			//Save_VD(0, VectorSize, () =>
			//{
			//	Load_VS(1, VectorSize);
			//	Load_VT(2, VectorSize);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//});
			//
			//Save_VD(1, VectorSize, () =>
			//{
			//	Load_VS(2, VectorSize);
			//	Load_VT(0, VectorSize);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//});
			//
			//Save_VD(2, VectorSize, () =>
			//{
			//	Load_VS(0, VectorSize);
			//	Load_VT(1, VectorSize);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//});
		}

		/// <summary>
		/// Cross product
		/// </summary>
		public AstNodeStm vcrsp_t()
		{
			throw(new NotImplementedException());
			//uint VectorSize = 3;
			//
			//Save_VD(0, VectorSize, () =>
			//{
			//	Load_VS(1, VectorSize);
			//	Load_VT(2, VectorSize);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//	Load_VS(2, VectorSize);
			//	Load_VT(1, VectorSize);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
			//});
			//
			//Save_VD(1, VectorSize, () =>
			//{
			//	Load_VS(2, VectorSize);
			//	Load_VT(0, VectorSize);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//	Load_VS(0, VectorSize);
			//	Load_VT(2, VectorSize);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
			//});
			//
			//Save_VD(2, VectorSize, () =>
			//{
			//	Load_VS(0, VectorSize);
			//	Load_VT(1, VectorSize);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//	Load_VS(1, VectorSize);
			//	Load_VT(0, VectorSize);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned);
			//});
			///*
			//v3[0] = +v1[1] * v2[2] - v1[2] * v2[1];
			//v3[1] = +v1[2] * v2[0] - v1[0] * v2[2];
			//v3[2] = +v1[0] * v2[1] - v1[1] * v2[0];
			//throw (new NotImplementedException(""));
			//*/
		}

		// Vfpu MINimum/MAXium/ADD/SUB/DIV/MUL
		public AstNodeStm vmin()
		{
			throw(new NotImplementedException());
			//VectorOperationSaveVd((Index) =>
			//{
			//	Load_VS_VT(Index);
			//	MipsMethodEmitter.CallMethod((Func<float, float, float>)MathFloat.Min);
			//});
		}
		public AstNodeStm vmax()
		{
			throw (new NotImplementedException());
			//VectorOperationSaveVd((Index) =>
			//{
			//	Load_VS_VT(Index);
			//	MipsMethodEmitter.CallMethod((Func<float, float, float>)MathFloat.Max);
			//});
		}

		/// <summary>
		/// 	+----------------------+--------------+----+--------------+---+--------------+ <para/>
		///     |31                 23 | 22        16 | 15 | 14         8 | 7 | 6         0  | <para/>
		///     +----------------------+--------------+----+--------------+---+--------------+ <para/>
		///     |  opcode 0x60000000   | vfpu_rt[6-0] |    | vfpu_rs[6-0] |   | vfpu_rd[6-0] | <para/>
		///     +----------------------+--------------+----+--------------+---+--------------+ <para/>
		///     
		///     VectorAdd.Single/Pair/Triple/Quad
		///     
		///     vadd.s %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Add Single <para/>
		///     vadd.p %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Add Pair <para/>
		///     vadd.t %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Add Triple <para/>
		///     vadd.q %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Add Quad <para/>
		///     <para/>
		///     %vfpu_rt:	VFPU Vector Source Register ([s|p|t|q]reg 0..127) <para/>
		///     %vfpu_rs:	VFPU Vector Source Register ([s|p|t|q]reg 0..127) <para/>
		///     %vfpu_rd:	VFPU Vector Destination Register ([s|p|t|q]reg 0..127) <para/>
		///     <para/>
		///     vfpu_regs[%vfpu_rd] &lt;- vfpu_regs[%vfpu_rs] + vfpu_regs[%vfpu_rt]
		/// </summary>
		public AstNodeStm vadd() { return AstVfpuStoreVd((Index) => AstLoadVs(Index) + AstLoadVt(Index)); }
		public AstNodeStm vsub() { return AstVfpuStoreVd((Index) => AstLoadVs(Index) - AstLoadVt(Index)); }
		public AstNodeStm vdiv() { return AstVfpuStoreVd((Index) => AstLoadVs(Index) / AstLoadVt(Index)); }
		public AstNodeStm vmul() { return AstVfpuStoreVd((Index) => AstLoadVs(Index) * AstLoadVt(Index)); }

		AstNodeStm _vidt_x(uint VectorSize, uint Register)
		{
			throw(new NotImplementedException());
			//uint IndexOne = BitUtils.Extract(Register, 0, 2);
			//foreach (var Index in XRange(VectorSize))
			//{
			//	VfpuSave_Register(Register, Index, VectorSize, PrefixDestinationNone, () =>
			//	{
			//		SafeILGenerator.Push((float)((Index == IndexOne) ? 1.0f : 0.0f));
			//	}
			//	//, Debug: true
			//	);
			//}
		}

		AstNodeStm _vzero_x(uint VectorSize, uint Register)
		{
			throw (new NotImplementedException());
			//uint IndexOne = BitUtils.Extract(Register, 0, 2);
			//foreach (var Index in XRange(VectorSize))
			//{
			//	VfpuSave_Register(Register, Index, VectorSize, PrefixDestinationNone, () =>
			//	{
			//		SafeILGenerator.Push((float) 0.0f);
			//	}
			//		//, Debug: true
			//	);
			//}
		}

		// Vfpu (Matrix) IDenTity
		public AstNodeStm vidt()
		{
			var MatrixSize = Instruction.ONE_TWO;
			return _vidt_x(MatrixSize, (uint)(Instruction.VD));
			//throw (new NotImplementedException(""));
		}

		// Vfpu load Integer IMmediate
		public AstNodeStm viim() { return AstSaveVfpuReg(Instruction.VT, 0, 1, ref PrefixNone, (float)Instruction.IMM); }
		public AstNodeStm vdet() { throw (new NotImplementedException("vdet")); }
		public AstNodeStm mfvme() { throw (new NotImplementedException("mfvme")); }
		public AstNodeStm mtvme() { throw (new NotImplementedException("mtvme")); }

		/// <summary>
		/// ID("vfim",        VM("110111:11:1:vt:imm16"), "%xs, %vh",      ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public AstNodeStm vfim()
		{
			throw(new NotImplementedException());
			//_VfpuLoadVectorWithIndexPointer(Instruction.VT, 0, 1);
			//SafeILGenerator.Push((float)Instruction.IMM_HF);
			//SafeILGenerator.StoreIndirect<float>();

			//_call_debug_vfpu();
		}


		public AstNodeStm vlgb() { throw (new NotImplementedException("")); }
		public AstNodeStm vsbn() { throw (new NotImplementedException("")); }

		public AstNodeStm vsbz() { throw (new NotImplementedException("")); }
		public AstNodeStm vsocp() { throw (new NotImplementedException("")); }
		public AstNodeStm vus2i() { throw (new NotImplementedException("")); }

		public AstNodeStm vwbn() { throw (new NotImplementedException("")); }
	}
}
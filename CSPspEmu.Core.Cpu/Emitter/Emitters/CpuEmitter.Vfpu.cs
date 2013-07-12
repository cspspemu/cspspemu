using System;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;
using System.Linq;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		// Vfpu DOT product
		// Vfpu SCaLe/ROTate
		/// <summary>
		/// ID("vdot",        VM("011001:001:vt:two:vs:one:vd"), "%zs, %yp, %xp", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public AstNodeStm vdot()
		{
			var Src = _Vector(VS);
			var Target = _Vector(VT);

			var Value = (AstNodeExpr)0f;
			foreach (var Index in Enumerable.Range(0, ONE_TWO)) Value += Src[Index] * Target[Index];
			return _Cell(VD).Set(Value);
		}

		public AstNodeStm vscl()
		{
			var Dest = _Vector(VD);
			var Src = _Vector(VS);
			var Target = _Cell(VT);

			return Dest.SetVector(Index => Src[Index] * Target.Get());
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

			var Dest = _Vector(VD, VFloat, VectorSize);
			var Src = _Cell(VS, VFloat);

			AstNodeExpr Sine = ast.CallStatic((Func<float, float>)MathFloat.SinV1, Src.Get());
			AstNodeExpr Cosine = ast.CallStatic((Func<float, float>)MathFloat.CosV1, Src.Get());
			if (NegateSin) Sine = -Sine;

			//Console.WriteLine("{0},{1},{2}", CosIndex, SinIndex, NegateSin);

			return Dest.SetVector((Index) =>
			{
				if (Index == CosIndex) return Cosine;
				if (Index == SinIndex) return Sine;
				//return (SinIndex == CosIndex) ? Cosine : Sine;
				return 0f;
			});
		}

		// vzero: Vector ZERO
		// vone : Vector ONE
		public AstNodeStm vzero() { return _Vector(VD).SetVector((Index) => 0f);  }
		public AstNodeStm vone() { return _Vector(VD).SetVector((Index) => 1f); }

		// vmov  : Vector MOVe
		// vsgn  : Vector SiGN
		// *     : Vector Reverse SQuare root/COSine/Arc SINe/LOG2
		// @CHECK
		public AstNodeStm vmov() { return _Vector(VD).SetVector((Index) => _Vector(VS)[Index]); }
		public AstNodeStm vabs() { return _Vector(VD).SetVector((Index) => ast.CallStatic((Func<float, float>)MathFloat.Abs, _Vector(VS)[Index])); }
		public AstNodeStm vneg() { return _Vector(VD).SetVector((Index) => -_Vector(VS)[Index]); }
		public AstNodeStm vocp() { return _Vector(VD).SetVector((Index) => 1f - _Vector(VS)[Index]); }
		public AstNodeStm vsgn() { return _Vector(VD).SetVector((Index) => ast.CallStatic((Func<float, float>)MathFloat.Sign, _Vector(VS)[Index])); }
		public AstNodeStm vrcp() { return _Vector(VD).SetVector((Index) => 1f / _Vector(VS)[Index]); }

		private AstNodeStm _vfpu_call_single_method(Delegate Delegate)
		{
			return _Vector(VD).SetVector((Index) => ast.CallStatic(Delegate, _Vector(VS)[Index]));
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
		public AstNodeStm vcst() { return _Cell(VD).Set(VfpuUtils.GetVfpuConstantsValue((int)Instruction.IMM5)); }

		// -
		public AstNodeStm vhdp()
		{
			return ast.AstNotImplemented("vhdp");
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
			return ast.AstNotImplemented("vcrs_t");
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
			return ast.AstNotImplemented("vcrsp_t");
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
			//*
			//v3[0] = +v1[1] * v2[2] - v1[2] * v2[1];
			//v3[1] = +v1[2] * v2[0] - v1[0] * v2[2];
			//v3[2] = +v1[0] * v2[1] - v1[1] * v2[0];
			//throw (new NotImplementedException(""));
			//*/
		}

		// Vfpu MINimum/MAXium/ADD/SUB/DIV/MUL
		public AstNodeStm vmin()
		{
			return _Vector(VD).SetVector((Index) =>
				ast.CallStatic(
					(Func<float, float, float>)MathFloat.Min,
					_Vector(VS)[Index],
					_Vector(VT)[Index]
				)
			);
		}
		public AstNodeStm vmax()
		{
			return _Vector(VD).SetVector((Index) =>
				ast.CallStatic(
					(Func<float, float, float>)MathFloat.Max,
					_Vector(VS)[Index],
					_Vector(VT)[Index]
				)
			);
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
		public AstNodeStm vadd() { return _Vector(VD).SetVector((Index) => _Vector(VS)[Index] + _Vector(VT)[Index]); }
		public AstNodeStm vsub() { return _Vector(VD).SetVector((Index) => _Vector(VS)[Index] - _Vector(VT)[Index]); }
		public AstNodeStm vdiv() { return _Vector(VD).SetVector((Index) => _Vector(VS)[Index] / _Vector(VT)[Index]); }
		public AstNodeStm vmul() { return _Vector(VD).SetVector((Index) => _Vector(VS)[Index] * _Vector(VT)[Index]); }

		AstNodeStm _vidt_x(int VectorSize, uint Register)
		{
			return ast.AstNotImplemented("_vidt_x");
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
			return ast.AstNotImplemented("_vzero_x");
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
		public AstNodeStm viim() { return _Cell(VT_NoPrefix, VFloat).Set((float)Instruction.IMM); }
		public AstNodeStm vdet() { return ast.AstNotImplemented("vdet"); }
		public AstNodeStm mfvme() { return ast.AstNotImplemented("mfvme"); }
		public AstNodeStm mtvme() { return ast.AstNotImplemented("mtvme"); }

		/// <summary>
		/// ID("vfim",        VM("110111:11:1:vt:imm16"), "%xs, %vh",      ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public AstNodeStm vfim()
		{
			return _Cell(VT_NoPrefix).Set((float)Instruction.IMM_HF);
		}


		public AstNodeStm vlgb() { return ast.AstNotImplemented("vlgb"); }
		public AstNodeStm vsbn() { return ast.AstNotImplemented("vsbn"); }

		public AstNodeStm vsbz() { return ast.AstNotImplemented("vsbz"); }
		public AstNodeStm vsocp() { return ast.AstNotImplemented("vsocp"); }
		public AstNodeStm vus2i() { return ast.AstNotImplemented("vus2i"); }

		public AstNodeStm vwbn() { return ast.AstNotImplemented("vwbn"); }

		public AstNodeStm vnop() { return ast.AstNotImplemented("vnop"); }
		public AstNodeStm vsync() { return ast.AstNotImplemented("vsync"); }
		public AstNodeStm vflush() { return ast.AstNotImplemented("vflush"); }

		public static uint _vt4444_step(uint i0, uint i1)
		{
			uint o0 = 0;
			o0 |= ((i0 >> 4) & 15) << 0;
			o0 |= ((i0 >> 12) & 15) << 4;
			o0 |= ((i0 >> 20) & 15) << 8;
			o0 |= ((i0 >> 28) & 15) << 12;
			o0 |= ((i1 >> 4) & 15) << 16;
			o0 |= ((i1 >> 12) & 15) << 20;
			o0 |= ((i1 >> 20) & 15) << 24;
			o0 |= ((i1 >> 28) & 15) << 28;
			//Console.Error.WriteLine("{0:X8} {1:X8} -> {2:X8}", i0, i1, o0);
			//throw(new Exception("" + i0 + ";" + i1));
			return o0;
		}

		public static uint _vt5551_step(uint i0, uint i1)
		{
			uint o0 = 0;
			o0 |= ((i0 >> 3) & 31) << 0;
			o0 |= ((i0 >> 11) & 31) << 5;
			o0 |= ((i0 >> 19) & 31) << 10;
			o0 |= ((i0 >> 31) & 1) << 15;
			o0 |= ((i1 >> 3) & 31) << 16;
			o0 |= ((i1 >> 11) & 31) << 21;
			o0 |= ((i1 >> 19) & 31) << 26;
			o0 |= ((i1 >> 31) & 1) << 31;
			//throw (new Exception("" + i0 + ";" + i1));
			return o0;
		}

		public static uint _vt5650_step(uint i0, uint i1)
		{
			uint o0 = 0;
			o0 |= ((i0 >> 3) & 31) << 0;
			o0 |= ((i0 >> 10) & 63) << 5;
			o0 |= ((i0 >> 19) & 31) << 11;
			o0 |= ((i1 >> 3) & 31) << 16;
			o0 |= ((i1 >> 10) & 63) << 21;
			o0 |= ((i1 >> 19) & 31) << 27;
			//throw (new Exception("" + i0 + ";" + i1));
			return o0;
		}

		private AstNodeStm _vtXXXX_q(Func<uint, uint, uint> _vtXXXX_stepCallback)
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 4) throw (new Exception("Not implemented _vtXXXX_q for VectorSize=" + VectorSize));
			var Dest = _Vector(VD_NoPrefix, VUInt, 2);
			var Src = _Vector(VS_NoPrefix, VUInt, 4);
			//AstLoadVfpuReg

			var Node = Dest.SetVector((Index) =>
			{
				return ast.CallStatic(
					_vtXXXX_stepCallback,
					Src[Index * 2 + 0],
					Src[Index * 2 + 1]
				);
			});

			//throw(new Exception(GeneratorCSharp.GenerateString<GeneratorCSharp>(Node)));

			return Node;
		}

		public AstNodeStm vt4444_q() { return _vtXXXX_q(_vt4444_step); }
		public AstNodeStm vt5551_q() { return _vtXXXX_q(_vt5551_step); }
		public AstNodeStm vt5650_q() { return _vtXXXX_q(_vt5650_step); }

		[PspUntested]
		public AstNodeStm vbfy1()
		{
			return ast.AstNotImplemented("vbfy1");

			//var VectorSize = Instruction.ONE_TWO;
			//if (VectorSize != 2 || VectorSize != 4)
			//{
			//	Console.Error.WriteLine("vbfy1 : just support .p or .q");
			//	return;
			//}
			//
			//VectorOperationSaveVd(VectorSize, (Index) =>
			//{
			//	switch (Index)
			//	{
			//		case 0: Load_VS(0); Load_VS(1); SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); break;
			//		case 1: Load_VS(0); Load_VS(1); SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); break;
			//		case 2: Load_VS(2); Load_VS(3); SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); break;
			//		case 3: Load_VS(2); Load_VS(3); SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); break;
			//	}
			//}, AsInteger: false);
		}

		[PspUntested]
		public AstNodeStm vbfy2()
		{
			return ast.AstNotImplemented("vbfy2");

			//var VectorSize = Instruction.ONE_TWO;
			//if (VectorSize != 4)
			//{
			//	Console.Error.WriteLine("vbfy2 : just support .q");
			//	return;
			//}
			//
			//VectorOperationSaveVd(VectorSize, (Index) =>
			//{
			//	switch (Index)
			//	{
			//		case 0: Load_VS(0); Load_VS(2); SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); break;
			//		case 1: Load_VS(1); Load_VS(3); SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); break;
			//		case 2: Load_VS(0); Load_VS(2); SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); break;
			//		case 3: Load_VS(1); Load_VS(3); SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); break;
			//	}
			//}, AsInteger: false);
		}

		private AstNodeStm _vsrt_doMinMax(Func<float, float, float> Func, int Left, int Right)
		{
			return ast.AstNotImplemented("_vsrt_doMinMax");

			//Load_VS(Left);
			//Load_VS(Right);
			//MipsMethodEmitter.CallMethod(Func);
		}

		[PspUntested]
		public AstNodeStm vsrt1()
		{
			return ast.AstNotImplemented("vsrt1");

			//var VectorSize = Instruction.ONE_TWO;
			//if (VectorSize != 4) Console.Error.WriteLine("vsrt1 : VectorSize != 4");
			//
			//VectorOperationSaveVd(4, (Index) =>
			//{
			//	switch (Index)
			//	{
			//		case 0: _vsrt_doMinMax(MathFloat.Min, 0, 1); break;
			//		case 1: _vsrt_doMinMax(MathFloat.Max, 0, 1); break;
			//		case 2: _vsrt_doMinMax(MathFloat.Min, 2, 3); break;
			//		case 3: _vsrt_doMinMax(MathFloat.Max, 2, 3); break;
			//	}
			//}, AsInteger: false);
		}

		[PspUntested]
		public AstNodeStm vsrt2()
		{
			return ast.AstNotImplemented("vsrt2");

			//var VectorSize = Instruction.ONE_TWO;
			//if (VectorSize != 4) Console.Error.WriteLine("vsrt1 : VectorSize != 4");
			//
			//VectorOperationSaveVd(4, (Index) =>
			//{
			//	switch (Index)
			//	{
			//		case 0: _vsrt_doMinMax(MathFloat.Min, 0, 3); break;
			//		case 1: _vsrt_doMinMax(MathFloat.Min, 1, 2); break;
			//		case 2: _vsrt_doMinMax(MathFloat.Max, 1, 2); break;
			//		case 3: _vsrt_doMinMax(MathFloat.Max, 0, 3); break;
			//	}
			//}, AsInteger: false);
		}

		[PspUntested]
		public AstNodeStm vsrt3()
		{
			return ast.AstNotImplemented("vsrt3");
			//var VectorSize = Instruction.ONE_TWO;
			//if (VectorSize != 4) Console.Error.WriteLine("vsrt1 : VectorSize != 4");
			//
			//VectorOperationSaveVd(4, (Index) =>
			//{
			//	switch (Index)
			//	{
			//		case 0: _vsrt_doMinMax(MathFloat.Max, 0, 1); break;
			//		case 1: _vsrt_doMinMax(MathFloat.Min, 0, 1); break;
			//		case 2: _vsrt_doMinMax(MathFloat.Max, 2, 3); break;
			//		case 3: _vsrt_doMinMax(MathFloat.Min, 2, 3); break;
			//	}
			//}, AsInteger: false);
		}

		[PspUntested]
		public AstNodeStm vsrt4()
		{
			return ast.AstNotImplemented("vsrt4");
			//var VectorSize = Instruction.ONE_TWO;
			//if (VectorSize != 4) Console.Error.WriteLine("vsrt1 : VectorSize != 4");
			//
			//VectorOperationSaveVd(4, (Index) =>
			//{
			//	switch (Index)
			//	{
			//		case 0: _vsrt_doMinMax(MathFloat.Max, 0, 3); break;
			//		case 1: _vsrt_doMinMax(MathFloat.Max, 1, 2); break;
			//		case 2: _vsrt_doMinMax(MathFloat.Min, 1, 2); break;
			//		case 3: _vsrt_doMinMax(MathFloat.Min, 0, 3); break;
			//	}
			//}, AsInteger: false);
		}

		/// <summary>
		/// +-------------------------------------+----+--------------+---+--------------+ <para/>
		/// |31                                16 | 15 | 14         8 | 7 | 6         0  | <para/>
		/// +-------------------------------------+----+--------------+---+--------------+ <para/>
		/// |        opcode 0xd046 (p)            | 0  | vfpu_rs[6-0] | 1 | vfpu_rd[6-0] | <para/>
		/// |        opcode 0xd046 (t)            | 1  | vfpu_rs[6-0] | 0 | vfpu_rd[6-0] | <para/>
		/// |        opcode 0xd046 (q)            | 1  | vfpu_rs[6-0] | 1 | vfpu_rd[6-0] | <para/>
		/// +-------------------------------------+----+--------------+---+--------------+ <para/>
		/// <para/>
		/// Float ADD?.Pair/Triple/Quad  --  Accumulate Components of Vector into Single Float
		/// <para/>
		/// vfad.p %vfpu_rd, %vfpu_rs  ; Accumulate Components of Pair <para/>
		/// vfad.t %vfpu_rd, %vfpu_rs  ; Accumulate Components of Triple <para/>
		/// vfad.q %vfpu_rd, %vfpu_rs  ; Accumulate Components of Quad <para/>
		/// <para/>
		/// %vfpu_rs:   VFPU Vector Source Register ([p|t|q]reg 0..127) <para/>
		/// %vfpu_rd:   VFPU Vector Destination Register (sreg 0..127) <para/>
		/// <para/>
		/// vfpu_regs[%vfpu_rd] &lt;- Sum_Of_Components(vfpu_regs[%vfpu_rs]) 
		/// </summary>
		public AstNodeStm vfad()
		{
			var VectorSize = Instruction.ONE_TWO;
			var Value = (AstNodeExpr)0f;
			foreach (var Index in Enumerable.Range(0, VectorSize)) Value += _Vector(VS)[Index];
			return _Cell(VD).Set(Value);
		}

		/// <summary>
		/// +-------------------------------------+----+--------------+---+--------------+ <para/>
		/// |31                                16 | 15 | 14         8 | 7 | 6         0  | <para/>
		/// +-------------------------------------+----+--------------+---+--------------+ <para/>
		/// |        opcode 0xd047 (p)            | 0  | vfpu_rs[6-0] | 1 | vfpu_rd[6-0] | <para/>
		/// |        opcode 0xd047 (t)            | 1  | vfpu_rs[6-0] | 0 | vfpu_rd[6-0] | <para/>
		/// |        opcode 0xd047 (q)            | 1  | vfpu_rs[6-0] | 1 | vfpu_rd[6-0] | <para/>
		/// +-------------------------------------+----+--------------+---+--------------+ <para/>
		/// <para/>
		///   VectorAverage.Pair/Triple/Quad  --  Average Components of Vector into Single Float
		/// <para/>
		/// 		vavg.p %vfpu_rd, %vfpu_rs  ; Accumulate Components of Pair <para/>
		/// 		vavg.t %vfpu_rd, %vfpu_rs  ; Accumulate Components of Triple <para/>
		/// 		vavg.q %vfpu_rd, %vfpu_rs  ; Accumulate Components of Quad <para/>
		/// <para/>
		/// 				%vfpu_rs:   VFPU Vector Source Register ([p|t|q]reg 0..127) <para/>
		/// 				%vfpu_rd:   VFPU Vector Destination Register (sreg 0..127) <para/>
		/// <para/>
		/// 		vfpu_regs[%vfpu_rd] &lt;- Average_Of_Components(vfpu_regs[%vfpu_rs]) 
		/// </summary>
		public AstNodeStm vavg()
		{
			var VectorSize = Instruction.ONE_TWO;
			var Value = (AstNodeExpr)0f;
			foreach (var Index in Enumerable.Range(0, VectorSize)) Value += _Vector(VS)[Index];
			return _Cell(VD).Set(Value / (float)VectorSize);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Prefixes
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public static void _vpfxd_impl(CpuThreadState CpuThreadState, uint Value)
		{
			CpuThreadState.PrefixDestination.Value = Value;
		}

		public static void _vpfxs_impl(CpuThreadState CpuThreadState, uint Value)
		{
			CpuThreadState.PrefixSource.Value = Value;
		}

		public static void _vpfxt_impl(CpuThreadState CpuThreadState, uint Value)
		{
			CpuThreadState.PrefixTarget.Value = Value;
		}

		private AstNodeStm _vpfx_dst(IVfpuPrefixCommon Prefix, Action<CpuThreadState, uint> _vpfx_dst_impl)
		{
			Prefix.EnableAndSetValueAndPc(Instruction.Value, PC);
			return ast.Statement(ast.CallStatic(_vpfx_dst_impl, ast.CpuThreadStateArgument(), Instruction.Value));
		}

		public AstNodeStm vpfxd() { return _vpfx_dst(PrefixDestination, _vpfxd_impl); }
		public AstNodeStm vpfxs() { return _vpfx_dst(PrefixSource, _vpfxs_impl); }
		public AstNodeStm vpfxt() { return _vpfx_dst(PrefixTarget, _vpfxt_impl); }

		public static void _vrnds(CpuThreadState CpuThreadState, int Seed)
		{
			CpuThreadState.Random = new Random(Seed);
		}

		public static int _vrndi(CpuThreadState CpuThreadState)
		{
			byte[] Data = new byte[4];
			CpuThreadState.Random.NextBytes(Data);
			return BitConverter.ToInt32(Data, 0);
		}

		public static float _vrndf1(CpuThreadState CpuThreadState)
		{
			return (float)(CpuThreadState.Random.NextDouble() * 2.0f);
		}

		public static float _vrndf2(CpuThreadState CpuThreadState)
		{
			return (float)(CpuThreadState.Random.NextDouble() * 4.0f);
		}

		/// <summary>
		/// Seed
		/// </summary>
		public AstNodeStm vrnds()
		{
			return ast.AstNotImplemented("vrnds");
			//SafeILGenerator.LoadArgument0CpuThreadState();
			//Load_VS(0, true);
			//MipsMethodEmitter.CallMethod((Action<CpuThreadState, int>)_vrnds);
		}

		/// <summary>
		/// -2^31 &lt;= value &lt; 2^31 
		/// </summary>
		public AstNodeStm vrndi()
		{
			return ast.AstNotImplemented("vrndi");
			//var VectorSize = Instruction.ONE_TWO;
			//Save_VD(Index: 0, VectorSize: VectorSize, Action: () =>
			//{
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		SafeILGenerator.LoadArgument0CpuThreadState();
			//		MipsMethodEmitter.CallMethod((Func<CpuThreadState, int>)_vrndi);
			//	}
			//}, AsInteger: true);
		}

		// 0.0 <= value < 2.0.
		/// <summary>
		/// 0.0 &lt;= value &lt; 2.0.
		/// </summary>
		public AstNodeStm vrndf1()
		{
			return ast.AstNotImplemented("vrndf1");
			//var VectorSize = Instruction.ONE_TWO;
			//Save_VD(Index: 0, VectorSize: VectorSize, Action: () =>
			//{
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		SafeILGenerator.LoadArgument0CpuThreadState();
			//		MipsMethodEmitter.CallMethod((Func<CpuThreadState, float>)_vrndf1);
			//	}
			//}, AsInteger: false);
		}

		// 0.0 <= value < 4.0 (max = 3.999979)
		/// <summary>
		/// 0.0 &lt;= value &lt; 4.0 (max = 3.999979)
		/// </summary>
		public AstNodeStm vrndf2()
		{
			return ast.AstNotImplemented("vrndf2");
			//var VectorSize = Instruction.ONE_TWO;
			//Save_VD(Index: 0, VectorSize: VectorSize, Action: () =>
			//{
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		SafeILGenerator.LoadArgument0CpuThreadState();
			//		MipsMethodEmitter.CallMethod((Func<CpuThreadState, float>)_vrndf2);
			//	}
			//}, AsInteger: false);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Matrix Operations
		/////////////////////////////////////////////////////////////////////////////////////////////////

		// Vfpu Matrix MULtiplication
		// @FIX!!!
		public AstNodeStm vmmul()
		{
			int VectorSize = Instruction.ONE_TWO;
			var Dest = _Matrix(VD_NoPrefix);
			var Src = _Matrix(VS_NoPrefix);
			var Target = _Matrix(VT_NoPrefix);

			return Dest.SetMatrix((Column, Row) =>
			{
				var Adder = (AstNodeExpr)ast.Immediate(0f);
				for (int n = 0; n < VectorSize; n++)
				{
					Adder += Target[Column, n] * Src[Row, n];
				}
				return Adder;
			});
		}

		// -

		private AstNodeStm _vtfm_x(uint VectorSize)
		{
			return ast.AstNotImplemented("_vtfm_x");
			//VectorOperationSaveVd(VectorSize, Index =>
			//{
			//	SafeILGenerator.Push((float)0.0f);
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		Load_VS(n, VectorSize, RegisterOffset: Index);
			//		Load_VT(n, VectorSize);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			//	}
			//});
		}

		private AstNodeStm _vhtfm_x(uint VectorSize)
		{
			return ast.AstNotImplemented("_vhtfm_x");
			//VectorOperationSaveVd(VectorSize, Index =>
			//{
			//	SafeILGenerator.Push((float)0.0f);
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		Load_VS(n, VectorSize, RegisterOffset: Index);
			//		if (n == VectorSize - 1)
			//		{
			//			SafeILGenerator.Push((float)1.0f);
			//		}
			//		else
			//		{
			//			Load_VT(n, VectorSize - 1);
			//		}
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
			//		SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			//	}
			//});
		}

		public AstNodeStm vtfm2() { return _vtfm_x(2); }
		public AstNodeStm vtfm3() { return _vtfm_x(3); }
		public AstNodeStm vtfm4() { return _vtfm_x(4); }

		public AstNodeStm vhtfm2() { return _vhtfm_x(2); }
		public AstNodeStm vhtfm3() { return _vhtfm_x(3); }
		public AstNodeStm vhtfm4() { return _vhtfm_x(4); }

		public AstNodeStm vmidt() { return _Matrix(VD).SetMatrix((Column, Row) => (Column == Row) ? 1f : 0f); }
		public AstNodeStm vmzero() { return _Matrix(VD).SetMatrix((Column, Row) => 0f); }
		public AstNodeStm vmone() { return _Matrix(VD).SetMatrix((Column, Row) => 1f); }

		/// <summary>
		/// +----------------------+--------------+----+--------------+---+--------------+ <para/>
		/// |31                 23 | 22        16 | 15 | 14         8 | 7 | 6         0  | <para/>
		/// +----------------------+--------------+----+--------------+---+--------------+ <para/>
		/// |  opcode 0x65008080   | vfpu_rt[6-0] |    | vfpu_rs[6-0] |   | vfpu_rd[6-0] | <para/>
		/// +----------------------+--------------+----+--------------+---+--------------+ <para/>
		/// <para/>
		/// MatrixScale.Pair/Triple/Quad, multiply all components by scale factor <para/>
		/// <para/>
		/// vmscl.p %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Scale 2x2 Matrix by %vfpu_rt <para/>
		/// vmscl.t %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Scale 3x3 Matrix by %vfpu_rt <para/>
		/// vmscl.q %vfpu_rd, %vfpu_rs, %vfpu_rt   ; Scale 4x4 Matrix by %vfpu_rt <para/>
		/// <para/>
		/// %vfpu_rt:       VFPU Vector Source Register, Scale (sreg 0..127) <para/>
		/// %vfpu_rs:       VFPU Vector Source Register, Matrix ([p|t|q]reg 0..127) <para/>
		/// %vfpu_rd:       VFPU Vector Destination Register, Matrix ([s|p|t|q]reg 0..127) <para/>
		/// <para/>
		/// vfpu_mtx[%vfpu_rd] &lt;- vfpu_mtx[%vfpu_rs] * vfpu_reg[%vfpu_rt]
		/// </summary>
		public AstNodeStm vmscl()
		{
			var D = _Matrix(VD);
			var S = _Matrix(VS);
			var T = _Cell(VT);
			return D.SetMatrix((Column, Row) =>
				S[Column, Row] * T.Get()
			);
		}

		public float _vqmul_row0(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return +(l0 * r3) + (l1 * r2) - (l2 * r1) + (l3 * r0);
			//v3[0] = +(v1[0] * v2[3]) + (v1[1] * v2[2]) - (v1[2] * v2[1]) + (v1[3] * v2[0]);
		}

		public float _vqmul_row1(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return -(l0 * r2) + (l1 * r3) + (l2 * r0) + (l3 * r1);
			//v3[1] = -(v1[0] * v2[2]) + (v1[1] * v2[3]) + (v1[2] * v2[0]) + (v1[3] * v2[1]);
		}

		public float _vqmul_row2(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return +(l0 * r1) - (l1 * r0) + (l2 * r3) + (l3 * r2);
			//v3[2] = +(v1[0] * v2[1]) - (v1[1] * v2[0]) + (v1[2] * v2[3]) + (v1[3] * v2[2]);
		}

		public float _vqmul_row3(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return -(l0 * r0) - (l1 * r1) - (l2 * r2) + (l3 * r3);
			//v3[3] = -(v1[0] * v2[0]) - (v1[1] * v2[1]) - (v1[2] * v2[2]) + (v1[3] * v2[3]);
		}

#if false
			loadVs(4, vs);
			loadVt(4, vt);

			v3[0] = +(v1[0] * v2[3]) + (v1[1] * v2[2]) - (v1[2] * v2[1]) + (v1[3] * v2[0]);
			v3[1] = -(v1[0] * v2[2]) + (v1[1] * v2[3]) + (v1[2] * v2[0]) + (v1[3] * v2[1]);
			v3[2] = +(v1[0] * v2[1]) - (v1[1] * v2[0]) + (v1[2] * v2[3]) + (v1[3] * v2[2]);
			v3[3] = -(v1[0] * v2[0]) - (v1[1] * v2[1]) - (v1[2] * v2[2]) + (v1[3] * v2[3]);

			saveVd(4, vd, v3);
#endif

		public AstNodeStm vqmul()
		{
			return ast.AstNotImplemented("vqmul");
			////var VectorSize = Instruction.ONE_TWO;
			//var VectorSize = (uint)4;
			//VectorOperationSaveVd(VectorSize, (Index) =>
			//{
			//	Load_VS(0);
			//	Load_VS(1);
			//	Load_VS(2);
			//	Load_VS(3);
			//	Load_VT(0);
			//	Load_VT(1);
			//	Load_VT(2);
			//	Load_VT(3);
			//	switch (Index)
			//	{
			//		case 0: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row0); break;
			//		case 1: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row1); break;
			//		case 2: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row2); break;
			//		case 3: SafeILGenerator.Call((Func<float, float, float, float, float, float, float, float, float>)_vqmul_row3); break;
			//	}
			//});
		}

		public AstNodeStm vmmov() { return _Matrix(VD).SetMatrix((Column, Row) => _Matrix(VS)[Column, Row]); }

	}
}
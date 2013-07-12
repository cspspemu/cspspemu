using System;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;
using System.Linq;

namespace CSPspEmu.Core.Cpu.Emitter
{
	unsafe public sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Vfpu DOT product
		// Vfpu SCaLe/ROTate
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm vdot() { return CEL_VD.Set(_Aggregate(0f, ONE_TWO, (Value, Index) => Value + VEC_VS[Index] * VEC_VT[Index])); }
		public AstNodeStm vscl() { return VEC_VD.SetVector(Index => VEC_VS[Index] * CEL_VT.Get()); }

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
		public AstNodeStm vzero() { return VEC_VD.SetVector((Index) => 0f);  }
		public AstNodeStm vone() { return VEC_VD.SetVector((Index) => 1f); }

		// vmov  : Vector MOVe
		// vsgn  : Vector SiGN
		// *     : Vector Reverse SQuare root/COSine/Arc SINe/LOG2
		// @CHECK
		public AstNodeStm vmov() { return VEC_VD.SetVector((Index) => VEC_VS[Index]); }
		public AstNodeStm vabs() { return VEC_VD.SetVector((Index) => ast.CallStatic((Func<float, float>)MathFloat.Abs, _Vector(VS)[Index])); }
		public AstNodeStm vneg() { return VEC_VD.SetVector((Index) => -VEC_VS[Index]); }
		public AstNodeStm vocp() { return VEC_VD.SetVector((Index) => 1f - VEC_VS[Index]); }
		public AstNodeStm vsgn() { return VEC_VD.SetVector((Index) => ast.CallStatic((Func<float, float>)MathFloat.Sign, _Vector(VS)[Index])); }
		public AstNodeStm vrcp() { return VEC_VD.SetVector((Index) => 1f / VEC_VS[Index]); }

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
		public AstNodeStm vcst() { return CEL_VD.Set(VfpuUtils.GetVfpuConstantsValue((int)Instruction.IMM5)); }

		// -
		public AstNodeStm vhdp()
		{
			return ast.NotImplemented("vhdp");
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
			return ast.NotImplemented("vcrs_t");
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
			return ast.NotImplemented("vcrsp_t");
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
		public AstNodeStm vmin() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float, float>)MathFloat.Min, VEC_VS[Index], VEC_VT[Index])); }
		public AstNodeStm vmax() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float, float>)MathFloat.Max, VEC_VS[Index], VEC_VT[Index])); }
		public AstNodeStm vadd() { return VEC_VD.SetVector(Index => VEC_VS[Index] + VEC_VT[Index]); }
		public AstNodeStm vsub() { return VEC_VD.SetVector(Index => VEC_VS[Index] - VEC_VT[Index]); }
		public AstNodeStm vdiv() { return VEC_VD.SetVector(Index => VEC_VS[Index] / VEC_VT[Index]); }
		public AstNodeStm vmul() { return VEC_VD.SetVector(Index => VEC_VS[Index] * VEC_VT[Index]); }

		AstNodeStm _vidt_x(int VectorSize, uint Register)
		{
			return ast.NotImplemented("_vidt_x");
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
			return ast.NotImplemented("_vzero_x");
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
		public AstNodeStm vidt() { return _vidt_x(ONE_TWO, (uint)(Instruction.VD)); }

		// Vfpu load Integer IMmediate
		public AstNodeStm viim() { return CEL_VT_NoPrefix.Set((float)Instruction.IMM); }
		public AstNodeStm vdet() { return ast.NotImplemented("vdet"); }
		public AstNodeStm mfvme() { return ast.NotImplemented("mfvme"); }
		public AstNodeStm mtvme() { return ast.NotImplemented("mtvme"); }
		public AstNodeStm vfim() { return CEL_VT_NoPrefix.Set((float)Instruction.IMM_HF); }


		public AstNodeStm vlgb() { return ast.NotImplemented("vlgb"); }
		public AstNodeStm vsbn() { return ast.NotImplemented("vsbn"); }

		public AstNodeStm vsbz() { return ast.NotImplemented("vsbz"); }
		public AstNodeStm vsocp() { return ast.NotImplemented("vsocp"); }
		public AstNodeStm vus2i() { return ast.NotImplemented("vus2i"); }

		public AstNodeStm vwbn() { return ast.NotImplemented("vwbn"); }

		public AstNodeStm vnop() { return ast.NotImplemented("vnop"); }
		public AstNodeStm vsync() { return ast.NotImplemented("vsync"); }
		public AstNodeStm vflush() { return ast.NotImplemented("vflush"); }

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
			var Dest = VEC(VD_NoPrefix, VUInt, 2);
			var Src = VEC(VS_NoPrefix, VUInt, 4);
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
			return ast.NotImplemented("vbfy1");

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
			return ast.NotImplemented("vbfy2");

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
			return ast.NotImplemented("_vsrt_doMinMax");

			//Load_VS(Left);
			//Load_VS(Right);
			//MipsMethodEmitter.CallMethod(Func);
		}

		[PspUntested]
		public AstNodeStm vsrt1()
		{
			return ast.NotImplemented("vsrt1");

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
			return ast.NotImplemented("vsrt2");

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
			return ast.NotImplemented("vsrt3");
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
			return ast.NotImplemented("vsrt4");
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

		public AstNodeStm vfad() { return CEL_VD.Set(_Aggregate(0f, (Value, Index) => Value + VEC_VS[Index])); }
		public AstNodeStm vavg() { return CEL_VD.Set(_Aggregate(0f, (Value, Index) => Value + VEC_VS[Index]) / (float)ONE_TWO); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Prefixes
		/////////////////////////////////////////////////////////////////////////////////////////////////

		private AstNodeStm _vpfx_dst(IVfpuPrefixCommon Prefix, Action<CpuThreadState, uint> _vpfx_dst_impl)
		{
			Prefix.EnableAndSetValueAndPc(Instruction.Value, PC);
			return ast.Statement(ast.CallStatic(_vpfx_dst_impl, ast.CpuThreadState, Instruction.Value));
		}

		public AstNodeStm vpfxd() { return _vpfx_dst(PrefixDestination, CpuEmitterUtils._vpfxd_impl); }
		public AstNodeStm vpfxs() { return _vpfx_dst(PrefixSource, CpuEmitterUtils._vpfxs_impl); }
		public AstNodeStm vpfxt() { return _vpfx_dst(PrefixTarget, CpuEmitterUtils._vpfxt_impl); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Random:
		//   Seed
		//   Integer: -2^31 <= value < 2^31 
		//   Float  : 0.0 <= value < 2.0.
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public AstNodeStm vrnds() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, int>)CpuEmitterUtils._vrnds, ast.CpuThreadState)); }
		public AstNodeStm vrndi() { return VEC_VD_i.SetVector(Index => ast.CallStatic((Func<CpuThreadState, int>)CpuEmitterUtils._vrndi, ast.CpuThreadState)); }
		public AstNodeStm vrndf1() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<CpuThreadState, float>)CpuEmitterUtils._vrndf1, ast.CpuThreadState)); }
		public AstNodeStm vrndf2() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<CpuThreadState, float>)CpuEmitterUtils._vrndf2, ast.CpuThreadState)); }

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
			return ast.NotImplemented("_vtfm_x");
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
			return ast.NotImplemented("_vhtfm_x");
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
		public AstNodeStm vmscl() { return _Matrix(VD).SetMatrix((Column, Row) => _Matrix(VS)[Column, Row] * _Cell(VT).Get() ); }


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
			return ast.NotImplemented("vqmul");
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

		public AstNodeStm vuc2i() { return VEC_VD_u.SetVector(Index => ast.Binary((ast.Binary(CEL_VS_u.Get(), ">>", (Index * 8)) & 0xFF) * 0x01010101, ">>", 1)); }
		public AstNodeStm vc2i()  { return VEC_VD_u.SetVector(Index => ast.Binary(CEL_VS_u.Get(), "<<", ((3 - Index) * 8)) & 0xFF000000); }

		// Vfpu Integer to(2) Color?
		public AstNodeStm vi2c() { var VEC_VS = VEC(VS, VType.VUInt, 4); return CEL_VD_u.Set(ast.CallStatic((Func<uint, uint, uint, uint, uint>)CpuEmitterUtils._vi2c_impl, VEC_VS[0], VEC_VS[1], VEC_VS[2], VEC_VS[3])); }
		public AstNodeStm vi2uc() { var VEC_VS = VEC(VS, VType.VInt, 4); return CEL_VD_u.Set(ast.CallStatic((Func<int, int, int, int, uint>)CpuEmitterUtils._vi2uc_impl, VEC_VS[0], VEC_VS[1], VEC_VS[2], VEC_VS[3])); }

		public AstNodeStm vs2i()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize > 2) throw (new NotImplementedException("vs2i.VectorSize"));
			var Dest = _Vector(VD, VUInt, VectorSize * 2);
			var Src = _Vector(VS, VUInt, VectorSize);
			return Dest.SetVector((Index) =>
			{
				var Value = Src[Index / 2];
				if ((Index % 2) == 0) Value = ast.Binary(Value, "<<", 16);
				return Value & 0xFFFF0000;
			});
		}

		public AstNodeStm vi2f() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, int, float>)MathFloat.Scalb, ast.Cast<float>(VEC_VS_i[Index]), -(int)Instruction.IMM5)); }

		public AstNodeStm vf2id()
		{
			return ast.NotImplemented("vf2id");
			//var VectorSize = Instruction.ONE_TWO;
			//var Imm5 = Instruction.IMM5;
			//VectorOperationSaveVd(VectorSize, Index =>
			//{
			//	Load_VS(Index, VectorSize);
			//	SafeILGenerator.Push((int)Imm5);
			//	MipsMethodEmitter.CallMethod((Func<float, int, float>)MathFloat.Scalb);
			//	MipsMethodEmitter.CallMethod((Func<float, int>)MathFloat.Floor);
			//}, AsInteger: true);
		}

		public AstNodeStm vf2in()
		{
			return ast.NotImplemented("vf2in");
			//var VectorSize = Instruction.ONE_TWO;
			//var Imm5 = Instruction.IMM5;
			//VectorOperationSaveVd(VectorSize, Index =>
			//{
			//	Load_VS(Index, VectorSize);
			//	SafeILGenerator.Push((int)Imm5);
			//	MipsMethodEmitter.CallMethod((Func<float, int, float>)MathFloat.Scalb);
			//	MipsMethodEmitter.CallMethod((Func<float, int>)MathFloat.Round);
			//}, AsInteger: true);
		}

		public AstNodeStm vf2iu()
		{
			return ast.NotImplemented("vf2iu");
			//var VectorSize = Instruction.ONE_TWO;
			//var Imm5 = Instruction.IMM5;
			//VectorOperationSaveVd(VectorSize, Index =>
			//{
			//	Load_VS(Index, VectorSize);
			//	SafeILGenerator.Push((int)Imm5);
			//	MipsMethodEmitter.CallMethod((Func<float, int, float>)MathFloat.Scalb);
			//	MipsMethodEmitter.CallMethod((Func<float, int>)MathFloat.Ceil);
			//}, AsInteger: true);
		}

		public AstNodeStm vf2iz()
		{
			return ast.NotImplemented("vf2iz");
			//var Imm5 = Instruction.IMM5;
			//VectorOperationSaveVd(Index =>
			//{
			//	Load_VS(Index);
			//	SafeILGenerator.Push((int)Imm5);
			//	MipsMethodEmitter.CallMethod((Func<float, int, float>)(CpuEmitter._vf2iz));
			//});
		}

		public AstNodeStm vi2s()
		{
			return ast.NotImplemented("vi2s");
			//var VectorSize = VectorSizeOneTwo;
			//Save_VD(0, VectorSize, () =>
			//{
			//	Load_VS(0);
			//	Load_VS(1);
			//	MipsMethodEmitter.CallMethod((Func<uint, uint, uint>)(CpuEmitter._vi2s));
			//}, AsInteger: true);
			//if (VectorSize == 4)
			//{
			//	Save_VD(1, VectorSize, () =>
			//	{
			//		Load_VS(2);
			//		Load_VS(3);
			//		MipsMethodEmitter.CallMethod((Func<uint, uint, uint>)(CpuEmitter._vi2s));
			//	}, AsInteger: true);
			//}
		}

		public AstNodeStm vf2h() { return ast.NotImplemented("vf2h"); }
		public AstNodeStm vh2f() { return ast.NotImplemented("vh2f"); }
		public AstNodeStm vi2us()
		{
			return ast.NotImplemented("vi2us");
			//var VectorSize = VectorSizeOneTwo;
			//VectorOperationSaveVd(VectorSize / 2, (Index) =>
			//{
			//	Load_VS(Index * 2 + 0, AsInteger: true);
			//	Load_VS(Index * 2 + 1, AsInteger: true);
			//	MipsMethodEmitter.CallMethod((Func<int, int, int>)(CpuEmitter._vi2us));
			//}, AsInteger: true);
		}

		public AstNodeStm vmfvc() { return ast.NotImplemented("vmfvc"); }
		public AstNodeStm vmtvc() { return ast.NotImplemented("vmtvc"); }

		public AstNodeStm mtv() { return _Cell(VD).Set(ast.GPR_f(RT)); }
		public AstNodeStm mtvc() { return ast.NotImplemented("mtvc"); }

		/// <summary>
		/// Copies a vfpu control register into a general purpose register.
		/// </summary>
		public AstNodeStm mfvc()
		{
			return ast.NotImplemented("mfvc");
			//MipsMethodEmitter.SaveGPR(RT, () =>
			//{
			//	SafeILGenerator.LoadArgument0CpuThreadState();
			//	SafeILGenerator.Push((int)(Instruction.IMM7 + 128));
			//	MipsMethodEmitter.CallMethod((Func<CpuThreadState, VfpuControlRegistersEnum, uint>)CpuEmitter._mfvc_impl);
			//});
		}

		// Move From/to Vfpu (C?)_
		public AstNodeStm mfv()
		{
			return ast.NotImplemented("mfv");
			//MipsMethodEmitter.SaveGPR_F(RT, () =>
			//{
			//	Load_VD(0, 1);
			//});
		}

		// Load/Store Vfpu (Left/Right)_
		// ID("lv.q",        VM("110110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		public AstNodeStm lv_q()
		{
			const int VectorSize = 4;
			var Dest = _Vector(VT5_1, VFloat, Size: VectorSize);

			return Dest.SetVector(Index => ast.MemoryGetValue<float>(Memory, Address_RS_IMM14(Index * 4)));
		}

		/// <summary>
		/// ID("sv.q",        VM("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public AstNodeStm sv_q()
		{
			int VectorSize = 4;
			var Dest = _Vector(VT5_1, VFloat, VectorSize);
			return _List(VectorSize, Index => ast.MemorySetValue<float>(Memory, Address_RS_IMM14(Index * 4), Dest[Index]));
		}

		private delegate void _lvl_svl_q_Delegate(CpuThreadState CpuThreadState, bool Save, float* r0, float* r1, float* r2, float* r3, uint Address);

		private AstNodeStm _lv_sv_l_r_q(bool left, bool save)
		{
			VfpuRegisterInt Register = Instruction.VT5_1;
			var MethodInfo = left
				? (_lvl_svl_q_Delegate)CpuEmitterUtils._lvl_svl_q
				: (_lvl_svl_q_Delegate)CpuEmitterUtils._lvr_svr_q
			;

			var VT5 = _Vector(VT5_1, VFloat, 4);

			return ast.Statement(ast.CallStatic(
				MethodInfo,
				ast.CpuThreadState,
				save,
				ast.GetAddress(VT5.GetIndexRef(0)),
				ast.GetAddress(VT5.GetIndexRef(1)),
				ast.GetAddress(VT5.GetIndexRef(2)),
				ast.GetAddress(VT5.GetIndexRef(3)),
				Address_RS_IMM14(0)
			));
		}

		public AstNodeStm lvl_q() { return _lv_sv_l_r_q(left: true, save: false); }
		public AstNodeStm svl_q() { return _lv_sv_l_r_q(left: true, save: true); }
	
		public AstNodeStm lvr_q() { return _lv_sv_l_r_q(left: false, save: false); }
		public AstNodeStm svr_q() { return _lv_sv_l_r_q(left: false, save: true); }

		public AstNodeStm lv_s() { return _Cell(VT5_2).Set(ast.MemoryGetValue<float>(Memory, Address_RS_IMM14())); }
		public AstNodeStm sv_s() { return ast.MemorySetValue<float>(Memory, Address_RS_IMM14(), _Cell(VT5_2).Get()); }
	}
}
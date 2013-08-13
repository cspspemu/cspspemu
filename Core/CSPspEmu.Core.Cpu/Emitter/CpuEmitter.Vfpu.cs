using System;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;
using System.Linq;
using System.Numerics;

namespace CSPspEmu.Core.Cpu.Emitter
{
	unsafe public sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Vfpu DOT product
		// Vfpu SCaLe/ROTate
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm vdot() { return CEL_VD.Set(_Aggregate(0f, ONE_TWO, (Aggregated, Index) => Aggregated + (VEC_VS[Index] * VEC_VT[Index])), PC); }
		public AstNodeStm vscl() { return VEC_VD.SetVector(Index => VEC_VS[Index] * CEL_VT.Get(), PC); }

		/// <summary>
		/// Vector ROTate
		/// </summary>
		public AstNodeStm vrot()
		{
			uint imm5 = Instruction.IMM5;
			var CosIndex = BitUtils.Extract(imm5, 0, 2);
			var SinIndex = BitUtils.Extract(imm5, 2, 2);
			bool NegateSin = BitUtils.ExtractBool(imm5, 4);

			var Dest = VEC_VD;
			var Src = CEL_VS;

			AstNodeExpr Sine = ast.CallStatic((Func<float, float>)MathFloat.SinV1, Src.Get());
			AstNodeExpr Cosine = ast.CallStatic((Func<float, float>)MathFloat.CosV1, Src.Get());
			if (NegateSin) Sine = -Sine;

			//Console.WriteLine("{0},{1},{2}", CosIndex, SinIndex, NegateSin);

			return Dest.SetVector((Index) =>
			{
				if (Index == CosIndex) return Cosine;
				if (Index == SinIndex) return Sine;
				return (SinIndex == CosIndex) ? Sine : 0f;
			}, PC);
		}

		// vzero: Vector ZERO
		// vone : Vector ONE
		public AstNodeStm vzero() { return VEC_VD.SetVector((Index) => 0f, PC); }
		public AstNodeStm vone() { return VEC_VD.SetVector((Index) => 1f, PC); }

		// vmov  : Vector MOVe
		// vsgn  : Vector SiGN
		// *     : Vector Reverse SQuare root/COSine/Arc SINe/LOG2
		// @CHECK
		public AstNodeStm vmov() {
			PrefixTarget.Consume();
			return VEC_VD.SetVector((Index) => VEC_VS[Index], PC);
		}
		public AstNodeStm vabs() { return VEC_VD.SetVector((Index) => ast.CallStatic((Func<float, float>)MathFloat.Abs, VEC_VS[Index]), PC); }
		public AstNodeStm vneg() { return VEC_VD.SetVector((Index) => -VEC_VS[Index], PC); }
		public AstNodeStm vocp() { return VEC_VD.SetVector((Index) => 1f - VEC_VS[Index], PC); }
		public AstNodeStm vsgn() { return VEC_VD.SetVector((Index) => ast.CallStatic((Func<float, float>)MathFloat.Sign, VEC_VS[Index]), PC); }
		public AstNodeStm vrcp() { return VEC_VD.SetVector((Index) => 1f / VEC_VS[Index], PC); }

		private AstNodeStm _vfpu_call_ff(Delegate Delegate)
		{
			return VEC_VD.SetVector((Index) => ast.CallStatic(Delegate, VEC_VS[Index]), PC);
		}

		// OP_V_INTERNAL_IN_N!(1, "1.0f / sqrt(v)");
		// vcst: Vfpu ConSTant
		public AstNodeStm vsqrt() { return _vfpu_call_ff((Func<float, float>)MathFloat.Sqrt); }
		public AstNodeStm vrsq() { return _vfpu_call_ff((Func<float, float>)MathFloat.RSqrt); }
		public AstNodeStm vsin() { return _vfpu_call_ff((Func<float, float>)MathFloat.SinV1); }
		public AstNodeStm vcos() { return _vfpu_call_ff((Func<float, float>)MathFloat.CosV1); }
		public AstNodeStm vexp2() { return _vfpu_call_ff((Func<float, float>)MathFloat.Exp2); }
		public AstNodeStm vlog2() { return _vfpu_call_ff((Func<float, float>)MathFloat.Log2); }
		public AstNodeStm vasin() { return _vfpu_call_ff((Func<float, float>)MathFloat.AsinV1); }
		public AstNodeStm vnrcp() { return _vfpu_call_ff((Func<float, float>)MathFloat.NRcp); }
		public AstNodeStm vnsin() { return _vfpu_call_ff((Func<float, float>)MathFloat.NSinV1); }
		public AstNodeStm vrexp2() { return _vfpu_call_ff((Func<float, float>)MathFloat.RExp2); }
		public AstNodeStm vsat0() { return _vfpu_call_ff((Func<float, float>)MathFloat.Vsat0); }
		public AstNodeStm vsat1() { return _vfpu_call_ff((Func<float, float>)MathFloat.Vsat1); }

		// Vector -> Cell operations
		public AstNodeStm vcst() { return CEL_VD.Set(VfpuConstants.GetConstantValueByIndex((int)Instruction.IMM5).Value, PC); }
		public AstNodeStm vhdp()
		{
			uint VectorSize = (uint)ONE_TWO;
			return CEL_VD.Set(_Aggregate(0f, (Aggregate, Index) =>
				Aggregate + VEC_VT[Index] * ((Index == VectorSize - 1) ? 1f : VEC_VS[Index])
			), PC);
		}

		public AstNodeStm vcrs_t()
		{
			var V_VD = VEC(VD, VType.VFloat, 3);
			var V_VS = VEC(VS, VType.VFloat, 3);
			var V_VT = VEC(VT, VType.VFloat, 3);
			return V_VD.SetVector(Index =>
			{
				switch (Index)
				{
					case 0: return V_VS[1] * V_VT[2];
					case 1: return V_VS[2] * V_VT[0];
					case 2: return V_VS[0] * V_VT[1];
					default: throw (new InvalidOperationException("vcrs_t.Assert!"));
				}
			}, PC);
		}

		/// <summary>
		/// Cross product
		/// </summary>
		public AstNodeStm vcrsp_t()
		{
			var d = VEC(VD, VType.VFloat, 3);
			var s = VEC(VS, VType.VFloat, 3);
			var t = VEC(VT, VType.VFloat, 3);

			return d.SetVector(Index =>
			{
				switch (Index)
				{
					case 0: return s[1] * t[2] - s[2] * t[1];
					case 1: return s[2] * t[0] - s[0] * t[2];
					case 2: return s[0] * t[1] - s[1] * t[0];
					default: throw (new InvalidOperationException("vcrsp_t.Assert!"));
				}
			}, PC);
		}

		// Vfpu MINimum/MAXium/ADD/SUB/DIV/MUL
		public AstNodeStm vmin() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float, float>)MathFloat.Min, VEC_VS[Index], VEC_VT[Index]), PC); }
		public AstNodeStm vmax() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, float, float>)MathFloat.Max, VEC_VS[Index], VEC_VT[Index]), PC); }
		public AstNodeStm vadd() { return VEC_VD.SetVector(Index => VEC_VS[Index] + VEC_VT[Index], PC); }
		public AstNodeStm vsub() { return VEC_VD.SetVector(Index => VEC_VS[Index] - VEC_VT[Index], PC); }
		public AstNodeStm vdiv() { return VEC_VD.SetVector(Index => VEC_VS[Index] / VEC_VT[Index], PC); }
		public AstNodeStm vmul() { return VEC_VD.SetVector(Index => VEC_VS[Index] * VEC_VT[Index], PC); }

		// Vfpu (Matrix) IDenTity
		public AstNodeStm vidt() {
			return VEC_VD.SetVector((Index) => (Index == (Instruction.IMM7 % ONE_TWO)) ? 1f : 0f, PC);
		}

		// Vfpu load Integer IMmediate
		public AstNodeStm viim() { return CEL_VT_NoPrefix.Set((float)Instruction.IMM, PC); }
		public AstNodeStm vdet() {
			var v1 = VEC(VS, VType.VFloat, 2);
			var v2 = VEC(VT, VType.VFloat, 2);
			return CEL_VD.Set(v1[0] * v2[1] - v1[1] * v2[0], PC);
		}
		public AstNodeStm mfvme() { return ast.NotImplemented("mfvme"); }
		public AstNodeStm mtvme() { return ast.NotImplemented("mtvme"); }
		public AstNodeStm vfim() { return CEL_VT_NoPrefix.Set((float)Instruction.IMM_HF, PC); }


		public AstNodeStm vlgb() { return ast.NotImplemented("vlgb"); }
		public AstNodeStm vsbn() { return ast.NotImplemented("vsbn"); }

		public AstNodeStm vsbz() { return ast.NotImplemented("vsbz"); }
		public AstNodeStm vsocp()
		{
			int VectorSize = ONE_TWO;
			//Console.WriteLine("VECTOR_SIZE: {0}", VectorSize);
			var VVD = VEC(VD, VType.VFloat, VectorSize * 2);
			var VVS = VEC(VS, VType.VFloat, VectorSize);
			return VVD.SetVector((Index) => {
				switch (Index)
				{
					case 0: return ast.CallStatic((Func<float, float, float, float>)MathFloat.Clamp, 1f - VVS[0], 0f, 1f);
					case 1: return ast.CallStatic((Func<float, float, float, float>)MathFloat.Clamp, VVS[0], 0f, 1f);
					case 2: return ast.CallStatic((Func<float, float, float, float>)MathFloat.Clamp, 1f - VVS[1], 0f, 1f);
					case 3: return ast.CallStatic((Func<float, float, float, float>)MathFloat.Clamp, VVS[1], 0f, 1f);
					default: throw (new NotImplementedException("vsocp: " + Index));
				}
			}, PC);
		}
		public AstNodeStm vus2i() { return ast.NotImplemented("vus2i"); }

		static public float _vwbn_impl(float Source, int Imm8)
		{
			return 0f;
#if true
			var exp = new BigInteger((int)Math.Pow(2, 127 - Imm8));
			var bn = new BigInteger((int)Source);
			if ((int)bn > 0)
			{
				bn = BigInteger.ModPow(bn, exp, bn);
			}
			return (float)(bn + ((Source < 0.0f) ? -exp : exp));

#else
			double exp = Math.Pow(2.0, 127 - Imm8);
			double bn = (double)Source;
			if (bn > 0.0) bn = Math.Pow(bn, exp) % bn;
			return (float)((Source < 0.0f) ? (bn - exp) : (bn + exp));
#endif
		}

		public AstNodeStm vwbn()
		{
			return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, int, float>)_vwbn_impl, VEC_VS[Index], (int)Instruction.IMM8), PC);
		}

		public AstNodeStm vnop() { return ast.Statement(); }
		public AstNodeStm vsync() { return ast.Statement(); }
		public AstNodeStm vflush() { return ast.Statement(); }

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
			}, PC);

			//throw(new Exception(GeneratorCSharp.GenerateString<GeneratorCSharp>(Node)));

			return Node;
		}

		public AstNodeStm vt4444_q() { return _vtXXXX_q(_vt4444_step); }
		public AstNodeStm vt5551_q() { return _vtXXXX_q(_vt5551_step); }
		public AstNodeStm vt5650_q() { return _vtXXXX_q(_vt5650_step); }

		public AstNodeStm vbfy1()
		{
			return VEC_VD.SetVector(Index => {
				switch (Index)
				{
					case 0: return VEC_VS[0] + VEC_VS[1];
					case 1: return VEC_VS[0] - VEC_VS[1];
					case 2: return VEC_VS[2] + VEC_VS[3];
					case 3: return VEC_VS[2] - VEC_VS[3];
					default: throw (new InvalidOperationException("vbfy1.Assert!"));
				}
			}, PC);
		}

		public AstNodeStm vbfy2()
		{
			return VEC_VD.SetVector(Index =>
			{
				switch (Index)
				{
					case 0: return VEC_VS[0] + VEC_VS[2];
					case 1: return VEC_VS[1] + VEC_VS[3];
					case 2: return VEC_VS[0] - VEC_VS[2];
					case 3: return VEC_VS[1] - VEC_VS[3];
					default: throw (new InvalidOperationException("vbfy2.Assert!"));
				}
			}, PC);
		}

		public AstNodeStm vsrt1()
		{
			int VectorSize = ONE_TWO;
			if (VectorSize != 4) return ast.Statement();

			var VVD = VEC(VD, VType.VFloat, VectorSize);
			var VVS = VEC(VS, VType.VFloat, VectorSize);

			return VVD.SetVector(Index =>
			{
				switch (Index)
				{
					case 0: return ast.CallStatic((Func<float, float, float>)MathFloat.Min, VVS[0], VVS[1]);
					case 1: return ast.CallStatic((Func<float, float, float>)MathFloat.Max, VVS[0], VVS[1]);
					case 2: return ast.CallStatic((Func<float, float, float>)MathFloat.Min, VVS[2], VVS[3]);
					case 3: return ast.CallStatic((Func<float, float, float>)MathFloat.Max, VVS[2], VVS[3]);
					default: throw (new InvalidOperationException("vsrt1.Assert!"));
				}
			}, PC);
		}

		public AstNodeStm vsrt2()
		{
			int VectorSize = ONE_TWO;
			if (VectorSize != 4) return ast.Statement();

			var VVD = VEC(VD, VType.VFloat, VectorSize);
			var VVS = VEC(VS, VType.VFloat, VectorSize);

			return VVD.SetVector(Index =>
			{
				switch (Index)
				{
					case 0: return ast.CallStatic((Func<float, float, float>)MathFloat.Min, VVS[0], VVS[3]);
					case 1: return ast.CallStatic((Func<float, float, float>)MathFloat.Min, VVS[1], VVS[2]);
					case 2: return ast.CallStatic((Func<float, float, float>)MathFloat.Max, VVS[1], VVS[2]);
					case 3: return ast.CallStatic((Func<float, float, float>)MathFloat.Max, VVS[0], VVS[3]);
					default: throw (new InvalidOperationException("vsrt2.Assert!"));
				}
			}, PC);
		}

		public AstNodeStm vsrt3()
		{
			int VectorSize = ONE_TWO;
			if (VectorSize != 4) return ast.Statement();

			var VVD = VEC(VD, VType.VFloat, VectorSize);
			var VVS = VEC(VS, VType.VFloat, VectorSize);

			return VVD.SetVector(Index =>
			{
				switch (Index)
				{
					case 0: return ast.CallStatic((Func<float, float, float>)MathFloat.Max, VVS[0], VVS[1]);
					case 1: return ast.CallStatic((Func<float, float, float>)MathFloat.Min, VVS[0], VVS[1]);
					case 2: return ast.CallStatic((Func<float, float, float>)MathFloat.Max, VVS[2], VVS[3]);
					case 3: return ast.CallStatic((Func<float, float, float>)MathFloat.Min, VVS[2], VVS[3]);
					default: throw (new InvalidOperationException("vsrt3.Assert!"));
				}
			}, PC);
		}

		public AstNodeStm vsrt4()
		{
			int VectorSize = ONE_TWO;
			if (VectorSize != 4) return ast.Statement();

			var VVD = VEC(VD, VType.VFloat, VectorSize);
			var VVS = VEC(VS, VType.VFloat, VectorSize);

			return VVD.SetVector(Index =>
			{
				switch (Index)
				{
					case 0: return ast.CallStatic((Func<float, float, float>)MathFloat.Max, VVS[0], VVS[3]);
					case 1: return ast.CallStatic((Func<float, float, float>)MathFloat.Max, VVS[1], VVS[2]);
					case 2: return ast.CallStatic((Func<float, float, float>)MathFloat.Min, VVS[1], VVS[2]);
					case 3: return ast.CallStatic((Func<float, float, float>)MathFloat.Min, VVS[0], VVS[3]);
					default: throw (new InvalidOperationException("vsrt4.Assert!"));
				}
			}, PC);
		}

		public AstNodeStm vfad() { return CEL_VD.Set(_Aggregate(0f, (Value, Index) => Value + VEC_VS[Index]), PC); }
		public AstNodeStm vavg() { return CEL_VD.Set(_Aggregate(0f, (Value, Index) => Value + VEC_VS[Index]) / (float)ONE_TWO, PC); }

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
		public AstNodeStm vrndi() { return VEC_VD_i.SetVector(Index => ast.CallStatic((Func<CpuThreadState, int>)CpuEmitterUtils._vrndi, ast.CpuThreadState), PC); }
		public AstNodeStm vrndf1() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<CpuThreadState, float>)CpuEmitterUtils._vrndf1, ast.CpuThreadState), PC); }
		public AstNodeStm vrndf2() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<CpuThreadState, float>)CpuEmitterUtils._vrndf2, ast.CpuThreadState), PC); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Matrix Operations
		/////////////////////////////////////////////////////////////////////////////////////////////////

		// Vfpu Matrix MULtiplication
		// @FIX!!!
		public AstNodeStm vmmul()
		{
			int VectorSize = Instruction.ONE_TWO;
			//var Dest = MAT(VD_NoPrefix);
			//var Src = MAT(VS_NoPrefix);
			//var Target = MAT(VT_NoPrefix);
			var Dest = MAT_VD;
			var Src = MAT_VS;
			var Target = MAT_VT;

			return Dest.SetMatrix((Column, Row) =>
			{
				var Adder = (AstNodeExpr)ast.Immediate(0f);
				for (int n = 0; n < VectorSize; n++)
				{
					Adder += Target[Column, n] * Src[Row, n];
					//Adder += Target[Column, n] * Src[Row, n];
					//Adder += Target[Row, n] * Src[Column, n];
					//Adder += Target[n, Column] * Src[n, Row];
				}
				return Adder;
			}, PC);
		}

		// -

		private AstNodeStm _vtfm_x(int VectorSize)
		{
			var VecVD = VEC(VD, VType.VFloat, VectorSize);
			var VecVT = VEC(VT, VType.VFloat, VectorSize);
			var MatVS = MAT(VS, VType.VFloat, VectorSize);

			return VecVD.SetVector(Index =>
				_Aggregate(0f, VectorSize, (AggregatedValue, Index2) => AggregatedValue + (MatVS[Index, Index2] * VecVT[Index2]))
			, PC);
		}

		private AstNodeStm _vhtfm_x(int VectorSize)
		{
			var VecVD = VEC(VD, VType.VFloat, VectorSize);
			var VecVT = VEC(VT, VType.VFloat, VectorSize);
			var MatVS = MAT(VS, VType.VFloat, VectorSize);

			return VecVD.SetVector(Index =>
				_Aggregate(0f, VectorSize, (Aggregated, Index2) => Aggregated + MatVS[Index, Index2] * ((Index2 == VectorSize - 1) ? 1f : VecVT[Index2]))
			, PC);
		}

		public AstNodeStm vtfm2() { return _vtfm_x(2); }
		public AstNodeStm vtfm3() { return _vtfm_x(3); }
		public AstNodeStm vtfm4() { return _vtfm_x(4); }

		public AstNodeStm vhtfm2() { return _vhtfm_x(2); }
		public AstNodeStm vhtfm3() { return _vhtfm_x(3); }
		public AstNodeStm vhtfm4() { return _vhtfm_x(4); }

		public AstNodeStm vmidt() { return MAT_VD.SetMatrix((Column, Row) => (Column == Row) ? 1f : 0f, PC); }
		public AstNodeStm vmzero() { return MAT_VD.SetMatrix((Column, Row) => 0f, PC); }
		public AstNodeStm vmone() { return MAT_VD.SetMatrix((Column, Row) => 1f, PC); }
		public AstNodeStm vmscl() { return MAT_VD.SetMatrix((Column, Row) => MAT_VS[Column, Row] * CEL_VT.Get(), PC); }

		public AstNodeStm vqmul()
		{
			var v1 = VEC(VS, VType.VFloat, 4);
			var v2 = VEC(VT, VType.VFloat, 4);

			return VEC(VD, VType.VFloat, 4).SetVector(Index =>
			{
				switch (Index)
				{
					case 0: return +(v1[0] * v2[3]) + (v1[1] * v2[2]) - (v1[2] * v2[1]) + (v1[3] * v2[0]);
					case 1: return -(v1[0] * v2[2]) + (v1[1] * v2[3]) + (v1[2] * v2[0]) + (v1[3] * v2[1]);
					case 2: return +(v1[0] * v2[1]) - (v1[1] * v2[0]) + (v1[2] * v2[3]) + (v1[3] * v2[2]);
					case 3: return -(v1[0] * v2[0]) - (v1[1] * v2[1]) - (v1[2] * v2[2]) + (v1[3] * v2[3]);
					default: throw (new InvalidOperationException("vqmul.Assert"));
				}
			}, PC);
		}

		public AstNodeStm vmmov() { return MAT_VD.SetMatrix((Column, Row) => MAT_VS[Column, Row], PC); }

		public AstNodeStm vuc2i() { return VEC_VD_u.SetVector(Index => ast.Binary((ast.Binary(CEL_VS_u.Get(), ">>", (Index * 8)) & 0xFF) * 0x01010101, ">>", 1), PC); }
		public AstNodeStm vc2i() { return VEC_VD_u.SetVector(Index => ast.Binary(CEL_VS_u.Get(), "<<", ((3 - Index) * 8)) & 0xFF000000, PC); }

		// Vfpu Integer to(2) Color?
		public AstNodeStm vi2c() { var VEC_VS = VEC(VS, VType.VUInt, 4); return CEL_VD_u.Set(ast.CallStatic((Func<uint, uint, uint, uint, uint>)CpuEmitterUtils._vi2c_impl, VEC_VS[0], VEC_VS[1], VEC_VS[2], VEC_VS[3]), PC); }
		public AstNodeStm vi2uc() { var VEC_VS = VEC(VS, VType.VInt, 4); return CEL_VD_u.Set(ast.CallStatic((Func<int, int, int, int, uint>)CpuEmitterUtils._vi2uc_impl, VEC_VS[0], VEC_VS[1], VEC_VS[2], VEC_VS[3]), PC); }

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
			}, PC);
		}

		public AstNodeStm vi2f() { return VEC_VD.SetVector(Index => ast.CallStatic((Func<float, int, float>)MathFloat.Scalb, ast.Cast<float>(VEC_VS_i[Index]), -(int)Instruction.IMM5), PC); }

		private AstNodeStm _vf2i_dnu(Func<float, int> RoundingFunc)
		{
			return VEC_VD_i.SetVector(Index =>
				ast.CallStatic(
					RoundingFunc,
					ast.CallStatic(
						(Func<float, int, float>)MathFloat.Scalb,
						VEC_VS[Index],
						(int)Instruction.IMM5
					)
				)
			, PC);
		}

		public AstNodeStm vf2id() { return _vf2i_dnu(MathFloat.Floor); }
		public AstNodeStm vf2in() { return _vf2i_dnu(MathFloat.Round); }
		public AstNodeStm vf2iu() { return _vf2i_dnu(MathFloat.Ceil); }

		public AstNodeStm vf2iz()
		{
			return VEC_VD_i.SetVector(Index =>
				ast.CallStatic(
					(Func<float, int, int>)CpuEmitterUtils._vf2iz,
					VEC_VS[Index],
					(int)Instruction.IMM5
				)
			, PC);
		}

		public AstNodeStm vi2s()
		{
			int VectorSize = ONE_TWO;
			return _Vector(VD, VType.VUInt, VectorSize / 2)
				.SetVector(Index => ast.CallStatic(
					(Func<uint, uint, uint>)CpuEmitterUtils._vi2s_impl,
					VEC_VS_u[Index * 2 + 0],
					VEC_VS_u[Index * 2 + 1]
				), PC)
			;
		}

		public AstNodeStm vf2h() 
		{
			var VEC_VD = VEC(VD, VType.VUInt, ONE_TWO / 2);
			var VEC_VS = VEC(VS, VType.VFloat, ONE_TWO);
			return VEC_VD.SetVector(Index =>
				ast.CallStatic(
					(Func<float, float, uint>)CpuEmitterUtils._vf2h_impl,
					VEC_VS[Index * 2 + 0],
					VEC_VS[Index * 2 + 1]
				)
			, PC);
		}

		public AstNodeStm vh2f()
		{
			var VEC_VD = VEC(VD, VType.VFloat, ONE_TWO * 2);
			var VEC_VS = VEC(VS, VType.VUInt, ONE_TWO);
			return VEC_VD.SetVector(Index =>
			{
				return ast.CallStatic(
					(((Index % 2) == 0) ? (Func<uint, float>)CpuEmitterUtils._vh2f_0 : (Func<uint, float>)CpuEmitterUtils._vh2f_1),
					VEC_VS[Index / 2]
				);
			}, PC);
		}
		public AstNodeStm vi2us()
		{
			int VectorSize = ONE_TWO;
			return _Vector(VD, VType.VInt, VectorSize / 2)
				.SetVector(Index => ast.CallStatic(
					(Func<int, int, int>)CpuEmitterUtils._vi2us_impl,
					VEC_VS_i[Index * 2 + 0],
					VEC_VS_i[Index * 2 + 1]
				), PC)
			;
		}

		public AstNodeStm vmfvc() { return ast.NotImplemented("vmfvc"); }
		public AstNodeStm vmtvc() { return ast.NotImplemented("vmtvc"); }

		public AstNodeStm mtv() { return CEL_VD.Set(ast.GPR_f(RT), PC); }
		public AstNodeStm mtvc() {
			return ast.Statement(ast.CallStatic(
				(Action<CpuThreadState, VfpuControlRegistersEnum, uint>)CpuEmitterUtils._mtvc_impl,
				ast.CpuThreadState,
				ast.Cast<VfpuControlRegistersEnum>((int)(Instruction.IMM7 + 128), false),
				CEL_VD_u.Get()
			));
			//_mtvc_impl
		}

		/// <summary>
		/// Copies a vfpu control register into a general purpose register.
		/// </summary>
		public AstNodeStm mfvc()
		{
			return ast.AssignGPR(RT, ast.CallStatic(
				(Func<CpuThreadState, VfpuControlRegistersEnum, uint>)CpuEmitterUtils._mfvc_impl,
				ast.CpuThreadState,
				ast.Cast<VfpuControlRegistersEnum>((int)(Instruction.IMM7 + 128), false)
			));
		}

		// Move From/to Vfpu (C?)_
		public AstNodeStm mfv() { return ast.AssignGPR_F(RT, CEL_VD.Get()); }

		// Load/Store Vfpu (Left/Right)_
		// ID("lv.q",        VM("110110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		public AstNodeStm lv_q()
		{
			const int VectorSize = 4;
			var Dest = _Vector(VT5_1, VFloat, Size: VectorSize);
			var MemoryVector = _MemoryVectorIMM14<float>(VectorSize);
			return Dest.SetVector(Index => MemoryVector[Index], PC);
		}

		/// <summary>
		/// ID("sv.q",        VM("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public AstNodeStm sv_q()
		{
			int VectorSize = 4;
			var Dest = _Vector(VT5_1, VFloat, VectorSize);
			var MemoryVector = _MemoryVectorIMM14<float>(VectorSize);
			return MemoryVector.SetVector(Index => Dest[Index]);
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

		public AstNodeStm lv_s() { return _Cell(VT5_2).Set(ast.MemoryGetValue<float>(Memory, Address_RS_IMM14()), PC); }
		public AstNodeStm sv_s() { return ast.MemorySetValue<float>(Memory, Address_RS_IMM14(), _Cell(VT5_2).Get()); }
	}
}
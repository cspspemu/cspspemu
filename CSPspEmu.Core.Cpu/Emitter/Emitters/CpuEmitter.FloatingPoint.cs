using System;
using CSharpUtils;
using SafeILGenerator;
using SafeILGenerator.Ast;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Binary Floating Point Unit Operations
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void add_s() { this.GenerateAssignFPR_F(FD, FPR(FS) + FPR(FT)); }
		public void sub_s() { this.GenerateAssignFPR_F(FD, FPR(FS) - FPR(FT)); }
		public void mul_s() { this.GenerateAssignFPR_F(FD, FPR(FS) * FPR(FT)); }
		public void div_s() { this.GenerateAssignFPR_F(FD, FPR(FS) / FPR(FT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Unary Floating Point Unit Operations
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void sqrt_s() { this.GenerateAssignFPR_F(FD, this.CallStatic((Func<float, float>)MathFloat.Sqrt, FPR(FS))); }
		public void abs_s() { this.GenerateAssignFPR_F(FD, this.CallStatic((Func<float, float>)MathFloat.Abs, FPR(FS))); }
		public void mov_s() { this.GenerateAssignFPR_F(FD, FPR(FS)); }
		public void neg_s() { this.GenerateAssignFPR_F(FD, -FPR(FS)); }
		public void trunc_w_s() { this.GenerateAssignFPR_I(FD, this.CallStatic((Func<float, int>)MathFloat.Cast, FPR(FS))); }
		public void round_w_s() { this.GenerateAssignFPR_I(FD, this.CallStatic((Func<float, int>)MathFloat.Round, FPR(FS))); }
		public void ceil_w_s() { this.GenerateAssignFPR_I(FD, this.CallStatic((Func<float, int>)MathFloat.Ceil, FPR(FS))); }
		public void floor_w_s() { this.GenerateAssignFPR_I(FD, this.CallStatic((Func<float, int>)MathFloat.Floor, FPR(FS))); }

		/// <summary>
		/// Convert FS register (stored as an int) to float and stores the result on FD.
		/// </summary>
		public void cvt_s_w() { this.GenerateAssignFPR_F(FD, this.Cast<float>(FPR_I(FS))); }

		/// <summary>
		/// Floating-Point Convert to Word Fixed-Point
		/// </summary>
		public void cvt_w_s() { this.GenerateAssignFPR_I(FD, this.CallStatic((Func<CpuThreadState, float, int>)CpuEmitterUtils._cvt_w_s_impl, this.CpuThreadStateArgument(), FPR(FS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (from/to) float point registers (reinterpreted)
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void mfc1() { this.GenerateAssignGPR(RT, this.CallStatic((Func<float, int>)MathFloat.ReinterpretFloatAsInt, FPR(FS))); }
		public void mtc1() { this.GenerateAssignFPR_F(FS, this.CallStatic((Func<int, float>)MathFloat.ReinterpretIntAsFloat, GPR_s(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// CFC1 -- move Control word from/to floating point (C1)
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void cfc1() { GenerateIL(this.Statement(this.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._cfc1_impl, this.CpuThreadStateArgument(), RD, RT))); }
		public void ctc1() { GenerateIL(this.Statement(this.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._ctc1_impl, this.CpuThreadStateArgument(), RD, RT))); }

		/// <summary>
		/// Compare (condition) Single_
		/// </summary>
		/// <param name="fc02"></param>
		/// <param name="fc3"></param>
		private void _comp(int fc02, int fc3)
		{
			bool fc_unordererd = ((fc02 & 1) != 0);
			bool fc_equal = ((fc02 & 2) != 0);
			bool fc_less = ((fc02 & 4) != 0);
			bool fc_inv_qnan = (fc3 != 0); // TODO -- Only used for detecting invalid operations?

			//MipsMethodEmitter.LoadFPR(FS);
			//MipsMethodEmitter.LoadFPR(FT);

			GenerateIL(this.Statement(this.CallStatic(
				(Action<CpuThreadState, float, float, bool, bool, bool, bool>)CpuEmitterUtils._comp_impl,
				this.CpuThreadStateArgument(),
				FPR(FS),
				FPR(FT),
				this.Immediate(fc_unordererd),
				this.Immediate(fc_equal),
				this.Immediate(fc_less),
				this.Immediate(fc_inv_qnan)
			)));
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// c.f.s: Compare False Single : This predicate is always False it never has a True Result
		// c.un.s: Compare UNordered Single
		// c.eq.s: Compare EQual Single
		// c.ueq.s: Compare Unordered or EQual Single
		// c.olt.s: Compare Ordered or Less Than Single
		// c.ult.s: Compare Unordered or Less Than Single
		// c.ole.s: Compare Ordered or Less than or Equal Single
		// c.ule.s: Compare Unordered or Less than or Equal Single
		// c.sf.s: Compare Signaling False Single : This predicate always False
		// c.ngle.s: Compare Non Greater Than or Less than or Equal Single
		// c.seq.s: Compare Signaling Equal Single
		// c.ngl.s: Compare Not Greater than or Less than Single
		// c.lt.s: Compare Less Than Single
		// c.nge.s: Compare Not Greater than or Equal Single
		// c.le.s: Compare Less than or Equal Single
		// c.ngt.s: Compare Not Greater Than Single
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void c_f_s() { _comp(0, 0); }
		public void c_un_s() { _comp(1, 0); }
		public void c_eq_s() { _comp(2, 0); }
		public void c_ueq_s() { _comp(3, 0); }
		public void c_olt_s() { _comp(4, 0); }
		public void c_ult_s() { _comp(5, 0); }
		public void c_ole_s() { _comp(6, 0); }
		public void c_ule_s() { _comp(7, 0); }

		public void c_sf_s() { _comp(0, 1); }
		public void c_ngle_s() { _comp(1, 1); }
		public void c_seq_s() { _comp(2, 1); }
		public void c_ngl_s() { _comp(3, 1); }
		public void c_lt_s() { _comp(4, 1); }
		public void c_nge_s() { _comp(5, 1); }
		public void c_le_s() { _comp(6, 1); }
		public void c_ngt_s() { _comp(7, 1); }
	}
}

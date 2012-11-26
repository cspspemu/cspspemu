using System;
using CSharpUtils;
using SafeILGenerator;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Binary Floating Point Unit Operations
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm add_s() { return AssignFPR_F(FD, FPR(FS) + FPR(FT)); }
		public AstNodeStm sub_s() { return AssignFPR_F(FD, FPR(FS) - FPR(FT)); }
		public AstNodeStm mul_s() { return AssignFPR_F(FD, FPR(FS) * FPR(FT)); }
		public AstNodeStm div_s() { return AssignFPR_F(FD, FPR(FS) / FPR(FT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Unary Floating Point Unit Operations
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm sqrt_s() { return AssignFPR_F(FD, ast.CallStatic((Func<float, float>)MathFloat.Sqrt, FPR(FS))); }
		public AstNodeStm abs_s() { return AssignFPR_F(FD, ast.CallStatic((Func<float, float>)MathFloat.Abs, FPR(FS))); }
		public AstNodeStm mov_s() { return AssignFPR_F(FD, FPR(FS)); }
		public AstNodeStm neg_s() { return AssignFPR_F(FD, -FPR(FS)); }
		public AstNodeStm trunc_w_s() { return AssignFPR_I(FD, ast.CallStatic((Func<float, int>)MathFloat.Cast, FPR(FS))); }
		public AstNodeStm round_w_s() { return AssignFPR_I(FD, ast.CallStatic((Func<float, int>)MathFloat.Round, FPR(FS))); }
		public AstNodeStm ceil_w_s() { return AssignFPR_I(FD, ast.CallStatic((Func<float, int>)MathFloat.Ceil, FPR(FS))); }
		public AstNodeStm floor_w_s() { return AssignFPR_I(FD, ast.CallStatic((Func<float, int>)MathFloat.Floor, FPR(FS))); }

		/// <summary>
		/// Convert FS register (stored as an int) to float and stores the result on FD.
		/// </summary>
		public AstNodeStm cvt_s_w() { return AssignFPR_F(FD, ast.Cast<float>(FPR_I(FS))); }

		/// <summary>
		/// Floating-Point Convert to Word Fixed-Point
		/// </summary>
		public AstNodeStm cvt_w_s() { return AssignFPR_I(FD, ast.CallStatic((Func<CpuThreadState, float, int>)CpuEmitterUtils._cvt_w_s_impl, this.CpuThreadStateArgument(), FPR(FS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (from/to) float point registers (reinterpreted)
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfc1() { return AssignGPR(RT, ast.CallStatic((Func<float, int>)MathFloat.ReinterpretFloatAsInt, FPR(FS))); }
		public AstNodeStm mtc1() { return AssignFPR_F(FS, ast.CallStatic((Func<int, float>)MathFloat.ReinterpretIntAsFloat, GPR_s(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// CFC1 -- move Control word from/to floating point (C1)
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm cfc1() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._cfc1_impl, this.CpuThreadStateArgument(), RD, RT)); }
		public AstNodeStm ctc1() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._ctc1_impl, this.CpuThreadStateArgument(), RD, RT)); }

		/// <summary>
		/// Compare (condition) Single_
		/// </summary>
		/// <param name="fc02"></param>
		/// <param name="fc3"></param>
		private AstNodeStm _comp(int fc02, int fc3)
		{
			bool fc_unordererd = ((fc02 & 1) != 0);
			bool fc_equal = ((fc02 & 2) != 0);
			bool fc_less = ((fc02 & 4) != 0);
			bool fc_inv_qnan = (fc3 != 0); // TODO -- Only used for detecting invalid operations?

			//MipsMethodEmitter.LoadFPR(FS);
			//MipsMethodEmitter.LoadFPR(FT);

			return ast.Statement(ast.CallStatic(
				(Action<CpuThreadState, float, float, bool, bool, bool, bool>)CpuEmitterUtils._comp_impl,
				this.CpuThreadStateArgument(),
				FPR(FS),
				FPR(FT),
				ast.Immediate(fc_unordererd),
				ast.Immediate(fc_equal),
				ast.Immediate(fc_less),
				ast.Immediate(fc_inv_qnan)
			));
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
		public AstNodeStm c_f_s() { return _comp(0, 0); }
		public AstNodeStm c_un_s() { return _comp(1, 0); }
		public AstNodeStm c_eq_s() { return _comp(2, 0); }
		public AstNodeStm c_ueq_s() { return _comp(3, 0); }
		public AstNodeStm c_olt_s() { return _comp(4, 0); }
		public AstNodeStm c_ult_s() { return _comp(5, 0); }
		public AstNodeStm c_ole_s() { return _comp(6, 0); }
		public AstNodeStm c_ule_s() { return _comp(7, 0); }

		public AstNodeStm c_sf_s() { return _comp(0, 1); }
		public AstNodeStm c_ngle_s() { return _comp(1, 1); }
		public AstNodeStm c_seq_s() { return _comp(2, 1); }
		public AstNodeStm c_ngl_s() { return _comp(3, 1); }
		public AstNodeStm c_lt_s() { return _comp(4, 1); }
		public AstNodeStm c_nge_s() { return _comp(5, 1); }
		public AstNodeStm c_le_s() { return _comp(6, 1); }
		public AstNodeStm c_ngt_s() { return _comp(7, 1); }
	}
}

using System;
using SafeILGenerator.Ast.Nodes;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Binary Floating Point Unit Operations
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm add_s() { return ast.AssignFPR_F(FD, ast.FPR(FS) + ast.FPR(FT)); }
		public AstNodeStm sub_s() { return ast.AssignFPR_F(FD, ast.FPR(FS) - ast.FPR(FT)); }
		public AstNodeStm mul_s() { return ast.AssignFPR_F(FD, ast.FPR(FS) * ast.FPR(FT)); }
		public AstNodeStm div_s() { return ast.AssignFPR_F(FD, ast.FPR(FS) / ast.FPR(FT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Unary Floating Point Unit Operations
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm sqrt_s() { return ast.AssignFPR_F(FD, ast.CallStatic((Func<float, float>)MathFloat.Sqrt, ast.FPR(FS))); }
		public AstNodeStm abs_s() { return ast.AssignFPR_F(FD, ast.CallStatic((Func<float, float>)MathFloat.Abs, ast.FPR(FS))); }
		public AstNodeStm mov_s() { return ast.AssignFPR_F(FD, ast.FPR(FS)); }
		public AstNodeStm neg_s() { return ast.AssignFPR_F(FD, -ast.FPR(FS)); }
		public AstNodeStm trunc_w_s() { return ast.AssignFPR_I(FD, ast.CallStatic((Func<float, int>)MathFloat.Cast, ast.FPR(FS))); }
		public AstNodeStm round_w_s() { return ast.AssignFPR_I(FD, ast.CallStatic((Func<float, int>)MathFloat.Round, ast.FPR(FS))); }
		public AstNodeStm ceil_w_s() { return ast.AssignFPR_I(FD, ast.CallStatic((Func<float, int>)MathFloat.Ceil, ast.FPR(FS))); }
		public AstNodeStm floor_w_s() { return ast.AssignFPR_I(FD, ast.CallStatic((Func<float, int>)MathFloat.Floor, ast.FPR(FS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Convert FS register (stored as an int) to float and stores the result on FD.
		// Floating-Point Convert to Word Fixed-Point
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm cvt_s_w() { return ast.AssignFPR_F(FD, ast.Cast<float>(ast.FPR_I(FS))); }
		public AstNodeStm cvt_w_s() { return ast.AssignFPR_I(FD, ast.CallStatic((Func<CpuThreadState, float, int>)CpuEmitterUtils._cvt_w_s_impl, ast.CpuThreadState, ast.FPR(FS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (from/to) float point registers (reinterpreted)
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfc1() { return ast.AssignGPR(RT, ast.CallStatic((Func<float, int>)MathFloat.ReinterpretFloatAsInt, ast.FPR(FS))); }
		public AstNodeStm mtc1() { return ast.AssignFPR_F(FS, ast.CallStatic((Func<int, float>)MathFloat.ReinterpretIntAsFloat, ast.GPR_s(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Word to Cop1 floating point.
		// Store Word from Cop1 floating point.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm lwc1() { return ast.AssignFPR_I(FT, ast.MemoryGetValue<int>(Memory, this.Address_RS_IMM())); }
		public AstNodeStm swc1() { return ast.MemorySetValue<int>(Memory, this.Address_RS_IMM(), ast.FPR_I(FT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// CFC1 -- move Control word from/to floating point (C1)
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm cfc1() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._cfc1_impl, ast.CpuThreadState, RD, RT)); }
		public AstNodeStm ctc1() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._ctc1_impl, ast.CpuThreadState, RD, RT)); }

		/// <summary>
		/// Compare (condition) Single_
		/// </summary>
		/// <param name="fc02"></param>
		/// <param name="fc3"></param>
		private AstNodeStm _comp(int fc02, int fc3)
		{
			var fc_unordererd = ((fc02 & 1) != 0);
			var fc_equal = ((fc02 & 2) != 0);
			var fc_less = ((fc02 & 4) != 0);
			var fc_inv_qnan = (fc3 != 0); // TODO -- Only used for detecting invalid operations?

			//if (float.IsNaN(s) || float.IsNaN(t))
			//{
			//	CpuThreadState.Fcr31.CC = fc_unordererd;
			//}
			//else
			//{
			//	//bool cc = false;
			//	//if (fc_equal) cc = cc || (s == t);
			//	//if (fc_less) cc = cc || (s < t);
			//	//return cc;
			//	bool equal = (fc_equal) && (s == t);
			//	bool less = (fc_less) && (s < t);
			//
			//	CpuThreadState.Fcr31.CC = (less || equal);
			//}

			//MipsMethodEmitter.LoadFPR(FS);
			//MipsMethodEmitter.LoadFPR(FT);

			return ast.Statement(ast.CallStatic(
				(Action<CpuThreadState, float, float, bool, bool, bool, bool>)CpuEmitterUtils._comp_impl,
				ast.CpuThreadState,
				ast.FPR(FS),
				ast.FPR(FT),
				ast.Immediate(fc_unordererd),
				ast.Immediate(fc_equal),
				ast.Immediate(fc_less),
				ast.Immediate(fc_inv_qnan)
			));
		}

		//private AstNodeStm _comp_assign(AstNodeExpr Value)
		//{
		//	return ast.Assign();
		//}

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

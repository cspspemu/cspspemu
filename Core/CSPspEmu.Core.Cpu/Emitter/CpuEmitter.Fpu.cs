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
        public AstNodeStm add_s() => ast.AssignFPR_F(FD, ast.Fpr(FS) + ast.Fpr(FT));
        public AstNodeStm sub_s() => ast.AssignFPR_F(FD, ast.Fpr(FS) - ast.Fpr(FT));
        public AstNodeStm mul_s() => ast.AssignFPR_F(FD, ast.Fpr(FS) * ast.Fpr(FT));
        public AstNodeStm div_s() => ast.AssignFPR_F(FD, ast.Fpr(FS) / ast.Fpr(FT));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Unary Floating Point Unit Operations
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm sqrt_s() => ast.AssignFPR_F(FD, ast.CallStatic((Func<float, float>) MathFloat.Sqrt, ast.Fpr(FS)));
        public AstNodeStm abs_s() => ast.AssignFPR_F(FD, ast.CallStatic((Func<float, float>) MathFloat.Abs, ast.Fpr(FS)));
        public AstNodeStm mov_s() => ast.AssignFPR_F(FD, ast.Fpr(FS));
        public AstNodeStm neg_s() => ast.AssignFPR_F(FD, -ast.Fpr(FS));
        public AstNodeStm trunc_w_s() => ast.AssignFPR_I(FD, ast.CallStatic((Func<float, int>) MathFloat.Cast, ast.Fpr(FS)));
        public AstNodeStm round_w_s() => ast.AssignFPR_I(FD, ast.CallStatic((Func<float, int>) MathFloat.Round, ast.Fpr(FS)));
        public AstNodeStm ceil_w_s() => ast.AssignFPR_I(FD, ast.CallStatic((Func<float, int>) MathFloat.Ceil, ast.Fpr(FS)));
        public AstNodeStm floor_w_s() => ast.AssignFPR_I(FD, ast.CallStatic((Func<float, int>) MathFloat.Floor, ast.Fpr(FS)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Convert FS register (stored as an int) to float and stores the result on FD.
        // Floating-Point Convert to Word Fixed-Point
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm cvt_s_w() => ast.AssignFPR_F(FD, ast.Cast<float>(ast.FPR_I(FS)));

        public AstNodeStm cvt_w_s() => ast.AssignFPR_I(FD,
            ast.CallStatic((Func<CpuThreadState, float, int>) CpuEmitterUtils._cvt_w_s_impl, ast.CpuThreadStateExpr,
                ast.Fpr(FS)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (from/to) float point registers (reinterpreted)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm mfc1() => ast.AssignGpr(RT, ast.CallStatic((Func<float, int>) MathFloat.ReinterpretFloatAsInt, ast.Fpr(FS)));

        public AstNodeStm mtc1() => ast.AssignFPR_F(FS,
            ast.CallStatic((Func<int, float>) MathFloat.ReinterpretIntAsFloat, ast.GPR_s(RT)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Word to Cop1 floating point.
        // Store Word from Cop1 floating point.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm lwc1() => ast.AssignFPR_I(FT, ast.MemoryGetValue<int>(Memory, this.Address_RS_IMM()));

        public AstNodeStm swc1() => ast.MemorySetValue<int>(Memory, this.Address_RS_IMM(), ast.FPR_I(FT));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // CFC1 -- move Control word from/to floating point (C1)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm cfc1() => ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>) CpuEmitterUtils._cfc1_impl,
            ast.CpuThreadStateExpr, RD, RT));

        public AstNodeStm ctc1() => ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>) CpuEmitterUtils._ctc1_impl,
            ast.CpuThreadStateExpr, RD, RT));

        /// <summary>
        /// Compare (condition) Single_
        /// </summary>
        /// <param name="fc02"></param>
        /// <param name="fc3"></param>
        private AstNodeStm _comp(int fc02, int fc3)
        {
            var fcUnordererd = ((fc02 & 1) != 0);
            var fcEqual = ((fc02 & 2) != 0);
            var fcLess = ((fc02 & 4) != 0);
            var fcInvQnan = (fc3 != 0); // TODO -- Only used for detecting invalid operations?

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
                (Action<CpuThreadState, float, float, bool, bool, bool, bool>) CpuEmitterUtils._comp_impl,
                ast.CpuThreadStateExpr,
                ast.Fpr(FS),
                ast.Fpr(FT),
                ast.Immediate(fcUnordererd),
                ast.Immediate(fcEqual),
                ast.Immediate(fcLess),
                ast.Immediate(fcInvQnan)
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
        public AstNodeStm c_f_s() => _comp(0, 0);
        public AstNodeStm c_un_s() => _comp(1, 0);
        public AstNodeStm c_eq_s() => _comp(2, 0);
        public AstNodeStm c_ueq_s() => _comp(3, 0);
        public AstNodeStm c_olt_s() => _comp(4, 0);
        public AstNodeStm c_ult_s() => _comp(5, 0);
        public AstNodeStm c_ole_s() => _comp(6, 0);
        public AstNodeStm c_ule_s() => _comp(7, 0);
        public AstNodeStm c_sf_s() => _comp(0, 1);
        public AstNodeStm c_ngle_s() => _comp(1, 1);
        public AstNodeStm c_seq_s() => _comp(2, 1);
        public AstNodeStm c_ngl_s() => _comp(3, 1);
        public AstNodeStm c_lt_s() => _comp(4, 1);
        public AstNodeStm c_nge_s() => _comp(5, 1);
        public AstNodeStm c_le_s() => _comp(6, 1);
        public AstNodeStm c_ngt_s() => _comp(7, 1);
    }
}
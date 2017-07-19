﻿using System;
using SafeILGenerator.Ast.Nodes;
using CSharpUtils;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Binary Floating Point Unit Operations
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("add.s")]
        public AstNodeStm add_s() => _ast.AssignFPR_F(Fd, _ast.Fpr(Fs) + _ast.Fpr(Ft));

        [InstructionName("sub.s")]
        public AstNodeStm sub_s() => _ast.AssignFPR_F(Fd, _ast.Fpr(Fs) - _ast.Fpr(Ft));

        [InstructionName("mul.s")]
        public AstNodeStm mul_s() => _ast.AssignFPR_F(Fd, _ast.Fpr(Fs) * _ast.Fpr(Ft));

        [InstructionName("div.s")]
        public AstNodeStm div_s() => _ast.AssignFPR_F(Fd, _ast.Fpr(Fs) / _ast.Fpr(Ft));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Unary Floating Point Unit Operations
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("sqrt.s")]
        public AstNodeStm sqrt_s() => _ast.AssignFPR_F(Fd, _ast.CallStatic((Func<float, float>) MathFloat.Sqrt, _ast.Fpr(Fs)));

        [InstructionName("abs.s")]
        public AstNodeStm abs_s() => _ast.AssignFPR_F(Fd, _ast.CallStatic((Func<float, float>) MathFloat.Abs, _ast.Fpr(Fs)));

        [InstructionName("mov.s")]
        public AstNodeStm mov_s() => _ast.AssignFPR_F(Fd, _ast.Fpr(Fs));

        [InstructionName("neg.s")]
        public AstNodeStm neg_s() => _ast.AssignFPR_F(Fd, -_ast.Fpr(Fs));

        [InstructionName("trunc.w.s")]
        public AstNodeStm trunc_w_s() => _ast.AssignFPR_I(Fd, _ast.CallStatic((Func<float, int>) MathFloat.Cast, _ast.Fpr(Fs)));

        [InstructionName("round.w.s")]
        public AstNodeStm round_w_s() => _ast.AssignFPR_I(Fd, _ast.CallStatic((Func<float, int>) MathFloat.Round, _ast.Fpr(Fs)));

        [InstructionName("ceil.w.s")]
        public AstNodeStm ceil_w_s() => _ast.AssignFPR_I(Fd, _ast.CallStatic((Func<float, int>) MathFloat.Ceil, _ast.Fpr(Fs)));

        [InstructionName("floor.w.s")]
        public AstNodeStm floor_w_s() => _ast.AssignFPR_I(Fd, _ast.CallStatic((Func<float, int>) MathFloat.Floor, _ast.Fpr(Fs)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Convert FS register (stored as an int) to float and stores the result on FD.
        // Floating-Point Convert to Word Fixed-Point
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("cvt.s.w")]
        public AstNodeStm cvt_s_w() => _ast.AssignFPR_F(Fd, _ast.Cast<float>(_ast.FPR_I(Fs)));

        [InstructionName("cvt.w.s")]
        public AstNodeStm cvt_w_s() => _ast.AssignFPR_I(Fd,
            _ast.CallStatic((Func<CpuThreadState, float, int>) CpuEmitterUtils._cvt_w_s_impl, _ast.CpuThreadStateExpr,
                _ast.Fpr(Fs)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (from/to) float point registers (reinterpreted)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("mfc1")]
        public AstNodeStm mfc1() => _ast.AssignGpr(Rt, _ast.CallStatic((Func<float, int>) MathFloat.ReinterpretFloatAsInt, _ast.Fpr(Fs)));

        [InstructionName("mtc1")]
        public AstNodeStm mtc1() => _ast.AssignFPR_F(Fs,
            _ast.CallStatic((Func<int, float>) MathFloat.ReinterpretIntAsFloat, _ast.GPR_s(Rt)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Word to Cop1 floating point.
        // Store Word from Cop1 floating point.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("lwc1")]
        public AstNodeStm lwc1() => _ast.AssignFPR_I(Ft, _ast.MemoryGetValue<int>(_memory, this.Address_RS_IMM()));

        [InstructionName("swc1")]
        public AstNodeStm swc1() => _ast.MemorySetValue<int>(_memory, this.Address_RS_IMM(), _ast.FPR_I(Ft));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // CFC1 -- move Control word from/to floating point (C1)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("cfc1")]
        public AstNodeStm cfc1() => _ast.Statement(_ast.CallStatic((Action<CpuThreadState, int, int>) CpuEmitterUtils._cfc1_impl,
            _ast.CpuThreadStateExpr, Rd, Rt));

        [InstructionName("ctc1")]
        public AstNodeStm ctc1() => _ast.Statement(_ast.CallStatic((Action<CpuThreadState, int, int>) CpuEmitterUtils._ctc1_impl,
            _ast.CpuThreadStateExpr, Rd, Rt));

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

            return _ast.Statement(_ast.CallStatic(
                (Action<CpuThreadState, float, float, bool, bool, bool, bool>) CpuEmitterUtils._comp_impl,
                _ast.CpuThreadStateExpr,
                _ast.Fpr(Fs),
                _ast.Fpr(Ft),
                _ast.Immediate(fcUnordererd),
                _ast.Immediate(fcEqual),
                _ast.Immediate(fcLess),
                _ast.Immediate(fcInvQnan)
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
        [InstructionName("c.f.s")]
        public AstNodeStm c_f_s() => _comp(0, 0);

        [InstructionName("c.un.s")]
        public AstNodeStm c_un_s() => _comp(1, 0);

        [InstructionName("c.eq.s")]
        public AstNodeStm c_eq_s() => _comp(2, 0);

        [InstructionName("c.ueq.s")]
        public AstNodeStm c_ueq_s() => _comp(3, 0);

        [InstructionName("c.olt.s")]
        public AstNodeStm c_olt_s() => _comp(4, 0);

        [InstructionName("c.ult.s")]
        public AstNodeStm c_ult_s() => _comp(5, 0);

        [InstructionName("c.ole.s")]
        public AstNodeStm c_ole_s() => _comp(6, 0);

        [InstructionName("c.ule.s")]
        public AstNodeStm c_ule_s() => _comp(7, 0);

        [InstructionName("c.sf.s")]
        public AstNodeStm c_sf_s() => _comp(0, 1);

        [InstructionName("c.ngle.s")]
        public AstNodeStm c_ngle_s() => _comp(1, 1);

        [InstructionName("c.seq.s")]
        public AstNodeStm c_seq_s() => _comp(2, 1);

        [InstructionName("c.ngl.s")]
        public AstNodeStm c_ngl_s() => _comp(3, 1);

        [InstructionName("c.lt.s")]
        public AstNodeStm c_lt_s() => _comp(4, 1);

        [InstructionName("c.nge.s")]
        public AstNodeStm c_nge_s() => _comp(5, 1);

        [InstructionName("c.le.s")]
        public AstNodeStm c_le_s() => _comp(6, 1);

        [InstructionName("c.ngt.s")]
        public AstNodeStm c_ngt_s() => _comp(7, 1);
    }
}
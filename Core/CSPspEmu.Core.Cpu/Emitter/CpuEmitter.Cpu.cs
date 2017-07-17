using System;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic operations.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm add() => ast.AssignGpr(RD, ast.GPR_s(RS) + ast.GPR_s(RT));

        public AstNodeStm addu() => ast.AssignGpr(RD, ast.GPR_u(RS) + ast.GPR_u(RT));
        public AstNodeStm sub() => ast.AssignGpr(RD, ast.GPR_s(RS) - ast.GPR_s(RT));
        public AstNodeStm subu() => ast.AssignGpr(RD, ast.GPR_u(RS) - ast.GPR_u(RT));
        public AstNodeStm addi() => ast.AssignGpr(RT, ast.GPR_s(RS) + IMM_s());
        public AstNodeStm addiu() => ast.AssignGpr(RT, ast.GPR_s(RS) + IMM_s());

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Logical Operations.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm and() => ast.AssignGpr(RD, ast.GPR_u(RS) & ast.GPR_u(RT));
        public AstNodeStm or() => ast.AssignGpr(RD, ast.GPR_u(RS) | ast.GPR_u(RT));
        public AstNodeStm xor() => ast.AssignGpr(RD, ast.GPR_u(RS) ^ ast.GPR_u(RT));
        public AstNodeStm nor() => ast.AssignGpr(RD, ~(ast.GPR_u(RS) | ast.GPR_u(RT)));
        public AstNodeStm andi() => ast.AssignGpr(RT, ast.GPR_u(RS) & IMM_u());
        public AstNodeStm ori() => ast.AssignGpr(RT, ast.GPR_u(RS) | IMM_u());
        public AstNodeStm xori() => ast.AssignGpr(RT, ast.GPR_u(RS) ^ IMM_u());

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Shift Left/Right Logical/Arithmethic (Variable).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm sll() => ast.AssignGpr(RD, ast.Binary(ast.GPR_u(RT), "<<", ast.Immediate((uint) Instruction.POS)));
        public AstNodeStm sra() => ast.AssignGpr(RD, ast.Binary(ast.GPR_s(RT), ">>", ast.Immediate((int) Instruction.POS)));
        public AstNodeStm srl() => ast.AssignGpr(RD, ast.Binary(ast.GPR_u(RT), ">>", ast.Immediate((uint) Instruction.POS)));

        public AstNodeStm rotr() => ast.AssignGpr(RD,
            ast.CallStatic((Func<uint, int, uint>) CpuEmitterUtils._rotr_impl, ast.GPR_u(RT),
                ast.Immediate((int) Instruction.POS)));

        public AstNodeStm sllv() => ast.AssignGpr(RD, ast.Binary(ast.GPR_u(RT), "<<", ast.GPR_u(RS) & 31));
        public AstNodeStm srav() => ast.AssignGpr(RD, ast.Binary(ast.GPR_s(RT), ">>", ast.GPR_s(RS) & 31));
        public AstNodeStm srlv() => ast.AssignGpr(RD, ast.Binary(ast.GPR_u(RT), ">>", ast.GPR_u(RS) & 31));
        public AstNodeStm rotrv() => ast.AssignGpr(RD,
            ast.CallStatic((Func<uint, int, uint>) CpuEmitterUtils._rotr_impl, ast.GPR_u(RT), ast.GPR_s(RS)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Set Less Than (Immediate) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm slt() => ast.AssignGpr(RD, ast.Binary(ast.GPR_s(RS), "<", ast.GPR_s(RT)));
        public AstNodeStm sltu() => ast.AssignGpr(RD, ast.Binary(ast.GPR_u(RS), "<", ast.GPR_u(RT)));
        public AstNodeStm slti() => ast.AssignGpr(RT, ast.Binary(ast.GPR_s(RS), "<", IMM_s()));
        public AstNodeStm sltiu() => ast.AssignGpr(RT, ast.Binary(ast.GPR_u(RS), "<", IMM_uex()));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Upper Immediate.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm lui() => ast.AssignGpr(RT, ast.Binary(IMM_u(), "<<", ast.Immediate((uint) 16)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Sign Extend Byte/Half word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm seb() => ast.AssignGpr(RD, ast.Cast<int>(ast.Cast<sbyte>(ast.GPR_u(RT))));
        public AstNodeStm seh() => ast.AssignGpr(RD, ast.Cast<int>(ast.Cast<short>(ast.GPR_u(RT))));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // BIT REVerse.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm bitrev() => ast.AssignGpr(RD, ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._bitrev_impl, ast.GPR_u(RT)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MAXimum/MINimum.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm max() => ast.AssignGpr(RD,
            ast.CallStatic((Func<int, int, int>) CpuEmitterUtils._max_impl, ast.GPR_s(RS), ast.GPR_s(RT)));

        public AstNodeStm min() => ast.AssignGpr(RD,
            ast.CallStatic((Func<int, int, int>) CpuEmitterUtils._min_impl, ast.GPR_s(RS), ast.GPR_s(RT)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // DIVide (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm div() => (ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>) CpuEmitterUtils._div_impl,
            ast.CpuThreadStateExpr, ast.GPR_s(RS), ast.GPR_s(RT))));

        public AstNodeStm divu() => (ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>) CpuEmitterUtils._divu_impl,
            ast.CpuThreadStateExpr, ast.GPR_u(RS), ast.GPR_u(RT))));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MULTiply (ADD/SUBstract) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm mult() => ast.AssignHilo(ast.GPR_sl(RS) * ast.GPR_sl(RT));
        public AstNodeStm multu() => ast.AssignHilo(ast.GPR_ul(RS) * ast.GPR_ul(RT));
        public AstNodeStm madd() => ast.AssignHilo(ast.HILO_sl() + ast.GPR_sl(RS) * ast.GPR_sl(RT));
        public AstNodeStm maddu() => ast.AssignHilo(ast.HILO_ul() + ast.GPR_ul(RS) * ast.GPR_ul(RT));
        public AstNodeStm msub() => ast.AssignHilo(ast.HILO_sl() - ast.GPR_sl(RS) * ast.GPR_sl(RT));
        public AstNodeStm msubu() => ast.AssignHilo(ast.HILO_ul() - ast.GPR_ul(RS) * ast.GPR_ul(RT));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move To/From HI/LO.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm mfhi() => ast.AssignGpr(RD, ast.Cast<uint>(ast.Hi()));
        public AstNodeStm mflo() => ast.AssignGpr(RD, ast.Cast<uint>(ast.Lo()));
        public AstNodeStm mthi() => ast.AssignHi(ast.GPR_s(RS));
        public AstNodeStm mtlo() => ast.AssignLo(ast.GPR_s(RS));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move if Zero/Non zero.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm movz() => ast.If(ast.Binary(ast.GPR_s(RT), "==", 0), ast.Assign(ast.Gpr(RD), ast.GPR_u(RS)));
        public AstNodeStm movn() => ast.If(ast.Binary(ast.GPR_s(RT), "!=", 0), ast.Assign(ast.Gpr(RD), ast.GPR_u(RS)));

        /// <summary>
        /// EXTract/INSert
        /// </summary>
        public AstNodeStm ext() => ast.AssignGpr(RT,
            ast.CallStatic((Func<uint, int, int, uint>) CpuEmitterUtils._ext_impl, ast.GPR_u(RS),
                ast.Immediate((int) Instruction.POS), ast.Immediate((int) Instruction.SIZE_E)));

        public AstNodeStm ins() => ast.AssignGpr(RT,
            ast.CallStatic((Func<uint, uint, int, int, uint>) CpuEmitterUtils._ins_impl, ast.GPR_u(RT),
                ast.GPR_u(RS), ast.Immediate((int) Instruction.POS), ast.Immediate((int) Instruction.SIZE_I)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Count Leading Ones/Zeros in word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm clz() => ast.AssignGpr(RD, ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._clz_impl, ast.GPR_u(RS)));
        public AstNodeStm clo() => ast.AssignGpr(RD, ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._clo_impl, ast.GPR_u(RS)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Word Swap Bytes Within Halfwords/Words.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm wsbh() => ast.AssignGpr(RD, ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._wsbh_impl, ast.GPR_u(RT)));
        public AstNodeStm wsbw() => ast.AssignGpr(RD, ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._wsbw_impl, ast.GPR_u(RT)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move Control (From/To) Cop0
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm cfc0() => ast.Unimplemented($"Unimplemented cfc0 : {RT}, {RD}").Statement();

        public AstNodeStm ctc0() => ast.Unimplemented($"Unimplemented ctc0 : {RT}, {RD}").Statement();

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) Cop0
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm mfc0() => ast.AssignGpr(RT, ast.C0R(RD));

        public AstNodeStm mtc0() => ast.AssignC0R(RD, ast.Gpr(RT));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Byte/Half word/Word (Left/Right/Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm lb() => ast.AssignGpr(RT, ast.MemoryGetValue<sbyte>(Memory, Address_RS_IMM()));

        public AstNodeStm lbu() => ast.AssignGpr(RT, ast.MemoryGetValue<byte>(Memory, Address_RS_IMM()));

        public AstNodeStm lh() => ast.AssignGpr(RT, ast.MemoryGetValue<short>(Memory, Address_RS_IMM()));

        public AstNodeStm lhu() => ast.AssignGpr(RT, ast.MemoryGetValue<ushort>(Memory, Address_RS_IMM()));

        public AstNodeStm lw() => ast.AssignGpr(RT, ast.MemoryGetValue<int>(Memory, Address_RS_IMM()));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Store Byte/Half word/Word (Left/Right).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm sb() => ast.MemorySetValue<byte>(Memory, Address_RS_IMM(), ast.GPR_u(RT));

        public AstNodeStm sh() => ast.MemorySetValue<ushort>(Memory, Address_RS_IMM(), ast.GPR_u(RT));

        public AstNodeStm sw() => ast.MemorySetValue<uint>(Memory, Address_RS_IMM(), ast.GPR_u(RT));

        public AstNodeStm lwl() => ast.AssignGpr(RT,
            ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>) CpuEmitterUtils._lwl_exec,
                ast.CpuThreadStateExpr, ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT)));

        public AstNodeStm lwr() => ast.AssignGpr(RT,
            ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>) CpuEmitterUtils._lwr_exec,
                ast.CpuThreadStateExpr, ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT)));

        public AstNodeStm swl() => ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>) CpuEmitterUtils._swl_exec,
            ast.CpuThreadStateExpr, ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT)));

        public AstNodeStm swr() => ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>) CpuEmitterUtils._swr_exec,
            ast.CpuThreadStateExpr, ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Linked word.
        // Store Conditional word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm ll() => lw();

        public AstNodeStm sc() => ast.Statements(sw(), ast.AssignGpr(RT, 1));

        public string SpecialName = "";

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Syscall
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm syscall()
        {
            if (Instruction.CODE == SyscallInfo.NativeCallSyscallCode)
            {
                var delegateId = Memory.Read4(PC + 4);
                var syscallInfoInfo = CpuProcessor.RegisteredNativeSyscallMethods[delegateId];
                SpecialName = syscallInfoInfo.FunctionEntryName;

                var statements = ast.StatementsInline(
                    ast.Assign(ast.Pc(), PC),
                    ast.Comment(syscallInfoInfo.Name),
                    ast.GetTickCall(true)
                );

                if (_DynarecConfig.FunctionCallWithStaticReferences)
                {
                    statements.AddStatement(ast.Statement(ast.CallDelegate(syscallInfoInfo.PoolItem.AstFieldAccess,
                        ast.CpuThreadStateExpr)));
                }
                else
                {
                    statements.AddStatement(ast.Statement(ast.CallInstance(ast.CpuThreadStateExpr,
                        (Action<uint>) CpuThreadState.Methods.SyscallNative, delegateId)));
                }

                statements.AddStatement(ast.Return());

                return statements;
            }
            else
            {
                return ast.StatementsInline(
                    ast.AssignPc(PC),
                    ast.GetTickCall(true),
                    ast.Statement(ast.CallInstance(ast.CpuThreadStateExpr, (Action<int>) CpuThreadState.Methods.Syscall,
                        (int) Instruction.CODE))
                );
            }
        }

        public AstNodeStm cache() => ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>) CpuEmitterUtils._cache_impl,
            ast.CpuThreadStateExpr, PC, Instruction.Value));

        public AstNodeStm sync() => ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>) CpuEmitterUtils._sync_impl,
            ast.CpuThreadStateExpr, PC, Instruction.Value));

        public AstNodeStm _break() => ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>) CpuEmitterUtils._break_impl,
            ast.CpuThreadStateExpr, PC, Instruction.Value));

        public AstNodeStm dbreak() => throw new NotImplementedException("dbreak");

        public AstNodeStm halt() => throw new NotImplementedException("halt");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // (D?/Exception) RETurn
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm dret() => throw new NotImplementedException("dret");

        public AstNodeStm eret() => throw new NotImplementedException("eret");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) IC
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm mfic() => ast.AssignGpr(RT, ast.Ic());

        public AstNodeStm mtic() => ast.AssignIc(ast.GPR_u(RT));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) DR
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm mfdr() => throw new NotImplementedException("mfdr");

        public AstNodeStm mtdr() => throw new NotImplementedException("mtdr");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Unknown instruction
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm unknown() => ast.Unimplemented($"UNKNOWN INSTRUCTION: 0x{Instruction.Value:X8} : 0x{Instruction.Value:X8} at 0x{PC:X8}").Statement();
    }
}
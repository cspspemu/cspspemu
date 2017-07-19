using System;
using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic operations.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("add")]
        public AstNodeStm Add() => _ast.AssignGpr(Rd, _ast.GPR_s(Rs) + _ast.GPR_s(Rt));

        [InstructionName("addu")]
        public AstNodeStm Addu() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) + _ast.GPR_u(Rt));
        
        [InstructionName("sub")]
        public AstNodeStm Sub() => _ast.AssignGpr(Rd, _ast.GPR_s(Rs) - _ast.GPR_s(Rt));

        [InstructionName("subu")]
        public AstNodeStm Subu() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) - _ast.GPR_u(Rt));

        [InstructionName("addi")]
        public AstNodeStm Addi() => _ast.AssignGpr(Rt, _ast.GPR_s(Rs) + IMM_s());

        [InstructionName("addiu")]
        public AstNodeStm Addiu() => _ast.AssignGpr(Rt, _ast.GPR_s(Rs) + IMM_s());

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Logical Operations.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("and")]
        public AstNodeStm And() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) & _ast.GPR_u(Rt));
        
        [InstructionName("or")]
        public AstNodeStm Or() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) | _ast.GPR_u(Rt));

        [InstructionName("xor")]
        public AstNodeStm Xor() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) ^ _ast.GPR_u(Rt));

        [InstructionName("nor")]
        public AstNodeStm Nor() => _ast.AssignGpr(Rd, ~(_ast.GPR_u(Rs) | _ast.GPR_u(Rt)));

        [InstructionName("andi")]
        public AstNodeStm Andi() => _ast.AssignGpr(Rt, _ast.GPR_u(Rs) & IMM_u());

        [InstructionName("ori")]
        public AstNodeStm Ori() => _ast.AssignGpr(Rt, _ast.GPR_u(Rs) | IMM_u());

        [InstructionName("xori")]
        public AstNodeStm Xori() => _ast.AssignGpr(Rt, _ast.GPR_u(Rs) ^ IMM_u());

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Shift Left/Right Logical/Arithmethic (Variable).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("sll")]
        public AstNodeStm Sll() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rt), "<<", _ast.Immediate((uint) _instruction.Pos)));

        [InstructionName("sra")]
        public AstNodeStm Sra() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_s(Rt), ">>", _ast.Immediate((int) _instruction.Pos)));

        [InstructionName("srl")]
        public AstNodeStm Srl() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rt), ">>", _ast.Immediate((uint) _instruction.Pos)));

        [InstructionName("rotr")]
        public AstNodeStm Rotr() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<uint, int, uint>) CpuEmitterUtils._rotr_impl, _ast.GPR_u(Rt),
                _ast.Immediate((int) _instruction.Pos)));

        [InstructionName("sllv")]
        public AstNodeStm Sllv() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rt), "<<", _ast.GPR_u(Rs) & 31));
        
        [InstructionName("srav")]
        public AstNodeStm Srav() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_s(Rt), ">>", _ast.GPR_s(Rs) & 31));

        [InstructionName("srlv")]
        public AstNodeStm Srlv() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rt), ">>", _ast.GPR_u(Rs) & 31));

        [InstructionName("rotrv")]
        public AstNodeStm Rotrv() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<uint, int, uint>) CpuEmitterUtils._rotr_impl, _ast.GPR_u(Rt), _ast.GPR_s(Rs)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Set Less Than (Immediate) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("slt")]
        public AstNodeStm Slt() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_s(Rs), "<", _ast.GPR_s(Rt)));

        [InstructionName("sltu")]
        public AstNodeStm Sltu() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rs), "<", _ast.GPR_u(Rt)));

        [InstructionName("slti")]
        public AstNodeStm Slti() => _ast.AssignGpr(Rt, _ast.Binary(_ast.GPR_s(Rs), "<", IMM_s()));

        [InstructionName("sltiu")]
        public AstNodeStm Sltiu() => _ast.AssignGpr(Rt, _ast.Binary(_ast.GPR_u(Rs), "<", IMM_uex()));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Upper Immediate.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("lui")]
        public AstNodeStm Lui() => _ast.AssignGpr(Rt, _ast.Binary(IMM_u(), "<<", _ast.Immediate((uint) 16)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Sign Extend Byte/Half word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("seb")]
        public AstNodeStm Seb() => _ast.AssignGpr(Rd, _ast.Cast<int>(_ast.Cast<sbyte>(_ast.GPR_u(Rt))));

        [InstructionName("seh")]
        public AstNodeStm Seh() => _ast.AssignGpr(Rd, _ast.Cast<int>(_ast.Cast<short>(_ast.GPR_u(Rt))));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // BIT REVerse.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("bitrev")]
        public AstNodeStm Bitrev() => _ast.AssignGpr(Rd, _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._bitrev_impl, _ast.GPR_u(Rt)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MAXimum/MINimum.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("max")]
        public AstNodeStm Max() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<int, int, int>) CpuEmitterUtils._max_impl, _ast.GPR_s(Rs), _ast.GPR_s(Rt)));

        [InstructionName("min")]
        public AstNodeStm Min() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<int, int, int>) CpuEmitterUtils._min_impl, _ast.GPR_s(Rs), _ast.GPR_s(Rt)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // DIVide (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("div")]
        public AstNodeStm Div() => (_ast.Statement(_ast.CallStatic((Action<CpuThreadState, int, int>) CpuEmitterUtils._div_impl,
            _ast.CpuThreadStateExpr, _ast.GPR_s(Rs), _ast.GPR_s(Rt))));

        [InstructionName("divu")]
        public AstNodeStm Divu() => (_ast.Statement(_ast.CallStatic((Action<CpuThreadState, uint, uint>) CpuEmitterUtils._divu_impl,
            _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), _ast.GPR_u(Rt))));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MULTiply (ADD/SUBstract) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("mult")]
        public AstNodeStm Mult() => _ast.AssignHilo(_ast.GPR_sl(Rs) * _ast.GPR_sl(Rt));

        [InstructionName("multu")]
        public AstNodeStm Multu() => _ast.AssignHilo(_ast.GPR_ul(Rs) * _ast.GPR_ul(Rt));

        [InstructionName("madd")]
        public AstNodeStm Madd() => _ast.AssignHilo(_ast.HILO_sl() + _ast.GPR_sl(Rs) * _ast.GPR_sl(Rt));

        [InstructionName("maddu")]
        public AstNodeStm Maddu() => _ast.AssignHilo(_ast.HILO_ul() + _ast.GPR_ul(Rs) * _ast.GPR_ul(Rt));

        [InstructionName("msub")]
        public AstNodeStm Msub() => _ast.AssignHilo(_ast.HILO_sl() - _ast.GPR_sl(Rs) * _ast.GPR_sl(Rt));

        [InstructionName("msubu")]
        public AstNodeStm Msubu() => _ast.AssignHilo(_ast.HILO_ul() - _ast.GPR_ul(Rs) * _ast.GPR_ul(Rt));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move To/From HI/LO.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("mfhi")]
        public AstNodeStm Mfhi() => _ast.AssignGpr(Rd, _ast.Cast<uint>(_ast.Hi()));

        [InstructionName("mflo")]
        public AstNodeStm Mflo() => _ast.AssignGpr(Rd, _ast.Cast<uint>(_ast.Lo()));

        [InstructionName("mthi")]
        public AstNodeStm Mthi() => _ast.AssignHi(_ast.GPR_s(Rs));

        [InstructionName("mtlo")]
        public AstNodeStm Mtlo() => _ast.AssignLo(_ast.GPR_s(Rs));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move if Zero/Non zero.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("movz")]
        public AstNodeStm Movz() => _ast.If(_ast.Binary(_ast.GPR_s(Rt), "==", 0), _ast.Assign(_ast.Gpr(Rd), _ast.GPR_u(Rs)));

        [InstructionName("movn")]
        public AstNodeStm Movn() => _ast.If(_ast.Binary(_ast.GPR_s(Rt), "!=", 0), _ast.Assign(_ast.Gpr(Rd), _ast.GPR_u(Rs)));

        /// <summary>
        /// EXTract/INSert
        /// </summary>
        [InstructionName("ext")]
        public AstNodeStm Ext() => _ast.AssignGpr(Rt,
            _ast.CallStatic((Func<uint, int, int, uint>) CpuEmitterUtils._ext_impl, _ast.GPR_u(Rs),
                _ast.Immediate((int) _instruction.Pos), _ast.Immediate((int) _instruction.SizeE)));

        [InstructionName("ins")]
        public AstNodeStm Ins() => _ast.AssignGpr(Rt,
            _ast.CallStatic((Func<uint, uint, int, int, uint>) CpuEmitterUtils._ins_impl, _ast.GPR_u(Rt),
                _ast.GPR_u(Rs), _ast.Immediate((int) _instruction.Pos), _ast.Immediate((int) _instruction.SizeI)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Count Leading Ones/Zeros in word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("clz")]
        public AstNodeStm Clz() => _ast.AssignGpr(Rd, _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._clz_impl, _ast.GPR_u(Rs)));

        [InstructionName("clo")]
        public AstNodeStm Clo() => _ast.AssignGpr(Rd, _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._clo_impl, _ast.GPR_u(Rs)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Word Swap Bytes Within Halfwords/Words.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("wsbh")]
        public AstNodeStm Wsbh() => _ast.AssignGpr(Rd, _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._wsbh_impl, _ast.GPR_u(Rt)));

        [InstructionName("wsbw")]
        public AstNodeStm Wsbw() => _ast.AssignGpr(Rd, _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._wsbw_impl, _ast.GPR_u(Rt)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move Control (From/To) Cop0
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("cfc0")]
        public AstNodeStm Cfc0() => _ast.Unimplemented($"Unimplemented cfc0 : {Rt}, {Rd}").Statement();

        [InstructionName("ctc0")]
        public AstNodeStm Ctc0() => _ast.Unimplemented($"Unimplemented ctc0 : {Rt}, {Rd}").Statement();

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) Cop0
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("mfc0")]
        public AstNodeStm Mfc0() => _ast.AssignGpr(Rt, _ast.C0R(Rd));

        [InstructionName("mtc0")]
        public AstNodeStm Mtc0() => _ast.AssignC0R(Rd, _ast.Gpr(Rt));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Byte/Half word/Word (Left/Right/Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("lb")]
        public AstNodeStm Lb() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<sbyte>(_memory, Address_RS_IMM()));

        [InstructionName("lbu")]
        public AstNodeStm Lbu() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<byte>(_memory, Address_RS_IMM()));

        [InstructionName("lh")]
        public AstNodeStm Lh() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<short>(_memory, Address_RS_IMM()));

        [InstructionName("lhu")]
        public AstNodeStm Lhu() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<ushort>(_memory, Address_RS_IMM()));

        [InstructionName("lw")]
        public AstNodeStm Lw() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<int>(_memory, Address_RS_IMM()));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Store Byte/Half word/Word (Left/Right).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("sb")]
        public AstNodeStm Sb() => _ast.MemorySetValue<byte>(_memory, Address_RS_IMM(), _ast.GPR_u(Rt));

        [InstructionName("sh")]
        public AstNodeStm Sh() => _ast.MemorySetValue<ushort>(_memory, Address_RS_IMM(), _ast.GPR_u(Rt));

        [InstructionName("sw")]
        public AstNodeStm Sw() => _ast.MemorySetValue<uint>(_memory, Address_RS_IMM(), _ast.GPR_u(Rt));

        [InstructionName("lwl")]
        public AstNodeStm Lwl() => _ast.AssignGpr(Rt,
            _ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>) CpuEmitterUtils._lwl_exec,
                _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), IMM_s(), _ast.GPR_u(Rt)));

        [InstructionName("lwr")]
        public AstNodeStm Lwr() => _ast.AssignGpr(Rt,
            _ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>) CpuEmitterUtils._lwr_exec,
                _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), IMM_s(), _ast.GPR_u(Rt)));

        [InstructionName("swl")]
        public AstNodeStm Swl() => _ast.Statement(_ast.CallStatic((Action<CpuThreadState, uint, int, uint>) CpuEmitterUtils._swl_exec,
            _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), IMM_s(), _ast.GPR_u(Rt)));

        [InstructionName("swr")]
        public AstNodeStm Swr() => _ast.Statement(_ast.CallStatic((Action<CpuThreadState, uint, int, uint>) CpuEmitterUtils._swr_exec,
            _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), IMM_s(), _ast.GPR_u(Rt)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Linked word.
        // Store Conditional word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("ll")]
        public AstNodeStm Ll() => Lw();

        [InstructionName("sc")]
        public AstNodeStm Sc() => _ast.Statements(Sw(), _ast.AssignGpr(Rt, 1));

        public string SpecialName = "";

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Syscall
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("syscall")]
        public AstNodeStm Syscall()
        {
            if (_instruction.Code == SyscallInfo.NativeCallSyscallCode)
            {
                var delegateId = _memory.Read4(_pc + 4);
                var syscallInfoInfo = _cpuProcessor.RegisteredNativeSyscallMethods[delegateId];
                SpecialName = syscallInfoInfo.FunctionEntryName;

                var statements = _ast.StatementsInline(
                    _ast.Assign(_ast.Pc(), _pc),
                    _ast.Comment(syscallInfoInfo.Name),
                    _ast.GetTickCall(true)
                );

                if (DynarecConfig.FunctionCallWithStaticReferences)
                {
                    statements.AddStatement(_ast.Statement(_ast.CallDelegate(syscallInfoInfo.PoolItem.AstFieldAccess,
                        _ast.CpuThreadStateExpr)));
                }
                else
                {
                    statements.AddStatement(_ast.Statement(_ast.CallInstance(_ast.CpuThreadStateExpr,
                        (Action<uint>) CpuThreadState.Methods.SyscallNative, delegateId)));
                }

                statements.AddStatement(_ast.Return());

                return statements;
            }
            else
            {
                return _ast.StatementsInline(
                    _ast.AssignPc(_pc),
                    _ast.GetTickCall(true),
                    _ast.Statement(_ast.CallInstance(_ast.CpuThreadStateExpr, (Action<int>) CpuThreadState.Methods.Syscall,
                        (int) _instruction.Code))
                );
            }
        }

        [InstructionName("cache")]
        public AstNodeStm Cache() => _ast.Statement(_ast.CallStatic((Action<CpuThreadState, uint, uint>) CpuEmitterUtils._cache_impl,
            _ast.CpuThreadStateExpr, _pc, _instruction.Value));

        [InstructionName("sync")]
        public AstNodeStm Sync() => _ast.Statement(_ast.CallStatic((Action<CpuThreadState, uint, uint>) CpuEmitterUtils._sync_impl,
            _ast.CpuThreadStateExpr, _pc, _instruction.Value));

        [InstructionName("break")]
        public AstNodeStm _break() => _ast.Statement(_ast.CallStatic((Action<CpuThreadState, uint, uint>) CpuEmitterUtils._break_impl,
            _ast.CpuThreadStateExpr, _pc, _instruction.Value));

        [InstructionName("dbreak")]
        public AstNodeStm Dbreak() => throw new NotImplementedException("dbreak");

        [InstructionName("halt")]
        public AstNodeStm Halt() => throw new NotImplementedException("halt");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // (D?/Exception) RETurn
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("dret")]
        public AstNodeStm Dret() => throw new NotImplementedException("dret");

        [InstructionName("eret")]
        public AstNodeStm Eret() => throw new NotImplementedException("eret");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) IC
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("mfic")]
        public AstNodeStm Mfic() => _ast.AssignGpr(Rt, _ast.Ic());

        [InstructionName("mtic")]
        public AstNodeStm Mtic() => _ast.AssignIc(_ast.GPR_u(Rt));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) DR
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("mfdr")]
        public AstNodeStm Mfdr() => throw new NotImplementedException("mfdr");

        [InstructionName("mtdr")]
        public AstNodeStm Mtdr() => throw new NotImplementedException("mtdr");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Unknown instruction
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName("unknown")]
        public AstNodeStm Unknown() => _ast.Unimplemented($"UNKNOWN INSTRUCTION: 0x{_instruction.Value:X8} : 0x{_instruction.Value:X8} at 0x{_pc:X8}").Statement();
    }
}
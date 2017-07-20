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
        [InstructionName(InstructionNames.Add)]
        public AstNodeStm Add() => _ast.AssignGpr(Rd, _ast.GPR_s(Rs) + _ast.GPR_s(Rt));

        [InstructionName(InstructionNames.Addu)]
        public AstNodeStm Addu() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) + _ast.GPR_u(Rt));

        [InstructionName(InstructionNames.Sub)]
        public AstNodeStm Sub() => _ast.AssignGpr(Rd, _ast.GPR_s(Rs) - _ast.GPR_s(Rt));

        [InstructionName(InstructionNames.Subu)]
        public AstNodeStm Subu() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) - _ast.GPR_u(Rt));

        [InstructionName(InstructionNames.Addi)]
        public AstNodeStm Addi() => _ast.AssignGpr(Rt, _ast.GPR_s(Rs) + IMM_s());

        [InstructionName(InstructionNames.Addiu)]
        public AstNodeStm Addiu() => _ast.AssignGpr(Rt, _ast.GPR_s(Rs) + IMM_s());

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Logical Operations.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.And)]
        public AstNodeStm And() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) & _ast.GPR_u(Rt));

        [InstructionName(InstructionNames.Or)]
        public AstNodeStm Or() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) | _ast.GPR_u(Rt));

        [InstructionName(InstructionNames.Xor)]
        public AstNodeStm Xor() => _ast.AssignGpr(Rd, _ast.GPR_u(Rs) ^ _ast.GPR_u(Rt));

        [InstructionName(InstructionNames.Nor)]
        public AstNodeStm Nor() => _ast.AssignGpr(Rd, ~(_ast.GPR_u(Rs) | _ast.GPR_u(Rt)));

        [InstructionName(InstructionNames.Andi)]
        public AstNodeStm Andi() => _ast.AssignGpr(Rt, _ast.GPR_u(Rs) & IMM_u());

        [InstructionName(InstructionNames.Ori)]
        public AstNodeStm Ori() => _ast.AssignGpr(Rt, _ast.GPR_u(Rs) | IMM_u());

        [InstructionName(InstructionNames.Xori)]
        public AstNodeStm Xori() => _ast.AssignGpr(Rt, _ast.GPR_u(Rs) ^ IMM_u());

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Shift Left/Right Logical/Arithmethic (Variable).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Sll)]
        public AstNodeStm Sll() =>
            _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rt), "<<", _ast.Immediate((uint) _instruction.Pos)));

        [InstructionName(InstructionNames.Sra)]
        public AstNodeStm Sra() =>
            _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_s(Rt), ">>", _ast.Immediate((int) _instruction.Pos)));

        [InstructionName(InstructionNames.Srl)]
        public AstNodeStm Srl() =>
            _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rt), ">>", _ast.Immediate((uint) _instruction.Pos)));

        [InstructionName(InstructionNames.Rotr)]
        public AstNodeStm Rotr() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<uint, int, uint>) CpuEmitterUtils._rotr_impl, _ast.GPR_u(Rt),
                _ast.Immediate((int) _instruction.Pos)));

        [InstructionName(InstructionNames.Sllv)]
        public AstNodeStm Sllv() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rt), "<<", _ast.GPR_u(Rs) & 31));

        [InstructionName(InstructionNames.Srav)]
        public AstNodeStm Srav() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_s(Rt), ">>", _ast.GPR_s(Rs) & 31));

        [InstructionName(InstructionNames.Srlv)]
        public AstNodeStm Srlv() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rt), ">>", _ast.GPR_u(Rs) & 31));

        [InstructionName(InstructionNames.Rotrv)]
        public AstNodeStm Rotrv() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<uint, int, uint>) CpuEmitterUtils._rotr_impl, _ast.GPR_u(Rt), _ast.GPR_s(Rs)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Set Less Than (Immediate) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Slt)]
        public AstNodeStm Slt() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_s(Rs), "<", _ast.GPR_s(Rt)));

        [InstructionName(InstructionNames.Sltu)]
        public AstNodeStm Sltu() => _ast.AssignGpr(Rd, _ast.Binary(_ast.GPR_u(Rs), "<", _ast.GPR_u(Rt)));

        [InstructionName(InstructionNames.Slti)]
        public AstNodeStm Slti() => _ast.AssignGpr(Rt, _ast.Binary(_ast.GPR_s(Rs), "<", IMM_s()));

        [InstructionName(InstructionNames.Sltiu)]
        public AstNodeStm Sltiu() => _ast.AssignGpr(Rt, _ast.Binary(_ast.GPR_u(Rs), "<", IMM_uex()));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Upper Immediate.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Lui)]
        public AstNodeStm Lui() => _ast.AssignGpr(Rt, _ast.Binary(IMM_u(), "<<", _ast.Immediate((uint) 16)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Sign Extend Byte/Half word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Seb)]
        public AstNodeStm Seb() => _ast.AssignGpr(Rd, _ast.Cast<int>(_ast.Cast<sbyte>(_ast.GPR_u(Rt))));

        [InstructionName(InstructionNames.Seh)]
        public AstNodeStm Seh() => _ast.AssignGpr(Rd, _ast.Cast<int>(_ast.Cast<short>(_ast.GPR_u(Rt))));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // BIT REVerse.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Bitrev)]
        public AstNodeStm Bitrev() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._bitrev_impl, _ast.GPR_u(Rt)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MAXimum/MINimum.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Max)]
        public AstNodeStm Max() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<int, int, int>) CpuEmitterUtils._max_impl, _ast.GPR_s(Rs), _ast.GPR_s(Rt)));

        [InstructionName(InstructionNames.Min)]
        public AstNodeStm Min() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<int, int, int>) CpuEmitterUtils._min_impl, _ast.GPR_s(Rs), _ast.GPR_s(Rt)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // DIVide (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Div)]
        public AstNodeStm Div() => (_ast.Statement(_ast.CallStatic(
            (Action<CpuThreadState, int, int>) CpuEmitterUtils._div_impl,
            _ast.CpuThreadStateExpr, _ast.GPR_s(Rs), _ast.GPR_s(Rt))));

        [InstructionName(InstructionNames.Divu)]
        public AstNodeStm Divu() => (_ast.Statement(_ast.CallStatic(
            (Action<CpuThreadState, uint, uint>) CpuEmitterUtils._divu_impl,
            _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), _ast.GPR_u(Rt))));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // MULTiply (ADD/SUBstract) (Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mult)]
        public AstNodeStm Mult() => _ast.AssignHilo(_ast.GPR_sl(Rs) * _ast.GPR_sl(Rt));

        [InstructionName(InstructionNames.Multu)]
        public AstNodeStm Multu() => _ast.AssignHilo(_ast.GPR_ul(Rs) * _ast.GPR_ul(Rt));

        [InstructionName(InstructionNames.Madd)]
        public AstNodeStm Madd() => _ast.AssignHilo(_ast.HILO_sl() + _ast.GPR_sl(Rs) * _ast.GPR_sl(Rt));

        [InstructionName(InstructionNames.Maddu)]
        public AstNodeStm Maddu() => _ast.AssignHilo(_ast.HILO_ul() + _ast.GPR_ul(Rs) * _ast.GPR_ul(Rt));

        [InstructionName(InstructionNames.Msub)]
        public AstNodeStm Msub() => _ast.AssignHilo(_ast.HILO_sl() - _ast.GPR_sl(Rs) * _ast.GPR_sl(Rt));

        [InstructionName(InstructionNames.Msubu)]
        public AstNodeStm Msubu() => _ast.AssignHilo(_ast.HILO_ul() - _ast.GPR_ul(Rs) * _ast.GPR_ul(Rt));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move To/From HI/LO.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfhi)]
        public AstNodeStm Mfhi() => _ast.AssignGpr(Rd, _ast.Cast<uint>(_ast.Hi()));

        [InstructionName(InstructionNames.Mflo)]
        public AstNodeStm Mflo() => _ast.AssignGpr(Rd, _ast.Cast<uint>(_ast.Lo()));

        [InstructionName(InstructionNames.Mthi)]
        public AstNodeStm Mthi() => _ast.AssignHi(_ast.GPR_s(Rs));

        [InstructionName(InstructionNames.Mtlo)]
        public AstNodeStm Mtlo() => _ast.AssignLo(_ast.GPR_s(Rs));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move if Zero/Non zero.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Movz)]
        public AstNodeStm Movz() =>
            _ast.If(_ast.Binary(_ast.GPR_s(Rt), "==", 0), _ast.Assign(_ast.Gpr(Rd), _ast.GPR_u(Rs)));

        [InstructionName(InstructionNames.Movn)]
        public AstNodeStm Movn() =>
            _ast.If(_ast.Binary(_ast.GPR_s(Rt), "!=", 0), _ast.Assign(_ast.Gpr(Rd), _ast.GPR_u(Rs)));

        /// <summary>
        /// EXTract/INSert
        /// </summary>
        [InstructionName(InstructionNames.Ext)]
        public AstNodeStm Ext() => _ast.AssignGpr(Rt,
            _ast.CallStatic((Func<uint, int, int, uint>) CpuEmitterUtils._ext_impl, _ast.GPR_u(Rs),
                _ast.Immediate((int) _instruction.Pos), _ast.Immediate((int) _instruction.SizeE)));

        [InstructionName(InstructionNames.Ins)]
        public AstNodeStm Ins() => _ast.AssignGpr(Rt,
            _ast.CallStatic((Func<uint, uint, int, int, uint>) CpuEmitterUtils._ins_impl, _ast.GPR_u(Rt),
                _ast.GPR_u(Rs), _ast.Immediate((int) _instruction.Pos), _ast.Immediate((int) _instruction.SizeI)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Count Leading Ones/Zeros in word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Clz)]
        public AstNodeStm Clz() =>
            _ast.AssignGpr(Rd, _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._clz_impl, _ast.GPR_u(Rs)));

        [InstructionName(InstructionNames.Clo)]
        public AstNodeStm Clo() =>
            _ast.AssignGpr(Rd, _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._clo_impl, _ast.GPR_u(Rs)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Word Swap Bytes Within Halfwords/Words.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Wsbh)]
        public AstNodeStm Wsbh() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._wsbh_impl, _ast.GPR_u(Rt)));

        [InstructionName(InstructionNames.Wsbw)]
        public AstNodeStm Wsbw() => _ast.AssignGpr(Rd,
            _ast.CallStatic((Func<uint, uint>) CpuEmitterUtils._wsbw_impl, _ast.GPR_u(Rt)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move Control (From/To) Cop0
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Cfc0)]
        public AstNodeStm Cfc0() => _ast.Unimplemented($"Unimplemented cfc0 : {Rt}, {Rd}").Statement();

        [InstructionName(InstructionNames.Ctc0)]
        public AstNodeStm Ctc0() => _ast.Unimplemented($"Unimplemented ctc0 : {Rt}, {Rd}").Statement();

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) Cop0
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfc0)]
        public AstNodeStm Mfc0() => _ast.AssignGpr(Rt, _ast.C0R(Rd));

        [InstructionName(InstructionNames.Mtc0)]
        public AstNodeStm Mtc0() => _ast.AssignC0R(Rd, _ast.Gpr(Rt));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Byte/Half word/Word (Left/Right/Unsigned).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Lb)]
        public AstNodeStm Lb() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<sbyte>(_memory, Address_RS_IMM()));

        [InstructionName(InstructionNames.Lbu)]
        public AstNodeStm Lbu() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<byte>(_memory, Address_RS_IMM()));

        [InstructionName(InstructionNames.Lh)]
        public AstNodeStm Lh() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<short>(_memory, Address_RS_IMM()));

        [InstructionName(InstructionNames.Lhu)]
        public AstNodeStm Lhu() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<ushort>(_memory, Address_RS_IMM()));

        [InstructionName(InstructionNames.Lw)]
        public AstNodeStm Lw() => _ast.AssignGpr(Rt, _ast.MemoryGetValue<int>(_memory, Address_RS_IMM()));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Store Byte/Half word/Word (Left/Right).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Sb)]
        public AstNodeStm Sb() => _ast.MemorySetValue<byte>(_memory, Address_RS_IMM(), _ast.GPR_u(Rt));

        [InstructionName(InstructionNames.Sh)]
        public AstNodeStm Sh() => _ast.MemorySetValue<ushort>(_memory, Address_RS_IMM(), _ast.GPR_u(Rt));

        [InstructionName(InstructionNames.Sw)]
        public AstNodeStm Sw() => _ast.MemorySetValue<uint>(_memory, Address_RS_IMM(), _ast.GPR_u(Rt));

        [InstructionName(InstructionNames.Lwl)]
        public AstNodeStm Lwl() => _ast.AssignGpr(Rt,
            _ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>) CpuEmitterUtils._lwl_exec,
                _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), IMM_s(), _ast.GPR_u(Rt)));

        [InstructionName(InstructionNames.Lwr)]
        public AstNodeStm Lwr() => _ast.AssignGpr(Rt,
            _ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>) CpuEmitterUtils._lwr_exec,
                _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), IMM_s(), _ast.GPR_u(Rt)));

        [InstructionName(InstructionNames.Swl)]
        public AstNodeStm Swl() => _ast.Statement(_ast.CallStatic(
            (Action<CpuThreadState, uint, int, uint>) CpuEmitterUtils._swl_exec,
            _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), IMM_s(), _ast.GPR_u(Rt)));

        [InstructionName(InstructionNames.Swr)]
        public AstNodeStm Swr() => _ast.Statement(_ast.CallStatic(
            (Action<CpuThreadState, uint, int, uint>) CpuEmitterUtils._swr_exec,
            _ast.CpuThreadStateExpr, _ast.GPR_u(Rs), IMM_s(), _ast.GPR_u(Rt)));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Load Linked word.
        // Store Conditional word.
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Ll)]
        public AstNodeStm Ll() => Lw();

        [InstructionName(InstructionNames.Sc)]
        public AstNodeStm Sc() => _ast.Statements(Sw(), _ast.AssignGpr(Rt, 1));

        public string SpecialName = "";

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Syscall
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Syscall)]
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
                    _ast.Statement(_ast.CallInstance(_ast.CpuThreadStateExpr,
                        (Action<int>) CpuThreadState.Methods.Syscall,
                        (int) _instruction.Code))
                );
            }
        }

        [InstructionName(InstructionNames.Cache)]
        public AstNodeStm Cache() => _ast.Statement(_ast.CallStatic(
            (Action<CpuThreadState, uint, uint>) CpuEmitterUtils._cache_impl,
            _ast.CpuThreadStateExpr, _pc, _instruction.Value));

        [InstructionName(InstructionNames.Sync)]
        public AstNodeStm Sync() => _ast.Statement(_ast.CallStatic(
            (Action<CpuThreadState, uint, uint>) CpuEmitterUtils._sync_impl,
            _ast.CpuThreadStateExpr, _pc, _instruction.Value));

        [InstructionName(InstructionNames.Break)]
        public AstNodeStm Break() => _ast.Statement(_ast.CallStatic(
            (Action<CpuThreadState, uint, uint>) CpuEmitterUtils._break_impl,
            _ast.CpuThreadStateExpr, _pc, _instruction.Value));

        [InstructionName(InstructionNames.Dbreak)]
        public AstNodeStm Dbreak() => throw new NotImplementedException("dbreak");

        [InstructionName(InstructionNames.Halt)]
        public AstNodeStm Halt() => throw new NotImplementedException("halt");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // (D?/Exception) RETurn
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Dret)]
        public AstNodeStm Dret() => throw new NotImplementedException("dret");

        [InstructionName(InstructionNames.Eret)]
        public AstNodeStm Eret() => throw new NotImplementedException("eret");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) IC
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfic)]
        public AstNodeStm Mfic() => _ast.AssignGpr(Rt, _ast.Ic());

        [InstructionName(InstructionNames.Mtic)]
        public AstNodeStm Mtic() => _ast.AssignIc(_ast.GPR_u(Rt));

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Move (From/To) DR
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Mfdr)]
        public AstNodeStm Mfdr() => throw new NotImplementedException("mfdr");

        [InstructionName(InstructionNames.Mtdr)]
        public AstNodeStm Mtdr() => throw new NotImplementedException("mtdr");

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // Unknown instruction
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Unknown)]
        public AstNodeStm Unknown() =>
            _ast.Unimplemented(
                    $"UNKNOWN INSTRUCTION: 0x{_instruction.Value:X8} : 0x{_instruction.Value:X8} at 0x{_pc:X8}")
                .Statement();
    }
}
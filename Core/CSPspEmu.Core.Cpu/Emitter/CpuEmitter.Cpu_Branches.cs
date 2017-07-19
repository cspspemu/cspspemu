using CSPspEmu.Core.Cpu.Table;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        public AstNodeStm _branch_likely(AstNodeStm code) => _ast.If(BranchFlag(), code);

        // Code executed after the delayed slot.
        public AstNodeStm _branch_post(AstLabel branchLabel, uint branchPc)
        {
            if (_andLink)
            {
                return _ast.If(
                    BranchFlag(),
                    _ast.StatementsInline(
                        _ast.AssignGpr(31, branchPc + 8),
                        CallFixedAddress(branchPc)
                    )
                );
            }
            else
            {
                return _ast.Statements(
                    //ast.AssignPC(PC),
                    //ast.GetTickCall(),
                    _ast.GotoIfTrue(branchLabel, BranchFlag())
                );
            }
        }

        bool _andLink = false;
        uint _branchPc = 0;

        private AstLocal _branchFlagLocal = null;

        private AstNodeExprLValue BranchFlag()
        {
            if (DynarecConfig.BranchFlagAsLocal)
            {
                if (_branchFlagLocal == null)
                {
                    _branchFlagLocal = AstLocal.Create<bool>("BranchFlag");
                }
                return _ast.Local(_branchFlagLocal);
            }
            else
            {
                return _ast.BranchFlag();
            }
        }

        private AstNodeStm AssignBranchFlag(AstNodeExpr expr, bool andLink = false)
        {
            _andLink = andLink;
            _branchPc = _pc;
            return _ast.Assign(BranchFlag(), _ast.Cast<bool>(expr, Explicit: false));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // beq(l)     : Branch on EQuals (Likely).
        // bne(l)     : Branch on Not Equals (Likely).
        // btz(al)(l) : Branch on Less Than Zero (And Link) (Likely).
        // blez(l)    : Branch on Less Or Equals than Zero (Likely).
        // bgtz(l)    : Branch on Great Than Zero (Likely).
        // bgez(al)(l): Branch on Greater Equal Zero (And Link) (Likely).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.Beq)]
        public AstNodeStm Beq() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "==", _ast.GPR_s(Rt)));

        [InstructionName(InstructionNames.Beql)]
        public AstNodeStm Beql() => Beq();

        [InstructionName(InstructionNames.Bne)]
        public AstNodeStm Bne() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "!=", _ast.GPR_s(Rt)));

        [InstructionName(InstructionNames.Bnel)]
        public AstNodeStm Bnel() => Bne();

        [InstructionName(InstructionNames.Bltz)]
        public AstNodeStm Bltz() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "<", 0));

        [InstructionName(InstructionNames.Bltzl)]
        public AstNodeStm Bltzl() => Bltz();

        [InstructionName(InstructionNames.Bltzal)]
        public AstNodeStm Bltzal() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "<", 0), andLink: true);

        [InstructionName(InstructionNames.Bltzall)]
        public AstNodeStm Bltzall() => Bltzal();

        [InstructionName(InstructionNames.Blez)]
        public AstNodeStm Blez() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "<=", 0));

        [InstructionName(InstructionNames.Blezl)]
        public AstNodeStm Blezl() => Blez();

        [InstructionName(InstructionNames.Bgtz)]
        public AstNodeStm Bgtz() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), ">", 0));

        [InstructionName(InstructionNames.Bgtzl)]
        public AstNodeStm Bgtzl() => Bgtz();

        [InstructionName(InstructionNames.Bgez)]
        public AstNodeStm Bgez() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), ">=", 0));

        [InstructionName(InstructionNames.Bgezl)]
        public AstNodeStm Bgezl() => Bgez();

        [InstructionName(InstructionNames.Bgezal)]
        public AstNodeStm Bgezal() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), ">=", 0), andLink: true);

        [InstructionName(InstructionNames.Bgezall)]
        public AstNodeStm Bgezall() => Bgezal();

        public bool PopulateCallStack =>
            !(_cpuProcessor.Memory.HasFixedGlobalAddress) && _cpuProcessor.CpuConfig.TrackCallStack;

        /*
        private AstNodeStm _popstack()
        {
            //if (PopulateCallStack && (RS == 31)) return ast.Statement(ast.CallInstance(CpuThreadStateArgument(), (Action)CpuThreadState.Methods.CallStackPop));
            return ast.Statement();
        }

        private AstNodeStm _pushstack()
        {
            //if (PopulateCallStack) return ast.Statement(ast.CallInstance(CpuThreadStateArgument(), (Action<uint>)CpuThreadState.Methods.CallStackPush, PC));
            return ast.Statement();
        }
        */

        private AstNodeStm _link() => _ast.AssignGpr(31, _ast.Immediate(_pc + 8));

        private AstNodeStm JumpDynamicToAddress(AstNodeExpr address)
        {
            if (DynarecConfig.EnableTailCalling)
            {
                return _ast.MethodCacheInfoCallDynamicPc(address, tailCall: true);
            }
            else
            {
                return _ast.Statements(
                    _ast.AssignPc(address),
                    _ast.Return()
                );
            }
        }

        private AstNodeStm CallDynamicAddress(AstNodeExpr address)
        {
            return _ast.StatementsInline(
                _link(),
                _ast.MethodCacheInfoCallDynamicPc(address, tailCall: false)
            );
        }

        private AstNodeStm JumpToFixedAddress(uint address)
        {
            if (DynarecConfig.EnableTailCalling)
            {
                return _ast.Statements(
                    _ast.Statement(_ast.TailCall(_ast.MethodCacheInfoCallStaticPc(_cpuProcessor, address))),
                    _ast.Return()
                );
            }
            else
            {
                return _ast.StatementsInline(
                    _ast.AssignPc(address),
                    _ast.Return()
                );
            }
        }

        private AstNodeStm CallFixedAddress(uint address)
        {
            return _ast.StatementsInline(
                _link(),
                _ast.Statement(_ast.MethodCacheInfoCallStaticPc(_cpuProcessor, address))
            );
        }

        //static public void DumpStackTrace()
        //{
        //	Console.WriteLine(Environment.StackTrace);
        //}

        private AstNodeStm ReturnFromFunction(AstNodeExpr astNodeExpr)
        {
            return _ast.StatementsInline(
                _ast.AssignPc(_ast.Gpr(31)),
                _ast.GetTickCall(BranchCount > 0),
                _ast.Return()
            );
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // j(al)(r): Jump (And Link) (Register)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [InstructionName(InstructionNames.J)]
        public AstNodeStm J() => JumpToFixedAddress(_instruction.GetJumpAddress(this._memory, _pc));

        [InstructionName(InstructionNames.Jal)]
        public AstNodeStm Jal() => CallFixedAddress(_instruction.GetJumpAddress(this._memory, _pc));

        [InstructionName(InstructionNames.Jr)]
        public AstNodeStm Jr() => Rs == 31 ? ReturnFromFunction(_ast.GPR_u(Rs)) : JumpDynamicToAddress(_ast.GPR_u(Rs));

        [InstructionName(InstructionNames.Jalr)]
        public AstNodeStm Jalr() => CallDynamicAddress(_ast.GPR_u(Rs));
    }
}
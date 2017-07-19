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
            if (this.AndLink)
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

        bool AndLink = false;
        uint BranchPC = 0;

        private AstLocal BranchFlagLocal = null;

        private AstNodeExprLValue BranchFlag()
        {
            if (DynarecConfig.BranchFlagAsLocal)
            {
                if (BranchFlagLocal == null)
                {
                    BranchFlagLocal = AstLocal.Create<bool>("BranchFlag");
                }
                return _ast.Local(BranchFlagLocal);
            }
            else
            {
                return _ast.BranchFlag();
            }
        }

        private AstNodeStm AssignBranchFlag(AstNodeExpr expr, bool andLink = false)
        {
            AndLink = andLink;
            BranchPC = _pc;
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
        public AstNodeStm beq() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "==", _ast.GPR_s(Rt)));

        public AstNodeStm beql() => beq();
        public AstNodeStm bne() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "!=", _ast.GPR_s(Rt)));
        public AstNodeStm bnel() => bne();
        public AstNodeStm bltz() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "<", 0));
        public AstNodeStm bltzl() => bltz();
        public AstNodeStm bltzal() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "<", 0), andLink: true);
        public AstNodeStm bltzall() => bltzal();
        public AstNodeStm blez() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), "<=", 0));
        public AstNodeStm blezl() => blez();
        public AstNodeStm bgtz() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), ">", 0));
        public AstNodeStm bgtzl() => bgtz();
        public AstNodeStm bgez() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), ">=", 0));
        public AstNodeStm bgezl() => bgez();
        public AstNodeStm bgezal() => AssignBranchFlag(_ast.Binary(_ast.GPR_s(Rs), ">=", 0), andLink: true);
        public AstNodeStm bgezall() => bgezal();

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
        public AstNodeStm j() => JumpToFixedAddress(_instruction.GetJumpAddress(this._memory, _pc));

        public AstNodeStm jal() => CallFixedAddress(_instruction.GetJumpAddress(this._memory, _pc));

        public AstNodeStm jr() => Rs == 31 ? ReturnFromFunction(_ast.GPR_u(Rs)) : JumpDynamicToAddress(_ast.GPR_u(Rs));

        public AstNodeStm jalr() => CallDynamicAddress(_ast.GPR_u(Rs));
    }
}
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        public AstNodeStm _branch_likely(AstNodeStm Code) => ast.If(BranchFlag(), Code);

        // Code executed after the delayed slot.
        public AstNodeStm _branch_post(AstLabel BranchLabel, uint BranchPC)
        {
            if (this.AndLink)
            {
                return ast.If(
                    BranchFlag(),
                    ast.StatementsInline(
                        ast.AssignGpr(31, BranchPC + 8),
                        CallFixedAddress(BranchPC)
                    )
                );
            }
            else
            {
                return ast.Statements(
                    //ast.AssignPC(PC),
                    //ast.GetTickCall(),
                    ast.GotoIfTrue(BranchLabel, BranchFlag())
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
                return ast.Local(BranchFlagLocal);
            }
            else
            {
                return ast.BranchFlag();
            }
        }

        private AstNodeStm AssignBranchFlag(AstNodeExpr expr, bool andLink = false)
        {
            AndLink = andLink;
            BranchPC = PC;
            return ast.Assign(BranchFlag(), ast.Cast<bool>(expr, Explicit: false));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // beq(l)     : Branch on EQuals (Likely).
        // bne(l)     : Branch on Not Equals (Likely).
        // btz(al)(l) : Branch on Less Than Zero (And Link) (Likely).
        // blez(l)    : Branch on Less Or Equals than Zero (Likely).
        // bgtz(l)    : Branch on Great Than Zero (Likely).
        // bgez(al)(l): Branch on Greater Equal Zero (And Link) (Likely).
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm beq() => AssignBranchFlag(ast.Binary(ast.GPR_s(RS), "==", ast.GPR_s(RT)));

        public AstNodeStm beql() => beq();
        public AstNodeStm bne() => AssignBranchFlag(ast.Binary(ast.GPR_s(RS), "!=", ast.GPR_s(RT)));
        public AstNodeStm bnel() => bne();
        public AstNodeStm bltz() => AssignBranchFlag(ast.Binary(ast.GPR_s(RS), "<", 0));
        public AstNodeStm bltzl() => bltz();
        public AstNodeStm bltzal() => AssignBranchFlag(ast.Binary(ast.GPR_s(RS), "<", 0), andLink: true);
        public AstNodeStm bltzall() => bltzal();
        public AstNodeStm blez() => AssignBranchFlag(ast.Binary(ast.GPR_s(RS), "<=", 0));
        public AstNodeStm blezl() => blez();
        public AstNodeStm bgtz() => AssignBranchFlag(ast.Binary(ast.GPR_s(RS), ">", 0));
        public AstNodeStm bgtzl() => bgtz();
        public AstNodeStm bgez() => AssignBranchFlag(ast.Binary(ast.GPR_s(RS), ">=", 0));
        public AstNodeStm bgezl() => bgez();
        public AstNodeStm bgezal() => AssignBranchFlag(ast.Binary(ast.GPR_s(RS), ">=", 0), andLink: true);
        public AstNodeStm bgezall() => bgezal();

        public bool PopulateCallStack =>
            !(CpuProcessor.Memory.HasFixedGlobalAddress) && CpuProcessor.CpuConfig.TrackCallStack;

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

        private AstNodeStm _link() => ast.AssignGpr(31, ast.Immediate(PC + 8));

        private AstNodeStm JumpDynamicToAddress(AstNodeExpr address)
        {
            if (DynarecConfig.EnableTailCalling)
            {
                return ast.MethodCacheInfoCallDynamicPc(address, tailCall: true);
            }
            else
            {
                return ast.Statements(
                    ast.AssignPc(address),
                    ast.Return()
                );
            }
        }

        private AstNodeStm CallDynamicAddress(AstNodeExpr address)
        {
            return ast.StatementsInline(
                _link(),
                ast.MethodCacheInfoCallDynamicPc(address, tailCall: false)
            );
        }

        private AstNodeStm JumpToFixedAddress(uint address)
        {
            if (DynarecConfig.EnableTailCalling)
            {
                return ast.Statements(
                    ast.Statement(ast.TailCall(ast.MethodCacheInfoCallStaticPc(CpuProcessor, address))),
                    ast.Return()
                );
            }
            else
            {
                return ast.StatementsInline(
                    ast.AssignPc(address),
                    ast.Return()
                );
            }
        }

        private AstNodeStm CallFixedAddress(uint address)
        {
            return ast.StatementsInline(
                _link(),
                ast.Statement(ast.MethodCacheInfoCallStaticPc(CpuProcessor, address))
            );
        }

        //static public void DumpStackTrace()
        //{
        //	Console.WriteLine(Environment.StackTrace);
        //}

        private AstNodeStm ReturnFromFunction(AstNodeExpr astNodeExpr)
        {
            return ast.StatementsInline(
                ast.AssignPc(ast.Gpr(31)),
                ast.GetTickCall(BranchCount > 0),
                ast.Return()
            );
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // j(al)(r): Jump (And Link) (Register)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm j() => JumpToFixedAddress(Instruction.GetJumpAddress(this.Memory, PC));

        public AstNodeStm jal() => CallFixedAddress(Instruction.GetJumpAddress(this.Memory, PC));

        public AstNodeStm jr() => RS == 31 ? ReturnFromFunction(ast.GPR_u(RS)) : JumpDynamicToAddress(ast.GPR_u(RS));

        public AstNodeStm jalr() => CallDynamicAddress(ast.GPR_u(RS));
    }
}
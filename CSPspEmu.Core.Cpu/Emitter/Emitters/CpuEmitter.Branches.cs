#define ENABLE_NATIVE_CALLS

using System;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		public AstNodeStm _branch_likely(AstNodeStm Code)
		{
			return ast.IfElse(
				BranchFlag(),
				Code
			);
		}

		// Code executed after the delayed slot.
		public AstNodeStm _branch_post(AstLabel BranchLabel, uint BranchPC)
		{
			if (this.AndLink)
			{
#if ENABLE_NATIVE_CALLS
				return ast.IfElse(
					BranchFlag(),
					ast.Statements(
						AssignGPR(31, BranchPC + 8),
						CallFixedAddress(BranchPC)
					)
				);
#else
				return ast.IfElse(
					BranchFlag(),
					ast.Statements(
						AssignGPR(31, BranchPC + 8),
						ast.GotoAlways(BranchLabel)
					)
				);
#endif
			}
			else
			{
				return ast.GotoIfTrue(BranchLabel, BranchFlag());
			}
		}

		bool AndLink = false;
		uint BranchPC = 0;

		private AstLocal BranchFlagLocal = null;

		private AstNodeExprLValue BranchFlag()
		{
#if true
			if (BranchFlagLocal == null)
			{
				BranchFlagLocal = AstLocal.Create(MipsMethodEmitter.ILGenerator, typeof(bool), "BranchFlag");
			}
			return ast.Local(BranchFlagLocal);
#else
			return REG("BranchFlag");
#endif
		}

		private AstNodeStm AssignBranchFlag(AstNodeExpr Expr, bool AndLink = false)
		{
			this.AndLink = AndLink;
			this.BranchPC = PC;
			return ast.Assign(BranchFlag(), ast.Cast<bool>(Expr, Explicit: false));
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// beq(l)     : Branch on EQuals (Likely).
		// bne(l)     : Branch on Not Equals (Likely).
		// btz(al)(l) : Branch on Less Than Zero (And Link) (Likely).
		// blez(l)    : Branch on Less Or Equals than Zero (Likely).
		// bgtz(l)    : Branch on Great Than Zero (Likely).
		// bgez(al)(l): Branch on Greater Equal Zero (And Link) (Likely).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm beq() { return AssignBranchFlag(ast.Binary(GPR_s(RS), "==", GPR_s(RT))); }
		public AstNodeStm beql() { return beq(); }
		public AstNodeStm bne() { return AssignBranchFlag(ast.Binary(GPR_s(RS), "!=", GPR_s(RT))); }
		public AstNodeStm bnel() { return bne(); }
		public AstNodeStm bltz() { return AssignBranchFlag(ast.Binary(GPR_s(RS), "<", 0)); }
		public AstNodeStm bltzl() { return bltz(); }
		[PspUntested]
		public AstNodeStm bltzal() { return AssignBranchFlag(ast.Binary(GPR_s(RS), "<", 0), AndLink: true); }
		public AstNodeStm bltzall() { return bltzal(); }
		public AstNodeStm blez() { return AssignBranchFlag(ast.Binary(GPR_s(RS), "<=", 0)); }
		public AstNodeStm blezl() { return blez(); }
		public AstNodeStm bgtz() { return AssignBranchFlag(ast.Binary(GPR_s(RS), ">", 0)); }
		public AstNodeStm bgtzl() { return bgtz(); }
		public AstNodeStm bgez() { return AssignBranchFlag(ast.Binary(GPR_s(RS), ">=", 0)); }
		public AstNodeStm bgezl() { return bgez(); }
		[PspUntested]
		public AstNodeStm bgezal() { return AssignBranchFlag(ast.Binary(GPR_s(RS), ">=", 0), AndLink: true); }
		public AstNodeStm bgezall() { return bgezal(); }

		public bool PopulateCallStack { get { return !(CpuProcessor.Memory is FastPspMemory) && CpuProcessor.PspConfig.TrackCallStack; } }

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

		private AstNodeStm _link()
		{
			return AssignGPR(31, ast.Immediate(PC + 8));
		}

		private AstNodeStm JumpToAddress(AstNodeExpr Address)
		{
			return ast.Statement(ast.CallTail(MipsMethodEmitter.MethodCacheInfoCallDynamicPC(Address)));
		}

		private AstNodeStm CallAddress(AstNodeExpr Address)
		{
			return ast.Statements(
				_link(),
#if ENABLE_NATIVE_CALLS
				ast.Statement(MipsMethodEmitter.MethodCacheInfoCallDynamicPC(Address))
#else
				JumpToAddress(Address)
#endif
			);
		}

		private AstNodeStm JumpToFixedAddress(uint Address)
		{
			return ast.Statement(ast.CallTail(MipsMethodEmitter.MethodCacheInfoCallStaticPC(CpuProcessor, Address)));
		}

		private AstNodeStm CallFixedAddress(uint Address)
		{
			return ast.Statements(
				_link(),
#if ENABLE_NATIVE_CALLS
				ast.Statement(MipsMethodEmitter.MethodCacheInfoCallStaticPC(CpuProcessor, Address))
#else
				JumpToFixedAddress(Address)
#endif
			);
		}

		//static public void DumpStackTrace()
		//{
		//	Console.WriteLine(Environment.StackTrace);
		//}

		private AstNodeStm ReturnFromFunction(AstNodeExpr AstNodeExpr)
		{
#if ENABLE_NATIVE_CALLS

			return ast.Statements(
				AssignREG("PC", GPR(31)),
				//ast.Statement(ast.CallStatic((Action)DumpStackTrace)),
				ast.Return()
			);
#else
			return JumpToAddress(AstNodeExpr);
#endif
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// j(al)(r): Jump (And Link) (Register)
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm j() { return this.JumpToFixedAddress(Instruction.GetJumpAddress(PC)); }
		public AstNodeStm jal() { return this.CallFixedAddress(Instruction.GetJumpAddress(PC)); }
		public AstNodeStm jr() {
			if (RS == 31)
			{
				return ReturnFromFunction(GPR_u(RS));
			}
			else
			{
				return JumpToAddress(GPR_u(RS));
			}
		}
		public AstNodeStm jalr() { return this.CallAddress(GPR_u(RS)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// bc1(f/t)(l): Branch on C1 (False/True) (Likely)
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm bc1f() { return AssignBranchFlag(ast.Unary("!", FCR31_CC())); }
		public AstNodeStm bc1fl() { return bc1f(); }
		public AstNodeStm bc1t() { return AssignBranchFlag(FCR31_CC()); }
		public AstNodeStm bc1tl() { return bc1t(); }
	}
}

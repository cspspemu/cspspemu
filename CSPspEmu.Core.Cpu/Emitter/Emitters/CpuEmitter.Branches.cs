using System;
using CSPspEmu.Core.Memory;
using SafeILGenerator;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		public SafeILGeneratorEx SafeILGenerator { get { return MipsMethodEmitter.SafeILGenerator; } }

		public void _branch_likely(Action Action)
		{
			var NullifyDelayedLabel = SafeILGenerator.DefineLabel("NullifyDelayedLabel");
			MipsMethodEmitter.LoadBranchFlag();
			SafeILGenerator.BranchIfFalse(NullifyDelayedLabel);
			{
				Action();
			}
			NullifyDelayedLabel.Mark();
		}

		// Code executed after the delayed slot.
		public void _branch_post(SafeLabel Label)
		{
			if (this.AndLink)
			{
				var SkipBranch = SafeILGenerator.DefineLabel("SkipBranch");
				MipsMethodEmitter.LoadBranchFlag();
				SafeILGenerator.BranchIfFalse(SkipBranch);
				{
					MipsMethodEmitter.SaveGPR(31, () =>
					{
						SafeILGenerator.Push((int)(BranchPC + 8));
					});

					SafeILGenerator.BranchAlways(Label);
				}
				SkipBranch.Mark();
			}
			else
			{
				MipsMethodEmitter.LoadBranchFlag();
				SafeILGenerator.BranchIfTrue(Label);
			}
		}

		bool AndLink = false;
		uint BranchPC = 0;

		private void GenerateAssignBranchFlag(AstNodeExpr Expr, bool AndLink = false)
		{
			this.AndLink = AndLink;
			this.BranchPC = PC;
			this.GenerateAssignREG(REG("BranchFlag"), this.Cast<bool>(Expr, Explicit: false)); 
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// beq(l)     : Branch on EQuals (Likely).
		// bne(l)     : Branch on Not Equals (Likely).
		// btz(al)(l) : Branch on Less Than Zero (And Link) (Likely).
		// blez(l)    : Branch on Less Or Equals than Zero (Likely).
		// bgtz(l)    : Branch on Great Than Zero (Likely).
		// bgez(al)(l): Branch on Greater Equal Zero (And Link) (Likely).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void beq() { this.GenerateAssignBranchFlag(this.Binary(GPR_s(RS), "==", GPR_s(RT))); }
		public void beql() { beq(); }
		public void bne() { this.GenerateAssignBranchFlag(this.Binary(GPR_s(RS), "!=", GPR_s(RT))); }
		public void bnel() { bne(); }
		public void bltz() { this.GenerateAssignBranchFlag(this.Binary(GPR_s(RS), "<", 0)); }
		public void bltzl() { bltz(); }
		[PspUntested]
		public void bltzal() { this.GenerateAssignBranchFlag(this.Binary(GPR_s(RS), "<", 0), AndLink: true); }
		public void bltzall() { bltzal(); }
		public void blez() { this.GenerateAssignBranchFlag(this.Binary(GPR_s(RS), "<=", 0)); }
		public void blezl() { blez(); }
		public void bgtz() { this.GenerateAssignBranchFlag(this.Binary(GPR_s(RS), ">", 0)); }
		public void bgtzl() { bgtz(); }
		public void bgez() { this.GenerateAssignBranchFlag(this.Binary(GPR_s(RS), ">=", 0)); }
		public void bgezl() { bgez(); }
		[PspUntested]
		public void bgezal() { this.GenerateAssignBranchFlag(this.Binary(GPR_s(RS), ">=", 0), AndLink: true); }
		public void bgezall() { bgezal(); }

		public bool PopulateCallStack { get { return !(CpuProcessor.Memory is FastPspMemory) && CpuProcessor.PspConfig.TrackCallStack; } }

		private AstNodeStm _popstack()
		{
			if (PopulateCallStack && (RS == 31))  return this.Statement(this.CallInstance(this.CpuThreadStateArgument(), (Action)CpuThreadState.Methods.CallStackPop));
			return this.Statement();
		}

		private AstNodeStm _pushstack()
		{
			if (PopulateCallStack) return this.Statement(this.CallInstance(this.CpuThreadStateArgument(), (Action<uint>)CpuThreadState.Methods.CallStackPush, PC));
			return this.Statement();
		}

		private void _link()
		{
			this.GenerateIL(this.Statements(this._pushstack(), this.AssignGPR(31, this.Immediate(PC + 8))));
		}

		private AstNodeStm JumpToAddress(AstNodeExpr Address)
		{
			return this.Statement(
				this.CallTail(
					this.CallInstance(
						this.CpuThreadStateArgument(),
						(Action<uint>)CpuThreadState.Methods.Jump,
						Address
					)
				)
			);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// j(al)(r): Jump (And Link) (Register)
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void j() { this.GenerateIL(this.JumpToAddress(Instruction.GetJumpAddress(PC))); }
		public void jal() { _link(); j(); }
		public void jr() { this.GenerateIL(this.Statements(_popstack(), JumpToAddress(GPR_u(RS)))); }
		public void jalr() { _link(); jr(); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// bc1(f/t)(l): Branch on C1 (False/True) (Likely)
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void bc1f() { this.GenerateAssignBranchFlag(this.Unary("!", FCR31_CC())); }
		public void bc1fl() { bc1f(); }
		public void bc1t() { this.GenerateAssignBranchFlag(FCR31_CC()); }
		public void bc1tl() { bc1t(); }
	}
}

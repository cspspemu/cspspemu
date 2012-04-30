using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	sealed public class DynarecBranchAnalyzer
	{
		static public Func<Instruction, JumpFlags> GetBranchInfo = (Instruction) =>
		{
			return _GetBranchInfo(Instruction.Value);
		};

		static private Func<uint, JumpFlags> _GetBranchInfo = EmitLookupGenerator.GenerateInfoDelegate<DynarecBranchAnalyzer, JumpFlags>(
			EmitLookupGenerator.GenerateSwitchDelegateReturn<DynarecBranchAnalyzer, JumpFlags>(
				InstructionTable.ALL, ThrowOnUnexistent: false
			),
			new DynarecBranchAnalyzer()
		);

		public Instruction Instruction;

		[Flags]
		public enum JumpFlags
		{
			NormalInstruction = (1 << 0),
			BranchOrJumpInstruction = (1 << 1),
			SyscallInstruction = (1 << 2),
			JumpInstruction = (1 << 3),
			FpuInstruction = (1 << 4),
			VFpuInstruction = (1 << 5),

			JumpAlways = (1 << 10),
			Likely = (1 << 11),
			AndLink = (1 << 12),

			FixedJump = 0,
			DynamicJump = (1 << 20),
		}

		public JumpFlags bvf() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction; }
		public JumpFlags bvt() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction; }
		public JumpFlags bvfl() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction | JumpFlags.Likely; }
		public JumpFlags bvtl() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.VFpuInstruction | JumpFlags.Likely; }

		public JumpFlags beq() { return JumpFlags.BranchOrJumpInstruction | ((Instruction.RS == Instruction.RT) ? JumpFlags.JumpAlways : 0); }
		public JumpFlags bne() { return JumpFlags.BranchOrJumpInstruction; }
		public JumpFlags beql() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely; }
		public JumpFlags bnel() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely; }

		public JumpFlags bltz() { return JumpFlags.BranchOrJumpInstruction; }
		public JumpFlags bltzal() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.AndLink; }
		public JumpFlags bltzl() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely; }
		public JumpFlags bltzall() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.AndLink | JumpFlags.Likely; }

		public JumpFlags blez() { return JumpFlags.BranchOrJumpInstruction; }
		public JumpFlags blezl() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely; }

		public JumpFlags bgtz() { return JumpFlags.BranchOrJumpInstruction; }
		public JumpFlags bgez() { return JumpFlags.BranchOrJumpInstruction; }
		public JumpFlags bgtzl() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely; }
		public JumpFlags bgezl() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.Likely; }
		public JumpFlags bgezal() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.AndLink; }
		public JumpFlags bgezall() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.AndLink | JumpFlags.Likely; }

		public JumpFlags j() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways; }
		public JumpFlags jr() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways | JumpFlags.DynamicJump; }
		public JumpFlags jalr() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways | JumpFlags.AndLink | JumpFlags.DynamicJump; }
		public JumpFlags jal() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.JumpInstruction | JumpFlags.JumpAlways | JumpFlags.AndLink; }

		public JumpFlags bc1f() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction; }
		public JumpFlags bc1t() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction; }
		public JumpFlags bc1fl() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction | JumpFlags.Likely; }
		public JumpFlags bc1tl() { return JumpFlags.BranchOrJumpInstruction | JumpFlags.FpuInstruction | JumpFlags.Likely; }

		public JumpFlags syscall() { return JumpFlags.SyscallInstruction; }

		public JumpFlags unhandled() { return JumpFlags.NormalInstruction; }
		public JumpFlags unknown() { return JumpFlags.NormalInstruction; }
	}

}

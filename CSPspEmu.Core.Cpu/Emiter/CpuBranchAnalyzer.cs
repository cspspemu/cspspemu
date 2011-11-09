using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public class CpuBranchAnalyzer
	{
		public Instruction Instruction;

		public enum Flags
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

		public Flags bvf() { return Flags.BranchOrJumpInstruction | Flags.VFpuInstruction; }
		public Flags bvt() { return Flags.BranchOrJumpInstruction | Flags.VFpuInstruction; }
		public Flags bvfl() { return Flags.BranchOrJumpInstruction | Flags.VFpuInstruction | Flags.Likely; }
		public Flags bvtl() { return Flags.BranchOrJumpInstruction | Flags.VFpuInstruction | Flags.Likely; }

		public Flags beq() { return Flags.BranchOrJumpInstruction | ((Instruction.RS == Instruction.RT) ? Flags.JumpAlways : 0); }
		public Flags bne() { return Flags.BranchOrJumpInstruction; }
		public Flags beql() { return Flags.BranchOrJumpInstruction | Flags.Likely; }
		public Flags bnel() { return Flags.BranchOrJumpInstruction | Flags.Likely; }

		public Flags bltz() { return Flags.BranchOrJumpInstruction; }
		public Flags bltzal() { return Flags.BranchOrJumpInstruction | Flags.AndLink; }
		public Flags bltzl() { return Flags.BranchOrJumpInstruction | Flags.Likely; }
		public Flags bltzall() { return Flags.BranchOrJumpInstruction | Flags.AndLink | Flags.Likely; }

		public Flags blez() { return Flags.BranchOrJumpInstruction; }
		public Flags blezl() { return Flags.BranchOrJumpInstruction | Flags.Likely; }

		public Flags bgtz() { return Flags.BranchOrJumpInstruction; }
		public Flags bgez() { return Flags.BranchOrJumpInstruction; }
		public Flags bgezal() { return Flags.BranchOrJumpInstruction | Flags.AndLink; }
		public Flags bgtzl() { return Flags.BranchOrJumpInstruction | Flags.Likely; }
		public Flags bgezl() { return Flags.BranchOrJumpInstruction | Flags.Likely; }
		public Flags bgezall() { return Flags.BranchOrJumpInstruction | Flags.AndLink | Flags.Likely; }

		public Flags j() { return Flags.BranchOrJumpInstruction | Flags.JumpInstruction | Flags.JumpAlways; }
		public Flags jr() { return Flags.BranchOrJumpInstruction | Flags.JumpInstruction | Flags.JumpAlways | Flags.DynamicJump; }
		public Flags jalr() { return Flags.BranchOrJumpInstruction | Flags.JumpInstruction | Flags.JumpAlways | Flags.AndLink | Flags.DynamicJump; }
		public Flags jal() { return Flags.BranchOrJumpInstruction | Flags.JumpInstruction | Flags.JumpAlways | Flags.AndLink; }

		public Flags bc1f() { return Flags.BranchOrJumpInstruction | Flags.FpuInstruction; }
		public Flags bc1t() { return Flags.BranchOrJumpInstruction | Flags.FpuInstruction; }
		public Flags bc1fl() { return Flags.BranchOrJumpInstruction | Flags.FpuInstruction | Flags.Likely; }
		public Flags bc1tl() { return Flags.BranchOrJumpInstruction | Flags.FpuInstruction | Flags.Likely; }

		public Flags syscall() { return Flags.SyscallInstruction; }

		public Flags unhandled() { return Flags.NormalInstruction; }
		//public Flags unknown() { throw (new InvalidOperationException()); }
		public Flags unknown() { return Flags.NormalInstruction; }
	}
}

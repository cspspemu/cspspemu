using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		private MipsMethodEmiter MipsMethodEmiter;
		public Instruction Instruction;

		public int RT { get { return Instruction.RT; } }
		public int RD { get { return Instruction.RD; } }
		public int RS { get { return Instruction.RS; } }
		public int IMM { get { return Instruction.IMM; } }
		public uint IMMU { get { return Instruction.IMMU; } }

		public CpuEmiter(MipsMethodEmiter MipsMethodEmiter)
		{
			this.MipsMethodEmiter = MipsMethodEmiter;
		}
	}
}

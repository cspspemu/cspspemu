using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Cpu.Emiter;

namespace CSPspEmu.Core.Cpu
{
	unsafe sealed public class Processor
	{
		public AbstractPspMemory Memory;
		public MethodCache MethodCache;

		public Processor(AbstractPspMemory Memory)
		{
			this.Memory = Memory;
			this.MethodCache = new MethodCache();
		}
	}
}

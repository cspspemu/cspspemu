using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	unsafe sealed public class Processor
	{
		public AbstractPspMemory Memory;

		public Processor(AbstractPspMemory Memory)
		{
			this.Memory = Memory;
		}
	}
}

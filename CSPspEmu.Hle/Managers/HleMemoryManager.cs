using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSharpUtils;

namespace CSPspEmu.Hle.Managers
{
	public class HleMemoryManager
	{
		public MemoryPartition RootPartition = new MemoryPartition(PspMemory.MainOffset, PspMemory.MainOffset + PspMemory.MainSize);
		public PspMemory Memory;

		public HleMemoryManager(PspMemory Memory)
		{
			this.Memory = Memory;
		}
	}
}

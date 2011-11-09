using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu
{
	unsafe public class GpuProcessor
	{
		public uint* CurrentAddress;
		public uint* StallAddress;

		public void Process()
		{
			for (; CurrentAddress < StallAddress; CurrentAddress++)
			{
				uint Instruction = *CurrentAddress;
				var GpuCommand = (GpuCommands)(Instruction & 0xFF);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu
{
	unsafe public class GpuProcessor
	{
		public uint* InstructionAddressCurrent;
		public uint* InstructionAddressStall;

		public void Process()
		{
			for (; InstructionAddressCurrent < InstructionAddressStall; InstructionAddressCurrent++)
			{
				uint Instruction = *InstructionAddressCurrent;
				var GpuCommand = (GpuCommands)(Instruction & 0xFF);
			}
		}
	}
}

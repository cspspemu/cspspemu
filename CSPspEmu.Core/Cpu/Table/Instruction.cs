using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Table
{
	public class Instruction
	{
		[InstructionInfo(Name = "add", BinaryEncoding = "000000:rs:rt:rd:00000:100000", AsmEncoding = "%d, %s, %t", AddressType = AddressType.None)]
		static public bool Add;
	}
}

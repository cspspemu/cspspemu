using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.Core.Cpu.Table
{
	public class InstructionInfo : Attribute
	{
		public String Name;
		public String BinaryEncoding;
		public String AsmEncoding;
		public AddressType AddressType;
	}
}

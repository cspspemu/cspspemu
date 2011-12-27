using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Memory
{
	unsafe public struct PspPointer
	{
		public uint Address;

		public PspPointer(uint Address)
		{
			this.Address = Address;
		}

		public override string ToString()
		{
			return String.Format("PspPointer(0x{0:X})", Address);
		}
	}

	/*
	public struct PspPointer<TType>
	{
		public uint Address;

		public PspPointer(uint Address)
		{
			this.Address = Address;
		}
	}
	*/
}

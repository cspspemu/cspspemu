using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Memory
{
	unsafe public struct PspPointer
	{
		public uint Address;

		public uint Low24
		{
			set
			{
				Address = (Address & 0xFF000000) | (value & 0x00FFFFFF);
			}
		}

		public uint High8
		{
			set
			{
				Address = (Address & 0x00FFFFFF) | (value & 0xFF000000);
			}
		}

		public PspPointer(uint Address)
		{
			this.Address = Address;
		}

		public static implicit operator uint(PspPointer that)
		{
			return that.Address;
		}

		public static implicit operator PspPointer(uint that)
		{
			return new PspPointer()
			{
				Address = that,
			};
		}

		public override string ToString()
		{
			return String.Format("PspPointer(0x{0:X})", Address);
		}

		public bool IsNull { get { return Address == 0; } }
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

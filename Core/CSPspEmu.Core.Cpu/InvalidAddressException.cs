using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	public class InvalidAddressException : Exception
	{
		public InvalidAddressException(string message) : base(message) { }
		public InvalidAddressException(string message, Exception innerException) : base(message, innerException) { }
		public InvalidAddressException(ulong Address) : base(String.Format("Invalid Address : 0x{0:X8}", Address)) { }
		public InvalidAddressException(ulong Address, Exception innerException) : base(String.Format("Invalid Address : 0x{0:X8}", Address), innerException) { }
	}
}

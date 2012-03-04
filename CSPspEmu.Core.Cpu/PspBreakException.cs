using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	public class PspBreakException : Exception
	{
		public PspBreakException(string Message) : base(Message) { }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public class PspManualResetEvent : PspResetEvent
	{
		public PspManualResetEvent(bool InitialValue)
			: base(InitialValue, AutoReset: false)
		{
		}
	}
}

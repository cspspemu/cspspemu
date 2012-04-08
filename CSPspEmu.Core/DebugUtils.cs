using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core
{
	public class DebugUtils
	{
		static public void IsDebuggerPresentDebugBreak()
		{
			if (Platform.IsDebuggerPresent()) Platform.DebugBreak();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CSPspEmu.Core
{
	public class DebugUtils
	{
		static public void IsDebuggerPresentDebugBreak()
		{
			if (Debugger.IsAttached) Debugger.Break();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Debug
{
	public class DebugUtils
	{
		static public void IsDebuggerPresentDebugBreak()
		{
			if (IsDebuggerPresent()) DebugBreak();
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern bool IsDebuggerPresent();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		internal static extern void DebugBreak();

	}
}

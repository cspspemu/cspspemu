using System.Diagnostics;

namespace CSPspEmu.Core
{
	public static class DebugUtils
	{
		public static void IsDebuggerPresentDebugBreak()
		{
			if (Debugger.IsAttached) Debugger.Break();
		}
	}
}

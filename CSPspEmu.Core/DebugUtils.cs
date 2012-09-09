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

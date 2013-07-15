using CSPspEmu.Core;
using CSPspEmu.Core.Types;
using CSPspEmu.Hle.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CSPspEmu.Hle
{
	public class HleConfig
	{
		[Inject]
		PspStoredConfig PspStoredConfig;

		public bool DebugThreadSwitching = false;
		public bool DebugNotImplemented = true;

		public PspLanguages Language;
		public PspConfirmButton ConfirmButton;
		public bool WlanIsOn = false;
		public bool UseCoRoutines = false;
		public bool DebugSyscalls = false;

		//public PspVersion FirmwareVersion = new PspVersion(6, 3, 0);
		public PspVersion FirmwareVersion = new PspVersion("6.3.0.0");
		public Assembly HleModulesDll;
		public bool TraceLastSyscalls;

		private HleConfig()
		{
		}
	}
}

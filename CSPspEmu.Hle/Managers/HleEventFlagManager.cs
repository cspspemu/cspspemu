using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Threading.EventFlags;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Managers
{
	public enum EventFlagId : int { }

	public class HleEventFlagManager : PspEmulatorComponent
	{
		public HleUidPoolSpecial<HleEventFlag, EventFlagId> EventFlags = new HleUidPoolSpecial<HleEventFlag, EventFlagId>()
		{
			OnKeyNotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_EVENT_FLAG,
		};

		public override void InitializeComponent()
		{
		}
	}
}

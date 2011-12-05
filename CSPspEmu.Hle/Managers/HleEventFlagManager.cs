using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Threading.EventFlags;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Managers
{
	public class HleEventFlagManager : PspEmulatorComponent
	{
		public HleUidPool<HleEventFlag> EventFlags;

		public override void InitializeComponent()
		{
			this.EventFlags = new HleUidPool<HleEventFlag>();
		}
	}
}

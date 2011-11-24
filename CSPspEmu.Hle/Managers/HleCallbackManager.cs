using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Managers
{
	public class HleCallbackManager : PspEmulatorComponent
	{
		public HleUidPool<HleCallback> Callbacks { get; protected set; }

		public HleCallbackManager(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			this.Callbacks = new HleUidPool<HleCallback>();
		}
	}
}

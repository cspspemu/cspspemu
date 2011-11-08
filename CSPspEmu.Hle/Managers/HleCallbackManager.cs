using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Managers
{
	public class HleCallbackManager
	{
		public HleState HleState;

		public HleUidPool<HleCallback> Callbacks { get; protected set; }

		public HleCallbackManager(HleState HleState)
		{
			this.HleState = HleState;
			this.Callbacks = new HleUidPool<HleCallback>();
		}
	}
}

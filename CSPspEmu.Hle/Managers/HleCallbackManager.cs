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
		private Queue<HleCallback> ScheduledCallbacks;

		public override void InitializeComponent()
		{
			this.Callbacks = new HleUidPool<HleCallback>();
			this.ScheduledCallbacks = new Queue<HleCallback>();
		}

		public void ScheduleCallback(HleCallback HleCallback)
		{
			lock (this)
			{
				this.ScheduledCallbacks.Enqueue(HleCallback);
			}
		}

		public HleCallback DequeueScheduledCallback()
		{
			return ScheduledCallbacks.Dequeue();
		}

		public bool HasScheduledCallbacks
		{
			get
			{
				lock (this)
				{
					return ScheduledCallbacks.Count > 0;
				}
			}
		}
	}
}

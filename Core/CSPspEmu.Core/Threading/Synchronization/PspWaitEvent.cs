using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public class PspWaitEvent
	{
		protected Queue<Action> Notifications = new Queue<Action>();

		public void Signal()
		{
			while (Notifications.Count > 0)
			{
				Notifications.Dequeue()();
			}
		}

		public void CallbackOnStateOnce(Action callback)
		{
			Notifications.Enqueue(callback);
		}
	}
}

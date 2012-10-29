using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public abstract class PspResetEvent
	{
		protected bool Value;
		protected bool AutoReset;
		Queue<Action> Actions = new Queue<Action>();

		protected PspResetEvent(bool InitialValue, bool AutoReset)
		{
			this.Value = InitialValue;
			this.AutoReset = AutoReset;
		}

		public void Set()
		{
			lock (this)
			{
				Value = true;
				if (Actions.Count > 0) Reset();
				while (Actions.Count > 0)
				{
					var Action = Actions.Dequeue();
					Action();
				}
			}
		}

		public void Reset()
		{
			lock (this)
			{
				Value = false;
			}
		}

		public void CallbackOnSet(Action Action)
		{
			lock (this)
			{
				bool CurrentValue = this.Value;
				if (CurrentValue)
				{
					Action();
					if (AutoReset) Reset();
				}
				else
				{
					Actions.Enqueue(Action);
				}
			}
		}
	}
}

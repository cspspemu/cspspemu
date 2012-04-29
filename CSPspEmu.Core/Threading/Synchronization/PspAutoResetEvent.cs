using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public class PspAutoResetEvent
	{
		protected bool Value;
		Queue<Action> Actions = new Queue<Action>();

		public PspAutoResetEvent(bool InitialValue)
		{
			this.Value = InitialValue;
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
					Reset();
				}
				else
				{
					Actions.Enqueue(Action);
				}
			}
		}
	}
}

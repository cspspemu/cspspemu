using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public abstract class PspResetEvent
	{
		private bool Value;
		private bool AutoReset;
		private Queue<Action> Actions = new Queue<Action>();

		protected PspResetEvent(bool InitialValue, bool AutoReset)
		{
			this.Value = InitialValue;
			this.AutoReset = AutoReset;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Set()
		{
			Value = true;
			if (Actions.Count > 0) Reset();
			while (Actions.Count > 0)
			{
				var Action = Actions.Dequeue();
				Action();
			}
			if (AutoReset) Reset();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Reset()
		{
			Value = false;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void CallbackOnSet(Action Action)
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

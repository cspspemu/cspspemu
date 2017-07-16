using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public abstract class PspResetEvent
	{
		private bool _value;
		private readonly bool _autoReset;
		private Queue<Action> Actions = new Queue<Action>();

		protected PspResetEvent(bool initialValue, bool autoReset)
		{
			_value = initialValue;
			_autoReset = autoReset;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Set()
		{
			_value = true;
			if (Actions.Count > 0) Reset();
			while (Actions.Count > 0)
			{
				var action = Actions.Dequeue();
				action();
			}
			if (_autoReset) Reset();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Reset()
		{
			_value = false;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void CallbackOnSet(Action action)
		{
			var currentValue = _value;
			if (currentValue)
			{
				action();
				if (_autoReset) Reset();
			}
			else
			{
				Actions.Enqueue(action);
			}
		}
	}
}

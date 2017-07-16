using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public sealed class WaitableStateMachine<TEnum>
	{
		private TEnum _value;
		private AutoResetEvent ValueUpdatedEvent = new AutoResetEvent(false);
		private bool Debug = false;
		private readonly Dictionary<TEnum, List<Action>> _notifications = new Dictionary<TEnum, List<Action>>();

		public WaitableStateMachine(bool debug = false)
		{
			Debug = debug;
		}

		public WaitableStateMachine(TEnum initialValue, bool debug = false)
		{
			SetValue(initialValue);
			Debug = debug;
		}

		public TEnum Value => _value;

		public void SetValue(TEnum value)
		{
			lock (_notifications)
			{
				if (Debug) Console.WriteLine("WaitableStateMachine::SetValue({0})", value);
				_value = value;
				_ValueWasUpdated();
			}
		}

		public void SetTemporalValue(TEnum value, Action action)
		{
			var oldValue = _value;
			SetValue(value);
			try
			{
				action();
			}
			finally
			{
				SetValue(oldValue);
			}
		}

		private void _ValueWasUpdated()
		{
			if (Debug) Console.WriteLine("WaitableStateMachine::ValueWasUpdated: " + Value);
			if (_notifications.ContainsKey(Value))
			{
				if (Debug) Console.WriteLine("  Contains");
				foreach (var callback in _notifications[Value])
				{
					if (Debug) Console.WriteLine("    Callback");
					callback();
				}
				_notifications[Value] = new List<Action>();
			}

			ValueUpdatedEvent.Set();
		}

		public void CallbackOnStateOnce(TEnum expectedValue, Action callback)
		{
			lock (_notifications)
			{
				if (Debug) Console.WriteLine($"CallbackOnStateOnce({expectedValue}, Callback). Current: {Value}");

				if (Value.Equals(expectedValue))
				{
					callback();
				}
				else
				{
					if (!_notifications.ContainsKey(expectedValue)) _notifications[expectedValue] = new List<Action>();
					_notifications[expectedValue].Add(callback);
				}
			}
		}

		public void WaitForAnyState(params TEnum[] expectedValues)
		{
			while (true)
			{
				if (expectedValues.Contains(Value))
				{
					return;
				}
				ValueUpdatedEvent.WaitOne();
			}
		}

		public void WaitForState(TEnum expectedValue)
		{
			WaitForAnyState(expectedValue);
		}
	}
}

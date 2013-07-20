using System;
using System.Collections.Generic;
using System.Threading;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public sealed class WaitableStateMachine<TEnum>
	{
		private TEnum _Value;
		private AutoResetEvent ValueUpdatedEvent = new AutoResetEvent(false);
		private bool Debug = false;
		private readonly Dictionary<TEnum, List<Action>> Notifications = new Dictionary<TEnum, List<Action>>();

		public WaitableStateMachine(bool Debug = false)
		{
			this.Debug = Debug;
		}

		public WaitableStateMachine(TEnum InitialValue, bool Debug = false)
		{
			this.SetValue(InitialValue);
			this.Debug = Debug;
		}

		public TEnum Value {
			get
			{
				return _Value;
			}
		}

		public void SetValue(TEnum value)
		{
			lock (Notifications)
			{
				if (Debug) Console.WriteLine("WaitableStateMachine::SetValue({0})", value);
				this._Value = value;
				this._ValueWasUpdated();
			}
		}

		private void _ValueWasUpdated()
		{
			if (Debug) Console.WriteLine("WaitableStateMachine::ValueWasUpdated: " + Value);
			if (Notifications.ContainsKey(Value))
			{
				if (Debug) Console.WriteLine("  Contains");
				foreach (var Callback in Notifications[Value])
				{
					if (Debug) Console.WriteLine("    Callback");
					Callback();
				}
				Notifications[Value] = new List<Action>();
			}

			ValueUpdatedEvent.Set();
		}

		public void CallbackOnStateOnce(TEnum ExpectedValue, Action Callback)
		{
			lock (Notifications)
			{
				if (Debug) Console.WriteLine(String.Format("CallbackOnStateOnce({0}, Callback). Current: {1}", ExpectedValue, Value));

				if (Value.Equals(ExpectedValue))
				{
					Callback();
				}
				else
				{
					if (!Notifications.ContainsKey(ExpectedValue)) Notifications[ExpectedValue] = new List<Action>();
					Notifications[ExpectedValue].Add(Callback);
				}
			}
		}

		public void WaitForAnyState(params TEnum[] ExpectedValues)
		{
			while (true)
			{
				foreach (var ExpectedValue in ExpectedValues) if (Value.Equals(ExpectedValue)) return;
				ValueUpdatedEvent.WaitOne();
			}
		}

		public void WaitForState(TEnum ExpectedValue)
		{
			WaitForAnyState(ExpectedValue);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public class WaitableStateMachine<TEnum>
	{
		protected TEnum _Value;
		protected AutoResetEvent ValueUpdatedEvent = new AutoResetEvent(false);
		protected bool Debug = false;

		public WaitableStateMachine(bool Debug = false)
		{
			this.Debug = Debug;
		}

		public TEnum Value {
			/*
			set
			{
				if (!_Value.Equals(value))
				{
					_Value = value;
					//ValueWasUpdated();
				}
				ValueWasUpdated();
			}
			*/
			get
			{
				return _Value;
			}
		}

		public void SetValue(TEnum value)
		{
			_Value = value;
			ValueWasUpdated();
		}

		Dictionary<TEnum, List<Action>> Notifications = new Dictionary<TEnum, List<Action>>();

		protected void ValueWasUpdated()
		{
			lock (Notifications)
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
			}

			ValueUpdatedEvent.Set();
		}

		public void CallbackOnStateOnce(TEnum ExpectedValue, Action Callback)
		{
			lock (Notifications)
			{
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

		public void WaitForState(TEnum ExpectedValue)
		{
			while (!ExpectedValue.Equals(Value))
			{
				ValueUpdatedEvent.WaitOne();
			}
		}
	}
}

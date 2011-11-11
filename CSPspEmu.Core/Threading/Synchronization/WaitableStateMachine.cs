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

		public TEnum Value {
			set
			{
				if (!_Value.Equals(value))
				{
					_Value = value;
					ValueWasUpdated();
				}
			}
			get
			{
				return _Value;
			}
		}

		protected void ValueWasUpdated()
		{
			ValueUpdatedEvent.Set();
		}

		public void WaitForState(TEnum ExpectedValue)
		{
			while (ExpectedValue.Equals(Value))
			{
				ValueUpdatedEvent.WaitOne();
			}
		}
	}
}

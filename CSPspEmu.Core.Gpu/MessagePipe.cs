using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSPspEmu.Core.Gpu
{
	public class MessagePipe<TMessage>
	{
		AutoResetEvent HasPendingMessages = new AutoResetEvent(false);
		Queue<TMessage> Messages = new Queue<TMessage>();
		//object CheckingAvailableMessagess = new object();

		public void Send(TMessage Message)
		{
			lock (Messages)
			{
				Messages.Enqueue(Message);
			}

			HasPendingMessages.Set();
		}

		public int MessageCount
		{
			get
			{
				lock (Messages)
				{
					return Messages.Count;
				}
			}
		}

		protected void WaitOne()
		{
			while (MessageCount == 0)
			{
				HasPendingMessages.WaitOne(1);
			}
		}

		public void Receive(Action<TMessage> Callback)
		{
			WaitOne();

			lock (Messages)
			{
				while (Messages.Count > 0)
				{
					Callback(Messages.Dequeue());
				}
			}
		}
	}
}

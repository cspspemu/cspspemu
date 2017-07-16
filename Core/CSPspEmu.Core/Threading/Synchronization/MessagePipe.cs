using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public class MessagePipe<TMessage>
	{
		public class WaitableMessage
		{
			public TMessage Message;
			public AutoResetEvent Event = new AutoResetEvent(false);
		}

		LinkedList<WaitableMessage> MessageQueue = new LinkedList<WaitableMessage>();
		AutoResetEvent MessageEvent = new AutoResetEvent(false);

		public void Receive(Action<TMessage> handler)
		{
			WaitableMessage waitableMessage;
			while (true)
			{
				lock (MessageQueue)
				{
					if (MessageQueue.Count > 0) break;
				}
				MessageEvent.WaitOne();
			}
			lock (MessageQueue)
			{
				waitableMessage = MessageQueue.First();
				MessageQueue.RemoveFirst();
			}
			handler(waitableMessage.Message);
			waitableMessage.Event.Set();
		}

		private WaitableMessage Push(TMessage message, bool last)
		{
			var waitableMessage = new WaitableMessage()
			{
				Message = message,
			};
			lock (MessageQueue)
			{
				if (last)
				{
					MessageQueue.AddLast(waitableMessage);
				}
				else
				{
					MessageQueue.AddFirst(waitableMessage);
				}
			}
			MessageEvent.Set();
			return waitableMessage;
		}

		public WaitableMessage PushLast(TMessage message)
		{
			return Push(message, last: true);
		}

		public WaitableMessage PushFirst(TMessage message)
		{
			return Push(message, last: false);
		}

		public void PushLastAndWait(TMessage message)
		{
			PushLast(message).Event.WaitOne();
		}

		public void PushFirstAndWait(TMessage message)
		{
			PushFirst(message).Event.WaitOne();
		}
	}
}

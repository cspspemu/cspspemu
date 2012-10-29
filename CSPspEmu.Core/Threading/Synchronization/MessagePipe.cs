using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public class MessagePipe<TMessage>
	{
		public class WaitableMessage<TMessage>
		{
			public TMessage Message;
			public AutoResetEvent Event = new AutoResetEvent(false);
		}

		LinkedList<WaitableMessage<TMessage>> MessageQueue = new LinkedList<WaitableMessage<TMessage>>();
		AutoResetEvent MessageEvent = new AutoResetEvent(false);

		public MessagePipe()
		{
		}

		public void Receive(Action<TMessage> Handler)
		{
			WaitableMessage<TMessage> WaitableMessage;
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
				WaitableMessage = MessageQueue.First();
				MessageQueue.RemoveFirst();
			}
			Handler(WaitableMessage.Message);
			WaitableMessage.Event.Set();
		}

		private WaitableMessage<TMessage> Push(TMessage Message, bool Last)
		{
			var WaitableMessage = new WaitableMessage<TMessage>()
			{
				Message = Message,
			};
			lock (MessageQueue)
			{
				if (Last)
				{
					MessageQueue.AddLast(WaitableMessage);
				}
				else
				{
					MessageQueue.AddFirst(WaitableMessage);
				}
			}
			MessageEvent.Set();
			return WaitableMessage;
		}

		public WaitableMessage<TMessage> PushLast(TMessage Message)
		{
			return Push(Message, Last: true);
		}

		public WaitableMessage<TMessage> PushFirst(TMessage Message)
		{
			return Push(Message, Last: false);
		}

		public void PushLastAndWait(TMessage Message)
		{
			PushLast(Message).Event.WaitOne();
		}

		public void PushFirstAndWait(TMessage Message)
		{
			PushFirst(Message).Event.WaitOne();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSharpUtils.Threading
{
	public class TaskQueue
	{
		public AutoResetEvent EnqueuedEvent { get; protected set; }
		protected Queue<Action> Tasks = new Queue<Action>();

		public TaskQueue()
		{
			EnqueuedEvent = new AutoResetEvent(false);
		}

		public void WaitAndHandleEnqueued()
		{
			WaitEnqueued();
			HandleEnqueued();
		}

		public void WaitEnqueued()
		{
			EnqueuedEvent.WaitOne();
		}

		public void HandleEnqueued()
		{
			while (Tasks.Count > 0)
			{
				Action Action;
				lock (Tasks)
				{
					Action = Tasks.Dequeue();
				}
				Action();
			}
		}

		public void Enqueue(Action Action)
		{
			lock (Tasks)
			{
				Tasks.Enqueue(Action);
			}
		}

		public void EnqueueAndWaitStarted(Action Action)
		{
			var Event = new AutoResetEvent(false);

			Enqueue(() =>
			{
				Event.Set();
				Action();
			});

			EnqueuedEvent.Set();

			Event.WaitOne();
		}

		public void EnqueueAndWaitCompleted(Action Action)
		{
			var Event = new AutoResetEvent(false);

			Enqueue(() =>
			{
				Action();
				Event.Set();
			});

			EnqueuedEvent.Set();

			Event.WaitOne();
		}
	}
}

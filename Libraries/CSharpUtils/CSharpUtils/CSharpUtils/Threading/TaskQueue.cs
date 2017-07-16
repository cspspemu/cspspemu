using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CSharpUtils.Threading
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class TaskQueue
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly AutoResetEvent EnqueuedEvent;

		/// <summary>
		/// 
		/// </summary>
		private Queue<Action> Tasks = new Queue<Action>();

		/// <summary>
		/// 
		/// </summary>
		public TaskQueue()
		{
			EnqueuedEvent = new AutoResetEvent(false);
		}

		/// <summary>
		/// 
		/// </summary>
		public void WaitAndHandleEnqueued()
		{
			//Console.WriteLine("WaitEnqueued");
			WaitEnqueued();
			//Console.WriteLine("HandleEnqueued");
			HandleEnqueued();
		}

		/// <summary>
		/// 
		/// </summary>
		public void WaitEnqueued()
		{
			int TasksCount;
			lock (Tasks) TasksCount = Tasks.Count;

			if (TasksCount == 0)
			{
				EnqueuedEvent.WaitOne();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void HandleEnqueued()
		{
			while (true)
			{
				Action Action;

				lock (Tasks)
				{
					if (Tasks.Count == 0) break;
					Action = Tasks.Dequeue();
				}

				Action();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Action"></param>
		public void EnqueueWithoutWaiting(Action Action)
		{
			lock (Tasks)
			{
				Tasks.Enqueue(Action);
				EnqueuedEvent.Set();
			}
		}

		public void EnqueueAndWaitStarted(Action Action)
		{
			var Event = new AutoResetEvent(false);

			EnqueueWithoutWaiting(() =>
			{
				Event.Set();
				Action();
			});

			Event.WaitOne();
		}

		public void EnqueueAndWaitStarted(Action Action, TimeSpan Timeout, Action ActionTimeout = null)
		{
			var Event = new AutoResetEvent(false);
		
			EnqueueWithoutWaiting(() =>
			{
				Event.Set();
				Action();
			});
				
			if (!Event.WaitOne(Timeout))
			{
				Console.WriteLine("Timeout!");
				if (ActionTimeout != null) ActionTimeout();
			}
		}

		public void EnqueueAndWaitCompleted(Action Action)
		{
			var Event = new AutoResetEvent(false);

			EnqueueWithoutWaiting(() =>
			{
				Action();
				Event.Set();
			});

			Event.WaitOne();
		}
	}
}

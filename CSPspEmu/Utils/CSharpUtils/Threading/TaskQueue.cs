using System;
using System.Collections.Generic;
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
        private readonly Queue<Action> _tasks = new Queue<Action>();

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
            int tasksCount;
            lock (_tasks) tasksCount = _tasks.Count;

            if (tasksCount == 0)
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
                Action action;

                lock (_tasks)
                {
                    if (_tasks.Count == 0) break;
                    action = _tasks.Dequeue();
                }

                action();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void EnqueueWithoutWaiting(Action action)
        {
            lock (_tasks)
            {
                _tasks.Enqueue(action);
                EnqueuedEvent.Set();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void EnqueueAndWaitStarted(Action action)
        {
            var Event = new AutoResetEvent(false);

            EnqueueWithoutWaiting(() =>
            {
                Event.Set();
                action();
            });

            Event.WaitOne();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeout"></param>
        /// <param name="actionTimeout"></param>
        public void EnqueueAndWaitStarted(Action action, TimeSpan timeout, Action actionTimeout = null)
        {
            var Event = new AutoResetEvent(false);

            EnqueueWithoutWaiting(() =>
            {
                Event.Set();
                action();
            });

            if (!Event.WaitOne(timeout))
            {
                Console.WriteLine("Timeout!");
                actionTimeout?.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void EnqueueAndWaitCompleted(Action action)
        {
            var Event = new AutoResetEvent(false);

            EnqueueWithoutWaiting(() =>
            {
                action();
                Event.Set();
            });

            Event.WaitOne();
        }
    }
}
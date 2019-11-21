using System;
using System.Collections.Generic;
using System.Threading;

namespace CSharpUtils.Threading
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomThreadPool
    {
        /// <summary>
        /// 
        /// </summary>
        public class WorkerThread
        {
            private bool _running;
            private readonly AutoResetEvent _moreTasksEvent;
            private Queue<Action> Tasks;
            internal long LoopIterCount;

            /// <summary>
            /// 
            /// </summary>
            public WorkerThread()
            {
                _running = true;
                LoopIterCount = 0;
                _moreTasksEvent = new AutoResetEvent(false);
                Tasks = new Queue<Action>();
                var thread = new Thread(ThreadBody);
                thread.IsBackground = true;
                thread.Start();
            }

            internal void AddTask(Action task)
            {
                Tasks.Enqueue(task);
                _moreTasksEvent.Set();
            }

            internal void Stop()
            {
                AddTask(() => { _running = false; });
            }

            /// <summary>
            /// 
            /// </summary>
            protected void ThreadBody()
            {
                //Console.WriteLine("CustomThreadPool.ThreadBody.Start()");
                try
                {
                    LoopIterCount = 0;
                    while (_running)
                    {
                        _moreTasksEvent.WaitOne();
                        LoopIterCount++;
                        while (Tasks.Count > 0)
                        {
                            Tasks.Dequeue()();
                        }
                    }
                }
                finally
                {
                    //Console.WriteLine("CustomThreadPool.ThreadBody.End()");
                }
            }
        }

        internal WorkerThread[] WorkerThreads;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfThreads"></param>
        public CustomThreadPool(int numberOfThreads)
        {
            WorkerThreads = new WorkerThread[numberOfThreads];
            for (int n = 0; n < numberOfThreads; n++)
            {
                WorkerThreads[n] = new WorkerThread();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadAffinity"></param>
        /// <returns></returns>
        public long GetLoopIterCount(int threadAffinity)
        {
            return WorkerThreads[threadAffinity % WorkerThreads.Length].LoopIterCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadAffinity"></param>
        /// <param name="task"></param>
        public void AddTask(int threadAffinity, Action task)
        {
            WorkerThreads[threadAffinity % WorkerThreads.Length].AddTask(task);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            foreach (var workerThread in WorkerThreads)
            {
                workerThread.Stop();
            }
        }
    }
}
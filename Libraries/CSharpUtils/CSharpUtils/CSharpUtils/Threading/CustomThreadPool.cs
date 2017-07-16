using System;
using System.Collections.Generic;
using System.Threading;

namespace CSharpUtils.Threading
{
    public class CustomThreadPool
    {
        public class WorkerThread
        {
            private bool Running;
            private AutoResetEvent MoreTasksEvent;
            private Thread Thread;
            private Queue<Action> Tasks;
            internal long LoopIterCount;

            public WorkerThread()
            {
                this.Running = true;
                this.LoopIterCount = 0;
                this.MoreTasksEvent = new AutoResetEvent(false);
                this.Tasks = new Queue<Action>();
                this.Thread = new Thread(ThreadBody);
                this.Thread.IsBackground = true;
                this.Thread.Start();
            }

            internal void AddTask(Action Task)
            {
                this.Tasks.Enqueue(Task);
                this.MoreTasksEvent.Set();
            }

            internal void Stop()
            {
                AddTask(() => { this.Running = false; });
            }

            protected void ThreadBody()
            {
                Console.WriteLine("CustomThreadPool.ThreadBody.Start()");
                try
                {
                    this.LoopIterCount = 0;
                    while (this.Running)
                    {
                        this.MoreTasksEvent.WaitOne();
                        this.LoopIterCount++;
                        while (this.Tasks.Count > 0)
                        {
                            this.Tasks.Dequeue()();
                        }
                    }
                }
                finally
                {
                    Console.WriteLine("CustomThreadPool.ThreadBody.End()");
                }
            }
        }

        internal WorkerThread[] WorkerThreads;

        public CustomThreadPool(int NumberOfThreads)
        {
            WorkerThreads = new WorkerThread[NumberOfThreads];
            for (int n = 0; n < NumberOfThreads; n++)
            {
                WorkerThreads[n] = new WorkerThread();
            }
        }

        public long GetLoopIterCount(int ThreadAffinity)
        {
            return this.WorkerThreads[ThreadAffinity % WorkerThreads.Length].LoopIterCount;
        }

        public void AddTask(int ThreadAffinity, Action Task)
        {
            this.WorkerThreads[ThreadAffinity % WorkerThreads.Length].AddTask(Task);
        }

        public void Stop()
        {
            foreach (var WorkerThread in WorkerThreads)
            {
                WorkerThread.Stop();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace CSharpUtils.Threading
{
    public class CoroutinePool : IDisposable
    {
        internal Coroutine CurrentCoroutine;
        internal List<Coroutine> Coroutines = new List<Coroutine>();
        internal AutoResetEvent CallerContinueEvent = new AutoResetEvent(false);
        internal Thread CallerThread;

        public Coroutine CreateCoroutine(String Name, Action Action)
        {
            return new Coroutine(Name, this, Action);
        }


        public void YieldInPool()
        {
            this.CurrentCoroutine.YieldInPool();
        }

        public void Dispose()
        {
            foreach (var Coroutine in Coroutines.ToArray()) Coroutine.Dispose();
        }
    }

    public sealed class Coroutine : IDisposable
    {
        internal CoroutinePool Pool;
        internal AutoResetEvent CoroutineContinueEvent = new AutoResetEvent(false);
        internal Thread Thread;
        private string _Name;

        public string Name
        {
            set { _Name = value; }
            get { return _Name; }
        }

        Exception RethrowException = null;


        private void CoroutineContinueEvent_WaitOne()
        {
            CoroutineContinueEvent.WaitOne();

            if (!IsAlive) throw (new InterruptException());
        }


        private void PoolCallerContinueEvent_WaitOne()
        {
            Pool.CallerContinueEvent.WaitOne();
        }

        private sealed class InterruptException : Exception
        {
        }

        bool MustStart = false;

        internal Coroutine(String Name, CoroutinePool Pool, Action Action)
        {
            this.Pool = Pool;
            IsAlive = true;
            Thread = new Thread(() =>
            {
                Console.WriteLine("Coroutine.Start()");
                try
                {
                    CoroutineContinueEvent_WaitOne();
                    Action();
                }
                catch (InterruptException)
                {
                }
                catch (Exception Exception)
                {
                    RethrowException = Exception;
                }
                finally
                {
                    Console.WriteLine("Coroutine.End()");
                    IsAlive = false;
                    Pool.CallerContinueEvent.Set();
                }
            })
            {
                CurrentCulture = new CultureInfo("en-US"),
                IsBackground = true,
            };
            this.Name = Name;
            this.MustStart = true;
        }


        public void ExecuteStep()
        {
            if (this.MustStart)
            {
                this.MustStart = false;
                Thread.Name = "Coroutine-" + Name;
                Thread.Start();
            }
            //Debug.WriteLine("ExecuteStep");
            if (IsAlive)
            {
                Pool.CurrentCoroutine = this;
                Pool.CallerThread = Thread.CurrentThread;

                Pool.CallerContinueEvent.Reset();
                CoroutineContinueEvent.Set();
                PoolCallerContinueEvent_WaitOne();
            }
            if (RethrowException != null)
            {
                try
                {
                    //StackTraceUtils.PreserveStackTrace(RethrowException);
                    throw (new GreenThreadException("GreenThread Exception", RethrowException));
                    //throw (RethrowException);
                }
                finally
                {
                    RethrowException = null;
                }
            }
        }


        public void YieldInPool()
        {
            //Debug.WriteLine("YieldInPool");
#if false
			if (Pool == null)
			{
				throw(new Exception("Pool can't be null"));
			}
			if (Pool.CurrentCoroutine == null)
			{
				throw (new Exception("Pool.CurrentCoroutine can't be null"));
			}
			if (Pool.CallerContinueEvent == null)
			{
				throw (new Exception("Pool.CallerContinueEvent can't be null"));
			}
			if (Pool.CurrentCoroutine.CoroutineContinueEvent == null)
			{
				throw (new Exception("Pool.CurrentCoroutine.CoroutineContinueEvent can't be null"));
			}
#endif

            if (Pool.CurrentCoroutine == null)
            {
                throw (new InvalidOperationException("Can't call YieldInPool outside the ExecuteStep"));
            }

            if (Pool.CurrentCoroutine != this)
            {
                Debug.WriteLine("Pool.CurrentCoroutine != this");
            }

            CoroutineContinueEvent.Reset();
            Pool.CallerContinueEvent.Set();
            //Pool.CurrentCoroutine.CoroutineContinueEvent_WaitOne();
            CoroutineContinueEvent_WaitOne();
        }

        public bool IsAlive { get; private set; }

        public bool IsCurrentlyActive
        {
            get { return Pool.CurrentCoroutine == this; }
        }

        public void Dispose()
        {
            IsAlive = false;
            CoroutineContinueEvent.Set();
            Pool.Coroutines.Remove(this);
        }
    }
}
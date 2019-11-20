using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace CSharpUtils.Threading
{
    /// <summary>
    /// 
    /// </summary>
    public class CoroutinePool : IDisposable
    {
        internal Coroutine CurrentCoroutine;
        internal List<Coroutine> Coroutines = new List<Coroutine>();
        internal AutoResetEvent CallerContinueEvent = new AutoResetEvent(false);
        internal Thread CallerThread;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public Coroutine CreateCoroutine(string name, Action action)
        {
            return new Coroutine(name, this, action);
        }


        /// <summary>
        /// 
        /// </summary>
        public void YieldInPool()
        {
            CurrentCoroutine.YieldInPool();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            foreach (var coroutine in Coroutines.ToArray()) coroutine.Dispose();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class Coroutine : IDisposable
    {
        internal CoroutinePool Pool;
        internal AutoResetEvent CoroutineContinueEvent = new AutoResetEvent(false);
        internal Thread Thread;

        /// <summary>
        /// 
        /// </summary>
        public string Name { set; get; }

        Exception _rethrowException;


        private void CoroutineContinueEvent_WaitOne()
        {
            CoroutineContinueEvent.WaitOne();

            if (!IsAlive) throw new InterruptException();
        }


        private void PoolCallerContinueEvent_WaitOne()
        {
            Pool.CallerContinueEvent.WaitOne();
        }

        private sealed class InterruptException : Exception
        {
        }

        bool _mustStart;

        internal Coroutine(string name, CoroutinePool pool, Action action)
        {
            Pool = pool;
            IsAlive = true;
            Thread = new Thread(() =>
            {
                Console.WriteLine("Coroutine.Start()");
                try
                {
                    CoroutineContinueEvent_WaitOne();
                    action();
                }
                catch (InterruptException)
                {
                }
                catch (Exception e)
                {
                    _rethrowException = e;
                }
                finally
                {
                    Console.WriteLine("Coroutine.End()");
                    IsAlive = false;
                    pool.CallerContinueEvent.Set();
                }
            })
            {
                CurrentCulture = new CultureInfo("en-US"),
                IsBackground = true,
            };
            Name = name;
            _mustStart = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="GreenThreadException"></exception>
        public void ExecuteStep()
        {
            if (_mustStart)
            {
                _mustStart = false;
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

            if (_rethrowException != null)
            {
                try
                {
                    //StackTraceUtils.PreserveStackTrace(RethrowException);
                    throw new GreenThreadException("GreenThread Exception", _rethrowException);
                    //throw (RethrowException);
                }
                finally
                {
                    _rethrowException = null;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
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
                throw new InvalidOperationException("Can't call YieldInPool outside the ExecuteStep");
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

        /// <summary>
        /// 
        /// </summary>
        public bool IsAlive { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsCurrentlyActive => Pool.CurrentCoroutine == this;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            IsAlive = false;
            CoroutineContinueEvent.Set();
            Pool.Coroutines.Remove(this);
        }
    }
}
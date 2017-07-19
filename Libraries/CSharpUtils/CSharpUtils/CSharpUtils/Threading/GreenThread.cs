using System;
using System.Threading;

namespace CSharpUtils.Threading
{
#if false
	public class GreenThread : IDisposable
	{
		public class StopException : Exception
		{
		}

		protected Action Action;

		protected Thread ParentThread;
		protected Thread CurrentThread;
		protected ManualResetEvent ParentEvent;
		protected ManualResetEvent ThisEvent;
		protected static ThreadLocal<GreenThread> ThisGreenThreadList = new ThreadLocal<GreenThread>();
		public static int GreenThreadLastId = 0;

		public static Thread MonitorThread;

		private Exception RethrowException;

		public bool Running { get; protected set; }
		protected bool Kill;

		public GreenThread()
		{
		}

		~GreenThread()
		{
		}

		void ThisSemaphoreWaitOrParentThreadStopped()
		{
			while (true)
			{
				// If the parent thread have been stopped. We should not wait any longer.
				if (Kill || !ParentThread.IsAlive)
				{
					break;
				}

				if (ThisEvent.WaitOne(20))
				{
					// Signaled.
					break;
				}
			}

			if (Kill || !ParentThread.IsAlive)
			{
				//throw(new StopException());
				Thread.CurrentThread.Abort();
				//throw (new StopException());
			}
		}

		public void InitAndStartStopped(Action Action)
		{
			this.Action = Action;
			this.ParentThread = Thread.CurrentThread;

			ParentEvent = new ManualResetEvent(false);
			ThisEvent = new ManualResetEvent(false);

			var This = this;

			this.CurrentThread = new Thread(() =>
			{
				ThisGreenThreadList.Value = This;
				ThisSemaphoreWaitOrParentThreadStopped();
				try
				{
					Running = true;
					Action();
				}
				catch (StopException)
				{
				}
				catch (Exception Exception)
				{
					RethrowException = Exception;
				}
				finally
				{
					Running = false;
					try
					{
						ParentEvent.Set();
					}
					catch
					{
					}
				}

				//Console.WriteLine("GreenThread.Running: {0}", Running);
			});

			this.CurrentThread.Name = "GreenThread-" + GreenThreadLastId++;

			this.CurrentThread.Start();
		}

		/// <summary>
		/// Called from the caller thread.
		/// This will give the control to the green thread.
		/// </summary>
		public void SwitchTo()
		{
			ParentThread = Thread.CurrentThread;
			ParentEvent.Reset();
			ThisEvent.Set();
			if (Kill) Thread.CurrentThread.Abort();
			//ThisSemaphoreWaitOrParentThreadStopped();
			ParentEvent.WaitOne();
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

		/// <summary>
		/// Called from the green thread.
		/// This will return the control to the caller thread.
		/// </summary>
		public static void Yield()
		{
			if (ThisGreenThreadList.IsValueCreated)
			{
				var GreenThread = ThisGreenThreadList.Value;
				if (GreenThread.Running)
				{
					try
					{
						GreenThread.Running = false;

						GreenThread.ThisEvent.Reset();
						GreenThread.ParentEvent.Set();
						GreenThread.ThisSemaphoreWaitOrParentThreadStopped();
					}
					finally
					{
						GreenThread.Running = true;
					}
				}
				else
				{
					throw (new InvalidOperationException("GreenThread has finalized"));
				}
			}
		}

		public static void StopAll()
		{
			throw (new NotImplementedException());
		}

		public void Stop()
		{
			Kill = true;
			ThisEvent.Set();
			//CurrentThread.Abort();
		}

		public void Dispose()
		{
			Stop();
			//Stop();
		}

		public string Name
		{
			get
			{
				return CurrentThread.Name;
			}
			set
			{
				//CurrentThread.Name = Name;
			}
		}
	}
#else
    /// <summary>
    /// 
    /// </summary>
    public class GreenThread : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public class StopException : Exception
        {
        }

        protected Action Action;

        protected Thread ParentThread;
        protected Thread CurrentThread;
        protected Semaphore ParentSemaphore;
        protected Semaphore ThisSemaphore;
        protected static ThreadLocal<GreenThread> ThisGreenThreadList = new ThreadLocal<GreenThread>();

        /// <summary>
        /// 
        /// </summary>
        public static int GreenThreadLastId;

        /// <summary>
        /// 
        /// </summary>
        public static Thread MonitorThread;

        private Exception _rethrowException;

        /// <summary>
        /// 
        /// </summary>
        public bool Running { get; protected set; }

        protected bool Kill;

        void ThisSemaphoreWaitOrParentThreadStopped()
        {
            while (true)
            {
                // If the parent thread have been stopped. We should not wait any longer.
                if (Kill || !ParentThread.IsAlive)
                {
                    break;
                }

                if (ThisSemaphore.WaitOne(20))
                {
                    // Signaled.
                    break;
                }
            }

            if (Kill || !ParentThread.IsAlive)
            {
                //throw(new StopException());
                Thread.CurrentThread.Abort();
                //throw (new StopException());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void InitAndStartStopped(Action action)
        {
            Action = action;
            ParentThread = Thread.CurrentThread;

            ParentSemaphore = new Semaphore(1, 1);
            ParentSemaphore.WaitOne();

            ThisSemaphore = new Semaphore(1, 1);
            ThisSemaphore.WaitOne();

            var This = this;

            CurrentThread = new Thread(() =>
            {
                Console.WriteLine("GreenThread.Start()");
                ThisGreenThreadList.Value = This;
                ThisSemaphoreWaitOrParentThreadStopped();
                try
                {
                    Running = true;
                    action();
                }
                catch (StopException)
                {
                }
#if !DO_NOT_PROPAGATE_EXCEPTIONS
                catch (Exception e)
                {
	                Console.Error.WriteLine(e);
                    _rethrowException = e;
                }
#endif
                finally
                {
                    Running = false;
                    try
                    {
                        ParentSemaphore.Release();
                    }
                    catch
                    {
                        // ignored
                    }
                    Console.WriteLine("GreenThread.End()");
                }

                //Console.WriteLine("GreenThread.Running: {0}", Running);
            });

            CurrentThread.Name = "GreenThread-" + GreenThreadLastId++;

            CurrentThread.Start();
        }

        /// <summary>
        /// Called from the caller thread.
        /// This will give the control to the green thread.
        /// </summary>
        public void SwitchTo()
        {
            ParentThread = Thread.CurrentThread;
            ThisSemaphore.Release();
            if (Kill)
            {
                Thread.CurrentThread.Abort();
            }
            //ThisSemaphoreWaitOrParentThreadStopped();
            ParentSemaphore.WaitOne();
            if (_rethrowException != null)
            {
                try
                {
                    //StackTraceUtils.PreserveStackTrace(RethrowException);
                    throw (new GreenThreadException("GreenThread Exception", _rethrowException));
                    //throw (RethrowException);
                }
                finally
                {
                    _rethrowException = null;
                }
            }
        }

        /// <summary>
        /// Called from the green thread.
        /// This will return the control to the caller thread.
        /// </summary>
        public static void Yield()
        {
            if (ThisGreenThreadList.IsValueCreated)
            {
                var greenThread = ThisGreenThreadList.Value;
                if (greenThread.Running)
                {
                    try
                    {
                        greenThread.Running = false;

                        greenThread.ParentSemaphore.Release();
                        greenThread.ThisSemaphoreWaitOrParentThreadStopped();
                    }
                    finally
                    {
                        greenThread.Running = true;
                    }
                }
                else
                {
                    throw(new InvalidOperationException("GreenThread has finalized"));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static void StopAll()
        {
            throw(new NotImplementedException());
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            Kill = true;
            ThisSemaphore.Release();
            //CurrentThread.Abort();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Stop();
            //Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
	        set
	        {
		        //CurrentThread.Name = value;
	        }
	        get => CurrentThread.Name;
	    }
    }
#endif
}
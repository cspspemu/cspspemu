using System;
using System.Globalization;
using System.Threading;
using CSharpUtils.Threading;
using CSPspEmu.Core;
using CSharpUtils;

namespace CSPspEmu.Runner.Components
{
	public abstract class ComponentThread : IRunnableComponent
	{
		static Logger Logger = Logger.GetLogger("ComponentThread");

		protected AutoResetEvent RunningUpdatedEvent = new AutoResetEvent(false);
		public bool Running = true;

		protected Thread ComponentThreadThread;
		protected AutoResetEvent StopCompleteEvent = new AutoResetEvent(false);
		protected AutoResetEvent PauseEvent = new AutoResetEvent(false);
		protected AutoResetEvent ResumeEvent = new AutoResetEvent(false);

		public readonly TaskQueue ThreadTaskQueue = new TaskQueue();
		protected abstract String ThreadName { get; }

		protected ComponentThread()
		{
		}

		public void StartSynchronized()
		{
			Logger.Notice("Component {0} StartSynchronized!", this);
			var ElapsedTime = Logger.Measure(() =>
			{
				ComponentThreadThread = new Thread(() =>
				{
					ComponentThreadThread.Name = this.ThreadName;
					Thread.CurrentThread.CurrentCulture = new CultureInfo(GlobalConfig.ThreadCultureName);
					try
					{
						Main();
					}
					finally
					{
						Running = false;
						RunningUpdatedEvent.Set();
						StopCompleteEvent.Set();
						Logger.Notice("Component {0} Stopped!", this);
					}
				});
				ComponentThreadThread.IsBackground = true;
				ComponentThreadThread.Start();
				ThreadTaskQueue.EnqueueAndWaitCompleted(() =>
				{
				});
			});
			Logger.Notice("Component {0} Started! StartedTime({1})", this, ElapsedTime.TotalSeconds);
		}

		public void StopSynchronized()
		{
			Logger.Notice("Component {0} StopSynchronized...", this);
			var ElapsedTime = Logger.Measure(() =>
			{
				if (Running)
				{
					StopCompleteEvent.Reset();
					{
						Running = false;
						RunningUpdatedEvent.Set();
					}
					if (!StopCompleteEvent.WaitOne(1000))
					{
						Logger.Error("Error stopping {0}", this);
						ComponentThreadThread.Abort();
					}
				}
			});
			Logger.Notice("Stopped! {0}", ElapsedTime);
		}

		public void PauseSynchronized()
		{
			Logger.Notice("Component {0} PauseSynchronized!", this);

			//Console.WriteLine("[1]");

			ThreadTaskQueue.EnqueueAndWaitStarted(() =>
			{
				//int MaxCounts = 200;
				//Console.WriteLine("[2]");
				while (!PauseEvent.WaitOne(TimeSpan.FromMilliseconds(10)))
				{
					//Console.WriteLine("[3]");
					if (!Running) break;
					//if (MaxCounts-- < 0)
					//{
					//	Console.Error.WriteLine("Infinite loop detected!");
					//	break;
					//}
				}
			}, TimeSpan.FromSeconds(2), () =>
			{
				Console.WriteLine("Timed Out!");
			});
		}

		public void ResumeSynchronized()
		{
			Logger.Notice("Component {0} ResumeSynchronized!", this);

			PauseEvent.Set();
		}

		protected abstract void Main();
	}
}

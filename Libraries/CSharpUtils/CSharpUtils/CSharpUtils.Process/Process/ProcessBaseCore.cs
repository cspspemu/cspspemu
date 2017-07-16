using System;
using System.Diagnostics;
using CSharpUtils.Process.Impl;

namespace CSharpUtils.Process
{
	public abstract class ProcessBaseCore
	{
		IProcessBaseImpl Impl;
		private bool Removed;

		/*static {
			OperatingSystem os = Environment.OSVersion;
			PlatformID pid = os.Platform;
		}*/

		static Func<IProcessBaseImpl> ProcessBaseImplFactory;

		static ProcessBaseCore()
		{
			OperatingSystem os = Environment.OSVersion;
			PlatformID pid = os.Platform;
			//Console.WriteLine(pid);
	
			// Safe Implementation.
			ProcessBaseImplFactory = () => { return new ProcessBaseImplThreads(); };

			switch (pid)
			{
				/*
				case PlatformID.Unix:
				case PlatformID.MacOSX:
					//GetType
					ProcessBaseImplFactory = () => { return new ProcessBaseImplUcontext(); };
				break;
				case PlatformID.Xbox:
				break;
				*/
				case PlatformID.Win32Windows:
				case PlatformID.Win32S:
				case PlatformID.Win32NT:
					//ProcessBaseImplFactory = () => { return new ProcessBaseImplFibers(); };
				break;
				default:
				break;
			}
		}

		public ProcessBaseCore()
		{
			//Console.WriteLine(this + " : " + parent);

			Impl = ProcessBaseImplFactory();
			State = State.Started;

			Impl.Init(() =>
			{
				Action method = Main;

			restart:
				try
				{
					State = State.Running;
					method();
					State = State.Ended;
					Yield();
				}
				catch (ChangeExecutionMethodException ChangeExecutionMethodException)
				{
					method = ChangeExecutionMethodException.method;
					goto restart;
				}
			});
		}

		~ProcessBaseCore()
		{
			Console.WriteLine("~ProcessBaseCore");
		}

		sealed class ChangeExecutionMethodException : Exception
		{
			internal Action method;

			public ChangeExecutionMethodException(Action method)
			{
				this.method = method; ;
			}
		}

		public void SetAction(Action method)
		{
			throw(new ChangeExecutionMethodException(method));
		}

		public State State { private set; get; }

		public void SwitchTo()
		{
			if (State != State.Ended)
			{
				Impl.SwitchTo();
			}
		}

		protected virtual void _Remove()
		{
			if (!Removed)
			{
				Removed = true;
				State = State.Ended;
				Impl.Remove();
			}
		}

		protected void Animate(TimeSpan Time, Action<float> StepAction)
		{
			Stopwatch Stopwatch = new Stopwatch();
			//Stopwatch.
			//Stopwatch.StartNew();
			Stopwatch.Start();
			//var Start = DateTime.Now;
			//var End = Start + Time;

			StepAction(0);
			while (true)
			{
				var ElapsedPercent = (float)((double)Stopwatch.ElapsedMilliseconds / (double)Time.TotalMilliseconds);
				//Console.WriteLine(Stopwatch.Elapsed.TotalMilliseconds);
				if (ElapsedPercent >= 1)
				{
					StepAction(1);
					break;
				}
				else
				{
					StepAction(ElapsedPercent);
				}
				Yield();
			}
		}

		protected void YieldWhile(Func<bool> Predicate)
		{
			while (Predicate()) Yield();
		}

		protected void Yield(int count = 1)
		{
			for (int n = 0; n < count; n++)
			{
				Impl.Yield();
			}
		}

		protected abstract void Main();
	}
}

using System;
using System.Threading;

namespace CSharpUtils.Process.Impl
{
	public class ProcessBaseImplThreads : IProcessBaseImpl
	{
		public static Semaphore SemaphoreGlobal;
		public static Thread MainThread;
		Semaphore Semaphore;
		Thread CurrentThread;
		/*Object Parent;

		public ProcessBaseImplThreads(Object Parent)
		{
			this.Parent = Parent;
		}*/

		~ProcessBaseImplThreads()
		{
			Console.WriteLine("~ProcessBaseImplThreads");
			Remove();
		}

		public static void Shutdown()
		{

		}

		public void Init(RunDelegate Delegate)
		{
			//Console.WriteLine("Init(" + Parent + ")");

			if (MainThread == null)
			{
				SemaphoreGlobal = new Semaphore(1, 1);
				SemaphoreGlobal.WaitOne();

				MainThread = Thread.CurrentThread;
				//mainMutex.WaitOne();
			}

			Semaphore = new Semaphore(1, 1);
			Semaphore.WaitOne();
			CurrentThread = new Thread(delegate()
			{
				Semaphore.WaitOne();
				//currentThread.Interrupt();
				Delegate();
			});

			CurrentThread.Start();

			//Mutex.WaitOne();
		}

		public void SwitchTo()
		{
			//Console.WriteLine("SwitchTo(" + Parent + ")");
			Semaphore.Release();
			SemaphoreGlobal.WaitOne();
		}

		public void Remove()
		{
			//Console.WriteLine("Remove(" + Parent + ")");
			CurrentThread.Abort();
		}

		public void Yield()
		{
			//Console.WriteLine("Yield(" + Parent + ")");
			SemaphoreGlobal.Release();
			while (!Semaphore.WaitOne(TimeSpan.FromMilliseconds(50)))
			{
				if (!MainThread.IsAlive)
				{
					Thread.CurrentThread.Abort();
				}
			}
		}
	}
}

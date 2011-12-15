using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu;
using System.Threading;
using System.Diagnostics;
using CSPspEmu.Core;
using CSPspEmu.Core.Rtc;
using CSharpUtils.Threading;

namespace CSPspEmu.Hle.Managers
{
	public class HleEmulatorSpecialAddresses
	{
		public const uint CODE_PTR_EXIT_THREAD = 0x08000010;
		public const uint CODE_PTR_FINALIZE_CALLBACK = 0x08000020;
	}

	public class HleThreadManager : PspEmulatorComponent, IDisposable
	{

		protected CpuProcessor Processor;
		public List<HleThread> Threads = new List<HleThread>();
		protected int LastId = 1;
		public HleThread Current;
		private HleCallbackManager HleCallbackManager;
		private HleInterruptManager HleInterruptManager;

		public override void InitializeComponent()
		{
			this.Processor = PspEmulatorContext.GetInstance<CpuProcessor>();
			this.HleCallbackManager = PspEmulatorContext.GetInstance<HleCallbackManager>();
			this.HleInterruptManager = PspEmulatorContext.GetInstance<HleInterruptManager>();
		}

		public HleThread GetThreadById(int Id)
		{
			//Debug.WriteLine(Threads.Count);
			return Threads.FirstOrDefault((Thread) => Thread.Id == Id);
		}

		public HleThread Create()
		{
			var HlePspThread = new HleThread(new CpuThreadState(Processor));
			HlePspThread.Id = LastId++;
			HlePspThread.Name = "Thread-" + HlePspThread.Id;
			HlePspThread.CurrentStatus = HleThread.Status.Stopped;
			Threads.Add(HlePspThread);
			return HlePspThread;
		}

		private HleThread CalculateNext()
		{
			HleThread MinThread = null;
			//Console.Write("{0},", Threads.Count);
			foreach (var Thread in Threads)
			{
				if ((Thread.CurrentStatus == HleThread.Status.Ready) || Thread.IsWaitingAndHandlingCallbacks)
				{
					if (MinThread == null || Thread.PriorityValue < MinThread.PriorityValue)
					{
						MinThread = Thread;
					}
				}
			}
			return MinThread;
		}

		public IEnumerable<HleThread> WaitingThreads
		{
			get
			{
				return Threads.Where(Thread => Thread.CurrentStatus == HleThread.Status.Waiting);
			}
		}

		bool MustReschedule = false;

		public void Reschedule()
		{
			MustReschedule = true;
		}

		public void ScheduleNext(HleThread ThreadToSchedule)
		{
			ThreadToSchedule.PriorityValue = Threads.Min(Thread => Thread.PriorityValue) - 1;
			Reschedule();
			//Console.WriteLine("!ScheduleNext: ");
		}

		public void StepNext()
		{
			MustReschedule = false;

			//HleInterruptManager.EnableDisable(() => {
			//});

			if (Threads.Count > 0)
			{
				HleInterruptManager.ExecuteQueued(Threads.First().CpuThreadState);
			}

			// Select the thread with the lowest PriorityValue
			var NextThread = CalculateNext();
			//Console.WriteLine("NextThread: {0} : {1}", NextThread.Id, NextThread.PriorityValue);

			// No thread found.
			if (NextThread == null)
			{
				if (Processor.PspConfig.VerticalSynchronization)
				{
					Thread.Sleep(1);
				}
				return;
			}

			// Run that thread
			Current = NextThread;
			{
				// Waiting, but listeing to callbacks.
				if (Current.IsWaitingAndHandlingCallbacks)
				{
					HleCallbackManager.ExecuteQueued(Current.CpuThreadState, MustReschedule);
				}
				// Executing normally.
				else
				{
					Current.CurrentStatus = HleThread.Status.Running;
					try
					{
						if (Processor.PspConfig.DebugThreadSwitching)
						{
							ConsoleUtils.SaveRestoreConsoleState(() =>
							{
								Console.ForegroundColor = ConsoleColor.Yellow;
								Console.WriteLine("Execute: {0} : PC: 0x{1:X}", Current, Current.CpuThreadState.PC);
							});
						}
						Current.Step();
					}
					finally
					{
						if (Current.CurrentStatus == HleThread.Status.Running)
						{
							Current.CurrentStatus = HleThread.Status.Ready;
						}
					}
				}
			}
			Current = null;

			// Decrement all threads by that PriorityValue.
			int DecrementValue = NextThread.PriorityValue;
			foreach (var Thread in Threads)
			{
				//Console.WriteLine(Thread.PriorityValue);
				Thread.PriorityValue -= DecrementValue;
			}

			// Increment.
			NextThread.PriorityValue += NextThread.Info.PriorityCurrent + 1;
		}

		public void ExitThread(HleThread HlePspThread)
		{
			Threads.Remove(HlePspThread);
		}

		public void Dispose()
		{
		}

		public unsafe void DeleteThread(HleThread Thread)
		{
			ExitThread(Thread);
		}
	}
}

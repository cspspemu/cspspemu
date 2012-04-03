//#define DISABLE_CALLBACKS

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

	public class HleThreadManager : PspEmulatorComponent
	{
		internal CpuProcessor Processor;
		public List<HleThread> Threads = new List<HleThread>();
		protected int LastId = 1;
		public HleThread Current;
		private HleCallbackManager HleCallbackManager;
		private HleInterruptManager HleInterruptManager;

		public HleThread CurrentOrAny
		{
			get
			{
				if (Current != null) return Current;
				return Threads.First();
			}
		}

		public override void InitializeComponent()
		{
			this.HleState = PspEmulatorContext.GetInstance<HleState>();
			this.Processor = PspEmulatorContext.GetInstance<CpuProcessor>();
			this.Processor.DebugCurrentThreadEvent += DebugCurrentThread;
			this.HleCallbackManager = PspEmulatorContext.GetInstance<HleCallbackManager>();
			this.HleInterruptManager = PspEmulatorContext.GetInstance<HleInterruptManager>();
		}

		public HleThread GetThreadById(int Id, bool AllowSelf = true)
		{
			//Debug.WriteLine(Threads.Count);
			if (AllowSelf && (Id == 0)) return Current;
			return Threads.FirstOrDefault((Thread) => Thread.Id == Id);
		}

		public HleThread Create()
		{
			var HlePspThread = new HleThread(this, new CpuThreadState(Processor));
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
		internal HleState HleState;

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

#if !DISABLE_CALLBACKS
			if (Threads.Count > 0)
			{
				HleInterruptManager.ExecuteQueued(Threads.First().CpuThreadState);
			}
#endif

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
				if (Threads.Count == 0)
				{
					Thread.Sleep(5);
				}
				return;
			}

			// Run that thread
			Current = NextThread;
			{
				// Waiting, but listeing to callbacks.
				if (Current.IsWaitingAndHandlingCallbacks)
				{
					/*
					if (Processor.PspConfig.VerticalSynchronization)
					{
						Thread.Sleep(1);
					}
					*/
#if !DISABLE_CALLBACKS
					HleCallbackManager.ExecuteQueued(Current.CpuThreadState, MustReschedule);
#endif
				}
				// Executing normally.
				else
				{
					//throw (new Exception("aaaaaaaaaaaa"));
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

		public unsafe void DeleteThread(HleThread Thread)
		{
			Thread.Stack.DeallocateFromParent();
			ExitThread(Thread);
		}

		public void DebugCurrentThread()
		{
			Console.Error.WriteLine("HleThreadManager.CpuProcessor.DebugCurrentThreadEvent:");
			Console.Error.WriteLine(Current);
		}
	}
}

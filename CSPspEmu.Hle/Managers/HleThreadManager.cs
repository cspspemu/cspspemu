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

namespace CSPspEmu.Hle.Managers
{
	public class HleThreadManager : PspEmulatorComponent
	{
		protected CpuProcessor Processor;
		protected List<HleThread> Threads = new List<HleThread>();
		protected int LastId = 1;
		public HleThread Current;
		protected PspRtc HlePspRtc;
		private HleThread _Next;

		public HleThreadManager(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			this.HlePspRtc = PspEmulatorContext.GetInstance<PspRtc>();
			this.Processor = PspEmulatorContext.GetInstance<CpuProcessor>();
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
			HlePspThread.CurrentStatus = Hle.HleThread.Status.Stopped;
			Threads.Add(HlePspThread);
			return HlePspThread;
		}

		public HleThread Next
		{
			get
			{
				if (_Next == null)
				{
					_Next = CalculateNext();
				}
				return _Next;
			}
		}

		private HleThread CalculateNext()
		{
			HleThread MinThread = null;
			foreach (var Thread in Threads)
			{
				if (Thread.CurrentStatus == HleThread.Status.Ready)
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

		public void StepNext()
		{
			// Select the thread with the lowest PriorityValue
			var NextThread = Next;
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
				Current.CurrentStatus = HleThread.Status.Running;
				try
				{
					if (Processor.PspConfig.DebugThreadSwitching)
					{
						ConsoleUtils.SaveRestoreConsoleState(() =>
						{
							Console.ForegroundColor = ConsoleColor.Yellow;
							Console.WriteLine("Execute: {0} : PC: 0x{1:X}", NextThread, NextThread.CpuThreadState.PC);
						});
					}
					NextThread.Step();
				}
				finally
				{
					if (Current.CurrentStatus == HleThread.Status.Running)
					{
						Current.CurrentStatus = HleThread.Status.Ready;
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
			NextThread.PriorityValue += DecrementValue + NextThread.Info.PriorityCurrent + 1;

			// Invalidate next.
			_Next = null;
		}

		public void Exit(HleThread HlePspThread)
		{
			Threads.Remove(HlePspThread);
			if (HlePspThread == Current)
			{
				HlePspThread.CpuThreadState.Yield();
			}
		}
	}
}

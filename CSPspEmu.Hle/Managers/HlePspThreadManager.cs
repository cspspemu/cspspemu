using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu;
using System.Threading;

namespace CSPspEmu.Hle.Managers
{
	public class HlePspThreadManager
	{
		protected Processor Processor;
		protected List<HlePspThread> Threads = new List<HlePspThread>();
		protected int LastId = 1;
		public HlePspThread Current;
		protected PspRtc HlePspRtc;

		public HlePspThreadManager(Processor Processor, PspRtc HlePspRtc)
		{
			this.HlePspRtc = HlePspRtc;
			this.Processor = Processor;
		}

		public HlePspThread GetThreadById(int Id)
		{
			return Threads.First((Thread) => Thread.Id == Id);
		}

		public HlePspThread Create()
		{
			var HlePspThread = new HlePspThread(new CpuThreadState(Processor));
			HlePspThread.Id = LastId++;
			HlePspThread.CurrentStatus = Hle.HlePspThread.Status.Stopped;
			Threads.Add(HlePspThread);
			return HlePspThread;
		}

		private HlePspThread _Next;
		public HlePspThread Next
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

		private HlePspThread CalculateNext()
		{
			HlePspThread MinThread = null;
			foreach (var Thread in Threads)
			{
				if (Thread.CurrentStatus == HlePspThread.Status.Ready)
				{
					if (MinThread == null || Thread.PriorityValue < MinThread.PriorityValue)
					{
						MinThread = Thread;
					}
				}
			}
			return MinThread;
		}

		public IEnumerable<HlePspThread> WaitingThreads
		{
			get
			{
				return Threads.Where(Thread => Thread.CurrentStatus == HlePspThread.Status.Waiting);
			}
		}

		private void AwakeOnTimeThreads()
		{
			foreach (var Thread in WaitingThreads)
			{
				if (Thread.CurrentWaitType == HlePspThread.WaitType.Timer)
				{
					if (HlePspRtc.CurrentDateTime >= Thread.AwakeOnTime)
					{
						Thread.CurrentStatus = HlePspThread.Status.Ready;
					}
				}
			}
		}

		public void StepNext()
		{
			AwakeOnTimeThreads();

			// Select the thread with the lowest PriorityValue
			var NextThread = Next;
			//Console.WriteLine("NextThread: {0} : {1}", NextThread.Id, NextThread.PriorityValue);

			// No thread found.
			if (NextThread == null)
			{
				Thread.Sleep(1);
				return;
			}

			// Run that thread
			Current = NextThread;
			{
				Current.CurrentStatus = HlePspThread.Status.Running;
				try
				{
					NextThread.Step();
				}
				finally
				{
					if (Current.CurrentStatus == HlePspThread.Status.Running)
					{
						Current.CurrentStatus = HlePspThread.Status.Ready;
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
			NextThread.PriorityValue += DecrementValue + NextThread.Priority + 1;

			// Invalidate next.
			_Next = null;
		}

		public void Exit(HlePspThread HlePspThread)
		{
			Threads.Remove(HlePspThread);
			if (HlePspThread == Current)
			{
				HlePspThread.CpuThreadState.Yield();
			}
		}
	}
}

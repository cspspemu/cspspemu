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
	public class HleThreadManager
	{
		protected Processor Processor;
		protected List<HleThread> Threads = new List<HleThread>();
		protected int LastId = 1;
		public HleThread Current;
		protected PspRtc HlePspRtc;

		public HleThreadManager(Processor Processor, PspRtc HlePspRtc)
		{
			this.HlePspRtc = HlePspRtc;
			this.Processor = Processor;
		}

		public HleThread GetThreadById(int Id)
		{
			return Threads.First((Thread) => Thread.Id == Id);
		}

		public HleThread Create()
		{
			var HlePspThread = new HleThread(new CpuThreadState(Processor));
			HlePspThread.Id = LastId++;
			HlePspThread.CurrentStatus = Hle.HleThread.Status.Stopped;
			Threads.Add(HlePspThread);
			return HlePspThread;
		}

		private HleThread _Next;
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

		private void AwakeOnTimeThreads()
		{
			foreach (var Thread in WaitingThreads)
			{
				if (Thread.CurrentWaitType == HleThread.WaitType.Timer)
				{
					if (HlePspRtc.CurrentDateTime >= Thread.AwakeOnTime)
					{
						Thread.CurrentStatus = HleThread.Status.Ready;
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
				Current.CurrentStatus = HleThread.Status.Running;
				try
				{
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
			NextThread.PriorityValue += DecrementValue + NextThread.Priority + 1;

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

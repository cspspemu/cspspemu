using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle
{
	public class HlePspThreadManager
	{
		protected Processor Processor;
		protected List<HlePspThread> Threads = new List<HlePspThread>();
		protected int LastId = 0;

		public HlePspThreadManager(Processor Processor)
		{
			this.Processor = Processor;
		}

		public HlePspThread Create()
		{
			var HlePspThread = new HlePspThread(new CpuThreadState(Processor));
			HlePspThread.Id = LastId++;
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
			HlePspThread MinThread = Threads[0];
			foreach (var Thread in Threads)
			{
				if (Thread.PriorityValue < MinThread.PriorityValue)
				{
					MinThread = Thread;
				}
			}
			return MinThread;
		}

		public void StepNext()
		{
			// Select the thread with the lowest PriorityValue
			var NextThread = Next;
			//Console.WriteLine("NextThread: {0} : {1}", NextThread.Id, NextThread.PriorityValue);

			// Run that thread
			NextThread.Step();

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
	}
}

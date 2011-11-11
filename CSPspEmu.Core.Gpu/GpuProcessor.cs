using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSPspEmu.Core.Gpu
{
	unsafe public class GpuProcessor
	{
		private PspMemory Memory;
		protected LinkedList<GpuDisplayList> DisplayListQueue;
		protected AutoResetEvent DisplayListQueueUpdated = new AutoResetEvent(false);

		public GpuProcessor(PspMemory Memory)
		{
			this.Memory = Memory;
			this.DisplayListQueue = new LinkedList<GpuDisplayList>();
		}

		public GpuDisplayList CreateDisplayList()
		{
			return new GpuDisplayList();
		}

		public void EnqueueDisplayListFirst(GpuDisplayList DisplayList)
		{
			DisplayListQueue.AddFirst(DisplayList);
			DisplayListQueueUpdated.Set();
		}

		public void EnqueueDisplayListLast(GpuDisplayList DisplayList)
		{
			DisplayListQueue.AddLast(DisplayList);
			DisplayListQueueUpdated.Set();
		}

		public void Process()
		{
			while (true)
			{
				DisplayListQueueUpdated.WaitOne();

				while (DisplayListQueue.Count > 0)
				{
					var CurrentGpuDisplayList = DisplayListQueue.First.Value;
					DisplayListQueue.RemoveFirst();
					CurrentGpuDisplayList.Process();
				}
			}
		}
	}
}

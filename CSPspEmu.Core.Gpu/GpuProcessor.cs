using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSPspEmu.Core.Gpu
{
	unsafe public class GpuProcessor
	{
		/// <summary>
		/// 
		/// </summary>
		public enum SyncTypeEnum : uint
		{
			/// <summary>
			/// 
			/// </summary>
			ListDone = 0,

			/// <summary>
			/// 
			/// </summary>
			ListQueued = 1,

			/// <summary>
			/// 
			/// </summary>
			ListDrawingDone = 2,

			/// <summary>
			/// 
			/// </summary>
			ListStallReached = 3,

			/// <summary>
			/// 
			/// </summary>
			ListCancelDone = 4,
		}

		protected PspConfig PspConfig;
		protected PspMemory Memory;

		/// <summary>
		/// 
		/// </summary>
		volatile public LinkedList<GpuDisplayList> DisplayListQueue;

		/// <summary>
		/// 
		/// </summary>
		volatile protected AutoResetEvent DisplayListQueueUpdated = new AutoResetEvent(false);

		/// <summary>
		/// 
		/// </summary>
		volatile protected Queue<GpuDisplayList> DisplayListFreeQueue;

		/// <summary>
		/// All the supported Psp Display Lists (Available and not available).
		/// </summary>
		readonly public GpuDisplayList[] DisplayLists = new GpuDisplayList[64];

		public event Action DrawSync;

		public GpuProcessor(PspConfig PspConfig, PspMemory Memory)
		{
			this.PspConfig = PspConfig;
			this.Memory = Memory;
			this.DisplayListQueue = new LinkedList<GpuDisplayList>();
			this.DisplayListFreeQueue = new Queue<GpuDisplayList>();
			for (int n = 0; n < DisplayLists.Length; n++)
			{
				var DisplayList = new GpuDisplayList();
				DisplayList.Id = n;
				this.DisplayLists[n] = DisplayList;
				this.DisplayListFreeQueue.Enqueue(DisplayLists[n]);
			}
		}

		public GpuDisplayList DequeueFreeDisplayList()
		{
			Console.WriteLine("Count: {0}", this.DisplayListFreeQueue.Count);
			return this.DisplayListFreeQueue.Dequeue();
		}

		public void EnqueueFreeDisplayList(GpuDisplayList GpuDisplayList)
		{
			this.DisplayListFreeQueue.Enqueue(GpuDisplayList);
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
			GpuDisplayList.InstructionSwitch = GpuDisplayList.GenerateSwitch();

			while (true)
			{
				DisplayListQueueUpdated.WaitOne();

				while (DisplayListQueue.Count > 0)
				{
					GpuDisplayList CurrentGpuDisplayList;

					//Console.WriteLine("**********************************************");
					lock (this)
					{
						CurrentGpuDisplayList = DisplayListQueue.First.Value;
					}
					DisplayListQueue.RemoveFirst();
					{
						CurrentGpuDisplayList.Process();
					}
					EnqueueFreeDisplayList(CurrentGpuDisplayList);
				}

				if (DrawSync != null) DrawSync();
			}
		}
	}
}

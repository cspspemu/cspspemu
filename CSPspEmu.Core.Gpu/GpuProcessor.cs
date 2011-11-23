using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Threading.Synchronization;

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

		/// <summary>
		/// 
		/// </summary>
		public PspConfig PspConfig;

		/// <summary>
		/// 
		/// </summary>
		public PspMemory Memory;

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

		public enum StatusEnum
		{
			Drawing = 0,
			Completed = 1,
		}

		/// <summary>
		/// 
		/// </summary>
		//public event Action DrawSync;
		readonly public WaitableStateMachine<StatusEnum> Status = new WaitableStateMachine<StatusEnum>(Debug: false);

		/// <summary>
		/// 
		/// </summary>
		public IGpuImpl GpuImpl;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="PspConfig"></param>
		/// <param name="Memory"></param>
		public GpuProcessor(PspConfig PspConfig, PspMemory Memory, IGpuImpl GpuImpl)
		{
			if (sizeof(GpuStateStruct) > sizeof(uint) * 512)
			{
				throw(new InvalidProgramException());
			}

			this.PspConfig = PspConfig;
			this.Memory = Memory;
			this.GpuImpl = GpuImpl;
			this.DisplayListQueue = new LinkedList<GpuDisplayList>();
			this.DisplayListFreeQueue = new Queue<GpuDisplayList>();
			for (int n = 0; n < DisplayLists.Length; n++)
			{
				var DisplayList = new GpuDisplayList(this, n);
				this.DisplayLists[n] = DisplayList;
				this.DisplayListFreeQueue.Enqueue(DisplayLists[n]);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public GpuDisplayList DequeueFreeDisplayList()
		{
			//Console.WriteLine("Count: {0}", this.DisplayListFreeQueue.Count);
			return this.DisplayListFreeQueue.Dequeue();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GpuDisplayList"></param>
		public void EnqueueFreeDisplayList(GpuDisplayList GpuDisplayList)
		{
			this.DisplayListFreeQueue.Enqueue(GpuDisplayList);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DisplayList"></param>
		public void EnqueueDisplayListFirst(GpuDisplayList DisplayList)
		{
			DisplayListQueue.AddFirst(DisplayList);
			DisplayListQueueUpdated.Set();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DisplayList"></param>
		public void EnqueueDisplayListLast(GpuDisplayList DisplayList)
		{
			DisplayListQueue.AddLast(DisplayList);
			DisplayListQueueUpdated.Set();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Process()
		{
			GpuDisplayList.InstructionSwitch = GpuDisplayList.GenerateSwitch();

			while (true)
			{
				Status.SetValue(StatusEnum.Completed);

				DisplayListQueueUpdated.WaitOne();

				while (DisplayListQueue.Count > 0)
				{
					Status.SetValue(StatusEnum.Drawing);
					GpuDisplayList CurrentGpuDisplayList;

					//Console.WriteLine("**********************************************");
					lock (this)
					{
						CurrentGpuDisplayList = DisplayListQueue.First.Value;
					}
					DisplayListQueue.RemoveFirst();
					{
						//Console.WriteLine("YYYYYYYYYYYYYYYYYYYYYYYYYYY");
						CurrentGpuDisplayList.Process();
						//Console.WriteLine("ZZZZZZZZZZZZZZZZZZZZZZZZZZ");
					}
					EnqueueFreeDisplayList(CurrentGpuDisplayList);
				}

				//if (DrawSync != null) DrawSync();
			}
		}

		public void GeDrawSync(SyncTypeEnum SyncType, Action SyncCallback)
		{
			if (SyncType != SyncTypeEnum.ListDone) throw new NotImplementedException();
			Status.CallbackOnStateOnce(StatusEnum.Completed, () =>
			{
				//Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
				SyncCallback();
			});
		}
	}
}

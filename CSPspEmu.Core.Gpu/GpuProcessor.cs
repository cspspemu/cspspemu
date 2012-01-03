using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Threading.Synchronization;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Gpu
{
	unsafe public class GpuProcessor : PspEmulatorComponent
	{
		/*
		 *   - GU_SYNC_FINISH - 0 - Wait until the last sceGuFinish command is reached
		 *   - GU_SYNC_SIGNAL - 1 - Wait until the last (?) signal is executed
		 *   - GU_SYNC_DONE   - 2 - Wait until all commands currently in list are executed
		 *   - GU_SYNC_LIST   - 3 - Wait for the currently executed display list (GU_DIRECT)
		 *   - GU_SYNC_SEND   - 4 - Wait for the last send list
		 *   
		 *   int sceGuSync(int mode, SyncTypeEnum what)
		 *	 {
		 *		 switch (mode)
		 *		 {
		 *			 case GU_SYNC_FINISH: return sceGeDrawSync(what);
		 *			 case GU_SYNC_LIST  : return sceGeListSync(ge_list_executed[0], what);
		 *			 case GU_SYNC_SEND  : return sceGeListSync(ge_list_executed[1], what);
		 *		 	 default: case GU_SYNC_SIGNAL: case GU_SYNC_DONE: return 0;
		 *	 	 }
		 *	 }
		 */
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
		//readonly public WaitableStateMachine<StatusEnum> Status = new WaitableStateMachine<StatusEnum>(Debug: true);

		/// <summary>
		/// 
		/// </summary>
		public GpuImpl GpuImpl;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="PspConfig"></param>
		/// <param name="Memory"></param>
		public override void InitializeComponent()
		{
			if (sizeof(GpuStateStruct) > sizeof(uint) * 512)
			{
				throw (new InvalidProgramException("GpuStateStruct too big. Maybe x64? . Size: " + sizeof(GpuStateStruct)));
			}

			this.PspConfig = PspEmulatorContext.PspConfig;
			this.Memory = PspEmulatorContext.GetInstance<PspMemory>();
			this.GpuImpl = PspEmulatorContext.GetInstance<GpuImpl>();
			this.DisplayListQueue = new LinkedList<GpuDisplayList>();
			this.DisplayListFreeQueue = new Queue<GpuDisplayList>();
			for (int n = 0; n < DisplayLists.Length; n++)
			{
				var DisplayList = new GpuDisplayList(Memory, this, n);
				this.DisplayLists[n] = DisplayList;
				//this.DisplayListFreeQueue.Enqueue(DisplayLists[n]);
				EnqueueFreeDisplayList(DisplayLists[n]);
			}
		}

		protected void AddedDisplayList()
		{
			GpuImpl.AddedDisplayList();
		}

		AutoResetEvent DisplayListFreeEvent = new AutoResetEvent(false);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public GpuDisplayList DequeueFreeDisplayList()
		{
			//Console.WriteLine("DequeueFreeDisplayList: {0}", this.DisplayListFreeQueue.Count);
			//Console.WriteLine("Count: {0}", this.DisplayListFreeQueue.Count);
			/*
			while (this.DisplayListFreeQueue.Count == 0)
			{
				DisplayListFreeEvent.WaitOne(500);
				if (this.DisplayListFreeQueue.Count > 0) break;
				Console.Error.WriteLine("Empty Count");
			}
			*/
			lock (DisplayListFreeQueue)
			{
				var DisplayList = this.DisplayListFreeQueue.Dequeue();
				DisplayList.Available = false;
				return DisplayList;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GpuDisplayList"></param>
		public void EnqueueFreeDisplayList(GpuDisplayList GpuDisplayList)
		{
			//Console.WriteLine("EnqueueFreeDisplayList: {0}", this.DisplayListFreeQueue.Count);
			AddedDisplayList();
			lock (DisplayListFreeQueue)
			{
				this.DisplayListFreeQueue.Enqueue(GpuDisplayList);
				GpuDisplayList.Freed();
			}
			DisplayListFreeEvent.Set();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DisplayList"></param>
		public void EnqueueDisplayListFirst(GpuDisplayList DisplayList)
		{
			//Console.WriteLine("EnqueueDisplayListFirst: {0}", this.DisplayListFreeQueue.Count);
			AddedDisplayList();
			lock (DisplayListQueue)
			{
				DisplayListQueue.AddFirst(DisplayList);
			}
			DisplayListQueueUpdated.Set();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DisplayList"></param>
		public void EnqueueDisplayListLast(GpuDisplayList DisplayList)
		{
			//Console.WriteLine("EnqueueDisplayListLast: {0}", this.DisplayListFreeQueue.Count);
			AddedDisplayList();
			lock (DisplayListQueue)
			{
				DisplayListQueue.AddLast(DisplayList);
			}
			DisplayListQueueUpdated.Set();
		}


		public void ProcessInit()
		{
			GpuDisplayList.InstructionSwitch = GpuDisplayList.GenerateSwitch();
		}

		/// <summary>
		/// 
		/// </summary>
		public void ProcessStep()
		{
			Status.SetValue(StatusEnum.Completed);

			//Thread.Sleep(1);
			DisplayListQueueUpdated.WaitOne(1);

			while (true)
			{
				lock (DisplayListQueue)
				{
					if (DisplayListQueue.Count <= 0) break;
				}
				Status.SetValue(StatusEnum.Drawing);
				GpuDisplayList CurrentGpuDisplayList;

				//Console.WriteLine("**********************************************");
				lock (DisplayListQueue)
				{
					CurrentGpuDisplayList = DisplayListQueue.First.Value;
					DisplayListQueue.RemoveFirst();
				}
				{
					//Console.WriteLine("YYYYYYYYYYYYYYYYYYYYYYYYYYY");
					CurrentGpuDisplayList.Process();
					//Console.WriteLine("ZZZZZZZZZZZZZZZZZZZZZZZZZZ");
				}
				EnqueueFreeDisplayList(CurrentGpuDisplayList);
			}

			//if (DrawSync != null) DrawSync();
		}

		public void GeDrawSync(SyncTypeEnum SyncType, Action SyncCallback)
		{
			//Console.WriteLine("GeDrawSync: {0}", this.DisplayListFreeQueue.Count);
			if (SyncType != SyncTypeEnum.ListDone) throw new NotImplementedException();
			Status.CallbackOnStateOnce(StatusEnum.Completed, () =>
			{
				//Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
				SyncCallback();
			});
		}

		internal void MarkDepthBufferLoad()
		{
			//throw new NotImplementedException();
		}
	}
}

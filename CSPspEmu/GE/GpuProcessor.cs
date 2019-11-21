using System;
using System.Collections.Generic;
using System.Threading;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Threading.Synchronization;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Gpu
{
    public unsafe class GpuProcessor : IInjectInitialize
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
        /// Wait conditions for sceGeListSync() and sceGeDrawSync()
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
        public GlobalGpuState GlobalGpuState = new GlobalGpuState();

        /// <summary>
        /// 
        /// </summary>
        internal volatile LinkedList<GpuDisplayList> DisplayListQueue;

        /// <summary>
        /// 
        /// </summary>
        public volatile AutoResetEvent DisplayListQueueUpdated = new AutoResetEvent(false);

        /// <summary>
        /// 
        /// </summary>
        protected volatile Queue<GpuDisplayList> DisplayListFreeQueue;

        public const int DisplayListsCount = 64;

        /// <summary>
        /// All the supported Psp Display Lists (Available and not available).
        /// </summary>
        private readonly GpuDisplayList[] DisplayLists = new GpuDisplayList[DisplayListsCount];

        public GpuDisplayList GetDisplayList(int Index)
        {
            lock (DisplayLists) return DisplayLists[Index];
            //return DisplayLists[Index];
        }

        public enum Status2Enum
        {
            Completed = 0,
            HavePendingLists = 1,
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly WaitableStateMachine<Status2Enum> Status2 =
            new WaitableStateMachine<Status2Enum>(Status2Enum.Completed, Debug: false);

        /// <summary>
        /// 
        /// </summary>
        [Inject] public GpuImpl GpuImpl;

        /// <summary>
        /// 
        /// </summary>
        [Inject] public GpuConfig GpuConfig;

        /// <summary>
        /// 
        /// </summary>
        [Inject] public PspMemory Memory;

        /// <summary>
        /// 
        /// </summary>
        [Inject] public IGpuConnector Connector;

        private GpuProcessor()
        {
        }

        void IInjectInitialize.Initialize()
        {
            DisplayListQueue = new LinkedList<GpuDisplayList>();
            DisplayListFreeQueue = new Queue<GpuDisplayList>();
            for (int n = 0; n < DisplayListsCount; n++)
            {
                var DisplayList = new GpuDisplayList(Memory, this, n);
                DisplayLists[n] = DisplayList;
                //this.DisplayListFreeQueue.Enqueue(DisplayLists[n]);
                EnqueueFreeDisplayList(DisplayLists[n]);
            }
        }

        AutoResetEvent DisplayListFreeEvent = new AutoResetEvent(false);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public GpuDisplayList DequeueFreeDisplayList()
        {
            lock (DisplayListFreeQueue)
            {
                var DisplayList = DisplayListFreeQueue.Dequeue();
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
            lock (DisplayListFreeQueue)
            {
                DisplayListFreeQueue.Enqueue(GpuDisplayList);
                GpuDisplayList.SetFree();
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
                DisplayList.SetQueued();
            }
            DisplayListQueueUpdated.Set();
            ListEnqueuedEvent.Set();
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
                DisplayList.SetQueued();
            }
            DisplayListQueueUpdated.Set();
            ListEnqueuedEvent.Set();
        }


        public void ProcessInit()
        {
        }

        public AutoResetEvent ListEnqueuedEvent = new AutoResetEvent(false);

        private volatile GpuDisplayList CurrentGpuDisplayList = null;
        private volatile GpuDisplayList LastProcessedGpuDisplayList = null;
        public bool UsingGe { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public void ProcessStep()
        {
            CurrentGpuDisplayList = null;

            if (DisplayListQueue.GetCountLock() > 0)
            {
                UsingGe = true;
                while (DisplayListQueue.GetCountLock() > 0)
                {
                    CurrentGpuDisplayList = DisplayListQueue.RemoveFirstAndGet();
                    CurrentGpuDisplayList.SetDequeued();
                    LastProcessedGpuDisplayList = CurrentGpuDisplayList;
                    CurrentGpuDisplayList.Process();
                    EnqueueFreeDisplayList(CurrentGpuDisplayList);
                }
                CurrentGpuDisplayList = null;

                Status2.SetValue(Status2Enum.Completed);
            }
        }

        protected void AddedDisplayList()
        {
            //Console.WriteLine("Running");
            Status2.SetValue(Status2Enum.HavePendingLists);
            GpuImpl.AddedDisplayList();
        }

        internal bool Syncing = false;

        public void GeDrawSync(Action SyncCallback)
        {
            Syncing = true;
            Status2.CallbackOnStateOnce(Status2Enum.Completed, () =>
            {
                CapturingWaypoint();
                GpuImpl.Sync(LastProcessedGpuDisplayList.GpuStateStructPointer);
                SyncCallback();
                Syncing = false;
            });
        }

        private void CapturingWaypoint()
        {
            if (CapturingFrame)
            {
                CapturingFrame = false;
                Console.WriteLine("EndCapturingFrame!");
                GpuImpl.EndCapture();
            }

            if (StartCapturingFrame)
            {
                StartCapturingFrame = false;
                CapturingFrame = true;
                GpuImpl.StartCapture();
                Console.WriteLine("StartCapturingFrame!");
            }
        }

        internal void MarkDepthBufferLoad()
        {
            //throw new NotImplementedException();
        }

        public void SetCurrent()
        {
            GpuImpl.SetCurrent();
        }

        public void UnsetCurrent()
        {
            GpuImpl.UnsetCurrent();
        }

        bool StartCapturingFrame = false;
        bool CapturingFrame = false;

        public void CaptureFrame()
        {
            StartCapturingFrame = true;
            Console.WriteLine("Waiting StartCapturingFrame!");
        }

        public int GeContinue()
        {
            throw new NotImplementedException();
        }

        public GpuDisplayList GetCurrentGpuDisplayList()
        {
            return CurrentGpuDisplayList;
        }

        public bool IsBreak = false;

        public DisplayListStatusEnum PeekStatus()
        {
            var GpuDisplayList = CurrentGpuDisplayList;
            if (GpuDisplayList == null) return DisplayListStatusEnum.Completed;
            return GpuDisplayList.PeekStatus();
        }
    }

    public enum TextureLevelMode
    {
        Auto = 0,
        Const = 1,
        Slope = 2
    }
    
    public enum SyncTypeEnum : byte
    {
        WaitForCompletion = 0,
        Peek = 1,
    }

    public enum SignalBehavior : byte
    {
        PSP_GE_SIGNAL_NONE = 0x00,
        PSP_GE_SIGNAL_HANDLER_SUSPEND = 0x01,
        PSP_GE_SIGNAL_HANDLER_CONTINUE = 0x02,
        PSP_GE_SIGNAL_HANDLER_PAUSE = 0x03,
        PSP_GE_SIGNAL_SYNC = 0x08,
        PSP_GE_SIGNAL_JUMP = 0x10,
        PSP_GE_SIGNAL_CALL = 0x11,
        PSP_GE_SIGNAL_RET = 0x12,
        PSP_GE_SIGNAL_RJUMP = 0x13,
        PSP_GE_SIGNAL_RCALL = 0x14,
        PSP_GE_SIGNAL_OJUMP = 0x15,
        PSP_GE_SIGNAL_OCALL = 0x16,

        PSP_GE_SIGNAL_RTBP0 = 0x20,
        PSP_GE_SIGNAL_RTBP1 = 0x21,
        PSP_GE_SIGNAL_RTBP2 = 0x22,
        PSP_GE_SIGNAL_RTBP3 = 0x23,
        PSP_GE_SIGNAL_RTBP4 = 0x24,
        PSP_GE_SIGNAL_RTBP5 = 0x25,
        PSP_GE_SIGNAL_RTBP6 = 0x26,
        PSP_GE_SIGNAL_RTBP7 = 0x27,
        PSP_GE_SIGNAL_OTBP0 = 0x28,
        PSP_GE_SIGNAL_OTBP1 = 0x29,
        PSP_GE_SIGNAL_OTBP2 = 0x2A,
        PSP_GE_SIGNAL_OTBP3 = 0x2B,
        PSP_GE_SIGNAL_OTBP4 = 0x2C,
        PSP_GE_SIGNAL_OTBP5 = 0x2D,
        PSP_GE_SIGNAL_OTBP6 = 0x2E,
        PSP_GE_SIGNAL_OTBP7 = 0x2F,
        PSP_GE_SIGNAL_RCBP = 0x30,
        PSP_GE_SIGNAL_OCBP = 0x38,
        PSP_GE_SIGNAL_BREAK1 = 0xF0,
        PSP_GE_SIGNAL_BREAK2 = 0xFF,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DisplayListStatusEnum
    {
        /// <summary>
        /// The list has been completed (PSP_GE_LIST_COMPLETED)
        /// </summary>
        Completed = 0,

        /// <summary>
        ///  list is queued but not executed yet (PSP_GE_LIST_QUEUED)
        /// </summary>
        Queued = 1,

        /// <summary>
        /// The list is currently being executed (PSP_GE_LIST_DRAWING)
        /// </summary>
        Drawing = 2,

        /// <summary>
        /// The list was stopped because it encountered stall address (PSP_GE_LIST_STALLING)
        /// </summary>
        Stalling = 3,

        /// <summary>
        /// The list is paused because of a signal or sceGeBreak (PSP_GE_LIST_PAUSED)
        /// </summary>
        Paused = 4,
    }

    public unsafe struct PspGeStack
    {
        public fixed uint Stack[8];
    }

    /// <summary>
    /// 
    /// </summary>
    public unsafe struct PspGeListArgs
    {
        /// <summary>
        /// Size of the structure
        /// </summary>
        public uint Size;

        /// <summary>
        /// Pointer to a GpuStateStruct
        /// </summary>
        public uint GpuStateStructAddress;

        /// <summary>
        /// 
        /// </summary>
        public uint NumberOfStacks;

        /// <summary>
        /// 
        /// </summary>
        public uint StacksAddress;
    }
}
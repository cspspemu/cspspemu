using System;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Hle.Modules.sysmem;
using CSPspEmu.Core.Gpu.State;
using CSharpUtils;
using System.Collections.Generic;
using CSPspEmu.Hle.Managers;
using System.Runtime.InteropServices;
using System.Threading;

namespace CSPspEmu.Hle.Modules.ge
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public unsafe partial class sceGe_user : HleModuleHost
    {
        static Logger Logger = Logger.GetLogger("sceGe");

        [Inject] HleThreadManager ThreadManager;

        [Inject] HleMemoryManager MemoryManager;

        [Inject] public GpuProcessor GpuProcessor;

        [Inject] public CpuProcessor CpuProcessor;

        [Inject] public SysMemUserForUser SysMemUserForUser;

        private MemoryPartition GpuStateStructPartition = null;
        private GpuStateStruct* GpuStateStructPointer = null;
        private int eDRAMMemoryWidth;
        int CallbackLastId = 1;

        public Dictionary<int, PspGeCallbackData> Callbacks = new Dictionary<int, PspGeCallbackData>();
        //PspGeCallbackData

        /// <summary>
        /// Get the address of VRAM.
        /// </summary>
        /// <returns>A pointer to the base of VRAM.</returns>
        [HlePspFunction(NID = 0xE47E40E4, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public uint sceGeEdramGetAddr()
        {
            return PspMemory.FrameBufferSegment.Low;
        }

        /// <summary>
        /// Get the size of VRAM.
        /// </summary>
        /// <returns>The size of VRAM (in bytes).</returns>
        [HlePspFunction(NID = 0x1F6752AD, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeEdramGetSize()
        {
            return (int) PspMemory.FrameBufferSegment.Size;
        }

        /// <summary>
        /// Save the GE's current state. Save the GE's current state.
        /// </summary>
        /// <param name="ContextPtr">Pointer to a <see cref="PspGeContext"/>.</param>
        /// <returns>&lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x438A385A, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeSaveContext(GpuStateStruct* Context)
        {
            *Context = *this.GpuStateStructPointer;
            //throw (new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Restore a previously saved GE context.
        /// </summary>
        /// <param name="contextAddr">Pointer to a <see cref="PspGeContext"/>.</param>
        /// <returns>&lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x0BF608FB, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceGeRestoreContext(GpuStateStruct* Context)
        {
            *this.GpuStateStructPointer = *Context;
            return 0;
        }

        /// <summary>
        /// Retrive the current value of a GE command.
        /// </summary>
        /// <param name="cmd">The GE command register to retrieve.</param>
        /// <returns>The value of the GE command.</returns>
        [HlePspFunction(NID = 0xDC93CFEF, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeGetCmd(int cmd)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// Retrieve a matrix of the given type.
        /// </summary>
        /// <param name="MatrixType">One of <see cref="PspGeMatrixTypes"/>.</param>
        /// <param name="MatrixAddress">Pointer to a variable to store the matrix.</param>
        /// <returns>&lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x57C8945B, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeGetMtx(PspGeMatrixTypes MatrixType, uint* MatrixAddress)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// Matrix types that can be retrieved.
        /// </summary>
        public enum PspGeMatrixTypes
        {
            Bone0 = 0,
            Bone1 = 1,
            Bone2 = 2,
            Bone3 = 3,
            Bone4 = 4,
            Bone5 = 5,
            Bone6 = 6,
            Bone7 = 7,
            World = 8,
            View = 9,
            Projection = 10,
            Texture = 11,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0xB77905EA, FirmwareVersion = 150)]
        public int sceGeEdramSetAddrTranslation(int Size)
        {
            try
            {
                return eDRAMMemoryWidth;
            }
            finally
            {
                eDRAMMemoryWidth = Size;
            }
        }

        /// <summary>
        /// Interrupt drawing queue.
        /// </summary>
        /// <param name="Mode">If set to 1, reset all the queues.</param>
        /// <param name="BreakAddress">Unused (just K1-checked).</param>
        /// <returns>The stopped queue ID if mode isn't set to 0, otherwise 0, and &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xB448EC0D, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeBreak(int Mode, void* BreakAddress)
        {
            throw(new NotImplementedException());
        }

        /// <summary>
        /// Restart drawing queue.
        /// </summary>
        /// <returns>&lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x4C06E472, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeContinue()
        {
            var currentList = GpuProcessor.GetCurrentGpuDisplayList();
            if (currentList == null)
            {
                return 0;
            }

            if (currentList.Status.Value == DisplayListStatusEnum.Paused)
            {
                if (!GpuProcessor.IsBreak)
                {
                    if (currentList.Signal == SignalBehavior.PSP_GE_SIGNAL_HANDLER_PAUSE)
                    {
                        return unchecked((int) SceKernelErrors.ERROR_BUSY);
                    }

                    currentList.Status.SetValue(DisplayListStatusEnum.Drawing);
                    currentList.Signal = SignalBehavior.PSP_GE_SIGNAL_NONE;

                    // TODO Restore context of DL is necessary
                    // TODO Restore BASE

                    // We have a list now, so it's not complete.
                    //drawCompleteTicks = (u64) - 1;
                }
                else
                {
                    currentList.Status.SetValue(DisplayListStatusEnum.Queued);
                }
            }
            else if (currentList.Status.Value == DisplayListStatusEnum.Drawing)
            {
                if (SysMemUserForUser.sceKernelGetCompiledSdkVersion() >= 0x02000000)
                {
                    return unchecked((int) SceKernelErrors.ERROR_ALREADY);
                }
                return -1;
            }
            else
            {
                if (SysMemUserForUser.sceKernelGetCompiledSdkVersion() >= 0x02000000)
                {
                    return unchecked((int) 0x80000004);
                }
                return -1;
            }

            //ProcessDLQueue();
            return 0;
        }

        /// <summary>
        /// Register callback handlers for the the Ge
        /// </summary>
        /// <param name="PspGeCallbackData">Configured callback data structure</param>
        /// <returns>The callback ID, less than 0 on error</returns>
        [HlePspFunction(NID = 0xA4FC06A4, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeSetCallback(ref PspGeCallbackData PspGeCallbackData)
        {
            int CallbackId = CallbackLastId++;
            Callbacks[CallbackId] = PspGeCallbackData;

            var CallbackData = PspGeCallbackData;

            /*
            ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Cyan, () =>
            {
                Console.WriteLine("PspGeCallbackData.Finish(0x{0:X}) : (0x{1:X})", CallbackData.FinishFunction, CallbackData.FinishArgument);
                Console.WriteLine("PspGeCallbackData.Signal(0x{0:X}) : (0x{1:X})", CallbackData.SignalFunction, CallbackData.SignalArgument);
            });
            */

            Logger.Info("PspGeCallbackData.Finish(0x{0:X}) : (0x{1:X})", PspGeCallbackData.FinishFunction,
                PspGeCallbackData.FinishArgument);
            Logger.Info("PspGeCallbackData.Signal(0x{0:X}) : (0x{1:X})", PspGeCallbackData.SignalFunction,
                PspGeCallbackData.SignalArgument);

            //Console.Error.WriteLine("{0}", *PspGeCallbackData);
            return CallbackId;
        }

        /// <summary>
        /// Unregister the callback handlers
        /// </summary>
        /// <param name="cbid">The ID of the callbacks from sceGeSetCallback</param>
        /// <returns>Less than 0 on error</returns>
        [HlePspFunction(NID = 0x05DB22CE, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeUnsetCallback(int cbid)
        {
            Callbacks.Remove(cbid);
            return 0;
        }

        protected override void ModuleInitialize()
        {
            GpuStateStructPartition = MemoryManager.GetPartition(MemoryPartitions.Kernel0).Allocate(
                sizeof(GpuStateStruct),
                Name: "GpuStateStruct"
            );
            GpuStateStructPointer = (GpuStateStruct*) GpuStateStructPartition.GetLowPointerSafe<GpuStateStruct>();
        }

        private GpuDisplayList GetDisplayListFromId(int DisplayListId)
        {
            if (DisplayListId < 0 || DisplayListId >= GpuProcessor.DisplayListsCount)
            {
                throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_ID));
            }

            return GpuProcessor.GetDisplayList(DisplayListId);
        }

        private GpuDisplayList _sceGeListEnQueue(uint InstructionAddressStart, uint InstructionAddressStall,
            int CallbackId, PspGeListArgs* Args)
        {
            var DisplayList = GpuProcessor.DequeueFreeDisplayList();

            DisplayList.SetInstructionAddressStartAndCurrent(InstructionAddressStart);
            DisplayList.SetInstructionAddressStall(InstructionAddressStall);
            DisplayList.CallbacksId = -1;
            DisplayList.Callbacks = default(PspGeCallbackData);

            if (CallbackId != -1)
            {
                DisplayList.Callbacks = Callbacks[CallbackId];
                DisplayList.CallbacksId = CallbackId;
            }

            DisplayList.GpuStateStructPointer = null;

            if (Args != null)
            {
                DisplayList.GpuStateStructPointer =
                    (GpuStateStruct*) CpuProcessor.Memory.PspAddressToPointerSafe(Args->GpuStateStructAddress,
                        Marshal.SizeOf(typeof(GpuStateStruct)));
            }

            if (DisplayList.GpuStateStructPointer == null)
            {
                DisplayList.GpuStateStructPointer = GpuStateStructPointer;
            }

            return DisplayList;
        }

        /// <summary>
        /// Enqueue a display list at the tail of the GE display list queue.
        /// </summary>
        /// <param name="InstructionAddressStart">The head of the list to queue.</param>
        /// <param name="InstructionAddressStall">The stall address. If NULL then no stall address set and the list is transferred immediately.</param>
        /// <param name="CallbackId">ID of the callback set by calling <see cref="sceGeSetCallback"/></param>
        /// <param name="Args">Structure containing GE context buffer address</param>
        /// <returns>The DisplayList ID</returns>
        [HlePspFunction(NID = 0xAB49E76A, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceGeListEnQueue(uint InstructionAddressStart, uint InstructionAddressStall, int CallbackId,
            PspGeListArgs* Args)
        {
            var DisplayList = _sceGeListEnQueue(InstructionAddressStart, InstructionAddressStall, CallbackId, Args);
            GpuProcessor.EnqueueDisplayListLast(DisplayList);
            return DisplayList.Id;
        }

        /// <summary>
        /// Enqueue a display list at the head of the GE display list queue.
        /// </summary>
        /// <param name="InstructionAddressStart">The head of the list to queue.</param>
        /// <param name="InstructionAddressStall">The stall address. If NULL then no stall address set and the list is transferred immediately.</param>
        /// <param name="CallbackId">ID of the callback set by calling <see cref="sceGeSetCallback"/></param>
        /// <param name="Args">Structure containing GE context buffer address</param>
        /// <returns>The DisplayList ID</returns>
        [HlePspFunction(NID = 0x1C0D95A6, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeListEnQueueHead(uint InstructionAddressStart, uint InstructionAddressStall, int CallbackId,
            PspGeListArgs* Args)
        {
            var DisplayList = _sceGeListEnQueue(InstructionAddressStart, InstructionAddressStall, CallbackId, Args);
            GpuProcessor.EnqueueDisplayListFirst(DisplayList);
            return DisplayList.Id;
        }

        /// <summary>
        /// Cancel a queued or running list.
        /// </summary>
        /// <param name="DisplayListId">A DisplayList ID</param>
        /// <returns>&lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x5FB86AB0, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceGeListDeQueue(int DisplayListId)
        {
            var DisplayList = GetDisplayListFromId(DisplayListId);
            DisplayList.DeQueue();
            return 0;
        }

        /// <summary>
        /// Update the stall address for the specified queue.
        /// </summary>
        /// <param name="DisplayListId">The ID of the queue.</param>
        /// <param name="InstructionAddressStall">The stall address to update</param>
        /// <returns>Unknown. Probably 0 if successful. &lt; 0 on error</returns>
        [HlePspFunction(NID = 0xE0D68148, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceGeListUpdateStallAddr(int DisplayListId, uint InstructionAddressStall)
        {
            //hleEatCycles(190);

            var DisplayList = GetDisplayListFromId(DisplayListId);

            //if (!PspMemory.IsAddressValid(InstructionAddressStall))
            //{
            //	throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_POINTER));
            //}

            if (DisplayList.Status.Value == DisplayListStatusEnum.Completed)
            {
                throw (new SceKernelException(SceKernelErrors.ERROR_ALREADY));
            }

            DisplayList.SetInstructionAddressStall(InstructionAddressStall);

            if (DisplayList.Signal == SignalBehavior.PSP_GE_SIGNAL_HANDLER_PAUSE)
            {
                DisplayList.Signal = SignalBehavior.PSP_GE_SIGNAL_HANDLER_SUSPEND;
            }

            return 0;
        }

        /// <summary>
        /// Wait for syncronisation of a list.
        /// </summary>
        /// <param name="DisplayListId">The queue ID of the list to sync.</param>
        /// <param name="SyncType">Specifies the condition to wait on.  One of PspGeSyncType.</param>
        /// <returns>???</returns>
        [HlePspFunction(NID = 0x03444EB4, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public DisplayListStatusEnum sceGeListSync(int DisplayListId, SyncTypeEnum SyncType)
        {
            var DisplayList = GetDisplayListFromId(DisplayListId);

            switch (SyncType)
            {
                case SyncTypeEnum.WaitForCompletion:
                    ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.GraphicEngine, "sceGeListSync",
                        DisplayList, (WakeUp) => { DisplayList.GeListSync(WakeUp); });
                    return 0;
                case SyncTypeEnum.Peek:
                    return DisplayList.PeekStatus();
                default:
                    throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_MODE));
            }
        }

        /// <summary>
        /// Wait for drawing to complete.
        /// </summary>
        /// <param name="SyncType">Specifies the condition to wait on.  One of ::PspGeSyncType.</param>
        /// <returns>???</returns>
        [HlePspFunction(NID = 0xB287BD61, FirmwareVersion = 150, CheckInsideInterrupt = true)]
        //[HlePspNotImplemented]
        public DisplayListStatusEnum sceGeDrawSync(SyncTypeEnum SyncType)
        {
            //Thread.Sleep(40);
            switch (SyncType)
            {
                case SyncTypeEnum.WaitForCompletion:
                    ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.GraphicEngine, "sceGeDrawSync",
                        GpuProcessor, (WakeUp) => { GpuProcessor.GeDrawSync(WakeUp); });
                    return 0;
                case SyncTypeEnum.Peek:
                    return GpuProcessor.PeekStatus();
                default:
                    throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_MODE));
            }
        }
    }
}
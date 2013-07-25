//#define LIST_SYNC

using System;
using System.Runtime.InteropServices;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core;
using CSPspEmu.Hle.Managers;
using System.Threading;
using CSPspEmu.Core.Cpu;
using CSharpUtils;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.ge
{
	public unsafe partial class sceGe_user
	{
		[Inject]
		HleThreadManager ThreadManager;

		[Inject]
		HleMemoryManager MemoryManager;

		private MemoryPartition GpuStateStructPartition = null;
		private GpuStateStruct* GpuStateStructPointer = null;

		protected override void ModuleInitialize()
		{
			GpuStateStructPartition = MemoryManager.GetPartition(MemoryPartitions.Kernel0).Allocate(
				sizeof(GpuStateStruct),
				Name: "GpuStateStruct"
			);
			GpuStateStructPointer = (GpuStateStruct*)GpuStateStructPartition.GetLowPointerSafe<GpuStateStruct>();
		}

		private GpuDisplayList GetDisplayListFromId(int DisplayListId)
		{
			if (DisplayListId < 0 || DisplayListId >= GpuProcessor.DisplayListsCount)
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_ID));
			}

			return GpuProcessor.GetDisplayList(DisplayListId);
		}

		private GpuDisplayList _sceGeListEnQueue(uint InstructionAddressStart, uint InstructionAddressStall, int CallbackId, PspGeListArgs* Args)
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
				DisplayList.GpuStateStructPointer = (GpuStateStruct*)CpuProcessor.Memory.PspAddressToPointerSafe(Args->GpuStateStructAddress, Marshal.SizeOf(typeof(GpuStateStruct)));
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
		public int sceGeListEnQueue(uint InstructionAddressStart, uint InstructionAddressStall, int CallbackId, PspGeListArgs* Args)
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
		//[HlePspNotImplemented]
		public int sceGeListEnQueueHead(uint InstructionAddressStart, uint InstructionAddressStall, int CallbackId, PspGeListArgs* Args)
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
		//[HlePspNotImplemented]
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
		//[HlePspNotImplemented]
		public DisplayListStatusEnum sceGeListSync(int DisplayListId, SyncTypeEnum SyncType)
		{
			var DisplayList = GetDisplayListFromId(DisplayListId);

			switch (SyncType)
			{
				case SyncTypeEnum.WaitForCompletion:
					ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.GraphicEngine, "sceGeListSync", DisplayList, (WakeUp) =>
					{
						DisplayList.GeListSync(WakeUp);
					});
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
			switch (SyncType)
			{
				case SyncTypeEnum.WaitForCompletion:
					ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.GraphicEngine, "sceGeDrawSync", GpuProcessor, (WakeUp) =>
					{
						GpuProcessor.GeDrawSync(WakeUp);
					});
					return 0;
				case SyncTypeEnum.Peek:
					return GpuProcessor.PeekStatus();
				default:
					throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_MODE));
			}
		}
	}
}

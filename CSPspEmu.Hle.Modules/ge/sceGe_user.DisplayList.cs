using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.ge
{
	unsafe public partial class sceGe_user
	{
		private GpuDisplayList GetDisplayListFromId(int DisplayListId) {
			return HleState.GpuProcessor.DisplayLists[DisplayListId];
		}

		public int _sceGeListEnQueue(uint* InstructionAddressStart, uint* InstructionAddressStall, int CallbackId, PspGeListArgs* Args, Action<GpuDisplayList> Action)
		{
			var DisplayList = HleState.GpuProcessor.DequeueFreeDisplayList();
			{
				DisplayList.InstructionAddressStart = InstructionAddressStart;
				DisplayList.InstructionAddressCurrent = InstructionAddressStart;
				DisplayList.InstructionAddressStall = InstructionAddressStall;
				if (Args != null)
				{
					//DisplayList.GpuStateStructPointer = (GpuStateStruct*)HleState.CpuProcessor.Memory.PspAddressToPointer(Args[0].GpuStateStructAddress);
					throw(new NotImplementedException());
				}
				else
				{
					DisplayList.GpuStateStructPointer = (GpuStateStruct*)HleState.CpuProcessor.Memory.PspAddressToPointerSafe(0x08107000);
				}
				Action(DisplayList);
			}
			return DisplayList.Id;
		}

		/// <summary>
		/// Enqueue a display list at the tail of the GE display list queue.
		/// </summary>
		/// <param name="InstructionAddressStart">The head of the list to queue.</param>
		/// <param name="InstructionAddressStall">The stall address. If NULL then no stall address set and the list is transferred immediately.</param>
		/// <param name="CallbackId">ID of the callback set by calling sceGeSetCallback</param>
		/// <param name="Args">Structure containing GE context buffer address</param>
		/// <returns>The DisplayList Id</returns>
		[HlePspFunction(NID = 0xAB49E76A, FirmwareVersion = 150)]
		[HlePspNotImplemented(PartialImplemented = true, Notice = false)]
		public int sceGeListEnQueue(uint* InstructionAddressStart, uint* InstructionAddressStall, int CallbackId, PspGeListArgs* Args)
		{
			return _sceGeListEnQueue(InstructionAddressStart, InstructionAddressStall, CallbackId, Args, (DisplayList) =>
			{
				HleState.GpuProcessor.EnqueueDisplayListLast(DisplayList);
			});
		}

		/// <summary>
		/// Enqueue a display list at the head of the GE display list queue.
		/// </summary>
		/// <param name="InstructionAddressStart">The head of the list to queue.</param>
		/// <param name="InstructionAddressStall">The stall address. If NULL then no stall address set and the list is transferred immediately.</param>
		/// <param name="CallbackId">ID of the callback set by calling sceGeSetCallback</param>
		/// <param name="Args">Structure containing GE context buffer address</param>
		/// <returns>The DisplayList Id</returns>
		[HlePspFunction(NID = 0x1C0D95A6, FirmwareVersion = 150)]
		[HlePspNotImplemented(PartialImplemented = true, Notice = false)]
		public int sceGeListEnQueueHead(uint* InstructionAddressStart, uint* InstructionAddressStall, int CallbackId, PspGeListArgs* Args)
		{
			return _sceGeListEnQueue(InstructionAddressStart, InstructionAddressStall, CallbackId, Args, (DisplayList) =>
			{
				HleState.GpuProcessor.EnqueueDisplayListFirst(DisplayList);
			});
		}

		/// <summary>
		/// Cancel a queued or running list.
		/// </summary>
		/// <param name="DisplayListId">A DisplayList Id</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0x5FB86AB0, FirmwareVersion = 150)]
		[HlePspNotImplemented(PartialImplemented = true)]
		public int sceGeListDeQueue(int DisplayListId)
		{
			var DisplayList = GetDisplayListFromId(DisplayListId);
			HleState.GpuProcessor.DisplayListQueue.Remove(DisplayList);
			return 0;
		}

		/// <summary>
		/// Update the stall address for the specified queue.
		/// </summary>
		/// <param name="DisplayListId">The ID of the queue.</param>
		/// <param name="InstructionAddressStall">The stall address to update</param>
		/// <returns>Unknown. Probably 0 if successful.</returns>
		[HlePspFunction(NID = 0xE0D68148, FirmwareVersion = 150)]
		public int sceGeListUpdateStallAddr(int DisplayListId, uint* InstructionAddressStall)
		{
			var DisplayList = GetDisplayListFromId(DisplayListId);
			DisplayList.InstructionAddressStall = InstructionAddressStall;
			return 0;
		}

		/// <summary>
		/// Wait for syncronisation of a list.
		/// </summary>
		/// <param name="DisplayListId">The queue ID of the list to sync.</param>
		/// <param name="SyncType">Specifies the condition to wait on.  One of ::PspGeSyncType.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0x03444EB4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeListSync(int DisplayListId, GpuProcessor.SyncTypeEnum SyncType)
		{
			var DisplayList = GetDisplayListFromId(DisplayListId);

			HleState.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.GraphicEngine, "sceGeListSync", (WakeUpCallbackDelegate) =>
			{
				DisplayList.GeListSync(SyncType, () =>
				{
					WakeUpCallbackDelegate();
				});
			});

			return 0;
		}

		/// <summary>
		/// Wait for drawing to complete.
		/// </summary>
		/// <param name="SyncType">Specifies the condition to wait on.  One of ::PspGeSyncType.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0xB287BD61, FirmwareVersion = 150)]
		[HlePspNotImplemented(PartialImplemented = true, Notice = false)]
		public int sceGeDrawSync(GpuProcessor.SyncTypeEnum SyncType)
		{
			HleState.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.GraphicEngine, "sceGeDrawSync", (WakeUpCallbackDelegate) =>
			{
				HleState.GpuProcessor.GeDrawSync(SyncType, () =>
				{
					WakeUpCallbackDelegate();
				});
			});

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public struct PspGeListArgs
		{
			/// <summary>
			/// Size
			/// </summary>
			public uint Size;

			/// <summary>
			/// Pointer to a GpuStateStruct
			/// </summary>
			public uint GpuStateStructAddress;
		}
	}
}

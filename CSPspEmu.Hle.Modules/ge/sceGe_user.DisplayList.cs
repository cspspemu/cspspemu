using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.ge
{
	unsafe public partial class sceGe_user
	{
		/// <summary>
		/// Enqueue a display list at the tail of the GE display list queue.
		/// </summary>
		/// <param name="InstructionAddressStart">The head of the list to queue.</param>
		/// <param name="InstructionAddressStall">The stall address. If NULL then no stall address set and the list is transferred immediately.</param>
		/// <param name="CallbackId">ID of the callback set by calling sceGeSetCallback</param>
		/// <param name="Args">Structure containing GE context buffer address</param>
		/// <returns>The ID of the queue.</returns>
		[HlePspFunction(NID = 0xAB49E76A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeListEnQueue(void* InstructionAddressStart, void* InstructionAddressStall, int CallbackId, PspGeListArgs* Args)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Enqueue a display list at the head of the GE display list queue.
		/// </summary>
		/// <param name="InstructionAddressStart">The head of the list to queue.</param>
		/// <param name="InstructionAddressStall">The stall address. If NULL then no stall address set and the list is transferred immediately.</param>
		/// <param name="CallbackId">ID of the callback set by calling sceGeSetCallback</param>
		/// <param name="Args">Structure containing GE context buffer address</param>
		/// <returns>The ID of the queue.</returns>
		[HlePspFunction(NID = 0x1C0D95A6, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeListEnQueueHead(void* InstructionAddressStart, void* InstructionAddressStall, int CallbackId, PspGeListArgs* Args)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Cancel a queued or running list.
		/// </summary>
		/// <param name="id">The ID of the queue.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0x5FB86AB0, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeListDeQueue(int id)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Update the stall address for the specified queue.
		/// </summary>
		/// <param name="QueueId">The ID of the queue.</param>
		/// <param name="StallAddress">The stall address to update</param>
		/// <returns>Unknown. Probably 0 if successful.</returns>
		[HlePspFunction(NID = 0xE0D68148, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeListUpdateStallAddr(int QueueId, void* StallAddress)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Wait for syncronisation of a list.
		/// </summary>
		/// <param name="QueueId">The queue ID of the list to sync.</param>
		/// <param name="SyncType">Specifies the condition to wait on.  One of ::PspGeSyncType.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0x03444EB4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeListSync(int QueueId, SyncTypeEnum SyncType)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Wait for drawing to complete.
		/// </summary>
		/// <param name="SyncType">Specifies the condition to wait on.  One of ::PspGeSyncType.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0xB287BD61, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeDrawSync(SyncTypeEnum SyncType)
		{
			//throw (new NotImplementedException());
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
			public uint GpuStateStructPointer;
		}

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
	}
}

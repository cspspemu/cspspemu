using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.ge
{
	unsafe public partial class sceGe_user : HleModuleHost
	{
		/// <summary>
		/// Get the address of VRAM.
		/// </summary>
		/// <returns>A pointer to the base of VRAM.</returns>
		[HlePspFunction(NID = 0xE47E40E4, FirmwareVersion = 150)]
		public uint sceGeEdramGetAddr()
		{
			return HleState.Processor.Memory.FrameBufferSegment.Low;
		}

		/// <summary>
		/// Enqueue a display list at the tail of the GE display list queue.
		/// </summary>
		/// <param name="InstructionAddressStart">The head of the list to queue.</param>
		/// <param name="InstructionAddressStall">
		///		The stall address.
		///		If NULL then no stall address set and the list is transferred immediately.
		///	</param>
		/// <param name="CallbackId">ID of the callback set by calling sceGeSetCallback</param>
		/// <param name="Args">Structure containing GE context buffer address</param>
		/// <returns>The ID of the queue.</returns>
		[HlePspFunction(NID = 0xAB49E76A, FirmwareVersion = 150)]
		public int sceGeListEnQueue(void* InstructionAddressStart, void* InstructionAddressStall, int CallbackId, PspGeListArgs *Args)
		{
			throw(new NotImplementedException());
			/*
			return cast(int)cast(void*)cpu.gpu.sceGeListEnQueue(list, stall);
			*/
		}

		/// <summary>
		/// Update the stall address for the specified queue.
		/// </summary>
		/// <param name="QueueId">The ID of the queue.</param>
		/// <param name="StallAddress">The stall address to update</param>
		/// <returns>Unknown. Probably 0 if successful.</returns>
		[HlePspFunction(NID = 0xE0D68148, FirmwareVersion = 150)]
		public int sceGeListUpdateStallAddr(int QueueId, void *StallAddress)
		{
			throw(new NotImplementedException());
			/*
			cpu.gpu.sceGeListUpdateStallAddr(cast(DisplayList*)qid, stall);
			return 0;
			*/
		}

		/// <summary>
		/// Wait for syncronisation of a list.
		/// </summary>
		/// <param name="QueueId">The queue ID of the list to sync.</param>
		/// <param name="SyncType">Specifies the condition to wait on.  One of ::PspGeSyncType.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0x03444EB4, FirmwareVersion = 150)]
		public int sceGeListSync(int QueueId, SyncTypeEnum SyncType)
		{
			throw(new NotImplementedException());
			/*
			bool waiting = true;
			(new Thread({
				cpu.gpu.sceGeListSync(cast(DisplayList*)qid, syncType);
				waiting = false;
			})).start();

			return moduleManager.get!(ThreadManForUser).threadManager.currentThread.pauseAndYield(
				"sceGeListSync", (PspThread pausedThread) {
					if (!waiting) {
						pausedThread.resumeAndReturn(0);
					}
				}
			);
			*/
		}

		/// <summary>
		/// Wait for drawing to complete.
		/// </summary>
		/// <param name="SyncType">Specifies the condition to wait on.  One of ::PspGeSyncType.</param>
		/// <returns>???</returns>
		[HlePspFunction(NID = 0xB287BD61, FirmwareVersion = 150)]
		public int sceGeDrawSync(SyncTypeEnum SyncType)
		{
			throw(new NotImplementedException());
			/*
			bool waiting = true;
			(new Thread({
				cpu.gpu.sceGeDrawSync(syncType);
				waiting = false;
			})).start();

			return moduleManager.get!(ThreadManForUser).threadManager.currentThread.pauseAndYield(
				"sceGeDrawSync", (PspThread pausedThread) {
					if (!waiting) {
						pausedThread.resumeAndReturn(0);
					}
				}
			);
			*/
		}

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

		public enum SyncTypeEnum : uint
		{
			ListDone = 0,
			ListQueued = 1,
			ListDrawingDone = 2,
			ListStallReached = 3,
			ListCancelDone = 4,
		}
	}
}

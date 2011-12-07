using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		/// <summary>
		/// Create a fixed pool
		/// </summary>
		/// <param name="Name">Name of the pool</param>
		/// <param name="PartitionId">The memory partition ID</param>
		/// <param name="Attributes">Attributes</param>
		/// <param name="BlockSize">Size of pool block</param>
		/// <param name="NumberOfBlocks">Number of blocks to allocate</param>
		/// <param name="Options">Options (set to NULL)</param>
		/// <returns>The UID of the created pool, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xC07BB470, FirmwareVersion = 150)]
		public int sceKernelCreateFpl(string Name, int PartitionId, int Attributes, uint BlockSize, uint NumberOfBlocks, void *Options)
		{
			throw(new NotImplementedException());
			/*
			//new MemorySegment
			logWarning("sceKernelCreateFpl('%s', %d, %d, %d, %d)", Name, PartitionId, Attributes, BlockSize, NumberOfBlocks);
			FixedPool fixedPool;
			fixedPool = new FixedPool(
				hleEmulatorState.moduleManager.get!SysMemUserForUser()._allocateMemorySegmentLow(PartitionId, dupStr(Name), BlockSize * NumberOfBlocks),
				BlockSize,
				NumberOfBlocks
			);
			logWarning("%s", fixedPool);
			return uniqueIdFactory.add(fixedPool);
			*/
		}

		/// <summary>
		/// Try to allocate from the pool immediately.
		/// </summary>
		/// <param name="PoolId">The UID of the pool</param>
		/// <param name="DataPointerPointer">Receives the address of the allocated data</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x623AE665, FirmwareVersion = 150)]
		public int sceKernelTryAllocateFpl(int PoolId, uint DataPointerPointer)
		{
			throw(new NotImplementedException());
			/*
			logWarning("sceKernelTryAllocateFpl(%d, %08X)", PoolId, DataPointerPointer);
			FixedPool fixedPool = uniqueIdFactory.get!FixedPool(PoolId);
			try {
				currentMemory().twrite(DataPointerPointer, cast(uint)fixedPool.allocate());
				return 0;
			} catch (Exception e) {
				return SceKernelErrors.ERROR_KERNEL_NO_MEMORY;
			}
			//return sceKernelTryAllocateVpl(uid, data);
			*/
		}

		/// <summary>
		/// Allocate from the pool. It will wait for a free block to be available the specified time.
		/// </summary>
		/// <param name="PoolId">The UID of the pool</param>
		/// <param name="DataPointerPointer">Receives the address of the allocated data</param>
		/// <param name="Timeout">Amount of time to wait for allocation?</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD979E9BF, FirmwareVersion = 150)]
		public int sceKernelAllocateFpl(int PoolId, uint DataPointerPointer, uint *Timeout)
		{
			throw(new NotImplementedException());
			/*
			logWarning("sceKernelAllocateFpl(%d, %08X, %08X) @TODO Not waiting", uid, dataPtr, cast(uint)timeout);
			return sceKernelTryAllocateFpl(uid, dataPtr);
			*/
		}

		/// <summary>
		/// Delete a fixed pool
		/// </summary>
		/// <param name="uid">The UID of the pool</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xED1410E0, FirmwareVersion = 150)]
		public int sceKernelDeleteFpl(int uid)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Free a block
		/// </summary>
		/// <param name="uid">The UID of the pool</param>
		/// <param name="data">The data block to deallocate</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xF6414A71, FirmwareVersion = 150)]
		public int sceKernelFreeFpl(int uid, void *data)
		{
			throw (new NotImplementedException());
		}
	}
}

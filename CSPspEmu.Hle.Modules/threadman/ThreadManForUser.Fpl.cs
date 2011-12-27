using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		public class FixedPool
		{
			public HleMemoryManager MemoryManager;
			public string Name;
			public HleMemoryManager.Partitions PartitionId;
			public int Attributes;
			public int BlockSize;
			public int NumberOfBlocks;
			public MemoryPartition MemoryPartition;
			public List<uint> FreeBlocks;
			public List<uint> UsedBlocks;

			public void Init()
			{
				var Partition = MemoryManager.GetPartition(PartitionId);
				this.MemoryPartition = Partition.Allocate(NumberOfBlocks * BlockSize);
				this.FreeBlocks = new List<uint>();
				this.UsedBlocks = new List<uint>();
				for (int n = 0; n < NumberOfBlocks; n++)
				{
					this.FreeBlocks.Add(GetAddressFromBlockIndex(n));
				}
			}

			public uint GetAddressFromBlockIndex(int Index)
			{
				return (uint)(MemoryPartition.Low + Index * BlockSize);
			}
		}

		public enum PoolId : int { }

		HleUidPoolSpecial<FixedPool, PoolId> FixedPoolList = new HleUidPoolSpecial<FixedPool, PoolId>()
		{
			OnKeyNotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_FPOOL,
		};

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
		public PoolId sceKernelCreateFpl(string Name, HleMemoryManager.Partitions PartitionId, int Attributes, int BlockSize, int NumberOfBlocks, void* Options)
		{
			if (Options != null) throw(new NotImplementedException());

			var FixedPool = new FixedPool()
			{
				MemoryManager = HleState.MemoryManager,
				Name = Name,
				PartitionId = PartitionId,
				Attributes = Attributes,
				BlockSize = BlockSize,
				NumberOfBlocks = NumberOfBlocks,
			};
			FixedPool.Init();

			return FixedPoolList.Create(FixedPool);
		}

		/// <summary>
		/// Try to allocate from the pool immediately.
		/// </summary>
		/// <param name="PoolId">The UID of the pool</param>
		/// <param name="DataPointerPointer">Receives the address of the allocated data</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x623AE665, FirmwareVersion = 150)]
		public int sceKernelTryAllocateFpl(PoolId PoolId, uint* DataPointerPointer)
		{
			var FixedPool = FixedPoolList.Get(PoolId);

			if (FixedPool.FreeBlocks.Count <= 0)
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_NO_MEMORY));
				//throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT));
			}

			var AllocatedBlock = FixedPool.FreeBlocks.First();
			FixedPool.FreeBlocks.Remove(AllocatedBlock);
			FixedPool.UsedBlocks.Add(AllocatedBlock);

			*DataPointerPointer = AllocatedBlock;
			return 0;
		}

		/// <summary>
		/// Allocate from the pool. It will wait for a free block to be available the specified time.
		/// </summary>
		/// <param name="PoolId">The UID of the pool</param>
		/// <param name="DataPointerPointer">Receives the address of the allocated data</param>
		/// <param name="Timeout">Amount of time to wait for allocation?</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD979E9BF, FirmwareVersion = 150)]
		public int sceKernelAllocateFpl(PoolId PoolId, uint DataPointerPointer, uint* Timeout)
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
		/// <param name="PoolId">The UID of the pool</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xED1410E0, FirmwareVersion = 150)]
		public int sceKernelDeleteFpl(PoolId PoolId)
		{
			FixedPoolList.Remove(PoolId);
			return 0;
		}

		/// <summary>
		/// Free a block
		/// </summary>
		/// <param name="PoolId">The UID of the pool</param>
		/// <param name="DataPointer">The data block to deallocate</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xF6414A71, FirmwareVersion = 150)]
		public int sceKernelFreeFpl(PoolId PoolId, uint DataPointer)
		{
			var FixedPool = FixedPoolList.Get(PoolId);
			if (!FixedPool.UsedBlocks.Contains(DataPointer))
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMBLOCK));
			}
			FixedPool.UsedBlocks.Remove(DataPointer);
			FixedPool.FreeBlocks.Add(DataPointer);
			return 0;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.sysmem
{
	public class SysMemUserForUser : HleModuleHost
	{
		/// <summary>
		/// Get the size of the largest free memory block.
		/// </summary>
		/// <returns>The size of the largest free memory block, in bytes.</returns>
		[HlePspFunction(NID = 0xA291F107, FirmwareVersion = 150)]
		public int sceKernelMaxFreeMemSize()
		{
			return HleState.MemoryManager.RootPartition.ChildPartitions
				.Where(Partition => !Partition.Allocated)
				.OrderByDescending(Partition => Partition.Size)
				.First()
				.Size
			;
		}

		/// <summary>
		/// Free a memory block allocated with ::sceKernelAllocPartitionMemory.
		/// </summary>
		/// <param name="blockid">UID of the block to free.</param>
		/// <returns>? on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x237DBD4F, FirmwareVersion = 150)]
		public int sceKernelFreePartitionMemory(uint blockid)
		{
			//reinterpret!(MemorySegment)(blockid).free();
			return 0;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.sysmem
{
	public class SysMemUserForUser : HleModuleHost
	{
		/// <summary>
		/// Get the firmware version.
		/// 
		/// 0x01000300 on v1.00 unit,
		/// 0x01050001 on v1.50 unit,
		/// 0x01050100 on v1.51 unit,
		/// 0x01050200 on v1.52 unit,
		/// 0x02000010 on v2.00/v2.01 unit,
		/// 0x02050010 on v2.50 unit,
		/// 0x02060010 on v2.60 unit,
		/// 0x02070010 on v2.70 unit,
		/// 0x02070110 on v2.71 unit.
		/// </summary>
		/// <returns>The firmware version.</returns>
		[HlePspFunction(NID = 0x3FC9AE6A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelDevkitVersion()
		{
			//Logger.log(Logger.Level.TRACE, "SysMemUserForUser", "sceKernelDevkitVersion");
			//return hleOsConfig.firmwareVersion;
			return 0x02070110;
		}

		// @TODO: Unknown.
		[HlePspFunction(NID = 0xF77D77CB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompilerVersion(uint param)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="param"></param>
		[HlePspFunction(NID = 0x7591C7DB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion(uint param)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="param"></param>
		[HlePspFunction(NID = 0x342061E5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion370(uint param)
		{
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="param"></param>
		[HlePspFunction(NID = 0x315AD3A0, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion380_390(uint param)
		{
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="param"></param>
		[HlePspFunction(NID = 0xEBD5C3E6, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion395(uint param)
		{
		}

		/// <summary>
		/// Get the size of the largest free memory block.
		/// </summary>
		/// <returns>The size of the largest free memory block, in bytes.</returns>
		[HlePspFunction(NID = 0xA291F107, FirmwareVersion = 150)]
		public int sceKernelMaxFreeMemSize()
		{
			//foreach (var Partition in HleState.MemoryManager.RootPartition.ChildPartitions) Console.WriteLine(Partition);
			//return 24 * 1024 * 1024;

			return HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User).ChildPartitions
				.Where(Partition => !Partition.Allocated)
				.OrderByDescending(Partition => Partition.Size)
				.First()
				.Size
			;
		}

		/// <summary>
		/// Get the total amount of free memory.
		/// </summary>
		/// <returns>The total amount of free memory, in bytes.</returns>
		[HlePspFunction(NID = 0xF919F628, FirmwareVersion = 150)]
		public int sceKernelTotalFreeMemSize()
		{
			return HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User).ChildPartitions
				.Where(Partition => !Partition.Allocated)
				.Aggregate(0, (Accumulated, Partition) => Accumulated + Partition.Size)
			;
		}

		/// <summary>
		/// Get the address of a memory block.
		/// </summary>
		/// <param name="BlockId">UID of the memory block.</param>
		/// <returns>The lowest address belonging to the memory block.</returns>
		[HlePspFunction(NID = 0x9D9A5BA1, FirmwareVersion = 150)]
		public uint sceKernelGetBlockHeadAddr(int BlockId)
		{
			return HleState.MemoryManager.MemoryPartitionsUid.Get(BlockId).Low;
		}

		/// <summary>
		/// Allocate a memory block from a memory partition. 
		/// </summary>
		/// <param name="PartitionId">The UID of the partition to allocate from.</param>
		/// <param name="Name">Name assigned to the new block.</param>
		/// <param name="Type">Specifies how the block is allocated within the partition.  One of ::PspSysMemBlockTypes.</param>
		/// <param name="Size">Size of the memory block, in bytes.</param>
		/// <param name="Address">If type is PSP_SMEM_Addr, then addr specifies the lowest address allocate the block from. If not, the alignment size.</param>
		/// <returns>The UID of the new block, or if less than 0 an error.</returns>
		[HlePspFunction(NID = 0x237DBD4F, FirmwareVersion = 150)]
		public int sceKernelAllocPartitionMemory(HleMemoryManager.Partitions PartitionId, string Name, HleMemoryManager.BlockTypeEnum Type, int Size, /* void* */uint Address)
		{
			MemoryPartition MemoryPartition;
			int Alignment = 1;
			switch (Type)
			{
				case HleMemoryManager.BlockTypeEnum.HighAligned:
				case HleMemoryManager.BlockTypeEnum.LowAligned:
					Alignment = (int)Address;
					break;
			}
			if (Type == HleMemoryManager.BlockTypeEnum.Low || Type == HleMemoryManager.BlockTypeEnum.LowAligned)
			{
				MemoryPartition = HleState.MemoryManager.GetPartition(PartitionId).Allocate(Size, MemoryPartition.Anchor.Low, Alignment: Alignment);
			}
			else
			{
				throw (new NotImplementedException("Not Implemented sceKernelAllocPartitionMemory with '" + Type + "'"));
			}

			return (int)HleState.MemoryManager.MemoryPartitionsUid.Create(MemoryPartition);

			/*
			throw(new NotImplementedException());
			const uint ERROR_KERNEL_ILLEGAL_MEMBLOCK_ALLOC_TYPE = 0x800200d8;
			const uint ERROR_KERNEL_FAILED_ALLOC_MEMBLOCK       = 0x800200d9;

			try {
				MemorySegment memorySegment;
			
				Logger.log(Logger.Level.INFO, "SysMemUserForUser", "sceKernelAllocPartitionMemory(%d:'%s':%s:%d,0x%08X)", PartitionId, Name, std.conv.to!string(Type), Size, Address);
				//Logger.log(Logger.Level.INFO, "SysMemUserForUser", "sceKernelAllocPartitionMemory(%d:'%s':%d:%d)", partitionid, name, (type), size);
			
				int alignment = 1;
				if ((Type == PspSysMemBlockTypes.PSP_SMEM_Low_Aligned) || (Type == PspSysMemBlockTypes.PSP_SMEM_High_Aligned)) {
					alignment = Address;
				}
	
				switch (Type) {
					default: return ERROR_KERNEL_ILLEGAL_MEMBLOCK_ALLOC_TYPE;
					case PspSysMemBlockTypes.PSP_SMEM_Low_Aligned:
					case PspSysMemBlockTypes.PSP_SMEM_Low : memorySegment = pspMemorySegment[PartitionId].allocByLow (Size, dupStr(Name), 0, alignment); break;
					case PspSysMemBlockTypes.PSP_SMEM_High_Aligned:
					case PspSysMemBlockTypes.PSP_SMEM_High: memorySegment = pspMemorySegment[PartitionId].allocByHigh(Size, dupStr(Name), alignment); break;
					case PspSysMemBlockTypes.PSP_SMEM_Addr: memorySegment = pspMemorySegment[PartitionId].allocByAddr(Address, Size, dupStr(Name)); break;
				}
	
				if (memorySegment is null) return ERROR_KERNEL_FAILED_ALLOC_MEMBLOCK;
			
				SceUID sceUid = uniqueIdFactory.add(memorySegment);
			
				Logger.log(Logger.Level.INFO, "SysMemUserForUser", "sceKernelAllocPartitionMemory(%d:'%s':%s:%d) :: (%d) -> %s", PartitionId, Name, std.conv.to!string(Type), Size, sceUid, memorySegment.block);
				//Logger.log(Logger.Level.INFO, "SysMemUserForUser", "sceKernelAllocPartitionMemory(%d:'%s':%d:%d) :: (%d) -> %s", partitionid, name, (type), size, sceUid, memorySegment.block);

				return sceUid;
			}
			catch (Exception Exception)
			{
				//logError("ERROR: %s", o);
				return ERROR_KERNEL_FAILED_ALLOC_MEMBLOCK;
			}
			*/
		}


		/// <summary>
		/// Free a memory block allocated with ::sceKernelAllocPartitionMemory.
		/// </summary>
		/// <param name="blockid">UID of the block to free.</param>
		/// <returns>? on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB6D61D02, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceKernelFreePartitionMemory(uint blockid)
		{
			//reinterpret!(MemorySegment)(blockid).free();
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		[HlePspFunction(NID = 0x13A5ABEF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelPrintf(CpuThreadState CpuThreadState)
		{
			throw(new NotImplementedException());
		}
	}
}

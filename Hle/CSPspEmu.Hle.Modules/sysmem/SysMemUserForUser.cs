using System;
using System.Linq;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules.sysmem
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00000011)]
	public class SysMemUserForUser : HleModuleHost
	{
		[Inject]
		HleConfig HleConfig;

		[Inject]
		HleMemoryManager MemoryManager;

		[Inject]
		KDebugForKernel KDebugForKernel;

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
		public int sceKernelDevkitVersion()
		{
			var Version = HleConfig.FirmwareVersion;
			return (Version.Major << 24) | (Version.Minor << 16) | (Version.Revision << 8) | 0x10;
		}

		public enum PspModelEnum : int
		{
			Phat = 0,
			Slim = 1,
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x6373995D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public PspModelEnum sceKernelGetModel()
		{
			return PspModel.IsSlim ? PspModelEnum.Slim : PspModelEnum.Phat;
		}

		/// <summary>
		/// 1.00 to 3.52, gone in 3.95+
		/// </summary>
		[HlePspFunction(NID = 0x35669D4C, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void SysMemUserForUser_35669D4C()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Version"></param>
		[HlePspFunction(NID = 0xF77D77CB, FirmwareVersion = 150)]
		public void sceKernelSetCompilerVersion(uint Version)
		{
			HleConfig.CompilerVersion = Version;
		}

		private void _sceKernelSetCompiledSdkVersion(uint SdkVersion)
		{
			HleConfig.CompiledSdkVersion = SdkVersion;
			HleConfig.SdkFlags |= SdkFlags.SCE_KERNEL_HASCOMPILEDSDKVERSION;
		}

		private void _sceKernelSetCompiledSdkVersion(uint SdkVersion, string Name, uint[] ValidMainVersions)
		{
			var SdkMainVersion = SdkVersion & 0xFFFF0000;
			if (!ValidMainVersions.Contains(SdkMainVersion))
			{
				Console.WriteLine("{0} unknown SDK : {1:X8}\n", Name, SdkVersion);
			}
			_sceKernelSetCompiledSdkVersion(SdkVersion);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Param"></param>
		[HlePspFunction(NID = 0x7591C7DB, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion(uint SdkVersion)
		{
			_sceKernelSetCompiledSdkVersion(SdkVersion);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Param"></param>
		[HlePspFunction(NID = 0x342061E5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion370(uint SdkVersion)
		{
			_sceKernelSetCompiledSdkVersion(SdkVersion, "sceKernelSetCompiledSdkVersion370", new uint[] { 0x3070000 });
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Param"></param>
		[HlePspFunction(NID = 0x315AD3A0, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion380_390(uint Param)
		{
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Param"></param>
        [HlePspFunction(NID = 0x358CA1BB, FirmwareVersion = 660)]
        [HlePspNotImplemented]
        public void sceKernelSetCompiledSdkVersion660(uint Param)
        {
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Param"></param>
		[HlePspFunction(NID = 0xEBD5C3E6, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion395(uint Param)
		{
		}

		/// <summary>
		/// Get the size of the largest free memory block.
		/// </summary>
		/// <returns>The size of the largest free memory block, in bytes.</returns>
		[HlePspFunction(NID = 0xA291F107, FirmwareVersion = 150)]
		public int sceKernelMaxFreeMemSize()
		{
			//foreach (var Partition in MemoryManager.RootPartition.ChildPartitions) Console.WriteLine(Partition);
			//return 24 * 1024 * 1024;

			//return (MemoryManager.GetPartition(MemoryPartitions.User).MaxFreeSize - 0x40000) & ~15;
			return (MemoryManager.GetPartition(MemoryPartitions.User).MaxFreeSize) & ~15;
		}

		/// <summary>
		/// Get the total amount of free memory.
		/// </summary>
		/// <returns>The total amount of free memory, in bytes.</returns>
		[HlePspFunction(NID = 0xF919F628, FirmwareVersion = 150)]
		public int sceKernelTotalFreeMemSize()
		{
			return MemoryManager.GetPartition(MemoryPartitions.User).TotalFreeSize - 0x8000;
		}

		/// <summary>
		/// Get the address of a memory block.
		/// </summary>
		/// <param name="BlockId">UID of the memory block.</param>
		/// <returns>The lowest address belonging to the memory block.</returns>
		[HlePspFunction(NID = 0x9D9A5BA1, FirmwareVersion = 150)]
		public uint sceKernelGetBlockHeadAddr(int BlockId)
		{
			if (BlockId == 0) return 0;
			if (!MemoryManager.MemoryPartitionsUid.Contains(BlockId)) return 0;
			return MemoryManager.MemoryPartitionsUid.Get(BlockId).Low;
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
		public int sceKernelAllocPartitionMemory(MemoryPartitions PartitionId, string Name, HleMemoryManager.BlockTypeEnum Type, int Size, /* void* */uint Address)
		{
			if ((int)PartitionId <= 0 || (int)PartitionId == 7 || (int)PartitionId >= 10) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_ARGUMENT));
			if (Size <= 0) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_FAILED_ALLOC_MEMBLOCK));

			//Console.Error.WriteLineColored(ConsoleColor.Yellow, "sceKernelAllocPartitionMemory: {0}, {1}, {2}, {3}, {4}", PartitionId, MemoryManager.GetPartition(PartitionId).MaxFreeSize, Name, Type, Size);
			
			if (Name == null) throw (new SceKernelException(SceKernelErrors.ERROR_ERROR));

			MemoryPartition MemoryPartition;
			int Alignment = 1;
			switch (Type)
			{
				case HleMemoryManager.BlockTypeEnum.HighAligned:
				case HleMemoryManager.BlockTypeEnum.LowAligned:
					Alignment = (int)Address;
					break;
			}

			if ((Alignment == 0) || !MathUtils.IsPowerOfTwo((uint)Alignment))
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_ALIGNMENT_SIZE));
			}

			try
			{
				switch (Type)
				{
					case HleMemoryManager.BlockTypeEnum.Low:
					case HleMemoryManager.BlockTypeEnum.LowAligned:
						MemoryPartition = MemoryManager.GetPartition(PartitionId).Allocate(Size, MemoryPartition.Anchor.Low, Alignment: Alignment, Name: Name );
						break;
					case HleMemoryManager.BlockTypeEnum.High:
					case HleMemoryManager.BlockTypeEnum.HighAligned:
						MemoryPartition = MemoryManager.GetPartition(PartitionId).Allocate(Size, MemoryPartition.Anchor.High, Alignment: Alignment, Name: Name);
						break;
					case HleMemoryManager.BlockTypeEnum.Address:
						if (Address == 0) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_FAILED_ALLOC_MEMBLOCK));
						MemoryPartition = MemoryManager.GetPartition(PartitionId).Allocate(Size, MemoryPartition.Anchor.Low, Position: Address, Alignment: Alignment, Name: Name);
						break;
					default:
						Console.Error.WriteLineColored(ConsoleColor.Red, "Not Implemented sceKernelAllocPartitionMemory with Type='" + Type + "'");
						throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMBLOCK_ALLOC_TYPE));
				}
			}
			catch (MemoryPartitionNoMemoryException MemoryPartitionNoMemoryException)
			{
				//Console.Error.WriteLine(InvalidOperationException);
				Console.Error.WriteLine(MemoryPartitionNoMemoryException);
				throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_NO_MEMORY));
			}

			//Console.Error.WriteLineColored(ConsoleColor.Cyan, "  sceKernelAllocPartitionMemory: {0}", MemoryPartition);

			return (int)MemoryManager.MemoryPartitionsUid.Create(MemoryPartition);
			//return MemoryPartition;
		}


		/// <summary>
		/// Free a memory block allocated with <see cref="sceKernelAllocPartitionMemory"/>.
		/// </summary>
		/// <param name="BlockId">UID of the block to free.</param>
		/// <returns>? on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB6D61D02, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceKernelFreePartitionMemory(int BlockId)
		{
			if (BlockId == 0) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_UNKNOWN_UID));
			if (!MemoryManager.MemoryPartitionsUid.Contains(BlockId)) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_UNKNOWN_UID));
			var MemoryPartition = MemoryManager.MemoryPartitionsUid.Get(BlockId);
			//Console.Error.WriteLine(MemoryPartition.ParentPartition.ChildPartitions.Where(Partition => Partition));
			//Console.Error.WriteLine(":[1]:" + sceKernelTotalFreeMemSize());
			MemoryPartition.ParentPartition.DeallocateLow(MemoryPartition.Low);
			MemoryManager.MemoryPartitionsUid.Remove(BlockId);
			//Console.Error.WriteLine(":[2]:" + sceKernelTotalFreeMemSize());
			//reinterpret!(MemorySegment)(blockid).free();
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Format"></param>
		/// <param name="CpuThreadState"></param>
		[HlePspFunction(NID = 0x13A5ABEF, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public void sceKernelPrintf(string Format, CpuThreadState CpuThreadState)
		{
			KDebugForKernel.Kprintf(Format, CpuThreadState);
			//throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0xfc114573, FirmwareVersion = 150)]
		public uint sceKernelGetCompiledSdkVersion()
		{
			if ((HleConfig.SdkFlags & SdkFlags.SCE_KERNEL_HASCOMPILEDSDKVERSION) != 0)
			{
				return HleConfig.CompiledSdkVersion;
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SdkVersion"></param>
		[HlePspFunction(NID = 0x91DE343C, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion500_505(uint SdkVersion)
		{
			_sceKernelSetCompiledSdkVersion(SdkVersion, "sceKernelSetCompiledSdkVersion500_505", new uint[] { 0x5000000, 0x5050000 });
		}

		[HlePspFunction(NID = 0xFE707FDF, FirmwareVersion = 150, Name = "SysMemUserForUser_FE707FDF")]
		[HlePspNotImplemented]
		public int AllocMemoryBlock(string Name, uint Type, uint Size, uint ParamsAddrPtr)
		{
			return 0;
		}

		[HlePspFunction(NID = 0xDB83A952, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void SysMemUserForUser_DB83A952()
		{
		}

		[HlePspFunction(NID = 0x50F61D8A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void SysMemUserForUser_50F61D8A()
		{
		}

		[HlePspFunction(NID = 0x1B4217BC, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceKernelSetCompiledSdkVersion603_605(uint SdkVersion)
		{
			_sceKernelSetCompiledSdkVersion(SdkVersion, "sceKernelSetCompiledSdkVersion603_605", new uint[] { 0x6040000, 0x6030000, 0x6050000 });
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		public enum VariablePoolId : int { }

		public class VariablePool
		{
			public SceKernelVplInfo Info;

			public void Init()
			{
				throw new NotImplementedException();
			}
		}

		HleUidPoolSpecial<VariablePool, VariablePoolId> VariablePoolList = new HleUidPoolSpecial<VariablePool, VariablePoolId>()
		{
			OnKeyNotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_VPOOL,
		};

		/// <summary>
		/// Create a variable pool
		/// </summary>
		/// <param name="Name">Name of the pool</param>
		/// <param name="PartitionId">The memory partition ID</param>
		/// <param name="Attribute">Attributes</param>
		/// <param name="Size">Size of pool</param>
		/// <param name="Options">Options (set to NULL)</param>
		/// <returns>
		///		 The UID of the created pool
		///		 less than 0 on error.
		/// </returns>
		[HlePspFunction(NID = 0x56C039B5, FirmwareVersion = 150)]
		public VariablePoolId sceKernelCreateVpl(string Name, HleMemoryManager.Partitions PartitionId, VplAttribute Attribute, int Size, void* Options)
		{
			var VariablePool = new VariablePool()
			{
				Info = new SceKernelVplInfo()
				{
					Attribute = Attribute,
					FreeSize = Size,
				}
			};

			/*
			if (Attribute.HasFlag(VplAttribute.PSP_VPL_ATTR_ADDR_HIGH))
			{
				HleState.MemoryManager.GetPartition(PartitionId).Allocate(Size)
			} else {
			}
			*/
			VariablePool.Init();

			return VariablePoolList.Create(VariablePool);

			/*
			throw(new NotImplementedException());
			const PSP_VPL_ATTR_MASK      = 0x41FF;  // Anything outside this mask is an illegal attr.
			const PSP_VPL_ATTR_ADDR_HIGH = 0x4000;  // Create the vpl in high memory.
			const PSP_VPL_ATTR_EXT       = 0x8000;  // Extend the vpl memory area (exact purpose is unknown).
			//new MemorySegment
			logWarning("sceKernelCreateVpl('%s', %d, %d, %d)", name, part, attr, size);
			VariablePool variablePool;
			if (attr & PSP_VPL_ATTR_ADDR_HIGH) {
				variablePool = new VariablePool(hleEmulatorState.moduleManager.get!SysMemUserForUser()._allocateMemorySegmentHigh(part, dupStr(name), size));
			} else {
				variablePool = new VariablePool(hleEmulatorState.moduleManager.get!SysMemUserForUser()._allocateMemorySegmentLow(part, dupStr(name), size));
			}
			logWarning("%s", variablePool);
			return uniqueIdFactory.add(variablePool);
			*/
		}

		/// <summary>
		/// Allocate from the pool
		/// </summary>
		/// <param name="VariablePoolId">The UID of the pool</param>
		/// <param name="Size">The size to allocate</param>
		/// <param name="Data">Receives the address of the allocated data</param>
		/// <param name="Timeout">Amount of time to wait for allocation?</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xD979E9BF, FirmwareVersion = 150)]
		public int sceKernelAllocateVpl(VariablePoolId VariablePoolId, uint Size, PspAddress* DataPointer, uint* Timeout)
		{
			throw(new NotImplementedException());
			/*
			logWarning("sceKernelAllocateVpl(%d, %d, %08X) @TODO Not waiting", uid, size, cast(uint)data);
			return sceKernelTryAllocateVpl(uid, size, data);
			*/
		}

		/// <summary>
		/// Allocate from the pool (Handling Callbacks)
		/// </summary>
		/// <param name="VariablePoolId">The UID of the pool</param>
		/// <param name="Size">The size to allocate</param>
		/// <param name="Data">Receives the address of the allocated data</param>
		/// <param name="Timeout">Amount of time to wait for allocation?</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xEC0A693F, FirmwareVersion = 150)]
		public int sceKernelAllocateVplCB(VariablePoolId VariablePoolId, uint Size, PspAddress* DataPointer, uint* Timeout)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Try to allocate from the pool 
		/// </summary>
		/// <param name="VariablePoolId">The UID of the pool</param>
		/// <param name="Size">The size to allocate</param>
		/// <param name="Data">Receives the address of the allocated data</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xAF36D708, FirmwareVersion = 150)]
		public int sceKernelTryAllocateVpl(VariablePoolId VariablePoolId, uint Size, PspAddress* DataPointer)
		{
			throw(new NotImplementedException());
			/*
			logWarning("sceKernelTryAllocateVpl(%d, %d, %08X)", uid, size, cast(uint)data);
			VariablePool variablePool = uniqueIdFactory.get!VariablePool(uid);
			*data = cast(uint *)variablePool.memorySegment.allocByLow(size).block.low;
			logWarning(" <<<---", *data);
			//unimplemented();
			return 0;
			*/
		}

		/// <summary>
		/// Get the status of an VPL
		/// </summary>
		/// <param name="VariablePoolId">The uid of the VPL</param>
		/// <param name="Info">Pointer to a ::SceKernelVplInfo structure</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x39810265, FirmwareVersion = 150)]
		public int sceKernelReferVplStatus(VariablePoolId VariablePoolId, SceKernelVplInfo* Info)
		{
			var VariablePool = VariablePoolList.Get(VariablePoolId);
			*Info = VariablePool.Info;
			return 0;
		}

		/// <summary>
		/// Free a block
		/// </summary>
		/// <param name="VariablePoolId">The UID of the pool</param>
		/// <param name="Data">The data block to deallocate</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xB736E9FF, FirmwareVersion = 150)]
		public int sceKernelFreeVpl(VariablePoolId VariablePoolId, PspAddress Data)
		{
			throw (new NotImplementedException());
			/*
			unimplemented();
			return -1;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		public enum VplAttribute : uint
		{
			/// <summary>
			/// Anything outside this mask is an illegal attr.
			/// </summary>
			PSP_VPL_ATTR_MASK      = 0x41FF,

			/// <summary>
			/// Create the vpl in high memory.
			/// </summary>
			PSP_VPL_ATTR_ADDR_HIGH = 0x4000,

			/// <summary>
			/// Extend the vpl memory area (exact purpose is unknown).
			/// </summary>
			PSP_VPL_ATTR_EXT       = 0x8000,
		}
		/// <summary>
		/// 
		/// </summary>
		public struct SceKernelVplInfo
		{
			/// <summary>
			/// 
			/// </summary>
			public uint StructSize;

			/// <summary>
			/// 
			/// </summary>
			public fixed byte Name[32];

			/// <summary>
			/// 
			/// </summary>
			public VplAttribute Attribute;

			/// <summary>
			/// 
			/// </summary>
			public int PoolSize;

			/// <summary>
			/// 
			/// </summary>
			public int FreeSize;

			/// <summary>
			/// 
			/// </summary>
			public int NumWaitThreads;
		}
	}
}

using System;
using System.Collections.Generic;
using CSharpUtils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.threadman
{
	public unsafe partial class ThreadManForUser
	{
		[HleUidPoolClass(FirstItem = 1, NotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_VPOOL)]
		public class VariablePool : IDisposable, IHleUidPoolClass
		{
			public ThreadManForUser ThreadManForUser;
			public MemoryPartitions PartitionId;
			public SceKernelVplInfo Info;
			public MemoryPartition MemoryPartition;
			public MemoryPartition.Anchor InternalMemoryAnchor;
			public MemoryPartition.Anchor InternalMemoryAnchorReturn;
			public MemoryPartition.Anchor ExternalMemoryAnchor;

			public class WaitVariablePoolItem
			{
				public int RequiredSize;
				public Action WakeUp;
			}

			public List<WaitVariablePoolItem> WaitList = new List<WaitVariablePoolItem>();

			public VariablePool(ThreadManForUser ThreadManForUser)
			{
				this.ThreadManForUser = ThreadManForUser;
			}

			public void Init()
			{
				var High = Info.Attribute.HasFlag(VplAttributeEnum.PSP_VPL_ATTR_ADDR_HIGH);

#if true
				ExternalMemoryAnchor = High ? Hle.MemoryPartition.Anchor.High : Hle.MemoryPartition.Anchor.Low;
				//InternalMemoryAnchor = Hle.MemoryPartition.Anchor.High;
				InternalMemoryAnchor = Hle.MemoryPartition.Anchor.Low;
				InternalMemoryAnchorReturn = Hle.MemoryPartition.Anchor.Low;
#else
				InternalMemoryAnchor = High ? Hle.MemoryPartition.Anchor.High : Hle.MemoryPartition.Anchor.Low;
				ExternalMemoryAnchor = Hle.MemoryPartition.Anchor.Low;
#endif

				this.MemoryPartition = ThreadManForUser.MemoryManager.GetPartition(PartitionId).Allocate(
					Info.PoolSize,
					ExternalMemoryAnchor,
					Name: "<Vpl> : " + Info.Name
				);
			}

			public void Allocate(CpuThreadState CpuThreadState, int Size, PspPointer* AddressPointer, uint* Timeout, bool HandleCallbacks)
			{
				if (!TryAllocate(CpuThreadState, Size, AddressPointer))
				{
					bool TimedOut = false;

					ThreadManForUser.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.Semaphore, "_sceKernelAllocateVplCB", this, (WakeUp) =>
					{
						ThreadManForUser.PspRtc.RegisterTimeout(Timeout, () =>
						{
							TimedOut = true;
							WakeUp();
						});
						WaitList.Add(new WaitVariablePoolItem()
						{
							RequiredSize = Size,
							WakeUp = () => {
								WakeUp();
								Allocate(CpuThreadState, Size, AddressPointer, Timeout, HandleCallbacks);
							},
						});
					}, HandleCallbacks: HandleCallbacks);

					if (TimedOut) throw(new SceKernelException(SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT));
				}
			}

			public bool TryAllocate(CpuThreadState CpuThreadState, int Size, PspPointer* AddressPointer)
			{
				Size = (int)MathUtils.NextAligned(Size, 4);
				if (Size > Info.PoolSize) throw (new SceKernelException((SceKernelErrors)(-1)));
				try
				{
					var AllocatedSegment = MemoryPartition.Allocate(Size, InternalMemoryAnchor);
					AddressPointer->Address = AllocatedSegment.GetAnchoredAddress(InternalMemoryAnchorReturn);
					return true;
				}
				catch (MemoryPartitionNoMemoryException)
				{
					//AddressPointer->Address = 0;
					return false;
				}
			}

			public void Free(CpuThreadState CpuThreadState, PspPointer Address)
			{
				MemoryPartition.DeallocateAnchoredAddress(Address, InternalMemoryAnchorReturn);

				var TotalFreeSize = MemoryPartition.TotalFreeSize;
				foreach (var Item in WaitList.ToArray())
				{
					if (TotalFreeSize >= Item.RequiredSize)
					{
						WaitList.Remove(Item);
						Item.WakeUp();
						break;
					}
				}
			}

			void IDisposable.Dispose()
			{
				if (this.MemoryPartition != null)
				{
					this.MemoryPartition.DeallocateFromParent();
					this.MemoryPartition = null;
				}
			}
		}

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
		public VariablePool sceKernelCreateVpl(string Name, MemoryPartitions PartitionId, VplAttributeEnum Attribute, int Size, void* Options)
		{
			var VariablePool = new VariablePool(this)
			{
				PartitionId= PartitionId,
				Info = new SceKernelVplInfo()
				{
					Attribute = Attribute,
					PoolSize = Size,
					FreeSize = Size,
				}
			};

			VariablePool.Init();

			return VariablePool;
		}

		/// <summary>
		/// Allocate from the pool
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <param name="VariablePool">The UID of the pool</param>
		/// <param name="Size">The size to allocate</param>
		/// <param name="AddressPointer">Receives the address of the allocated data</param>
		/// <param name="Timeout">Amount of time to wait for allocation?</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xBED27435, FirmwareVersion = 150)]
		public int sceKernelAllocateVpl(CpuThreadState CpuThreadState, VariablePool VariablePool, int Size, PspPointer* AddressPointer, uint* Timeout)
		{
			VariablePool.Allocate(CpuThreadState, Size, AddressPointer, Timeout, HandleCallbacks: false);
			return 0;
		}

		/// <summary>
		/// Allocate from the pool (Handling Callbacks)
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <param name="VariablePool">The UID of the pool</param>
		/// <param name="Size">The size to allocate</param>
		/// <param name="AddressPointer">Receives the address of the allocated data</param>
		/// <param name="Timeout">Amount of time to wait for allocation?</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xEC0A693F, FirmwareVersion = 150)]
		public int sceKernelAllocateVplCB(CpuThreadState CpuThreadState, VariablePool VariablePool, int Size, PspPointer* AddressPointer, uint* Timeout)
		{
			VariablePool.Allocate(CpuThreadState, Size, AddressPointer, Timeout, HandleCallbacks: true);
			return 0;
		}

		/// <summary>
		/// Try to allocate from the pool 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <param name="VariablePool">The UID of the pool</param>
		/// <param name="Size">The size to allocate</param>
		/// <param name="AddressPointer">Receives the address of the allocated data</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xAF36D708, FirmwareVersion = 150)]
		public int sceKernelTryAllocateVpl(CpuThreadState CpuThreadState, VariablePool VariablePool, int Size, PspPointer* AddressPointer)
		{
			if (VariablePool.TryAllocate(CpuThreadState, Size, AddressPointer))
			{
				return 0;
			}
			else
			{
				return (int)SceKernelErrors.ERROR_KERNEL_NO_MEMORY;
			}
		}

		/// <summary>
		/// Get the status of an VPL
		/// </summary>
		/// <param name="VariablePool">The uid of the VPL</param>
		/// <param name="Info">Pointer to a <see cref="SceKernelVplInfo"/> structure</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x39810265, FirmwareVersion = 150)]
		public int sceKernelReferVplStatus(VariablePool VariablePool, SceKernelVplInfo* Info)
		{
			*Info = VariablePool.Info;
			return 0;
		}

		/// <summary>
		/// Free a block
		/// </summary>
		///<param name="CpuThreadState"></param>
		///<param name="VariablePool">The UID of the pool</param>
		/// <param name="Data">The data block to deallocate</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xB736E9FF, FirmwareVersion = 150)]
		public int sceKernelFreeVpl(CpuThreadState CpuThreadState, VariablePool VariablePool, PspPointer Data)
		{
			VariablePool.Free(CpuThreadState, Data);
			return 0;
		}

		/// <summary>
		/// Delete a variable pool
		/// </summary>
		/// <param name="VariablePool">The UID of the pool</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x89B3D48C, FirmwareVersion = 150)]
		public int sceKernelDeleteVpl(VariablePool VariablePool)
		{
			VariablePool.RemoveUid(InjectContext);
			return 0;
		}


		/// <summary>
		/// 
		/// </summary>
		[Flags]
		public enum VplAttributeEnum : uint
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
		/// Variable pool status info.
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
			public fixed byte RawName[32];

			/// <summary>
			/// 
			/// </summary>
			public String Name { get { fixed (byte* Pointer = RawName) return PointerUtils.PtrToStringUtf8(Pointer); } }

			/// <summary>
			/// 
			/// </summary>
			public VplAttributeEnum Attribute;

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

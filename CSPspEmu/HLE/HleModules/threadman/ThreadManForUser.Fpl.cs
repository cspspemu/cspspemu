using System;
using System.Collections.Generic;
using System.Linq;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.threadman
{
    public unsafe partial class ThreadManForUser
    {
        [Flags]
        public enum FplAttributes : uint
        {
            /// <summary>
            /// 
            /// </summary>
            PSP_FPL_ATTR_FIFO = 0,

            /// <summary>
            /// 
            /// </summary>
            PSP_FPL_ATTR_PRIORITY = 0x100,

            /// <summary>
            /// Create the fpl in high memory.
            /// </summary>
            PSP_FPL_ATTR_ADDR_HIGH = 0x4000,
        }

        public struct FplOptionsStruct
        {
            public int StructSize;
            public int Alignment;
        }

        [HleUidPoolClass(FirstItem = 1, NotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_FPOOL)]
        public class FixedPool : IDisposable, IHleUidPoolClass
        {
            public class WaitItem
            {
                public HleThread Thread;
                public Action WakeUp;
            }

            public ThreadManForUser ThreadManForUser;
            public HleMemoryManager MemoryManager;
            public string Name;
            public MemoryPartitions PartitionId;
            public FplAttributes Attributes;
            public int BlockSize;
            public int NumberOfBlocks;
            public MemoryPartition MemoryPartition;
            public List<uint> FreeBlocks;
            public List<uint> UsedBlocks;
            public List<WaitItem> WaitItemList;
            public FplOptionsStruct Options;

            public FixedPool(ThreadManForUser ThreadManForUser)
            {
                this.ThreadManForUser = ThreadManForUser;
            }

            public void Init()
            {
                var Alignment = Options.Alignment;
                if (Alignment == 0) Alignment = 4;
                //if (Alignment == 0) Alignment = 0x1000;
                if (Attributes != (FplAttributes) 0)
                {
                    Console.Error.WriteLine("FPL: Unhandled Attribute : {0}", Attributes);
                    //throw (new NotImplementedException());
                }

                if (BlockSize == 0)
                {
                    throw new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMSIZE);
                }

                var Partition = MemoryManager.GetPartition(PartitionId);

                //var TEST_FIXED_ADDRESS = 0x08800000U;
                //var TEST_FIXED_ADDRESS = 0x08865980U;
                //var TEST_FIXED_ADDRESS = 0x088A0000U;
                //this.MemoryPartition = Partition.Allocate(NumberOfBlocks * BlockSize, Hle.MemoryPartition.Anchor.Set, TEST_FIXED_ADDRESS, Alignment);
                //Partition.Dump();

                try
                {
                    this.MemoryPartition = Partition.Allocate(
                        NumberOfBlocks * BlockSize,
                        Hle.MemoryPartition.Anchor.Low,
                        0,
                        Alignment,
                        "<Fpl>: " + Name
                    );
                }
                catch (MemoryPartitionNoMemoryException)
                {
                    throw new SceKernelException(SceKernelErrors.ERROR_KERNEL_NO_MEMORY);
                }

                //Console.Error.WriteLine("FixedPool.Init: 0x{0:X}", this.MemoryPartition.Low);
                this.FreeBlocks = new List<uint>();
                this.UsedBlocks = new List<uint>();
                this.WaitItemList = new List<WaitItem>();
                for (int n = 0; n < NumberOfBlocks; n++)
                {
                    this.FreeBlocks.Add(GetAddressFromBlockIndex(n));
                }


                //Console.Error.WriteLine(this);
            }

            public uint GetAddressFromBlockIndex(int Index)
            {
                return (uint) (MemoryPartition.Low + Index * BlockSize);
            }

            public void Allocate(PspPointer* DataPointer, uint* Timeout, bool HandleCallbacks)
            {
                if (!TryAllocate(DataPointer))
                {
                    if (Timeout != null) throw new NotImplementedException();
                    var CurrentThread = ThreadManForUser.ThreadManager.Current;
                    CurrentThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Semaphore, "_sceKernelAllocateVplCB", this,
                        (WakeUp) =>
                        {
                            WaitItemList.Add(new WaitItem()
                            {
                                Thread = CurrentThread,
                                WakeUp = () =>
                                {
                                    WakeUp();
                                    Allocate(DataPointer, Timeout, HandleCallbacks);
                                },
                            });
                        }, HandleCallbacks: HandleCallbacks);
                }
            }

            public bool TryAllocate(PspPointer* DataPointer)
            {
                if (FreeBlocks.Count > 0)
                {
                    var AllocatedBlock = FreeBlocks.First();
                    FreeBlocks.Remove(AllocatedBlock);
                    UsedBlocks.Add(AllocatedBlock);
                    //Console.Error.WriteLine("TryAllocate(0x{0:X})", AllocatedBlock);
                    DataPointer->Address = AllocatedBlock;

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Free(PspPointer DataPointer)
            {
                if (!UsedBlocks.Contains(DataPointer))
                {
                    throw new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMBLOCK);
                }
                UsedBlocks.Remove(DataPointer);
                FreeBlocks.Add(DataPointer);

                foreach (var WaitItem in WaitItemList.ToArray())
                {
                    //Console.Error.WriteLine("Free!");
                    WaitItemList.Remove(WaitItem);
                    WaitItem.WakeUp();
                    ThreadManForUser.ThreadManager.Current.CpuThreadState.Yield();
                    break;
                }
            }

            public override string ToString()
            {
                return this.ToStringDefault();
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
        public FixedPool sceKernelCreateFpl(string Name, MemoryPartitions PartitionId, FplAttributes Attributes,
            int BlockSize, int NumberOfBlocks, FplOptionsStruct* Options)
        {
            var FixedPool = new FixedPool(this)
            {
                MemoryManager = MemoryManager,
                Name = Name,
                PartitionId = PartitionId,
                Attributes = Attributes,
                BlockSize = BlockSize,
                NumberOfBlocks = NumberOfBlocks,
            };
            if (Options != null) FixedPool.Options = *Options;
            FixedPool.Init();

            return FixedPool;
        }

        /// <summary>
        /// Try to allocate from the pool immediately.
        /// </summary>
        /// <param name="FixedPool">The UID of the pool</param>
        /// <param name="DataPointer">Receives the address of the allocated data</param>
        /// <returns>0 on success, less than 0 on error</returns>
        [HlePspFunction(NID = 0x623AE665, FirmwareVersion = 150)]
        public int sceKernelTryAllocateFpl(FixedPool FixedPool, PspPointer* DataPointer)
        {
            if (!FixedPool.TryAllocate(DataPointer))
            {
                throw new SceKernelException(SceKernelErrors.ERROR_KERNEL_NO_MEMORY);
            }

            return 0;
        }

        /// <summary>
        /// Allocate from the pool. It will wait for a free block to be available the specified time.
        /// </summary>
        /// <param name="FixedPool">The UID of the pool</param>
        /// <param name="DataPointer">Receives the address of the allocated data</param>
        /// <param name="Timeout">Amount of time to wait for allocation?</param>
        /// <returns>0 on success, less than 0 on error</returns>
        [HlePspFunction(NID = 0xD979E9BF, FirmwareVersion = 150)]
        public int sceKernelAllocateFpl(FixedPool FixedPool, PspPointer* DataPointer, uint* Timeout)
        {
            FixedPool.Allocate(DataPointer, Timeout, HandleCallbacks: false);

            //Console.WriteLine("Allocated: Address: 0x{0:X}", DataPointer->Address);
            return 0;
        }

        /// <summary>
        /// Allocate from the pool (with callback)
        /// </summary>
        /// <param name="FixedPool">The UID of the pool</param>
        /// <param name="DataPointer">Receives the address of the allocated data</param>
        /// <param name="Timeout">Amount of time to wait for allocation?</param>
        /// <returns>0 on success, less than 0 on error</returns>
        [HlePspFunction(NID = 0xE7282CB6, FirmwareVersion = 150)]
        public int sceKernelAllocateFplCB(FixedPool FixedPool, PspPointer* DataPointer, uint* Timeout)
        {
            FixedPool.Allocate(DataPointer, Timeout, HandleCallbacks: true);

            //Console.WriteLine("Allocated: Address: 0x{0:X}", DataPointer->Address);
            return 0;
        }

        /// <summary>
        /// Free a block
        /// </summary>
        /// <param name="FixedPool">The UID of the pool</param>
        /// <param name="DataPointer">The data block to deallocate</param>
        /// <returns>
        ///		0 on success
        ///		less than 0 on error
        /// </returns>
        [HlePspFunction(NID = 0xF6414A71, FirmwareVersion = 150)]
        public int sceKernelFreeFpl(FixedPool FixedPool, PspPointer DataPointer)
        {
            //if (!FixedPoolList.Contains(FixedPool)) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMBLOCK));
            FixedPool.Free(DataPointer);
            return 0;
        }

        /// <summary>
        /// Delete a fixed pool
        /// </summary>
        /// <param name="FixedPool">The UID of the pool</param>
        /// <returns>
        ///		0 on success
        ///		less than 0 on error
        /// </returns>
        [HlePspFunction(NID = 0xED1410E0, FirmwareVersion = 150)]
        public int sceKernelDeleteFpl(FixedPool FixedPool)
        {
            FixedPool.RemoveUid(InjectContext);
            return 0;
        }
    }
}
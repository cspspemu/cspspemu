using CSPspEmu.Core;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Managers
{
	public class HleMemoryManager : PspEmulatorComponent
	{
		/// <summary>
		/// Specifies the type of allocation used for memory blocks.
		/// </summary>
		public enum BlockTypeEnum : uint
		{
			/// <summary>
			/// Allocate from the lowest available address.
			/// </summary>
			Low = 0,

			/// <summary>
			/// Allocate from the highest available address.
			/// </summary>
			High = 1,

			/// <summary>
			/// Allocate from the specified address.
			/// </summary>
			Address = 2,

			/// <summary>
			/// 
			/// </summary>
			LowAligned = 3,

			/// <summary>
			/// 
			/// </summary>
			HighAligned = 4,
		}

		public enum Partitions : int
		{
			Kernel0 = 0,
			Kernel1 = 1,
			User = 2,
			VolatilePartition = 3,
			UserStacks = 12,
		}

		//public MemoryPartition RootPartition = new MemoryPartition(PspMemory.MainOffset, PspMemory.MainOffset + PspMemory.MainSize);
		[Inject]
		public PspMemory Memory;

		public HleUidPool<MemoryPartition> MemoryPartitionsUid = new HleUidPool<MemoryPartition>();

		public MemoryPartition GetPartition(Partitions Partition)
		{
			return MemoryPartitionsUid.Get((int)Partition);
		}

		public override void InitializeComponent()
		{
#if true
				MemoryPartitionsUid.Set(0, new MemoryPartition(Low: 0x88000000, High: 0x88300000, Allocated: false, Name: "Kernel Partition 1")); // 3MB
				MemoryPartitionsUid.Set(1, new MemoryPartition(Low: 0x88300000, High: 0x88400000, Allocated: false, Name: "Kernel Partition 2")); // 1MB
				//MemoryPartitionsUid.Set(2, new MemoryPartition(Low: 0x08800000, High: 0x0A000000, Allocated: false, Name: "User Partition")); // 24MB
				MemoryPartitionsUid.Set(2, new MemoryPartition(Low: 0x08800000, High: 0x0B000000, Allocated: false, Name: "User Partition")); // 24MB
				MemoryPartitionsUid.Set(12, new MemoryPartition(Low: 0x08800000, High: 0x0B000000, Allocated: false, Name: "User Stacks Partition")); // 24MB
				MemoryPartitionsUid.Set(3, new MemoryPartition(Low: 0x08400000, High: 0x08800000, Allocated: false, Name: "Volatile Partition")); // 4MB
				MemoryPartitionsUid.Set(4, new MemoryPartition(Low: 0x8A000000, High: 0x8BC00000, Allocated: false, Name: "UMD Cache Partition")); // 28MB
				MemoryPartitionsUid.Set(5, new MemoryPartition(Low: 0x8BC00000, High: 0x8C000000, Allocated: false, Name: "ME Partition")); // 4MB
#else
				MemoryPartitionsUid.Set(0, new MemoryPartition(Low: 0x88000000, High: 0x88300000, Allocated: false, Name: "Kernel Partition 1")); // 3MB
				MemoryPartitionsUid.Set(1, new MemoryPartition(Low: 0x88300000, High: 0x88400000, Allocated: false, Name: "Kernel Partition 2")); // 1MB
				MemoryPartitionsUid.Set(2, new MemoryPartition(Low: 0x08800000, High: 0x0B000000, Allocated: false, Name: "User Partition")); // 48MB
#endif
		}
	}
}

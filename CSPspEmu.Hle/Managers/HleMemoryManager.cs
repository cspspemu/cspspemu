using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSharpUtils;

namespace CSPspEmu.Hle.Managers
{
	public class HleMemoryManager
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

		public MemoryPartition RootPartition = new MemoryPartition(PspMemory.MainOffset, PspMemory.MainOffset + PspMemory.MainSize);
		public PspMemory Memory;
		public HleUidPool<MemoryPartition> MemoryPartitionsUid = new HleUidPool<MemoryPartition>();

		public HleMemoryManager(PspMemory Memory)
		{
			this.Memory = Memory;
		}
	}
}

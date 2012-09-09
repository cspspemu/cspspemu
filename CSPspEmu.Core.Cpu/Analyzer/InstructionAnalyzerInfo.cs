using System.Collections.Generic;
using CSPspEmu.Core.Cpu.Table;

namespace CSPspEmu.Core.Cpu.Analyzer
{
	public class InstructionAnalyzerInfo
	{
		public enum JumpTypeEnum
		{
			NoJump,
			JumpAlways,
			JumpSometimes,
		}

		/// <summary>
		/// 
		/// </summary>
		public InstructionInfo InstructionInfo;

		/// <summary>
		/// This instruction performs a memory transfer.
		/// </summary>
		public bool MemoryTransfer;

		/// <summary>
		/// 
		/// </summary>
		public HashSet<int> RegistersWritten;

		/// <summary>
		/// 
		/// </summary>
		public HashSet<int> RegistersReaded;

		/// <summary>
		/// 
		/// </summary>
		public JumpTypeEnum JumpType;
	}
}

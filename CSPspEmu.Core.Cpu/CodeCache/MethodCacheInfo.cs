using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.CodeCache
{
	public class MethodCacheInfo
	{
		/// <summary>
		/// Index in MethodCache.Methods
		/// </summary>
		public int MethodIndex;

		/// <summary>
		/// 
		/// </summary>
		public bool FollowPspCallingConventions;

		/// <summary>
		/// EntryPoint for this function.
		/// </summary>
		public uint EntryPC;

		/// <summary>
		/// Address of the start of the function. Usually is equal to EntryPC but not always.
		/// </summary>
		public uint MinPC;

		/// <summary>
		/// Last address with code for this function.
		/// </summary>
		public uint MaxPC;

		/// <summary>
		/// Ast for this function.
		/// </summary>
		public AstNodeStm AstTree;

		/// <summary>
		/// Function that will be executed.
		/// </summary>
		public Action<CpuThreadState> Delegate;
	}
}

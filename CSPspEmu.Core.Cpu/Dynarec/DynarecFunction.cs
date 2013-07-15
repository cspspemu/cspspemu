using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	/// <summary>
	/// Class that represents a PSP ALLEGREX function converted into a .NET IL function.
	/// </summary>
	public class DynarecFunction
	{
		public string Name;

		/// <summary>
		/// 
		/// </summary>
		public AstNodeStm AstNode;

		/// <summary>
		/// Delegate to execute this function.
		/// </summary>
		public Action<CpuThreadState> Delegate;

		/// <summary>
		/// A list of functions that have embedded this function.
		/// </summary>
		public List<DynarecFunction> InlinedAtFunctions;

		/// <summary>
		/// 
		/// </summary>
		public uint[] CallingPCs;
		
		/// <summary>
		/// 
		/// </summary>
		public uint EntryPC;

		/// <summary>
		/// 
		/// </summary>
		public uint MinPC;

		/// <summary>
		/// 
		/// </summary>
		public uint MaxPC;

		/// <summary>
		/// 
		/// </summary>
		public TimeSpan TimeAnalyzeBranches;
		public TimeSpan TimeGenerateCode;
		public TimeSpan TimeCreateDelegate;
	}
}

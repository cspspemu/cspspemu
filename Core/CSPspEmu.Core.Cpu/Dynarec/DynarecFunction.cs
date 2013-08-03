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
		/// <summary>
		/// Special function name.
		/// </summary>
		public string Name;

		/// <summary>
		/// Root node containing the whole function code.
		/// </summary>
		public AstNodeStm AstNode;

		/// <summary>
		/// Delegate to execute this function.
		/// </summary>
		public Action<CpuThreadState> Delegate;

		/// <summary>
		/// A list of functions that have embedded this function.
		/// </summary>
		public List<DynarecFunction> InlinedAtFunctions = new List<DynarecFunction>();

		/// <summary>
		/// A list of Calling PCs
		/// </summary>
		public List<uint> CallingPCs = new List<uint>();
		
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

		public bool DisableOptimizations;

		/// <summary>
		/// 
		/// </summary>
		public TimeSpan TimeAnalyzeBranches;
		public TimeSpan TimeGenerateAst;
		public TimeSpan TimeOptimize;
		public TimeSpan TimeGenerateIL;
		public TimeSpan TimeCreateDelegate;
		public TimeSpan TimeLinking;
		public Dictionary<string, uint> InstructionStats;

		public TimeSpan TimeTotal
		{
			get
			{
				return TimeAnalyzeBranches + TimeGenerateAst + TimeOptimize + TimeGenerateIL + TimeCreateDelegate + TimeLinking;
			}
		}
	}
}

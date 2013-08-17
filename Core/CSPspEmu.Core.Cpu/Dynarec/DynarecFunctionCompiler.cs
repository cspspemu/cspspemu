using System;
using CSPspEmu.Core.Cpu.Emitter;
using CSPspEmu.Core.Cpu.Assembler;
using SafeILGenerator.Ast.Nodes;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	/// <summary>
	/// Compiles functions
	/// </summary>
	public partial class DynarecFunctionCompiler
	{
		[Inject]
		CpuProcessor CpuProcessor;

		[Inject]
		InjectContext InjectContext;

		private DynarecFunctionCompiler()
		{
		}

		public DynarecFunction CreateFunction(IInstructionReader InstructionReader, uint PC, Action<uint> ExploreNewPcCallback = null, bool DoDebug = false, bool DoLog = false)
		{
			switch (PC)
			{
				case SpecialCpu.ReturnFromFunction:
					return new DynarecFunction()
					{
						AstNode = new AstNodeStmEmpty(),
						EntryPC = PC,
						Name = "SpecialCpu.ReturnFromFunction",
						InstructionStats = new Dictionary<string,uint>(),
						Delegate = (CpuThreadState) =>
						{
							if (CpuThreadState == null) return;
							throw (new SpecialCpu.ReturnFromFunctionException());
						}
					};
				default:
					var MipsMethodEmiter = new MipsMethodEmitter(CpuProcessor, PC, DoDebug, DoLog);
					var InternalFunctionCompiler = new InternalFunctionCompiler(InjectContext, MipsMethodEmiter, this, InstructionReader, ExploreNewPcCallback, PC, DoLog);
					return InternalFunctionCompiler.CreateFunction();
			}
		}
	}

	public class SpecialCpu
	{
		public const uint ReturnFromFunction = 0xDEAD0001;

		public class ReturnFromFunctionException : Exception
		{
		}
	}
}

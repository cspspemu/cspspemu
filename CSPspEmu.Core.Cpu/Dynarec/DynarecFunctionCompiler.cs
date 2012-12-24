using System;
using CSPspEmu.Core.Cpu.Emitter;

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
			var MipsMethodEmiter = new MipsMethodEmitter(CpuProcessor, PC, DoDebug, DoLog);
			var InternalFunctionCompiler = new InternalFunctionCompiler(InjectContext, MipsMethodEmiter, this, InstructionReader, ExploreNewPcCallback, PC, DoLog);
			return InternalFunctionCompiler.CreateFunction();
		}
	}
}

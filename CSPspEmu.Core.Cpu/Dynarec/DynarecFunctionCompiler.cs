using System;
using CSPspEmu.Core.Cpu.Emitter;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	/// <summary>
	/// Compiles functions
	/// </summary>
	public partial class DynarecFunctionCompiler : PspEmulatorComponent
	{
		[Inject]
		CpuProcessor CpuProcessor;

		public DynarecFunction CreateFunction(IInstructionReader InstructionReader, uint PC, Action<uint> ExploreNewPcCallback = null, bool DoDebug = false, bool DoLog = false)
		{
			DynarecFunction DynarecFunction;

			//var Stopwatch = new Logger.Stopwatch();
			//Stopwatch.Tick();
			
			var MipsMethodEmiter = new MipsMethodEmitter(CpuProcessor, PC, DoDebug, DoLog);
			var InternalFunctionCompiler = new InternalFunctionCompiler(CpuProcessor, MipsMethodEmiter, this, InstructionReader, ExploreNewPcCallback, PC, DoLog);
			DynarecFunction = InternalFunctionCompiler.CreateFunction();

			//Stopwatch.Tick();
			//Console.WriteLine("Function at PC 0x{0:X} generated in {1}", PC, Stopwatch);

			return DynarecFunction;
		}
	}
}

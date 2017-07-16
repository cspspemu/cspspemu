//#define DEBUG_GENERATE_IL
//#define DEBUG_GENERATE_IL_CSHARP
#define USE_DYNAMIC_METHOD

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.Dynarec.Ast;
using SafeILGenerator.Ast.Optimizers;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Utils;

namespace CSPspEmu.Core.Cpu.Emitter
{
	/// <summary>
	/// <see cref="http://www.mrc.uidaho.edu/mrc/people/jff/digital/MIPSir.html"/>
	/// </summary>
	public unsafe sealed class MipsMethodEmitter
	{
		private uint PC;
		private CpuProcessor Processor;

		private static readonly AstMipsGenerator Ast = AstMipsGenerator.Instance;

		//public readonly Dictionary<string, uint> InstructionStats = new Dictionary<string, uint>();

		public MipsMethodEmitter(CpuProcessor processor, uint pc, bool doDebug = false, bool doLog = false)
		{
			this.Processor = processor;
			this.PC = pc;
		}

		public class Result
		{
			public TimeSpan TimeOptimize;
			public TimeSpan TimeGenerateIL;
			public TimeSpan TimeCreateDelegate;
			public Action<CpuThreadState> Delegate;
			public bool DisableOptimizations;
		}

		public Result CreateDelegate(AstNodeStm astNodeStm, int totalInstructions)
		{
			var time0 = DateTime.UtcNow;

			astNodeStm = AstOptimizerPsp.GlobalOptimize(Processor, Ast.Statements(astNodeStm, Ast.Return()));

			var time1 = DateTime.UtcNow;

#if DEBUG_GENERATE_IL
			Console.WriteLine("{0}", GeneratorIL.GenerateToString<GeneratorILPsp>(DynamicMethod, AstNodeStm));
#endif
#if DEBUG_GENERATE_IL_CSHARP
			Console.WriteLine("{0}", AstNodeExtensions.ToCSharpString(AstNodeStm).Replace("CpuThreadState.", ""));
#endif

			Action<CpuThreadState> Delegate;
			var time2 = time1;

			var disableOptimizations = _DynarecConfig.DisableDotNetJitOptimizations;
			if (totalInstructions >= _DynarecConfig.InstructionCountToDisableOptimizations) disableOptimizations = true;

			if (Platform.IsMono) disableOptimizations = false;
			if (_DynarecConfig.ForceJitOptimizationsOnEvenLargeFunctions) disableOptimizations = false;

			try
			{
				Delegate = MethodCreator.CreateDynamicMethod<Action<CpuThreadState>>(
				//Delegate = MethodCreator.CreateMethodInClass<Action<CpuThreadState>>(
					Assembly.GetExecutingAssembly().ManifestModule,
					String.Format("DynamicMethod_0x{0:X}", this.PC),
					disableOptimizations,
					(DynamicMethod) =>
					{
						astNodeStm.GenerateIl(DynamicMethod);
						time2 = DateTime.UtcNow;
					}
				);
			}
			catch (InvalidProgramException InvalidProgramException)
			{
				Console.Error.WriteLine("Invalid Delegate:");
#if LOG_TRACE
				Console.WriteLine("Invalid Delegate:");
				foreach (var Line in SafeILGenerator.GetEmittedInstructions())
				{
					if (Line.Substr(0, 1) == ":")
					{
						Console.WriteLine("{0}", Line);
					}
					else
					{
						Console.WriteLine("    {0}", Line);
					}
				}
#endif
				throw;
			}

			var time3 = DateTime.UtcNow;

			return new Result()
			{
				Delegate = Delegate,
				DisableOptimizations = disableOptimizations,
				TimeOptimize = time1 - time0,
				TimeGenerateIL = time2 - time1,
				TimeCreateDelegate = time3 - time2,
			};
		}
	}
}

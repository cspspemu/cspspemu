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

        private static readonly AstMipsGenerator ast = AstMipsGenerator.Instance;

        //public readonly Dictionary<string, uint> InstructionStats = new Dictionary<string, uint>();

        public MipsMethodEmitter(CpuProcessor Processor, uint PC, bool DoDebug = false, bool DoLog = false)
        {
            this.Processor = Processor;
            this.PC = PC;
        }

        public class Result
        {
            public TimeSpan TimeOptimize;
            public TimeSpan TimeGenerateIL;
            public TimeSpan TimeCreateDelegate;
            public Action<CpuThreadState> Delegate;
            public bool DisableOptimizations;
        }

        public Result CreateDelegate(AstNodeStm AstNodeStm, int TotalInstructions)
        {
            var Time0 = DateTime.UtcNow;

            AstNodeStm = AstOptimizerPsp.GlobalOptimize(Processor, ast.Statements(AstNodeStm, ast.Return()));

            var Time1 = DateTime.UtcNow;

#if DEBUG_GENERATE_IL
			Console.WriteLine("{0}", GeneratorIL.GenerateToString<GeneratorILPsp>(DynamicMethod, AstNodeStm));
#endif
#if DEBUG_GENERATE_IL_CSHARP
			Console.WriteLine("{0}", AstNodeExtensions.ToCSharpString(AstNodeStm).Replace("CpuThreadState.", ""));
#endif

            Action<CpuThreadState> Delegate;
            var Time2 = Time1;

            bool DisableOptimizations = _DynarecConfig.DisableDotNetJitOptimizations;
            if (TotalInstructions >= _DynarecConfig.InstructionCountToDisableOptimizations) DisableOptimizations = true;

            if (Platform.IsMono) DisableOptimizations = false;
            if (_DynarecConfig.ForceJitOptimizationsOnEvenLargeFunctions) DisableOptimizations = false;

            try
            {
                Delegate = MethodCreator.CreateDynamicMethod<Action<CpuThreadState>>(
                    //Delegate = MethodCreator.CreateMethodInClass<Action<CpuThreadState>>(
                    Assembly.GetExecutingAssembly().ManifestModule,
                    String.Format("DynamicMethod_0x{0:X}", this.PC),
                    DisableOptimizations,
                    (DynamicMethod) =>
                    {
                        AstNodeStm.GenerateIL(DynamicMethod);
                        Time2 = DateTime.UtcNow;
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
                throw (InvalidProgramException);
            }

            var Time3 = DateTime.UtcNow;

            return new Result()
            {
                Delegate = Delegate,
                DisableOptimizations = DisableOptimizations,
                TimeOptimize = Time1 - Time0,
                TimeGenerateIL = Time2 - Time1,
                TimeCreateDelegate = Time3 - Time2,
            };
        }
    }
}
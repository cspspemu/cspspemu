using System;
using CSPspEmu.Core.Cpu.Emitter;
using SafeILGenerator.Ast.Nodes;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Dynarec
{
    /// <summary>
    /// Compiles functions
    /// </summary>
    public partial class DynarecFunctionCompiler
    {
        [Inject] CpuProcessor CpuProcessor;

        [Inject] InjectContext InjectContext;

        private DynarecFunctionCompiler()
        {
        }

        public DynarecFunction CreateFunction(
            IInstructionReader instructionReader, uint pc,
            Action<uint> exploreNewPcCallback = null, bool doDebug = false, bool doLog = false,
            bool checkValidAddress = true
        )
        {
            switch (pc)
            {
                case SpecialCpu.ReturnFromFunction:
                    return new DynarecFunction()
                    {
                        AstNode = new AstNodeStmEmpty(),
                        EntryPc = pc,
                        Name = "SpecialCpu.ReturnFromFunction",
                        InstructionStats = new Dictionary<string, uint>(),
                        Delegate = cpuThreadState =>
                        {
                            if (cpuThreadState == null) return;
                            throw (new SpecialCpu.ReturnFromFunctionException());
                        }
                    };
                default:
                    var mipsMethodEmiter = new MipsMethodEmitter(CpuProcessor, pc, doDebug, doLog);
                    var internalFunctionCompiler = new InternalFunctionCompiler(InjectContext, mipsMethodEmiter, this,
                        instructionReader, exploreNewPcCallback, pc, doLog, checkValidAddress: checkValidAddress);
                    return internalFunctionCompiler.CreateFunction();
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
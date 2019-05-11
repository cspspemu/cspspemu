using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using Xunit;

namespace Tests.CSPspEmu.Core.Cpu.Dynarec
{
    
    public class DynarecFunctionCompilerTest
    {
        [Fact(Skip = "Check")]
        public void TestMethod1()
        {
            var cpuProcessor = CpuUtils.CreateCpuProcessor();

            var dynarecFunction = cpuProcessor.DynarecFunctionCompiler.CreateFunction(
                new InstructionArrayReader(MipsAssembler.StaticAssembleInstructions(@"
					addi r1, r1, 1
					jr r31
					nop
				").Instructions),
                0, checkValidAddress: false
            );

            var cpuThreadState = new CpuThreadState(cpuProcessor);
            Assert.Equal(0, cpuThreadState.Gpr[1]);
            dynarecFunction.Delegate(cpuThreadState);
            Assert.Equal(1, cpuThreadState.Gpr[1]);
        }
    }
}
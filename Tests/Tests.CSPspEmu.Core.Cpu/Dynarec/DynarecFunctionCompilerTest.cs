using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu;
using Tests.CSPspEmu.Core.Cpu.Cpu;
using Xunit;


namespace CSPspEmu.Tests.Cpu.Dynarec
{
    
    public class DynarecFunctionCompilerTest
    {
        [Fact(Skip = "Check")]
        public void TestMethod1()
        {
            var CpuProcessor = CpuUtils.CreateCpuProcessor();

            var DynarecFunction = CpuProcessor.DynarecFunctionCompiler.CreateFunction(
                new InstructionArrayReader(MipsAssembler.StaticAssembleInstructions(@"
					addi r1, r1, 1
					jr r31
					nop
				").Instructions),
                0, checkValidAddress: false
            );

            var CpuThreadState = new CpuThreadState(CpuProcessor);
            Assert.Equal(0, CpuThreadState.Gpr[1]);
            DynarecFunction.Delegate(CpuThreadState);
            Assert.Equal(1, CpuThreadState.Gpr[1]);
        }
    }
}
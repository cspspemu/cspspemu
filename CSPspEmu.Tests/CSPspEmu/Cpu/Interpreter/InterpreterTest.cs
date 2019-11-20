using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu.Switch;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.CPU.Interpreter;
using SafeILGenerator.Ast;
using Xunit;

namespace CSPspEmu.Tests.CSPspEmu.Cpu.Interpreter
{
    public class InterpreterTest
    {
        [Fact]
        public void TestMethod1()
        {
            var instructions = MipsAssembler.StaticAssembleInstructions(@"add r1, r2, r3").Instructions;
            var processor = new CpuProcessor();
            var state = new CpuThreadState(processor);
            var interpreter = new CpuInterpreter(state) {i = instructions[0]};
            state.Gpr2 = 3;
            state.Gpr3 = 7;
            interpreter.Add();
            Assert.Equal(3 + 7, (int) state.Gpr1);
        }


        [Fact]
        public void TestMethod2()
        {
            var processor = new CpuProcessor();
            var state = new CpuThreadState(processor);
            var interpreter = new CpuInterpreter(state);
            state.Gpr2 = 3;
            state.Gpr3 = 7;
            interpreter.Interpret(MipsAssembler.StaticAssembleInstructions(@"add r1, r2, r3").Instructions[0]);
            Assert.Equal(3 + 7, (int) state.Gpr1);
        }
    }
}
using CSPspEmu.Core.Cpu.Assembler;
using Xunit;


namespace CSPspEmu.Core.Tests.Cpu.Assembler
{
    
    public class MipsDisassemblerTest
    {
        [Fact(Skip = "Check")]
        public void DisassembleRegisterInstruction()
        {
            Assert.Equal(@"sll r0, r0, 0",
                new MipsDisassembler().Disassemble(pc: 0x00000000, instruction: 0x00000000).ToString());
        }

        [Fact(Skip = "Check")]
        public void DisassembleJumpInstruction()
        {
            var AssemblerResult = MipsAssembler.StaticAssembleInstructions(@"
			label1:
				j label2
			label2:
				j label1
				nop
			");

            Assert.Equal((uint) 4, AssemblerResult.Labels["label2"]);
            Assert.Equal((uint) 0, AssemblerResult.Labels["label1"]);

            Assert.Equal(@"j 0x00000004",
                new MipsDisassembler().Disassemble(pc: 0 * 4, instruction: AssemblerResult.Instructions[0]).ToString());
            Assert.Equal(@"j 0x00000000",
                new MipsDisassembler().Disassemble(pc: 1 * 4, instruction: AssemblerResult.Instructions[1]).ToString());
            Assert.Equal(@"and r0, r0, r0",
                new MipsDisassembler().Disassemble(pc: 2 * 4, instruction: AssemblerResult.Instructions[2]).ToString());
        }
    }
}
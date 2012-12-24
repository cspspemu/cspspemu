using System;
using System.Linq;
using System.IO;
using CSharpUtils.Streams;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using NUnit.Framework;

namespace CSPspEmu.Core.Tests.Cpu.Assembler
{
	[TestFixture]
	public class MipsDisassemblerTest
	{
		[Test]
		public void DisassembleRegisterInstruction()
		{
			Assert.AreEqual(@"sll r0, r0, 0", new MipsDisassembler().Disassemble(PC: 0x00000000, Instruction: 0x00000000).ToString());
		}

		[Test]
		public void DisassembleJumpInstruction()
		{
			var AssemblerResult = MipsAssembler.StaticAssembleInstructions(@"
			label1:
				j label2
			label2:
				j label1
				nop
			");

			Assert.AreEqual((uint)4, AssemblerResult.Labels["label2"]);
			Assert.AreEqual((uint)0, AssemblerResult.Labels["label1"]);

			Assert.AreEqual(@"j 0x00000004", new MipsDisassembler().Disassemble(PC: 0 * 4, Instruction: AssemblerResult.Instructions[0]).ToString());
			Assert.AreEqual(@"j 0x00000000", new MipsDisassembler().Disassemble(PC: 1 * 4, Instruction: AssemblerResult.Instructions[1]).ToString());
			Assert.AreEqual(@"and r0, r0, r0", new MipsDisassembler().Disassemble(PC: 2 * 4, Instruction: AssemblerResult.Instructions[2]).ToString());
		}
	}
}

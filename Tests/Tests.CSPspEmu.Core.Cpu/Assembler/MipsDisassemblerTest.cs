using System;
using System.Linq;
using System.IO;
using CSharpUtils.Streams;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSPspEmu.Core.Tests.Cpu.Assembler
{
	[TestClass]
	public class MipsDisassemblerTest
	{
		[TestMethod]
		public void DisassembleRegisterInstruction()
		{
			Assert.AreEqual(@"sll r0, r0, 0", new MipsDisassembler().Disassemble(pc: 0x00000000, instruction: 0x00000000).ToString());
		}

		[TestMethod]
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

			Assert.AreEqual(@"j 0x00000004", new MipsDisassembler().Disassemble(pc: 0 * 4, instruction: AssemblerResult.Instructions[0]).ToString());
			Assert.AreEqual(@"j 0x00000000", new MipsDisassembler().Disassemble(pc: 1 * 4, instruction: AssemblerResult.Instructions[1]).ToString());
			Assert.AreEqual(@"and r0, r0, r0", new MipsDisassembler().Disassemble(pc: 2 * 4, instruction: AssemblerResult.Instructions[2]).ToString());
		}
	}
}

using System;
using CSPspEmu.Core.Cpu.Assembler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSPspEmu.Core.Tests.Cpu.Assembler
{
	[TestClass]
	public class MipsDisassemblerTest
	{
		[TestMethod]
		public void DisassembleTest()
		{
			var MipsDisassembler = new MipsDisassembler();
			Assert.AreEqual(@"sll r0, r0, 0", MipsDisassembler.Disassemble(0x00000000).ToString());
		}
	}
}

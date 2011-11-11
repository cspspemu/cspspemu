using CSPspEmu.Core.Cpu.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class InstructionTableTest
	{
		[TestMethod]
		public void PspInstructionsTest()
		{
			var Instructions = InstructionTable.ALL;
		}
	}
}

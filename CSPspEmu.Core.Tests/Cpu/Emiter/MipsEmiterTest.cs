using CSPspEmu.Core.Cpu.Emiter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class MipsEmiterTest
	{
		[TestMethod]
		public void CreateDelegateTest()
		{
			var Processor = new Processor();
			var MipsEmiter = new MipsMethodEmiter(new MipsEmiter());
			Processor.GPR[1] = 1;
			Processor.GPR[2] = 2;
			Processor.GPR[3] = 3;
			MipsEmiter.OP_3REG(1, 2, 2, OpCodes.Add);
			MipsEmiter.OP_3REG(0, 2, 2, OpCodes.Add);
			MipsEmiter.OP_2REG_IMM(10, 0, 1000, OpCodes.Add);
			MipsEmiter.CreateDelegate()(Processor);
			Assert.AreEqual(4, Processor.GPR[1]);
			Assert.AreEqual(0, Processor.GPR[0]);
			Assert.AreEqual(1000, Processor.GPR[10]);
		}
	}
}

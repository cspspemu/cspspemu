using CSPspEmu.Core.Cpu.Emiter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu;

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
			MipsEmiter.ADD(1, 2, 2);
			MipsEmiter.ADD(0, 2, 2);
			MipsEmiter.ADDI(10, 0, 1000);
			MipsEmiter.CreateDelegate()(Processor);
			Assert.AreEqual(4U, Processor.GPR[1]);
			Assert.AreEqual(0U, Processor.GPR[0]);
			Assert.AreEqual(1000U, Processor.GPR[10]);
		}
	}
}

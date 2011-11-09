using CSPspEmu.Core.Cpu.Emiter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	unsafe public class MipsEmiterTest
	{
		[TestMethod]
		public void CreateDelegateTest()
		{
			var Memory = new NormalPspMemory();
			var PspConfig = new PspConfig();
			var Processor = new Processor(PspConfig, Memory);
			var CpuThreadState = new CpuThreadState(Processor);
			var MipsEmiter = new MipsMethodEmiter(new MipsEmiter(), Processor);
			CpuThreadState.GPR[1] = 1;
			CpuThreadState.GPR[2] = 2;
			CpuThreadState.GPR[3] = 3;
			MipsEmiter.OP_3REG_Unsigned(1, 2, 2, OpCodes.Add);
			MipsEmiter.OP_3REG_Unsigned(0, 2, 2, OpCodes.Add);
			MipsEmiter.OP_2REG_IMM_Signed(10, 0, 1000, OpCodes.Add);
			MipsEmiter.CreateDelegate()(CpuThreadState);
			Assert.AreEqual(4, CpuThreadState.GPR[1]);
			Assert.AreEqual(0, CpuThreadState.GPR[0]);
			Assert.AreEqual(1000, CpuThreadState.GPR[10]);
		}
	}
}

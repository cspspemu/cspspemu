using CSPspEmu.Core.Cpu.Emiter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu;
using System.IO;
using CSharpUtils.Extensions;
using System.Collections.Generic;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class CpuEmiterTest
	{
		[TestMethod]
		public void ArithmeticTest()
		{
			var Processor = new Processor();

			Processor.GPR[1] = -1;
			Processor.GPR[2] = -1;
			Processor.GPR[3] = -1;
			Processor.GPR[4] = -1;
			Processor.GPR[11] = 11;
			Processor.GPR[12] = 12;

			Processor.ExecuteAssembly(@"
				add  r1, r0, r11
				add  r2, r0, r12
				sub  r3, r2, r1
				addi r4, r0, 1234
			");

			Assert.AreEqual(11, Processor.GPR[1]);
			Assert.AreEqual(12, Processor.GPR[2]);
			Assert.AreEqual(1, Processor.GPR[3]);
			Assert.AreEqual(1234, Processor.GPR[4]);
		}

		[TestMethod]
		public void SyscallTest()
		{
			var Events = new List<int>();

			var Processor = new Processor();

			Processor.RegisterNativeSyscall(1, () =>
			{
				Events.Add(1);
			});

			Processor.RegisterNativeSyscall(1000, () =>
			{
				Events.Add(1000);
			});

			Processor.ExecuteAssembly(@"
				syscall 1
				syscall 1000
			");

			Assert.AreEqual("[1,1000]", Events.ToJson());
		}
	}
}

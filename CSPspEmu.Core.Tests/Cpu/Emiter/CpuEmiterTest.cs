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

		[TestMethod]
		public void BranchTest()
		{
			var Processor = new Processor();

			Processor.ExecuteAssembly(@"
				beq r3, r0, label1
				li r3, 1
				li r1, 1
			label1:
				li r2, 1
			");

			Assert.AreEqual(0, Processor.GPR[1]);
			Assert.AreEqual(1, Processor.GPR[2]);
			Assert.AreEqual(1, Processor.GPR[3]);

			//Assert.AreEqual("[1,1000]", Events.ToJson());
		}

		[TestMethod]
		public void LoopTest()
		{
			var Processor = new Processor();

			Processor.ExecuteAssembly(@"
				li r1, 10
				li r2, 0
			loop:
				addi r2, r2, 1
				bne r1, r0, loop
				addi r1, r1, -1
			");

			Assert.AreEqual(-1, Processor.GPR[1]);
			Assert.AreEqual(11, Processor.GPR[2]);
		}
	}
}

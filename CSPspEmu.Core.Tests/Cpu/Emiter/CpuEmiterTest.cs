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
	unsafe public class CpuEmiterTest
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

			Assert.AreEqual(
				"[0,1,1]",
				Processor.GPRList(1, 2, 3).ToJson()
			);

			//Assert.AreEqual("[1,1000]", Events.ToJson());
		}

		[TestMethod]
		public void Branch2Test()
		{
			var Processor = new Processor();

			Processor.ExecuteAssembly(@"
				li r1, 1
				beq r0, r0, label1 ; Taken. Should skip a +1 and the r2=1
				addi r1, r1, 1
				addi r1, r1, 1
				li r2, 1
			label1:
				nop
			");

			Assert.AreEqual(2, Processor.GPR[1]);
			Assert.AreEqual(0, Processor.GPR[2]);
		}

		[TestMethod]
		public void BranchLikelyTest()
		{
			var Processor = new Processor();

			Processor.ExecuteAssembly(@"
				li r1, 1
				beql r1, r1, label1 ; Taken. The delayed branch is executed.
				li r2, 1
			label1:
				beql r1, r0, label2 ; Not Taken. The delayed branch is not executed.
				li r3, 1
			label2:
				nop
			");

			Assert.AreEqual(1, Processor.GPR[2]);
			Assert.AreEqual(0, Processor.GPR[3]);
		}

		[TestMethod]
		public void BranchFullTest()
		{
			var Processor = new Processor();

			var RegsV = new String[] {
				"r10, r10",
				"r10, r11",
				"r10, r12",

				"r11, r10",
				"r11, r11",
				"r11, r12",

				"r12, r10",
				"r12, r11",
				"r12, r12",
			};

			var Regs0 = new String[] {
				"r10",
				"r11",
				"r12",
			};

			Func<String, String[], IEnumerable<int>> Generator = (String Branch, String[] RegsList) =>
			{
				var Results = new List<int>();
				foreach (var Regs in RegsList)
				{
					Processor.ExecuteAssembly(
						@"
							li r10, -1
							li r11,  0
							li r12, +1
							%BRANCH% %REGS%, label_yes
							li r1, 2
							b label_no
							nop

						label_yes:
							li r1, 1
							b label_end
							nop

						label_no:
							li r1, 0
							b label_end
							nop

						label_end:
							nop
						"
						.Replace("%BRANCH%", Branch)
						.Replace("%REGS%", Regs)
					);

					Results.Add(Processor.GPR[1]);
				}

				return Results;
			};

			Assert.AreEqual("[1,0,0,0,1,0,0,0,1]", Generator("beq", RegsV).ToJson());
			Assert.AreEqual("[0,1,1,1,0,1,1,1,0]", Generator("bne", RegsV).ToJson());

			Assert.AreEqual("[1,0,0]", Generator("bltz", Regs0).ToJson());
			Assert.AreEqual("[1,1,0]", Generator("blez", Regs0).ToJson());
			Assert.AreEqual("[0,0,1]", Generator("bgtz", Regs0).ToJson());
			Assert.AreEqual("[0,1,1]", Generator("bgez", Regs0).ToJson());
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

		[TestMethod]
		public void BitrevTest()
		{
			var Processor = new Processor();

			Processor.ExecuteAssembly(@"
				li r1, 0b_00000011111101111101111011101101
				bitrev r2, r1
			");

			Assert.AreEqual("00000011111101111101111011101101", "%032b".Sprintf(Processor.GPR[1]));
			Assert.AreEqual("10110111011110111110111111000000", "%032b".Sprintf(Processor.GPR[2]));
		}

		[TestMethod]
		public void LoadStoreTest()
		{
			var Processor = new Processor();

			Processor.ExecuteAssembly(@"
				li r1, 0x12345678
				li r2, 0x88000000
				sw r1, 0(r2)
				lb r3, 0(r2)          ; Little Endian
			");
			
			Assert.AreEqual("12345678", "%08X".Sprintf(Processor.GPR[1]));
			Assert.AreEqual("88000000", "%08X".Sprintf(Processor.GPR[2]));
			Assert.AreEqual("00000078", "%08X".Sprintf(Processor.GPR[3]));
		}

		[TestMethod]
		public void FloatTest()
		{
			var Processor = new Processor();

			Processor.FPR[29] = 81.0f;
			Processor.FPR[30] = -1.0f;
			Processor.FPR[31] = 3.5f;

			Processor.ExecuteAssembly(@"
				; Unary
				mov.s  f0, f30
				neg.s  f1, f31
				sqrt.s f2, f29
				abs.s  f3, f30
				abs.s  f4, f31

				; Binary
				add.s f10, f30, f31
				sub.s f11, f30, f31
			");

			// Unary
			Assert.AreEqual(Processor.FPR[0], Processor.FPR[30]);
			Assert.AreEqual(Processor.FPR[1], -Processor.FPR[31]);
			Assert.AreEqual(Processor.FPR[2], Math.Sqrt(Processor.FPR[29]));
			Assert.AreEqual(Processor.FPR[3], Math.Abs(Processor.FPR[30]));
			Assert.AreEqual(Processor.FPR[4], Math.Abs(Processor.FPR[31]));

			Assert.AreEqual(Processor.FPR[10], Processor.FPR[30] + Processor.FPR[31]);
			Assert.AreEqual(Processor.FPR[11], Processor.FPR[30] - Processor.FPR[31]);
		}
	}
}

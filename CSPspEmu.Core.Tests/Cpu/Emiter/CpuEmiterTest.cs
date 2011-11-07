using CSPspEmu.Core.Cpu.Emiter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu;
using System.IO;
using CSharpUtils.Extensions;
using System.Collections.Generic;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	unsafe public class CpuEmiterTest
	{
		protected AbstractPspMemory Memory;
		protected Processor Processor;
		protected CpuThreadState CpuThreadState;

		[TestInitialize]
		public void SetUp()
		{
			Memory = new LazyPspMemory();
			Processor = new Processor(Memory);
			CpuThreadState = new CpuThreadState(Processor);
		}

		[TestMethod]
		public void ArithmeticTest()
		{
			CpuThreadState.GPR[1] = -1;
			CpuThreadState.GPR[2] = -1;
			CpuThreadState.GPR[3] = -1;
			CpuThreadState.GPR[4] = -1;
			CpuThreadState.GPR[11] = 11;
			CpuThreadState.GPR[12] = 12;

			CpuThreadState.ExecuteAssembly(@"
				add  r1, r0, r11
				add  r2, r0, r12
				sub  r3, r2, r1
				addi r4, r0, 1234
			");

			Assert.AreEqual(11, CpuThreadState.GPR[1]);
			Assert.AreEqual(12, CpuThreadState.GPR[2]);
			Assert.AreEqual(1, CpuThreadState.GPR[3]);
			Assert.AreEqual(1234, CpuThreadState.GPR[4]);
		}

		[TestMethod]
		public void SyscallTest()
		{
			var Events = new List<int>();

			CpuThreadState.RegisterNativeSyscall(1, () =>
			{
				Events.Add(1);
			});

			CpuThreadState.RegisterNativeSyscall(1000, () =>
			{
				Events.Add(1000);
			});

			CpuThreadState.ExecuteAssembly(@"
				syscall 1
				syscall 1000
			");

			Assert.AreEqual("[1,1000]", Events.ToJson());
		}

		[TestMethod]
		public void BranchTest()
		{
			CpuThreadState.ExecuteAssembly(@"
				beq r3, r0, label1
				li r3, 1
				li r1, 1
			label1:
				li r2, 1
			");

			Assert.AreEqual(
				"[0,1,1]",
				CpuThreadState.GPRList(1, 2, 3).ToJson()
			);

			//Assert.AreEqual("[1,1000]", Events.ToJson());
		}

		[TestMethod]
		public void Branch2Test()
		{
			CpuThreadState.ExecuteAssembly(@"
				li r1, 1
				beq r0, r0, label1 ; Taken. Should skip a +1 and the r2=1
				addi r1, r1, 1
				addi r1, r1, 1
				li r2, 1
			label1:
				nop
			");

			Assert.AreEqual(2, CpuThreadState.GPR[1]);
			Assert.AreEqual(0, CpuThreadState.GPR[2]);
		}

		[TestMethod]
		public void BranchLikelyTest()
		{
			CpuThreadState.ExecuteAssembly(@"
				li r1, 1
				beql r1, r1, label1 ; Taken. The delayed branch is executed.
				li r2, 1
			label1:
				beql r1, r0, label2 ; Not Taken. The delayed branch is not executed.
				li r3, 1
			label2:
				nop
			");

			Assert.AreEqual(1, CpuThreadState.GPR[2]);
			Assert.AreEqual(0, CpuThreadState.GPR[3]);
		}

		[TestMethod]
		public void BranchFullTest()
		{
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
					CpuThreadState.ExecuteAssembly(
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

					Results.Add(CpuThreadState.GPR[1]);
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
			CpuThreadState.ExecuteAssembly(@"
				li r1, 10
				li r2, 0
			loop:
				addi r2, r2, 1
				bne r1, r0, loop
				addi r1, r1, -1
			");

			Assert.AreEqual(-1, CpuThreadState.GPR[1]);
			Assert.AreEqual(11, CpuThreadState.GPR[2]);
		}

		[TestMethod]
		public void BitrevTest()
		{
			CpuThreadState.ExecuteAssembly(@"
				li r1, 0b_00000011111101111101111011101101
				bitrev r2, r1
			");

			Assert.AreEqual("00000011111101111101111011101101", "%032b".Sprintf(CpuThreadState.GPR[1]));
			Assert.AreEqual("10110111011110111110111111000000", "%032b".Sprintf(CpuThreadState.GPR[2]));
		}

		[TestMethod]
		public void LoadStoreTest()
		{
			CpuThreadState.ExecuteAssembly(@"
				li r1, 0x12345678
				li r2, 0x88000000
				sw r1, 0(r2)
				lb r3, 0(r2)          ; Little Endian
			");
			
			Assert.AreEqual("12345678", "%08X".Sprintf(CpuThreadState.GPR[1]));
			Assert.AreEqual("88000000", "%08X".Sprintf(CpuThreadState.GPR[2]));
			Assert.AreEqual("00000078", "%08X".Sprintf(CpuThreadState.GPR[3]));
		}

		[TestMethod]
		public void FloatTest()
		{
			CpuThreadState.FPR[29] = 81.0f;
			CpuThreadState.FPR[30] = -1.0f;
			CpuThreadState.FPR[31] = 3.5f;

			CpuThreadState.ExecuteAssembly(@"
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
			Assert.AreEqual(CpuThreadState.FPR[0], CpuThreadState.FPR[30]);
			Assert.AreEqual(CpuThreadState.FPR[1], -CpuThreadState.FPR[31]);
			Assert.AreEqual(CpuThreadState.FPR[2], Math.Sqrt(CpuThreadState.FPR[29]));
			Assert.AreEqual(CpuThreadState.FPR[3], Math.Abs(CpuThreadState.FPR[30]));
			Assert.AreEqual(CpuThreadState.FPR[4], Math.Abs(CpuThreadState.FPR[31]));

			Assert.AreEqual(CpuThreadState.FPR[10], CpuThreadState.FPR[30] + CpuThreadState.FPR[31]);
			Assert.AreEqual(CpuThreadState.FPR[11], CpuThreadState.FPR[30] - CpuThreadState.FPR[31]);
		}

		[TestMethod]
		public void JumpTest()
		{
			CpuThreadState.ExecuteAssembly(@"
				li r1, 1
				li r2, 1

				jal test
				nop
			ret:
				li r1, 2
			test:
				li r2, 2
				nop
			");

			Assert.AreEqual(1, CpuThreadState.GPR[1]);
			Assert.AreEqual(1, CpuThreadState.GPR[2]);
			Assert.AreEqual(4 * 4, (int)CpuThreadState.GPR[31]);
			Assert.AreEqual(5 * 4, (int)CpuThreadState.PC);
		}
	}
}

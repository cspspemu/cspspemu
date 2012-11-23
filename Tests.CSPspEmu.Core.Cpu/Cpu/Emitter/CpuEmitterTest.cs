using CSPspEmu.Core.Cpu.Emitter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu;
using System.IO;
using CSharpUtils;
using System.Collections.Generic;
using CSPspEmu.Core.Memory;
using CSharpUtils.Factory;
using System.Threading;
using CSPspEmu.Core.Cpu.Dynarec;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public unsafe partial class CpuEmitterTest
	{
		[TestMethod]
		public void SimplestTest()
		{
			//CpuThreadState.GPR[11] = 11;

			ExecuteAssembly(@"
				add  r1, r0, r0
			");

			//Assert.AreEqual(11, CpuThreadState.GPR[1]);
		}

		[TestMethod]
		public void TestRotating()
		{
			//CpuThreadState.GPR[11] = 0x;
			ExecuteAssembly(@"
				li r2 , 0b_10110111011110111110111111011111;
				li r11, 0b_11110110111011110111110111111011;
				rotr r1, r2, 3
			");
			Assert.AreEqual(CpuThreadState.GPR[11], CpuThreadState.GPR[1]);
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

			ExecuteAssembly(@"
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

			CpuProcessor.RegisterNativeSyscall(1, () => { Events.Add(1); });
			CpuProcessor.RegisterNativeSyscall(1000, () => { Events.Add(1000); });

			ExecuteAssembly(@"
				syscall 1
				syscall 1000
			");

			Assert.AreEqual("[1,1000]", Events.ToJson());
		}

		[TestMethod]
		public void BranchTest()
		{
			ExecuteAssembly(@"
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
			ExecuteAssembly(@"
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
			ExecuteAssembly(@"
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
			var RegsV = new[] {
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

			var Regs0 = new[] {
				"r10",
				"r11",
				"r12",
			};

			Func<String, String[], IEnumerable<int>> Generator = (String Branch, String[] RegsList) =>
			{
				var Results = new List<int>();
				foreach (var Regs in RegsList)
				{
					ExecuteAssembly(
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

			//Assert.AreEqual("[0,0,1]", Generator("bgtzl", Regs0).ToJson());
		}

		[TestMethod]
		public void LoopTest()
		{
			ExecuteAssembly(@"
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
			ExecuteAssembly(@"
				li r1, 0b_00000011111101111101111011101101
				bitrev r2, r1
			");

			Assert.AreEqual("00000011111101111101111011101101", "%032b".Sprintf(CpuThreadState.GPR[1]));
			Assert.AreEqual("10110111011110111110111111000000", "%032b".Sprintf(CpuThreadState.GPR[2]));
		}

		[TestMethod]
		public void LoadStoreTest()
		{
			ExecuteAssembly(@"
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
			//CpuThreadState.FPR[27] = 2.0f;
			//CpuThreadState.FPR[28] = 20.0f;
			CpuThreadState.FPR[29] = 81.0f;
			CpuThreadState.FPR[30] = -1.0f;
			CpuThreadState.FPR[31] = 3.5f;

			ExecuteAssembly(@"
				; Unary
				mov.s  f0, f30
				neg.s  f1, f31
				sqrt.s f2, f29
				abs.s  f3, f30
				abs.s  f4, f31

				; Binary
				add.s f10, f30, f31
				sub.s f11, f30, f31
				div.s f12, f30, f31
				mul.s f13, f30, f31
			");

			// Unary
			Assert.AreEqual(CpuThreadState.FPR[0], CpuThreadState.FPR[30]);
			Assert.AreEqual(CpuThreadState.FPR[1], -CpuThreadState.FPR[31]);
			Assert.AreEqual(CpuThreadState.FPR[2], Math.Sqrt(CpuThreadState.FPR[29]));
			Assert.AreEqual(CpuThreadState.FPR[3], Math.Abs(CpuThreadState.FPR[30]));
			Assert.AreEqual(CpuThreadState.FPR[4], Math.Abs(CpuThreadState.FPR[31]));

			Assert.AreEqual(CpuThreadState.FPR[10], CpuThreadState.FPR[30] + CpuThreadState.FPR[31]);
			Assert.AreEqual(CpuThreadState.FPR[11], CpuThreadState.FPR[30] - CpuThreadState.FPR[31]);
			Assert.AreEqual(CpuThreadState.FPR[12], CpuThreadState.FPR[30] / CpuThreadState.FPR[31]);
			Assert.AreEqual(CpuThreadState.FPR[13], CpuThreadState.FPR[30] * CpuThreadState.FPR[31]);
		}

		[TestMethod]
		public void ShiftTest()
		{
			ExecuteAssembly(@"
				li   r10, 0b_10110111011110111110111111000000
				li   r11, 0b_01011011101111011111011111100000
				li   r12, 7

				sll  r1, r10, 7
				sllv r2, r10, r12
				srl  r3, r10, 7
				srlv r4, r10, r12
				sra  r5, r10, 7
				srav r6, r10, r12
				sra  r7, r11, 7
				srav r8, r11, r12
			");

			// Check input not modified.
			Assert.AreEqual("10110111011110111110111111000000", "%032b".Sprintf(CpuThreadState.GPR[10]));
			Assert.AreEqual(7, CpuThreadState.GPR[12]);

			Assert.AreEqual("10111101111101111110000000000000", "%032b".Sprintf(CpuThreadState.GPR[1]));
			Assert.AreEqual("10111101111101111110000000000000", "%032b".Sprintf(CpuThreadState.GPR[2]));
			Assert.AreEqual("00000001011011101111011111011111", "%032b".Sprintf(CpuThreadState.GPR[3]));
			Assert.AreEqual("00000001011011101111011111011111", "%032b".Sprintf(CpuThreadState.GPR[4]));
			Assert.AreEqual("11111111011011101111011111011111", "%032b".Sprintf(CpuThreadState.GPR[5]));
			Assert.AreEqual("11111111011011101111011111011111", "%032b".Sprintf(CpuThreadState.GPR[6]));
			Assert.AreEqual("00000000101101110111101111101111", "%032b".Sprintf(CpuThreadState.GPR[7]));
			Assert.AreEqual("00000000101101110111101111101111", "%032b".Sprintf(CpuThreadState.GPR[8]));
		}

		[TestMethod]
		public void SetLessThanTest()
		{
			ExecuteAssembly(@"
				li r1, 0x77777777
				li r10, 0
				li r11, -100
				li r12, +100
				sltiu r1, r10, 0
				sltiu r2, r10, 7
				sltiu r3, r11, -200
				slti  r4, r11, -200
			");

			Assert.AreEqual(0, CpuThreadState.GPR[1]);
			Assert.AreEqual(1, CpuThreadState.GPR[2]);
			Assert.AreEqual(0, CpuThreadState.GPR[3]);
			Assert.AreEqual(0, CpuThreadState.GPR[4]);
		}

		[TestMethod]
		public void MoveLoHiTest()
		{
			ExecuteAssembly(@"
				li r21, 0x12345678
				li r22, 0x87654321
				mtlo r21
				mthi r22
				mflo r1
				mfhi r2
			");

			Assert.AreEqual(CpuThreadState.GPR[21], CpuThreadState.LO);
			Assert.AreEqual(CpuThreadState.GPR[22], CpuThreadState.HI);
			Assert.AreEqual(CpuThreadState.GPR[1], CpuThreadState.LO);
			Assert.AreEqual(CpuThreadState.GPR[2], CpuThreadState.HI);
		}

		[TestMethod]
		public void SetDivTest()
		{
			ExecuteAssembly(@"
				li r10, 100
				li r11, 12
				div r10, r11
			");

			Assert.AreEqual(4, (int)CpuThreadState.HI);
			Assert.AreEqual(8, (int)CpuThreadState.LO);
		}

		[TestMethod]
		public void SetMulSimpleTest()
		{
			ExecuteAssembly(@"
				li r10, 7
				li r11, 13
				mult r10, r11
			");

			long Expected = ((long)CpuThreadState.GPR[10] * (long)CpuThreadState.GPR[11]);

			Assert.AreEqual((uint)((Expected >> 0) & 0xFFFFFFFF), (uint)CpuThreadState.LO);
			Assert.AreEqual((uint)((Expected >> 32) & 0xFFFFFFFF), (uint)CpuThreadState.HI);
		}

		[TestMethod]
		public void SetMulTest()
		{
			ExecuteAssembly(@"
				li r10, 0x12345678
				li r11, 0x87654321
				mult r10, r11
			");

			long Expected = ((long)(int)CpuThreadState.GPR[10] * (long)(int)CpuThreadState.GPR[11]);

			//Console.WriteLine(CpuThreadState.GPR[10]);
			//Console.WriteLine(CpuThreadState.GPR[11]);
			//Console.WriteLine(Expected);

			Assert.AreEqual((uint)((((long)Expected) >> 0) & 0xFFFFFFFF), (uint)CpuThreadState.LO);
			Assert.AreEqual((uint)((((long)Expected) >> 32) & 0xFFFFFFFF), (uint)CpuThreadState.HI);
		}

		[TestMethod]
		public void SetMultuTest()
		{
			ExecuteAssembly(@"
				li r10, 0x12345678
				li r11, 0x87654321
				multu r10, r11
			");

			ulong Expected = ((ulong)(uint)CpuThreadState.GPR[10] * (ulong)(uint)CpuThreadState.GPR[11]);

			//Console.WriteLine(CpuThreadState.GPR[10]);
			//Console.WriteLine(CpuThreadState.GPR[11]);
			//Console.WriteLine(Expected);

			Assert.AreEqual((uint)((((ulong)Expected) >> 0) & 0xFFFFFFFF), (uint)CpuThreadState.LO);
			Assert.AreEqual((uint)((((ulong)Expected) >> 32) & 0xFFFFFFFF), (uint)CpuThreadState.HI);
		}

		[TestMethod]
		public void TrySet0Test()
		{
			ExecuteAssembly(@"
				li r0, 0x12345678
			");

			Assert.AreEqual(0, CpuThreadState.GPR[0]);
		}

		[TestMethod]
		public void TryDecWithAddTest()
		{
			ExecuteAssembly(@"
				li r1, 100
				addiu r1, r1, -1
			");

			Assert.AreEqual(99, CpuThreadState.GPR[1]);
		}

		[TestMethod]
		public void SignExtendTest()
		{
			ExecuteAssembly(@"
				li  r10, 0xFF
				li  r11, 0xFFFF
				or  r1, r0, r10
				seb r2, r10
				or  r3, r0, r11
				seh r4, r11
			");

			Assert.AreEqual((uint)0x000000FF, (uint)CpuThreadState.GPR[1]);
			Assert.AreEqual((uint)0xFFFFFFFF, (uint)CpuThreadState.GPR[2]);
			Assert.AreEqual((uint)0x0000FFFF, (uint)CpuThreadState.GPR[3]);
			Assert.AreEqual((uint)0xFFFFFFFF, (uint)CpuThreadState.GPR[4]);
		}

		[TestMethod]
		public void MovZeroNumberTest()
		{
			ExecuteAssembly(@"
				li  r10, 0xFF
				li  r11, 0x777
				movz r1, r11, r10
				movn r2, r11, r10
				movz r3, r11, r0
				movn r4, r11, r0
			");

			Assert.AreEqual(0x000, (int)CpuThreadState.GPR[1]);
			Assert.AreEqual(0x777, (int)CpuThreadState.GPR[2]);

			Assert.AreEqual(0x777, (int)CpuThreadState.GPR[3]);
			Assert.AreEqual(0x000, (int)CpuThreadState.GPR[4]);
		}

		[TestMethod]
		public void MinMaxTest()
		{
			ExecuteAssembly(@"
				li  r10, -100
				li  r11, 0
				li  r12, +100
				max r1, r10, r12
				max r2, r12, r10
				min r3, r10, r12
				min r4, r12, r10
			");

			Assert.AreEqual(+100, (int)CpuThreadState.GPR[1]);
			Assert.AreEqual(+100, (int)CpuThreadState.GPR[2]);
			Assert.AreEqual(-100, (int)CpuThreadState.GPR[3]);
			Assert.AreEqual(-100, (int)CpuThreadState.GPR[4]);
		}

		[TestMethod]
		public void LoadStoreFPUTest()
		{
			CpuThreadState.FPR[0] = 1.0f;
			ExecuteAssembly(@"
				li r1, 0x08000000
				swc1 f0, 0(r1)
				lwc1 f1, 0(r1)
			");
			Assert.AreEqual(0x3F800000, (int)CpuThreadState.CpuProcessor.Memory.ReadSafe<uint>(0x08000000));
			Assert.AreEqual(CpuThreadState.FPR[1], CpuThreadState.FPR[0]);
		}

		[TestMethod]
		public void MoveFPUTest()
		{
			CpuThreadState.GPR[1] = 17;
			CpuThreadState.FPR[2] = 8.3f;
			ExecuteAssembly(@"
				mtc1 r1, f1
				mfc1 r2, f2
			");
			Assert.AreEqual(CpuThreadState.GPR[1], CpuThreadState.FPR_I[1]);
			Assert.AreEqual(CpuThreadState.FPR_I[2], CpuThreadState.GPR[2]);
		}

		[TestMethod]
		public void OrNorTest()
		{
			ExecuteAssembly(@"
				li r20, 0b_11000110001100011000110001100011
				li r21, 0b_00010000100001000010000100001000
				or  r1, r20, r21
				nor r2, r20, r21
			");

			Assert.AreEqual("11010110101101011010110101101011", "%032b".Sprintf(CpuThreadState.GPR[1]));
			Assert.AreEqual("00101001010010100101001010010100", "%032b".Sprintf(CpuThreadState.GPR[2]));
		}

		[TestMethod]
		public void ExtractInsertTest()
		{
			// %t, %s, %a, %ne
			ExecuteAssembly(@"
				li r2, 0b_11000011001000000011111011101101
				ext r1, r2, 3, 10
			");

			Assert.AreEqual("1111011101", "%010b".Sprintf(CpuThreadState.GPR[1]));
		}

		[TestMethod]
		public void FloatCompTest()
		{
			// %t, %s, %a, %ne
			CpuThreadState.FPR[1] = 1.0f;
			CpuThreadState.FPR[2] = 2.0f;

			ExecuteAssembly(@"c.eq.s f1, f2");
			Assert.AreEqual(false, CpuThreadState.Fcr31.CC);

			ExecuteAssembly(@"c.eq.s f1, f1");
			Assert.AreEqual(true, CpuThreadState.Fcr31.CC);

			ExecuteAssembly(@"c.lt.s f1, f2");
			Assert.AreEqual(true, CpuThreadState.Fcr31.CC);

			ExecuteAssembly(@"c.lt.s f2, f1");
			Assert.AreEqual(false, CpuThreadState.Fcr31.CC);

			ExecuteAssembly(@"c.lt.s f1, f1");
			Assert.AreEqual(false, CpuThreadState.Fcr31.CC);

			Action<String> Gen = (INSTRUCTION_NAME) =>
			{
				ExecuteAssembly(@"
					li r1, -1
					c.eq.s f1, f1
					%INSTRUCTION_NAME% label
					nop
					li r1, 0
					b end
					nop
				label:
					li r1, 1
					b end
					nop
				end:
					nop
				".Replace("%INSTRUCTION_NAME%", INSTRUCTION_NAME));
			};

			Gen("bc1t");
			Assert.AreEqual(1, CpuThreadState.GPR[1]);

			Gen("bc1f");
			Assert.AreEqual(0, CpuThreadState.GPR[1]);
		}

		[TestMethod]
		public void FloatControlRegisterTest()
		{
			CpuThreadState.Fcr31.Value = 0x12345678;
			ExecuteAssembly(@"
				li r2, 0x87654321
				cfc1 r1, 31
				ctc1 r2, 31
			");
			Assert.AreEqual(0x12345678, CpuThreadState.GPR[1]);
			Assert.AreEqual(0x87654321, CpuThreadState.Fcr31.Value);
		}

		[TestMethod]
		public void CountLeadingOnesAndZeros()
		{
			ExecuteAssembly(@"
				li  r11, 0b_00000000000000000000000000000000
				li  r12, 0b_11111111111111111111111111111111
				li  r13, 0b_11100000000000000000000000000111
				li  r14, 0b_00011111111111111111111111111000
				li  r15, 0b_00011111111111111111111111111111
				li  r16, 0b_11100000000000000000000000000000

				clz r1, r11
				clo r2, r11

				clz r3, r12
				clo r4, r12

				clz r5, r13
				clo r6, r13

				clz r7, r14
				clo r8, r14

				clz r9, r15
				clo r10, r15
			");
			// r11
			Assert.AreEqual(32, CpuThreadState.GPR[1]);
			Assert.AreEqual(0, CpuThreadState.GPR[2]);
	
			// r12
			Assert.AreEqual(0, CpuThreadState.GPR[3]);
			Assert.AreEqual(32, CpuThreadState.GPR[4]);

			// r13
			Assert.AreEqual(0, CpuThreadState.GPR[5]);
			Assert.AreEqual(3, CpuThreadState.GPR[6]);

			// r14
			Assert.AreEqual(3, CpuThreadState.GPR[7]);
			Assert.AreEqual(0, CpuThreadState.GPR[8]);

			// r15
			Assert.AreEqual(3, CpuThreadState.GPR[9]);
			Assert.AreEqual(0, CpuThreadState.GPR[10]);
		}

		[TestMethod]
		public void JalTest1()
		{
			ExecuteAssembly(@"
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
			Assert.AreEqual(2, CpuThreadState.GPR[2]);
			Assert.AreEqual(4 * 4, (int)CpuThreadState.GPR[31]);
			Assert.AreEqual(7 * 4, (int)CpuThreadState.PC);
		}

		[TestMethod]
		public void JumpTest2()
		{
			var Events = new List<int>();

			CpuProcessor.RegisterNativeSyscall(1, () => { Events.Add(1); });
			CpuProcessor.RegisterNativeSyscall(2, () => { Events.Add(2); });
			CpuProcessor.RegisterNativeSyscall(3, () => { Events.Add(3); });
			CpuProcessor.RegisterNativeSyscall(4, () => { Events.Add(4); });

			ExecuteAssembly(@"
				syscall 1
				j skip
				nop
				syscall 2
			skip:
				syscall 3
			");

			Assert.AreEqual("[1,3]", Events.ToJson());
		}

		[TestMethod]
		public void JalTest()
		{
			var Events = new List<int>();

			CpuProcessor.RegisterNativeSyscall(1, () => { Events.Add(1); });
			CpuProcessor.RegisterNativeSyscall(2, () => { Events.Add(2); });
			CpuProcessor.RegisterNativeSyscall(3, () => { Events.Add(3); });
			CpuProcessor.RegisterNativeSyscall(4, () => { Events.Add(4); });

			ExecuteAssembly(@"
				li r1, 100
				syscall 1

				jal function1
				nop
				jal function1
				nop
				jal function1
				nop
				syscall 3

			j end
			nop

			function1:
				syscall 2
				addi r1, r1, 1
				jr r31
				nop

			end:
				nop
				syscall 4
			");

			Assert.AreEqual("[1,2,2,2,3,4]", Events.ToJson());
			Assert.AreEqual(103, CpuThreadState.GPR[1]);
		}

		[TestMethod]
		public void BltzalTest()
		{
			var Events = new List<int>();

			CpuProcessor.RegisterNativeSyscall(1, () => { Events.Add(1); });
			CpuProcessor.RegisterNativeSyscall(2, () => { Events.Add(2); });
			CpuProcessor.RegisterNativeSyscall(3, () => { Events.Add(3); });
			CpuProcessor.RegisterNativeSyscall(4, () => { Events.Add(4); });

			ExecuteAssembly(@"
				li r1, 1
				li r2, -1

				bltzal r1, function1
				nop

				bltzal r2, function2
				nop

			j end
			nop

			function1:
				syscall 1
				jr r31
				nop

			function2:
				syscall 2
				jr r31
				nop

			end:
				nop
				syscall 3
			");

			Assert.AreEqual("[2,3]", Events.ToJson());
		}

		[TestMethod]
		public void ConvertFloat1Test()
		{
			ExecuteAssembly(@"
				li r1, 100
				mtc1 r1, f1
				cvt.s.w f2, f1
			");

			Assert.AreEqual(100.0f, CpuThreadState.FPR[2]);
		}

		[TestMethod]
		public void LoadUnalignedTest()
		{
			var Value = 0x87654321;
			Memory.WriteSafe<uint>(0x08010004, 0x87654321);
			ExecuteAssembly(@"
				li r2, 0x08010000
				lwl r1, 7(r2)
				lwr r1, 4(r2)
			");
			Assert.AreEqual((uint)Value, (uint)CpuThreadState.GPR[1]);
		}

		[TestMethod]
		public void ConvertFloat2Test()
		{
			CpuThreadState.FPR[29] = 13.4f;
			CpuThreadState.FPR[30] = 13.6f;
			CpuThreadState.FPR[31] = 13.5f;

			ExecuteAssembly(@"
				trunc.w.s f1, f29
				floor.w.s f2, f29
				round.w.s f3, f29
				ceil.w.s  f4, f29

				trunc.w.s f11, f30
				floor.w.s f12, f30
				round.w.s f13, f30
				ceil.w.s  f14, f30

				trunc.w.s f21, f31
				floor.w.s f22, f31
				round.w.s f23, f31
				ceil.w.s  f24, f31
			");

			Assert.AreEqual(13, CpuThreadState.FPR_I[1]);
			Assert.AreEqual(13, CpuThreadState.FPR_I[2]);
			Assert.AreEqual(13, CpuThreadState.FPR_I[3]);
			Assert.AreEqual(14, CpuThreadState.FPR_I[4]);

			Assert.AreEqual(13, CpuThreadState.FPR_I[11]);
			Assert.AreEqual(13, CpuThreadState.FPR_I[12]);
			Assert.AreEqual(14, CpuThreadState.FPR_I[13]);
			Assert.AreEqual(14, CpuThreadState.FPR_I[14]);

			Assert.AreEqual(13, CpuThreadState.FPR_I[21]);
			Assert.AreEqual(13, CpuThreadState.FPR_I[22]);
			Assert.AreEqual(14, CpuThreadState.FPR_I[23]);
			Assert.AreEqual(14, CpuThreadState.FPR_I[24]);
		}
	}

	public unsafe partial class CpuEmitterTest
	{
		[Inject]
		protected CpuProcessor CpuProcessor;

		[Inject]
		protected DynarecFunctionCompiler DynarecFunctionCompiler;

		protected CpuThreadState CpuThreadState;

		protected static PspConfig PspConfig;
		protected static PspEmulatorContext PspEmulatorContext;
		protected static PspMemory Memory;

		[ClassInitialize]
		public static void ClassInit(TestContext context)
		{
			PspConfig = new PspConfig();
			PspEmulatorContext = new PspEmulatorContext(PspConfig);
			PspEmulatorContext.SetInstanceType<PspMemory, LazyPspMemory>();
			Memory = PspEmulatorContext.GetInstance<PspMemory>();
		}

		[TestInitialize]
		public void SetUp()
		{
			PspEmulatorContext.InjectDependencesTo(this);
			CpuThreadState = new CpuThreadState(CpuProcessor);
		}

		protected void ExecuteAssembly(String Assembly, bool DoDebug = false, bool DoLog = false)
		{
			CpuProcessor.MethodCache.Clear();

			Assembly += "\r\nbreak\r\n";
			var MemoryStream = new MemoryStream();
			MemoryStream.PreservePositionAndLock(() =>
			{
				var MipsAssembler = new MipsAssembler(MemoryStream);

				MipsAssembler.Assemble(Assembly);
			});
			var InstructionReader = new InstructionStreamReader(MemoryStream);

			//Console.WriteLine(Assembly);

			Action<CpuThreadState> Method = (_CpuThreadState) =>
			{
				_CpuThreadState.PC = 0;

				//Console.WriteLine("PC: {0:X}", _CpuThreadState.PC);
				try
				{
					while (true)
					{
						//Console.WriteLine("PC: {0:X}", _CpuThreadState.PC);
						var Delegate = DynarecFunctionCompiler.CreateFunction(InstructionReader, _CpuThreadState.PC, null, DoDebug: DoDebug, DoLog: DoLog);
						_CpuThreadState.StepInstructionCount = 1000000;
						Delegate.Delegate(_CpuThreadState);
					}
				}
				catch (PspBreakException)
				{
				}
			};

			Method(CpuThreadState);
		}
	}
}

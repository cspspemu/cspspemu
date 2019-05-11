using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu.VFpu;
using CSPspEmu.Core.Memory;
using Xunit;

namespace Tests.CSPspEmu.Core.Cpu.Emitter
{
    
    public unsafe partial class CpuEmitterTest
    {
        [Fact(Skip = "Check")]
        public void SimplestTest()
        {
            //CpuThreadState.GPR[11] = 11;

            ExecuteAssembly(@"
				add  r1, r0, r0
			");

            //Assert.Equal(11, CpuThreadState.GPR[1]);
        }

        [Fact]
        public void TestRotating()
        {
            //CpuThreadState.GPR[11] = 0x;
            ExecuteAssembly(@"
				li r2 , 0b_10110111011110111110111111011111;
				li r11, 0b_11110110111011110111110111111011;
				rotr r1, r2, 3
			");
            Assert.Equal(CpuThreadState.Gpr[11], CpuThreadState.Gpr[1]);
        }

        [Fact]
        public void ArithmeticTest()
        {
            CpuThreadState.Gpr[1] = -1;
            CpuThreadState.Gpr[2] = -1;
            CpuThreadState.Gpr[3] = -1;
            CpuThreadState.Gpr[4] = -1;
            CpuThreadState.Gpr[11] = 11;
            CpuThreadState.Gpr[12] = 12;

            ExecuteAssembly(@"
				add  r1, r0, r11
				add  r2, r0, r12
				sub  r3, r2, r1
				addi r4, r0, 1234
			");

            Assert.Equal(11, CpuThreadState.Gpr[1]);
            Assert.Equal(12, CpuThreadState.Gpr[2]);
            Assert.Equal(1, CpuThreadState.Gpr[3]);
            Assert.Equal(1234, CpuThreadState.Gpr[4]);
        }

        [Fact]
        public void SubstractTest()
        {
            ExecuteAssembly(@"
				li r26, 0
				li r27, 1
				li r28, -1
				li r29, 0x7FFFFFFF
				li r30, 0x80000000

				sub r1, r26, r26
				sub r2, r26, r27
				sub r3, r26, r28
				sub r4, r26, r29
				sub r5, r26, r30

				sub r6, r27, r26
				sub r7, r27, r27
				sub r8, r27, r28
				sub r9, r27, r29
				sub r10, r27, r30

				sub r11, r28, r26
				sub r12, r28, r27
				sub r13, r28, r28
				sub r14, r28, r29
				sub r15, r28, r30

				sub r16, r29, r26
				sub r17, r29, r27
				sub r18, r29, r28
				sub r19, r29, r29
				sub r20, r29, r30

				sub r21, r30, r26
				sub r22, r30, r27
				sub r23, r30, r28
				sub r24, r30, r29
				sub r25, r30, r30
			");

            var expectedValues = new[]
            {
                0, -1, 1, -2147483647, -2147483648, 1, 0, 2, -2147483646, -2147483647, -1, -2, 0, -2147483648,
                2147483647, 2147483647, 2147483646, -2147483648, 0, -1, -2147483648, 2147483647, -2147483647, 1, 0
            };

            for (var n = 1; n <= 25; n++)
            {
                Assert.Equal(expectedValues[n - 1], CpuThreadState.Gpr[n]);
                //Console.WriteLine(CpuThreadState.GPR[n]);
            }
        }


        [Fact]
        public void OpRrrTest()
        {
            var results = new Queue<int>();

            var instructionNames = new[] {"add", "addu", "sub", "subu", "and", "or", "xor", "nor"};
            var numbers = new[]
            {
                0, 0, 1, -99999, 77777, -1, 0x12345678, unchecked((int) 0x87654321), int.MaxValue, int.MaxValue - 1,
                int.MinValue, int.MinValue + 1
            };
            var resultOffset = 17;
            var inputOffset = 0;

            foreach (var instructionName in instructionNames)
            {
                for (var n = 0; n < numbers.Length; n++)
                {
                    var assembly = "";

                    for (var m = 0; m < numbers.Length; m++)
                    {
                        CpuThreadState.Gpr[inputOffset + m] = numbers[m];

                        assembly += $"{instructionName} r{m + resultOffset}, r{n + inputOffset}, r{m + inputOffset}\n";
                    }

                    //Console.WriteLine("{0}", Assembly);

                    ExecuteAssembly(assembly);

                    for (var m = 0; m < numbers.Length; m++)
                    {
                        results.Enqueue(CpuThreadState.Gpr[m + resultOffset]);
                    }
                }
            }

	        /*
            var expectedLines = new Queue<string>(StringToLines(@"
				add.0: 0,0,1,-99999,77777,-1,305419896,-2023406815,2147483647,2147483646,-2147483648,-2147483647,
				add.0: 0,0,1,-99999,77777,-1,305419896,-2023406815,2147483647,2147483646,-2147483648,-2147483647,
				add.1: 1,1,2,-99998,77778,0,305419897,-2023406814,-2147483648,2147483647,-2147483647,-2147483646,
				add.-99999: -99999,-99999,-99998,-199998,-22222,-100000,305319897,-2023506814,2147383648,2147383647,2147383649,2147383650,
				add.77777: 77777,77777,77778,-22222,155554,77776,305497673,-2023329038,-2147405872,-2147405873,-2147405871,-2147405870,
				add.-1: -1,-1,0,-100000,77776,-2,305419895,-2023406816,2147483646,2147483645,2147483647,-2147483648,
				add.305419896: 305419896,305419896,305419897,305319897,305497673,305419895,610839792,-1717986919,-1842063753,-1842063754,-1842063752,-1842063751,
				add.-2023406815: -2023406815,-2023406815,-2023406814,-2023506814,-2023329038,-2023406816,-1717986919,248153666,124076832,124076831,124076833,124076834,
				add.2147483647: 2147483647,2147483647,-2147483648,2147383648,-2147405872,2147483646,-1842063753,124076832,-2,-3,-1,0,
				add.2147483646: 2147483646,2147483646,2147483647,2147383647,-2147405873,2147483645,-1842063754,124076831,-3,-4,-2,-1,
				add.-2147483648: -2147483648,-2147483648,-2147483647,2147383649,-2147405871,2147483647,-1842063752,124076833,-1,-2,0,1,
				add.-2147483647: -2147483647,-2147483647,-2147483646,2147383650,-2147405870,-2147483648,-1842063751,124076834,0,-1,1,2,
				addu.0: 0,0,1,-99999,77777,-1,305419896,-2023406815,2147483647,2147483646,-2147483648,-2147483647,
				addu.0: 0,0,1,-99999,77777,-1,305419896,-2023406815,2147483647,2147483646,-2147483648,-2147483647,
				addu.1: 1,1,2,-99998,77778,0,305419897,-2023406814,-2147483648,2147483647,-2147483647,-2147483646,
				addu.-99999: -99999,-99999,-99998,-199998,-22222,-100000,305319897,-2023506814,2147383648,2147383647,2147383649,2147383650,
				addu.77777: 77777,77777,77778,-22222,155554,77776,305497673,-2023329038,-2147405872,-2147405873,-2147405871,-2147405870,
				addu.-1: -1,-1,0,-100000,77776,-2,305419895,-2023406816,2147483646,2147483645,2147483647,-2147483648,
				addu.305419896: 305419896,305419896,305419897,305319897,305497673,305419895,610839792,-1717986919,-1842063753,-1842063754,-1842063752,-1842063751,
				addu.-2023406815: -2023406815,-2023406815,-2023406814,-2023506814,-2023329038,-2023406816,-1717986919,248153666,124076832,124076831,124076833,124076834,
				addu.2147483647: 2147483647,2147483647,-2147483648,2147383648,-2147405872,2147483646,-1842063753,124076832,-2,-3,-1,0,
				addu.2147483646: 2147483646,2147483646,2147483647,2147383647,-2147405873,2147483645,-1842063754,124076831,-3,-4,-2,-1,
				addu.-2147483648: -2147483648,-2147483648,-2147483647,2147383649,-2147405871,2147483647,-1842063752,124076833,-1,-2,0,1,
				addu.-2147483647: -2147483647,-2147483647,-2147483646,2147383650,-2147405870,-2147483648,-1842063751,124076834,0,-1,1,2,
				sub.0: 0,0,-1,99999,-77777,1,-305419896,2023406815,-2147483647,-2147483646,-2147483648,2147483647,
				sub.0: 0,0,-1,99999,-77777,1,-305419896,2023406815,-2147483647,-2147483646,-2147483648,2147483647,
				sub.1: 1,1,0,100000,-77776,2,-305419895,2023406816,-2147483646,-2147483645,-2147483647,-2147483648,
				sub.-99999: -99999,-99999,-100000,0,-177776,-99998,-305519895,2023306816,2147383650,2147383651,2147383649,2147383648,
				sub.77777: 77777,77777,77776,177776,0,77778,-305342119,2023484592,-2147405870,-2147405869,-2147405871,-2147405872,
				sub.-1: -1,-1,-2,99998,-77778,0,-305419897,2023406814,-2147483648,-2147483647,2147483647,2147483646,
				sub.305419896: 305419896,305419896,305419895,305519895,305342119,305419897,0,-1966140585,-1842063751,-1842063750,-1842063752,-1842063753,
				sub.-2023406815: -2023406815,-2023406815,-2023406816,-2023306816,-2023484592,-2023406814,1966140585,0,124076834,124076835,124076833,124076832,
				sub.2147483647: 2147483647,2147483647,2147483646,-2147383650,2147405870,-2147483648,1842063751,-124076834,0,1,-1,-2,
				sub.2147483646: 2147483646,2147483646,2147483645,-2147383651,2147405869,2147483647,1842063750,-124076835,-1,0,-2,-3,
				sub.-2147483648: -2147483648,-2147483648,2147483647,-2147383649,2147405871,-2147483647,1842063752,-124076833,1,2,0,-1,
				sub.-2147483647: -2147483647,-2147483647,-2147483648,-2147383648,2147405872,-2147483646,1842063753,-124076832,2,3,1,0,
				subu.0: 0,0,-1,99999,-77777,1,-305419896,2023406815,-2147483647,-2147483646,-2147483648,2147483647,
				subu.0: 0,0,-1,99999,-77777,1,-305419896,2023406815,-2147483647,-2147483646,-2147483648,2147483647,
				subu.1: 1,1,0,100000,-77776,2,-305419895,2023406816,-2147483646,-2147483645,-2147483647,-2147483648,
				subu.-99999: -99999,-99999,-100000,0,-177776,-99998,-305519895,2023306816,2147383650,2147383651,2147383649,2147383648,
				subu.77777: 77777,77777,77776,177776,0,77778,-305342119,2023484592,-2147405870,-2147405869,-2147405871,-2147405872,
				subu.-1: -1,-1,-2,99998,-77778,0,-305419897,2023406814,-2147483648,-2147483647,2147483647,2147483646,
				subu.305419896: 305419896,305419896,305419895,305519895,305342119,305419897,0,-1966140585,-1842063751,-1842063750,-1842063752,-1842063753,
				subu.-2023406815: -2023406815,-2023406815,-2023406816,-2023306816,-2023484592,-2023406814,1966140585,0,124076834,124076835,124076833,124076832,
				subu.2147483647: 2147483647,2147483647,2147483646,-2147383650,2147405870,-2147483648,1842063751,-124076834,0,1,-1,-2,
				subu.2147483646: 2147483646,2147483646,2147483645,-2147383651,2147405869,2147483647,1842063750,-124076835,-1,0,-2,-3,
				subu.-2147483648: -2147483648,-2147483648,2147483647,-2147383649,2147405871,-2147483647,1842063752,-124076833,1,2,0,-1,
				subu.-2147483647: -2147483647,-2147483647,-2147483648,-2147383648,2147405872,-2147483646,1842063753,-124076832,2,3,1,0,
				and.0: 0,0,0,0,0,0,0,0,0,0,0,0,
				and.0: 0,0,0,0,0,0,0,0,0,0,0,0,
				and.1: 0,0,1,1,1,1,0,1,1,0,0,1,
				and.-99999: 0,0,1,-99999,10561,-99999,305418336,-2023472863,2147383649,2147383648,-2147483648,-2147483647,
				and.77777: 0,0,1,10561,77777,77777,1616,66305,77777,77776,0,1,
				and.-1: 0,0,1,-99999,77777,-1,305419896,-2023406815,2147483647,2147483646,-2147483648,-2147483647,
				and.305419896: 0,0,0,305418336,1616,305419896,305419896,35930656,305419896,305419896,0,0,
				and.-2023406815: 0,0,1,-2023472863,66305,-2023406815,35930656,-2023406815,124076833,124076832,-2147483648,-2147483647,
				and.2147483647: 0,0,1,2147383649,77777,2147483647,305419896,124076833,2147483647,2147483646,0,1,
				and.2147483646: 0,0,0,2147383648,77776,2147483646,305419896,124076832,2147483646,2147483646,0,0,
				and.-2147483648: 0,0,0,-2147483648,0,-2147483648,0,-2147483648,0,0,-2147483648,-2147483648,
				and.-2147483647: 0,0,1,-2147483647,1,-2147483647,0,-2147483647,1,0,-2147483648,-2147483647,
				or.0: 0,0,1,-99999,77777,-1,305419896,-2023406815,2147483647,2147483646,-2147483648,-2147483647,
				or.0: 0,0,1,-99999,77777,-1,305419896,-2023406815,2147483647,2147483646,-2147483648,-2147483647,
				or.1: 1,1,1,-99999,77777,-1,305419897,-2023406815,2147483647,2147483647,-2147483647,-2147483647,
				or.-99999: -99999,-99999,-99999,-99999,-32783,-1,-98439,-33951,-1,-1,-99999,-99999,
				or.77777: 77777,77777,77777,-32783,77777,-1,305496057,-2023395343,2147483647,2147483647,-2147405871,-2147405871,
				or.-1: -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
				or.305419896: 305419896,305419896,305419897,-98439,305496057,-1,305419896,-1753917575,2147483647,2147483646,-1842063752,-1842063751,
				or.-2023406815: -2023406815,-2023406815,-2023406815,-33951,-2023395343,-1,-1753917575,-2023406815,-1,-1,-2023406815,-2023406815,
				or.2147483647: 2147483647,2147483647,2147483647,-1,2147483647,-1,2147483647,-1,2147483647,2147483647,-1,-1,
				or.2147483646: 2147483646,2147483646,2147483647,-1,2147483647,-1,2147483646,-1,2147483647,2147483646,-2,-1,
				or.-2147483648: -2147483648,-2147483648,-2147483647,-99999,-2147405871,-1,-1842063752,-2023406815,-1,-2,-2147483648,-2147483647,
				or.-2147483647: -2147483647,-2147483647,-2147483647,-99999,-2147405871,-1,-1842063751,-2023406815,-1,-1,-2147483647,-2147483647,
				xor.0: 0,0,1,-99999,77777,-1,305419896,-2023406815,2147483647,2147483646,-2147483648,-2147483647,
				xor.0: 0,0,1,-99999,77777,-1,305419896,-2023406815,2147483647,2147483646,-2147483648,-2147483647,
				xor.1: 1,1,0,-100000,77776,-2,305419897,-2023406816,2147483646,2147483647,-2147483647,-2147483648,
				xor.-99999: -99999,-99999,-100000,0,-43344,99998,-305516775,2023438912,-2147383650,-2147383649,2147383649,2147383648,
				xor.77777: 77777,77777,77776,-43344,0,-77778,305494441,-2023461648,2147405870,2147405871,-2147405871,-2147405872,
				xor.-1: -1,-1,-2,99998,-77778,0,-305419897,2023406814,-2147483648,-2147483647,2147483647,2147483646,
				xor.305419896: 305419896,305419896,305419897,-305516775,305494441,-305419897,0,-1789848231,1842063751,1842063750,-1842063752,-1842063751,
				xor.-2023406815: -2023406815,-2023406815,-2023406816,2023438912,-2023461648,2023406814,-1789848231,0,-124076834,-124076833,124076833,124076832,
				xor.2147483647: 2147483647,2147483647,2147483646,-2147383650,2147405870,-2147483648,1842063751,-124076834,0,1,-1,-2,
				xor.2147483646: 2147483646,2147483646,2147483647,-2147383649,2147405871,-2147483647,1842063750,-124076833,1,0,-2,-1,
				xor.-2147483648: -2147483648,-2147483648,-2147483647,2147383649,-2147405871,2147483647,-1842063752,124076833,-1,-2,0,1,
				xor.-2147483647: -2147483647,-2147483647,-2147483648,2147383648,-2147405872,2147483646,-1842063751,124076832,-2,-1,1,0,
				nor.0: -1,-1,-2,99998,-77778,0,-305419897,2023406814,-2147483648,-2147483647,2147483647,2147483646,
				nor.0: -1,-1,-2,99998,-77778,0,-305419897,2023406814,-2147483648,-2147483647,2147483647,2147483646,
				nor.1: -2,-2,-2,99998,-77778,0,-305419898,2023406814,-2147483648,-2147483648,2147483646,2147483646,
				nor.-99999: 99998,99998,99998,99998,32782,0,98438,33950,0,0,99998,99998,
				nor.77777: -77778,-77778,-77778,32782,-77778,0,-305496058,2023395342,-2147483648,-2147483648,2147405870,2147405870,
				nor.-1: 0,0,0,0,0,0,0,0,0,0,0,0,
				nor.305419896: -305419897,-305419897,-305419898,98438,-305496058,0,-305419897,1753917574,-2147483648,-2147483647,1842063751,1842063750,
				nor.-2023406815: 2023406814,2023406814,2023406814,33950,2023395342,0,1753917574,2023406814,0,0,2023406814,2023406814,
				nor.2147483647: -2147483648,-2147483648,-2147483648,0,-2147483648,0,-2147483648,0,-2147483648,-2147483648,0,0,
				nor.2147483646: -2147483647,-2147483647,-2147483648,0,-2147483648,0,-2147483647,0,-2147483648,-2147483647,1,0,
				nor.-2147483648: 2147483647,2147483647,2147483646,99998,2147405870,0,1842063751,2023406814,0,1,2147483647,2147483646,
				nor.-2147483647: 2147483646,2147483646,2147483646,99998,2147405870,0,1842063750,2023406814,0,0,2147483646,2147483646,
			"));
	        */

            foreach (var instructionName in instructionNames)
            {
                foreach (var leftNumber in numbers)
                {
                    var line = $"{instructionName}.{leftNumber}: ";
                    foreach (var unused in numbers)
                    {
                        line += $"{results.Dequeue()},";
                    }
                    Console.WriteLine(line);
                    //Assert.Equal(ExpectedLines.Dequeue(), Line);
                }
            }
        }

        private static IEnumerable<string> StringToLines(string str)
        {
            return str.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0);
        }

        [Fact(Skip = "check")]
        public void OpRriTest()
        {
            var results = new Queue<int>();

            var instructionNames = new[] {"addi", "addiu", "andi", "ori", "xori"};
            var numbers = new[]
            {
                0, 0, 1, -99999, 77777, -1, 0x12345678, unchecked((int) 0x87654321), int.MaxValue, int.MaxValue - 1,
                int.MinValue, int.MinValue + 1
            };
            var resultOffset = 17;
            var inputOffset = 0;

            foreach (var instructionName in instructionNames)
            {
                for (var n = 0; n < numbers.Length; n++)
                {
                    var assembly = "";

                    for (var m = 0; m < numbers.Length; m++)
                    {
                        CpuThreadState.Gpr[inputOffset + m] = numbers[m];

                        assembly +=
	                        $"{instructionName} r{m + resultOffset}, r{n + inputOffset}, {(numbers[m] & 0xFFFF)}\n";
                    }

                    //Console.WriteLine("{0}", Assembly);

                    ExecuteAssembly(assembly);

                    for (var m = 0; m < numbers.Length; m++)
                    {
                        results.Enqueue(CpuThreadState.Gpr[m + resultOffset]);
                    }
                }
            }

            var expectedLines = new Queue<string>(StringToLines(@"
				addi.0: 0,0,1,31073,12241,-1,22136,17185,-1,-2,0,1,
				addi.0: 0,0,1,31073,12241,-1,22136,17185,-1,-2,0,1,
				addi.1: 1,1,2,31074,12242,0,22137,17186,0,-1,1,2,
				addi.-99999: -99999,-99999,-99998,-68926,-87758,-100000,-77863,-82814,-100000,-100001,-99999,-99998,
				addi.77777: 77777,77777,77778,108850,90018,77776,99913,94962,77776,77775,77777,77778,
				addi.-1: -1,-1,0,31072,12240,-2,22135,17184,-2,-3,-1,0,
				addi.305419896: 305419896,305419896,305419897,305450969,305432137,305419895,305442032,305437081,305419895,305419894,305419896,305419897,
				addi.-2023406815: -2023406815,-2023406815,-2023406814,-2023375742,-2023394574,-2023406816,-2023384679,-2023389630,-2023406816,-2023406817,-2023406815,-2023406814,
				addi.2147483647: 2147483647,2147483647,-2147483648,-2147452576,-2147471408,2147483646,-2147461513,-2147466464,2147483646,2147483645,2147483647,-2147483648,
				addi.2147483646: 2147483646,2147483646,2147483647,-2147452577,-2147471409,2147483645,-2147461514,-2147466465,2147483645,2147483644,2147483646,2147483647,
				addi.-2147483648: -2147483648,-2147483648,-2147483647,-2147452575,-2147471407,2147483647,-2147461512,-2147466463,2147483647,2147483646,-2147483648,-2147483647,
				addi.-2147483647: -2147483647,-2147483647,-2147483646,-2147452574,-2147471406,-2147483648,-2147461511,-2147466462,-2147483648,2147483647,-2147483647,-2147483646,
				addiu.0: 0,0,1,31073,12241,-1,22136,17185,-1,-2,0,1,
				addiu.0: 0,0,1,31073,12241,-1,22136,17185,-1,-2,0,1,
				addiu.1: 1,1,2,31074,12242,0,22137,17186,0,-1,1,2,
				addiu.-99999: -99999,-99999,-99998,-68926,-87758,-100000,-77863,-82814,-100000,-100001,-99999,-99998,
				addiu.77777: 77777,77777,77778,108850,90018,77776,99913,94962,77776,77775,77777,77778,
				addiu.-1: -1,-1,0,31072,12240,-2,22135,17184,-2,-3,-1,0,
				addiu.305419896: 305419896,305419896,305419897,305450969,305432137,305419895,305442032,305437081,305419895,305419894,305419896,305419897,
				addiu.-2023406815: -2023406815,-2023406815,-2023406814,-2023375742,-2023394574,-2023406816,-2023384679,-2023389630,-2023406816,-2023406817,-2023406815,-2023406814,
				addiu.2147483647: 2147483647,2147483647,-2147483648,-2147452576,-2147471408,2147483646,-2147461513,-2147466464,2147483646,2147483645,2147483647,-2147483648,
				addiu.2147483646: 2147483646,2147483646,2147483647,-2147452577,-2147471409,2147483645,-2147461514,-2147466465,2147483645,2147483644,2147483646,2147483647,
				addiu.-2147483648: -2147483648,-2147483648,-2147483647,-2147452575,-2147471407,2147483647,-2147461512,-2147466463,2147483647,2147483646,-2147483648,-2147483647,
				addiu.-2147483647: -2147483647,-2147483647,-2147483646,-2147452574,-2147471406,-2147483648,-2147461511,-2147466462,-2147483648,2147483647,-2147483647,-2147483646,
				andi.0: 0,0,0,0,0,0,0,0,0,0,0,0,
				andi.0: 0,0,0,0,0,0,0,0,0,0,0,0,
				andi.1: 0,0,1,1,1,1,0,1,1,0,0,1,
				andi.-99999: 0,0,1,31073,10561,31073,20576,16673,31073,31072,0,1,
				andi.77777: 0,0,1,10561,12241,12241,1616,769,12241,12240,0,1,
				andi.-1: 0,0,1,31073,12241,65535,22136,17185,65535,65534,0,1,
				andi.305419896: 0,0,0,20576,1616,22136,22136,16928,22136,22136,0,0,
				andi.-2023406815: 0,0,1,16673,769,17185,16928,17185,17185,17184,0,1,
				andi.2147483647: 0,0,1,31073,12241,65535,22136,17185,65535,65534,0,1,
				andi.2147483646: 0,0,0,31072,12240,65534,22136,17184,65534,65534,0,0,
				andi.-2147483648: 0,0,0,0,0,0,0,0,0,0,0,0,
				andi.-2147483647: 0,0,1,1,1,1,0,1,1,0,0,1,
				ori.0: 0,0,1,31073,12241,65535,22136,17185,65535,65534,0,1,
				ori.0: 0,0,1,31073,12241,65535,22136,17185,65535,65534,0,1,
				ori.1: 1,1,1,31073,12241,65535,22137,17185,65535,65535,1,1,
				ori.-99999: -99999,-99999,-99999,-99999,-98319,-65537,-98439,-99487,-65537,-65537,-99999,-99999,
				ori.77777: 77777,77777,77777,98289,77777,131071,98297,94193,131071,131071,77777,77777,
				ori.-1: -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
				ori.305419896: 305419896,305419896,305419897,305430393,305430521,305463295,305419896,305420153,305463295,305463294,305419896,305419897,
				ori.-2023406815: -2023406815,-2023406815,-2023406815,-2023392415,-2023395343,-2023358465,-2023401607,-2023406815,-2023358465,-2023358465,-2023406815,-2023406815,
				ori.2147483647: 2147483647,2147483647,2147483647,2147483647,2147483647,2147483647,2147483647,2147483647,2147483647,2147483647,2147483647,2147483647,
				ori.2147483646: 2147483646,2147483646,2147483647,2147483647,2147483647,2147483647,2147483646,2147483647,2147483647,2147483646,2147483646,2147483647,
				ori.-2147483648: -2147483648,-2147483648,-2147483647,-2147452575,-2147471407,-2147418113,-2147461512,-2147466463,-2147418113,-2147418114,-2147483648,-2147483647,
				ori.-2147483647: -2147483647,-2147483647,-2147483647,-2147452575,-2147471407,-2147418113,-2147461511,-2147466463,-2147418113,-2147418113,-2147483647,-2147483647,
				xori.0: 0,0,1,31073,12241,65535,22136,17185,65535,65534,0,1,
				xori.0: 0,0,1,31073,12241,65535,22136,17185,65535,65534,0,1,
				xori.1: 1,1,0,31072,12240,65534,22137,17184,65534,65535,1,0,
				xori.-99999: -99999,-99999,-100000,-131072,-108880,-96610,-119015,-116160,-96610,-96609,-99999,-100000,
				xori.77777: 77777,77777,77776,87728,65536,118830,96681,93424,118830,118831,77777,77776,
				xori.-1: -1,-1,-2,-31074,-12242,-65536,-22137,-17186,-65536,-65535,-1,-2,
				xori.305419896: 305419896,305419896,305419897,305409817,305428905,305441159,305397760,305403225,305441159,305441158,305419896,305419897,
				xori.-2023406815: -2023406815,-2023406815,-2023406816,-2023409088,-2023396112,-2023375650,-2023418535,-2023424000,-2023375650,-2023375649,-2023406815,-2023406816,
				xori.2147483647: 2147483647,2147483647,2147483646,2147452574,2147471406,2147418112,2147461511,2147466462,2147418112,2147418113,2147483647,2147483646,
				xori.2147483646: 2147483646,2147483646,2147483647,2147452575,2147471407,2147418113,2147461510,2147466463,2147418113,2147418112,2147483646,2147483647,
				xori.-2147483648: -2147483648,-2147483648,-2147483647,-2147452575,-2147471407,-2147418113,-2147461512,-2147466463,-2147418113,-2147418114,-2147483648,-2147483647,
				xori.-2147483647: -2147483647,-2147483647,-2147483648,-2147452576,-2147471408,-2147418114,-2147461511,-2147466464,-2147418114,-2147418113,-2147483647,-2147483648,
			"));

            foreach (var instructionName in instructionNames)
            {
                foreach (var leftNumber in numbers)
                {
                    var line = $"{instructionName}.{leftNumber}: ";
                    foreach (var unused in numbers)
                    {
                        line += $"{results.Dequeue()},";
                    }
                    //Console.WriteLine(Line);
                    Assert.Equal(expectedLines.Dequeue(), line);
                }
            }
        }

        [Fact]
        public void SyscallTest()
        {
            var events = new List<int>();

            CpuProcessor.RegisterNativeSyscall(1, () => { events.Add(1); });
            CpuProcessor.RegisterNativeSyscall(1000, () => { events.Add(1000); });

            ExecuteAssembly(@"
				syscall 1
				syscall 1000
			");

            Assert.Equal("[1,1000]", events.ToJson());
        }

        [Fact]
        public void BranchTest()
        {
            ExecuteAssembly(@"
				beq r3, r0, label1
				li r3, 1
				li r1, 1
			label1:
				li r2, 1
			");

            Assert.Equal(
                "[0,1,1]",
                CpuThreadState.GprItems(1, 2, 3).ToJson()
            );

            //Assert.Equal("[1,1000]", Events.ToJson());
        }

        [Fact]
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

            Assert.Equal(2, CpuThreadState.Gpr[1]);
            Assert.Equal(0, CpuThreadState.Gpr[2]);
        }

        [Fact]
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

            Assert.Equal(1, CpuThreadState.Gpr[2]);
            Assert.Equal(0, CpuThreadState.Gpr[3]);
        }

        [Fact(Skip = "check")]
        public void BranchFullTest()
        {
            var regsV = new[]
            {
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

            var regs0 = new[]
            {
                "r10",
                "r11",
                "r12",
            };

	        IEnumerable<int> Generator(string branch, string[] regsList)
	        {
		        var results = new List<int>();
		        foreach (var regs in regsList)
		        {
			        ExecuteAssembly(@"
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
						".Replace("%BRANCH%", branch)
				        .Replace("%REGS%", regs));

			        results.Add(CpuThreadState.Gpr[1]);
		        }

		        return results;
	        }

	        Assert.Equal("[1,0,0,0,1,0,0,0,1]", Generator("beq", regsV).ToJson());
            Assert.Equal("[0,1,1,1,0,1,1,1,0]", Generator("bne", regsV).ToJson());

            Assert.Equal("[1,0,0]", Generator("bltz", regs0).ToJson());
            Assert.Equal("[1,1,0]", Generator("blez", regs0).ToJson());
            Assert.Equal("[0,0,1]", Generator("bgtz", regs0).ToJson());
            Assert.Equal("[0,1,1]", Generator("bgez", regs0).ToJson());

            //Assert.Equal("[0,0,1]", Generator("bgtzl", Regs0).ToJson());
        }

        [Fact]
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

            Assert.Equal(-1, CpuThreadState.Gpr[1]);
            Assert.Equal(11, CpuThreadState.Gpr[2]);
        }

        [Fact]
        public void BitrevTest()
        {
            ExecuteAssembly(@"
				li r1, 0b_00000011111101111101111011101101
				bitrev r2, r1
			");

            Assert.Equal("00000011111101111101111011101101", "%032b".Sprintf(CpuThreadState.Gpr[1]));
            Assert.Equal("10110111011110111110111111000000", "%032b".Sprintf(CpuThreadState.Gpr[2]));
        }

        [Fact]
        public void LoadStoreTest()
        {
            ExecuteAssembly(@"
				li r1, 0x12345678
				li r2, 0x88000000
				sw r1, 0(r2)
				lb r3, 0(r2)          ; Little Endian
			");

            Assert.Equal("12345678", "%08X".Sprintf(CpuThreadState.Gpr[1]));
            Assert.Equal("88000000", "%08X".Sprintf(CpuThreadState.Gpr[2]));
            Assert.Equal("00000078", "%08X".Sprintf(CpuThreadState.Gpr[3]));
        }

        [Fact]
        public void FloatTest()
        {
            //CpuThreadState.FPR[27] = 2.0f;
            //CpuThreadState.FPR[28] = 20.0f;
            CpuThreadState.Fpr[29] = 81.0f;
            CpuThreadState.Fpr[30] = -1.0f;
            CpuThreadState.Fpr[31] = 3.5f;
            var memoryValue = -2.0f;

            CpuThreadState.Memory.WriteSafe(0x88000000, memoryValue);

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

				li r10, 0x88000000
				lwc1 f14, 0(r10)
				swc1 f14, 4(r10)
				lwc1 f15, 4(r10)
			");

            // Unary
            Assert.Equal(CpuThreadState.Fpr[0], CpuThreadState.Fpr[30]);
            Assert.Equal(CpuThreadState.Fpr[1], -CpuThreadState.Fpr[31]);
            Assert.Equal(CpuThreadState.Fpr[2], Math.Sqrt(CpuThreadState.Fpr[29]));
            Assert.Equal(CpuThreadState.Fpr[3], Math.Abs(CpuThreadState.Fpr[30]));
            Assert.Equal(CpuThreadState.Fpr[4], Math.Abs(CpuThreadState.Fpr[31]));

            Assert.Equal(CpuThreadState.Fpr[10], CpuThreadState.Fpr[30] + CpuThreadState.Fpr[31]);
            Assert.Equal(CpuThreadState.Fpr[11], CpuThreadState.Fpr[30] - CpuThreadState.Fpr[31]);
            Assert.Equal(CpuThreadState.Fpr[12], CpuThreadState.Fpr[30] / CpuThreadState.Fpr[31]);
            Assert.Equal(CpuThreadState.Fpr[13], CpuThreadState.Fpr[30] * CpuThreadState.Fpr[31]);
            Assert.Equal(CpuThreadState.Fpr[14], memoryValue);
            Assert.Equal(CpuThreadState.Fpr[15], memoryValue);
        }

        [Fact]
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
            Assert.Equal("10110111011110111110111111000000", "%032b".Sprintf(CpuThreadState.Gpr[10]));
            Assert.Equal(7, CpuThreadState.Gpr[12]);

            Assert.Equal("10111101111101111110000000000000", "%032b".Sprintf(CpuThreadState.Gpr[1]));
            Assert.Equal("10111101111101111110000000000000", "%032b".Sprintf(CpuThreadState.Gpr[2]));
            Assert.Equal("00000001011011101111011111011111", "%032b".Sprintf(CpuThreadState.Gpr[3]));
            Assert.Equal("00000001011011101111011111011111", "%032b".Sprintf(CpuThreadState.Gpr[4]));
            Assert.Equal("11111111011011101111011111011111", "%032b".Sprintf(CpuThreadState.Gpr[5]));
            Assert.Equal("11111111011011101111011111011111", "%032b".Sprintf(CpuThreadState.Gpr[6]));
            Assert.Equal("00000000101101110111101111101111", "%032b".Sprintf(CpuThreadState.Gpr[7]));
            Assert.Equal("00000000101101110111101111101111", "%032b".Sprintf(CpuThreadState.Gpr[8]));
        }

        [Fact]
        public void SetLessThanImmediateTest()
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

            Assert.Equal(0, CpuThreadState.Gpr[1]);
            Assert.Equal(1, CpuThreadState.Gpr[2]);
            Assert.Equal(0, CpuThreadState.Gpr[3]);
            Assert.Equal(0, CpuThreadState.Gpr[4]);
        }

        [Fact]
        public void SetLessThanTest()
        {
            ExecuteAssembly(@"
				li r1, 0x77777777
				li r10, 0
				li r11, -100
				li r12, +100

				li r20, 0				
				li r21, 7
				li r22, -200

				sltu r1, r10, r20
				sltu r2, r10, r21
				sltu r3, r11, r22
				slt  r4, r11, r22
			");

            Assert.Equal(0, CpuThreadState.Gpr[1]);
            Assert.Equal(1, CpuThreadState.Gpr[2]);
            Assert.Equal(0, CpuThreadState.Gpr[3]);
            Assert.Equal(0, CpuThreadState.Gpr[4]);
        }

        [Fact]
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

            Assert.Equal(CpuThreadState.Gpr[21], CpuThreadState.Lo);
            Assert.Equal(CpuThreadState.Gpr[22], CpuThreadState.Hi);
            Assert.Equal(CpuThreadState.Gpr[1], CpuThreadState.Lo);
            Assert.Equal(CpuThreadState.Gpr[2], CpuThreadState.Hi);
        }

        [Fact]
        public void SetDivTest()
        {
            ExecuteAssembly(@"
				li r10, 100
				li r11, 12
				div r10, r11
			");

            Assert.Equal(4, CpuThreadState.Hi);
            Assert.Equal(8, CpuThreadState.Lo);
        }

        [Fact]
        public void SetMulSimpleTest()
        {
            ExecuteAssembly(@"
				li r10, 7
				li r11, 13
				mult r10, r11
			");

            var expected = ((long) CpuThreadState.Gpr[10] * CpuThreadState.Gpr[11]);

            Assert.Equal((uint) ((expected >> 0) & 0xFFFFFFFF), (uint) CpuThreadState.Lo);
            Assert.Equal((uint) ((expected >> 32) & 0xFFFFFFFF), (uint) CpuThreadState.Hi);
        }

        [Fact]
        public void SetMulTest()
        {
            ExecuteAssembly(@"
				li r10, 0x12345678
				li r11, 0x87654321
				mult r10, r11
			");

            var expected = (long) CpuThreadState.Gpr[10] * CpuThreadState.Gpr[11];

            //Console.WriteLine(CpuThreadState.GPR[10]);
            //Console.WriteLine(CpuThreadState.GPR[11]);
            //Console.WriteLine(Expected);

            Assert.Equal((uint) ((expected >> 0) & 0xFFFFFFFF), (uint) CpuThreadState.Lo);
            Assert.Equal((uint) ((expected >> 32) & 0xFFFFFFFF), (uint) CpuThreadState.Hi);
        }

        [Fact]
        public void SetMultuTest()
        {
            ExecuteAssembly(@"
				li r10, 0x12345678
				li r11, 0x87654321
				multu r10, r11
			");

            var expected = ((ulong) (uint) CpuThreadState.Gpr[10] * (uint) CpuThreadState.Gpr[11]);

            //Console.WriteLine(CpuThreadState.GPR[10]);
            //Console.WriteLine(CpuThreadState.GPR[11]);
            //Console.WriteLine(Expected);

            Assert.Equal((uint) ((expected >> 0) & 0xFFFFFFFF), (uint) CpuThreadState.Lo);
            Assert.Equal((uint) ((expected >> 32) & 0xFFFFFFFF), (uint) CpuThreadState.Hi);
        }

        [Fact]
        public void TrySet0Test()
        {
            ExecuteAssembly(@"
				li r0, 0x12345678
			");

            Assert.Equal(0, CpuThreadState.Gpr[0]);
        }

        [Fact]
        public void TryDecWithAddTest()
        {
            ExecuteAssembly(@"
				li r1, 100
				addiu r1, r1, -1
			");

            Assert.Equal(99, CpuThreadState.Gpr[1]);
        }

        [Fact]
        public void SignExtendTest()
        {
            ExecuteAssembly(@"
				li  r10, 0x666666_81
				li  r11, 0x6666_8123
				or  r1, r0, r10
				seb r2, r10
				or  r3, r0, r11
				seh r4, r11
			");

            Assert.Equal("66666681", $"{CpuThreadState.Gpr[1]:X8}");
            Assert.Equal("FFFFFF81", $"{CpuThreadState.Gpr[2]:X8}");
            Assert.Equal("66668123", $"{CpuThreadState.Gpr[3]:X8}");
            Assert.Equal("FFFF8123", $"{CpuThreadState.Gpr[4]:X8}");
        }

        [Fact]
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

            Assert.Equal(0x000, CpuThreadState.Gpr[1]);
            Assert.Equal(0x777, CpuThreadState.Gpr[2]);

            Assert.Equal(0x777, CpuThreadState.Gpr[3]);
            Assert.Equal(0x000, CpuThreadState.Gpr[4]);
        }

        [Fact]
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

            Assert.Equal(+100, CpuThreadState.Gpr[1]);
            Assert.Equal(+100, CpuThreadState.Gpr[2]);
            Assert.Equal(-100, CpuThreadState.Gpr[3]);
            Assert.Equal(-100, CpuThreadState.Gpr[4]);
        }

        [Fact]
        public void LoadStoreFpuTest()
        {
            CpuThreadState.Fpr[0] = 1.0f;
            ExecuteAssembly(@"
				li r1, 0x08000000
				swc1 f0, 0(r1)
				lwc1 f1, 0(r1)
			");
            Assert.Equal(0x3F800000, (int) Memory.ReadSafe<uint>(0x08000000));
            Assert.Equal(CpuThreadState.Fpr[1], CpuThreadState.Fpr[0]);
        }

        [Fact]
        public void MoveFpuTest()
        {
            CpuThreadState.Gpr[1] = 17;
            CpuThreadState.Fpr[2] = 8.3f;
            ExecuteAssembly(@"
				mtc1 r1, f1
				mfc1 r2, f2
			");
            Assert.Equal(CpuThreadState.Gpr[1], CpuThreadState.FprI[1]);
            Assert.Equal(CpuThreadState.FprI[2], CpuThreadState.Gpr[2]);
        }

        [Fact]
        public void OrNorTest()
        {
            ExecuteAssembly(@"
				li r20, 0b_11000110001100011000110001100011
				li r21, 0b_00010000100001000010000100001000
				or  r1, r20, r21
				nor r2, r20, r21
			");

            Assert.Equal("11010110101101011010110101101011", "%032b".Sprintf(CpuThreadState.Gpr[1]));
            Assert.Equal("00101001010010100101001010010100", "%032b".Sprintf(CpuThreadState.Gpr[2]));
        }

        [Fact]
        public void ExtractTest()
        {
            // %t, %s, %a, %ne
            ExecuteAssembly(@"
				li r2, 0b_11000011001000000011111011101101
				ext r1, r2, 3, 10
			");

            Assert.Equal("1111011101", "%010b".Sprintf(CpuThreadState.Gpr[1]));
        }

        [Fact]
        public void InsertTest()
        {
            // %t, %s, %a, %ne
            ExecuteAssembly(@"
				li r22, 0b_11011111110111111011111011101101
				li r23, 0b_00100000001000000100000100010010
				
				add r2, r22, r0
				add r3, r22, r0

				ins r2, r23, 0, 8
				ins r3, r23, 4, 10
			");

            //Assert.Inconclusive();

            Console.WriteLine("{0}", "%032b".Sprintf(CpuThreadState.Gpr[22]));
            Console.WriteLine("{0}", "%032b".Sprintf(CpuThreadState.Gpr[23]));
            Console.WriteLine("{0}", "%032b".Sprintf(CpuThreadState.Gpr[2]));
            Console.WriteLine("{0}", "%032b".Sprintf(CpuThreadState.Gpr[3]));

            Assert.Equal("11011111110111111011111011101101", "%032b".Sprintf(CpuThreadState.Gpr[22]));
            Assert.Equal("00100000001000000100000100010010", "%032b".Sprintf(CpuThreadState.Gpr[23]));
            Assert.Equal("11011111110111111011111000010010", "%032b".Sprintf(CpuThreadState.Gpr[2]));
            Assert.Equal("11011111110111111001000100101101", "%032b".Sprintf(CpuThreadState.Gpr[3]));
        }

        [Fact(Skip = "check")]
        public void FloatCompTest()
        {
            // %t, %s, %a, %ne
            CpuThreadState.Fpr[1] = 1.0f;
            CpuThreadState.Fpr[2] = 2.0f;

            ExecuteAssembly(@"c.eq.s f1, f2");
            Assert.Equal(false, CpuThreadState.Fcr31.Cc);

            ExecuteAssembly(@"c.eq.s f1, f1");
            Assert.Equal(true, CpuThreadState.Fcr31.Cc);

            ExecuteAssembly(@"c.lt.s f1, f2");
            Assert.Equal(true, CpuThreadState.Fcr31.Cc);

            ExecuteAssembly(@"c.lt.s f2, f1");
            Assert.Equal(false, CpuThreadState.Fcr31.Cc);

            ExecuteAssembly(@"c.lt.s f1, f1");
            Assert.Equal(false, CpuThreadState.Fcr31.Cc);

	        void Gen(string instructionName)
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
				".Replace("%INSTRUCTION_NAME%", instructionName));
	        }

	        Gen("bc1t");
            Assert.Equal(1, CpuThreadState.Gpr[1]);

            Gen("bc1f");
            Assert.Equal(0, CpuThreadState.Gpr[1]);
        }

        [Fact]
        public void FloatControlRegisterTest()
        {
            CpuThreadState.Fcr31.Value = 0x12345678;
            ExecuteAssembly(@"
				li r2, 0x87654321
				cfc1 r1, 31
				ctc1 r2, 31
			");
            Assert.Equal(0x12345678, CpuThreadState.Gpr[1]);
            Assert.Equal(0x87654321, CpuThreadState.Fcr31.Value);
        }

        [Fact]
        public void WsbwTest()
        {
            ExecuteAssembly(@"
				li  r11, 0x_12_34_56_78

				wsbw r1, r11
				wsbh r2, r11
			");

            Assert.Equal("12345678", $"{CpuThreadState.Gpr[11]:X8}");
            Assert.Equal("78563412", $"{CpuThreadState.Gpr[1]:X8}");
            Assert.Equal("34127856", $"{CpuThreadState.Gpr[2]:X8}");
        }

        [Fact]
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
            Assert.Equal(32, CpuThreadState.Gpr[1]);
            Assert.Equal(0, CpuThreadState.Gpr[2]);

            // r12
            Assert.Equal(0, CpuThreadState.Gpr[3]);
            Assert.Equal(32, CpuThreadState.Gpr[4]);

            // r13
            Assert.Equal(0, CpuThreadState.Gpr[5]);
            Assert.Equal(3, CpuThreadState.Gpr[6]);

            // r14
            Assert.Equal(3, CpuThreadState.Gpr[7]);
            Assert.Equal(0, CpuThreadState.Gpr[8]);

            // r15
            Assert.Equal(3, CpuThreadState.Gpr[9]);
            Assert.Equal(0, CpuThreadState.Gpr[10]);
        }

        [Fact]
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
            CpuThreadState.Gpr[31] -= (int) PspMemory.ScratchPadOffset;
            CpuThreadState.Pc -= PspMemory.ScratchPadOffset;

            Assert.Equal(1, CpuThreadState.Gpr[1]);
            Assert.Equal(2, CpuThreadState.Gpr[2]);
            Assert.Equal(4 * 4, CpuThreadState.Gpr[31]);
            Assert.Equal(7 * 4, (int) CpuThreadState.Pc);
        }

        private static string AssemblySingleInstruction(string text)
        {
            var data = new byte[4];
            fixed (byte* dataPtr = data)
            {
                var instruction = (new MipsAssembler(new MemoryStream())).AssembleInstruction(text);
                *(uint*) dataPtr = instruction;
            }
            return $"{data[0]:X2}{data[1]:X2}{data[2]:X2}{data[3]:X2}";
        }

	    // ReSharper disable once UnusedParameter.Local
        private void TestAssembly(string encoded, string assembly)
        {
            //Assert.Equal(Encoded, AssemblySingleInstruction(Assembly), Assembly);
	        Assert.Equal(encoded, AssemblySingleInstruction(assembly));
        }

        [Fact]
        public void VfpuAssemblyRegisterVector4Test()
        {
            TestAssembly("A08000D0", "vmov.q  R000.q, C000.q");
            TestAssembly("AC8C00D0", "vmov.q  R300.q, C300.q");
            TestAssembly("BC9C00D0", "vmov.q  R700.q, C700.q");
            TestAssembly("A59900D0", "vmov.q  R101.q, C610.q");
            TestAssembly("AA9200D0", "vmov.q  R202.q, C420.q");
            TestAssembly("B78300D0", "vmov.q  R503.q, C030.q");
        }

        [Fact]
        public void VfpuAssemblyRegisterVector3Test()
        {
            TestAssembly("248400D0", "vmov.t  R100.t, C100.t");
            TestAssembly("6BCB00D0", "vmov.t  R213.t, C231.t");
            TestAssembly("2E8E00D0", "vmov.t  R302.t, C320.t");
        }

        [Fact]
        public void VfpuAssemblyRegisterVector2Test()
        {
            TestAssembly("A00000D0", "vmov.p  R000.p, C000.p");
            TestAssembly("AD0D00D0", "vmov.p  R301.p, C310.p");
            TestAssembly("AF0F00D0", "vmov.p  R303.p, C330.p");
            TestAssembly("FD5D00D0", "vmov.p  R721.p, C712.p");
            TestAssembly("FF5F00D0", "vmov.p  R723.p, C732.p");
        }

        [Fact]
        public void VfpuAssemblyRegisterSingleTest()
        {
            TestAssembly("667A00D0", "vmov.s  S123.s, S623.s");
        }

        [Fact]
        public void VfpuAssemblyLvTest()
        {
            TestAssembly("00007ED8", "lv.q C720.q, 0+r3");
            TestAssembly("3000CBD8", "lv.q C230.q, 0x30+r6");
        }

        [Fact]
        public void VfpuAssemblyTest()
        {
            TestAssembly("A08007D0", "vone.q R000");
            TestAssembly("000448D0", "vsrt3.s S000.s, S100.s");
            TestAssembly("80844AD0", "vsgn.q  C000.q, C100.q");
        }

        [Fact]
        public void VfpuAssemblyPrefixTest()
        {
            TestAssembly("00F000DC", "vpfxs [0, 0, 0, 0]");
            TestAssembly("E51000DC", "vpfxs [1, y, z, w]");
            TestAssembly("550000DE", "vpfxd [0:1, 0:1, 0:1, 0:1]");
            TestAssembly("24F800DC", "vpfxs [0, 1, 2, 3]");
            TestAssembly("100B00DE", "vpfxd [m, m, 0:1, m]");
            TestAssembly("18F100DC", "vpfxs [3, 2, 1, 0]");
            TestAssembly("65200ADC", "vpfxs [y, -1, z, -y]");
        }

        [Fact(Skip = "not working yet")]
        public void VfpuAssemblyVrotTest()
        {
            TestAssembly("B434A4F3", "vrot.p  R500, S501, [c,s]");
        }

        [Fact]
        public void VfpuAssemblyTest2()
        {
            //var Instruction = (Instruction)MathUtils.ByteSwap(0xD67A5EF0);
            //{
            //	var Instruction = (Instruction)MathUtils.ByteSwap(0xF65A7EF0);
            //	Console.WriteLine("%032b".Sprintf((uint)Instruction));
            //	Console.WriteLine("%07b".Sprintf((uint)Instruction.VD));
            //	Console.WriteLine("%07b".Sprintf((uint)Instruction.VS));
            //	Console.WriteLine("%07b".Sprintf((uint)Instruction.VT));
            //}
            //{
            //	var Instruction = (Instruction)MathUtils.ByteSwap(0xD67A5EF0);
            //	Console.WriteLine("%032b".Sprintf((uint)Instruction));
            //	Console.WriteLine("%07b".Sprintf((uint)Instruction.VD));
            //	Console.WriteLine("%07b".Sprintf((uint)Instruction.VS));
            //	Console.WriteLine("%07b".Sprintf((uint)Instruction.VT));
            //}

            //Console.WriteLine("%07b".Sprintf((uint)Instruction.VD.M_TRANSPOSED));
            //Console.WriteLine("%07b".Sprintf((uint)Instruction.VS.M_TRANSPOSED));
            //Console.WriteLine("%07b".Sprintf((uint)Instruction.VT.M_TRANSPOSED));

            TestAssembly("A08428F0", "vmmul.q E000.q, E100.q, E200.q");
            TestAssembly("D67A5EF0", "vmmul.p M522.p, M622.p, M722.p");
            TestAssembly("F65A7EF0", "vmmul.p E522.p, E622.p, E722.p");
        }

        [Fact(Skip = "not working yet")]
        public void VfpuAssemblyTest3()
        {
            TestAssembly("802488F0", "vhtfm2.p C000.p, E100.p, C200.p");
            TestAssembly("802408F1", "vhtfm3.t C000.t, E100.t, C200.t");
        }

        [Fact]
        public void VfpuColorConversionEncode()
        {
            TestAssembly("818059D0", "vt4444.q C010.p, C000.q");
            TestAssembly("81805AD0", "vt5551.q C010.p, C000.q");
            TestAssembly("81805BD0", "vt5650.q C010.p, C000.q");
        }

        [Fact]
        public void VfpuTransferUnalignedTest()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            CpuThreadState.Gpr[4] = (int) PspMemory.MainOffset;
            var ptrIn = (float*) Memory.PspAddressToPointerSafe((uint) CpuThreadState.Gpr[4]);
            ptrIn[0] = 1f;
            ptrIn[1] = 2f;
            ptrIn[2] = 3f;
            ptrIn[3] = 4f;

            ExecuteAssembly(@"
				lvl.q C100, 12+r4
				lvr.q C100, 0+r4
			");

            CpuThreadState.DumpVfpuRegisters(Console.Error);

            Assert.Equal("1,2,3,4", string.Join(",", CpuThreadState.Vfpr[4, "C100"]));
        }

        [Fact]
        public void VfpuColorConversion()
        {
            CpuThreadState.Gpr[4] = (int) PspMemory.MainOffset;
            //CpuThreadState.GPR[5] = (int)PspMemory.MainOffset + 0x100;
            var ptrIn = (uint*) Memory.PspAddressToPointerSafe((uint) CpuThreadState.Gpr[4]);
            //var PtrOut = (ushort*)Memory.PspAddressToPointerSafe((uint)CpuThreadState.GPR[5]);

            ptrIn[0] = 0xFFFF00FF;
            ptrIn[1] = 0x801100FF;
            ptrIn[2] = 0x7F5500FF;
            ptrIn[3] = 0x00aa00FF;

            ExecuteAssembly(@"
				lvl.q C000, 12+r4
				lvr.q C000, 0+r4
				vt4444.q C010, C000
			");

            Assert.Equal("810FFF0F,0A0F750F",
                string.Join(",",
                    CpuThreadState.Vfpr["C010.p"]
                        .Select(item => $"{MathFloat.ReinterpretFloatAsInt(item):X8}")));

            //Assert.Equal(0xFF0F, PtrOut[0]);
            //Assert.Equal(0x810F, PtrOut[1]);
            //Assert.Equal(0x750F, PtrOut[2]);
            //Assert.Equal(0x0A0F, PtrOut[3]);
        }

        [Fact]
        public void VfpuConstantsTest()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            var names = VfpuConstants.Constants.Select(info => info.Name).ToArray();
            var assembly = "";

	        void Iterate(Action<string, int, int, int, int> action)
	        {
		        for (var n = 0; n < names.Length; n++)
		        {
			        var constantName = names[n];
			        var matrix = (n >> 4) & 7;
			        var column = (n >> 2) & 3;
			        var row = (n >> 0) & 3;

			        action(constantName, n, matrix, column, row);
		        }
	        }

	        Iterate((constantName, n, matrix, column, row) =>
            {
                assembly += $"vcst.s S{matrix}{column}{row}, {constantName}\n";
            });

            Console.WriteLine("{0}", assembly);

            ExecuteAssembly(assembly);

            CpuThreadState.DumpVfpuRegisters(Console.Error);

            Iterate((constantName, n, matrix, column, row) =>
            {
                Console.WriteLine(
                    "{0}, {1}",
                    VfpuUtils.GetRegisterName(matrix, column, row) + " : " + constantName + " : " +
                    VfpuConstants.GetConstantValueByName(constantName).Value,
                    VfpuUtils.GetRegisterName(matrix, column, row) + " : " + constantName + " : " +
                    CpuThreadState.Vfpr[matrix, column, row]
                );
                Assert.Equal(
                    VfpuUtils.GetRegisterName(matrix, column, row) + " : " + constantName + " : " +
                    VfpuConstants.GetConstantValueByName(constantName).Value,
                    VfpuUtils.GetRegisterName(matrix, column, row) + " : " + constantName + " : " +
                    CpuThreadState.Vfpr[matrix, column, row]
                );
            });
        }

        [Fact(Skip = "review")]
        public void VfpuAddSubTest()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            CpuThreadState.Vfpr[4, "R100"] = new float[] {1, 2, 3, 4};
            CpuThreadState.Vfpr[4, "R101"] = new float[] {50, 60, 70, 80};

            ExecuteAssembly(@"
				vadd.q C200, R100, R101
				vsub.q C210, R100, R101
			");

            CpuThreadState.DumpVfpuRegisters(Console.Error);

	        Assert.Equal("50,60.5,70.25,80.16666", string.Join(",", CpuThreadState.Vfpr[4, "C210"]));
            Assert.Equal("-49,-58,-67,-76", string.Join(",", CpuThreadState.Vfpr[4, "C210"]));
        }

        [Fact(Skip = "not working yet")]
        public void VfpuPrefixTest()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            CpuThreadState.Vfpr[4, "R100"] = new float[] {1, 2, 0, 0};
            CpuThreadState.Vfpr[4, "R101"] = new float[] {50, 60, 70, 80};

            ExecuteAssembly(@"
				vpfxs [x,-x,y,-y]
				vadd.q C200, R100, R101

				vpfxs [0, 1/2, 1/4, 1/6]
				vadd.q C210, R100, R101

				vpfxs [0, 0, 0, 0]
				vpfxt [-2, 2, -2, 2]
				vpfxd [0:1, M, -1:1, 0:1]
				vadd.q C220, R100, R101
			");

            CpuThreadState.DumpVfpuRegisters(Console.Error);

            Assert.Equal("51,59,72,78", string.Join(",", CpuThreadState.Vfpr[4, "C200"]));
            Assert.Equal("50,60,70,80", string.Join(",", CpuThreadState.Vfpr[4, "C210"]));
            Assert.Equal("0,2,-1,1", string.Join(",", CpuThreadState.Vfpr[4, "C220"]));
        }

        [Fact]
        public void VfpuZeroOneMov2Test()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            ExecuteAssembly(@"
				vone.p C120
				vzero.p C122
			");

            CpuThreadState.DumpVfpuRegisters(Console.Error);

            Assert.Equal(1f, CpuThreadState.Vfpr[1, 2, 0]);
            Assert.Equal(1f, CpuThreadState.Vfpr[1, 2, 1]);
            Assert.Equal(0f, CpuThreadState.Vfpr[1, 2, 2]);
            Assert.Equal(0f, CpuThreadState.Vfpr[1, 2, 3]);
        }

        /*
        [Fact]
        public void VfpuZeroOneMov3Test()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            ExecuteAssembly(@"
                vone.p R120
                vzero.p R122
            ");

            CpuThreadState.DumpVfpuRegisters(Console.Error);

            Assert.Equal(1f, CpuThreadState.Vfpr[1, 2, 0]);
            Assert.Equal(1f, CpuThreadState.Vfpr[1, 2, 1]);
            Assert.Equal(0f, CpuThreadState.Vfpr[1, 2, 2]);
            Assert.Equal(0f, CpuThreadState.Vfpr[1, 2, 3]);
        }
        */

        [Fact]
        public void VfpuSvQ()
        {
            var address = 0x08800000U;
            CpuThreadState.Gpr[10] = (int) address;
            Memory.WriteSafe(address + 0x10 + 0, 1f);
            Memory.WriteSafe(address + 0x10 + 4, 2f);
            Memory.WriteSafe(address + 0x10 + 8, 3f);
            Memory.WriteSafe(address + 0x10 + 12, 4f);
            ExecuteAssembly(@"
				vpfxs [0, 1/2, 1/4, 1/6]
				vpfxt [0, 0, 0, 0]
				vadd.q R000, R000, R000
				sv.q    R000, 0x10+r10
			");
            Assert.Equal(0f, Memory.ReadSafe<float>(address + 0x10 + 0));
            Assert.Equal(1f / 2f, Memory.ReadSafe<float>(address + 0x10 + 4));
            Assert.Equal(1f / 4f, Memory.ReadSafe<float>(address + 0x10 + 8));
            Assert.Equal(1f / 6f, Memory.ReadSafe<float>(address + 0x10 + 12));
        }

        [Fact]
        public void VfpuLvQ()
        {
            var address = 0x08800000U;
            CpuThreadState.Gpr[10] = (int) address;
            Memory.WriteSafe(address + 0x10 + 0, 1f);
            Memory.WriteSafe(address + 0x10 + 4, 2f);
            Memory.WriteSafe(address + 0x10 + 8, 3f);
            Memory.WriteSafe(address + 0x10 + 12, 4f);
            ExecuteAssembly(@"
				lv.q    R000, 0x10+r10
			");
            Assert.Equal("1,2,3,4", string.Join(",", CpuThreadState.Vfpr[4, "R000"]));
        }

        [Fact(Skip = "not working yet")]
        public void VfpuMtvViim()
        {
            CpuThreadState.Gpr[10] = MathFloat.ReinterpretFloatAsInt(777f);

            ExecuteAssembly(@"
				mtv r10, S501
				viim S502, -3
				viim S503, 32767
				viim S504, -1
			");

            Assert.Equal(777f, CpuThreadState.Vfpr["S501.s"][0]);
            Assert.Equal(-3f, CpuThreadState.Vfpr["S502.s"][0]);
            Assert.Equal(32767f, CpuThreadState.Vfpr["S503.s"][0]);
            Assert.Equal(-1f, CpuThreadState.Vfpr["S504.s"][0]);
        }

        [Fact(Skip = "not working yet")]
        public void VFim()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            ExecuteAssembly(@"
				vfim.s	 S000, 00
				vfim.s	 S001, 01
				vfim.s	 S002, 02
				vfim.s	 S003, 03
				vfim.s	 S010, 10
				vfim.s	 S011, 11
				vfim.s	 S012, 12
				vfim.s	 S013, 13
				vfim.s	 S020, 20
				vfim.s	 S021, 21
				vfim.s	 S022, 22
				vfim.s	 S023, 23
				vfim.s	 S030, 30
				vfim.s	 S031, 31
				vfim.s	 S032, 32
				vfim.s	 S033, 33
				vfim.s	 S200, -1
				vmscl.q M100, E000, S200
			");

            Assert.Equal(",", string.Join(",", CpuThreadState.Vfpr["E000.q"]));
        }

        [Fact(Skip = "not working yet")]
        public void Vfad()
        {
            CpuThreadState.Vfpr["C100.q"] = new float[] {3, 7, 11, 13};

            ExecuteAssembly(@"
				vfad.q  S000.s, C100.q
			");

            Assert.Equal(3 + 7 + 11 + 13, CpuThreadState.Vfpr["S000.s"][0]);
        }


        [Fact(Skip = "not working yet")]
        public void Vrot()
        {
            CpuThreadState.Gpr[10] = MathFloat.ReinterpretFloatAsInt(0.2f);

            ExecuteAssembly(@"
				mtv r10, S000
				vrot.p	R500, S000, [c, s]
				vrot.q	R600, S000, [c, 0, -s, 0]
			");

            Assert.Equal("0.9510565,0.309017", string.Join(",", CpuThreadState.Vfpr["R500.p"]));
            Assert.Equal("0.9510565,0,-0.309017,0", string.Join(",", CpuThreadState.Vfpr["R600.q"]));
        }

        [Fact(Skip = "not working yet")]
        public void VfpuMatrixMultAndMov()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            CpuThreadState.Vfpr["M100.q"] = new float[]
            {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 10, 11, 12,
                13, 14, 15, 16,
            };

            CpuThreadState.Vfpr["M200.q"] = new float[]
            {
                17, 18, 19, 20,
                21, 22, 23, 24,
                25, 26, 27, 28,
                29, 30, 31, 32,
            };

            ExecuteAssembly(@"
				vmmul.q M000, M100, M200
				vmmov.q M400, M000
			");

            Assert.Equal(
                "250,260,270,280," +
                "618,644,670,696," +
                "986,1028,1070,1112," +
                "1354,1412,1470,1528",
                string.Join(",", CpuThreadState.Vfpr["M000.q"])
            );

            Assert.Equal(
                string.Join(",", CpuThreadState.Vfpr["M400.q"]),
                string.Join(",", CpuThreadState.Vfpr["M000.q"])
            );
        }

        [Fact]
        public void VfpuVMidtZeroOne()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            ExecuteAssembly(@"
				vmidt.q M300
				vmzero.q M400
				vmone.q M500
			");

            Assert.Equal(
                "1,0,0,0," +
                "0,1,0,0," +
                "0,0,1,0," +
                "0,0,0,1"
                ,
                string.Join(",", CpuThreadState.Vfpr["M300.q"])
            );

            Assert.Equal(
                "0,0,0,0," +
                "0,0,0,0," +
                "0,0,0,0," +
                "0,0,0,0"
                ,
                string.Join(",", CpuThreadState.Vfpr["M400.q"])
            );

            Assert.Equal(
                "1,1,1,1," +
                "1,1,1,1," +
                "1,1,1,1," +
                "1,1,1,1"
                ,
                string.Join(",", CpuThreadState.Vfpr["M500.q"])
            );
        }

        [Fact]
        public void VfpuZeroOneMov4Test()
        {
            CpuThreadState.Vfpr.ClearAll(float.NaN);

            var vector0 = "0,0,0,0";
            var vector1 = "1,1,1,1";
            var vector2 = "0,1,1,0";
            //CpuThreadState.Vfpr[4, "R320"] = new float[] { 2, 2, 2, 2 };

            ExecuteAssembly(@"
				vzero.q R100
				vone.q  R101
				vone.q  R102
				vzero.q R103

				vzero.q C200
				vone.q  C210
				vone.q  C220
				vzero.q C230

				vmov.q R400, C100
			");

            CpuThreadState.DumpVfpuRegisters(Console.Error);

	        string GetLine(string name) => CpuThreadState.Vfpr[4, name].JoinToString(",");

	        Assert.Equal(vector0, GetLine("R100"));
            Assert.Equal(vector1, GetLine("R101"));
            Assert.Equal(vector1, GetLine("R102"));
            Assert.Equal(vector0, GetLine("R103"));

            Assert.Equal(vector2, GetLine("C100"));
            Assert.Equal(vector2, GetLine("C110"));
            Assert.Equal(vector2, GetLine("C120"));
            Assert.Equal(vector2, GetLine("C130"));

            Assert.Equal(vector0, GetLine("C200"));
            Assert.Equal(vector1, GetLine("C210"));
            Assert.Equal(vector1, GetLine("C220"));
            Assert.Equal(vector0, GetLine("C230"));

            Assert.Equal(vector2, GetLine("R200"));
            Assert.Equal(vector2, GetLine("R201"));
            Assert.Equal(vector2, GetLine("R202"));
            Assert.Equal(vector2, GetLine("R203"));

            Assert.Equal(vector2, GetLine("R400"));
        }

        [Fact]
        public void JumpTest2()
        {
            var events = new List<int>();

            CpuProcessor.RegisterNativeSyscall(1, () => { events.Add(1); });
            CpuProcessor.RegisterNativeSyscall(2, () => { events.Add(2); });
            CpuProcessor.RegisterNativeSyscall(3, () => { events.Add(3); });
            CpuProcessor.RegisterNativeSyscall(4, () => { events.Add(4); });

            ExecuteAssembly(@"
				syscall 1
				j skip
				nop
				syscall 2
			skip:
				syscall 3
			");

            Assert.Equal("[1,3]", events.ToJson());
        }

        [Fact]
        public void JalTest()
        {
            var events = new List<int>();

            CpuProcessor.RegisterNativeSyscall(1, () => { events.Add(1); });
            CpuProcessor.RegisterNativeSyscall(2, () => { events.Add(2); });
            CpuProcessor.RegisterNativeSyscall(3, () => { events.Add(3); });
            CpuProcessor.RegisterNativeSyscall(4, () => { events.Add(4); });

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

            Assert.Equal("[1,2,2,2,3,4]", events.ToJson());
            Assert.Equal(103, CpuThreadState.Gpr[1]);
        }

        [Fact]
        public void JalTest2()
        {
            ExecuteAssembly(@"
				li r2, 0
				li r3, 0

				jal function1
				nop
				jal function1
				nop
				jal function1
				nop

				j end
				nop

			function1:
				addiu r29, r29, -4
				sw r31, 0(r29)

				jal function2
				nop
				jal function2
				nop
				jal function2
				nop

				addiu r29, r29, 4
				lw r31, 0(r29)

				jr r31
				addi r3, r3, 1

			function2:
				jr r31
				addi r2, r2, 1

			end:
			");

            Assert.Equal(3 * 3, CpuThreadState.Gpr[2]);
            Assert.Equal(3, CpuThreadState.Gpr[3]);
        }

        [Fact]
        public void BltzalTest()
        {
            var events = new List<int>();

            CpuProcessor.DebugFunctionCreation = true;
            CpuProcessor.RegisterNativeSyscall(1, () => { events.Add(1); });
            CpuProcessor.RegisterNativeSyscall(2, () => { events.Add(2); });
            CpuProcessor.RegisterNativeSyscall(3, () => { events.Add(3); });
            CpuProcessor.RegisterNativeSyscall(4, () => { events.Add(4); });

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

            Assert.Equal("[2,3]", events.ToJson());
        }

        [Fact]
        public void ConvertFloat1Test()
        {
            ExecuteAssembly(@"
				li r1, 100
				mtc1 r1, f1
				cvt.s.w f2, f1
			");

            Assert.Equal(100.0f, CpuThreadState.Fpr[2]);
        }

        [Fact(Skip = "check")]
        public void LoadUnalignedTest()
        {
            var value = 0x87654321;
            for (var n = 0; n <= 7; n++)
            {
                var offset = (uint) n;
                const uint Base = 0x08010000U;
                Memory.WriteSafe(Base + offset, value);
                CpuThreadState.Gpr[2] = (int) Base;
	            // ReSharper disable once UseStringInterpolation
                ExecuteAssembly(string.Format(@"
					lwl r1, {0}(r2)
					lwr r1, {1}(r2)
				", offset + 3, offset + 0));
                Assert.Equal($"{value:X8}", $"{CpuThreadState.Gpr[1]:X8}");
            }
        }

        [Fact(Skip = "check")]
        public void LoadInvalidUnalignedTest()
        {
            var value = 0x87654321;
            var offset = (uint) 3;
            var Base = (uint) 0x08010000;
            Memory.WriteSafe(Base + offset, value);
            CpuThreadState.Gpr[2] = (int) Base;
	        // ReSharper disable once UseStringInterpolation
            ExecuteAssembly(string.Format(@"
				lwl r1, {0}(r2)
				sll r1, r1, 31
				lwr r1, {1}(r2)
			", offset + 3, offset + 0));
            Assert.Equal($"{0x21:X8}", $"{CpuThreadState.Gpr[1]:X8}");
        }

        [Fact(Skip = "check")]
        public void LoadInvalidUnalignedTest2()
        {
            var value = 0x87654321;
            var offset = (uint) 3;
            var Base = (uint) 0x08010000;
            Memory.WriteSafe(Base + offset, value);
            Memory.WriteSafe(Base + offset + 0x10, 0x00000000U);
            CpuThreadState.Gpr[2] = (int) Base;
	        // ReSharper disable once UseStringInterpolation
            ExecuteAssembly(string.Format(@"
				lwl r1, {0}(r2)
				addi r2, r2, 0x10
				lwr r1, {1}(r2)
			", offset + 3, offset + 0));
            Assert.Equal($"{0x87654300:X8}", $"{CpuThreadState.Gpr[1]:X8}");
        }

        [Fact]
        public void StoreUnalignedTest()
        {
            var value = 0x87654321U;
            CpuThreadState.Gpr[1] = (int) value;
            CpuThreadState.Gpr[2] = 0x08010000;
            ExecuteAssembly(@"
				swl r1, 7(r2)
				swr r1, 4(r2)
			");
            Assert.Equal(value, Memory.ReadSafe<uint>(0x08010004));
        }

        [Fact]
        public void ConvertFloat2Test()
        {
            CpuThreadState.Fpr[29] = 13.4f;
            CpuThreadState.Fpr[30] = 13.6f;
            CpuThreadState.Fpr[31] = 13.5f;

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

            Assert.Equal(13, CpuThreadState.FprI[1]);
            Assert.Equal(13, CpuThreadState.FprI[2]);
            Assert.Equal(13, CpuThreadState.FprI[3]);
            Assert.Equal(14, CpuThreadState.FprI[4]);

            Assert.Equal(13, CpuThreadState.FprI[11]);
            Assert.Equal(13, CpuThreadState.FprI[12]);
            Assert.Equal(14, CpuThreadState.FprI[13]);
            Assert.Equal(14, CpuThreadState.FprI[14]);

            Assert.Equal(13, CpuThreadState.FprI[21]);
            Assert.Equal(13, CpuThreadState.FprI[22]);
            Assert.Equal(14, CpuThreadState.FprI[23]);
            Assert.Equal(14, CpuThreadState.FprI[24]);
        }
    }

    public partial class CpuEmitterTest
    {
        protected CpuProcessor CpuProcessor;
        protected CpuThreadState CpuThreadState;

        protected PspMemory Memory => CpuProcessor.Memory;

	    public CpuEmitterTest()
        {
            CpuProcessor = CpuUtils.CreateCpuProcessor();
            CpuThreadState = new CpuThreadState(CpuProcessor);
        }

        protected void ExecuteAssembly(string assembly, bool doDebug = false, bool doLog = false)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(GlobalConfig.ThreadCultureName);
            CpuProcessor.MethodCache.FlushAll();

            var startOffset = PspMemory.ScratchPadOffset;
            assembly =
	            $".code 0x{startOffset:X8}\r\n" +
                assembly +
                "\r\nbreak 0\r\n"
                ;
            var assemblerStream = new PspMemoryStream(Memory);
            new MipsAssembler(assemblerStream).Assemble(assembly);

            try
            {
                CpuThreadState.Sp = PspMemory.MainSegment.High - 0x10;
                CpuThreadState.ExecuteAt(startOffset);
            }
            catch (PspBreakException)
            {
            }
        }
    }
}
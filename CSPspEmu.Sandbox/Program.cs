using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using CSPspEmu.Core.Cpu.Table;
using System.Runtime.InteropServices;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using System.IO;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Tests;

namespace CSPspEmu.Sandbox
{
	unsafe class Program
	{
		static void Test()
		{
			var Memory = new FastPspMemory();
			var MemoryStream = new PspMemoryStream(Memory);
			var BinaryWriter = new BinaryWriter(MemoryStream);
			var BinaryReader = new BinaryReader(MemoryStream);
			//var Memory = new NormalMemory();
			var Processor = new Processor();

			BinaryWriter.BaseStream.Position = FastPspMemory.MainOffset;
			BinaryWriter.BaseStream.PreservePositionAndLock(() =>
			{
				BinaryWriter.Write((uint)0xFFFFFFFF);
			});

			Console.WriteLine("{0:X}", BinaryReader.ReadUInt32());

			Memory.Write4(FastPspMemory.MainOffset, 0x12345678);
			Console.WriteLine("{0:X}", Memory.Read4(FastPspMemory.MainOffset));

			Processor.RegisterNativeSyscall(100, () =>
			{
				Console.WriteLine("syscall!");
			});
			Processor.GPR[2] = 10;
			Processor.GPR[3] = 20;
			Processor.ExecuteAssembly(@"
				add r1, r2, r3
				addi r4, r1, 1000
				li r7, 777
				lui r8, 0xFEDC
				ori r8, r8, 0x1234
				li r9, 0xFEDCBA98
				li r10, -3
				syscall 100
			");
			Console.WriteLine("{0}", Processor.GPR[4]);
			Console.WriteLine("{0}", Processor.GPR[7]);
			Console.WriteLine("{0:X}", Processor.GPR[8]);
			Console.WriteLine("{0:X}", Processor.GPR[9]);
			Console.WriteLine("{0}", Processor.GPR[10]);
			Console.ReadKey();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://en.wikipedia.org/wiki/Common_Intermediate_Language"/>
		/// <see cref="http://en.wikipedia.org/wiki/List_of_CIL_instructions"/>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Console.WriteLine("[1]");
			new CpuEmiterTest().BranchFullTest();
			Console.WriteLine("[2]");
			Console.ReadKey();
		}
	}
}

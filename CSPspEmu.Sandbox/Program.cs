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
using System.Diagnostics;
using CSPspEmu.Hle;
using CSPspEmu.Core.Cpu.Cpu;

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
			var Processor = new Processor(Memory);
			var CpuThreadState = new CpuThreadState(Processor);

			BinaryWriter.BaseStream.Position = FastPspMemory.MainOffset;
			BinaryWriter.BaseStream.PreservePositionAndLock(() =>
			{
				BinaryWriter.Write((uint)0xFFFFFFFF);
			});

			Console.WriteLine("{0:X}", BinaryReader.ReadUInt32());

			Memory.Write4(FastPspMemory.MainOffset, 0x12345678);
			Console.WriteLine("{0:X}", Memory.Read4(FastPspMemory.MainOffset));

			CpuThreadState.RegisterNativeSyscall(100, () =>
			{
				Console.WriteLine("syscall!");
			});
			CpuThreadState.GPR[2] = 10;
			CpuThreadState.GPR[3] = 20;
			CpuThreadState.ExecuteAssembly(@"
				add r1, r2, r3
				addi r4, r1, 1000
				li r7, 777
				lui r8, 0xFEDC
				ori r8, r8, 0x1234
				li r9, 0xFEDCBA98
				li r10, -3
				syscall 100
			");
			Console.WriteLine("{0}", CpuThreadState.GPR[4]);
			Console.WriteLine("{0}", CpuThreadState.GPR[7]);
			Console.WriteLine("{0:X}", CpuThreadState.GPR[8]);
			Console.WriteLine("{0:X}", CpuThreadState.GPR[9]);
			Console.WriteLine("{0}", CpuThreadState.GPR[10]);
			Console.ReadKey();
		}

		static void PerfTest()
		{

			/*
			debug047:004DF3AC ; ---------------------------------------------------------------------------
			debug047:004DF3AC mov     eax, [esi+1Ch]
			debug047:004DF3AF mov     dword ptr [eax+4], offset unk_20000
			debug047:004DF3B6 mov     eax, [esi+1Ch]
			debug047:004DF3B9 or      dword ptr [eax+4], 2000h
			debug047:004DF3C0 mov     eax, [esi+1Ch]
			debug047:004DF3C3 mov     dword ptr [eax+8], 88000000h
			debug047:004DF3CA mov     eax, [esi+1Ch]
			debug047:004DF3CD mov     edx, [esi+1Ch]
			debug047:004DF3D0 mov     edx, [edx+8]
			debug047:004DF3D3 mov     [eax+8], edx
			debug047:004DF3D6 mov     eax, [esi+1Ch]
			debug047:004DF3D9 mov     dword ptr [eax+0Ch], 77h
			debug047:004DF3E0
			debug047:004DF3E0 loc_4DF3E0:                             ; CODE XREF: debug047:004DF3FDj
			 * addi r1, r1, -1
			debug047:004DF3E0   mov     eax, [esi+1Ch]
			debug047:004DF3E3   dec     dword ptr [eax+4]
			 * BranchFlag = ... ; bgez r1, loop
			debug047:004DF3E6   mov     eax, [esi+1Ch]
			debug047:004DF3E9   cmp     dword ptr [eax+4], 0
			debug047:004DF3ED   setnl   al
			debug047:004DF3F0   mov     [esi+2Ch], al
			 * addi r2, r2, 1
			debug047:004DF3F3   mov     eax, [esi+1Ch]
			debug047:004DF3F6   inc     dword ptr [eax+8]
			 * bgez r1, loop
			debug047:004DF3F9   cmp     byte ptr [esi+2Ch], 0
			debug047:004DF3FD   jnz     short loc_4DF3E0
			debug047:004DF3FF pop     esi
			debug047:004DF400 pop     ebp
			debug047:004DF401 retn
			debug047:004DF401 ; ---------------------------------------------------------------------------
			 * 
			 * 
			 * 
			debug045:002DF65C ; ---------------------------------------------------------------------------
			debug045:002DF65C mov     dword ptr [esi+20h], offset unk_20000
			debug045:002DF663 or      dword ptr [esi+20h], 2000h
			debug045:002DF66A mov     dword ptr [esi+24h], 88000000h
			 * WAHT!
			debug045:002DF671 mov     eax, [esi+24h]
			debug045:002DF674 mov     [esi+24h], eax
			 * 
			debug045:002DF677 mov     dword ptr [esi+28h], 77h
			debug045:002DF67E
			debug045:002DF67E loc_2DF67E:                             ; CODE XREF: debug045:002DF698j
			 * addi r1, r1, -1
			debug045:002DF67E   dec     dword ptr [esi+20h]
			 * BranchFlag = ... ; bgez r1, loop
			debug045:002DF681   cmp     dword ptr [esi+20h], 0
			debug045:002DF685   setnl   al
			debug045:002DF688   mov     [esi+124h], al
			 * addi r2, r2, 1
			debug045:002DF68E   inc     dword ptr [esi+24h]
			 *  bgez r1, loop
			debug045:002DF691   cmp     byte ptr [esi+124h], 0
			debug045:002DF698   jnz     short loc_2DF67E
			debug045:002DF69A pop     esi
			debug045:002DF69B pop     ebp
			debug045:002DF69C retn
			debug045:002DF69C ; ---------------------------------------------------------------------------
			*/

			Console.WriteLine("[a]");
			var Memory = new FastPspMemory();
			//var Memory = new NormalPspMemory();
			//var Memory = new LazyPspMemory();
			var Processor = new Processor(Memory);
			var CpuThreadState = new CpuThreadState(Processor);
			//var Processor = new Processor(new NormalPspMemory());
			Memory.Write4(0x04000000, 0x12345678);
			Console.WriteLine("[b]");

			var Action = CpuThreadState.CreateDelegateForString(@"
				li r1, 139264
				li r2, 0x04000000
				li r3, 0x77
			loop:
				sb r3, 0(r2)
				addi r1, r1, -1
				bgez r1, loop
				addi r2, r2, 1
			");

			Console.WriteLine("[c]");

			var Stopwatch = new Stopwatch();
			Stopwatch.Start();
			Action(CpuThreadState);
			Action(CpuThreadState);
			Action(CpuThreadState);
			Action(CpuThreadState);
			Stopwatch.Stop();

			Console.WriteLine("%08X".Sprintf(Memory.Read4(0x04000000)));

			Console.WriteLine(Stopwatch.Elapsed);
			Console.WriteLine(Stopwatch.ElapsedMilliseconds);
		}

		static void ThreadTest()
		{
			var Memory = new LazyPspMemory();
			var Processor = new Processor(Memory);
			var CpuThreadState = new CpuThreadState(Processor);
			var Thread = new HlePspThread(CpuThreadState);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://en.wikipedia.org/wiki/Common_Intermediate_Language"/>
		/// <see cref="http://en.wikipedia.org/wiki/List_of_CIL_instructions"/>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			//var Processor = new Processor(new LazyPspMemory());
			//Processor.TestLoadReg(Processor);
			PerfTest();
			//ThreadTest();
			Console.ReadKey();
			/*
			Console.WriteLine("[1]");
			new CpuEmiterTest().BranchFullTest();
			Console.WriteLine("[2]");
			Console.ReadKey();
			*/
		}
	}
}

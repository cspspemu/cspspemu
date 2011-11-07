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
using CSPspEmu.Core.Cpu.Assembler;
using System.Threading;

namespace CSPspEmu.Sandbox
{
	unsafe class Program
	{
		static void ThreadTest()
		{
			var Memory = new LazyPspMemory();
			var Processor = new Processor(Memory);
			var CpuThreadState = new CpuThreadState(Processor);

			var MipsAssembler = new MipsAssembler(new PspMemoryStream(Memory));

			MipsAssembler.Assemble(@"
			.code 0x08000000
				li r31, 0x08000000
			start:
				li r1, 139264
				li r2, 0x04000000
				li r3, 0x77
			loop:
				sb r3, 0(r2)
				addi r1, r1, -1
				bgez r1, loop
				addi r2, r2, 1
				j start
				;jr r31
				nop
			");

			MipsAssembler.Assemble(@"
			.code 0x08000800
				li r31, 0x08000800
			start:
				li r1, 139264
				li r2, 0x04000000
				li r3, 0x77
			loop:
				sb r3, 0(r2)
				addi r1, r1, -1
				bgez r1, loop
				addi r2, r2, 1
				j start
				;jr r31
				nop
			");

			var ThreadManager = new HlePspThreadManager(Processor);
			var Thread1 = ThreadManager.Create();
			Thread1.CpuThreadState.PC = 0x08000000;

			var Thread2 = ThreadManager.Create();
			Thread2.CpuThreadState.PC = 0x08000800;

			while (true)
			{
				Console.WriteLine("%08X".Sprintf(ThreadManager.Next.CpuThreadState.PC));
				ThreadManager.StepNext();
				//Thread.Sleep(500);
				/*
				Thread1.Step();
				Console.WriteLine("Thread1.PC: %08X".Sprintf(Thread1.CpuThreadState.PC));
				Thread2.Step();
				Console.WriteLine("Thread2.PC: %08X".Sprintf(Thread2.CpuThreadState.PC));
				*/
			}
		}

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

		static void MiniFire()
		{
			// COPY_TO    : 08900000
			// ENTRY_POINT: 08900008
			byte[] MiniFireData = {
				0x00, 0x00, 0x01, 0x01, 0x55, 0x4E, 0x50, 0x00, 0x90, 0x08, 0x10, 0x3C,
				0x04, 0x00, 0x04, 0x26, 0x50, 0x00, 0x05, 0x26, 0x14, 0x00, 0x06, 0x24,
				0x02, 0x00, 0x07, 0x3C, 0x00, 0x80, 0x08, 0x3C, 0x21, 0x48, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x4C, 0x1B, 0x08, 0x00, 0x21, 0x20, 0x40, 0x00,
				0x21, 0x28, 0x00, 0x00, 0x21, 0x30, 0x00, 0x00, 0xCC, 0x1B, 0x08, 0x00,
				0x21, 0x20, 0x00, 0x00, 0x4C, 0x1C, 0x08, 0x00, 0x20, 0xF6, 0xBD, 0x27,
				0x21, 0x20, 0xA0, 0x03, 0xCC, 0x2F, 0x08, 0x00, 0xA0, 0x08, 0x11, 0x3C,
				0xB0, 0x08, 0x12, 0x3C, 0x00, 0x44, 0x10, 0x3C, 0x21, 0x20, 0x00, 0x00,
				0xE0, 0x01, 0x05, 0x24, 0x10, 0x01, 0x06, 0x24, 0x8C, 0x4E, 0x08, 0x00,
				0x21, 0x10, 0x20, 0x02, 0x00, 0xB6, 0x08, 0x34, 0x00, 0x00, 0x40, 0xA0,
				0xFF, 0xFF, 0x08, 0x25, 0xFD, 0xFF, 0x01, 0x05, 0x01, 0x00, 0x42, 0x24,
				0x00, 0xB4, 0x34, 0x36, 0xFF, 0x01, 0x93, 0x26, 0x21, 0x20, 0xA0, 0x03,
				0x0C, 0x30, 0x08, 0x00, 0x01, 0x00, 0x94, 0x26, 0x2B, 0x18, 0x93, 0x02,
				0xFB, 0xFF, 0x60, 0x14, 0x00, 0x00, 0x82, 0xA2, 0xFE, 0x01, 0x15, 0x24,
				0x21, 0x20, 0xA0, 0x03, 0x0C, 0x30, 0x08, 0x00, 0x59, 0x00, 0x03, 0x24,
				0x02, 0x00, 0x60, 0x14, 0x1B, 0x00, 0x43, 0x00, 0x0D, 0x00, 0x07, 0x00,
				0x12, 0x10, 0x00, 0x00, 0x10, 0x18, 0x00, 0x00, 0x01, 0x00, 0x63, 0x24,
				0x40, 0x1A, 0x03, 0x00, 0x20, 0xA0, 0x71, 0x00, 0x24, 0x10, 0x55, 0x00,
				0x01, 0x00, 0x42, 0x24, 0x20, 0xA0, 0x82, 0x02, 0x00, 0x00, 0x95, 0xA2,
				0x59, 0x00, 0x08, 0x24, 0x00, 0xB2, 0x29, 0x36, 0x00, 0xB2, 0x4B, 0x36,
				0xFE, 0x01, 0x2A, 0x25, 0x00, 0x02, 0x24, 0x91, 0x01, 0x02, 0x25, 0x91,
				0x21, 0x20, 0x85, 0x00, 0x02, 0x02, 0x25, 0x91, 0x21, 0x20, 0x85, 0x00,
				0x01, 0x00, 0x25, 0x91, 0x21, 0x20, 0x85, 0x00, 0x82, 0x20, 0x04, 0x00,
				0x01, 0x00, 0x80, 0x5C, 0xFF, 0xFF, 0x84, 0x24, 0x01, 0x00, 0x64, 0xA1,
				0x01, 0x00, 0x29, 0x25, 0x2B, 0x10, 0x2A, 0x01, 0xF2, 0xFF, 0x40, 0x14,
				0x01, 0x00, 0x6B, 0x25, 0xFF, 0xFF, 0x08, 0x25, 0x02, 0xFC, 0x6B, 0x25,
				0xED, 0xFF, 0x00, 0x1D, 0x02, 0xFC, 0x29, 0x25, 0x10, 0x00, 0x08, 0x3C,
				0x26, 0x80, 0x08, 0x02, 0x21, 0x10, 0x00, 0x02, 0x21, 0x40, 0x00, 0x00,
				0x21, 0x50, 0x40, 0x02, 0x21, 0x20, 0x00, 0x02, 0x00, 0x02, 0x4B, 0x25,
				0x00, 0x00, 0x42, 0x91, 0x00, 0x00, 0x82, 0xAC, 0x00, 0x08, 0x82, 0xAC,
				0x00, 0x10, 0x82, 0xAC, 0x01, 0x00, 0x4A, 0x25, 0x2B, 0x18, 0x4B, 0x01,
				0xF9, 0xFF, 0x60, 0x14, 0x04, 0x00, 0x84, 0x24, 0x01, 0x00, 0x08, 0x25,
				0x5A, 0x00, 0x02, 0x2D, 0xF4, 0xFF, 0x40, 0x14, 0x00, 0x10, 0x84, 0x24,
				0xD0, 0x09, 0xA4, 0x27, 0x01, 0x00, 0x05, 0x24, 0x0C, 0x54, 0x08, 0x00,
				0xD4, 0x09, 0xA8, 0x8F, 0x01, 0x00, 0x00, 0x55, 0xCC, 0x3A, 0x08, 0x00,
				0xCC, 0x51, 0x08, 0x00, 0x21, 0x20, 0x00, 0x02, 0x00, 0x02, 0x05, 0x24,
				0x03, 0x00, 0x06, 0x24, 0x01, 0x00, 0x07, 0x24, 0xCC, 0x4F, 0x08, 0x00,
				0x21, 0x40, 0x20, 0x02, 0x21, 0x88, 0x40, 0x02, 0x24, 0x00, 0x24, 0x0A,
				0x21, 0x90, 0x00, 0x01
			};

			var Memory = new FastPspMemory();
			var MemoryStream = new PspMemoryStream(Memory);
			MemoryStream.Position = 0x08900000;
			MemoryStream.WriteBytes(MiniFireData);
			var BinaryWriter = new BinaryWriter(MemoryStream);
			var BinaryReader = new BinaryReader(MemoryStream);
			var Processor = new Processor(Memory);
			var ThreadManager = new HlePspThreadManager(Processor);
			var Thread = ThreadManager.Create();
			Thread.CpuThreadState.PC = 0x08900008;

			while (true)
			{
				Console.WriteLine("{0:X}", Thread.CpuThreadState.PC);
				ThreadManager.StepNext();
			}
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
			//PerfTest();
			//ThreadTest();
			//var HlePspThreadTest = new HlePspThreadTest(); HlePspThreadTest.SetUp(); HlePspThreadTest.CpuThreadStateTest();

			MiniFire();

			Console.WriteLine("... Ended");
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

/*
# Mini fire demo (c) TyRaNiD 2k6

	.set noreorder
	.set noat

#ifdef BUILD_V1
#include "syscallv1.h"
#else
#include "syscallv15.h"
#endif

#define MAX_X 512
#define MAX_Y 91

.section .rodata.sceModuleInfo
	.global _info
_info:
	.hword 0x0000
	.byte  1, 1
_name:
	.asciz "UNP"

	.global _start
_start:
	# 7 dirty dirty Instructions
	lui	   $s0, %hi(_name)
	addiu  $a0, $s0, %lo(_name)
	addiu  $a1, $s0, %lo(thread)
	addiu  $a2, $zero, 20
	lui    $a3, 0x2
	lui    $t0, 0x8000
	move   $t1, $zero
	.word  0
	.word  0
	.word  0
	.word  0

# Create Thread
	syscall _sceKernelCreateThread
	move $a0, $v0
	move $a1, $zero
	move $a2, $zero
# Start Thread
	syscall _sceKernelStartThread
	move $a0, $zero
# Exit Thread
	syscall _sceKernelExitThread

# Main thread procedure
# s0 - Frame Buffer
# s1 - p1
# s2 - p2

thread:
	# Allocate stack frame for random number gen and controller
	addiu $sp, $sp, -(2512+16)
	move  $a0, $sp
	# We leave $a1 as what ever random value it starts with, probably 0
	syscall _sceKernelUtilsMt19937Init

# Load addresses of the buffers
	lui   $s1, 0x08A0
	lui   $s2, 0x08B0

# Load VRAM address
	lui  $s0, 0x4400
	move $a0, $zero
	li   $a1, 480
	li   $a2, 272
	syscall _sceDisplaySetMode

# Clear buffer
	move $v0, $s1
	li   $t0, (MAX_X*MAX_Y)
clear:
	sb   $zero, 0($v0)
	addiu $t0, $t0, -1
	bgez  $t0, clear
	addiu $v0, 1

mainloop:

# Setup random line
# s3 end position
# s4 current position
	ori   $s4, $s1, (MAX_Y-1)*MAX_X
	addiu $s3, $s4, (MAX_X-1)
setup:
	move  $a0, $sp
	syscall _sceKernelUtilsMt19937UInt
	addiu $s4, $s4, 1
	sltu  $v1, $s4, $s3
	bnez  $v1, setup
	sb    $v0, 0($s4)

# Add a random point, create abit of interest
	li    $s5, 0x1FE
	move  $a0, $sp
	syscall _sceKernelUtilsMt19937UInt
	li    $v1, MAX_Y-2
	divu  $v0, $v1
# Pull out remainder, quotient already in $v0
	mfhi  $v1
	addiu $v1, $v1, 1
	sll   $v1, $v1, 9
	add   $s4, $v1, $s1
# Pull out quotient
	and   $v0, $v0, $s5
	addiu $v0, $v0, 1
	add   $s4, $s4, $v0
	sb    $s5, 0($s4)

# Merge values
# t0 current y
# t1 current x
# t2 end x
# t3 p2
	li    $t0, MAX_Y-2
	ori   $t1, $s1, (MAX_Y-2)*MAX_X
	ori   $t3, $s2, (MAX_Y-2)*MAX_X
merge_y:
	addiu $t2, $t1, (MAX_X-2)
merge_x:
# Accumulate (y+1,x-1),(y+1,x),(y+1,x+1),(y,x)
	lbu   $a0, MAX_X($t1)
	lbu   $a1, MAX_X+1($t1)
	addu  $a0, $a0, $a1
	lbu   $a1, MAX_X+2($t1)
	addu  $a0, $a0, $a1
	lbu   $a1, 1($t1)
	addu  $a0, $a0, $a1
# Get average
	srl   $a0, $a0, 2
# Decrement if > 0
	bgtzl $a0, 1f
	addiu $a0, $a0, -1
1:
	sb    $a0, 1($t3)

	addiu $t1, $t1, 1
	sltu  $v0, $t1, $t2
	bnez  $v0, merge_x
	addiu $t3, $t3, 1

	addiu $t0, $t0, -1
	addiu $t3, $t3, -((MAX_X*2)-2)
	bgtz  $t0, merge_y
	addiu $t1, $t1, -((MAX_X*2)-2)

# Copy loop, put data to VRAM
# $t0 current y
# $t2 current p2
# $t3 end p2 for this line
# $a0 frame buffer

# Do double buffering (not really necessary but hey)
	li   $t0, 1024*1024
	xor  $s0, $s0, $t0

	move $v0, $s0
	move $t0, $zero
	move $t2, $s2
	move $a0, $s0
copy_y_loop:
	addiu $t3, $t2, MAX_X
copy_x_loop:
	lbu   $v0, 0($t2)
	sw    $v0, 0($a0)
	sw    $v0, (MAX_X*4)($a0)
	sw    $v0, (MAX_X*8)($a0)
	addiu $t2, $t2, 1
	sltu  $v1, $t2, $t3
	bnez  $v1, copy_x_loop
	addiu $a0, $a0, 4
	addiu $t0, $t0, 1
	sltiu $v0, $t0, (MAX_Y-1) 
	bnez  $v0, copy_y_loop
	addiu $a0, $a0, (8*MAX_X)

# Check the pad to see if we want to exit
# Any button will do
	addiu $a0, $sp, 2512
	li    $a1, 1
	syscall _sceCtrlPeekBufferPositive
	lw    $t0, 2516($sp)
	bnezl $t0, ctrl
	syscall _sceKernelExitGame
ctrl:

# Delay
	syscall _sceDisplayWaitVblankStart
# Flip buffers
	move $a0, $s0
	li   $a1, 512
	li   $a2, 3
	li   $a3, 1
	syscall _sceDisplaySetFrameBuf

# Swap buffers
	move  $t0, $s1
	move  $s1, $s2

	j     mainloop
	move  $s2, $t0
*/
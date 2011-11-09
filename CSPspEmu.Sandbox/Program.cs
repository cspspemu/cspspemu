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
using System.Windows.Forms;
using CSPspEmu.Gui.Winforms;
using CSPspEmu.Hle.Modules.threadman;
using CSPspEmu.Hle.Modules.utils;
using CSPspEmu.Hle.Modules.display;
using CSPspEmu.Hle.Modules.loadexec;
using CSPspEmu.Hle.Modules.ctrl;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Loader;

namespace CSPspEmu.Sandbox
{
	unsafe class Program
	{
		static void MiniFire()
		{
			var Memory = new FastPspMemory();
			//var Memory = new LazyPspMemory();
			//var Memory = new NormalPspMemory();
			Memory.Reset();

			var MemoryStream = new PspMemoryStream(Memory);
			var Loader = new ElfPspLoader();
			var Processor = new Processor(Memory);
			//Processor.ShowInstructionStats = true;
			//Processor.TraceJIT = true;
			//Processor.CountInstructionsAndYield = false;
			var HleState = new HleState(Processor);
			var HlePspRtc = HleState.PspRtc;
			var ThreadManager = HleState.ThreadManager;
			var Assembler = new MipsAssembler(MemoryStream);

			//var ElfStream = File.OpenRead("../../../TestInput/HelloWorld.elf");
			//var ElfStream = File.OpenRead("../../../TestInput/minifire.elf");
			//var ElfStream = File.OpenRead("../../../TestInput/HelloWorldPSP.elf");
			var ElfStream = File.OpenRead("../../../TestInput/counter.elf");
			//var ElfStream = File.OpenRead(@"C:\projects\pspemu\pspautotests\tests\string\string.elf");
			//var ElfStream = File.OpenRead(@"C:\juegos\jpcsp2\demos\compilerPerf.elf");
			//var ElfStream = File.OpenRead(@"C:\juegos\jpcsp2\demos\fputest.elf");
			//var ElfStream = File.OpenRead(@"C:\projects\pspemu\pspautotests\demos\mytest.elf");
			//var ElfStream = File.OpenRead(@"C:\projects\pspemu\demos\dumper.elf");
			//var ElfStream = File.OpenRead(@"C:\projects\pspemu\pspautotests\tests\cpu\cpu\cpu.elf");

			Loader.LoadAllocateAndWrite(
				ElfStream,
				MemoryStream,
				HleState.MemoryManager.RootPartition
			);

			Loader.UpdateModuleImports(new PspMemoryStream(Memory), HleState.ModuleManager);

			Console.WriteLine("{0:X}", Loader.InitInfo.PC);
			Console.WriteLine("{0:X}", Memory.Read4(Loader.InitInfo.PC));

			var ThreadManForUser = HleState.ModuleManager.GetModule<ThreadManForUser>();


			Processor.RegisterNativeSyscall(0x206D, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<ThreadManForUser>("sceKernelCreateThread")(CpuThreadState);
			});

			Processor.RegisterNativeSyscall(0x206F, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<ThreadManForUser>("sceKernelStartThread")(CpuThreadState);
			});

			Processor.RegisterNativeSyscall(0x2071, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<ThreadManForUser>("sceKernelExitDeleteThread")(CpuThreadState);
			});

			Processor.RegisterNativeSyscall(0x20BF, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<UtilsForUser>("sceKernelUtilsMt19937Init")(CpuThreadState);
			});

			Processor.RegisterNativeSyscall(0x20C0, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<UtilsForUser>("sceKernelUtilsMt19937UInt")(CpuThreadState);
			});

			Processor.RegisterNativeSyscall(0x213A, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<sceDisplay>("sceDisplaySetMode")(CpuThreadState);
			});

			Processor.RegisterNativeSyscall(0x2147, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<sceDisplay>("sceDisplayWaitVblankStart")(CpuThreadState);
			});

			Processor.RegisterNativeSyscall(0x213f, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<sceDisplay>("sceDisplaySetFrameBuf")(CpuThreadState);
			});

			Processor.RegisterNativeSyscall(0x20eb, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<LoadExecForUser>("sceKernelExitGame")(CpuThreadState);
			});

			Processor.RegisterNativeSyscall(0x2150, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<sceCtrl>("sceCtrlPeekBufferPositive")(CpuThreadState);
			});

			var MainThread = ThreadManager.Create();
			MainThread.CpuThreadState.PC = Loader.InitInfo.PC;
			MainThread.CpuThreadState.GP = Loader.InitInfo.GP;
			MainThread.CpuThreadState.SP = (uint)(0x09000000 - 10000);
			MainThread.CpuThreadState.RA = (uint)0x08000000;

			Assembler.Assemble(@"
				.code 0x08000000
				syscall 0x7777
				jr r31
				nop
			");

			Processor.RegisterNativeSyscall(0x7777, (Code, CpuThreadState) =>
			{
				var SleepThread = HleState.ThreadManager.Current;
				SleepThread.CurrentStatus = HleThread.Status.Waiting;
				SleepThread.CurrentWaitType = HleThread.WaitType.None;
				CpuThreadState.Yield();
			});


			MainThread.CurrentStatus = HleThread.Status.Ready;
			bool Running = true;

			// Execution Thread.
			new Thread(() =>
			{
				try
				{
					while (Running)
					{
						HlePspRtc.Update();
						ThreadManager.StepNext();
					}
				}
				catch (Exception Exception)
				{
					Console.WriteLine(Exception);
					//throw (new Exception("Unhandled Exception " + Exception.ToString(), Exception));
					//throw (new Exception(Exception.InnerException.ToString(), Exception.InnerException));
				}
			}).Start();

			// GUI Thread.
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new PspDisplayForm(Memory, HleState.PspDisplay, HleState.PspController));
			Running = false;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://en.wikipedia.org/wiki/Common_Intermediate_Language"/>
		/// <see cref="http://en.wikipedia.org/wiki/List_of_CIL_instructions"/>
		/// <param name="args"></param>
		[STAThread]
		static void Main(string[] args)
		{
			Console.SetWindowSize(160, 60);
			MiniFire();
		}
	}
}

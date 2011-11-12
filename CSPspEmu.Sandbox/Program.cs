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
using CSharpUtils;
using CSharpUtils.Threading;
using System.Reflection;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Core.Gpu;

namespace CSPspEmu.Sandbox
{
	unsafe class Program : IGuiExternalInterface
	{
		PspConfig PspConfig;
		PspRtc PspRtc;
		PspDisplay PspDisplay;
		PspController PspController;
		PspMemory Memory;
		CpuProcessor CpuProcessor;
		GpuProcessor GpuProcessor;
		PspMemoryStream MemoryStream;
		HleState HleState;
		TaskQueue CpuTaskQueue = new TaskQueue();
		Assembly HleModulesDll;
		AutoResetEvent PauseEvent;
		AutoResetEvent ResumeEvent;

		Thread CpuThread;
		Thread GpuThread;

		PspMemory IGuiExternalInterface.GetMemory()
		{
			return Memory;
		}

		public PspDisplay GetDisplay()
		{
			return PspDisplay;
		}

		public PspConfig GetConfig()
		{
			return PspConfig;
		}

		public PspController GetController()
		{
			return PspController;
		}

		public void PauseResume(Action Action)
		{
			if (Paused)
			{
				Action();
			}
			else
			{
				Pause();
				try
				{
					Action();
				}
				finally
				{
					Resume();
				}
			}
		}

		public bool IsPaused()
		{
			return Paused;
		}

		public bool Paused
		{
			get
			{
				return (PauseEvent != null);
			}
		}

		public void Pause()
		{
			if (!Paused)
			{
				PauseEvent = new AutoResetEvent(false);
				ResumeEvent = new AutoResetEvent(false);

				CpuTaskQueue.EnqueueAndWaitStarted(() =>
				{
					while (!PauseEvent.WaitOne(TimeSpan.FromMilliseconds(10)))
					{
						if (!CpuProcessor.IsRunning) break;
					}
					PauseEvent = null;
					ResumeEvent.Set();
				});
			}
		}

		public void Resume()
		{
			if (Paused)
			{
				PauseEvent.Set();
				while (!ResumeEvent.WaitOne(TimeSpan.FromMilliseconds(10)))
				{
					if (!CpuProcessor.IsRunning) break;
				}
			}
		}

		public void LoadFile(String FileName)
		{
			CpuTaskQueue.Enqueue(() =>
			{
				_LoadFile(FileName);
			});
		}

		private void _LoadFile(String FileName)
		{
			CpuProcessor.Reset();
			Memory.Reset();
			CreateNewHleState();

			var Loader = new ElfPspLoader();
			Stream LoadStream = File.OpenRead(FileName);
			Stream ElfLoadStream = null;

			var Format = new FormatDetector().Detect(LoadStream);
			switch (Format)
			{
				case "Pbp":
					ElfLoadStream = new Pbp().Load(LoadStream)["psp.data"];
					break;
				case "Elf":
					ElfLoadStream = LoadStream;
					break;
				default:
					throw (new NotImplementedException("Can't load format '" + Format + "'"));
			}

			Loader.Load(
				ElfLoadStream,
				MemoryStream,
				HleState.MemoryManager.RootPartition,
				HleState.ModuleManager
			);

			uint argc = 1;
			uint argv = 0x08001000;

			new StreamWriter(MemoryStream.SliceWithLength(0x08001000, 1000)).Write("/PSP/GAME/virtual/EBOOT.PBP\0");

			var MainThread = HleState.ThreadManager.Create();
			MainThread.CpuThreadState.PC = Loader.InitInfo.PC;
			MainThread.CpuThreadState.GP = Loader.InitInfo.GP;
			MainThread.CpuThreadState.SP = HleState.MemoryManager.RootPartition.Allocate(0x1000, MemoryPartition.Anchor.High).High;
			MainThread.CpuThreadState.K0 = MainThread.CpuThreadState.SP;
			MainThread.CpuThreadState.RA = (uint)0x08000000;
			MainThread.CpuThreadState.GPR[4] = (int)argc;
			MainThread.CpuThreadState.GPR[5] = (int)argv;
			MainThread.CurrentStatus = HleThread.Status.Ready;

			/*
			registers.GP = modulePsp.sceModule.gp_value;
			registers.SP = hleEmulatorState.memoryManager.allocStack(PspPartition.User, "Stack for main thread", 0x4000) - 0x10;
			registers.K0 = registers.SP;
			registers.RA = ModuleManager.CODE_PTR_EXIT_THREAD;
			registers.A0 = argc;
			registers.A1 = argv;
			*/

			RegisterSyscalls();
		}

		void RegisterSyscalls()
		{
			new MipsAssembler(MemoryStream).Assemble(@"
			.code 0x08000000
				syscall 0x7777
				jr r31
				nop
			");

			CpuProcessor.RegisterNativeSyscall(0x7777, (Code, CpuThreadState) =>
			{
				var SleepThread = HleState.ThreadManager.Current;
				SleepThread.CurrentStatus = HleThread.Status.Waiting;
				SleepThread.CurrentWaitType = HleThread.WaitType.None;
				CpuThreadState.Yield();
			});

			var ThreadManForUser = HleState.ModuleManager.GetModule<ThreadManForUser>();

			CpuProcessor.RegisterNativeSyscall(0x206D, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<ThreadManForUser>("sceKernelCreateThread")(CpuThreadState);
			});

			CpuProcessor.RegisterNativeSyscall(0x206F, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<ThreadManForUser>("sceKernelStartThread")(CpuThreadState);
			});

			CpuProcessor.RegisterNativeSyscall(0x2071, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<ThreadManForUser>("sceKernelExitDeleteThread")(CpuThreadState);
			});

			CpuProcessor.RegisterNativeSyscall(0x20BF, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<UtilsForUser>("sceKernelUtilsMt19937Init")(CpuThreadState);
			});

			CpuProcessor.RegisterNativeSyscall(0x20C0, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<UtilsForUser>("sceKernelUtilsMt19937UInt")(CpuThreadState);
			});

			CpuProcessor.RegisterNativeSyscall(0x213A, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<sceDisplay>("sceDisplaySetMode")(CpuThreadState);
			});

			CpuProcessor.RegisterNativeSyscall(0x2147, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<sceDisplay>("sceDisplayWaitVblankStart")(CpuThreadState);
			});

			CpuProcessor.RegisterNativeSyscall(0x213f, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<sceDisplay>("sceDisplaySetFrameBuf")(CpuThreadState);
			});

			CpuProcessor.RegisterNativeSyscall(0x20eb, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<LoadExecForUser>("sceKernelExitGame")(CpuThreadState);
			});

			CpuProcessor.RegisterNativeSyscall(0x2150, (Code, CpuThreadState) =>
			{
				HleState.ModuleManager.GetModuleDelegate<sceCtrl>("sceCtrlPeekBufferPositive")(CpuThreadState);
			});
		}

		void CreateNewHleState() {
			HleState = new HleState(CpuProcessor, GpuProcessor, PspConfig, PspRtc, PspDisplay, PspController, HleModulesDll);
		}

		void Execute()
		{
			HleModulesDll = Assembly.LoadFile(Path.GetDirectoryName(typeof(Program).Assembly.Location) + @"\CSPspEmu.Hle.Modules.dll");
			PspConfig = new PspConfig();
			PspRtc = new PspRtc();
			PspDisplay = new PspDisplay(PspRtc);
			PspController = new PspController();
			Memory = new FastPspMemory();
			//Memory = new NormalPspMemory();
			MemoryStream = new PspMemoryStream(Memory);
			CpuProcessor = new CpuProcessor(PspConfig, Memory);
			GpuProcessor = new GpuProcessor(PspConfig, Memory);
			CreateNewHleState();

			//PspConfig.DebugSyscalls = true;
			//PspConfig.ShowInstructionStats = true;
			//PspConfig.TraceJIT = true;
			//PspConfig.CountInstructionsAndYield = false;

			Thread.CurrentThread.Name = "GuiThread";

			OnInit();

			GpuThread = new Thread(GpuThreadEntryPoint)
			{
				Name = "GpuThread",
			};
			GpuThread.Start();

			CpuThread = new Thread(CpuThreadEntryPoint)
			{
				Name = "CpuThread",
			};
			CpuThread.Start();

			// GUI Thread.
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new PspDisplayForm(this));
			try { CpuProcessor.IsRunning = false; } catch { }
		}

		protected void GpuThreadEntryPoint()
		{
			while (CpuProcessor.IsRunning)
			{
				GpuProcessor.Process();
			}
		}

		protected void CpuThreadEntryPoint()
		{
			while (CpuProcessor.IsRunning)
			{
				try
				{
					try
					{
						if (CpuTaskQueue != null)
						{
							CpuTaskQueue.HandleEnqueued();
						}
						CpuTaskQueue = new TaskQueue();

						while (CpuProcessor.IsRunning)
						{
							CpuTaskQueue.HandleEnqueued();
							if (!CpuProcessor.IsRunning) break;
							HleState.PspRtc.Update();
							HleState.ThreadManager.StepNext();
						}
					}
					finally
					{
						CpuTaskQueue = null;
					}
				}
				catch (Exception Exception)
				{
					ConsoleUtils.SaveRestoreConsoleState(() =>
					{
						Console.ForegroundColor = ConsoleColor.Red;

						Console.WriteLine(Exception);

						try
						{
							HleState.ThreadManager.Current.CpuThreadState.DumpRegisters();

							Console.WriteLine(
								"Last registered PC = 0x{0:X}, RA = 0x{1:X}",
								HleState.ThreadManager.Current.CpuThreadState.PC,
								HleState.ThreadManager.Current.CpuThreadState.RA
							);
						}
						catch
						{
						}
					});
					//throw (new Exception("Unhandled Exception " + Exception.ToString(), Exception));
					//throw (new Exception(Exception.InnerException.ToString(), Exception.InnerException));
				}
			}
		}

		protected void OnInit()
		{
			//Console.WriteLine("OnInit");
			//LoadFile(@"../../../TestInput/minifire.elf");
			//LoadFile(@"../../../TestInput/HelloWorld.elf");
			//LoadFile(@"../../../TestInput/HelloWorldPSP.elf");
			//LoadFile(@"../../../TestInput/counter.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\string\string.elf");
			//LoadFile(@"C:\projects\jpcsp\demos\compilerPerf.pbp");
			LoadFile(@"C:\juegos\jpcsp2\demos\cube.pbp");
			//LoadFile(@"C:\juegos\jpcsp2\demos\fputest.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos\mytest.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos\cube.pbp");
			//LoadFile(@"C:\projects\pspemu\demos\dumper.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\cpu\cpu\cpu.elf");
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
			Console.SetBufferSize(160, 2000);
			new Program().Execute();
		}
	}
}

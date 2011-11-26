using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using System.IO;
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
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Hle.Modules.emulator;
using CSPspEmu.Hle.Vfs.Local;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Vfs.Emulator;
using System.Globalization;
using CSPspEmu.Core.Audio.Imple.Openal;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Audio;
using System.Linq;

namespace CSPspEmu.Sandbox
{
	unsafe class Program : IGuiExternalInterface
	{
		protected PspConfig PspConfig;
		protected PspDisplay PspDisplay;
		protected PspController PspController;
		protected CpuProcessor CpuProcessor;

		protected GpuProcessor GpuProcessor;

		protected PspAudio PspAudio;

		protected PspMemoryStream MemoryStream;
		protected HleState HleState;
		protected TaskQueue CpuTaskQueue = new TaskQueue();
		protected AutoResetEvent PauseEvent;
		protected AutoResetEvent ResumeEvent;

		protected Thread CpuThread;
		protected Thread GpuThread;

		PspMemory IGuiExternalInterface.GetMemory()
		{
			return PspEmulatorContext.GetInstance<PspMemory>();
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
			PspEmulatorContext.GetInstance<PspMemory>().Reset();
			CreateNewHleState();

			var Loader = PspEmulatorContext.NewInstance<ElfPspLoader>();
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
				HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User),
				HleState.ModuleManager
			);

			RegisterSyscalls();


			uint CODE_PTR_ARGUMENTS = 0x08000100;

			{
				var BinaryWriter = new BinaryWriter(MemoryStream);
				var StreamWriter = new StreamWriter(MemoryStream); StreamWriter.AutoFlush = true;
				MemoryStream.Position = CODE_PTR_ARGUMENTS;

				BinaryWriter.Write((uint)(CODE_PTR_ARGUMENTS + 4)); BinaryWriter.Flush();
				StreamWriter.Write("ms0:/PSP/GAME/virtual/EBOOT.PBP\0"); StreamWriter.Flush();
			}

			uint argc = 1;
			uint argv = CODE_PTR_ARGUMENTS + 4;
			//uint argv = CODE_PTR_ARGUMENTS;

			var MainThread = HleState.ThreadManager.Create();
			MainThread.CpuThreadState.PC = Loader.InitInfo.PC;
			MainThread.CpuThreadState.GP = Loader.InitInfo.GP;
			MainThread.CpuThreadState.SP = HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User).Allocate(0x1000, MemoryPartition.Anchor.High, Alignment: 0x100).High;
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
		}

		void RegisterModuleSyscall<TType>(int SyscallCode, string FunctionName) where TType : HleModuleHost
		{
			var Delegate = HleState.ModuleManager.GetModuleDelegate<TType>(FunctionName);
			CpuProcessor.RegisterNativeSyscall(SyscallCode, (Code, CpuThreadState) =>
			{
				Delegate(CpuThreadState);
			});
		}

		void RegisterSyscalls()
		{
			new MipsAssembler(MemoryStream).Assemble(@"
			.code 0x08000000
				syscall 0x7777
				jr r31
				nop
			");

			//var ThreadManForUser = HleState.ModuleManager.GetModule<ThreadManForUser>();

			RegisterModuleSyscall<ThreadManForUser>(0x206D, "sceKernelCreateThread");
			RegisterModuleSyscall<ThreadManForUser>(0x206F, "sceKernelStartThread");
			RegisterModuleSyscall<ThreadManForUser>(0x2071, "sceKernelExitDeleteThread");

			RegisterModuleSyscall<UtilsForUser>(0x20BF, "sceKernelUtilsMt19937Init");
			RegisterModuleSyscall<UtilsForUser>(0x20C0, "sceKernelUtilsMt19937UInt");

			RegisterModuleSyscall<sceDisplay>(0x213A, "sceDisplaySetMode");
			RegisterModuleSyscall<sceDisplay>(0x2147, "sceDisplayWaitVblankStart");
			RegisterModuleSyscall<sceDisplay>(0x213F, "sceDisplaySetFrameBuf");

			RegisterModuleSyscall<LoadExecForUser>(0x20EB, "sceKernelExitGame");
			
			RegisterModuleSyscall<sceCtrl>(0x2150, "sceCtrlPeekBufferPositive");

			RegisterModuleSyscall<Emulator>(0x1010, "emitInt");
			RegisterModuleSyscall<Emulator>(0x1011, "emitFloat");
			RegisterModuleSyscall<Emulator>(0x1012, "emitString");
			RegisterModuleSyscall<Emulator>(0x1013, "emitMemoryBlock");
			RegisterModuleSyscall<Emulator>(0x1014, "emitHex");
			RegisterModuleSyscall<Emulator>(0x1015, "emitUInt");
			RegisterModuleSyscall<Emulator>(0x1016, "emitLong");
			RegisterModuleSyscall<Emulator>(0x1017, "testArguments");
			RegisterModuleSyscall<Emulator>(0x7777, "waitThreadForever");
		}

		void CreateNewHleState() {
			HleState = PspEmulatorContext.NewInstance<HleState>();
			//Console.WriteLine();
			//Console.ReadKey();
			string VirtualDirectory = Path.GetDirectoryName(Application.ExecutablePath);
			try { Directory.CreateDirectory(VirtualDirectory + "/PSP/GAME/virtual"); }
			catch { }

			HleState.HleIoManager.AddDriver("ms:", new HleIoDriverLocalFileSystem(VirtualDirectory).AsReadonlyHleIoDriver());
			HleState.HleIoManager.AddDriver("emulator:", new HleIoDriverEmulator(HleState));
		}

		AutoResetEvent GpuInitializedCompleteEvent = new AutoResetEvent(false);
		AutoResetEvent CpuInitializedCompleteEvent = new AutoResetEvent(false);
		private PspEmulatorContext PspEmulatorContext;

		void Execute()
		{
			PspConfig = new PspConfig();
			PspConfig.HleModulesDll = Assembly.LoadFile(Path.GetDirectoryName(typeof(Program).Assembly.Location) + @"\CSPspEmu.Hle.Modules.dll");

			PspEmulatorContext = new PspEmulatorContext(PspConfig);

			{
				PspEmulatorContext.SetInstanceType<GpuImpl, OpenglGpuImpl>();
				PspEmulatorContext.SetInstanceType<PspAudioImpl, PspAudioOpenalImpl>();

				if (PspConfig.UseFastAndUnsaferMemory)
				{
					PspEmulatorContext.SetInstanceType<PspMemory, FastPspMemory>();
				}
				else
				{
					PspEmulatorContext.SetInstanceType<PspMemory, NormalPspMemory>();
				}
			}


			//PspRtc = PspEmulatorContext.GetInstance<PspRtc>();
			PspDisplay = PspEmulatorContext.GetInstance<PspDisplay>();
			PspController = PspEmulatorContext.GetInstance<PspController>();

			MemoryStream = new PspMemoryStream(PspEmulatorContext.GetInstance<PspMemory>());
			CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();

			// Gpu
			GpuProcessor = PspEmulatorContext.GetInstance<GpuProcessor>();

			// Audio
			PspAudio = PspEmulatorContext.GetInstance<PspAudio>();

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
			GpuInitializedCompleteEvent.WaitOne();

			CpuThread = new Thread(CpuThreadEntryPoint)
			{
				Name = "CpuThread",
			};
			CpuThread.Start();
			CpuInitializedCompleteEvent.WaitOne();

			// GUI Thread.
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			var Form = new PspDisplayForm(this);
			/*
			Form.Shown += new EventHandler((o, ea) =>
			{
				Form.Focus();
			});
			*/
			Application.Run(Form);
			try { CpuProcessor.IsRunning = false; }
			catch { }
		}

		protected void GpuThreadEntryPoint()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo(PspConfig.CultureName);
			PspEmulatorContext.GetInstance<GpuImpl>().Init();
			GpuInitializedCompleteEvent.Set();

			while (CpuProcessor.IsRunning)
			{
				GpuProcessor.Process();
			}
		}

		protected void CpuThreadEntryPoint()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo(PspConfig.CultureName);
			CpuInitializedCompleteEvent.Set();
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
							PspEmulatorContext.GetInstance<PspRtc>().Update();
							{
								HleState.ThreadManager.StepNext();
							}
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

						try
						{
							Console.WriteLine("Error on thread {0}", HleState.ThreadManager.Current);

							Console.WriteLine(Exception);

							HleState.ThreadManager.Current.CpuThreadState.DumpRegisters();

							Console.WriteLine(
								"Last registered PC = 0x{0:X}, RA = 0x{1:X}",
								HleState.ThreadManager.Current.CpuThreadState.PC,
								HleState.ThreadManager.Current.CpuThreadState.RA
							);
						}
						catch (Exception Exception2)
						{
							Console.WriteLine("{0}", Exception2);
						}
					});
					//throw (new Exception("Unhandled Exception " + Exception.ToString(), Exception));
					//throw (new Exception(Exception.InnerException.ToString(), Exception.InnerException));
				}
			}
		}

		public void ShowDebugInformation()
		{
			foreach (var Key in CpuProcessor.GlobalInstructionStats.Keys.OrderBy(Value => Value))
			{
				Console.WriteLine("{0} -> {1}", Key, CpuProcessor.GlobalInstructionStats[Key]);
			}
		}

		protected void OnInit()
		{
			//PspConfig.DebugSyscalls = true;
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\audio\polyphonic\polyphonic.elf");

			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\fpu.elf");

			//LoadFile(@"C:\pspsdk\psp\sdk\samples\gu\ortho\ortho.elf");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\gu\lines\lines.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos_ex\sdl\main.elf");
			LoadFile(@"C:\projects\csharp\cspspemu\games\cavestory\EBOOT.PBP");
			
			
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\gu\cube\cube.elf");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\gu\text\gufont.elf");

			//LoadFile(@"C:\projects\jpcsp\demos\compilerPerf.pbp");
			//LoadFile(@"C:\juegos\jpcsp2\demos\fputest.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\alu.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\fpu.elf");
			//LoadFile(@"C:\projects\cspspemu\PspAutoTests\gum.elf");
			//LoadFile(@"C:\juegos\pspemu\demos\controller.pbp");
			//LoadFile(@"C:\juegos\jpcsp-windows-x86\demos\sound.prx");
			//LoadFile(@"C:\juegos\jpcsp-windows-x86\demos\cube.pbp");
			//LoadFile(@"C:\juegos\jpcsp-windows-x86\demos\ortho.pbp");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\cpu\cpu\cpu.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos\threadstatus.pbp");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\io\io\io.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\cpu\fpu\fpu.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\malloc\malloc.elf");
			
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\args.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\alu.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\fpu.elf");
			//LoadFile(@"C:\projects\csharp\cspspemu\PspAutoTests\malloc.elf");

			//LoadFile(@"C:\pspsdk\psp\sdk\samples\kernel\sysevent\EBOOT.PBP");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\kernel\systimer\EBOOT.PBP");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\kernel\loadmodule\EBOOT.PBP");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\prx\prx_loader\EBOOT.PBP");
			//LoadFile(@"C:\pspsdk\psp\sdk\samples\prx\prx_loader\EBOOT.PBP");
			//LoadFile(@"../../../TestInput/minifire.elf");
			//PspConfig.ShowInstructionStats = true;

			//LoadFile(@"../../../TestInput/HelloWorld.elf");
			//LoadFile(@"../../../TestInput/HelloWorldPSP.elf");
			//LoadFile(@"../../../TestInput/counter.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\tests\string\string.elf");
			//LoadFile(@"C:\juegos\jpcsp2\demos\cube.pbp");
			//LoadFile(@"C:\juegos\jpcsp2\demos\nehetutorial02.pbp");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos\mytest.elf");
			//LoadFile(@"C:\projects\pspemu\pspautotests\demos\cube.pbp");
			//LoadFile(@"C:\projects\pspemu\demos\dumper.elf");
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
			//var PspAudioImpl = new PspAudioOpenalImpl(); Thread.Sleep(int.MaxValue);
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Hle;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Loader;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Modules.ctrl;
using CSPspEmu.Hle.Modules.display;
using CSPspEmu.Hle.Modules.emulator;
using CSPspEmu.Hle.Modules.loadexec;
using CSPspEmu.Hle.Modules.threadman;
using CSPspEmu.Hle.Modules.utils;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Vfs.Local;
using CSPspEmu.Hle.Vfs.Emulator;
using System.Windows.Forms;
using System.Threading;
using CSPspEmu.Hle.Vfs.MemoryStick;

namespace CSPspEmu.Runner
{
	sealed public class CpuComponentThread : ComponentThread
	{
		protected override string ThreadName { get { return "CpuThread"; } }

		CpuProcessor CpuProcessor;
		PspRtc PspRtc;
		HleThreadManager ThreadManager;
		HleState HleState;
		PspMemory PspMemory;
		HleIoDriverMountable MemoryStickMountable;

		static public uint CODE_PTR_EXIT_THREAD = 0x08000000;

		public CpuComponentThread(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			PspRtc = PspEmulatorContext.GetInstance<PspRtc>();
			ThreadManager = PspEmulatorContext.GetInstance<HleThreadManager>();
			HleState = PspEmulatorContext.GetInstance<HleState>();
			PspMemory = PspEmulatorContext.GetInstance<PspMemory>();
		}

		public override void InitializeComponent()
		{
			RegisterDevices();
		}

		void RegisterDevices()
		{
			string MemoryStickRootFolder = Path.GetDirectoryName(Application.ExecutablePath) + "/ms";
			try { Directory.CreateDirectory(MemoryStickRootFolder); }
			catch { }
			/*
			*/

			MemoryStickMountable = new HleIoDriverMountable();
			MemoryStickMountable.Mount("/", new HleIoDriverLocalFileSystem(MemoryStickRootFolder));
			var MemoryStick = new HleIoDriverMemoryStick(MemoryStickMountable);
			//var MemoryStick = new HleIoDriverMemoryStick(new HleIoDriverLocalFileSystem(VirtualDirectory).AsReadonlyHleIoDriver());
			HleState.HleIoManager.AddDriver("ms:", MemoryStick);
			HleState.HleIoManager.AddDriver("fatms:", MemoryStick);
			HleState.HleIoManager.AddDriver("disc:", MemoryStick);
			HleState.HleIoManager.AddDriver("emulator:", new HleIoDriverEmulator(HleState));
		}

		void SetVirtualFolder(string VirtualDirectory)
		{
			MemoryStickMountable.Mount(
				"/PSP/GAME/virtual",
				new HleIoDriverLocalFileSystem(VirtualDirectory)
					//.AsReadonlyHleIoDriver()
			);
		}

		void RegisterSyscalls()
		{
			new MipsAssembler(new PspMemoryStream(PspMemory)).Assemble(
				@"
					.code CODE_PTR_EXIT_THREAD
						syscall 0x7777
						jr r31
						nop
				"
				.Replace("CODE_PTR_EXIT_THREAD", String.Format("0x{0:X}", CODE_PTR_EXIT_THREAD))
			);

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
			//RegisterModuleSyscall<Emulator>(0x7777, "waitThreadForever");
			RegisterModuleSyscall<ThreadManForUser>(0x7777, "sceKernelExitDeleteThread");
		}

		void RegisterModuleSyscall<TType>(int SyscallCode, string FunctionName) where TType : HleModuleHost
		{
			var Delegate = HleState.ModuleManager.GetModuleDelegate<TType>(FunctionName);
			CpuProcessor.RegisterNativeSyscall(SyscallCode, (Code, CpuThreadState) =>
			{
				Delegate(CpuThreadState);
			});
		}

		public void _LoadFile(String FileName)
		{
			//GC.Collect();

			SetVirtualFolder(Path.GetDirectoryName(FileName));

			var MemoryStream = new PspMemoryStream(PspMemory);

			var Loader = new ElfPspLoader(PspEmulatorContext);
			Stream LoadStream = File.OpenRead(FileName);
			//using ()
			{
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
				var CpuThreadState = MainThread.CpuThreadState;
				{
					CpuThreadState.PC = Loader.InitInfo.PC;
					CpuThreadState.GP = Loader.InitInfo.GP;
					CpuThreadState.SP = HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User).Allocate(0x1000, MemoryPartition.Anchor.High, Alignment: 0x100).High;
					CpuThreadState.K0 = MainThread.CpuThreadState.SP;
					CpuThreadState.RA = CODE_PTR_EXIT_THREAD;
					CpuThreadState.GPR[4] = (int)argc; // A0
					CpuThreadState.GPR[5] = (int)argv; // A1
				}
				MainThread.CurrentStatus = HleThread.Status.Ready;
			}
		}

		protected override void Main()
		{
			while (Running)
			{
				try
				{
					while (true)
					{
						ThreadTaskQueue.HandleEnqueued();
						if (!Running) return;
						PspRtc.Update();
						ThreadManager.StepNext();
					}
				}
				catch (Exception Exception)
				{
					ConsoleUtils.SaveRestoreConsoleState(() =>
					{
						Console.ForegroundColor = ConsoleColor.Red;

						try
						{
							Console.WriteLine("Error on thread {0}", ThreadManager.Current);

							Console.WriteLine(Exception);

							ThreadManager.Current.CpuThreadState.DumpRegisters();

							Console.WriteLine(
								"Last registered PC = 0x{0:X}, RA = 0x{1:X}",
								ThreadManager.Current.CpuThreadState.PC,
								ThreadManager.Current.CpuThreadState.RA
							);

							foreach (var Thread in ThreadManager.Threads)
							{
								Console.WriteLine("{0}", Thread);
							}

							Console.WriteLine("Executable had relocation: {0}", PspEmulatorContext.PspConfig.InfoExeHasRelocation);
						}
						catch (Exception Exception2)
						{
							Console.WriteLine("{0}", Exception2);
						}
					});

					// Inconsistent state. Wait for stopping.
					while (true)
					{
						ThreadTaskQueue.HandleEnqueued();
						if (!Running) return;
						Thread.Sleep(1);
					}

					//throw (new Exception("Unhandled Exception " + Exception.ToString(), Exception));
					//throw (new Exception(Exception.InnerException.ToString(), Exception.InnerException));
				}
			}
		}
	}
}
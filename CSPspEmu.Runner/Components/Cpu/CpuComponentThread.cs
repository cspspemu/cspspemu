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
using CSPspEmu.Hle.Vfs.Iso;

namespace CSPspEmu.Runner.Components.Cpu
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
		HleIoDriverEmulator HleIoDriverEmulator;
		public AutoResetEvent StoppedEndedEvent = new AutoResetEvent(false);

		public override void InitializeComponent()
		{
			CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			PspRtc = PspEmulatorContext.GetInstance<PspRtc>();
			ThreadManager = PspEmulatorContext.GetInstance<HleThreadManager>();
			HleState = PspEmulatorContext.GetInstance<HleState>();
			PspMemory = PspEmulatorContext.GetInstance<PspMemory>();

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
			HleIoDriverEmulator = new HleIoDriverEmulator(HleState);
			var MemoryStick = new HleIoDriverMemoryStick(MemoryStickMountable);
			//var MemoryStick = new HleIoDriverMemoryStick(new HleIoDriverLocalFileSystem(VirtualDirectory).AsReadonlyHleIoDriver());
			HleState.HleIoManager.SetDriver("ms:", MemoryStick);
			HleState.HleIoManager.SetDriver("fatms:", MemoryStick);
			HleState.HleIoManager.SetDriver("mscmhc:", MemoryStick);
			HleState.HleIoManager.SetDriver("disc:", MemoryStick);
			HleState.HleIoManager.SetDriver("emulator:", HleIoDriverEmulator);
			HleState.HleIoManager.SetDriver("kemulator:", HleIoDriverEmulator);
		}

		public IsoFile SetIso(string IsoFile)
		{
			var IsoFileStream = (Stream)File.OpenRead(IsoFile);
			string DetectedFormat;
			switch (DetectedFormat = new FormatDetector().Detect(IsoFileStream))
			{
				case "Cso":
					IsoFileStream = new CsoProxyStream(new Cso(IsoFileStream));
					break;
				case "Iso":
					break;
				default:
					throw (new InvalidDataException("Can't set an ISO for '" + DetectedFormat + "'"));
			}
			//"../../../TestInput/cube.iso"
			var Iso = new IsoFile(IsoFileStream, IsoFile);
			var Umd = new HleIoDriverIso(Iso);
			HleState.HleIoManager.SetDriver("disc:", Umd);
			HleState.HleIoManager.SetDriver(":", Umd);
			return Iso;
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
					.code CODE_PTR_FINALIZE_CALLBACK
						syscall 0x7778
						jr r31
						nop
				"
				.Replace("CODE_PTR_EXIT_THREAD", String.Format("0x{0:X}", HleEmulatorSpecialAddresses.CODE_PTR_EXIT_THREAD))
				.Replace("CODE_PTR_FINALIZE_CALLBACK", String.Format("0x{0:X}", HleEmulatorSpecialAddresses.CODE_PTR_FINALIZE_CALLBACK))
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
			RegisterModuleSyscall<ThreadManForUser>(0x7777, "_hle_sceKernelExitDeleteThread");
			RegisterModuleSyscall<Emulator>(0x7778, "finalizeCallback");
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

			var Loader = PspEmulatorContext.GetInstance<ElfPspLoader>();
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
					case "Cso":
					case "Iso":
						{
							var Iso = SetIso(FileName);
							ElfLoadStream = Iso.Root.Locate("/PSP_GAME/SYSDIR/BOOT.BIN").Open();
						}
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
					CpuThreadState.RA = HleEmulatorSpecialAddresses.CODE_PTR_EXIT_THREAD;
					CpuThreadState.GPR[4] = (int)argc; // A0
					CpuThreadState.GPR[5] = (int)argv; // A1
				}
				CpuThreadState.DumpRegisters();
				HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User).Dump();
					
				MainThread.CurrentStatus = HleThread.Status.Ready;
			}
		}

		private void Main_Ended()
		{
			StoppedEndedEvent.Set();

			// Completed execution. Wait for stopping.
			while (true)
			{
				ThreadTaskQueue.HandleEnqueued();
				if (!Running) return;
				Thread.Sleep(1);
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
					if (Exception is SceKernelSelfStopUnloadModuleException || Exception.InnerException is SceKernelSelfStopUnloadModuleException)
					{
						Console.WriteLine("SceKernelSelfStopUnloadModuleException");
						Main_Ended();
						return;
					}

					var ErrorOut = Console.Error;

					ConsoleUtils.SaveRestoreConsoleState(() =>
					{
						Console.ForegroundColor = ConsoleColor.Red;

						try
						{
							ErrorOut.WriteLine("Error on thread {0}", ThreadManager.Current);

							ErrorOut.WriteLine(Exception);

							ThreadManager.Current.CpuThreadState.DumpRegisters(ErrorOut);

							ErrorOut.WriteLine(
								"Last registered PC = 0x{0:X}, RA = 0x{1:X}",
								ThreadManager.Current.CpuThreadState.PC,
								ThreadManager.Current.CpuThreadState.RA
							);

							foreach (var Thread in ThreadManager.Threads)
							{
								ErrorOut.WriteLine("{0}", Thread);
							}

							ErrorOut.WriteLine("Executable had relocation: {0}", PspEmulatorContext.PspConfig.InfoExeHasRelocation);
						}
						catch (Exception Exception2)
						{
							Console.WriteLine("{0}", Exception2);
						}
					});

					Main_Ended();
				}
			}
		}
	}
}
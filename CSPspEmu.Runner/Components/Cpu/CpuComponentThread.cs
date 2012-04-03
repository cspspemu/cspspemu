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
using System.Diagnostics;
using CSPspEmu.Hle.Vfs.Zip;
using CSPspEmu.Resources;
using CSPspEmu.Hle.Formats.Archive;

namespace CSPspEmu.Runner.Components.Cpu
{
	unsafe sealed public class CpuComponentThread : ComponentThread
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
			string MemoryStickRootFolder = ApplicationPaths.MemoryStickRootFolder;
			//Console.Error.WriteLine(MemoryStickRootFolder);
			//Console.ReadKey();
			try { Directory.CreateDirectory(MemoryStickRootFolder); }
			catch { }
			/*
			*/

			MemoryStickMountable = new HleIoDriverMountable();
			MemoryStickMountable.Mount("/", new HleIoDriverLocalFileSystem(MemoryStickRootFolder));
			HleIoDriverEmulator = new HleIoDriverEmulator(HleState);
			var MemoryStick = new HleIoDriverMemoryStick(HleState, MemoryStickMountable);
			//var MemoryStick = new HleIoDriverMemoryStick(new HleIoDriverLocalFileSystem(VirtualDirectory).AsReadonlyHleIoDriver());

			// http://forums.ps2dev.org/viewtopic.php?t=5680
			HleState.HleIoManager.SetDriver("ms:", MemoryStick);
			HleState.HleIoManager.SetDriver("fatms:", MemoryStick);
			HleState.HleIoManager.SetDriver("fatmsOem:", MemoryStick);
			HleState.HleIoManager.SetDriver("mscmhc:", MemoryStick);

			HleState.HleIoManager.SetDriver("msstor:", new ReadonlyHleIoDriver(MemoryStick));
			HleState.HleIoManager.SetDriver("msstor0p:", new ReadonlyHleIoDriver(MemoryStick));

			HleState.HleIoManager.SetDriver("disc:", MemoryStick);
			HleState.HleIoManager.SetDriver("umd:", MemoryStick);

			HleState.HleIoManager.SetDriver("emulator:", HleIoDriverEmulator);
			HleState.HleIoManager.SetDriver("kemulator:", HleIoDriverEmulator);

			HleState.HleIoManager.SetDriver("flash:", new HleIoDriverZip(new ZipArchive(ResourceArchive.GetFlash0ZipFileStream())));
		}

		public IsoFile SetIso(string IsoFile)
		{
			var IsoFileStream = (Stream)File.OpenRead(IsoFile);
			string DetectedFormat;
			switch (DetectedFormat = new FormatDetector().Detect(IsoFileStream))
			{
				case "Cso":
					IsoFileStream = new CompressedIsoProxyStream(new Cso(IsoFileStream));
					break;
				case "Dax":
					IsoFileStream = new CompressedIsoProxyStream(new Dax(IsoFileStream));
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
			HleState.HleIoManager.SetDriver("umd:", Umd);
			HleState.HleIoManager.SetDriver("host:", Umd);
			HleState.HleIoManager.SetDriver(":", Umd);
			HleState.HleIoManager.Chdir("disc0:/PSP_GAME/USRDIR");
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
			CpuProcessor.RegisterNativeSyscall(SyscallCode, (CpuThreadState, Code) =>
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
				List<Stream> ElfLoadStreamTry = new List<Stream>();
				//Stream ElfLoadStream = null;

				var Format = new FormatDetector().Detect(LoadStream);
				String Title = null;
				switch (Format)
				{
					case "Pbp":
						{
							var Pbp = new Pbp().Load(LoadStream);
							ElfLoadStreamTry.Add(Pbp[Pbp.Types.PspData]);
							try
							{
								var ParamSfo = new Psf().Load(Pbp[Pbp.Types.ParamSfo]);
								Title = (String)ParamSfo.EntryDictionary["TITLE"];
							}
							catch (Exception Exception)
							{
								Console.Error.WriteLine(Exception);
							}
						}
						break;
					case "Elf":
						ElfLoadStreamTry.Add(LoadStream);
						break;
					case "Dax":
					case "Cso":
					case "Iso":
						{
							var Iso = SetIso(FileName);
							try
							{
								var ParamSfo = new Psf().Load(Iso.Root.Locate("/PSP_GAME/PARAM.SFO").Open());
								Title = (String)ParamSfo.EntryDictionary["TITLE"];
							}
							catch (Exception Exception)
							{
								Console.Error.WriteLine(Exception);
							}

							var FilesToTry = new[] {
								"/PSP_GAME/SYSDIR/BOOT.BIN",
								"/PSP_GAME/SYSDIR/EBOOT.BIN",
							};

							foreach (var FileToTry in FilesToTry)
							{
								ElfLoadStreamTry.Add(Iso.Root.Locate(FileToTry).Open());
								//if (ElfLoadStream.Length != 0) break;
							}

							/*
							if (ElfLoadStream.Length == 0)
							{
								throw (new Exception(String.Format("{0} files are empty", String.Join(", ", FilesToTry))));
							}
							*/
						}
						break;
					default:
						throw (new NotImplementedException("Can't load format '" + Format + "'"));
				}

				Exception LoadException = null;

				foreach (var ElfLoadStream in ElfLoadStreamTry)
				{
					try
					{
						Loader.Load(
							ElfLoadStream,
							MemoryStream,
							HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User),
							HleState.ModuleManager,
							Title
						);

						LoadException = null;

						break;
					}
					catch (Exception Exception)
					{
						LoadException = Exception;
					}
				}

				if (LoadException != null) throw (LoadException);

				RegisterSyscalls();

				uint StartArgumentAddress = 0x08000100;
				uint EndArgumentAddress = StartArgumentAddress;

				var Arguments = new[] {
					"ms0:/PSP/GAME/virtual/EBOOT.PBP",
				};

				var ArgumentsChunk = Arguments
					.Select(Argument => Encoding.UTF8.GetBytes(Argument + "\0"))
					.Aggregate(new byte[] { }, (Accumulate, Chunk) => Accumulate.Concat(Chunk))
				;

				var ReservedSyscallsPartition = HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.Kernel0).Allocate(
					0x100,
					Name: "ReservedSyscallsPartition"
				);
				var ArgumentsPartition = HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.Kernel0).Allocate(
					ArgumentsChunk.Length,
					Name: "ArgumentsPartition"
				);
				PspMemory.WriteBytes(ArgumentsPartition.Low, ArgumentsChunk);

				var ThreadManForUser = HleState.ModuleManager.GetModule<ThreadManForUser>();
				Debug.Assert(ThreadManForUser != null);


				//var MainThread = HleState.ThreadManager.Create();
				//var CpuThreadState = MainThread.CpuThreadState;
				var CurrentCpuThreadState = new CpuThreadState(CpuProcessor);
				{
					//CpuThreadState.PC = Loader.InitInfo.PC;
					CurrentCpuThreadState.GP = Loader.InitInfo.GP;

					int ThreadId = (int)ThreadManForUser.sceKernelCreateThread(CurrentCpuThreadState, "<EntryPoint>", Loader.InitInfo.PC, 10, 0x1000, PspThreadAttributes.ClearStack, null);
					ThreadManForUser._sceKernelStartThread(CurrentCpuThreadState, ThreadId, ArgumentsPartition.Size, ArgumentsPartition.Low);
				}
				CurrentCpuThreadState.DumpRegisters();
				HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User).Dump();
					
				//MainThread.CurrentStatus = HleThread.Status.Ready;
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
							try
							{
								ErrorOut.WriteLine(Exception);
							}
							catch
							{
							}

							ThreadManager.Current.CpuThreadState.DumpRegisters(ErrorOut);

							ErrorOut.WriteLine(
								"Last registered PC = 0x{0:X}, RA = 0x{1:X}, RelocatedBaseAddress=0x{2:X}, UnrelocatedPC=0x{3:X}",
								ThreadManager.Current.CpuThreadState.PC,
								ThreadManager.Current.CpuThreadState.RA,
								PspEmulatorContext.PspConfig.RelocatedBaseAddress,
								ThreadManager.Current.CpuThreadState.PC - PspEmulatorContext.PspConfig.RelocatedBaseAddress
							);

							ErrorOut.WriteLine("Last called syscalls: ");
							foreach (var CalledCallback in HleState.ModuleManager.LastCalledCallbacks.Reverse())
							{
								ErrorOut.WriteLine("  {0}", CalledCallback);
							}

							foreach (var Thread in ThreadManager.Threads)
							{
								ErrorOut.WriteLine("{0}", Thread);
								ErrorOut.WriteLine(
									"Last valid PC: 0x{0:X} :, 0x{1:X}",
									Thread.CpuThreadState.LastValidPC,
									Thread.CpuThreadState.LastValidPC - PspEmulatorContext.PspConfig.RelocatedBaseAddress
								);
								Thread.DumpStack(ErrorOut);
							}

							ErrorOut.WriteLine(
								"Executable had relocation: {0}. RelocationAddress: 0x{1:X}",
								PspEmulatorContext.PspConfig.InfoExeHasRelocation,
								PspEmulatorContext.PspConfig.RelocatedBaseAddress
							);

							ErrorOut.WriteLine("");
							ErrorOut.WriteLine("Error on thread {0}", ThreadManager.Current);
							ErrorOut.WriteLine(Exception);
							ErrorOut.WriteLine("Saved a memory dump to 'error_memorydump.bin'", ThreadManager.Current);

							var Memory = HleState.MemoryManager.Memory;

							Memory.Dump("error_memorydump.bin");
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

		public void DumpThreads()
		{
			var ErrorOut = Console.Out;
			foreach (var Thread in ThreadManager.Threads.ToArray())
			{
				ErrorOut.WriteLine("{0}", Thread);
				Thread.DumpStack(ErrorOut);
			}
			//throw new NotImplementedException();
		}
	}
}
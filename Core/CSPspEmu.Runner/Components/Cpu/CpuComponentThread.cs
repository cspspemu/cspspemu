using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CSharpUtils;
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
using CSPspEmu.Hle.Vfs;
using CSPspEmu.Hle.Vfs.Local;
using CSPspEmu.Hle.Vfs.MemoryStick;
using CSPspEmu.Hle.Vfs.Iso;
using CSPspEmu.Hle.Vfs.Zip;
using CSPspEmu.Resources;
using CSPspEmu.Hle.Formats.Archive;
using System.Reflection;
using CSPspEmu.Interop;
using CSPspEmu.Hle.Vfs.Emulator;

namespace CSPspEmu.Runner.Components.Cpu
{
    public unsafe sealed class CpuComponentThread : ComponentThread, IInjectInitialize
    {
        static Logger Logger = Logger.GetLogger("CpuComponentThread");

        protected override string ThreadName => "CpuThread";

        [Inject] public CpuProcessor CpuProcessor;

        [Inject] public PspRtc PspRtc;

        [Inject] public HleThreadManager HleThreadManager;

        [Inject] public PspMemory PspMemory;

        [Inject] public ElfPspLoader Loader;

        [Inject] public HleMemoryManager MemoryManager;

        [Inject] public HleCallbackManager HleCallbackManager;

        [Inject] public HleModuleManager ModuleManager;

        [Inject] public HleIoManager HleIoManager;

        [Inject] public ThreadManForUser ThreadManForUser;

        [Inject] public HleIoDriverEmulator HleIoDriverEmulator;

        [Inject] public ElfConfig ElfConfig;

        [Inject] public HleConfig HleConfig;

        public HleIoDriverMountable MemoryStickMountable;

        public AutoResetEvent StoppedEndedEvent = new AutoResetEvent(false);

        void IInjectInitialize.Initialize()
        {
            RegisterDevices();
        }

        void RegisterDevices()
        {
            var memoryStickRootFolder = ApplicationPaths.MemoryStickRootFolder;
            //Console.Error.WriteLine(MemoryStickRootFolder);
            //Console.ReadKey();
            try
            {
                Directory.CreateDirectory(memoryStickRootFolder);
            }
            catch
            {
                // ignored
            }
            /*
            */

            MemoryStickMountable = new HleIoDriverMountable();
            MemoryStickMountable.Mount("/", new HleIoDriverLocalFileSystem(memoryStickRootFolder));
            var memoryStick = new HleIoDriverMemoryStick(PspMemory, HleCallbackManager, MemoryStickMountable);
            //var MemoryStick = new HleIoDriverMemoryStick(new HleIoDriverLocalFileSystem(VirtualDirectory).AsReadonlyHleIoDriver());

            // http://forums.ps2dev.org/viewtopic.php?t=5680
            HleIoManager.SetDriver("host:", memoryStick);
            HleIoManager.SetDriver("ms:", memoryStick);
            HleIoManager.SetDriver("fatms:", memoryStick);
            HleIoManager.SetDriver("fatmsOem:", memoryStick);
            HleIoManager.SetDriver("mscmhc:", memoryStick);

            HleIoManager.SetDriver("msstor:", new ReadonlyHleIoDriver(memoryStick));
            HleIoManager.SetDriver("msstor0p:", new ReadonlyHleIoDriver(memoryStick));

            HleIoManager.SetDriver("disc:", memoryStick);
            HleIoManager.SetDriver("umd:", memoryStick);

            HleIoManager.SetDriver("emulator:", HleIoDriverEmulator);
            HleIoManager.SetDriver("kemulator:", HleIoDriverEmulator);

            HleIoManager.SetDriver("flash:",
                new HleIoDriverZip(new ZipArchive(ResourceArchive.GetFlash0ZipFileStream())));
        }

        public IsoFile SetIso(string isoFile)
        {
            //"../../../TestInput/cube.iso"
            var iso = IsoLoader.GetIso(isoFile);
            var umd = new HleIoDriverIso(iso);
            HleIoManager.SetDriver("disc:", umd);
            HleIoManager.SetDriver("umd:", umd);
            //HleIoManager.SetDriver("host:", Umd);
            HleIoManager.SetDriver(":", umd);
            HleIoManager.Chdir("disc0:/PSP_GAME/USRDIR");
            return iso;
        }

        void SetVirtualFolder(string virtualDirectory)
        {
            MemoryStickMountable.Mount(
                "/PSP/GAME/virtual",
                new HleIoDriverLocalFileSystem(virtualDirectory)
                //.AsReadonlyHleIoDriver()
            );
        }

        void RegisterSyscalls()
        {
            new MipsAssembler(new PspMemoryStream(PspMemory)).Assemble(
                @"
					.code CODE_PTR_EXIT_THREAD
						syscall CODE_PTR_EXIT_THREAD_SYSCALL
						jr r31
						nop
					.code CODE_PTR_FINALIZE_CALLBACK
						syscall CODE_PTR_FINALIZE_CALLBACK_SYSCALL
						jr r31
						nop
				"
                    .Replace("CODE_PTR_EXIT_THREAD_SYSCALL", $"0x{HleEmulatorSpecialAddresses.CODE_PTR_EXIT_THREAD_SYSCALL:X}")
                    .Replace("CODE_PTR_FINALIZE_CALLBACK_SYSCALL", $"0x{HleEmulatorSpecialAddresses.CODE_PTR_FINALIZE_CALLBACK_SYSCALL:X}")
                    .Replace("CODE_PTR_EXIT_THREAD", $"0x{HleEmulatorSpecialAddresses.CODE_PTR_EXIT_THREAD:X}")
                    .Replace("CODE_PTR_FINALIZE_CALLBACK", $"0x{HleEmulatorSpecialAddresses.CODE_PTR_FINALIZE_CALLBACK:X}")
            );

            //var ThreadManForUser = ModuleManager.GetModule<ThreadManForUser>();

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
            RegisterModuleSyscall<ThreadManForUser>(HleEmulatorSpecialAddresses.CODE_PTR_EXIT_THREAD_SYSCALL,
                (Func<CpuThreadState, int>) new ThreadManForUser()._hle_sceKernelExitDeleteThread);
            RegisterModuleSyscall<Emulator>(HleEmulatorSpecialAddresses.CODE_PTR_FINALIZE_CALLBACK_SYSCALL,
                (Action<CpuThreadState>) new Emulator().finalizeCallback);
        }

        void RegisterModuleSyscall<TType>(int syscallCode, Delegate Delegate) where TType : HleModuleHost
        {
            RegisterModuleSyscall<TType>(syscallCode, Delegate.Method.Name);
        }

        void RegisterModuleSyscall<TType>(int syscallCode, string functionName) where TType : HleModuleHost
        {
            var Delegate = ModuleManager.GetModuleDelegate<TType>(functionName);
            CpuProcessor.RegisterNativeSyscall(syscallCode, (cpuThreadState, code) => { Delegate(cpuThreadState); });
        }

        public void _LoadFile(String fileName)
        {
            //GC.Collect();
            SetVirtualFolder(Path.GetDirectoryName(fileName));

            var memoryStream = new PspMemoryStream(PspMemory);

            var arguments = new[]
            {
                "ms0:/PSP/GAME/virtual/EBOOT.PBP",
            };

            Stream loadStream = File.OpenRead(fileName);
            //using ()
            {
                var elfLoadStreamTry = new List<Stream>();
                //Stream ElfLoadStream = null;

                var format = new FormatDetector().DetectSubType(loadStream);
                string title = null;
                switch (format)
                {
                    case FormatDetector.SubType.Pbp:
                    {
                        var pbp = new Pbp().Load(loadStream);
                        elfLoadStreamTry.Add(pbp[Pbp.Types.PspData]);
                        Logger.TryCatch(() =>
                        {
                            var paramSfo = new Psf().Load(pbp[Pbp.Types.ParamSfo]);

                            if (paramSfo.EntryDictionary.ContainsKey("TITLE"))
                            {
                                title = (string) paramSfo.EntryDictionary["TITLE"];
                            }

                            if (paramSfo.EntryDictionary.ContainsKey("PSP_SYSTEM_VER"))
                            {
                                HleConfig.FirmwareVersion = paramSfo.EntryDictionary["PSP_SYSTEM_VER"].ToString();
                            }
                        });
                    }
                        break;
                    case FormatDetector.SubType.Elf:
                        elfLoadStreamTry.Add(loadStream);
                        break;
                    case FormatDetector.SubType.Dax:
                    case FormatDetector.SubType.Cso:
                    case FormatDetector.SubType.Iso:
                    {
                        arguments[0] = "disc0:/PSP/GAME/SYSDIR/EBOOT.BIN";

                        var iso = SetIso(fileName);
                        Logger.TryCatch(() =>
                        {
                            var paramSfo = new Psf().Load(iso.Root.Locate("/PSP_GAME/PARAM.SFO").Open());
                            title = (string) paramSfo.EntryDictionary["TITLE"];
                        });

                        var filesToTry = new[]
                        {
                            "/PSP_GAME/SYSDIR/BOOT.BIN",
                            "/PSP_GAME/SYSDIR/EBOOT.BIN",
                            "/PSP_GAME/SYSDIR/EBOOT.OLD",
                        };

                        foreach (var fileToTry in filesToTry)
                        {
                            try
                            {
                                elfLoadStreamTry.Add(iso.Root.Locate(fileToTry).Open());
                            }
                            catch
                            {
                                // ignored
                            }
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
                        throw (new NotImplementedException("Can't load format '" + format + "'"));
                }

                Exception loadException = null;
                HleModuleGuest hleModuleGuest = null;

                foreach (var elfLoadStream in elfLoadStreamTry)
                {
                    try
                    {
                        loadException = null;

                        if (elfLoadStream.Length < 256) throw(new InvalidProgramException("File too short"));

                        hleModuleGuest = Loader.LoadModule(
                            elfLoadStream,
                            memoryStream,
                            MemoryManager.GetPartition(MemoryPartitions.User),
                            ModuleManager,
                            title,
                            ModuleName: fileName,
                            IsMainModule: true
                        );

                        loadException = null;

                        break;
                    }
                    catch (InvalidProgramException exception)
                    {
                        loadException = exception;
                    }
                }

                if (loadException != null) throw (loadException);

                RegisterSyscalls();

                uint StartArgumentAddress = 0x08000100;
                uint endArgumentAddress = StartArgumentAddress;

                var argumentsChunk = arguments
                        .Select(argument => Encoding.UTF8.GetBytes(argument + "\0"))
                        .Aggregate(new byte[] { }, (accumulate, chunk) => (byte[]) accumulate.Concat(chunk))
                    ;

                var reservedSyscallsPartition = MemoryManager.GetPartition(MemoryPartitions.Kernel0).Allocate(
                    0x100,
                    Name: "ReservedSyscallsPartition"
                );
                var argumentsPartition = MemoryManager.GetPartition(MemoryPartitions.Kernel0).Allocate(
                    argumentsChunk.Length,
                    Name: "ArgumentsPartition"
                );
                PspMemory.WriteBytes(argumentsPartition.Low, argumentsChunk);

                Debug.Assert(ThreadManForUser != null);

                // @TODO: Use Module Manager

                //var MainThread = ThreadManager.Create();
                //var CpuThreadState = MainThread.CpuThreadState;
                var currentCpuThreadState = new CpuThreadState(CpuProcessor);
                {
                    //CpuThreadState.PC = Loader.InitInfo.PC;
                    currentCpuThreadState.GP = hleModuleGuest.InitInfo.GP;
                    currentCpuThreadState.CallerModule = hleModuleGuest;

                    int threadId = (int) ThreadManForUser.sceKernelCreateThread(currentCpuThreadState, "<EntryPoint>",
                        hleModuleGuest.InitInfo.PC, 10, 0x1000, PspThreadAttributes.ClearStack, null);

                    //var Thread = HleThreadManager.GetThreadById(ThreadId);
                    ThreadManForUser._sceKernelStartThread(currentCpuThreadState, threadId, argumentsPartition.Size,
                        argumentsPartition.Low);
                    //Console.WriteLine("RA: 0x{0:X}", CurrentCpuThreadState.RA);
                }
                currentCpuThreadState.DumpRegisters();
                MemoryManager.GetPartition(MemoryPartitions.User).Dump();
                //ModuleManager.LoadedGuestModules.Add(HleModuleGuest);

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
#if !DO_NOT_PROPAGATE_EXCEPTIONS
                try
#endif
                {
                    // HACK! TODO: Update PspRtc every 2 thread switchings.
                    // Note: It should update the RTC after selecting the next thread to run.
                    // But currently is is not possible since updating the RTC and waking up
                    // threads has secondary effects that I have to consideer first.
                    var tickAlternate = false;

                    //PspRtc.Update();
                    while (true)
                    {
                        ThreadTaskQueue.HandleEnqueued();
                        if (!Running) return;

                        if (!tickAlternate) PspRtc.Update();
                        tickAlternate = !tickAlternate;

                        HleThreadManager.StepNext(DoBeforeSelectingNext: () =>
                        {
                            //PspRtc.Update();
                        });
                    }
                }
#if !DO_NOT_PROPAGATE_EXCEPTIONS
                catch (Exception exception)
                {
                    if (exception is SceKernelSelfStopUnloadModuleException ||
                        exception.InnerException is SceKernelSelfStopUnloadModuleException)
                    {
                        Console.WriteLine("SceKernelSelfStopUnloadModuleException");
                        Main_Ended();
                        return;
                    }

                    var errorOut = Console.Error;

                    ConsoleUtils.SaveRestoreConsoleState(() =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;

                        try
                        {
                            errorOut.WriteLine("Error on thread {0}", HleThreadManager.Current);
                            try
                            {
                                errorOut.WriteLine(exception);
                            }
                            catch
                            {
                                // ignored
                            }

                            HleThreadManager.Current.CpuThreadState.DumpRegisters(errorOut);

                            errorOut.WriteLine(
                                "Last registered PC = 0x{0:X}, RA = 0x{1:X}, RelocatedBaseAddress=0x{2:X}, UnrelocatedPC=0x{3:X}",
                                HleThreadManager.Current.CpuThreadState.PC,
                                HleThreadManager.Current.CpuThreadState.RA,
                                ElfConfig.RelocatedBaseAddress,
                                HleThreadManager.Current.CpuThreadState.PC - ElfConfig.RelocatedBaseAddress
                            );

                            errorOut.WriteLine("Last called syscalls: ");
                            foreach (var calledCallback in ModuleManager.LastCalledCallbacks.Reverse())
                            {
                                errorOut.WriteLine("  {0}", calledCallback);
                            }

                            foreach (var thread in HleThreadManager.Threads)
                            {
                                errorOut.WriteLine("{0}", thread.ToExtendedString());
                                errorOut.WriteLine(
                                    "Last valid PC: 0x{0:X} :, 0x{1:X}",
                                    thread.CpuThreadState.LastValidPC,
                                    thread.CpuThreadState.LastValidPC - ElfConfig.RelocatedBaseAddress
                                );
                                thread.DumpStack(errorOut);
                            }

                            errorOut.WriteLine(
                                "Executable had relocation: {0}. RelocationAddress: 0x{1:X}",
                                ElfConfig.InfoExeHasRelocation,
                                ElfConfig.RelocatedBaseAddress
                            );

                            errorOut.WriteLine("");
                            errorOut.WriteLine("Error on thread {0}", HleThreadManager.Current);
                            errorOut.WriteLine(exception);

                            //ErrorOut.WriteLine("Saved a memory dump to 'error_memorydump.bin'", HleThreadManager.Current);
                            //MemoryManager.Memory.Dump("error_memorydump.bin");
                        }
                        catch (Exception exception2)
                        {
                            Console.WriteLine("{0}", exception2);
                        }
                    });

                    Main_Ended();
                }
#endif
            }
        }

        public void DumpThreads()
        {
            var errorOut = Console.Out;
            foreach (var thread in HleThreadManager.Threads.ToArray())
            {
                errorOut.WriteLine("{0}", thread);
                thread.DumpStack(errorOut);
            }
            //throw new NotImplementedException();
        }
    }
}
using System;
using System.Linq;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
//using CSPspEmu.Core.Audio.Impl.Openal;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Gui.Winforms;
using CSPspEmu.Runner;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Hle.Loader;
using CSPspEmu.cheats;
using CSPspEmu.Inject;
using CSPspEmu.Gui.texture;

namespace CSPspEmu
{
    class PspEmulator : IGuiExternalInterface, IDisposable
    {
        [Inject] CpuConfig CpuConfig;

        [Inject] GpuConfig GpuConfig;

        [Inject] public GuiConfig GuiConfig;

        [Inject] HleConfig HleConfig;

        [Inject] DisplayConfig DisplayConfig;

        [Inject] PspMemory PspMemory;

        [Inject] ElfConfig ElfConfig;

        [Inject] GpuImpl GpuImpl;

        [Inject] PspDisplay PspDisplay;

        [Inject] CWCheatPlugin CWCheatPlugin;

        [Inject] TextureHookPlugin TextureHookPlugin;

        public InjectContext InjectContext
        {
            get
            {
                lock (this)
                {
                    return _InjectContext;
                }
            }
        }

        [Inject] private InjectContext _InjectContext;

        [Inject] PspRunner PspRunner;

        PspStoredConfig StoredConfig;

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
                if (PspRunner == null) return false;
                return PspRunner.Paused;
            }
        }

        public void Pause()
        {
            if (!Paused)
            {
                Console.WriteLine("Pausing...");
                PspRunner.PauseSynchronized();
                Console.WriteLine("Pausing...Ok");
            }
        }

        public void Resume()
        {
            if (Paused)
            {
                PspRunner.ResumeSynchronized();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PspEmulator()
        {
            StoredConfig = PspStoredConfig.Load();
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartAndLoad(string File, bool TraceSyscalls = false, bool ShowMenus = true,
            bool TrackCallStack = true, bool? EnableMpeg = null)
        {
            Start(() =>
            {
                CpuConfig.DebugSyscalls = TraceSyscalls;
                CpuConfig.TrackCallStack = TrackCallStack;
                LoadFile(File);
            }, ShowMenus: ShowMenus, AutoLoad: true);
        }

        /// <summary>
        /// Start.
        /// </summary>
        public void Start(Action CallbackOnInit = null, bool ShowMenus = true, bool AutoLoad = false,
            bool TrackCallStack = true)
        {
            try
            {
                // Creates a temporal context.
                //PspEmulatorContext = new PspEmulatorContext(PspConfig);

                // Creates a new context.
                CreateNewContextAndRemoveOldOne();

                if (CallbackOnInit != null)
                {
                    CallbackOnInit();
                }
                // GUI Thread.
                Thread.CurrentThread.Name = "GuiThread";

                GuiConfig.ShowMenus = ShowMenus;
                GuiConfig.AutoLoad = AutoLoad;
                GuiConfig.DefaultDisplayScale = ShowMenus ? 1 : 2;
                //ContextInitialized.WaitOne();

                new GuiRunner(this).Start();

                PspRunner.StopSynchronized();
            }
            catch (Exception Exception)
            {
                Console.Error.WriteLine(Exception);
            }
            finally
            {
                StoredConfig.Save();
            }

            Console.WriteLine("Exiting...");
            //foreach (var thread in Process.GetCurrentProcess().Threads.Cast<ProcessThread>())
            //{
            //	Console.WriteLine("Thread: {0}, {1}", thread.ThreadState, (thread.ThreadState == System.Diagnostics.ThreadState.Wait) ?  thread.WaitReason.ToString() : "");
            //}
            //Environment.Exit(0);
            return;
        }

        [Inject] MessageBus MessageBus;

        public void LoadFile(string FileName)
        {
            Console.WriteLine("LoadFile...{0}", FileName);
            CreateNewContextAndRemoveOldOne();

            MessageBus.Dispatch(new LoadFileMessage() {FileName = FileName});

            PspRunner.CpuComponentThread.ThreadTaskQueue.EnqueueAndWaitCompleted(() =>
            {
                PspRunner.CpuComponentThread._LoadFile(FileName);
            });
        }

        void CreateNewContextAndRemoveOldOne()
        {
            Console.WriteLine("----------------------------------------------");
            // Stops the current context if it has one already.
            if (PspRunner != null)
            {
                PspRunner.StopSynchronized();

                InjectContext.GetInstance<PspMemory>().Dispose();
                InjectContext.GetInstance<GpuImpl>().StopSynchronized();
                InjectContext.GetInstance<PspAudioImpl>().StopSynchronized();

                PspRunner = null;
                _InjectContext.Dispose();
                _InjectContext = null;
                GC.Collect();
            }

            lock (this)
            {
                _InjectContext = PspInjectContext.CreateInjectContext(StoredConfig, test: false);
                _InjectContext.SetInstanceType<IGuiExternalInterface, PspEmulator>();

                _InjectContext.InjectDependencesTo(this);

                PspRunner.StartSynchronized();
            }

            //GpuImpl.InitSynchronizedOnce();
        }

        public void ShowDebugInformation()
        {
            var CpuProcessor = InjectContext.GetInstance<CpuProcessor>();
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("ShowDebugInformation:");
            Console.WriteLine("-----------------------------------------------------------------");
            try
            {
                foreach (var Pair in CpuProcessor.GlobalInstructionStats.OrderBy(Pair => Pair.Value))
                {
                    Console.WriteLine("{0} -> {1}", Pair.Key, Pair.Value);
                }
            }
            catch (Exception Exception)
            {
                Console.Error.WriteLine(Exception);
            }

            /*
            Console.WriteLine("-----------------------------------------------------------------");
            foreach (var Pair in CpuProcessor.GlobalInstructionStats.OrderBy(Pair => Pair.Key)) Console.WriteLine("{0} -> {1}", Pair.Key, Pair.Value);
            */

            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("Last called syscalls: ");
            try
            {
                foreach (var CalledCallback in InjectContext.GetInstance<HleModuleManager>().LastCalledCallbacks
                    .ToArray().Reverse())
                {
                    Console.WriteLine("  {0}", CalledCallback);
                }
            }
            catch (Exception Exception)
            {
                Console.Error.WriteLine(Exception);
            }
            Console.WriteLine("-----------------------------------------------------------------");
            try
            {
                PspRunner.CpuComponentThread.DumpThreads();
            }
            catch (Exception Exception)
            {
                Console.Error.WriteLine(Exception);
            }
            Console.WriteLine("-----------------------------------------------------------------");

            //foreach (var Instruction in CpuProcessor.GlobalInstructionStats.OrderBy(Item => Item.Key))
            //{
            //	Console.WriteLine("{0}: {1}", Instruction.Key, Instruction.Value);
            //}
            //
            //Console.WriteLine("-----------------------------------------------------------------");
        }

        public PluginInfo GetAudioPluginInfo()
        {
            return InjectContext.GetInstance<PspAudioImpl>().PluginInfo;
        }

        public PluginInfo GetGpuPluginInfo()
        {
            return InjectContext.GetInstance<GpuImpl>().PluginInfo;
        }

        public void CaptureGpuFrame()
        {
            InjectContext.GetInstance<GpuProcessor>().CaptureFrame();
        }

        public object GetCpuProcessor()
        {
            return InjectContext.GetInstance<CpuProcessor>();
        }

        void IDisposable.Dispose()
        {
            Console.WriteLine("PspEmulator.Dispose()");
            InjectContext.Dispose();
            _InjectContext = null;
        }
    }
}
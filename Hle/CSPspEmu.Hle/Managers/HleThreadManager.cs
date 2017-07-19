//#define DEBUG_THREADS

//#define DISABLE_CALLBACKS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CSharpUtils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core;
using CSPspEmu.Core.Components.Display;
using CSharpUtils.Threading;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Hle.Interop;
using CSPspEmu.Hle.Vfs.MemoryStick;

namespace CSPspEmu.Hle.Managers
{
    public partial class HleThreadManager : IInjectInitialize, ICpuConnector, IGpuConnector
    {
        private static readonly Logger Logger = Logger.GetLogger("HleThreadManager");

        [Inject] internal CpuProcessor Processor;

        [Inject] DisplayConfig DisplayConfig;

        [Inject] HleConfig HleConfig;

        [Inject] private HleCallbackManager HleCallbackManager;

        [Inject] private HleInterruptManager HleInterruptManager;

        [Inject] private HleInterop HleInterop;

        [Inject] private InjectContext InjectContext;

        public readonly List<HleThread> Threads = new List<HleThread>(128);

        public readonly Dictionary<int, HleThread> ThreadsById = new Dictionary<int, HleThread>(128);

        protected int LastId = 1;

        public HleThread Current;

        public CoroutinePool CoroutinePool = new CoroutinePool();

        private HleThreadManager()
        {
        }

        void ICpuConnector.Yield(CpuThreadState CpuThreadState)
        {
            if (HleConfig.UseCoRoutines)
            {
                CoroutinePool.YieldInPool();
            }
            else
            {
                GreenThread.Yield();
            }
        }

        void IGpuConnector.Signal(uint PC, PspGeCallbackData CallbackData, uint Signal, SignalBehavior Behavior,
            bool ExecuteNow)
        {
            if (_DynarecConfig.EnableGpuSignalsCallback)
            {
                if (HleConfig.CompilerVersion <= 0x01FFFFFF) PC = 0;

                Console.Error.WriteLine(
                    "HleThreadManager:: IGpuConnector.Signal :: 0x{0:X8}, 0x{1:X8}, 0x{2:X8}, {3}, {4}",
                    CallbackData.SignalFunction, CallbackData.SignalArgument, PC, Signal, Behavior);
                HleInterop.ExecuteFunctionNowLater(CallbackData.SignalFunction, ExecuteNow,
                    new object[] {Signal, CallbackData.SignalArgument, PC});
            }
            else
            {
            }
        }

        void IGpuConnector.Finish(uint PC, PspGeCallbackData CallbackData, uint Arg, bool ExecuteNow)
        {
            if (_DynarecConfig.EnableGpuFinishCallback)
            {
                if (HleConfig.CompilerVersion <= 0x01FFFFFF) PC = 0;

                Console.Error.WriteLine("HleThreadManager:: IGpuConnector.Finish :: 0x{0:X8}, 0x{1:X8}, 0x{2:X8}, {3}",
                    CallbackData.FinishFunction, CallbackData.FinishArgument, PC, Arg);
                HleInterop.ExecuteFunctionNowLater(CallbackData.FinishFunction, ExecuteNow,
                    new object[] {Arg, CallbackData.FinishArgument, PC});
            }
        }

        void IInjectInitialize.Initialize()
        {
            Processor.DebugCurrentThreadEvent += DebugCurrentThread;
        }

        /// <summary>
        /// 
        /// </summary>
        public PreemptiveScheduler<HleThread> PreemptiveScheduler =
            new PreemptiveScheduler<HleThread>(NewItemsFirst: true, ThrowException: false);

        public enum SCE_KERNEL_DISPATCHTHREAD_STATE : uint
        {
            DISABLED = 0,
            ENABLED = 1,
        }

        public SCE_KERNEL_DISPATCHTHREAD_STATE DispatchingThreads = SCE_KERNEL_DISPATCHTHREAD_STATE.ENABLED;

        public HleThread CurrentOrAny
        {
            get
            {
                if (Current != null) return Current;
                return Threads.First();
            }
        }

        public void DebugCurrentThread()
        {
            Console.Error.WriteLine("HleThreadManager.CpuProcessor.DebugCurrentThreadEvent:");
            Console.Error.WriteLine(Current);
        }

        /// <summary>
        /// Execute current thread steps until it can execute other thread.
        /// </summary>
        /// <param name="Current"></param>
        private void ExecuteCurrent(HleThread Current)
        {
            do
            {
                ExecuteQueuedCallbacks();
                ExecuteQueuedInterrupts();

                if (Current.HasAllStatus(HleThread.Status.Suspend)) return;
                Current.Step();
            } while (DispatchingThreads == SCE_KERNEL_DISPATCHTHREAD_STATE.DISABLED);

            Current = null;
        }

        private void ExecuteQueuedInterrupts()
        {
#if !DISABLE_CALLBACKS
            if (Threads.Count > 0)
            {
                HleInterruptManager.ExecuteQueued(Threads.First().CpuThreadState);
            }
#endif
        }

        private HleThread FindCallbackHandlerWithHighestPriority()
        {
            return Threads.Where(Thread => Thread.IsWaitingAndHandlingCallbacks)
                .OrderByDescending(Thread => Thread.PriorityValue).FirstOrDefault();
        }

        private void ExecuteQueuedCallbacks()
        {
#if !DISABLE_CALLBACKS
            bool HasScheduledCallbacks = HleCallbackManager.HasScheduledCallbacks;
            bool HasQueuedFunctions = HleInterop.HasQueuedFunctions;

            if (HasScheduledCallbacks || HasQueuedFunctions)
            {
                var Thread = FindCallbackHandlerWithHighestPriority();
                if (Thread != null)
                {
                    if (HasScheduledCallbacks) HleCallbackManager.ExecuteQueued(Thread.CpuThreadState, true);
                    if (HasQueuedFunctions) HleInterop.ExecuteAllQueuedFunctionsNow();
                }
            }
#endif
        }

        /// <summary>
        /// Inside the active thread, yields the execution, and terminates the step.
        /// </summary>
        public void Yield()
        {
            if (Current != null)
            {
                Current.CpuThreadState.Yield();
            }
        }

        public void UpdatedThread(HleThread HleThread)
        {
            PreemptiveScheduler.Update(HleThread);
        }

        private HleThread CalculateNext()
        {
            PreemptiveScheduler.Next();
            return PreemptiveScheduler.Current;
        }

        internal Queue<Action> ChangeStatusActions = new Queue<Action>();

        public void StepNext(Action DoBeforeSelectingNext)
        {
            //HleInterruptManager.EnableDisable(() => {
            //};

            // Select the thread with the lowest PriorityValue
            var NextThread = CalculateNext();

#if DEBUG_THREADS
			if (NextThread != null)
			{
				Console.Error.WriteLine("+++++++++++++++++++++++++++++++++");
				foreach (var Thread in Threads)
				{
					Console.Error.WriteLine(Thread);
				}
				Console.Error.WriteLine("NextThread: {0} : {1}", NextThread.Id, NextThread.PriorityValue);
			}
#endif

            //Console.WriteLine("{0} -> {1}", String.Join(",", PreemptiveScheduler.GetThreadsInQueue().Select(Item => Item.Name)), (NextThread != null) ? NextThread.Name : "-");

            ExecuteQueuedInterrupts();
            ExecuteQueuedCallbacks();

            lock (ChangeStatusActions)
            {
                if (ChangeStatusActions.Count > 0)
                {
                    while (ChangeStatusActions.Count > 0)
                    {
                        var Action = ChangeStatusActions.Dequeue();
                        if (Action != null) Action();
                    }

                    NextThread = CalculateNext();
                    //if (NextThread == null)
                    //{
                    //	StepNext(DoBeforeSelectingNext);
                    //	return;
                    //}
                }
            }

            // No thread found.
            if (NextThread == null)
            {
                if (DisplayConfig.VerticalSynchronization)
                {
                    Thread.Sleep(1);
                }
                if (Threads.Count == 0)
                {
                    Thread.Sleep(5);
                }
                return;
            }

            // Run that thread
            this.Current = NextThread;
            var CurrentCurrent = Current;
            {
                // Ready -> Running
                CurrentCurrent.SetStatus(HleThread.Status.Running);

                try
                {
                    if (HleConfig.DebugThreadSwitching)
                    {
                        ConsoleUtils.SaveRestoreConsoleState(() =>
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Execute: {0} : PC: 0x{1:X}", CurrentCurrent,
                                CurrentCurrent.CpuThreadState.Pc);
                        });
                    }

                    if (DoBeforeSelectingNext != null) DoBeforeSelectingNext();

                    ExecuteCurrent(CurrentCurrent);
                }
                finally
                {
                    // Running -> Ready
                    if (CurrentCurrent.HasAllStatus(HleThread.Status.Running))
                    {
                        CurrentCurrent.SetStatus(HleThread.Status.Ready);
                    }
                }
            }
            this.Current = null;
        }

        public HleThread GetThreadById(int Id, bool AllowSelf = true)
        {
            //Debug.WriteLine(Threads.Count);
            if (Id == 0)
            {
                if (!AllowSelf) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_THREAD));
                return Current;
            }
            HleThread HleThread = null;
            ThreadsById.TryGetValue(Id, out HleThread);
            if (HleThread == null) throw(new SceKernelException(SceKernelErrors.ERROR_KERNEL_NOT_FOUND_THREAD));
            return HleThread;
        }

        public HleThread Create()
        {
            var HlePspThread = new HleThread(InjectContext, new CpuThreadState(Processor));
            HlePspThread.Id = LastId++;
            HlePspThread.Name = "Thread-" + HlePspThread.Id;
            HlePspThread.SetStatus(HleThread.Status.Stopped);

            //Console.Error.WriteLine("Created: {0}", HlePspThread);
            PreemptiveScheduler.Update(HlePspThread);
            Threads.Add(HlePspThread);
            ThreadsById[HlePspThread.Id] = HlePspThread;

            return HlePspThread;
        }

        public void ExitThread(HleThread HleThread, int ExitStatus)
        {
#if DEBUG_THREADS
			ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
			{
				Console.Error.WriteLine("TerminateThread: {0}", HleThread);
			});
#endif
            HleThread.Info.ExitStatus = ExitStatus;
            HleThread.SetStatus(HleThread.Status.Killed);
            HleThread.Terminate();
            if (HleThread == Current)
            {
                HleThread.CpuThreadState.Yield();
            }
        }

        public void TerminateThread(HleThread HleThread)
        {
            ExitThread(HleThread, -1);
        }

        public unsafe void DeleteThread(HleThread HleThread)
        {
#if DEBUG_THREADS
			ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
			{
				Console.Error.WriteLine("DeleteThread: {0}", HleThread);
				Console.Error.WriteLine("{0}", Environment.StackTrace);
			});
#endif
            HleThread.Stack.DeallocateFromParent();
            Threads.Remove(HleThread);
            ThreadsById.Remove(HleThread.Id);
            PreemptiveScheduler.Remove(HleThread);
        }


        public void ScheduleNext()
        {
        }

        public unsafe void SuspendThread(HleThread Thread)
        {
            Thread.SetStatus(HleThread.Status.Suspend);
            if (Thread == this.Current)
            {
                this.Yield();
            }
        }

        public unsafe void ResumeThread(HleThread Thread)
        {
            Thread.SetStatus(HleThread.Status.Ready);
        }
    }
}
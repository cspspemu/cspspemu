#define DISABLE_CALLBACKS

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
using CSPspEmu.Hle.Vfs.MemoryStick;

namespace CSPspEmu.Hle.Managers
{
	public partial class HleThreadManager : IInjectInitialize, ICpuConnector, IGpuConnector
	{
		static Logger Logger = Logger.GetLogger("HleThreadManager");

		[Inject]
		internal CpuProcessor Processor;

		[Inject]
		DisplayConfig DisplayConfig;

		[Inject]
		HleConfig HleConfig;

		[Inject]
		private HleCallbackManager HleCallbackManager;

		[Inject]
		private HleInterruptManager HleInterruptManager;

		[Inject]
		private HleInterop HleInterop;

		[Inject]
		private InjectContext InjectContext;

		public List<HleThread> Threads = new List<HleThread>();

		public Dictionary<int, HleThread> ThreadsById = new Dictionary<int, HleThread>();

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

		void IGpuConnector.Signal(uint Signal, GpuDisplayList.GuBehavior Behavior)
		{
			//Console.WriteLine("IGpuConnector.Signal");
		}

		void IGpuConnector.Finish(uint Arg)
		{
			//Console.WriteLine("IGpuConnector.Finish");
		}

		void IInjectInitialize.Initialize()
		{
			Processor.DebugCurrentThreadEvent += DebugCurrentThread;
		}

		/// <summary>
		/// 
		/// </summary>
		public PreemptiveScheduler<HleThread> PreemptiveScheduler = new PreemptiveScheduler<HleThread>(NewItemsFirst: true, ThrowException: false);

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
			}
			while (DispatchingThreads == SCE_KERNEL_DISPATCHTHREAD_STATE.DISABLED);

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
			return Threads.Where(Thread => Thread.IsWaitingAndHandlingCallbacks).OrderByDescending(Thread => Thread.PriorityValue).FirstOrDefault();
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

		public void StepNext(Action DoBeforeSelectingNext)
		{
			//HleInterruptManager.EnableDisable(() => {
			//};

			// Select the thread with the lowest PriorityValue
			var NextThread = CalculateNext();
			//Console.WriteLine("NextThread: {0} : {1}", NextThread.Id, NextThread.PriorityValue);

			//Console.WriteLine("{0} -> {1}", String.Join(",", PreemptiveScheduler.GetThreadsInQueue().Select(Item => Item.Name)), (NextThread != null) ? NextThread.Name : "-");

			ExecuteQueuedInterrupts();
			ExecuteQueuedCallbacks();

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
							Console.WriteLine("Execute: {0} : PC: 0x{1:X}", CurrentCurrent, CurrentCurrent.CpuThreadState.PC);
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

			PreemptiveScheduler.Update(HlePspThread);
			Threads.Add(HlePspThread);
			ThreadsById[HlePspThread.Id] = HlePspThread;

			return HlePspThread;
		}

		public void Remove(HleThread HleThread)
		{
			ExitThread(HleThread);
		}

		public void ExitThread(HleThread HleThread)
		{
			Threads.Remove(HleThread);
			ThreadsById.Remove(HleThread.Id);
			PreemptiveScheduler.Remove(HleThread);
		}

		public unsafe void DeleteThread(HleThread Thread)
		{
			Thread.SetStatus(HleThread.Status.Killed);
			Thread.Stack.DeallocateFromParent();
			ExitThread(Thread);
		}

	}
}

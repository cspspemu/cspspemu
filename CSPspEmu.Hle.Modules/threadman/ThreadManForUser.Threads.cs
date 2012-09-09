//#define TEST_STOP_THREADS_INSTEAD_OF_KILLING
#define USE_RIGHT_PRIORITY_VALUE

using System;
using CSharpUtils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Modules.threadman
{
	public unsafe partial class ThreadManForUser
	{
		[Inject]
		public HleThreadManager ThreadManager;

		[Inject]
		public HleMemoryManager MemoryManager;

		private HleThread GetThreadById(int ThreadId)
		{
			HleThread HleThread = ThreadManager.GetThreadById(ThreadId);
			if (HleThread == null) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_NOT_FOUND_THREAD));
			return HleThread;
		}

		/// <summary>
		/// Create a thread
		/// </summary>
		/// <example>
		/// SceUID thid;
		/// thid = sceKernelCreateThread("my_thread", threadFunc, 0x18, 0x10000, 0, NULL);
		/// </example>
		/// <param name="Name">An arbitrary thread name.</param>
		/// <param name="EntryPoint">The thread function to run when started.</param>
		/// <param name="InitPriority">The initial priority of the thread. Less if higher priority.</param>
		/// <param name="StackSize">The size of the initial stack.</param>
		/// <param name="Attribute">The thread attributes, zero or more of ::PspThreadAttributes.</param>
		/// <param name="Option">Additional options specified by ::SceKernelThreadOptParam.</param>
		/// <returns>UID of the created thread, or an error code.</returns>
		[HlePspFunction(NID = 0x446D8DE6, FirmwareVersion = 150)]
		public uint sceKernelCreateThread(CpuThreadState CpuThreadState, string Name, uint /*SceKernelThreadEntry*/ EntryPoint, int InitPriority, int StackSize, PspThreadAttributes Attribute, SceKernelThreadOptParam* Option)
		{
			// 512 byte min. (required for interrupts)
			StackSize = Math.Max(StackSize, 0x200);
			// Aligned to 256 bytes.
			StackSize = (int)MathUtils.NextAligned(StackSize, 0x100);

			var Thread = ThreadManager.Create();
			Thread.Name = Name;
			Thread.Info.PriorityCurrent = InitPriority;
			Thread.Info.PriorityInitially = InitPriority;
			
#if USE_RIGHT_PRIORITY_VALUE
			Thread.PriorityValue = InitPriority;
#endif

			Thread.Attribute = Attribute;
			Thread.GP = CpuThreadState.GP;
			Thread.Info.EntryPoint = (SceKernelThreadEntry)EntryPoint;
			
			//var ThreadStackPartition = MemoryManager.GetPartition(HleMemoryManager.Partitions.User);
			//var ThreadStackPartition = MemoryManager.GetPartition(HleMemoryManager.Partitions.UserStacks);
			var ThreadStackPartition = MemoryManager.GetPartition(HleMemoryManager.Partitions.Kernel0);

			Thread.Stack = ThreadStackPartition.Allocate(
				StackSize,
				MemoryPartition.Anchor.High,
				Alignment: 0x100,
				Name: "<Stack> : " + Name
			);

			if (!Thread.Attribute.HasFlag(PspThreadAttributes.NoFillStack))
			{
				MemoryManager.Memory.WriteRepeated1(0xFF, Thread.Stack.Low, Thread.Stack.Size - 0x100);
				//Console.Error.WriteLine("-------------------------------------------------");
				//Console.Error.WriteLine("'{0}', '{1}'", StackSize, Thread.Stack.Size);
				//Console.Error.WriteLine("-------------------------------------------------");
			}
			Thread.Info.StackPointer = Thread.Stack.High;
			Thread.Info.StackSize = StackSize;

			// Used K0 from parent thread.
			// @FAKE. K0 should be preserved between thread calls. Though probably not modified by user modules.
			if (ThreadManager.Current != null)
			{
				Thread.CpuThreadState.CopyRegistersFrom(ThreadManager.Current.CpuThreadState);
			}

			Thread.CpuThreadState.PC = (uint)EntryPoint;
			Thread.CpuThreadState.RA = (uint)HleEmulatorSpecialAddresses.CODE_PTR_EXIT_THREAD;
			Thread.SetStatus(HleThread.Status.Stopped);
			//Thread.CpuThreadState.RA = (uint)0;


			uint StackLow = Thread.Stack.Low;
			uint SP = Thread.Stack.High - 0x200;
			uint K0 = Thread.Stack.High - 0x100;

			CpuThreadState.CpuProcessor.Memory.WriteStruct(StackLow, Thread.Id);
			CpuThreadState.CpuProcessor.Memory.WriteRepeated1(0x00, K0, 0x100);
			CpuThreadState.CpuProcessor.Memory.WriteStruct(K0 + 0xC0, StackLow);
			CpuThreadState.CpuProcessor.Memory.WriteStruct(K0 + 0xCA, Thread.Id);
			CpuThreadState.CpuProcessor.Memory.WriteStruct(K0 + 0xF8, 0xFFFFFFFF);
			CpuThreadState.CpuProcessor.Memory.WriteStruct(K0 + 0xFC, 0xFFFFFFFF);

			Thread.CpuThreadState.SP = SP;
			//ThreadToStart.CpuThreadState.FP = 0xDEADBEEF;
			Thread.CpuThreadState.K0 = K0;

			//Console.WriteLine("STACK: {0:X}", Thread.CpuThreadState.SP);

			return (uint)Thread.Id;
		}

		public void _sceKernelStartThread(CpuThreadState CpuThreadState, int ThreadId, int UserDataLength, uint UserDataPointer)
		{
			var ThreadToStart = GetThreadById((int)ThreadId);
			//Console.WriteLine("LEN: {0:X}", ArgumentsLength);
			//Console.WriteLine("PTR: {0:X}", ArgumentsPointer);

			var CopiedDataAddress = (uint)((ThreadToStart.Stack.High - 0x100) - ((UserDataLength + 0xF) & ~0xF));

			if (UserDataPointer == 0)
			{
				ThreadToStart.CpuThreadState.GPR[4] = 0;
				ThreadToStart.CpuThreadState.GPR[5] = 0;
			}
			else
			{
				CpuThreadState.CpuProcessor.Memory.Copy(UserDataPointer, CopiedDataAddress, UserDataLength);
				ThreadToStart.CpuThreadState.GPR[4] = (int)UserDataLength;
				ThreadToStart.CpuThreadState.GPR[5] = (int)CopiedDataAddress;
			}

			ThreadToStart.CpuThreadState.GP = (uint)CpuThreadState.GP;
			ThreadToStart.CpuThreadState.SP = (uint)(CopiedDataAddress - 0x40);

			ThreadToStart.CpuThreadState.CallerModule = CpuThreadState.CallerModule;

			ThreadToStart.SetStatus(HleThread.Status.Ready);
		}

		/// <summary>
		/// Start a created thread
		/// </summary>
		/// <param name="ThreadId">Thread id from sceKernelCreateThread</param>
		/// <param name="UserDataLength">Length of the data pointed to by argp, in bytes</param>
		/// <param name="UserDataPointer">Pointer to the arguments.</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF475845D, FirmwareVersion = 150)]
		public int sceKernelStartThread(CpuThreadState CpuThreadState, int ThreadId, int UserDataLength, uint UserDataPointer)
		{
			var ThreadToStart = GetThreadById((int)ThreadId);
			_sceKernelStartThread(CpuThreadState, ThreadId, UserDataLength, UserDataPointer);
			// Schedule new thread?
			CpuThreadState.Yield();
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <param name="HandleCallbacks"></param>
		/// <returns></returns>
		private int _sceKernelSleepThreadCB(CpuThreadState CpuThreadState, bool HandleCallbacks)
		{
			var ThreadToSleep = ThreadManager.Current;
			ThreadToSleep.ChangeWakeUpCount(-1, null, HandleCallbacks: HandleCallbacks);
			return 0;
		}

		/// <summary>
		/// Sleep thread until sceKernelWakeUp is called.
		/// </summary>
		/// <returns>Less than zero on error</returns>
		[HlePspFunction(NID = 0x9ACE131E, FirmwareVersion = 150)]
		public int sceKernelSleepThread(CpuThreadState CpuThreadState)
		{

			return _sceKernelSleepThreadCB(CpuThreadState, HandleCallbacks: false);
		}

		/// <summary>
		/// Sleep thread but service any callbacks as necessary
		/// </summary>
		/// <example>
		///		// Once all callbacks have been setup call this function
		///		sceKernelSleepThreadCB();
		/// </example>
		/// <returns></returns>
		[HlePspFunction(NID = 0x82826F70, FirmwareVersion = 150)]
		public int sceKernelSleepThreadCB(CpuThreadState CpuThreadState)
		{
			return _sceKernelSleepThreadCB(CpuThreadState, HandleCallbacks: true);
		}

		/// <summary>
		/// Get the current thread Id
		/// </summary>
		/// <returns>The thread id of the calling thread.</returns>
		[HlePspFunction(NID = 0x293B45B8, FirmwareVersion = 150)]
		public int sceKernelGetThreadId()
		{
			if (ThreadManager.Current == null) return 0;
			return ThreadManager.Current.Id;
		}

		/// <summary>
		/// Get the status information for the specified thread.
		/// </summary>
		/// <example>
		///		SceKernelThreadInfo status;
		///		status.size = sizeof(SceKernelThreadInfo);
		///		if (sceKernelReferThreadStatus(thid, &status) == 0) { Do something... }
		/// </example>
		/// <param name="ThreadId">Id of the thread to get status</param>
		/// <param name="SceKernelThreadInfo">
		///		Pointer to the info structure to receive the data.
		///		Note: The structures size field should be set to
		///		sizeof(SceKernelThreadInfo) before calling this function.
		///	</param>
		/// <returns>0 if successful, otherwise the error code.</returns>
		[HlePspFunction(NID = 0x17C1684E, FirmwareVersion = 150)]
		public int sceKernelReferThreadStatus(int ThreadId, out SceKernelThreadInfo SceKernelThreadInfo)
		{
			var Thread = GetThreadById(ThreadId);
			SceKernelThreadInfo = Thread.Info;
			return 0;
		}

		/// <summary>
		/// Wake a thread previously put into the sleep state.
		/// </summary>
		/// <remarks>
		/// This function increments a wakeUp count and sceKernelSleep(CB) decrements it.
		/// So when calling sceKernelSleep(CB) if this function have been executed before one or more times,
		/// the thread won't sleep until Sleeps is executed as many times as sceKernelWakeupThread.
		/// 
		/// ?? This waits until the thread has been awaken? TO CONFIRM.
		/// </remarks>
		/// <param name="thid">UID of the thread to wake.</param>
		/// <returns>Success if greater or equal 0, an error if less than 0.</returns>
		[HlePspFunction(NID = 0xD59EAD2F, FirmwareVersion = 150)]
		public int sceKernelWakeupThread(int ThreadId)
		{
			try
			{
				var ThreadCurrent = ThreadManager.Current;
				var ThreadToWakeUp = ThreadManager.GetThreadById(ThreadId);
				ThreadToWakeUp.ChangeWakeUpCount(+1, ThreadCurrent);
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
			return 0;
		}

		/// <summary>
		/// Cancel a thread that was to be woken with ::sceKernelWakeupThread.
		/// </summary>
		/// <param name="ThreadId">UID of the thread to cancel.</param>
		/// <returns>Success if greater or equal than 0, an error if less  than 0.</returns>
		[HlePspFunction(NID = 0xFCCFAD26, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelCancelWakeupThread(int ThreadId)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ThreadId"></param>
		/// <param name="Timeout"></param>
		/// <returns></returns>
		private int _sceKernelWaitThreadEndCB(int ThreadId, uint* Timeout, bool HandleCallbacks)
		{
			var ThreadToWaitEnd = GetThreadById(ThreadId);

			if (ThreadToWaitEnd.HasAnyStatus(HleThread.Status.Stopped))
			{
				return 0;
			}

			if (ThreadToWaitEnd.HasAnyStatus(HleThread.Status.Killed))
			{
				return 0;
			}

			bool TimedOut = false;

			ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.None, "sceKernelWaitThreadEnd", ThreadToWaitEnd, WakeUpCallback =>
			{
				if (Timeout != null)
				{
					PspRtc.RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(*Timeout), () =>
					{
						TimedOut = true;
						WakeUpCallback();
					});
				}

				Console.WriteLine("Wait End!");
				ThreadToWaitEnd.End += () =>
				{
					Console.WriteLine("Ended!");
					//throw(new Exception("aaaaaaaaaaaa"));
					WakeUpCallback();
				};
			}, HandleCallbacks: HandleCallbacks);

			if (TimedOut)
			{
				return (int)SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Wait until a thread has ended.
		/// </summary>
		/// <param name="ThreadId">Id of the thread to wait for.</param>
		/// <param name="Timeout">Timeout in microseconds (assumed).</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x278C0DF5, FirmwareVersion = 150)]
		public int sceKernelWaitThreadEnd(int ThreadId, uint* Timeout)
		{
			return _sceKernelWaitThreadEndCB(ThreadId, Timeout, HandleCallbacks: false);
		}

		/// <summary>
		/// Wait until a thread has ended and handle callbacks if necessary.
		/// </summary>
		/// <param name="ThreadId">Id of the thread to wait for.</param>
		/// <param name="Timeout">Timeout in microseconds (assumed).</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x840E8133, FirmwareVersion = 150)]
		public int sceKernelWaitThreadEndCB(int ThreadId, uint* Timeout)
		{
			return _sceKernelWaitThreadEndCB(ThreadId, Timeout, HandleCallbacks: true);
		}

		private int _sceKernelDelayThreadCB(uint DelayInMicroseconds, bool HandleCallbacks)
		{
			var CurrentThread = ThreadManager.Current;

#if true
			if (DelayInMicroseconds < 1000)
			{
				if (HandleCallbacks)
				{
					sceKernelCheckCallback(CurrentThread.CpuThreadState);
				}
				//ThreadManager.ScheduleNext();
				CurrentThread.CpuThreadState.Yield();
			}
			else
#endif
			{
				CurrentThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Timer, "sceKernelDelayThread", null, WakeUpCallback =>
				{
					PspRtc.RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(DelayInMicroseconds), () =>
					{
						WakeUpCallback();
					});
				}, HandleCallbacks: HandleCallbacks);
			}

			return 0;
		}

		/// <summary>
		/// Delay the current thread by a specified number of microseconds
		/// </summary>
		/// <param name="DelayInMicroseconds">Delay in microseconds.</param>
		/// <example>
		///		sceKernelDelayThread(1000000); // Delay for a second
		/// </example>
		/// <returns></returns>
		[HlePspFunction(NID = 0xCEADEB47, FirmwareVersion = 150)]
		public int sceKernelDelayThread(uint DelayInMicroseconds)
		{
			return _sceKernelDelayThreadCB(DelayInMicroseconds, HandleCallbacks: false);
		}

		/// <summary>
		/// Delay the current thread by a specified number of microseconds and handle any callbacks.
		/// </summary>
		/// <param name="DelayInMicroseconds">Delay in microseconds.</param>
		/// <example>
		///		sceKernelDelayThread(1000000); // Delay for a second
		/// </example>
		/// <returns></returns>
		[HlePspFunction(NID = 0x68DA9E36, FirmwareVersion = 150)]
		public int sceKernelDelayThreadCB(uint DelayInMicroseconds)
		{
			return _sceKernelDelayThreadCB(DelayInMicroseconds, HandleCallbacks: true);
		}

		/// <summary>
		/// Modify the attributes of the current thread.
		/// </summary>
		/// <param name="unknown">Set to 0.</param>
		/// <param name="attr">The thread attributes to modify.  One of ::PspThreadAttributes.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xEA748E31, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelChangeCurrentThreadAttr(PspThreadAttributes RemoveAttributes, PspThreadAttributes AddAttributes)
		{
			ThreadManager.Current.Attribute &= ~RemoveAttributes;
			ThreadManager.Current.Attribute |= AddAttributes;
			return 0;
		}

		/// <summary>
		/// Change the threads current priority.
		/// </summary>
		/// <param name="ThreadId">The ID of the thread (from sceKernelCreateThread or sceKernelGetThreadId)</param>
		/// <param name="Priority">The new priority (the lower the number the higher the priority)</param>
		/// <example>
		///		int thid = sceKernelGetThreadId();
		///		// Change priority of current thread to 16
		///		sceKernelChangeThreadPriority(thid, 16);
		/// </example>
		/// <returns>0 if successful, otherwise the error code.</returns>
		[HlePspFunction(NID = 0x71BC9871, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelChangeThreadPriority(CpuThreadState CpuThreadState, int ThreadId, int Priority)
		{
			var Thread = GetThreadById(ThreadId);
			Thread.PriorityValue = Priority;
			CpuThreadState.Yield();
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Get the exit status of a thread.
		/// </summary>
		/// <param name="ThreadId">The UID of the thread to check.</param>
		/// <returns>The exit status</returns>
		[HlePspFunction(NID = 0x3B183E26, FirmwareVersion = 150)]
		public int sceKernelGetThreadExitStatus(int ThreadId)
		{
			var Thread = ThreadManager.GetThreadById(ThreadId);
			return Thread.Info.ExitStatus;
		}

		/// <summary>
		/// Exit a thread and delete itself.
		/// </summary>
		/// <param name="ExitStatus">Exit status</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x809CE29B, FirmwareVersion = 150)]
		public int sceKernelExitDeleteThread(int ExitStatus)
		{
			var CurrentThreadId = ThreadManager.Current.Id;
			int ResultExit = sceKernelExitThread(ExitStatus);
			int ResultDelete = sceKernelDeleteThread(CurrentThreadId);
			return ResultDelete;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x11111111, FirmwareVersion = 150)]
		public int _hle_sceKernelExitDeleteThread(CpuThreadState CpuThreadState)
		{
			//CpuThreadState.DumpRegisters(Console.Error);

			//Console.Error.WriteLine(CpuThreadState.GPR[2]);
			return sceKernelExitDeleteThread(CpuThreadState.GPR[2]);
		}

		/// <summary>
		/// Exit a thread
		/// </summary>
		/// <param name="ExitStatus">Exit status.</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xAA73C935, FirmwareVersion = 150)]
		public int sceKernelExitThread(int ExitStatus)
		{
			var Thread = ThreadManager.Current;
			
			//Console.Error.WriteLine(ExitStatus);
			
			Thread.Info.ExitStatus = ExitStatus;

#if TEST_STOP_THREADS_INSTEAD_OF_KILLING
			Thread.SetStatus(HleThread.Status.Stopped);
#else
			Thread.SetStatus(HleThread.Status.Killed);
#endif

			Thread.Exit();

			//ThreadManager.ExitThread(Thread);

			Thread.CpuThreadState.Yield();


			return 0;
		}

		/// <summary>
		/// Delete a thread
		/// </summary>
		/// <param name="ThreadId">UID of the thread to be deleted.</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x9FA03CD3, FirmwareVersion = 150)]
		public int sceKernelDeleteThread(int ThreadId)
		{
			var Thread = ThreadManager.GetThreadById(ThreadId);
			ThreadManager.DeleteThread(Thread);
			ThreadManager.ExitThread(Thread);
			return 0;
			//return _sceKernelExitDeleteThread(-1, GetThreadById(ThreadId));
		}

		/// <summary>
		/// Terminate and delete a thread.
		/// </summary>
		/// <param name="ThreadId">UID of the thread to terminate and delete.</param>
		/// <returns>Success if greater or equal to 0, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x383F7BCC, FirmwareVersion = 150)]
		public int sceKernelTerminateDeleteThread(int ThreadId)
		{
			return sceKernelDeleteThread(ThreadId);
			//throw(new NotImplementedException());

		}

		/// <summary>
		/// Get the current priority of the thread you are in.
		/// </summary>
		/// <returns>The current thread priority</returns>
		[HlePspFunction(NID = 0x94AA61EE, FirmwareVersion = 150)]
		public int sceKernelGetThreadCurrentPriority()
		{
			return ThreadManager.Current.Info.PriorityCurrent;
		}

		/// <summary>
		/// Get the free stack size for a thread.
		/// </summary>
		/// <param name="ThreadId">
		///		The thread ID. Seem to take current thread if set to 0.
		/// </param>
		/// <returns>The free size.</returns>
		[HlePspFunction(NID = 0x52089CA1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelGetThreadStackFreeSize(int ThreadId)
		{
			var HleThread = ThreadManager.GetThreadById(ThreadId, AllowSelf: true);
			var SpHigh = (uint)HleThread.Info.StackPointer;
			var SpLow = (uint)HleThread.Info.StackPointer - HleThread.Info.StackSize;
			var SpCurrent = (uint)HleThread.CpuThreadState.SP;
			Console.Error.WriteLine("{0:X} - {1:X} - {2:X}", SpLow, SpCurrent, SpHigh);
			return (int)(SpCurrent - SpLow);
			//throw(new NotImplementedException());
			//return SpHigh - SpCurrent;
		}

		/// <summary>
		/// Rotate thread ready queue at a set priority
		/// </summary>
		/// <param name="priority">The priority of the queue</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x912354A7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelRotateThreadReadyQueue(CpuThreadState CpuThreadState, int priority)
		{
			// @TODO!
			//throw(new NotImplementedException());
			CpuThreadState.Yield();
			return 0;
		}

		/// <summary>
		/// Threadman types for ::sceKernelGetThreadmanIdList
		/// </summary>
		public enum SceKernelIdListType
		{
			SCE_KERNEL_TMID_Thread = 1,
			SCE_KERNEL_TMID_Semaphore = 2,
			SCE_KERNEL_TMID_EventFlag = 3,
			SCE_KERNEL_TMID_Mbox = 4,
			SCE_KERNEL_TMID_Vpl = 5,
			SCE_KERNEL_TMID_Fpl = 6,
			SCE_KERNEL_TMID_Mpipe = 7,
			SCE_KERNEL_TMID_Callback = 8,
			SCE_KERNEL_TMID_ThreadEventHandler = 9,
			SCE_KERNEL_TMID_Alarm = 10,
			SCE_KERNEL_TMID_VTimer = 11,
			SCE_KERNEL_TMID_SleepThread = 64,
			SCE_KERNEL_TMID_DelayThread = 65,
			SCE_KERNEL_TMID_SuspendThread = 66,
			SCE_KERNEL_TMID_DormantThread = 67,
		};

		/// <summary>
		/// Get a list of UIDs from threadman. Allows you to enumerate 
		/// resources such as threads or semaphores.
		/// </summary>
		/// <param name="Type">The type of resource to list, one of ::SceKernelIdListType.</param>
		/// <param name="List">A pointer to a buffer to store the list.</param>
		/// <param name="ListMax">The size of the buffer in SceUID units.</param>
		/// <param name="OutListCount">Pointer to an integer in which to return the number of ids in the list.</param>
		/// <returns>Less than 0 on error. Either 0 or the same as idcount on success.</returns>
		[HlePspFunction(NID = 0x94416130, FirmwareVersion = 150)]
		public int sceKernelGetThreadmanIdList(SceKernelIdListType Type, int* List, int ListMax, int* OutListCount)
		{
			int n = 0;
			switch (Type)
			{
				case SceKernelIdListType.SCE_KERNEL_TMID_Thread:
					foreach (var Thread in ThreadManager.Threads) List[n++] = Thread.Id;
					break;
				default:
					throw (new NotImplementedException("sceKernelGetThreadmanIdList: " + Type));
			}
			if (OutListCount != null) *OutListCount = n;
			return 0;
		}

		/// <summary>
		/// Suspend a thread.
		/// </summary>
		/// <param name="ThreadId">UID of the thread to resume.</param>
		/// <returns>
		///		Success if greater or equalthan 0,
		///		an error if less than 0.
		/// </returns>
		[HlePspFunction(NID = 0x9944F31F, FirmwareVersion = 150)]
		[PspUntested]
		public int sceKernelSuspendThread(int ThreadId)
		{
			var Thread = GetThreadById(ThreadId);
			Thread.SetStatus(HleThread.Status.Suspend);
			if (Thread == ThreadManager.Current)
			{
				ThreadManager.Yield();
			}
			return 0;
		}

		/// <summary>
		/// Resume a thread previously put into a suspended state with ::sceKernelSuspendThread.
		/// </summary>
		/// <param name="ThreadId">UID of the thread to resume.</param>
		/// <returns>Success if greater or equal to 0, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x75156E8F, FirmwareVersion = 150)]
		[PspUntested]
		public int sceKernelResumeThread(int ThreadId)
		{
			var Thread = GetThreadById(ThreadId);
			Thread.SetStatus(HleThread.Status.Ready);
			return 0;
		}

		/// <summary>
		/// Resume the dispatch thread
		/// </summary>
		/// <param name="State">
		/// The state of the dispatch thread 
		/// (from ::sceKernelSuspendDispatchThread)
		/// </param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x27E22EC2, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		[PspUntested]
		public int sceKernelResumeDispatchThread(HleThreadManager.SCE_KERNEL_DISPATCHTHREAD_STATE State)
		{
			ThreadManager.DispatchingThreads = State;
			return 0;
		}

		/// <summary>
		/// Suspend the dispatch thread
		/// </summary>
		/// <returns>The current state of the dispatch thread, less than 0 on error</returns>
		[HlePspFunction(NID = 0x3AD58B8C, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		[PspUntested]
		public HleThreadManager.SCE_KERNEL_DISPATCHTHREAD_STATE sceKernelSuspendDispatchThread()
		{
			try
			{
				return ThreadManager.DispatchingThreads;
			}
			finally
			{
				ThreadManager.DispatchingThreads = HleThreadManager.SCE_KERNEL_DISPATCHTHREAD_STATE.DISABLED;
			}
		}

		/// <summary>
		/// Terminate a thread.
		/// </summary>
		/// <param name="ThreadId">UID of the thread to terminate.</param>
		/// <returns>Success if greater than 0, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x616403BA, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelTerminateThread(int ThreadId)
		{
			var Thread = GetThreadById(ThreadId);

			//Console.Error.WriteLine(ExitStatus);

			Thread.Info.ExitStatus = -1;
			Thread.SetStatus(HleThread.Status.Killed);
			Thread.Exit();
			return 0;
		}

		/// <summary>
		/// Release a thread in the wait state.
		/// </summary>
		/// <param name="ThreadId">The UID of the thread.</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x2C34E053, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelReleaseWaitThread(int ThreadId)
		{
			var Thread = GetThreadById(ThreadId);
			var CurrentThread = ThreadManager.Current;
			if (Thread == CurrentThread) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_THREAD));
			if (!Thread.HasAnyStatus(HleThread.Status.Waiting)) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_THREAD_IS_NOT_WAIT));
			Thread.WakeUp();
			return 0;
		}

		public struct SceKernelSystemStatus
		{
			/// <summary>
			/// Size of the structure (should be set prior to the call). 
			/// </summary>
			public uint Size;
				
			/// <summary>
			/// The status ? 
			/// </summary>
			public uint Status;
				
			/// <summary>
			/// SceKernelSysClock : idleClocks (The number of cpu clocks in the idle thread. )
			/// </summary>
			public uint IdleClocks;

			/// <summary>
			/// Number of times we resumed from idle. 
			/// </summary>
			public uint ComesOutOfIdleCount;
				
			/// <summary>
			/// Number of thread context switches. 
			/// </summary>
			public uint ThreadSwitchCount;
				
			/// <summary>
			/// Number of vfpu switches ? 
			/// </summary>
			public uint VfpuSwitchCount;
		}

		/// <summary>
		/// Get the current system status.
		/// </summary>
		/// <param name="?">Pointer to a ::SceKernelSystemStatus structure.</param>
		/// <param name="?"></param>
		/// </summary>
		[HlePspFunction(NID = 0x627E6F3A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelReferSystemStatus(ref SceKernelSystemStatus SceKernelSystemStatus)
		{
			SceKernelSystemStatus.Status = 0;

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x8218B4DD, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelReferGlobalProfiler()
		{
			// Can be safely ignored. Only valid in debug mode on a real PSP.
			return 0;
		}

		/*
		public int _sceKernelExitDeleteThread(int Status, HleThread Thread)
		{
			if (Thread != null)
			{
				Thread.Finalize();
				ThreadManager.Exit(Thread);
			}
			return 0;
		}
		*/
	}
}

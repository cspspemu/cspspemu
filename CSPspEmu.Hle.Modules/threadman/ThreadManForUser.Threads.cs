using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSharpUtils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		private HleThread GetThreadById(int ThreadId)
		{
			HleThread HleThread = HleState.ThreadManager.GetThreadById(ThreadId);
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
			var Thread = HleState.ThreadManager.Create();
			Thread.Name = Name;
			Thread.Info.PriorityCurrent = InitPriority;
			Thread.Info.PriorityInitially = InitPriority;
			Thread.Attribute = Attribute;
			Thread.GP = CpuThreadState.GP;
			Thread.Info.EntryPoint = (SceKernelThreadEntry)EntryPoint;
			Thread.Stack = HleState.MemoryManager.GetPartition(HleMemoryManager.Partitions.User).Allocate(StackSize, MemoryPartition.Anchor.High, Alignment: 0x100);
			if (!Thread.Attribute.HasFlag(PspThreadAttributes.NoFillStack))
			{
				HleState.MemoryManager.Memory.WriteRepeated1(0xFF, Thread.Stack.Low, Thread.Stack.Size - 0x100);
				//Console.Error.WriteLine("-------------------------------------------------");
				//Console.Error.WriteLine("'{0}', '{1}'", StackSize, Thread.Stack.Size);
				//Console.Error.WriteLine("-------------------------------------------------");
			}
			Thread.Info.StackPointer = Thread.Stack.High;
			Thread.Info.StackSize = StackSize;

			// Used K0 from parent thread.
			// @FAKE. K0 should be preserved between thread calls. Though probably not modified by user modules.
			Thread.CpuThreadState.CopyRegistersFrom(HleState.ThreadManager.Current.CpuThreadState);

			Thread.CpuThreadState.PC = (uint)EntryPoint;
			Thread.CpuThreadState.RA = (uint)HleEmulatorSpecialAddresses.CODE_PTR_EXIT_THREAD;
			Thread.CurrentStatus = HleThread.Status.Stopped;
			//Thread.CpuThreadState.RA = (uint)0;

			//Console.WriteLine("STACK: {0:X}", Thread.CpuThreadState.SP);

			return (uint)Thread.Id;
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
			//Console.WriteLine("LEN: {0:X}", ArgumentsLength);
			//Console.WriteLine("PTR: {0:X}", ArgumentsPointer);

			var CopiedDataAddress = (uint)((ThreadToStart.Stack.High - 0x100) - ((UserDataLength + 0xF) & ~0xF));

			if (UserDataPointer == 0) {
				ThreadToStart.CpuThreadState.GPR[4] = 0;
				ThreadToStart.CpuThreadState.GPR[5] = 0;
			} else {
				CpuThreadState.CpuProcessor.Memory.Copy(UserDataPointer, CopiedDataAddress, UserDataLength);
				ThreadToStart.CpuThreadState.GPR[4] = (int)UserDataLength;
				ThreadToStart.CpuThreadState.GPR[5] = (int)CopiedDataAddress;
			}

			ThreadToStart.CpuThreadState.GP = (uint)CpuThreadState.GP;
			ThreadToStart.CpuThreadState.SP = (uint)(CopiedDataAddress - 0x40);

			ThreadToStart.CurrentStatus = HleThread.Status.Ready;

			// Schedule new thread?
			HleState.ThreadManager.ScheduleNext(ThreadToStart);
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
			var ThreadToSleep = HleState.ThreadManager.Current;
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
			if (HleState.ThreadManager.Current == null) return 0;
			return HleState.ThreadManager.Current.Id;
		}

		/// <summary>
		/// Get the status information for the specified thread.
		/// </summary>
		/// <example>
		///		SceKernelThreadInfo status;
		///		status.size = sizeof(SceKernelThreadInfo);
		///		if (sceKernelReferThreadStatus(thid, &status) == 0) { Do something... }
		/// </example>
		/// <param name="thid">Id of the thread to get status</param>
		/// <param name="info">
		///		Pointer to the info structure to receive the data.
		///		Note: The structures size field should be set to
		///		sizeof(SceKernelThreadInfo) before calling this function.
		///	</param>
		/// <returns>0 if successful, otherwise the error code.</returns>
		[HlePspFunction(NID = 0x17C1684E, FirmwareVersion = 150)]
		public int sceKernelReferThreadStatus(int ThreadId, SceKernelThreadInfo* SceKernelThreadInfo)
		{
			var Thread = GetThreadById(ThreadId);
			*SceKernelThreadInfo = Thread.Info;
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
			var ThreadCurrent = HleState.ThreadManager.Current;
			var ThreadToWakeUp = HleState.ThreadManager.GetThreadById(ThreadId);
			ThreadToWakeUp.ChangeWakeUpCount(+1, ThreadCurrent);
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

			if (ThreadToWaitEnd.CurrentStatus == HleThread.Status.Stopped)
			{
				return 0;
			}

			if (ThreadToWaitEnd.CurrentStatus == HleThread.Status.Killed)
			{
				return 0;
			}

			bool TimedOut = false;

			HleState.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.None, "sceKernelWaitThreadEnd", WakeUpCallback =>
			{
				if (Timeout != null)
				{
					HleState.PspRtc.RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(*Timeout), () =>
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

			return 0;
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
			var CurrentThread = HleState.ThreadManager.Current;

			if (DelayInMicroseconds < 1000)
			{
				sceKernelCheckCallback(CurrentThread.CpuThreadState);
				CurrentThread.CpuThreadState.Yield();
			}
			else
			{
				CurrentThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Timer, "sceKernelDelayThread", WakeUpCallback =>
				{
					HleState.PspRtc.RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(DelayInMicroseconds), () =>
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
		//[HlePspNotImplemented]
		public int sceKernelChangeCurrentThreadAttr(int Unknown, PspThreadAttributes Attributes)
		{
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
			GetThreadById(ThreadId).PriorityValue = Priority;
			HleState.ThreadManager.Reschedule();
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
			var Thread = HleState.ThreadManager.GetThreadById(ThreadId);
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
			var CurrentThreadId = HleState.ThreadManager.Current.Id;
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
			var Thread = HleState.ThreadManager.Current;
			
			//Console.Error.WriteLine(ExitStatus);
			
			Thread.Info.ExitStatus = ExitStatus;

			Thread.CurrentStatus = HleThread.Status.Killed;
			HleState.ThreadManager.Reschedule();

			Thread.Exit();

			//HleState.ThreadManager.ExitThread(Thread);

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
			var Thread = HleState.ThreadManager.GetThreadById(ThreadId);
			HleState.ThreadManager.DeleteThread(Thread);
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
			return HleState.ThreadManager.Current.Info.PriorityCurrent;
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
			var HleThread = HleState.ThreadManager.GetThreadById(ThreadId, AllowSelf: true);
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
					foreach (var Thread in HleState.ThreadManager.Threads) List[n++] = Thread.Id;
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
		public int sceKernelSuspendThread(int ThreadId)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Resume a thread previously put into a suspended state with ::sceKernelSuspendThread.
		/// </summary>
		/// <param name="thid">UID of the thread to resume.</param>
		/// <returns>Success if greater or equal to 0, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x75156E8F, FirmwareVersion = 150)]
		public int sceKernelResumeThread(int thid)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Resume the dispatch thread
		/// </summary>
		/// <param name="state">
		/// The state of the dispatch thread 
		/// (from ::sceKernelSuspendDispatchThread)
		/// </param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x27E22EC2, FirmwareVersion = 150)]
		public int sceKernelResumeDispatchThread(int state)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Suspend the dispatch thread
		/// </summary>
		/// <returns>The current state of the dispatch thread, less than 0 on error</returns>
		[HlePspFunction(NID = 0x3AD58B8C, FirmwareVersion = 150)]
		public int sceKernelSuspendDispatchThread()
		{
			throw(new NotImplementedException());
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

			Thread.CurrentStatus = HleThread.Status.Killed;
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
			var CurrentThread = HleState.ThreadManager.Current;
			if (Thread == CurrentThread) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_THREAD));
			if (Thread.CurrentStatus != HleThread.Status.Waiting) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_THREAD_IS_NOT_WAIT));
			Thread.WakeUp();
			return 0;
		}

		/*
		public int _sceKernelExitDeleteThread(int Status, HleThread Thread)
		{
			if (Thread != null)
			{
				Thread.Finalize();
				HleState.ThreadManager.Exit(Thread);
			}
			return 0;
		}
		*/
	}
}

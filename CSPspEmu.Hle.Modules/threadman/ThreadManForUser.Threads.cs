using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
				HleState.MemoryManager.Memory.WriteRepeated1(0xFF, Thread.Stack.Low, Thread.Stack.Size);
				//Console.Error.WriteLine("-------------------------------------------------");
				//Console.Error.WriteLine("'{0}', '{1}'", StackSize, Thread.Stack.Size);
				//Console.Error.WriteLine("-------------------------------------------------");
			}
			Thread.Info.StackPointer = Thread.Stack.High;
			Thread.Info.StackSize = StackSize;
			Thread.CpuThreadState.PC = (uint)EntryPoint;
			Thread.CpuThreadState.GP = (uint)CpuThreadState.GP;
			Thread.CpuThreadState.SP = (uint)(Thread.Stack.High);
			Thread.CpuThreadState.RA = (uint)0x08000000;
			Thread.CurrentStatus = HleThread.Status.Stopped;
			//Thread.CpuThreadState.RA = (uint)0;

			//Console.WriteLine("STACK: {0:X}", Thread.CpuThreadState.SP);

			return (uint)Thread.Id;
		}

		/// <summary>
		/// Start a created thread
		/// </summary>
		/// <param name="ThreadId">Thread id from sceKernelCreateThread</param>
		/// <param name="ArgumentsLength">Length of the data pointed to by argp, in bytes</param>
		/// <param name="ArgumentsPointer">Pointer to the arguments.</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF475845D, FirmwareVersion = 150)]
		public int sceKernelStartThread(CpuThreadState CpuThreadState, int ThreadId, uint ArgumentsLength, uint ArgumentsPointer)
		{
			var ThreadToStart = GetThreadById((int)ThreadId);
			//Console.WriteLine("LEN: {0:X}", ArgumentsLength);
			//Console.WriteLine("PTR: {0:X}", ArgumentsPointer);
			ThreadToStart.CpuThreadState.GPR[4] = (int)ArgumentsLength;
			ThreadToStart.CpuThreadState.GPR[5] = (int)ArgumentsPointer;
			ThreadToStart.CurrentStatus = HleThread.Status.Ready;

			// Schedule new thread?
			CpuThreadState.Yield();
			return 0;
		}

		public int _sceKernelExitDeleteThread(int Status, HleThread Thread)
		{
			if (Thread != null)
			{
				Thread.Finalize();
				HleState.ThreadManager.Exit(Thread);
			}
			return 0;
		}

		/// <summary>
		/// Exit a thread and delete itself.
		/// </summary>
		/// <param name="Status">Exit status</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x809CE29B, FirmwareVersion = 150)]
		public int sceKernelExitDeleteThread(int Status)
		{
			return _sceKernelExitDeleteThread(Status, HleState.ThreadManager.Current);
		}

		/// <summary>
		/// Exit a thread
		/// </summary>
		/// <param name="Status">Exit status.</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xAA73C935, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelExitThread(int Status)
		{
			return sceKernelExitDeleteThread(Status);
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
			HleState.ThreadManager.Current.CurrentStatus = HleThread.Status.Waiting;
			HleState.ThreadManager.Current.CurrentWaitType = HleThread.WaitType.None;
			CpuThreadState.Yield();
			return 0;
		}

		/// <summary>
		/// Get the current thread Id
		/// </summary>
		/// <returns>The thread id of the calling thread.</returns>
		[HlePspFunction(NID = 0x293B45B8, FirmwareVersion = 150)]
		public int sceKernelGetThreadId()
		{
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
			*SceKernelThreadInfo = GetThreadById(ThreadId).Info;
			return 0;
		}

		/// <summary>
		/// Sleep thread until sceKernelWakeUp is called.
		/// </summary>
		/// <returns>Less than zero on error</returns>
		[HlePspFunction(NID = 0x9ACE131E, FirmwareVersion = 150)]
		public int sceKernelSleepThread(CpuThreadState CpuThreadState)
		{
			//logInfo("sceKernelSleepThread()");
			//return _sceKernelSleepThreadCB(false);
			HleState.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.None, "sceKernelSleepThread", WakeUpCallback =>
			{
			});
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
			throw(new NotImplementedException());
			/*
			ThreadState threadState = uniqueIdFactory.get!(ThreadState)(thid);
		
			logInfo("sceKernelWakeupThread");
		
			threadState.sleepingCriticalSection.tryLock({
				threadState.incrementWakeUpCount();
			}, {
				threadState.resetWakeUpCount();
				threadState.wakeUpEvent.signal();
			
				// Must wait until terminated?
				threadState.sleepingCriticalSection.waitEnded();
			});

			return 0;
			*/
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
			var ThreadToWaitEnd = GetThreadById(ThreadId);

			if (ThreadToWaitEnd.CurrentStatus == HleThread.Status.Stopped)
			{
				return 0;
			}

			if (ThreadToWaitEnd.CurrentStatus == HleThread.Status.Killed)
			{
				return 0;
			}

			HleState.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.None, "sceKernelWaitThreadEnd", WakeUpCallback =>
			{
				if (Timeout != null) throw (new NotImplementedException());
				Console.WriteLine("Wait End!");
				ThreadToWaitEnd.End += () =>
				{
					Console.WriteLine("Ended!");
					//throw(new Exception("aaaaaaaaaaaa"));
					WakeUpCallback();
				};
			});

			return 0;
		}

		private int _sceKernelDelayThreadCB(uint DelayInMicroseconds, bool HandleCallbacks)
		{
			HleState.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.Timer, "sceKernelDelayThread", WakeUpCallback =>
			{
				HleState.PspRtc.RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(DelayInMicroseconds), () =>
				{
					WakeUpCallback();
				});
			});

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
		/// Delete a thread
		/// </summary>
		/// <param name="ThreadId">UID of the thread to be deleted.</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x9FA03CD3, FirmwareVersion = 150)]
		public int sceKernelDeleteThread(int ThreadId)
		{
			return _sceKernelExitDeleteThread(-1, GetThreadById(ThreadId));
		}

		/// <summary>
		/// Terminate and delete a thread.
		/// </summary>
		/// <param name="ThreadId">UID of the thread to terminate and delete.</param>
		/// <returns>Success if greater or equal to 0, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x383F7BCC, FirmwareVersion = 150)]
		public int sceKernelTerminateDeleteThread(int ThreadId)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Modify the attributes of the current thread.
		/// </summary>
		/// <param name="unknown">Set to 0.</param>
		/// <param name="attr">The thread attributes to modify.  One of ::PspThreadAttributes.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xEA748E31, FirmwareVersion = 150)]
		[HlePspNotImplemented]
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
		public int sceKernelChangeThreadPriority(int ThreadId, int Priority)
		{
			GetThreadById(ThreadId).PriorityValue = Priority;
			//throw(new NotImplementedException());
			return 0;
		}
	}
}

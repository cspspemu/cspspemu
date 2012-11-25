using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Threading.EventFlags;
using CSharpUtils;
using CSharpUtils.Threading;

namespace CSPspEmu.Hle
{
	[Flags]
	public enum PspThreadAttributes : uint
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Enable VFPU access for the thread.
		/// </summary>
		Vfpu = 0x00004000,

		/// <summary>
		/// Start the thread in user mode (done automatically if the thread creating it is in user mode).
		/// </summary>
		User = 0x80000000,

		/// <summary>
		/// Thread is part of the USB/WLAN API.
		/// </summary>
		UsbWlan = 0xa0000000,
		
		/// <summary>
		/// Thread is part of the VSH API.
		/// </summary>
		Vsh = 0xc0000000,

		/// <summary>
		/// Allow using scratchpad memory for a thread, NOT USABLE ON V1.0
		/// </summary>
		ScratchRamEnable = 0x00008000,
		
		/// <summary>
		/// Disables filling the stack with 0xFF on creation
		/// </summary>
		NoFillStack = 0x00100000,
		
		/// <summary>
		/// Clear the stack when the thread is deleted
		/// </summary>
		ClearStack = 0x00200000,
	}

	public struct HleFunctionEntry
	{
		public uint NID;
		public String Name;
		public String Description;
		public HleModuleHost Module;
		public string ModuleName;

		public override string ToString()
		{
			return String.Format("FunctionEntry(NID=0x{0:X}, Name='{1}', Description='{2}', Module='{3}')", NID, Name, Description, Module);
		}
	}

	public struct DelegateInfo
	{
		public int CallIndex;
		public uint PC;
		public uint RA;
		public string ModuleImportName;
		public HleFunctionEntry FunctionEntry;
		public Action<CpuThreadState> Action;
		public HleThread Thread;

		public override string ToString()
		{
			try
			{
				return String.Format(
					"{0}: PC=0x{3:X}, RA=0x{4:X} => '{5}' : {1}::{2}",
					CallIndex, ModuleImportName, FunctionEntry.Name, PC, RA, (Thread != null) ? Thread.Name : "-");
			}
			catch (Exception Exception)
			{
				return String.Format("Invalid DelegateInfo : " + Exception);
			}
			//return this.ToStringDefault();
		}
	}

	public unsafe class HleThread : IDisposable, IPreemptiveItem
	{
		protected MethodCacheFast MethodCache;

		/// <summary>
		/// Lower priority value is a higher priority, so we negate the priorityvalue.
		/// </summary>
		int IPreemptiveItem.Priority
		{
			get
			{
				return -PriorityValue;
			}
		}

		bool IPreemptiveItem.Ready
		{
			get
			{
				//throw new NotImplementedException();
				if (CurrentStatus.HasFlag(Status.Running)) return true;
				if (CurrentStatus.HasFlag(Status.Ready)) return true;
				return false;
			}
		}

		private int _PriorityValue;

		/// <summary>
		/// Value used to schedule threads.
		/// </summary>
		public int PriorityValue
		{
			get
			{
				return _PriorityValue;
			}
			set
			{
				if (_PriorityValue != value)
				{
					_PriorityValue = value;
					StatusUpdated();
				}
			}
		}

		public DelegateInfo LastCalledHleFunction;

		//public int Priority = 1;
		protected GreenThread GreenThread;

		protected Coroutine Coroutine;

		HleInterruptManager HleInterruptManager;
		public CpuThreadState CpuThreadState { get; protected set; }
		//protected int MinimalInstructionCountForYield = 1000000;
		public int Id;
		//public String Name;
		private Status CurrentStatus;
		public WaitType CurrentWaitType;
		//public DateTime AwakeOnTime;
		public MemoryPartition Stack;
		public String WaitDescription;
		public object WaitObject;
		//public int InitPriority;
		public PspThreadAttributes Attribute;
		public SceKernelThreadInfo Info;
		public bool HandleCallbacks;
		public Action WakeUpCallback;
		public List<Action> WakeUpList = new List<Action>();

		public bool HasAllStatus(Status Has)
		{
			return (CurrentStatus & Has) == Has;
		}

		public bool HasAnyStatus(Status Has)
		{
			return (CurrentStatus & Has) != 0;
		}

		private void StatusUpdated()
		{
			HleThreadManager.UpdatedThread(this);
		}

		public void SetStatus(Status NewStatus)
		{
			//Console.WriteLine("@ {0} :: {1} -> {2}", this, this.CurrentStatus, NewStatus);
			if (CurrentStatus != NewStatus)
			{
				CurrentStatus = NewStatus;
				StatusUpdated();
			}
		}

#if false
		public void ChangeStatus(Status Add, Status Remove = 0)
		{
			AddStatus(Add);
			if (Remove != 0) RemoveStatus(Remove);
		}
#endif

		/// <summary>
		/// Number of times the thread have been paused.
		/// </summary>
		protected int YieldCount = 0;

		public bool IsWaitingAndHandlingCallbacks
		{
			get
			{
				return HasAllStatus(Status.Waiting) && HandleCallbacks;
			}
		}

		public uint GP
		{
			get { return Info.GP; }
			set { Info.GP = value; CpuThreadState.GP = value;  }
		}

		public String Name
		{
			get { fixed (byte* NamePtr = Info.Name) return PointerUtils.PtrToString(NamePtr, Encoding.ASCII); }
			set {
				fixed (byte* NamePtr = Info.Name) PointerUtils.StoreStringOnPtr(value, Encoding.ASCII, NamePtr);
				if (this.PspConfig.UseCoRoutines)
				{
					this.Coroutine.Name = value;
				}
				else
				{
					this.GreenThread.Name = value;
				}
			}
		}

		public HleThread(PspEmulatorContext PspEmulatorContext, CpuThreadState CpuThreadState)
		{
			this.HleInterruptManager = PspEmulatorContext.GetInstance<HleInterruptManager>();
			this.HleThreadManager = PspEmulatorContext.GetInstance<HleThreadManager>();
			this.MethodCache = CpuThreadState.CpuProcessor.MethodCache;
			this.PspConfig = CpuThreadState.CpuProcessor.PspConfig;

			if (this.PspConfig.UseCoRoutines)
			{
				this.Coroutine = HleThreadManager.Processor.CoroutinePool.CreateCoroutine(this.Name, MainLoop);
			}
			else
			{
				this.GreenThread = new GreenThread();
				GreenThread.InitAndStartStopped(MainLoop);
			}
			
			this.CpuThreadState = CpuThreadState;
		}

		public void ChangeWakeUpCount(int Increment, HleThread WakeupThread, bool HandleCallbacks = false)
		{
			var PreviousWakeupCount = Info.WakeupCount;
			Info.WakeupCount += Increment;
			var CurrentWakeupCount = Info.WakeupCount;

			/*
			Console.Error.WriteLine(
				"{0} : ChangeWakeUpCount : {1} -> {2}",
				this, PreviousWakeupCount, CurrentWakeupCount
			);
			*/

			var ThreadToSleep = this;

			// Sleep if sleeping decrement.
			if (Increment < 0 && CurrentWakeupCount < 0)
			{
				ThreadToSleep.SetWaitAndPrepareWakeUp(HleThread.WaitType.None, "sceKernelSleepThread", null, WakeUpCallback =>
				{
					ThreadToSleep.WakeUpCallback = () =>
					{
						WakeUpCallback();
						WakeUpCallback = null;
					};
				}, HandleCallbacks: HandleCallbacks);
			}
			// Awake 
			else if (Increment > 0 && PreviousWakeupCount < 0 && CurrentWakeupCount >= 0)
			{
				Action[] WakeUpListCopy;
				lock (WakeUpList)
				{
					WakeUpListCopy = WakeUpList.ToArray();
					WakeUpList.Clear();
				}

				if (WakeUpCallback != null)
				{
					WakeUpCallback();
				}
				else
				{
					Console.Error.WriteLine("Unexpected!");
				}

				foreach (var WakeUp in WakeUpListCopy)
				{
					WakeUp();
				}
			}

			if (Increment > 0)
			{
				/*
				return;
				Console.Error.WriteLine("Increment > 0 - Wakeup");

				WakeupThread.SetWaitAndPrepareWakeUp(WaitType.None, "sceKernelWakeupThread", WakeUpCallback =>
				{
					lock (WakeUpList)
					{
						WakeUpList.Add(() =>
						{
							WakeUpCallback();
						};
					}
				}, HandleCallbacks: HandleCallbacks);
				*/
				//WakeUpList.Add
			}
		}

		[HandleProcessCorruptedStateExceptions]
		protected void MainLoop()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo(PspConfig.ThreadCultureName);
			var Memory = CpuThreadState.CpuProcessor.Memory;
			try
			{
				while (true)
				{
					if (PspConfig.TraceThreadLoop)
					{
						Console.Out.WriteLine("HleThread.MainLoop :: Thread({0:X}) : PC: {1:X}", this.Id, CpuThreadState.PC);
					}
					//Console.WriteLine("PC:{0:X}", CpuThreadState.PC);
					uint PC = CpuThreadState.PC & PspMemory.MemoryMask;
					if (PC == 0)
					{
						ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
						{
							Console.Error.WriteLine("Trying to jump to 0x{0:X8}", PC);
						});
						SetWaitAndPrepareWakeUp(WaitType.None, "JUMP 0", new object(), (WakeupCallback) =>
						{
						});
						Thread.Sleep(-1);
					}
					var Delegate = GetDelegateAt(PC);
					{
						CpuThreadState.LastValidPC = PC;
					}
					{
						Delegate.Delegate(CpuThreadState);
					}
#if false
					if (!Memory.IsAddressValid(CpuThreadState.PC))
					{
						throw (new Exception(
							String.Format(
								"Instruction at address 0x{0:X} changed the PC to an invalid address 0x{1:X}",
								PC,
								CpuThreadState.PC
							)
						));
					}
#endif
				}
			}
			catch (AccessViolationException AccessViolationException)
			{
				Console.Error.WriteLine(AccessViolationException);

				var Field = typeof(AccessViolationException).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Single(FieldInfo => FieldInfo.Name == "_target");
				var Address = (ulong)((IntPtr)Field.GetValue(AccessViolationException)).ToInt64();
				throw (new PspMemory.InvalidAddressException(Address));
				//AccessViolationException.
			}
		}

		// 8903E08

		DynarecFunctionCompilerTask DynarecFunctionCompilerTask;

		public DynarecFunction GetDelegateAt(uint PC)
		{
			var Delegate = MethodCache.TryGetMethodAt(PC);
			if (Delegate == null)
			{
				if (DynarecFunctionCompilerTask == null)
				{
					DynarecFunctionCompilerTask = CpuThreadState.CpuProcessor.GetPspEmulatorContext().GetInstance<DynarecFunctionCompilerTask>();
				}

				MethodCache.SetMethodAt(
					PC,
					Delegate = DynarecFunctionCompilerTask.GetFunctionForAddress(PC)
				);
			}

			return Delegate;
		}

		public void Step(int InstructionCountForYield = 1000000)
		{
			do
			{
				//CpuThreadState.hlest
				CpuThreadState.StepInstructionCount = InstructionCountForYield;
				//this.MinimalInstructionCountForYield = InstructionCountForYield;
				if (this.PspConfig.UseCoRoutines)
				{
					Coroutine.ExecuteStep();
				}
				else
				{
					GreenThread.SwitchTo();
				}
			} while (!HleInterruptManager.Enabled);
		}

		public void WakeUp()
		{
			if (!this.HasAllStatus(Status.Waiting))
			{
				Console.Error.WriteLine("Trying to awake a non waiting thread '{0}'", this.CurrentStatus);
				//throw (new InvalidOperationException());
			}

			//Console.WriteLine("Thread:{0}:{1}", this, Thread.CurrentThread.Name);

			this.SetStatus(Status.Ready);

			//this.CurrentStatus.pree
			//if (CurrentWaitType != WaitType.Timer && CurrentWaitType != WaitType.Display)
			{
				//this.GreenThread.SwitchTo();
				//CpuThreadState.Yield();
			}
		}

		public void SetWaitAndPrepareWakeUp(WaitType WaitType, String WaitDescription, object WaitObject, Action<WakeUpCallbackDelegate> PrepareCallback, bool HandleCallbacks = false)
		{
			if (this.HasAllStatus(Status.Waiting))
			{
				Console.Error.WriteLine("Trying to sleep an already sleeping thread!");
			}

			bool CalledAlready = false;
			YieldCount++;
			SetWait0(WaitType, WaitDescription, WaitObject, HandleCallbacks);
			{
				//PrepareCallback(WakeUp);
				PrepareCallback(() =>
				{
					if (!CalledAlready)
					{
						CalledAlready = true;
						WakeUp();
					}
				});
			}
			SetWait1();
		}

		protected void SetWait0(WaitType WaitType, String WaitDescription, object WaitObject, bool HandleCallbacks)
		{
			this.SetStatus(Status.Waiting);
			this.CurrentWaitType = WaitType;
			this.WaitDescription = WaitDescription;
			this.WaitObject = WaitObject;
			this.HandleCallbacks = HandleCallbacks;
		}

		protected void SetWait1()
		{
			if (this.HasAllStatus(Status.Waiting))
			{
				CpuThreadState.Yield();
			}
		}

		/*
		public void SetWait(WaitType WaitType, String WaitDescription, bool HandleCallbacks)
		{
			SetWait0(WaitType, WaitDescription, HandleCallbacks);
			SetWait1();
		}
		*/

		public override string ToString()
		{
			return String.Format("HleThread(Id={0}, Priority={1}, Name='{2}', Status={3})", Id, PriorityValue, Name, CurrentStatus);
		}

		public string ToExtendedString()
		{
			var Ret = String.Format(
				"HleThread(Id={0}, Priority={1}, PC=0x{2:X}, LastValidPC=0x{3:X}, SP=0x{4:X}, Name='{5}', Status={6}, WaitCount={7}",
				Id, PriorityValue,
				CpuThreadState.PC, CpuThreadState.LastValidPC, CpuThreadState.SP,
				Name, CurrentStatus, YieldCount
			);
			switch (CurrentStatus)
			{
				case Status.Waiting:
					Ret += String.Format(
						", CurrentWaitType={0}, WaitDescription={1}, WaitObject={2}, HandleCallbacks={3}",
						CurrentWaitType, WaitDescription, WaitObject, HandleCallbacks
					);
					break;
			}
			//Ret += String.Format(", LastCalledHleFunction={0}", LastCalledHleFunction);
			return Ret + ")";
		}

		public void Exit()
		{
			if (End != null)
			{
				var End2 = End;
				End = null;
				End2();
			}
		}

		public event Action End;
		private PspConfig PspConfig;
		private HleThreadManager HleThreadManager;

		public void Dispose()
		{
			if (Coroutine!= null) Coroutine.Dispose();
			if (GreenThread != null) GreenThread.Dispose();
		}

		public void DumpStack(TextWriter TextWriter)
		{
			var FullCallStack = CpuThreadState.GetCurrentCallStack();
			TextWriter.WriteLine("   LastCalledHleFunction: {0}", LastCalledHleFunction);
			TextWriter.WriteLine("   CallStack({0})", FullCallStack.Length);
			foreach (var CallerPC in FullCallStack.Slice(0, 4))
			{
				TextWriter.WriteLine("     MEM(0x{0:X}) : NOREL(0x{1:X})", CallerPC, CallerPC - PspConfig.RelocatedBaseAddress);
			}
			if (FullCallStack.Length > 4)
			{
				TextWriter.WriteLine("     ...");
			}
		}

		public enum WaitType
		{
			None,
			Timer,
			GraphicEngine,
			Audio,
			Display,
			Semaphore,
			Mutex,
		}

		[Flags]
		public enum Status
		{
			/// <summary>
			/// This is the current thread and is running right now.
			/// </summary>
			Running = 1,

			/// <summary>
			/// The thread is not running right now, but it will be scheduled.
			/// </summary>
			Ready = 2,

			/// <summary>
			/// The thread is waiting for a event.
			/// </summary>
			Waiting = 4,

			/// <summary>
			/// ?
			/// </summary>
			Suspend = 8,

			/// <summary>
			/// ?
			/// </summary>
			Stopped = 16,

			/// <summary>
			/// ?
			/// </summary>
			Killed = 32,
		}
	}

	public unsafe struct SceKernelSysClock
	{
		//ulong Value;
		public uint Low;
		public uint High;

		public long MicroSeconds
		{
			get
			{
				fixed (uint* LowPtr = &Low)
				{
					return *(long*)LowPtr;
				}
			}
			set
			{
				fixed (uint* LowPtr = &Low)
				{
					*(long*)LowPtr = value;
				}
			}
		}
	}

	/// <summary>
	/// Event flag wait types
	/// </summary>
	/*
	public enum PspEventFlagWaitTypes : uint
	{
		/// <summary>
		/// Wait for all bits in the pattern to be set 
		/// </summary>
		PSP_EVENT_WAITAND = 0x00,

		/// <summary>
		/// Wait for one or more bits in the pattern to be set
		/// </summary>
		PSP_EVENT_WAITOR = 0x01,

		/// <summary>
		/// Clear all the wait pattern when it matches
		/// </summary>
		PSP_EVENT_WAITCLEARALL = 0x10,

		/// <summary>
		/// Clear the wait pattern when it matches
		/// </summary>
		PSP_EVENT_WAITCLEAR = 0x20,
	};
	*/

	[Flags]
	public enum PspThreadStatus : uint
	{
		/// <summary>
		/// 0x01 - Running.
		/// </summary>
		PSP_THREAD_RUNNING = 1,

		/// <summary>
		/// 0x02 - Ready.
		/// </summary>
		PSP_THREAD_READY = 2,

		/// <summary>
		/// 0x04 - Waiting.
		/// </summary>
		PSP_THREAD_WAITING = 4,

		/// <summary>
		/// 0x08 - Suspended.
		/// </summary>
		PSP_THREAD_SUSPEND = 8,

		/// <summary>
		/// 0x10 - Stopped. (Before startThread)
		/// </summary>
		PSP_THREAD_STOPPED = 16,

		/// <summary>
		/// 0x20 - Thread manager has killed the thread (stack overflow)
		/// </summary>
		PSP_THREAD_KILLED = 32,
	}

	//alias int function(SceSize args, void* argp) SceKernelThreadEntry;
	public enum SceKernelThreadEntry : uint
	{
	}

	public unsafe struct SceKernelThreadInfo
	{
		/// <summary>
		/// 0x0000 - Size of the structure
		/// </summary>
		public int Size;

		/// <summary>
		/// 0x0004 - Null terminated name of the thread
		/// </summary>
		public fixed byte Name[32];

		/// <summary>
		/// 0x0024 - Thread attributes
		/// </summary>
		public uint Attributes;

		/// <summary>
		/// 0x0028 - Thread status
		/// </summary>
		public PspThreadStatus Status;

		/// <summary>
		/// Thread entry point
		/// </summary>
		public SceKernelThreadEntry EntryPoint;

		/// <summary>
		/// Thread stack pointer
		/// </summary>
		public uint StackPointer;

		/// <summary>
		/// Thread stack size
		/// </summary>
		public int StackSize;

		/// <summary>
		/// Pointer to the gp
		/// </summary>
		public uint GP;

		/// <summary>
		/// Initial Priority
		/// </summary>
		public int PriorityInitially;

		/// <summary>
		/// Current Priority
		/// </summary>
		public int PriorityCurrent;

		/// <summary>
		/// Wait Type
		/// </summary>
		public EventFlagWaitTypeSet WaitType;

		/// <summary>
		/// Wait id
		/// </summary>
		public int WaitId;

		/// <summary>
		/// Wakeup count
		/// </summary>
		public int WakeupCount;

		/// <summary>
		/// Exit status of the thread
		/// </summary>
		public int ExitStatus;

		/// <summary>
		/// Number of clock cycles run
		/// </summary>
		public SceKernelSysClock RunClocks;

		/// <summary>
		/// Interrupt preemption count
		/// </summary>
		public int InterruptPreemptionCount;

		/// <summary>
		/// Thread preemption count
		/// </summary>
		public int ThreadPreemptionCount;

		/// <summary>
		/// Release count
		/// </summary>
		public int ReleaseCount;
	}

	public struct SceKernelThreadOptParam
	{
	}
}

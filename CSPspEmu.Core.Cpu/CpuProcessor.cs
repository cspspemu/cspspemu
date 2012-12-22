using System;
using System.Collections.Generic;
using CSPspEmu.Core.Cpu.InstructionCache;
using CSPspEmu.Core.Cpu.Dynarec;
using SafeILGenerator.Utils;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu
{
	/// <summary>
	/// CpuState shareed by all CpuThreadState
	/// </summary>
    public sealed class CpuProcessor
	{
		public readonly Dictionary<string, uint> GlobalInstructionStats = new Dictionary<string, uint>();

		[Inject]
		public CpuConfig CpuConfig;

		[Inject]
		public PspMemory Memory;

		[Inject]
		public ICpuConnector CpuConnector;

		[Inject]
		public DynarecFunctionCompiler DynarecFunctionCompiler;

		[Inject]
		public IInterruptManager IInterruptManager;

		public MethodCache MethodCache = new MethodCache();

		public Dictionary<uint, NativeSyscallInfo> RegisteredNativeSyscallMethods = new Dictionary<uint, NativeSyscallInfo>();
		private Dictionary<int, Action<CpuThreadState, int>> RegisteredNativeSyscalls = new Dictionary<int,Action<CpuThreadState,int>>();
		public HashSet<uint> NativeBreakpoints = new HashSet<uint>();

		public event Action DebugCurrentThreadEvent;
		public bool DebugFunctionCreation;

		public bool InterruptEnabled = true;
		public bool InterruptFlag = false;

		public void ExecuteInterrupt(CpuThreadState CpuThreadState)
		{
			if (InterruptEnabled && InterruptFlag)
			{
				IInterruptManager.Interrupt(CpuThreadState);
			}
		}

		private CpuProcessor()
		{
		}

		public CpuProcessor RegisterNativeSyscall(int Code, Action Callback)
		{
			return RegisterNativeSyscall(Code, (_Code, _Processor) => Callback());
		}

		public CpuProcessor RegisterNativeSyscall(int Code, Action<CpuThreadState, int> Callback)
		{
			RegisteredNativeSyscalls[Code] = Callback;
			return this;
		}

		public Action<CpuThreadState, int> GetSyscall(int Code)
		{
			Action<CpuThreadState, int> Callback;
			if (RegisteredNativeSyscalls.TryGetValue(Code, out Callback))
			{
				return Callback;
			}
			else
			{
				return null;
			}
		}

		public void Syscall(int Code, CpuThreadState CpuThreadState)
		{
			Action<CpuThreadState, int> Callback;
			if ((Callback = GetSyscall(Code)) != null)
			{
				Callback(CpuThreadState, Code);
			}
			else
			{
				Console.WriteLine("Undefined syscall: {0:X6} at 0x{1:X8}", Code, CpuThreadState.PC);
			}
		}

		public void sceKernelDcacheWritebackInvalidateAll()
		{
		}

		public void sceKernelDcacheWritebackRange(uint Address, uint Size)
		{
		}

		public void sceKernelDcacheWritebackInvalidateRange(uint Address, uint Size)
		{
		}

		public void sceKernelDcacheInvalidateRange(uint Address, uint Size)
		{
		}

		public void sceKernelDcacheWritebackAll()
		{
		}

		public void sceKernelIcacheInvalidateAll()
		{
			MethodCache.FlushAll();
		}

		public void sceKernelIcacheInvalidateRange(uint Address, uint Size)
		{
			MethodCache.FlushRange(Address, Address + Size);
		}

		public static void DebugCurrentThread(CpuThreadState CpuThreadState)
		{
			var CpuProcessor = CpuThreadState.CpuProcessor;
			Console.Error.WriteLine("*******************************************");
			Console.Error.WriteLine("* DebugCurrentThread **********************");
			Console.Error.WriteLine("*******************************************");
			CpuProcessor.DebugCurrentThreadEvent();
			Console.Error.WriteLine("*******************************************");
			CpuThreadState.DumpRegisters();
			Console.Error.WriteLine("*******************************************");
		}
	}
}

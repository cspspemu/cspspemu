using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Emiter;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu
{
	unsafe sealed public class CpuProcessor : PspEmulatorComponent, IResetable
	{
		readonly public Dictionary<string, uint> GlobalInstructionStats = new Dictionary<string, uint>();

		public PspConfig PspConfig;
		public PspMemory Memory;
		public MethodCacheFast MethodCache;
		private Dictionary<int, Action<int, CpuThreadState>> RegisteredNativeSyscalls;
		public HashSet<uint> NativeBreakpoints;
		public bool IsRunning;
		public bool RunningCallback;

		public PspEmulatorContext GetPspEmulatorContext()
		{
			return PspEmulatorContext;
		}

		public override void InitializeComponent()
		{
			this.PspConfig = PspEmulatorContext.PspConfig;
			this.Memory = PspEmulatorContext.GetInstance<PspMemory>();
			Reset();
		}

		public void Reset()
		{
			MethodCache = new MethodCacheFast();
			NativeBreakpoints = new HashSet<uint>();
			RegisteredNativeSyscalls = new Dictionary<int, Action<int, CpuThreadState>>();
			IsRunning = true;
		}

		public CpuProcessor RegisterNativeSyscall(int Code, Action Callback)
		{
			return RegisterNativeSyscall(Code, (_Code, _Processor) => Callback());
		}

		public CpuProcessor RegisterNativeSyscall(int Code, Action<int, CpuThreadState> Callback)
		{
			RegisteredNativeSyscalls[Code] = Callback;
			return this;
		}

		public void Syscall(int Code, CpuThreadState CpuThreadState)
		{
			Action<int, CpuThreadState> Callback;
			if (RegisteredNativeSyscalls.TryGetValue(Code, out Callback))
			{
				Callback(Code, CpuThreadState);
			}
			else
			{
				Console.WriteLine("Undefined syscall: %06X at 0x%08X".Sprintf(Code, CpuThreadState.PC));
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
			MethodCache.Clear();
		}

		public void sceKernelIcacheInvalidateRange(uint Address, uint Size)
		{
			//Console.Error.WriteLine("sceKernelIcacheInvalidateRange!!! (0x{0:X}, {1})", Address, Size);
			MethodCache.ClearRange(Address, Address + Size);
			//MethodCache.Clear();
		}

		public event Action DebugCurrentThreadEvent;

		static public void DebugCurrentThread(CpuThreadState CpuThreadState)
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

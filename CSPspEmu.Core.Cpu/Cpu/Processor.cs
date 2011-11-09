using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Cpu.Emiter;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Cpu
{
	unsafe sealed public class Processor : IResetable
	{
		public PspConfig PspConfig;
		public PspMemory Memory;
		public MethodCache MethodCache;
		private Dictionary<int, Action<int, CpuThreadState>> RegisteredNativeSyscalls;
		public HashSet<uint> NativeBreakpoints;
		public bool IsRunning;

		public Processor(PspConfig PspConfig, PspMemory Memory)
		{
			this.PspConfig = PspConfig;
			this.Memory = Memory;
			Reset();
		}

		public void Reset()
		{
			MethodCache = new MethodCache();
			NativeBreakpoints = new HashSet<uint>();
			RegisteredNativeSyscalls = new Dictionary<int, Action<int, CpuThreadState>>();
			IsRunning = true;
		}

		public Processor RegisterNativeSyscall(int Code, Action Callback)
		{
			return RegisterNativeSyscall(Code, (_Code, _Processor) => Callback());
		}

		public Processor RegisterNativeSyscall(int Code, Action<int, CpuThreadState> Callback)
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
	}
}

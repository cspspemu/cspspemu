using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Cpu.Emiter;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Cpu
{
	unsafe sealed public class Processor
	{
		public bool TraceJIT = false;
		public PspMemory Memory;
		public MethodCache MethodCache;
		private Dictionary<int, Action<int, CpuThreadState>> RegisteredNativeSyscalls = new Dictionary<int, Action<int, CpuThreadState>>();
		public HashSet<uint> NativeBreakpoints = new HashSet<uint>();

		public Processor(PspMemory Memory)
		{
			this.Memory = Memory;
			this.MethodCache = new MethodCache();
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

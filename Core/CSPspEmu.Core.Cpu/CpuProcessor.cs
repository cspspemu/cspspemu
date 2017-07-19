using System;
using System.Collections.Generic;
using CSPspEmu.Core.Cpu.InstructionCache;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu
{
    /// <summary>
    /// CpuState shareed by all CpuThreadState
    /// </summary>
    public sealed class CpuProcessor
    {
        public readonly Dictionary<string, uint> GlobalInstructionStats = new Dictionary<string, uint>();

        [Inject] public InjectContext InjectContext;

        [Inject] public CpuConfig CpuConfig;

        [Inject] public PspMemory Memory;

        [Inject] public ICpuConnector CpuConnector;

        [Inject] public DynarecFunctionCompiler DynarecFunctionCompiler;

        [Inject] public IInterruptManager InterruptManager;

        [Inject] public MethodCache MethodCache;

        public Dictionary<uint, NativeSyscallInfo> RegisteredNativeSyscallMethods =
            new Dictionary<uint, NativeSyscallInfo>();

        private readonly Dictionary<int, Action<CpuThreadState, int>> _registeredNativeSyscalls =
            new Dictionary<int, Action<CpuThreadState, int>>();

        public HashSet<uint> NativeBreakpoints = new HashSet<uint>();

        public event Action DebugCurrentThreadEvent;
        public bool DebugFunctionCreation;

        public volatile bool InterruptEnabled = true;
        public volatile bool InterruptFlag = false;

        public void ExecuteInterrupt(CpuThreadState cpuThreadState)
        {
            if (InterruptEnabled && InterruptFlag)
            {
                InterruptManager.Interrupt(cpuThreadState);
            }
        }

        public CpuProcessor RegisterNativeSyscall(int code, Action callback) =>
            RegisterNativeSyscall(code, (_, processor) => callback());

        public CpuProcessor RegisterNativeSyscall(int code, Action<CpuThreadState, int> callback)
        {
            _registeredNativeSyscalls[code] = callback;
            return this;
        }

        public Action<CpuThreadState, int> GetSyscall(int code)
        {
            Action<CpuThreadState, int> callback;
            return _registeredNativeSyscalls.TryGetValue(code, out callback) ? callback : null;
        }

        public void Syscall(int code, CpuThreadState cpuThreadState)
        {
            Action<CpuThreadState, int> callback;
            if ((callback = GetSyscall(code)) != null)
            {
                callback(cpuThreadState, code);
            }
            else
            {
                Console.WriteLine("Undefined syscall: {0:X6} at 0x{1:X8}", code, cpuThreadState.Pc);
            }
        }

        public void SceKernelDcacheWritebackInvalidateAll()
        {
        }

        public void SceKernelDcacheWritebackRange(uint address, int size)
        {
        }

        public void SceKernelDcacheWritebackInvalidateRange(uint address, int size)
        {
        }

        public void SceKernelDcacheInvalidateRange(uint address, int size)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void SceKernelDcacheWritebackAll()
        {
        }

        public void SceKernelIcacheInvalidateAll() => MethodCache.FlushAll();

        public void SceKernelIcacheInvalidateRange(uint address, uint size) =>
            MethodCache.FlushRange(address, address + size);

        public static void DebugCurrentThread(CpuThreadState cpuThreadState)
        {
            var cpuProcessor = cpuThreadState.CpuProcessor;
            Console.Error.WriteLine("*******************************************");
            Console.Error.WriteLine("* DebugCurrentThread **********************");
            Console.Error.WriteLine("*******************************************");
            cpuProcessor.DebugCurrentThreadEvent();
            Console.Error.WriteLine("*******************************************");
            cpuThreadState.DumpRegisters();
            Console.Error.WriteLine("*******************************************");
        }
    }
}
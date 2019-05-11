using System;
using System.Collections.Generic;
using CSharpUtils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Interop
{
    public class HleInterop
    {
        //ThreadLocal<HleThread> CurrentFakeHleThreads;

        [Inject] protected HleThreadManager HleThreadManager;

        [Inject] protected CpuProcessor CpuProcessor;

        [Inject] protected InjectContext InjectContext;

        private HleInterop()
        {
        }

        public uint ExecuteFunctionNowLater(uint function, bool executeNow, params object[] arguments)
        {
            if (executeNow)
            {
                return ExecuteFunctionNow(function, arguments);
            }
            else
            {
                ExecuteFunctionLater(function, arguments);
                return 0;
            }
        }

        public uint ExecuteFunctionNow(uint function, params object[] arguments)
        {
            var currentFakeHleThread = HleThreadManager.CurrentOrAny;
            currentFakeHleThread.CpuThreadState.CopyRegistersFrom(HleThreadManager.CurrentOrAny.CpuThreadState);
            SetArgumentsToCpuThreadState(currentFakeHleThread.CpuThreadState, function, arguments);
            Console.Out.WriteLineColored(ConsoleColor.Magenta, "ExecuteFunctionNow: 0x{0:X8}", function);
            currentFakeHleThread.CpuThreadState.ExecuteFunctionAndReturn(currentFakeHleThread.CpuThreadState.Pc);
            Console.Out.WriteLineColored(ConsoleColor.Magenta, "... {0}",
                currentFakeHleThread.CpuThreadState.Gpr2);
            return currentFakeHleThread.CpuThreadState.Gpr2;
        }

        public class QueuedExecution
        {
            public uint Function;
            public Action<uint> ExecutedCallback;
            public object[] Arguments;
        }

        readonly Queue<QueuedExecution> _queuedExecutions = new Queue<QueuedExecution>();

        public bool HasQueuedFunctions => _queuedExecutions.Count > 0;

        public int ExecuteAllQueuedFunctionsNow()
        {
            lock (_queuedExecutions)
            {
                var executedCount = 0;
                while (_queuedExecutions.Count > 0)
                {
                    var queuedExecution = _queuedExecutions.Dequeue();
                    var result = ExecuteFunctionNow(queuedExecution.Function, queuedExecution.Arguments);
                    queuedExecution.ExecutedCallback?.Invoke(result);
                    executedCount++;
                }
                return executedCount;
            }
        }

        public void ExecuteFunctionLater(uint function, params object[] arguments) =>
            ExecuteFunctionLater(function, result => { }, arguments);

        public void ExecuteFunctionLater(uint function, Action<uint> executedCallback, params object[] arguments)
        {
            lock (_queuedExecutions)
            {
                _queuedExecutions.Enqueue(new QueuedExecution
                {
                    Function = function,
                    ExecutedCallback = executedCallback,
                    Arguments = arguments,
                });
            }
        }

        public HleThread Execute(CpuThreadState fakeCpuThreadState)
        {
            var currentFakeHleThread = HleThreadManager.CurrentOrAny;

            currentFakeHleThread.CpuThreadState.CopyRegistersFrom(fakeCpuThreadState);
            //HleCallback.SetArgumentsToCpuThreadState(CurrentFake.CpuThreadState);

            currentFakeHleThread.CpuThreadState.ExecuteAt(currentFakeHleThread.CpuThreadState.Pc);

            ////CurrentFake.CpuThreadState.PC = HleCallback.Function;
            //CurrentFakeHleThread.CpuThreadState.RA = HleEmulatorSpecialAddresses.CODE_PTR_FINALIZE_CALLBACK;
            ////Current.CpuThreadState.RA = 0;
            //
            //CpuProcessor.RunningCallback = true;
            //while (CpuProcessor.RunningCallback)
            //{
            //	//Console.WriteLine("AAAAAAA {0:X}", CurrentFake.CpuThreadState.PC);
            //	CurrentFakeHleThread.Step();
            //}

            return currentFakeHleThread;
        }

        public static void SetArgumentsToCpuThreadState(CpuThreadState cpuThreadState, uint function,
            params object[] arguments)
        {
            var gprIndex = 4;

            //int FprIndex = 0;
            void Align(int alignment) => gprIndex = (int) MathUtils.NextAligned((uint) gprIndex, alignment);

            foreach (var argument in arguments)
            {
                var argumentType = argument.GetType();
                if (argumentType == typeof(uint))
                {
                    Align(1);
                    cpuThreadState.Gpr[gprIndex++] = (int) (uint) argument;
                }
                else if (argumentType == typeof(int))
                {
                    Align(1);
                    cpuThreadState.Gpr[gprIndex++] = (int) argument;
                }
                else if (argumentType == typeof(PspPointer))
                {
                    Align(1);
                    cpuThreadState.Gpr[gprIndex++] = (int) (uint) (PspPointer) argument;
                }
                else if (argumentType.IsEnum)
                {
                    Align(1);
                    cpuThreadState.Gpr[gprIndex++] = Convert.ToInt32(argument);
                }
                else
                {
                    throw new NotImplementedException($"Can't handle type '{argumentType}'");
                }
            }

            cpuThreadState.Pc = function;
            //Console.Error.WriteLine(CpuThreadState);
            //CpuThreadState.DumpRegisters(Console.Error);
        }
    }
}
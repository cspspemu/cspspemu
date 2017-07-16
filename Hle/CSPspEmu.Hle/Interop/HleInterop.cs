using System;
using System.Collections.Generic;
using System.Threading;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Interop;
using CSPspEmu.Core.Cpu.Dynarec;

namespace CSPspEmu.Hle
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

        public uint ExecuteFunctionNowLater(uint Function, bool ExecuteNow, params object[] Arguments)
        {
            if (ExecuteNow)
            {
                return ExecuteFunctionNow(Function, Arguments);
            }
            else
            {
                ExecuteFunctionLater(Function, Arguments);
                return 0;
            }
        }

        public uint ExecuteFunctionNow(uint Function, params object[] Arguments)
        {
            var CurrentFakeHleThread = HleThreadManager.CurrentOrAny;
            CurrentFakeHleThread.CpuThreadState.CopyRegistersFrom(HleThreadManager.CurrentOrAny.CpuThreadState);
            SetArgumentsToCpuThreadState(CurrentFakeHleThread.CpuThreadState, Function, Arguments);
            Console.Out.WriteLineColored(ConsoleColor.Magenta, "ExecuteFunctionNow: 0x{0:X8}", Function);
            CurrentFakeHleThread.CpuThreadState.ExecuteFunctionAndReturn(CurrentFakeHleThread.CpuThreadState.PC);
            Console.Out.WriteLineColored(ConsoleColor.Magenta, "... {0}",
                (uint) CurrentFakeHleThread.CpuThreadState.GPR2);
            return (uint) CurrentFakeHleThread.CpuThreadState.GPR2;
        }

        public class QueuedExecution
        {
            public uint Function;
            public Action<uint> ExecutedCallback;
            public object[] Arguments;
        }

        Queue<QueuedExecution> QueuedExecutions = new Queue<QueuedExecution>();

        public bool HasQueuedFunctions
        {
            get { return QueuedExecutions.Count > 0; }
        }

        public int ExecuteAllQueuedFunctionsNow()
        {
            lock (QueuedExecutions)
            {
                int ExecutedCount = 0;
                while (QueuedExecutions.Count > 0)
                {
                    var QueuedExecution = QueuedExecutions.Dequeue();
                    var Result = ExecuteFunctionNow(QueuedExecution.Function, QueuedExecution.Arguments);
                    if (QueuedExecution.ExecutedCallback != null) QueuedExecution.ExecutedCallback(Result);
                    ExecutedCount++;
                }
                return ExecutedCount;
            }
        }

        public void ExecuteFunctionLater(uint Function, params object[] Arguments)
        {
            ExecuteFunctionLater(Function, (Result) => { }, Arguments);
        }

        public void ExecuteFunctionLater(uint Function, Action<uint> ExecutedCallback, params object[] Arguments)
        {
            lock (QueuedExecutions)
            {
                QueuedExecutions.Enqueue(new QueuedExecution()
                {
                    Function = Function,
                    ExecutedCallback = ExecutedCallback,
                    Arguments = Arguments,
                });
            }
        }

        public HleThread Execute(CpuThreadState FakeCpuThreadState)
        {
            var CurrentFakeHleThread = HleThreadManager.CurrentOrAny;

            CurrentFakeHleThread.CpuThreadState.CopyRegistersFrom(FakeCpuThreadState);
            //HleCallback.SetArgumentsToCpuThreadState(CurrentFake.CpuThreadState);

            CurrentFakeHleThread.CpuThreadState.ExecuteAT(CurrentFakeHleThread.CpuThreadState.PC);

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

            return CurrentFakeHleThread;
        }

        public static void SetArgumentsToCpuThreadState(CpuThreadState CpuThreadState, uint Function,
            params object[] Arguments)
        {
            int GprIndex = 4;
            //int FprIndex = 0;
            Action<int> GprAlign = (int Alignment) =>
            {
                GprIndex = (int) MathUtils.NextAligned((uint) GprIndex, Alignment);
            };
            foreach (var Argument in Arguments)
            {
                var ArgumentType = Argument.GetType();
                if (ArgumentType == typeof(uint))
                {
                    GprAlign(1);
                    CpuThreadState.GPR[GprIndex++] = (int) (uint) Argument;
                }
                else if (ArgumentType == typeof(int))
                {
                    GprAlign(1);
                    CpuThreadState.GPR[GprIndex++] = (int) Argument;
                }
                else if (ArgumentType == typeof(PspPointer))
                {
                    GprAlign(1);
                    CpuThreadState.GPR[GprIndex++] = (int) (uint) (PspPointer) Argument;
                }
                else if (ArgumentType.IsEnum)
                {
                    GprAlign(1);
                    CpuThreadState.GPR[GprIndex++] = Convert.ToInt32(Argument);
                }
                else
                {
                    throw (new NotImplementedException(String.Format("Can't handle type '{0}'", ArgumentType)));
                }
            }

            CpuThreadState.PC = Function;
            //Console.Error.WriteLine(CpuThreadState);
            //CpuThreadState.DumpRegisters(Console.Error);
        }
    }
}
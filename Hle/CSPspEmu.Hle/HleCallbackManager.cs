using System;
using System.Collections.Generic;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Interop;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Vfs.MemoryStick;

#pragma warning disable 649
#pragma warning disable 169

namespace CSPspEmu.Hle.Managers
{
    [Obsolete("Should check Interop and decide which one to use, or refactor or something.")]
    public class HleCallbackManager : IMemoryStickEventHandler
    {
        public HleUidPool<HleCallback> Callbacks { get; protected set; }
        private readonly Queue<HleCallback> _scheduledCallbacks = new Queue<HleCallback>();

        [Inject] private CpuProcessor _cpuProcessor;

        [Inject] private HleInterop _hleInterop;

        private HleCallbackManager()
        {
            Callbacks = new HleUidPool<HleCallback>();
        }

        public void ScheduleCallback(HleCallback hleCallback)
        {
            Console.WriteLine("ScheduleCallback! {0}", hleCallback);
            lock (this)
            {
                _scheduledCallbacks.Enqueue(hleCallback);
            }
        }

        public HleCallback DequeueScheduledCallback()
        {
            lock (this)
            {
                var hleCallback = _scheduledCallbacks.Dequeue();
                Console.WriteLine("DequeueScheduledCallback! : {0}", hleCallback);
                return hleCallback;
            }
        }

        public bool HasScheduledCallbacks
        {
            get
            {
                lock (this)
                {
                    return _scheduledCallbacks.Count > 0;
                }
            }
        }

        public int ExecuteQueued(CpuThreadState cpuThreadState, bool mustReschedule)
        {
            var executedCount = 0;

            //Console.WriteLine("ExecuteQueued");
            if (!HasScheduledCallbacks) return executedCount;
            //Console.WriteLine("ExecuteQueued.HasScheduledCallbacks!");
            //Console.Error.WriteLine("STARTED CALLBACKS");
            while (HasScheduledCallbacks)
            {
                var hleCallback = DequeueScheduledCallback();

                /*
                    var FakeCpuThreadState = new CpuThreadState(CpuProcessor);
                    FakeCpuThreadState.CopyRegistersFrom(CpuThreadState);
                    HleCallback.SetArgumentsToCpuThreadState(FakeCpuThreadState);

                    if (FakeCpuThreadState.PC == 0x88040E0) FakeCpuThreadState.PC = 0x880416C;
                    */
                //Console.WriteLine("ExecuteCallback: PC=0x{0:X}", FakeCpuThreadState.PC);
                //Console.WriteLine("               : A0=0x{0:X}", FakeCpuThreadState.GPR[4]);
                //Console.WriteLine("               : A1=0x{0:X}", FakeCpuThreadState.GPR[5]);
                try
                {
                    _hleInterop.ExecuteFunctionNow(hleCallback.Function, hleCallback.Arguments);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
                finally
                {
                    //Console.WriteLine("               : PC=0x{0:X}", FakeCpuThreadState.PC);
                    executedCount++;
                }

                //Console.Error.WriteLine("  CALLBACK ENDED : " + HleCallback);
                if (mustReschedule)
                {
                    //Console.Error.WriteLine("    RESCHEDULE");
                    break;
                }
            }

            executedCount += _hleInterop.ExecuteAllQueuedFunctionsNow();
            //Console.Error.WriteLine("ENDED CALLBACKS");

            return executedCount;
        }

        void IMemoryStickEventHandler.ScheduleCallback(int callbackId, int arg1, int arg2)
        {
            var callback = Callbacks.Get(callbackId);
            ScheduleCallback(callback.Clone().PrependArguments(arg1, arg2));
            //Console.WriteLine("IMemoryStickEventHandler.ScheduleCallback");
        }
    }
}
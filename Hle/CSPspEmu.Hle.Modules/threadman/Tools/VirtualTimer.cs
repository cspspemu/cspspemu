using CSharpUtils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Components.Rtc;
using CSPspEmu.Hle.Interop;

namespace CSPspEmu.Hle.Modules.threadman
{
    [HleUidPoolClass(NotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_VTIMER)]
    public unsafe class VirtualTimer : IHleUidPoolClass, IDisposable
    {
        [Inject] PspRtc PspRtc;

        [Inject] HleMemoryManager MemoryManager;

        [Inject] CpuProcessor CpuProcessor;

        [Inject] PspMemory Memory;

        [Inject] HleInterop HleInterop;

        protected PspVirtualTimer Timer;
        public int Id;
        public string Name;
        public ThreadManForUser.SceKernelVTimerOptParam SceKernelVTimerOptParam;
        protected long PreviousUpdatedTime;
        protected long CurrentUpdatedTime;
        protected long ElapsedAccumulatedTime;

        protected bool HandlerEnabled;
        protected long HandlerTime;
        protected bool HandlerIsWide;
        protected PspPointer HandlerCallback;
        protected PspPointer HandlerArgument;

        public struct PspSharedInfoStruct
        {
            public SceKernelSysClock ElapsedScheduled;
            public SceKernelSysClock ElapsedReal;
        }

        MemoryPartition PspSharedInfoMemoryPartition;
        PspSharedInfoStruct* PspSharedInfo;

        public VirtualTimer(InjectContext InjectContext, string Name)
        {
            InjectContext.InjectDependencesTo(this);

            this.Timer = PspRtc.CreateVirtualTimer(Handler);
            this.Name = Name;
            this.Timer.Enabled = false;
            this.PspSharedInfoMemoryPartition = MemoryManager.GetPartition(MemoryPartitions.Kernel0).Allocate(
                sizeof(PspSharedInfoStruct),
                Name: "VTimer.PspSharedInfoStruct"
            );
            this.PspSharedInfo =
                (PspSharedInfoStruct*) CpuProcessor.Memory.PspAddressToPointerSafe(
                    this.PspSharedInfoMemoryPartition.Low);
        }

        public void Dispose()
        {
            this.PspSharedInfoMemoryPartition.DeallocateFromParent();
        }

        public void Handler()
        {
            Console.Error.WriteLine("Handler!!");
            if (HandlerEnabled)
            {
                UpdateElapsedTime(true);
                PspSharedInfo->ElapsedScheduled.MicroSeconds = HandlerTime;
                PspSharedInfo->ElapsedReal.MicroSeconds = ElapsedAccumulatedTime;

                uint Result = HleInterop.ExecuteFunctionNow(
                    HandlerCallback,
                    Id,
                    Memory.PointerToPspAddressSafe(&PspSharedInfo->ElapsedScheduled),
                    Memory.PointerToPspAddressSafe(&PspSharedInfo->ElapsedReal),
                    HandlerArgument
                );
                Console.Error.WriteLine("Handler ENABLED!! {0}", Result);
            }
        }

        public void UpdateElapsedTime(bool Increment)
        {
            PspRtc.Update();
            this.CurrentUpdatedTime = PspRtc.Elapsed.GetTotalMicroseconds();
            {
                if (Increment)
                {
                    this.ElapsedAccumulatedTime += (this.CurrentUpdatedTime - this.PreviousUpdatedTime);
                }
            }
            this.PreviousUpdatedTime = this.CurrentUpdatedTime;
        }

        public long ElapsedMicroseconds
        {
            get
            {
                lock (Timer)
                {
                    UpdateElapsedTime(this.Timer.Enabled);
                    return this.ElapsedAccumulatedTime;
                }
            }
        }

        public void CancelHandler()
        {
            lock (Timer)
            {
                this.HandlerEnabled = false;
            }
        }

        protected void UpdateHandlerTime()
        {
            PspRtc.Update();
            Console.Error.WriteLine("UpdateHandlerTime: {0}", this.HandlerTime - ElapsedAccumulatedTime);
            this.Timer.DateTime = PspRtc.CurrentDateTime +
                                  TimeSpanUtils.FromMicroseconds(this.HandlerTime - ElapsedAccumulatedTime);
        }

        public void SetHandler(long Time, PspPointer HandlerCallback, PspPointer HandlerArgument, bool HandlerIsWide)
        {
            lock (Timer)
            {
                this.HandlerTime = Time;
                this.HandlerCallback = HandlerCallback;
                this.HandlerArgument = HandlerArgument;
                this.HandlerIsWide = HandlerIsWide;
                this.HandlerEnabled = true;
                UpdateHandlerTime();
            }
        }

        public void Start()
        {
            lock (Timer)
            {
                if (!this.Timer.Enabled)
                {
                    UpdateElapsedTime(false);
                    UpdateHandlerTime();
                    this.Timer.Enabled = true;
                }
            }
        }

        public void Stop()
        {
            lock (Timer)
            {
                if (this.Timer.Enabled)
                {
                    this.Timer.Enabled = false;
                    UpdateElapsedTime(false);
                }
            }
        }
    }
}
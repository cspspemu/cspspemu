using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Interop;

namespace CSPspEmu.Hle.Managers
{
    public class HleSubinterruptHandler
    {
        public bool Enabled;
        public int Index;
        public uint Address;
        public uint Argument;

        public HleSubinterruptHandler(int index)
        {
            Index = index;
            Enabled = false;
        }
    }

    public sealed class HleInterruptHandler
    {
        public PspInterrupts PspInterrupt;
        public HleSubinterruptHandler[] SubinterruptHandlers;
        private HleCallbackManager _hleCallbackManager;
        private HleInterruptManager _hleInterruptManager;

        internal HleInterruptHandler(HleInterruptManager hleInterruptManager, PspInterrupts pspInterrupt,
            HleCallbackManager hleCallbackManager)
        {
            _hleInterruptManager = hleInterruptManager;
            PspInterrupt = pspInterrupt;
            _hleCallbackManager = hleCallbackManager;
            SubinterruptHandlers = new HleSubinterruptHandler[16];
            for (int index = 0; index < SubinterruptHandlers.Length; index++)
            {
                SubinterruptHandlers[index] = new HleSubinterruptHandler(index);
            }
        }

        public void Trigger()
        {
            if (_hleInterruptManager.Enabled)
            {
                foreach (var handler in SubinterruptHandlers.Where(handler => handler.Enabled))
                {
                    _hleInterruptManager.Queue(HleCallback.Create("InterruptTrigger", handler.Address, handler.Index,
                        handler.Argument));
                }
            }
            //Console.Error.WriteLine("Trigger: " + PspInterrupt);
        }
    }

    public sealed class HleInterruptManager : IInterruptManager
    {
        [Inject] private HleCallbackManager _hleCallbackManager;

        [Inject] private CpuProcessor _cpuProcessor;

        [Inject] private HleInterop _hleInterop;

        /// <summary>
        /// Global Interrupt Enable
        /// </summary>
        public bool Enabled
        {
            get => _cpuProcessor.InterruptEnabled;
            set => _cpuProcessor.InterruptEnabled = value;
        }
        //public bool Enabled;

        /// <summary>
        /// Global Interrupt Flag
        /// </summary>
        public bool Flag
        {
            get => _cpuProcessor.InterruptFlag;
            set => _cpuProcessor.InterruptFlag = value;
        }
        //public bool Flag;

        /// <summary>
        /// 
        /// </summary>
        private readonly HleInterruptHandler[] _interruptHandlers = new HleInterruptHandler[(int) PspInterrupts.Max];

        public HleInterruptHandler GetInterruptHandler(PspInterrupts pspInterrupt) => _interruptHandlers[(int) pspInterrupt];

        private HleInterruptManager()
        {
            for (var n = 0; n < _interruptHandlers.Length; n++)
            {
                _interruptHandlers[n] = new HleInterruptHandler(
                    this,
                    (PspInterrupts) n,
                    _hleCallbackManager
                );
            }
        }

        List<HleCallback> _hleCallbackList = new List<HleCallback>();

        public void Queue(HleCallback hleCallback)
        {
            lock (_hleCallbackList)
            {
                _hleCallbackList.Add(hleCallback);
                Flag = true;
            }
        }

        void IInterruptManager.Interrupt(CpuThreadState cpuThreadState)
        {
            ExecuteQueued(cpuThreadState);
        }

        public void ExecuteQueued(CpuThreadState baseCpuThreadState)
        {
            if (Enabled)
            {
                HleCallback[] hleCallbackListCopy;
                lock (_hleCallbackList)
                {
                    hleCallbackListCopy = _hleCallbackList.ToArray();
                    _hleCallbackList.Clear();
                    Flag = false;
                }

                foreach (var hleCallback in hleCallbackListCopy)
                {
                    var fakeCpuThreadState = new CpuThreadState(_cpuProcessor);
                    fakeCpuThreadState.CopyRegistersFrom(baseCpuThreadState);
                    hleCallback.SetArgumentsToCpuThreadState(fakeCpuThreadState);

                    fakeCpuThreadState.EnableYielding = false;
                    fakeCpuThreadState.ExecuteAt(fakeCpuThreadState.PC);
                    //HleInterop.Execute(FakeCpuThreadState);
                    //Console.Error.WriteLine("Execute queued");

                    // Execute just one!
                    //break;
                }
            }
        }

        public uint SceKernelCpuSuspendIntr()
        {
            try
            {
                return (uint) (Enabled ? 1 : 0);
            }
            finally
            {
                Enabled = false;
            }
        }

        public void SceKernelCpuResumeIntr(uint flags)
        {
            //if (set != true) throw new NotImplementedException();
            Enabled = (flags != 0);
        }
    }

    public enum PspInterrupts : uint
    {
        PspGpioInt = 4,
        PspAtaInt = 5,
        PspUmdInt = 6,
        PspMscm0Int = 7,
        PspWlanInt = 8,
        PspAudioInt = 10,
        PspI2CInt = 12,
        PspSircsInt = 14,
        PspSystimer0Int = 15, // Calls to register or enable on these interrupts always yield 0x80020065 (illegal intr code), which seems plausible if they are system interrupts. QueryIntrHandlerInfo yields the following interesting information:
        PspSystimer1Int = 16,
        PspSystimer2Int = 17,
        PspSystimer3Int = 18,
        PspThread0Int = 19,
        PspNandInt = 20,
        PspDmacplusInt = 21,
        PspDma0Int = 22,
        PspDma1Int = 23,
        PspMemlmdInt = 24,
        PspGeInt = 25,

        /// <summary>
        /// The vblank interrupt triggers every 1/60 second. Using the following function: 
        /// 
        ///		int sceKernelRegisterSubIntrHandler(int intno, int no, void* handler, void* arg);
        ///		
        /// up to 16 individual subinterrupt handlers may be installed for the vblank interrupt (intno = PSP_VBLANK_INT, no = 0 - 15).
        /// The prototype for vblank handler functions is: 
        /// 
        ///		void vblank_handler(int no, void* arg);
        /// </summary>
        PspVblankInt = 30,
        PspMecodecInt = 31,
        PspHpremoteInt = 36,
        PspMscm1Int = 60,
        PspMscm2Int = 61,
        PspThread1Int = 65,
        PspInterruptInt = 66,
        Max = 67,
    }

    /*
    public enum PspSubInterrupts : uint
    {
        PSP_GPIO_SUBINT = PspInterrupts.PSP_GPIO_INT,
        PSP_ATA_SUBINT = PspInterrupts.PSP_ATA_INT,
        PSP_UMD_SUBINT = PspInterrupts.PSP_UMD_INT,
        PSP_DMACPLUS_SUBINT = PspInterrupts.PSP_DMACPLUS_INT,
        PSP_GE_SUBINT = PspInterrupts.PSP_GE_INT,
        PSP_DISPLAY_SUBINT = PspInterrupts.PSP_VBLANK_INT,
    }
    */

    public struct PspIntrHandlerOptionParam
    {
        public int Size;
        public uint Entry;
        public uint Common;
        public uint Gp;
        public ushort IntrCode;
        public ushort SubCount;
        public ushort IntrLevel;
        public ushort Enabled;
        public uint Calls;
        public uint Field_1C;
        public uint TotalClockLo;
        public uint TotalClockHi;
        public uint MinClockLo;
        public uint MinClockHi;
        public uint MaxClockLo;
        public uint MaxClockHi;
    }
}
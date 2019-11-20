//#define DEBUG_FUNCTION_CREATION

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu.VFpu;
using CSPspEmu.Core.Cpu.InstructionCache;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Memory;
using CSharpUtils;
using CSPspEmu.Core.Cpu.Emitter;
using System.Runtime;

namespace CSPspEmu.Core.Cpu
{
    internal unsafe delegate void* GetMemoryPtrSafeWithErrorDelegate(uint address, string errorDescription,
        bool canBeNull);

    internal unsafe delegate void* GetMemoryPtrNotNullDelegate(uint address);

    public sealed unsafe class CpuThreadState
    {
        public static readonly CpuThreadState Methods = new CpuThreadState();

        public CpuProcessor CpuProcessor;

        //
        public PspMemory Memory => CpuProcessor.Memory;

        public MethodCache MethodCache;

        public object CallerModule;

        public int StepInstructionCount;
        public long TotalInstructionCount;

        /// <summary>
        /// Las Valid Registered PC
        /// </summary>
        public uint LastValidPc = 0xFFFFFFFF;

        /// <summary>
        /// Current PC
        /// </summary>
        public uint Pc;
        //public uint nPC;

        /// <summary>
        /// LOw, HIgh registers.
        /// Used for mult/div.
        /// </summary>
        public int Lo, Hi;

        public long HiLo
        {
            
            get
            {
                fixed (int* loPtr = &Lo) return *(long*) loPtr;
            }
            
            set
            {
                fixed (int* loPtr = &Lo) *(long*) loPtr = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public uint Ic;

        /// <summary>
        /// 
        /// </summary>
        public bool BranchFlag;

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // GPR: General Purporse Registers
        // FPR: Floating Point Registers
        // VFR: Vfpu registers
        // C0R: Cop0 registers
        // VFR_CC: Vfpu comparison flags
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public uint Gpr0,
            Gpr1,
            Gpr2,
            Gpr3,
            Gpr4,
            Gpr5,
            Gpr6,
            Gpr7,
            Gpr8,
            Gpr9,
            Gpr10,
            Gpr11,
            Gpr12,
            Gpr13,
            Gpr14,
            Gpr15,
            Gpr16,
            Gpr17,
            Gpr18,
            Gpr19,
            Gpr20,
            Gpr21,
            Gpr22,
            Gpr23,
            Gpr24,
            Gpr25,
            Gpr26,
            Gpr27,
            Gpr28,
            Gpr29,
            Gpr30,
            Gpr31;

        public float Fpr0,
            Fpr1,
            Fpr2,
            Fpr3,
            Fpr4,
            Fpr5,
            Fpr6,
            Fpr7,
            Fpr8,
            Fpr9,
            Fpr10,
            Fpr11,
            Fpr12,
            Fpr13,
            Fpr14,
            Fpr15,
            Fpr16,
            Fpr17,
            Fpr18,
            Fpr19,
            Fpr20,
            Fpr21,
            Fpr22,
            Fpr23,
            Fpr24,
            Fpr25,
            Fpr26,
            Fpr27,
            Fpr28,
            Fpr29,
            Fpr30,
            Fpr31;

        public uint C0R0,
            C0R1,
            C0R2,
            C0R3,
            C0R4,
            C0R5,
            C0R6,
            C0R7,
            C0R8,
            C0R9,
            C0R10,
            C0R11,
            C0R12,
            C0R13,
            C0R14,
            C0R15,
            C0R16,
            C0R17,
            C0R18,
            C0R19,
            C0R20,
            C0R21,
            C0R22,
            C0R23,
            C0R24,
            C0R25,
            C0R26,
            C0R27,
            C0R28,
            C0R29,
            C0R30,
            C0R31;

        public float Vfr0,
            Vfr1,
            Vfr2,
            Vfr3,
            Vfr4,
            Vfr5,
            Vfr6,
            Vfr7,
            Vfr8,
            Vfr9,
            Vfr10,
            Vfr11,
            Vfr12,
            Vfr13,
            Vfr14,
            Vfr15,
            Vfr16,
            Vfr17,
            Vfr18,
            Vfr19,
            Vfr20,
            Vfr21,
            Vfr22,
            Vfr23,
            Vfr24,
            Vfr25,
            Vfr26,
            Vfr27,
            Vfr28,
            Vfr29,
            Vfr30,
            Vfr31,
            Vfr32,
            Vfr33,
            Vfr34,
            Vfr35,
            Vfr36,
            Vfr37,
            Vfr38,
            Vfr39,
            Vfr40,
            Vfr41,
            Vfr42,
            Vfr43,
            Vfr44,
            Vfr45,
            Vfr46,
            Vfr47,
            Vfr48,
            Vfr49,
            Vfr50,
            Vfr51,
            Vfr52,
            Vfr53,
            Vfr54,
            Vfr55,
            Vfr56,
            Vfr57,
            Vfr58,
            Vfr59,
            Vfr60,
            Vfr61,
            Vfr62,
            Vfr63,
            Vfr64,
            Vfr65,
            Vfr66,
            Vfr67,
            Vfr68,
            Vfr69,
            Vfr70,
            Vfr71,
            Vfr72,
            Vfr73,
            Vfr74,
            Vfr75,
            Vfr76,
            Vfr77,
            Vfr78,
            Vfr79,
            Vfr80,
            Vfr81,
            Vfr82,
            Vfr83,
            Vfr84,
            Vfr85,
            Vfr86,
            Vfr87,
            Vfr88,
            Vfr89,
            Vfr90,
            Vfr91,
            Vfr92,
            Vfr93,
            Vfr94,
            Vfr95,
            Vfr96,
            Vfr97,
            Vfr98,
            Vfr99,
            Vfr100,
            Vfr101,
            Vfr102,
            Vfr103,
            Vfr104,
            Vfr105,
            Vfr106,
            Vfr107,
            Vfr108,
            Vfr109,
            Vfr110,
            Vfr111,
            Vfr112,
            Vfr113,
            Vfr114,
            Vfr115,
            Vfr116,
            Vfr117,
            Vfr118,
            Vfr119,
            Vfr120,
            Vfr121,
            Vfr122,
            Vfr123,
            Vfr124,
            Vfr125,
            Vfr126,
            Vfr127;

        //var name = 'C0R'; for (var n = 0; n < 32; n += 4) console.log('nameof(' + name + (n) + '), nameof(' + name + (n + 1) + '), nameof(' + name + (n + 2) + '), nameof(' + name + (n + 3) + '),');
        public static readonly string[] C0RNames =
        {
            nameof(C0R0), nameof(C0R1), nameof(C0R2), nameof(C0R3),
            nameof(C0R4), nameof(C0R5), nameof(C0R6), nameof(C0R7),
            nameof(C0R8), nameof(C0R9), nameof(C0R10), nameof(C0R11),
            nameof(C0R12), nameof(C0R13), nameof(C0R14), nameof(C0R15),
            nameof(C0R16), nameof(C0R17), nameof(C0R18), nameof(C0R19),
            nameof(C0R20), nameof(C0R21), nameof(C0R22), nameof(C0R23),
            nameof(C0R24), nameof(C0R25), nameof(C0R26), nameof(C0R27),
            nameof(C0R28), nameof(C0R29), nameof(C0R30), nameof(C0R31),
        };

        public static readonly string[] GprNames =
        {
            nameof(Gpr0), nameof(Gpr1), nameof(Gpr2), nameof(Gpr3),
            nameof(Gpr4), nameof(Gpr5), nameof(Gpr6), nameof(Gpr7),
            nameof(Gpr8), nameof(Gpr9), nameof(Gpr10), nameof(Gpr11),
            nameof(Gpr12), nameof(Gpr13), nameof(Gpr14), nameof(Gpr15),
            nameof(Gpr16), nameof(Gpr17), nameof(Gpr18), nameof(Gpr19),
            nameof(Gpr20), nameof(Gpr21), nameof(Gpr22), nameof(Gpr23),
            nameof(Gpr24), nameof(Gpr25), nameof(Gpr26), nameof(Gpr27),
            nameof(Gpr28), nameof(Gpr29), nameof(Gpr30), nameof(Gpr31),
        };

        public static readonly string[] FprNames =
        {
            nameof(Fpr0), nameof(Fpr1), nameof(Fpr2), nameof(Fpr3),
            nameof(Fpr4), nameof(Fpr5), nameof(Fpr6), nameof(Fpr7),
            nameof(Fpr8), nameof(Fpr9), nameof(Fpr10), nameof(Fpr11),
            nameof(Fpr12), nameof(Fpr13), nameof(Fpr14), nameof(Fpr15),
            nameof(Fpr16), nameof(Fpr17), nameof(Fpr18), nameof(Fpr19),
            nameof(Fpr20), nameof(Fpr21), nameof(Fpr22), nameof(Fpr23),
            nameof(Fpr24), nameof(Fpr25), nameof(Fpr26), nameof(Fpr27),
            nameof(Fpr28), nameof(Fpr29), nameof(Fpr30), nameof(Fpr31),
        };

        public static readonly string[] VfrNames =
        {
            nameof(Vfr0), nameof(Vfr1), nameof(Vfr2), nameof(Vfr3),
            nameof(Vfr4), nameof(Vfr5), nameof(Vfr6), nameof(Vfr7),
            nameof(Vfr8), nameof(Vfr9), nameof(Vfr10), nameof(Vfr11),
            nameof(Vfr12), nameof(Vfr13), nameof(Vfr14), nameof(Vfr15),
            nameof(Vfr16), nameof(Vfr17), nameof(Vfr18), nameof(Vfr19),
            nameof(Vfr20), nameof(Vfr21), nameof(Vfr22), nameof(Vfr23),
            nameof(Vfr24), nameof(Vfr25), nameof(Vfr26), nameof(Vfr27),
            nameof(Vfr28), nameof(Vfr29), nameof(Vfr30), nameof(Vfr31),
            nameof(Vfr32), nameof(Vfr33), nameof(Vfr34), nameof(Vfr35),
            nameof(Vfr36), nameof(Vfr37), nameof(Vfr38), nameof(Vfr39),
            nameof(Vfr40), nameof(Vfr41), nameof(Vfr42), nameof(Vfr43),
            nameof(Vfr44), nameof(Vfr45), nameof(Vfr46), nameof(Vfr47),
            nameof(Vfr48), nameof(Vfr49), nameof(Vfr50), nameof(Vfr51),
            nameof(Vfr52), nameof(Vfr53), nameof(Vfr54), nameof(Vfr55),
            nameof(Vfr56), nameof(Vfr57), nameof(Vfr58), nameof(Vfr59),
            nameof(Vfr60), nameof(Vfr61), nameof(Vfr62), nameof(Vfr63),
            nameof(Vfr64), nameof(Vfr65), nameof(Vfr66), nameof(Vfr67),
            nameof(Vfr68), nameof(Vfr69), nameof(Vfr70), nameof(Vfr71),
            nameof(Vfr72), nameof(Vfr73), nameof(Vfr74), nameof(Vfr75),
            nameof(Vfr76), nameof(Vfr77), nameof(Vfr78), nameof(Vfr79),
            nameof(Vfr80), nameof(Vfr81), nameof(Vfr82), nameof(Vfr83),
            nameof(Vfr84), nameof(Vfr85), nameof(Vfr86), nameof(Vfr87),
            nameof(Vfr88), nameof(Vfr89), nameof(Vfr90), nameof(Vfr91),
            nameof(Vfr92), nameof(Vfr93), nameof(Vfr94), nameof(Vfr95),
            nameof(Vfr96), nameof(Vfr97), nameof(Vfr98), nameof(Vfr99),
            nameof(Vfr100), nameof(Vfr101), nameof(Vfr102), nameof(Vfr103),
            nameof(Vfr104), nameof(Vfr105), nameof(Vfr106), nameof(Vfr107),
            nameof(Vfr108), nameof(Vfr109), nameof(Vfr110), nameof(Vfr111),
            nameof(Vfr112), nameof(Vfr113), nameof(Vfr114), nameof(Vfr115),
            nameof(Vfr116), nameof(Vfr117), nameof(Vfr118), nameof(Vfr119),
            nameof(Vfr120), nameof(Vfr121), nameof(Vfr122), nameof(Vfr123),
            nameof(Vfr124), nameof(Vfr125), nameof(Vfr126), nameof(Vfr127),
        };

        public static readonly string[] VfrCcNames =
        {
            nameof(VfrCc0),
            nameof(VfrCc1),
            nameof(VfrCc2),
            nameof(VfrCc3),
            nameof(VfrCc4),
            nameof(VfrCc5),
            nameof(VfrCc6),
            nameof(VfrCc7),
        };

        public bool VfrCc0, VfrCc1, VfrCc2, VfrCc3, VfrCc4, VfrCc5, VfrCc6, VfrCc7;

        public bool VfrCcAny => VfrCc4;

        public bool VfrCcAll => VfrCc5;

        public uint VfrCcValue
        {
            get
            {
                uint value = 0;
                fixed (bool* vfrCc = &VfrCc0)
                {
                    for (int n = 0; n < 8; n++) value |= (uint) (vfrCc[n] ? 1 << n : 0);
                }
                return value;
            }
            set
            {
                fixed (bool* vfrCc = &VfrCc0)
                {
                    for (int n = 0; n < 8; n++)
                    {
                        vfrCc[n] = ((value >> n) & 1) != 0;
                    }
                }
            }
        }

        public VfpuPrefix PrefixNone = new VfpuPrefix();
        public VfpuPrefix PrefixSource = new VfpuPrefix();
        public VfpuDestinationPrefix PrefixDestination = new VfpuDestinationPrefix();
        public VfpuPrefix PrefixTarget = new VfpuPrefix();

        public Random Random = new Random();

        public Fcr31Struct Fcr31;

        public readonly uint[] CallStack = new uint[10240];
        public int CallStackCount;

        public uint[] GetCurrentCallStack()
        {
            var Out = new List<uint>();
            var count = Math.Min(10240, CallStackCount);
            for (var n = 0; n < count; n++)
                Out.Add(CallStack[(CallStackCount - n - 1) % CallStack.Length]);
            return Out.ToArray();
        }

        public void CallStackPush(uint pc)
        {
            if (CallStackCount >= 0 && CallStackCount < CallStack.Length)
                CallStack[CallStackCount] = pc;
            CallStackCount++;
        }

        public void CallStackPop()
        {
            if (CallStackCount > 0) CallStackCount--;
        }

        // http://msdn.microsoft.com/en-us/library/ms253512(v=vs.80).aspx
        // http://logos.cs.uic.edu/366/notes/mips%20quick%20tutorial.htm

        /// <summary>
        /// Points to the middle of the 64K block of memory in the static data segment.
        /// </summary>
        public uint Gp
        {
            get => Gpr28;
            set => Gpr28 = value;
        }

        /// <summary>
        /// Points to last location on the stack.
        /// </summary>
        public uint Sp
        {
            get => Gpr29;
            set => Gpr29 = value;
        }

        /// <summary>
        /// Reserved for use by the interrupt/trap handler 
        /// </summary>
        public uint K0
        {
            get => Gpr26;
            set => Gpr26 = value;
        }

        /// <summary>
        /// saved value / frame pointer
        /// Preserved across procedure calls
        /// </summary>
        public uint Fp
        {
            get => Gpr30;
            set => Gpr30 = value;
        }

        /// <summary>
        /// Return Address
        /// </summary>
        public uint Ra
        {
            get => Gpr31;
            set => Gpr31 = value;
        }

        /// <summary>
        /// V0
        /// </summary>
        public uint V0
        {
            get => Gpr2;
            set => Gpr2 = value;
        }

        public GprList Gpr;
        public C0RList C0R;
        public FprList Fpr;
        public VfprList Vfpr;

        public FprListInteger FprI;
        //readonly public float* FPR;

        public void* GetMemoryPtr(uint address)
        {
            var pointer = Memory.PspAddressToPointerUnsafe(address);
            //Console.WriteLine("%08X".Sprintf((uint)Pointer));
            return pointer;
        }

        public void* GetMemoryPtrNotNull(uint address) => Memory.PspAddressToPointerNotNull(address);

        public void* GetMemoryPtrSafe(uint address) => Memory.PspAddressToPointerSafe(address);

        public void* GetMemoryPtrSafeWithError(uint address, string errorDescription, bool canBeNull,
            InvalidAddressAsEnum invalid)
        {
            //Console.Error.WriteLine("{0:X8}, {1}, {2}", Address, CanBeNull, InvalidAsNull);
            try
            {
                var result = Memory.PspAddressToPointerSafe(address, 0, canBeNull);
                /*
                if (Result == null && !CanBeNull)
                {
                    throw(new PspMemory.InvalidAddressException(""));
                }
                */
                return result;
            }
            catch (InvalidAddressException invalidAddressException)
            {
                if (invalid == InvalidAddressAsEnum.Null) return null;
                if (invalid == InvalidAddressAsEnum.InvalidAddress) return PspMemory.InvalidPointer;
                throw new InvalidAddressException(
                    $"GetMemoryPtrSafeWithError:{errorDescription} : {invalidAddressException.Message}",
                    invalidAddressException);
            }
            catch (Exception exception)
            {
                if (invalid == InvalidAddressAsEnum.Null) return null;
                if (invalid == InvalidAddressAsEnum.InvalidAddress) return PspMemory.InvalidPointer;
                throw new Exception($"GetMemoryPtrSafeWithError: {errorDescription} : {exception.Message}",
                    exception);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public IEnumerable<int> GprItems(params int[] indexes) => indexes.Select(index => Gpr[index]);

        private CpuThreadState()
        {
        }

        static public CpuThreadState Dummy;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processor"></param>
        public CpuThreadState(CpuProcessor processor)
        {
            CpuProcessor = processor;
            MethodCache = processor.MethodCache;
            //this.Memory = Processor.Memory;

            Gpr = new GprList {CpuThreadState = this};
            Fpr = new FprList {CpuThreadState = this};
            C0R = new C0RList {CpuThreadState = this};
            FprI = new FprListInteger {CpuThreadState = this};
            Vfpr = new VfprList {CpuThreadState = this};

            for (var n = 0; n < 32; n++) Gpr[n] = 0;
            for (var n = 0; n < 32; n++) Fpr[n] = 0.0f;

            VfrCc7 = VfrCc6 = VfrCc5 = VfrCc4 = VfrCc3 = VfrCc2 = VfrCc1 = VfrCc0 = true;

            for (var n = 0; n < 128; n++) Vfpr[n] = 0.0f;
        }

        /// <summary>
        /// Calls a syscall.
        /// </summary>
        /// <param name="code"></param>
        public void Syscall(int code) => CpuProcessor.Syscall(code, this);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegateId"></param>
        public void SyscallNative(uint delegateId) =>
            CpuProcessor.RegisteredNativeSyscallMethods[delegateId].PoolItem.Value(this);

        //private DateTime LastTick;
        private int _tickCount;

        DateTime _lastTickYield = DateTime.UtcNow;

        public bool EnableYielding = true;

        /// <summary>
        /// Function called on some situations, that allow
        /// to yield the thread.
        /// </summary>
        
        
        public void Tick()
        {
            //Console.WriteLine("Tick1");
            if (EnableYielding)
            {
                _tickCount++;

                if ((_tickCount & 0x1F) == 1)
                    //if ((TickCount & 3) == 1)
                {
                    Tick2();
                }
            }
        }

        private void Tick2()
        {
            CpuProcessor.ExecuteInterrupt(this);
            if (_tickCount > 10000)
            {
                _tickCount = 0;
                if ((DateTime.UtcNow - _lastTickYield).TotalMilliseconds >= 2)
                {
                    _lastTickYield = DateTime.UtcNow;
                    Yield();
                }
            }
        }

        public void Reschedule()
        {
            Yield();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Yield()
        {
            if (EnableYielding)
            {
                CpuProcessor.CpuConnector.Yield(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static MipsDisassembler _mipsDisassembler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pc"></param>
        public void Trace(uint pc)
        {
            if (_mipsDisassembler == null) _mipsDisassembler = new MipsDisassembler();
            var result = _mipsDisassembler.Disassemble(pc, Memory.Read4(pc));
            Console.WriteLine("  Trace: PC:0x{0:X8} : DATA:0x{1:X8} : {2}", pc, Memory.Read4(pc), result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/ms253512(v=vs.80).aspx"/>
        private static readonly string[] RegisterMnemonicNames =
        {
            "zr", "at", "v0", "v1", "a0", "a1", "a2", "a3",
            "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7",
            "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7",
            "t8", "t9", "k0", "k1", "gp", "sp", "fp", "ra",
        };

        /// <summary>
        /// 
        /// </summary>
        public void DumpRegisters()
        {
            DumpRegisters(Console.Out);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textWriter"></param>
        public void DumpRegistersCpu(TextWriter textWriter)
        {
            textWriter.WriteLine("PC: 0x{0:X8}, HI: 0x{1:X8}, LO: 0x{2:X8}", Pc, Hi, Lo);
            for (var n = 0; n < 32; n++)
            {
                if (n % 4 != 0) textWriter.Write(", ");
                textWriter.Write("r{0,2}({1}) : 0x{2:X8}", n, RegisterMnemonicNames[n], Gpr[n]);
                if (n % 4 == 3) textWriter.WriteLine();
            }
            textWriter.WriteLine();
        }

        public void DumpRegistersFpu(TextWriter textWriter)
        {
            for (var n = 0; n < 32; n++)
            {
                if (n % 4 != 0) textWriter.Write(", ");
                textWriter.Write("f{0,2} : 0x{1:X8}, {2}", n, FprI[n], Fpr[n]);
                if (n % 4 == 3) textWriter.WriteLine();
            }
            textWriter.WriteLine();
        }

        public void DumpRegistersVFpu(TextWriter textWriter)
        {
            for (var n = 0; n < 32; n++)
            {
                if (n % 4 != 0) textWriter.Write(", ");
                textWriter.Write("c0r{0,2} : 0x{1:X8}", n, C0R[n]);
                if (n % 4 == 3) textWriter.WriteLine();
            }
            textWriter.WriteLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textWriter"></param>
        public void DumpRegisters(TextWriter textWriter)
        {
            DumpRegistersCpu(textWriter);
            DumpRegistersFpu(textWriter);
            DumpRegistersVFpu(textWriter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textWriter"></param>
        public void DumpVfpuRegisters(TextWriter textWriter)
        {
            for (var matrix = 0; matrix < 8; matrix++)
            {
                textWriter.WriteLine("Matrix: {0}", matrix);
                for (var row = 0; row < 4; row++)
                {
                    var line = "";
                    for (var column = 0; column < 4; column++)
                    {
                        line += $", {Vfpr[matrix, column, row]}";
                    }
                    textWriter.WriteLine(line);
                }
                textWriter.WriteLine("");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        public void CopyRegistersFrom(CpuThreadState that)
        {
            Pc = that.Pc;
            Fcr31 = that.Fcr31;
            Ic = that.Ic;
            Lo = that.Lo;
            Hi = that.Hi;
            fixed (float* thisFpr = &Fpr0)
            fixed (float* thatFpr = &that.Fpr0)
            fixed (uint* thisGpr = &Gpr0)
            fixed (uint* thatGpr = &that.Gpr0)
            {
                for (var n = 0; n < 32; n++)
                {
                    thisFpr[n] = thatFpr[n];
                    thisGpr[n] = thatGpr[n];
                }
            }

            fixed (float* thisVfr = &Vfr0)
            fixed (float* thatVfr = &that.Vfr0)
            {
                for (var n = 0; n < 128; n++)
                    thisVfr[n] = thatVfr[n];
            }
        }

        public void ExecuteFunctionAndReturn(uint pc)
        {
            ExecuteAt(pc);
        }

        public void ExecuteAt(uint pc)
        {
            try
            {
                Pc = pc;
                Ra = SpecialCpu.ReturnFromFunction;
                MethodCache.GetForPc(Pc).CallDelegate(this);
            }
            catch (SpecialCpu.ReturnFromFunctionException)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodCacheInfo"></param>
        /// <param name="pc"></param>
        public void _MethodCacheInfo_SetInternal(MethodCacheInfo methodCacheInfo, uint pc) =>
            MethodCache._MethodCacheInfo_SetInternal(this, methodCacheInfo, pc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="pc"></param>
        public void SetPcWriteAddress(uint address, uint pc) => Memory.SetPCWriteAddress(address, pc);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Action<CpuThreadState> GetFuncAtPc(uint pc) => CpuProcessor.MethodCache.GetForPc(pc).CallDelegate;

        public struct Fcr31Struct
        {
            public enum TypeEnum : uint
            {
                Rint = 0,
                Cast = 1,
                Ceil = 2,
                Floor = 3,
            }

            private uint _value;

            public uint Value
            {
                get
                {
                    _value = BitUtils.Insert(_value, 0, 2, (uint) Rm);
                    _value = BitUtils.Insert(_value, 23, 1, (uint) (Cc ? 1 : 0));
                    _value = BitUtils.Insert(_value, 24, 1, (uint) (Fs ? 1 : 0));
                    return _value;
                }
                set
                {
                    _value = value;
                    Cc = BitUtils.Extract(value, 23, 1) != 0;
                    Fs = BitUtils.Extract(value, 24, 1) != 0;
                    Rm = (TypeEnum) BitUtils.Extract(value, 0, 2);
                }
            }

            public TypeEnum Rm;
            public bool Cc;
            public bool Fs;
        }

        public class GprList
        {
            public CpuThreadState CpuThreadState;

            private int NameToIndex(string name) => Array.IndexOf(RegisterMnemonicNames, name);

            public int this[string name]
            {
                get => this[NameToIndex(name)];
                set => this[NameToIndex(name)] = value;
            }

            public int this[int index]
            {
                get
                {
                    fixed (uint* ptr = &CpuThreadState.Gpr0) return (int) ptr[index];
                }
                set
                {
                    if (index == 0) return;
                    fixed (uint* ptr = &CpuThreadState.Gpr0) ptr[index] = (uint) value;
                }
            }
        }

        public class C0RList
        {
            public CpuThreadState CpuThreadState;

            public uint this[int index]
            {
                get
                {
                    fixed (uint* ptr = &CpuThreadState.C0R0) return ptr[index];
                }
                set
                {
                    fixed (uint* ptr = &CpuThreadState.C0R0) ptr[index] = value;
                }
            }
        }

        public class FprList
        {
            public CpuThreadState CpuThreadState;

            public float this[int index]
            {
                get
                {
                    fixed (float* ptr = &CpuThreadState.Fpr0) return ptr[index];
                }
                set
                {
                    fixed (float* ptr = &CpuThreadState.Fpr0) ptr[index] = value;
                }
            }
        }

        public class FprListInteger
        {
            public CpuThreadState CpuThreadState;

            public int this[int index]
            {
                get
                {
                    fixed (float* ptr = &CpuThreadState.Fpr0) return ((int*) ptr)[index];
                }
                set
                {
                    fixed (float* ptr = &CpuThreadState.Fpr0) ((int*) ptr)[index] = value;
                }
            }
        }

        public class VfprList
        {
            public CpuThreadState CpuThreadState;

            public float this[int index]
            {
                get
                {
                    fixed (float* ptr = &CpuThreadState.Vfr0) return ptr[index];
                }
                set
                {
                    fixed (float* ptr = &CpuThreadState.Vfr0) ptr[index] = value;
                }
            }

            public float this[int matrix, int column, int row]
            {
                get => this[VfpuUtils.GetIndexCell(matrix, column, row)];
                set => this[VfpuUtils.GetIndexCell(matrix, column, row)] = value;
            }

            public float[] this[string nameWithSufix]
            {
                get { return VfpuUtils.GetIndices(nameWithSufix).Select(item => this[item]).ToArray(); }
                set
                {
                    var indices = VfpuUtils.GetIndices(nameWithSufix);
                    for (var n = 0; n < value.Length; n++) this[indices[n]] = value[n];
                }
            }

            public float[] this[int size, string name]
            {
                get { return VfpuUtils.GetIndices(size, name).Select(item => this[item]).ToArray(); }
                set
                {
                    var indices = VfpuUtils.GetIndices(size, name);
                    for (var n = 0; n < value.Length; n++) this[indices[n]] = value[n];
                }
            }

            public void ClearAll(float value = 0f)
            {
                for (var n = 0; n < 128; n++) this[n] = value;
            }
        }
    }
}
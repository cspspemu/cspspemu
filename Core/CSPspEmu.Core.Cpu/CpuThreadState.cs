//#define DEBUG_FUNCTION_CREATION

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;
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
    unsafe delegate void* GetMemoryPtrSafeWithErrorDelegate(uint Address, string ErrorDescription, bool CanBeNull);

    unsafe delegate void* GetMemoryPtrNotNullDelegate(uint Address);

    public sealed unsafe partial class CpuThreadState
    {
        public static readonly CpuThreadState Methods = new CpuThreadState();

        public CpuProcessor CpuProcessor;

        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PspMemory Memory => CpuProcessor.Memory;

        public MethodCache MethodCache;

        public object CallerModule;

        public int StepInstructionCount;
        public long TotalInstructionCount;

        /// <summary>
        /// Las Valid Registered PC
        /// </summary>
        public uint LastValidPC = 0xFFFFFFFF;

        /// <summary>
        /// Current PC
        /// </summary>
        public uint PC;
        //public uint nPC;

        /// <summary>
        /// LOw, HIgh registers.
        /// Used for mult/div.
        /// </summary>
        public int LO, HI;

        public long HI_LO
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                fixed (int* LOPtr = &LO) return *(long*) LOPtr;
            }
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                fixed (int* LOPtr = &LO) *(long*) LOPtr = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public uint IC;

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
            GPR5,
            GPR6,
            GPR7,
            GPR8,
            GPR9,
            GPR10,
            GPR11,
            GPR12,
            GPR13,
            GPR14,
            GPR15,
            GPR16,
            GPR17,
            GPR18,
            GPR19,
            GPR20,
            GPR21,
            GPR22,
            GPR23,
            GPR24,
            GPR25,
            GPR26,
            GPR27,
            GPR28,
            GPR29,
            GPR30,
            GPR31;

        public float FPR0,
            FPR1,
            FPR2,
            FPR3,
            FPR4,
            FPR5,
            FPR6,
            FPR7,
            FPR8,
            FPR9,
            FPR10,
            FPR11,
            FPR12,
            FPR13,
            FPR14,
            FPR15,
            FPR16,
            FPR17,
            FPR18,
            FPR19,
            FPR20,
            FPR21,
            FPR22,
            FPR23,
            FPR24,
            FPR25,
            FPR26,
            FPR27,
            FPR28,
            FPR29,
            FPR30,
            FPR31;

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

        public float VFR0,
            VFR1,
            VFR2,
            VFR3,
            VFR4,
            VFR5,
            VFR6,
            VFR7,
            VFR8,
            VFR9,
            VFR10,
            VFR11,
            VFR12,
            VFR13,
            VFR14,
            VFR15,
            VFR16,
            VFR17,
            VFR18,
            VFR19,
            VFR20,
            VFR21,
            VFR22,
            VFR23,
            VFR24,
            VFR25,
            VFR26,
            VFR27,
            VFR28,
            VFR29,
            VFR30,
            VFR31,
            VFR32,
            VFR33,
            VFR34,
            VFR35,
            VFR36,
            VFR37,
            VFR38,
            VFR39,
            VFR40,
            VFR41,
            VFR42,
            VFR43,
            VFR44,
            VFR45,
            VFR46,
            VFR47,
            VFR48,
            VFR49,
            VFR50,
            VFR51,
            VFR52,
            VFR53,
            VFR54,
            VFR55,
            VFR56,
            VFR57,
            VFR58,
            VFR59,
            VFR60,
            VFR61,
            VFR62,
            VFR63,
            VFR64,
            VFR65,
            VFR66,
            VFR67,
            VFR68,
            VFR69,
            VFR70,
            VFR71,
            VFR72,
            VFR73,
            VFR74,
            VFR75,
            VFR76,
            VFR77,
            VFR78,
            VFR79,
            VFR80,
            VFR81,
            VFR82,
            VFR83,
            VFR84,
            VFR85,
            VFR86,
            VFR87,
            VFR88,
            VFR89,
            VFR90,
            VFR91,
            VFR92,
            VFR93,
            VFR94,
            VFR95,
            VFR96,
            VFR97,
            VFR98,
            VFR99,
            VFR100,
            VFR101,
            VFR102,
            VFR103,
            VFR104,
            VFR105,
            VFR106,
            VFR107,
            VFR108,
            VFR109,
            VFR110,
            VFR111,
            VFR112,
            VFR113,
            VFR114,
            VFR115,
            VFR116,
            VFR117,
            VFR118,
            VFR119,
            VFR120,
            VFR121,
            VFR122,
            VFR123,
            VFR124,
            VFR125,
            VFR126,
            VFR127;

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
            nameof(Gpr4), nameof(GPR5), nameof(GPR6), nameof(GPR7),
            nameof(GPR8), nameof(GPR9), nameof(GPR10), nameof(GPR11),
            nameof(GPR12), nameof(GPR13), nameof(GPR14), nameof(GPR15),
            nameof(GPR16), nameof(GPR17), nameof(GPR18), nameof(GPR19),
            nameof(GPR20), nameof(GPR21), nameof(GPR22), nameof(GPR23),
            nameof(GPR24), nameof(GPR25), nameof(GPR26), nameof(GPR27),
            nameof(GPR28), nameof(GPR29), nameof(GPR30), nameof(GPR31),
        };

        public static readonly string[] FprNames =
        {
            nameof(FPR0), nameof(FPR1), nameof(FPR2), nameof(FPR3),
            nameof(FPR4), nameof(FPR5), nameof(FPR6), nameof(FPR7),
            nameof(FPR8), nameof(FPR9), nameof(FPR10), nameof(FPR11),
            nameof(FPR12), nameof(FPR13), nameof(FPR14), nameof(FPR15),
            nameof(FPR16), nameof(FPR17), nameof(FPR18), nameof(FPR19),
            nameof(FPR20), nameof(FPR21), nameof(FPR22), nameof(FPR23),
            nameof(FPR24), nameof(FPR25), nameof(FPR26), nameof(FPR27),
            nameof(FPR28), nameof(FPR29), nameof(FPR30), nameof(FPR31),
        };

        public static readonly string[] VfrNames =
        {
            nameof(VFR0), nameof(VFR1), nameof(VFR2), nameof(VFR3),
            nameof(VFR4), nameof(VFR5), nameof(VFR6), nameof(VFR7),
            nameof(VFR8), nameof(VFR9), nameof(VFR10), nameof(VFR11),
            nameof(VFR12), nameof(VFR13), nameof(VFR14), nameof(VFR15),
            nameof(VFR16), nameof(VFR17), nameof(VFR18), nameof(VFR19),
            nameof(VFR20), nameof(VFR21), nameof(VFR22), nameof(VFR23),
            nameof(VFR24), nameof(VFR25), nameof(VFR26), nameof(VFR27),
            nameof(VFR28), nameof(VFR29), nameof(VFR30), nameof(VFR31),
            nameof(VFR32), nameof(VFR33), nameof(VFR34), nameof(VFR35),
            nameof(VFR36), nameof(VFR37), nameof(VFR38), nameof(VFR39),
            nameof(VFR40), nameof(VFR41), nameof(VFR42), nameof(VFR43),
            nameof(VFR44), nameof(VFR45), nameof(VFR46), nameof(VFR47),
            nameof(VFR48), nameof(VFR49), nameof(VFR50), nameof(VFR51),
            nameof(VFR52), nameof(VFR53), nameof(VFR54), nameof(VFR55),
            nameof(VFR56), nameof(VFR57), nameof(VFR58), nameof(VFR59),
            nameof(VFR60), nameof(VFR61), nameof(VFR62), nameof(VFR63),
            nameof(VFR64), nameof(VFR65), nameof(VFR66), nameof(VFR67),
            nameof(VFR68), nameof(VFR69), nameof(VFR70), nameof(VFR71),
            nameof(VFR72), nameof(VFR73), nameof(VFR74), nameof(VFR75),
            nameof(VFR76), nameof(VFR77), nameof(VFR78), nameof(VFR79),
            nameof(VFR80), nameof(VFR81), nameof(VFR82), nameof(VFR83),
            nameof(VFR84), nameof(VFR85), nameof(VFR86), nameof(VFR87),
            nameof(VFR88), nameof(VFR89), nameof(VFR90), nameof(VFR91),
            nameof(VFR92), nameof(VFR93), nameof(VFR94), nameof(VFR95),
            nameof(VFR96), nameof(VFR97), nameof(VFR98), nameof(VFR99),
            nameof(VFR100), nameof(VFR101), nameof(VFR102), nameof(VFR103),
            nameof(VFR104), nameof(VFR105), nameof(VFR106), nameof(VFR107),
            nameof(VFR108), nameof(VFR109), nameof(VFR110), nameof(VFR111),
            nameof(VFR112), nameof(VFR113), nameof(VFR114), nameof(VFR115),
            nameof(VFR116), nameof(VFR117), nameof(VFR118), nameof(VFR119),
            nameof(VFR120), nameof(VFR121), nameof(VFR122), nameof(VFR123),
            nameof(VFR124), nameof(VFR125), nameof(VFR126), nameof(VFR127),
        };

        public static readonly string[] VfrCcNames =
        {
            nameof(VFR_CC_0),
            nameof(VFR_CC_1),
            nameof(VFR_CC_2),
            nameof(VFR_CC_3),
            nameof(VFR_CC_4),
            nameof(VFR_CC_5),
            nameof(VFR_CC_6),
            nameof(VFR_CC_7),
        };

        public bool VFR_CC_0, VFR_CC_1, VFR_CC_2, VFR_CC_3, VFR_CC_4, VFR_CC_5, VFR_CC_6, VFR_CC_7;

        public bool VFR_CC_ANY => VFR_CC_4;

        public bool VFR_CC_ALL => VFR_CC_5;

        public uint VFR_CC_Value
        {
            get
            {
                uint Value = 0;
                fixed (bool* VFR_CC = &VFR_CC_0)
                {
                    for (int n = 0; n < 8; n++) Value |= (uint) (VFR_CC[n] ? (1 << n) : 0);
                }
                return Value;
            }
            set
            {
                fixed (bool* VFR_CC = &VFR_CC_0)
                {
                    for (int n = 0; n < 8; n++)
                    {
                        VFR_CC[n] = (((value >> n) & 1) != 0);
                    }
                }
            }
        }

        public VfpuPrefix PrefixNone = new VfpuPrefix();
        public VfpuPrefix PrefixSource = new VfpuPrefix();
        public VfpuDestinationPrefix PrefixDestination = new VfpuDestinationPrefix();
        public VfpuPrefix PrefixTarget = new VfpuPrefix();

        public Random Random = new Random();

        public FCR31 Fcr31;

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
        public uint GP
        {
            get => GPR28;
            set => GPR28 = value;
        }

        /// <summary>
        /// Points to last location on the stack.
        /// </summary>
        public uint SP
        {
            get => GPR29;
            set => GPR29 = value;
        }

        /// <summary>
        /// Reserved for use by the interrupt/trap handler 
        /// </summary>
        public uint K0
        {
            get => GPR26;
            set => GPR26 = value;
        }

        /// <summary>
        /// saved value / frame pointer
        /// Preserved across procedure calls
        /// </summary>
        public uint FP
        {
            get => GPR30;
            set => GPR30 = value;
        }

        /// <summary>
        /// Return Address
        /// </summary>
        public uint RA
        {
            get => GPR31;
            set => GPR31 = value;
        }

        /// <summary>
        /// V0
        /// </summary>
        public uint V0
        {
            get { return Gpr2; }
            set { Gpr2 = value; }
        }

        public GprList GPR;
        public C0rList C0R;
        public FprList FPR;
        public VfprList Vfpr;

        public FprListInteger FPR_I;
        //readonly public float* FPR;

        public void* GetMemoryPtr(uint address)
        {
            var pointer = Memory.PspAddressToPointerUnsafe(address);
            //Console.WriteLine("%08X".Sprintf((uint)Pointer));
            return pointer;
        }

        public void* GetMemoryPtrNotNull(uint address) => Memory.PspAddressToPointerNotNull(address);

        public void* GetMemoryPtrSafe(uint address) => Memory.PspAddressToPointerSafe(address, 0);

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
                throw (new InvalidAddressException(
                    $"GetMemoryPtrSafeWithError:{errorDescription} : {invalidAddressException.Message}",
                    invalidAddressException));
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
        public IEnumerable<int> GPRList(params int[] indexes) => indexes.Select(index => GPR[index]);

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

            GPR = new GprList {CpuThreadState = this};
            FPR = new FprList {CpuThreadState = this};
            C0R = new C0rList {CpuThreadState = this};
            FPR_I = new FprListInteger {CpuThreadState = this};
            Vfpr = new VfprList {CpuThreadState = this};

            for (var n = 0; n < 32; n++) GPR[n] = 0;
            for (var n = 0; n < 32; n++) FPR[n] = 0.0f;

            VFR_CC_7 = VFR_CC_6 = VFR_CC_5 = VFR_CC_4 = VFR_CC_3 = VFR_CC_2 = VFR_CC_1 = VFR_CC_0 = true;

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
        private int TickCount = 0;

        DateTime LastTickYield = DateTime.UtcNow;

        public bool EnableYielding = true;

        /// <summary>
        /// Function called on some situations, that allow
        /// to yield the thread.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void Tick()
        {
            //Console.WriteLine("Tick1");
            if (EnableYielding)
            {
                TickCount++;

                if ((TickCount & 0x1F) == 1)
                    //if ((TickCount & 3) == 1)
                {
                    Tick2();
                }
            }
        }

        private void Tick2()
        {
            CpuProcessor.ExecuteInterrupt(this);
            if (TickCount > 10000)
            {
                TickCount = 0;
                if ((DateTime.UtcNow - LastTickYield).TotalMilliseconds >= 2)
                {
                    LastTickYield = DateTime.UtcNow;
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
        static MipsDisassembler MipsDisassembler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pc"></param>
        public void Trace(uint pc)
        {
            if (MipsDisassembler == null) MipsDisassembler = new MipsDisassembler();
            var Result = MipsDisassembler.Disassemble(pc, Memory.Read4(pc));
            Console.WriteLine("  Trace: PC:0x{0:X8} : DATA:0x{1:X8} : {2}", pc, Memory.Read4(pc), Result);
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
            textWriter.WriteLine("PC: 0x{0:X8}, HI: 0x{1:X8}, LO: 0x{2:X8}", PC, HI, LO);
            for (var n = 0; n < 32; n++)
            {
                if (n % 4 != 0) textWriter.Write(", ");
                textWriter.Write("r{0,2}({1}) : 0x{2:X8}", n, RegisterMnemonicNames[n], GPR[n]);
                if (n % 4 == 3) textWriter.WriteLine();
            }
            textWriter.WriteLine();
        }

        public void DumpRegistersFpu(TextWriter textWriter)
        {
            for (var n = 0; n < 32; n++)
            {
                if (n % 4 != 0) textWriter.Write(", ");
                textWriter.Write("f{0,2} : 0x{1:X8}, {2}", n, FPR_I[n], FPR[n]);
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
            PC = that.PC;
            Fcr31 = that.Fcr31;
            IC = that.IC;
            LO = that.LO;
            HI = that.HI;
            fixed (float* thisFpr = &FPR0)
            fixed (float* thatFpr = &that.FPR0)
            fixed (uint* thisGpr = &Gpr0)
            fixed (uint* thatGpr = &that.Gpr0)
            {
                for (var n = 0; n < 32; n++)
                {
                    thisFpr[n] = thatFpr[n];
                    thisGpr[n] = thatGpr[n];
                }
            }

            fixed (float* thisVfr = &VFR0)
            fixed (float* thatVfr = &that.VFR0)
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
                RA = SpecialCpu.ReturnFromFunction;
                MethodCache.GetForPc(PC).CallDelegate(this);
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using CSPspEmu.Core.Gpu.Run;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Threading.Synchronization;
using CSharpUtils;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Gpu
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public struct GpuInstruction
    {
        public uint Instruction;

        public GpuOpCodes OpCode => (GpuOpCodes) ((Instruction >> 24) & 0xFF);

        public uint Params => Instruction & 0xFFFFFF;
    }

    public sealed unsafe class GpuDisplayList
    {
        private static readonly Logger Logger = Logger.GetLogger("Gpu");

        private static bool Debug = false;
        //private static bool Debug = true;

        public uint[] callstack = new uint[1024];
        public uint callstackIndex;

        /// <summary>
        /// 
        /// </summary>
        public struct OptionalParams
        {
            public int ContextAddress;
            public int StackDepth;
            public int StackAddress;
        }

        public GpuStats stats = new GpuStats();

        /// <summary>
        /// A value between 0 and 63 inclusive.
        /// </summary>
        public int Id;

        /// <summary>
        /// 
        /// </summary>
        public GpuProcessor GpuProcessor;

        /// <summary>
        /// 
        /// </summary>
        private volatile uint InstructionAddressStart;

        /// <summary>
        /// 
        /// </summary>
        [Obsolete] private volatile uint InstructionAddressCurrent;

        /// <summary>
        /// 
        /// </summary>
        private volatile uint InstructionAddressStall;

        private uint current4
        {
            get => InstructionAddressCurrent << 2;
            set => InstructionAddressCurrent = value >> 2;
        }

        private uint stall4
        {
            get => InstructionAddressStall << 2;
            set => InstructionAddressStall = value >> 2;
        }


        /// <summary>
        /// 
        /// </summary>
        AutoResetEvent StallAddressUpdated = new AutoResetEvent(false);

        /// <summary>
        /// 
        /// </summary>
        public GpuStateStruct* GpuStateStructPointer { get; set; }

        //volatile public uint InstructionAddressCurrent;

        /// <summary>
        /// Stack with the InstructionAddressCurrent for the CALL/RET opcodes.
        /// </summary>
        private readonly Stack<IntPtr> ExecutionStack = new Stack<IntPtr>();

        /// <summary>
        /// Current status of the DisplayList.
        /// </summary>
        public readonly WaitableStateMachine<DisplayListStatusEnum> Status =
            new WaitableStateMachine<DisplayListStatusEnum>();

        /// <summary>
        /// Indicates if the list can be used.
        /// </summary>
        public bool Available { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public OptionalParams pspGeListOptParam;

        /// <summary>
        /// 
        /// </summary>
        internal bool Done;

        /// <summary>
        /// 
        /// </summary>
        private GpuDisplayListRunner GpuDisplayListRunner;

        /// <summary>
        /// 
        /// </summary>
        public Stack<uint> CallStack = new Stack<uint>(0x10);

        internal PspMemory Memory;


        //PspWaitEvent OnFreed = new PspWaitEvent();
        //public enum Status2Enum
        //{
        //	Drawing,
        //	Free,
        //}
        //public readonly WaitableStateMachine<Status2Enum> Status2 = new WaitableStateMachine<Status2Enum>(Debug: false);

        public PspGeCallbackData Callbacks;
        public int CallbacksId;

        public SignalBehavior Signal;

        //Action[] InstructionSwitch = new Action[256];

        /// <summary>
        /// Constructor
        /// </summary>
        internal GpuDisplayList(PspMemory Memory, GpuProcessor GpuProcessor, int Id)
        {
            this.Memory = Memory;
            this.GpuProcessor = GpuProcessor;
            this.Id = Id;
            GlobalGpuState = GpuProcessor.GlobalGpuState;
            GpuDisplayListRunner = new GpuDisplayListRunner(this, GpuProcessor.GlobalGpuState);
        }

        public void SetInstructionAddressStartAndCurrent(uint value)
        {
            InstructionAddressCurrent = value & PspMemory.MemoryMask;
            InstructionAddressStart = value & PspMemory.MemoryMask;
        }

        public void SetInstructionAddressStall(uint value)
        {
            InstructionAddressStall = value & PspMemory.MemoryMask;
            if (InstructionAddressStall != 0 && !PspMemory.IsAddressValid(InstructionAddressStall))
            {
                throw new InvalidOperationException(string.Format("Invalid StallAddress! 0x{0}",
                    InstructionAddressStall));
            }
            if (Debug) Console.WriteLine("GpuDisplayList.SetInstructionAddressStall:{0:X8}", value);
            StallAddressUpdated.Set();
        }

        /// <summary>
        /// Executes this Display List.
        /// </summary>
        internal void Process()
        {
            Status.SetValue(DisplayListStatusEnum.Drawing);

            if (Debug)
                Console.WriteLine("Process() : {0} : 0x{1:X8} : 0x{2:X8} : 0x{3:X8}", Id,
                    InstructionAddressCurrent, InstructionAddressStart, InstructionAddressStall);

            Done = false;
            while (!Done)
            {
                if (InstructionAddressStall != 0 && InstructionAddressCurrent >= InstructionAddressStall)
                {
                    if (Debug)
                        Console.WriteLine(
                            "- STALLED --------------------------------------------------------------------");
                    Status.SetValue(DisplayListStatusEnum.Stalling);
                    while (!StallAddressUpdated.WaitOne(TimeSpan.FromSeconds(2)))
                    {
                        ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
                        {
                            Console.WriteLine("DisplayListQueue.GetCountLock(): {0}",
                                GpuProcessor.DisplayListQueue.GetCountLock());
                            Console.WriteLine("CurrentGpuDisplayList.Status: {0}", Status.ToStringDefault());
                        });
                        if (GpuProcessor.Syncing)
                        {
                            Done = true;
                            Status.SetValue(DisplayListStatusEnum.Completed);
                            return;
                        }
                    }
                }

                ProcessInstruction();
            }

            Status.SetValue(DisplayListStatusEnum.Completed);
        }

        internal GpuInstruction ReadInstructionAndMoveNext()
        {
            var value = Memory.ReadSafe<GpuInstruction>(InstructionAddressCurrent);
            InstructionAddressCurrent += 4;
            return value;
        }

        private GuPrimitiveType primBatchPrimitiveType = GuPrimitiveType.Invalid;
        private DisplayListStatusEnum status = DisplayListStatusEnum.Queued;

        private bool isStalled => stall4 != 0 && current4 >= stall4;

        private bool completed = false;

        private bool hasMoreInstructions => !completed && !this.isStalled;

        private void runUntilStall()
        {
            status = DisplayListStatusEnum.Drawing;
            while (hasMoreInstructions)
            {
                runUntilStallInner();
            }
        }

        private void runUntilStallInner()
        {
            var memory = Memory;
            //let showOpcodes = this.showOpcodes;
            var stall4 = this.stall4;
            var state = this.state;
            var totalCommandsLocal = 0;
            var current4 = this.current4;
            var localPrimCount = 0;
            var stats = this.stats;
            //let startTime = 0;
            if (stall4 == 0) stall4 = 0x7FFFFFFF;

            while (current4 < stall4)
            {
                totalCommandsLocal++;
                var instructionPC4 = current4++;
                var instruction = memory.Read4(instructionPC4 << 2);
                var op = (GpuOpCodes) ((instruction >> 24) & 0xFF);
                var p = instruction & 0xFFFFFF;

                if (totalCommandsLocal >= 30000)
                {
                    gpuHang();
                    totalCommandsLocal = 0;
                    break;
                }

                //if (dumpFrameCommands) dumpFrameCommandsList.push($"{Op[op]}:{addressToHex(p)}");

                switch (op)
                {
                    case GpuOpCodes.PRIM:
                    {
                        var rprimCount = 0;

                        this.current4 = current4;
                        localPrimCount++;
                        var primitiveType = (GuPrimitiveType) param3(p, 16);
                        if (this.primBatchPrimitiveType != primitiveType) finishPrimBatch();
                        if (this.prim(param24(p)) == PrimAction.FLUSH_PRIM)
                        {
                            finishPrimBatch();
                        }
                        current4 = this.current4;
                        //stats.primCount++;
                        break;
                    }
                    case GpuOpCodes.BEZIER:
                        finishPrimBatch();
                        bezier(param24(p));
                        break;
                    case GpuOpCodes.END:
                        finishPrimBatch();
                        gpuEnd();
                        complete();
                        goto BreakLoop;
                    case GpuOpCodes.TFLUSH:
                        gpuTextureFlush(state);
                        finishPrimBatch();
                        break;
                    case GpuOpCodes.TSYNC:
                        gpuTextureSync(state);
                        break;
                    case GpuOpCodes.NOP: break;
                    case GpuOpCodes.DUMMY: break;
                    case GpuOpCodes.JUMP:
                    case GpuOpCodes.CALL:
                        if (op == GpuOpCodes.CALL)
                        {
                            callstack[callstackIndex++] = (instructionPC4 << 2) + 4;
                            callstack[callstackIndex++] = (state.baseOffset >> 2) & PspMemory.MemoryMask;
                        }
                        current4 = (this.state.baseAddress + (param24(p) & ~3) >> 2) & PspMemory.MemoryMask;
                        break;
                    case GpuOpCodes.RET:
                        if (callstackIndex > 0 && callstackIndex < 1024)
                        {
                            state.baseOffset = callstack[--callstackIndex];
                            current4 = (callstack[--callstackIndex] >> 2) & PspMemory.MemoryMask;
                        }
                        else
                        {
                            Console.WriteLine("gpu callstack empty or overflow");
                        }
                        break;
                    case GpuOpCodes.FINISH:
                    {
                        Console.WriteLine("Not implemented: GPU FINISH");
                        //this.finish();
                        //let callback = this.gpu.callbacks.get(this.callbackId);
                        //if (callback && callback.cpuState && callback.finishFunction)
                        //{
                        //    this.cpuExecutor.execute(callback.cpuState, callback.finishFunction, [param24(p),
                        //        callback.finishArgument]);
                        //}
                        break;
                    }
                    case GpuOpCodes.SIGNAL:
                        Console.WriteLine("Not implemented: GPU SIGNAL");
                        break;

                    //case Op.PROJMATRIXNUMBER: console.log(state.projectionMatrix); break;
                    case GpuOpCodes.PROJ:
                        state.writeFloat(GpuOpCodes.PROJ, GpuOpCodes.PMS, float1(p));
                        break;
                    case GpuOpCodes.VIEW:
                        state.writeFloat(GpuOpCodes.VIEW, GpuOpCodes.VMS, float1(p));
                        break;
                    case GpuOpCodes.WORLD:
                        state.writeFloat(GpuOpCodes.WORLD, GpuOpCodes.WMS, float1(p));
                        break;
                    case GpuOpCodes.BONE:
                        state.writeFloat(GpuOpCodes.BONE, GpuOpCodes.BOFS, float1(p));
                        break;
                    case GpuOpCodes.TMATRIX:
                        state.writeFloat(GpuOpCodes.TMATRIX, GpuOpCodes.TMS, float1(p));
                        break;

                    // No invalidate prim
                    case GpuOpCodes.BASE:
                    case GpuOpCodes.IADDR:
                    case GpuOpCodes.VADDR:
                    case GpuOpCodes.OFFSET_ADDR:
                        break;

                    default:
                        if (state.data[op] != p) finishPrimBatch();
                        break;
                }
                state.data[op] = p;
            }

            BreakLoop:

            this.current4 = current4;

            this.stats.totalStalls++;
            this.stats.primCount = localPrimCount;
            this.stats.totalCommands += totalCommandsLocal;
            this.status = this.isStalled ? DisplayListStatusEnum.Stalling : DisplayListStatusEnum.Completed;
        }

        private PrimAction prim(uint u)
        {
            throw new NotImplementedException();
        }

        private void gpuTextureSync(object state)
        {
            throw new NotImplementedException();
        }

        private void gpuTextureFlush(object state)
        {
            throw new NotImplementedException();
        }

        private void gpuEnd()
        {
            throw new NotImplementedException();
        }

        private void bezier(uint u)
        {
            throw new NotImplementedException();
        }

        private void complete()
        {
            throw new NotImplementedException();
        }

        private void finishPrimBatch()
        {
            Console.WriteLine("Not implemented: finishPrimBatch");
        }

        private void gpuHang()
        {
            Console.WriteLine("Not implemented: gpuHang");
        }

        private static float float1(uint v)
        {
            return MathFloat.ReinterpretUIntAsFloat(v << 8);
        }

        private static uint param24(uint v)
        {
            return v;
        }

        private static uint param3(uint v, int offset)
        {
            return (v >> offset) & 0b111;
        }

        //private void ProcessInstruction()
        //{
        //    GpuDisplayListRunner.Pc = InstructionAddressCurrent;
        //    var instruction = ReadInstructionAndMoveNext();
        //    GpuDisplayListRunner.OpCode = instruction.OpCode;
        //    GpuDisplayListRunner.Params24 = instruction.Params;
        //
        //    //InstructionSwitch(GpuDisplayListRunner, instruction.OpCode, instruction.Params);
        //
        //    // https://github.com/jspspemu/jspspemu/blob/d9c3ebf40a91abfd891becd703f6e832e082f07c/source/src/core/gpu/gpu_core.ts#L112
        //   
        //    if (Debug)
        //    {
        //        var WritePC = Memory.GetPCWriteAddress(GpuDisplayListRunner.Pc);
        //
        //        Console.Error.WriteLine(
        //            "CODE(0x{0:X}-0x{1:X}) : PC(0x{2:X}) : {3} : 0x{4:X} : Done:{5}",
        //            InstructionAddressCurrent,
        //            InstructionAddressStall,
        //            WritePC,
        //            instruction.OpCode,
        //            instruction.Params,
        //            Done
        //        );
        //    }
        //}

        internal void JumpRelativeOffset(uint Address)
        {
            InstructionAddressCurrent = GpuStateStructPointer->GetAddressRelativeToBaseOffset(Address);
        }

        internal void JumpAbsolute(uint Address)
        {
            InstructionAddressCurrent = Address;
        }

        internal void CallRelativeOffset(uint Address)
        {
            CallStack.Push(InstructionAddressCurrent);
            CallStack.Push((uint) GpuStateStructPointer->BaseOffset);
            //CallStack.Push(InstructionAddressCurrent);
            JumpRelativeOffset(Address);
            //throw new NotImplementedException();
        }

        internal void Ret()
        {
            if (CallStack.Count > 0)
            {
                GpuStateStructPointer->BaseOffset = CallStack.Pop();
                JumpAbsolute(CallStack.Pop());
            }
            else
            {
                Console.Error.WriteLine("Stack is empty");
            }
        }

        public void GeListSync(Action NotifyOnceCallback)
        {
            //Thread.Sleep(200);
            //Status2.CallbackOnStateOnce(Status2Enum.Free, NotifyOnceCallback);
            Status.CallbackOnStateOnce(DisplayListStatusEnum.Completed, NotifyOnceCallback);
        }

        public void DoFinish(uint PC, uint Arg, bool ExecuteNow)
        {
            if (Debug) Console.WriteLine("FINISH: Arg:{0}", Arg);

            if (Callbacks.FinishFunction != 0)
            {
                GpuProcessor.Connector.Finish(PC, Callbacks, Arg, ExecuteNow);
            }
        }

        public void DoSignal(uint PC, uint Signal, SignalBehavior Behavior, bool ExecuteNow)
        {
            if (Debug) Console.WriteLine("SIGNAL : {0}: Behavior:{1}", Signal, Behavior);

            Status.SetValue(DisplayListStatusEnum.Paused);

            if (Callbacks.SignalFunction != 0)
            {
                //Console.Error.WriteLine("OP_SIGNAL! ({0}, {1})", Signal, Behavior);
                GpuProcessor.Connector.Signal(PC, Callbacks, Signal, Behavior, ExecuteNow);
            }

            Status.SetValue(DisplayListStatusEnum.Drawing);
        }

        public void SetQueued()
        {
            Status.SetValue(DisplayListStatusEnum.Queued);
        }

        public void SetDequeued()
        {
            //Status2.SetValue(Status2Enum.Dequeued);
        }

        public void SetFree()
        {
            //Status2.SetValue(Status2Enum.Free);
            Available = true;
        }

        public DisplayListStatusEnum PeekStatus()
        {
            return Status.Value;
        }

        public void DeQueue()
        {
            Done = true;
            GpuProcessor.DisplayListQueue.Remove(this);
        }
    }

    enum PrimAction
    {
        NOTHING = 0,
        FLUSH_PRIM = 1,
    }
}
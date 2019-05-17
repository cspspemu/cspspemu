#define PRIM_BATCH

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Threading.Synchronization;
using CSharpUtils;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu.VertexReading;

namespace CSPspEmu.Core.Gpu
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public struct GpuInstruction
    {
        public uint Instruction;

        public GpuOpCodes OpCode => (GpuOpCodes) ((Instruction >> 24) & 0xFF);

        public uint Params => ((Instruction) & 0xFFFFFF);
    }

    public sealed unsafe class GpuDisplayList
    {
        public static uint* DummyData = (uint *)Marshal.AllocHGlobal(GpuStateStruct.StructSizeInBytes);
        
        private static readonly Logger Logger = Logger.GetLogger("Gpu");

        private static bool Debug = false;
        //private static bool Debug = true;

        public struct OptionalParams
        {
            public int ContextAddress;
            public int StackDepth;
            public int StackAddress;
        }

        public int Id;
        public GpuProcessor GpuProcessor;
        private volatile uint InstructionAddressStart;
        private volatile uint InstructionAddressCurrent;
        private volatile uint InstructionAddressStall;
        AutoResetEvent StallAddressUpdated = new AutoResetEvent(false);
        public GpuStateStruct GpuStateStructPointer = new GpuStateStruct(new GpuStateData(DummyData)); 
        public GpuStateData GpuStateData => GpuStateStructPointer.data;
        private GlobalGpuState GlobalGpuState;
        private readonly Stack<IntPtr> ExecutionStack = new Stack<IntPtr>();
        public readonly WaitableStateMachine<DisplayListStatusEnum> Status = new WaitableStateMachine<DisplayListStatusEnum>();
        public bool Available { set; get; }
        public OptionalParams pspGeListOptParam;
        internal bool Done;
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
                throw (new InvalidOperationException($"Invalid StallAddress! 0x{InstructionAddressStall}"));
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
                if ((InstructionAddressStall != 0) && (InstructionAddressCurrent >= InstructionAddressStall))
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
            var Value = *(GpuInstruction*) Memory.PspAddressToPointerUnsafe(InstructionAddressCurrent);
            InstructionAddressCurrent += 4;
            return Value;
        }
        
        private void ProcessInstruction()
        {
            var Pc = InstructionAddressCurrent;
            var Instruction = ReadInstructionAndMoveNext();
            var Params24 = Instruction.Params;

            //InstructionSwitch(GpuDisplayListRunner, Instruction.OpCode, Instruction.Params);
            GpuStateStructPointer.data[Instruction.OpCode] = Instruction.Params;
            switch (Instruction.OpCode)
            {
                case GpuOpCodes.END:
                    Done = true;
                    GpuProcessor.GpuImpl.End(GpuStateStructPointer);
                    break;
                case GpuOpCodes.FINISH:
                    GpuProcessor.GpuImpl.Finish(GpuStateStructPointer);
                    DoFinish(InstructionAddressCurrent, Params24, ExecuteNow: true);
                    break;
                case GpuOpCodes.RET:
                    Ret();
                    break;
                case GpuOpCodes.CALL:
                    CallRelativeOffset((uint) (Params24 & ~3));
                    break;
                case GpuOpCodes.JUMP:
                    JumpRelativeOffset((uint) (Params24 & ~3));
                    break;
                case GpuOpCodes.TFLUSH:
                    GpuProcessor.GpuImpl.TextureFlush(GpuStateStructPointer);
                    break;
                case GpuOpCodes.TSYNC:
                    GpuProcessor.GpuImpl.TextureSync(GpuStateStructPointer);
                    break;
                case GpuOpCodes.SPLINE:
                    // @TODO
                    //auto sp_ucount = command.extract!(uint,  0, 8); 
                    //auto sp_vcount = command.extract!(uint,  8, 8);
                    //auto sp_utype  = command.extract!(uint, 16, 2);
                    //auto sp_vtype  = command.extract!(uint, 18, 2);
                    //gpu.logWarning("OP_SPLINE(%d, %d, %d, %d)", sp_ucount, sp_vcount, sp_utype, sp_vtype);
                    break;
                
                case GpuOpCodes.TRXKICK:
                    GpuStateStructPointer.TextureTransferState.TexelSize = (TextureTransferStateStruct.TexelSizeEnum) Params24.Extract(0, 1);
                    GpuProcessor.GpuImpl.Transfer(GpuStateStructPointer);
                    break;
                case GpuOpCodes.PPRIM:
                    //gpu.state.patch.type = command.extract!(PatchPrimitiveType, 0);
                    var primitiveType = (GuPrimitiveType) Params24.Extract(16, 3);
                    var vertexCount = (ushort) Params24.Extract(0, 16);

#if PRIM_BATCH
            var nextInstruction = *(GpuInstruction*) Memory.PspAddressToPointerUnsafe(Pc + 4);

            if (_primCount == 0)
            {
                GpuProcessor.GpuImpl.BeforeDraw(GpuStateStructPointer);
                GpuProcessor.GpuImpl.PrimStart(GlobalGpuState, GpuStateStructPointer,
                    primitiveType);
            }

            if (vertexCount > 0)
            {
                GpuProcessor.GpuImpl.Prim(vertexCount);
            }

            if (nextInstruction.OpCode == GpuOpCodes.PRIM &&
                ((GuPrimitiveType) BitUtils.Extract(nextInstruction.Params, 16, 3) == primitiveType))
            {
                //Console.WriteLine();
                _primCount++;
            }
            else
            {
                //Console.WriteLine("{0:X8}", PC);

                _primCount = 0;
                GpuProcessor.GpuImpl.PrimEnd();
            }
#else
                    GpuDisplayList.GpuProcessor.GpuImpl.BeforeDraw(GpuDisplayList.GpuStateStructPointer);
                    GpuDisplayList.GpuProcessor.GpuImpl.PrimStart(GlobalGpuState, GpuDisplayList.GpuStateStructPointer);
                    GpuDisplayList.GpuProcessor.GpuImpl.Prim(GlobalGpuState, GpuDisplayList.GpuStateStructPointer, primitiveType, vertexCount);
                    GpuDisplayList.GpuProcessor.GpuImpl.PrimEnd(GlobalGpuState, GpuDisplayList.GpuStateStructPointer);
#endif
                    break;
                case GpuOpCodes.ZBW:
                    GpuProcessor.MarkDepthBufferLoad(); // @TODO: Is this required?
                    break;
                case GpuOpCodes.SIGNAL:
                {
                    var signal = Params24.Extract(0, 16);
                    var behaviour = (SignalBehavior) Params24.Extract(16, 8);

                    Console.Out.WriteLineColored(ConsoleColor.Green, "OP_SIGNAL: {0}, {1}", signal, behaviour);

                    switch (behaviour)
                    {
                        case SignalBehavior.PSP_GE_SIGNAL_NONE:
                            break;
                        case SignalBehavior.PSP_GE_SIGNAL_HANDLER_CONTINUE:
                        case SignalBehavior.PSP_GE_SIGNAL_HANDLER_PAUSE:
                        case SignalBehavior.PSP_GE_SIGNAL_HANDLER_SUSPEND:
                            var next = ReadInstructionAndMoveNext();
                            if (next.OpCode != GpuOpCodes.END)
                            {
                                throw new NotImplementedException("Error! Next Signal not an END! : " + next.OpCode);
                            }
                            break;
                        default:
                            throw new NotImplementedException($"Not implemented {behaviour}");
                    }

                    DoSignal(Pc, signal, behaviour, ExecuteNow: true);

                    break;
                }
                case GpuOpCodes.BEZIER:
                {
                    var uCount = (byte)Params24.Extract(0, 8);
                    var vCount = (byte)Params24.Extract(8, 8);
                    DrawBezier(uCount, vCount);
                    break;
                }
                case GpuOpCodes.TMS:
                {
                    GpuStateData[GpuOpCodes.TMS] = 0;
                    break;
                }
                case GpuOpCodes.TMATRIX:
                {
                    var pos = GpuStateData[GpuOpCodes.TMS]++;
                    GpuStateData[GpuOpCodes.TMATRIX_BASE + (ushort) pos] = Params24 << 8;
                    break;
                }
                case GpuOpCodes.VMS:
                {
                    GpuStateData[GpuOpCodes.VMS] = 0;
                    break;
                }
                case GpuOpCodes.VIEW:
                {
                    var pos = GpuStateData[GpuOpCodes.VMS]++;
                    GpuStateData[GpuOpCodes.VIEW_MATRIX_BASE + (ushort) pos] = Params24 << 8;
                    break;
                }
                case GpuOpCodes.WMS:
                {
                    GpuStateData[GpuOpCodes.WMS] = 0;
                    break;
                }
                case GpuOpCodes.WORLD:
                {
                    var pos = GpuStateData[GpuOpCodes.WMS]++;
                    GpuStateData[GpuOpCodes.WORLD_MATRIX_BASE + (ushort) pos] = Params24 << 8;
                    break;
                }
                case GpuOpCodes.PMS:
                {
                    GpuStateData[GpuOpCodes.PMS] = 0;
                    break;
                }
                case GpuOpCodes.PROJ:
                {
                    var pos = GpuStateData[GpuOpCodes.PMS]++;
                    GpuStateData[GpuOpCodes.PROJ_MATRIX_BASE + (ushort) pos] = Params24 << 8;
                    break;
                }
                case GpuOpCodes.BOFS:
                {
                    GpuStateData[GpuOpCodes.BOFS] = Params24;
                    break;
                }
                case GpuOpCodes.BONE:
                {
                    var pos = GpuStateData[GpuOpCodes.BOFS]++;
                    GpuStateData[GpuOpCodes.BONE_MATRIX_BASE + (ushort) pos] = Params24 << 8;
                    break;
                }
                case GpuOpCodes.ORIGIN_ADDR:
                    
                    break;
                case GpuOpCodes.OFFSET_ADDR:
                    GpuStateData[GpuOpCodes.OFFSET_ADDR] = Params24 << 8;
                    break;
            }

            if (Debug)
            {
                var WritePC = Memory.GetPCWriteAddress(Pc);

                Console.Error.WriteLine(
                    "CODE(0x{0:X}-0x{1:X}) : PC(0x{2:X}) : {3} : 0x{4:X} : Done:{5}",
                    InstructionAddressCurrent,
                    InstructionAddressStall,
                    WritePC,
                    Instruction.OpCode,
                    Instruction.Params,
                    Done
                );
            }
        }

        private static float[] BernsteinCoeff(float u)
        {
            var uPow2 = u * u;
            var uPow3 = uPow2 * u;
            var u1 = 1 - u;
            var u1Pow2 = u1 * u1;
            var u1Pow3 = u1Pow2 * u1;

            return new[]
            {
                u1Pow3,
                3 * u * u1Pow2,
                3 * uPow2 * u1,
                uPow3,
            };
        }

        private static void PointMultAdd(ref VertexInfo dest, ref VertexInfo src, float f)
        {
            dest.Position += src.Position * f;
            dest.Texture += src.Texture * f;
            dest.Color += src.Color * f;
            dest.Normal += src.Normal * f;
        }

        private VertexInfo[,] GetControlPoints(int uCount, int vCount)
        {
            var controlPoints = new VertexInfo[uCount, vCount];

            var vertexPtr =
                (byte*) GpuProcessor.Memory.PspAddressToPointerSafe(
                    GpuStateStructPointer.GetAddressRelativeToBaseOffset(GpuStateStructPointer.VertexAddress));
            var vertexReader = new VertexReader();
            vertexReader.SetVertexTypeStruct(GpuStateStructPointer.VertexState.Type, vertexPtr);

            for (var u = 0; u < uCount; u++)
            {
                for (var v = 0; v < vCount; v++)
                {
                    controlPoints[u, v] = vertexReader.ReadVertex(v * uCount + u);
                    //Console.WriteLine("getControlPoints({0}, {1}) : {2}", u, v, controlPoints[u, v]);
                }
            }
            return controlPoints;
        }

        int _primCount;
        
        private void DrawBezier(int uCount, int vCount)
        {
            var divS = GpuStateStructPointer.PatchState.DivS;
            var divT = GpuStateStructPointer.PatchState.DivT;

            if ((uCount - 1) % 3 != 0 || (vCount - 1) % 3 != 0)
            {
                Logger.Warning("Unsupported bezier parameters ucount=" + uCount + " vcount=" + vCount);
                return;
            }
            if (divS <= 0 || divT <= 0)
            {
                Logger.Warning("Unsupported bezier patches patch_div_s=" + divS + " patch_div_t=" + divT);
                return;
            }

            //initRendering();
            //boolean useTexture = context.vinfo.texture != 0 || context.textureFlag.isEnabled();
            //boolean useNormal = context.lightingFlag.isEnabled();

            var anchors = GetControlPoints(uCount, vCount);

            // Don't capture the ram if the vertex list is embedded in the display list. TODO handle stall_addr == 0 better
            // TODO may need to move inside the loop if indices are used, or find the largest index so we can calculate the size of the vertex list
            /*
            if (State.captureGeNextFrame && !isVertexBufferEmbedded()) {
                Logger.Info("Capture drawBezier");
                CaptureManager.captureRAM(context.vinfo.ptr_vertex, context.vinfo.vertexSize * ucount * vcount);
            }
            */

            // Generate patch VertexState.
            var patch = new VertexInfo[divS + 1, divT + 1];

            // Number of patches in the U and V directions
            var upcount = uCount / 3;
            var vpcount = vCount / 3;

            var ucoeff = new float[divS + 1][];

            for (var j = 0; j <= divT; j++)
            {
                var vglobal = (float) j * vpcount / divT;

                var vpatch = (int) vglobal; // Patch number
                var v = vglobal - vpatch;
                if (j == divT)
                {
                    vpatch--;
                    v = 1.0f;
                }
                var vcoeff = BernsteinCoeff(v);

                for (var i = 0; i <= divS; i++)
                {
                    var uglobal = (float) i * upcount / divS;
                    var upatch = (int) uglobal;
                    var u = uglobal - upatch;
                    if (i == divS)
                    {
                        upatch--;
                        u = 1.0f;
                    }
                    ucoeff[i] = BernsteinCoeff(u);

                    var p = default(VertexInfo);
                    p.Position = Vector4.Zero;
                    p.Normal = Vector4.Zero;

                    for (var ii = 0; ii < 4; ++ii)
                    {
                        for (var jj = 0; jj < 4; ++jj)
                        {
                            /*
                            Console.WriteLine(
                                "({0}, {1}) : {2} : {3} : {4}",
                                ii, jj,
                                p.Position, anchors[3 * upatch + ii, 3 * vpatch + jj].Position,
                                ucoeff[i][ii] * vcoeff[jj]
                            );
                            */
                            PointMultAdd(
                                ref p,
                                ref anchors[3 * upatch + ii, 3 * vpatch + jj],
                                ucoeff[i][ii] * vcoeff[jj]
                            );
                        }
                    }

                    p.Texture.X = uglobal;
                    p.Texture.Y = vglobal;

                    patch[i, j] = p;

                    /*
                    Console.WriteLine(
                        "W: ({0}, {1}) : {2}",
                        i, j,
                        patch[i, j] 
                    );
                    */

                    /*
                    if (useTexture && context.vinfo.texture == 0)
                    {
                        p.t[0] = uglobal;
                        p.t[1] = vglobal;
                    }
                    */
                }
            }

            GpuProcessor.GpuImpl.BeforeDraw(GpuStateStructPointer);
            GpuProcessor.GpuImpl.DrawCurvedSurface(GlobalGpuState, GpuStateStructPointer,
                patch, uCount, vCount);
        }
        

        internal void JumpRelativeOffset(uint Address)
        {
            InstructionAddressCurrent = GpuStateStructPointer.GetAddressRelativeToBaseOffset(Address);
        }

        internal void JumpAbsolute(uint Address)
        {
            InstructionAddressCurrent = Address;
        }

        internal void CallRelativeOffset(uint Address)
        {
            CallStack.Push(InstructionAddressCurrent);
            CallStack.Push((uint) GpuStateStructPointer.BaseOffset);
            //CallStack.Push(InstructionAddressCurrent);
            JumpRelativeOffset(Address);
            //throw new NotImplementedException();
        }

        internal void Ret()
        {
            if (CallStack.Count > 0)
            {
                GpuStateStructPointer.BaseOffset = CallStack.Pop();
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
}

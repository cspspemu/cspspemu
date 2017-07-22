using System;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using System.Runtime;
using CSharpPlatform;
using CSPspEmu.Core.Gpu.VertexReading;
using CSPspEmu.Core.Types;

#define PRIM_BATCH

namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
    {
        public static readonly GpuDisplayListRunner Methods = new GpuDisplayListRunner();

        private static readonly Logger Logger = Logger.GetLogger("GpuDisplayListRunner");

        public GlobalGpuState GlobalGpuState;
        public GpuDisplayList GpuDisplayList;
        public GpuOpCodes OpCode;
        public uint Params24;
        public uint Pc;


        private GpuDisplayListRunner()
        {
        }

        public GpuDisplayListRunner(GpuDisplayList gpuDisplayList, GlobalGpuState globalGpuState)
        {
            GpuDisplayList = gpuDisplayList;
            GlobalGpuState = globalGpuState;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public ushort Param16(int offset) => (ushort) (Params24 >> offset);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public byte Param8(int offset) => (byte) (Params24 >> offset);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public uint Extract(int offset, int count) => BitUtils.Extract(Params24, offset, count);

        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //public TType Extract<TType>(int Offset, int Count)
        //{
        //	return (TType)BitUtils.Extract(Params24, Offset, Count);
        //}

        public GpuStateStruct* GpuState => GpuDisplayList.GpuStateStructPointer;
        public float Float1 => MathFloat.ReinterpretUIntAsFloat(Params24 << 8);
        public bool Bool1 => Params24 != 0;

        public void UNIMPLEMENTED_NOTICE()
        {
            if (GpuDisplayList.GpuProcessor.GpuConfig.NoticeUnimplementedGpuCommands)
            {
                Console.Error.WriteLineColored(ConsoleColor.Red, "Unimplemented GpuOpCode: {0} : {1:X}", OpCode,
                    Params24);
            }
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.BBOX)]
        public void OP_BBOX()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.BJUMP)]
        public void OP_BJUMP()
        {
        }

        ClutStateStruct* ClutState => &GpuState->TextureMappingState.ClutState;

        [GpuInstructionAttribute(GpuOpCodes.CBP)]
        public void OP_CBP() => ClutState->Address = (ClutState->Address & 0xFF000000) | ((Params24 << 0) & 0x00FFFFFF);

        [GpuInstructionAttribute(GpuOpCodes.CBPH)]
        public void OP_CBPH() =>
            ClutState->Address = (ClutState->Address & 0x00FFFFFF) | ((Params24 << 8) & 0xFF000000);

        [GpuInstructionAttribute(GpuOpCodes.CLOAD)]
        public void OP_CLOAD() => ClutState->NumberOfColors = Param8(0) * 8;

        // Clut MODE
        [GpuInstructionAttribute(GpuOpCodes.CMODE)]
        public void OP_CMODE()
        {
            ClutState->PixelFormat = (GuPixelFormats) Extract(0, 2);
            ClutState->Shift = (int) Extract(2, 5);
            ClutState->Mask = (int) Extract(8, 8);
            ClutState->Start = (int) Extract(16, 5);
        }

        [GpuInstructionAttribute(GpuOpCodes.DMC)]
        public void OP_DMC() => GpuState->LightingState.DiffuseModelColor.SetRGB_A1(Params24);

        [GpuInstructionAttribute(GpuOpCodes.SMC)]
        public void OP_SMC() => GpuState->LightingState.SpecularModelColor.SetRGB_A1(Params24);

        [GpuInstructionAttribute(GpuOpCodes.EMC)]
        public void OP_EMC() => GpuState->LightingState.EmissiveModelColor.SetRGB_A1(Params24);

        [GpuInstructionAttribute(GpuOpCodes.AMC)]
        public void OP_AMC() =>
            GpuState->LightingState.AmbientModelColor.SetRgb(Params24); // When lighting is off, this is like glColor*

        [GpuInstructionAttribute(GpuOpCodes.AMA)]
        public void OP_AMA() => GpuState->LightingState.AmbientModelColor.SetA(Params24);

        [GpuInstructionAttribute(GpuOpCodes.CMAT)]
        public void OP_CMAT() => GpuState->LightingState.MaterialColorComponents =
            (LightComponentsSet) BitUtils.Extract(Params24, 0, 8);

        // Alpha Blend Enable (GU_BLEND)
        [GpuInstructionAttribute(GpuOpCodes.ABE)]
        public void OP_ABE() => GpuState->BlendingState.Enabled = Bool1;

        // Blend Equation and Functions
        [GpuInstructionAttribute(GpuOpCodes.ALPHA)]
        public void OP_ALPHA()
        {
            GpuState->BlendingState.FunctionSource = (GuBlendingFactorSource) ((Params24 >> 0) & 0xF);
            GpuState->BlendingState.FunctionDestination = (GuBlendingFactorDestination) ((Params24 >> 4) & 0xF);
            GpuState->BlendingState.Equation = (BlendingOpEnum) ((Params24 >> 8) & 0xF);
        }

        [GpuInstructionAttribute(GpuOpCodes.SFIX)]
        public void OP_SFIX() => GpuState->BlendingState.FixColorSource.SetRGB_A1(Params24);

        [GpuInstructionAttribute(GpuOpCodes.DFIX)]
        public void OP_DFIX() => GpuState->BlendingState.FixColorDestination.SetRGB_A1(Params24);

        // Pixel MasK Color
        [GpuInstructionAttribute(GpuOpCodes.PMSKC)]
        public void OP_PMSKC()
        {
            GpuState->BlendingState.ColorMask.R = Param8(0);
            GpuState->BlendingState.ColorMask.G = Param8(8);
            GpuState->BlendingState.ColorMask.B = Param8(16);
            //Console.Error.WriteLine("OP_PMSKC");
        }

        [GpuInstructionAttribute(GpuOpCodes.PMSKA)]
        public void OP_PMSKA() => GpuState->BlendingState.ColorMask.A = Param8(0);

        [GpuInstructionAttribute(GpuOpCodes.CTST)]
        public void OP_CTST() => GpuState->ColorTestState.Function = (ColorTestFunctionEnum) Extract(0, 2);

        // Color REFerence
        [GpuInstructionAttribute(GpuOpCodes.CREF)]
        public void OP_CREF()
        {
            //Console.Error.WriteLine("OP_CREF");
            GpuState->ColorTestState.Ref.R = (byte) Extract(8 * 0, 8);
            GpuState->ColorTestState.Ref.G = (byte) Extract(8 * 1, 8);
            GpuState->ColorTestState.Ref.B = (byte) Extract(8 * 2, 8);
            GpuState->ColorTestState.Ref.A = 0x00;
            //Console.Error.WriteLine("CREF: {0}", GpuState->ColorTestState.ToStringDefault());
        }

        // Color MaSK
        [GpuInstructionAttribute(GpuOpCodes.CMSK)]
        public void OP_CMSK()
        {
            //Console.Error.WriteLine("OP_CMSK");
            GpuState->ColorTestState.Mask.R = (byte) Extract(8 * 0, 8);
            GpuState->ColorTestState.Mask.G = (byte) Extract(8 * 1, 8);
            GpuState->ColorTestState.Mask.B = (byte) Extract(8 * 2, 8);
            GpuState->ColorTestState.Mask.A = 0x00;
            //Console.Error.WriteLine("CMSK: {0}", GpuState->ColorTestState.ToStringDefault());
        }

        // Depth Buffer Pointer
        [GpuInstructionAttribute(GpuOpCodes.ZBP)]
        public void OP_ZBP() => GpuState->DepthBufferState.LowAddress = Params24;

        // Depth Buffer Width
        [GpuInstructionAttribute(GpuOpCodes.ZBW)]
        public void OP_ZBW()
        {
            GpuState->DepthBufferState.HighAddress = Param8(16);
            GpuState->DepthBufferState.Width = Param16(0);
            GpuDisplayList.GpuProcessor.MarkDepthBufferLoad();
        }

        [GpuInstructionAttribute(GpuOpCodes.ZTE)]
        public void OP_ZTE() => GpuState->DepthTestState.Enabled = Bool1;

        // void sceGuDepthFunc(int function); // OP_ZTST
        [GpuInstructionAttribute(GpuOpCodes.ZTST)]
        public void OP_ZTST() => GpuState->DepthTestState.Function = (TestFunctionEnum) Param8(0);

        // Alpha Test Enable (GU_ALPHA_TEST) glAlphaFunc(GL_GREATER, 0.03f);
        [GpuInstructionAttribute(GpuOpCodes.ATE)]
        public void OP_ATE() => GpuState->AlphaTestState.Enabled = Bool1;

        // void sceGuAlphaFunc(int func, int value, int mask); // OP_ATST
        [GpuInstructionAttribute(GpuOpCodes.ATST)]
        public void OP_ATST()
        {
            GpuState->AlphaTestState.Function = (TestFunctionEnum) Param8(0);
            GpuState->AlphaTestState.Value = Param8(8);
            GpuState->AlphaTestState.Mask = Param8(16);
        }

        // Stencil Test Enable (GL_STENCIL_TEST)
        [GpuInstructionAttribute(GpuOpCodes.STE)]
        public void OP_STE() => GpuState->StencilState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.STST)]
        public void OP_STST()
        {
            GpuState->StencilState.Function = (TestFunctionEnum) Param8(0);
            GpuState->StencilState.FunctionRef = Param8(8);
            GpuState->StencilState.FunctionMask = Param8(16);
        }

        // Stencil OPeration
        [GpuInstructionAttribute(GpuOpCodes.SOP)]
        public void OP_SOP()
        {
            GpuState->StencilState.OperationFail = (StencilOperationEnum) Param8(0);
            GpuState->StencilState.OperationZFail = (StencilOperationEnum) Param8(8);
            GpuState->StencilState.OperationZPass = (StencilOperationEnum) Param8(16);
        }

        // glDepthMask
        [GpuInstructionAttribute(GpuOpCodes.ZMSK)]
        public void OP_ZMSK() => GpuState->DepthTestState.Mask = Param16(0);

        // void sceGuDepthRange(int near, int far); // OP_NEARZ + OP_FARZ
        // void sceGuDepthOffset(unsigned int offset);
        [GpuInstructionAttribute(GpuOpCodes.NEARZ)]
        public void OP_NEARZ() => GpuState->DepthTestState.RangeFar = ((float) Param16(0)) / ushort.MaxValue;

        [GpuInstructionAttribute(GpuOpCodes.FARZ)]
        public void OP_FARZ() => GpuState->DepthTestState.RangeNear = ((float) Param16(0)) / ushort.MaxValue;

        private void _OP_DTH(int n)
        {
            GpuState->DitherMatrix[4 * n + 0] = (sbyte) BitUtils.ExtractSigned(Params24, 4 * 0, 4);
            GpuState->DitherMatrix[4 * n + 1] = (sbyte) BitUtils.ExtractSigned(Params24, 4 * 1, 4);
            GpuState->DitherMatrix[4 * n + 2] = (sbyte) BitUtils.ExtractSigned(Params24, 4 * 2, 4);
            GpuState->DitherMatrix[4 * n + 3] = (sbyte) BitUtils.ExtractSigned(Params24, 4 * 3, 4);
        }

        [GpuInstructionAttribute(GpuOpCodes.DTH0)]
        public void OP_DTH0() => _OP_DTH(0);

        [GpuInstructionAttribute(GpuOpCodes.DTH1)]
        public void OP_DTH1() => _OP_DTH(1);

        [GpuInstructionAttribute(GpuOpCodes.DTH2)]
        public void OP_DTH2() => _OP_DTH(2);

        [GpuInstructionAttribute(GpuOpCodes.DTH3)]
        public void OP_DTH3() => _OP_DTH(3);

        [GpuInstructionAttribute(GpuOpCodes.CLEAR)]
        public void OP_CLEAR()
        {
            // Set flags and Start the clearing mode.
            if ((Params24 & 1) != 0)
            {
                GpuState->ClearFlags = (ClearBufferSet) Param8(8);
                GpuState->ClearingMode = true;
            }
            // Stop the clearing mode.
            else
            {
                GpuState->ClearingMode = false;
            }
        }

        // Vertex Type
        [GpuInstructionAttribute(GpuOpCodes.VTYPE)]
        public void OP_VTYPE() => GpuState->VertexState.Type.Value = Params24;

        [GpuInstructionAttribute(GpuOpCodes.RNORM)]
        public void OP_RNORM() => GpuState->VertexState.Type.ReversedNormal = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.VADDR)]
        public void OP_VADDR() => GpuState->VertexAddress = Params24;

        [GpuInstructionAttribute(GpuOpCodes.IADDR)]
        public void OP_IADDR() => GpuState->IndexAddress = Params24;

        [GpuInstructionAttribute(GpuOpCodes.BEZIER)]
        public void OP_BEZIER()
        {
            var uCount = Param8(0);
            var vCount = Param8(8);

            DrawBezier(uCount, vCount);
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
                (byte*) GpuDisplayList.GpuProcessor.Memory.PspAddressToPointerSafe(
                    GpuState->GetAddressRelativeToBaseOffset(GpuState->VertexAddress));
            var vertexReader = new VertexReader();
            vertexReader.SetVertexTypeStruct(GpuState->VertexState.Type, vertexPtr);

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

        private void DrawBezier(int uCount, int vCount)
        {
            var divS = GpuState->PatchState.DivS;
            var divT = GpuState->PatchState.DivT;

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

            var anchors = GetControlPoints(uCount, vCount);

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
                    p.Position = Vector4f.Zero;
                    p.Normal = Vector4f.Zero;

                    for (var ii = 0; ii < 4; ++ii)
                    {
                        for (var jj = 0; jj < 4; ++jj)
                        {
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
                }
            }

            GpuDisplayList.GpuProcessor.GpuImpl.BeforeDraw(GpuDisplayList.GpuStateStructPointer);
            GpuDisplayList.GpuProcessor.GpuImpl.DrawCurvedSurface(GlobalGpuState, GpuDisplayList.GpuStateStructPointer,
                patch, uCount, vCount);
        }

        int _primCount;

        /// <summary>
        /// Primitive Kick - draw PRIMitive
        /// </summary>
        [GpuInstructionAttribute(GpuOpCodes.PRIM)]
        public void OP_PRIM()
        {
            var primitiveType = (GuPrimitiveType) Extract(16, 3);
            var vertexCount = (ushort) Extract(0, 16);

#if PRIM_BATCH
            var nextInstruction = *(GpuInstruction*) GpuDisplayList.Memory.PspAddressToPointerUnsafe(Pc + 4);

            if (_primCount == 0)
            {
                GpuDisplayList.GpuProcessor.GpuImpl.BeforeDraw(GpuDisplayList.GpuStateStructPointer);
                GpuDisplayList.GpuProcessor.GpuImpl.PrimStart(GlobalGpuState, GpuDisplayList.GpuStateStructPointer,
                    primitiveType);
            }

            if (vertexCount > 0)
            {
                GpuDisplayList.GpuProcessor.GpuImpl.Prim(vertexCount);
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
                GpuDisplayList.GpuProcessor.GpuImpl.PrimEnd();
            }
#else
			GpuDisplayList.GpuProcessor.GpuImpl.BeforeDraw(GpuDisplayList.GpuStateStructPointer);
			GpuDisplayList.GpuProcessor.GpuImpl.PrimStart(GlobalGpuState, GpuDisplayList.GpuStateStructPointer);
			GpuDisplayList.GpuProcessor.GpuImpl.Prim(GlobalGpuState, GpuDisplayList.GpuStateStructPointer, primitiveType, vertexCount);
			GpuDisplayList.GpuProcessor.GpuImpl.PrimEnd(GlobalGpuState, GpuDisplayList.GpuStateStructPointer);
#endif
        }

        [GpuInstructionAttribute(GpuOpCodes.BCE)]
        public void OP_BCE() => GpuState->BackfaceCullingState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.DTE)]
        public void OP_DTE() => GpuState->DitheringState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.CPE)]
        public void OP_CPE() => GpuState->ClipPlaneState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.AAE)]
        public void OP_AAE() => GpuState->LineSmoothState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.PCE)]
        public void OP_PCE() => GpuState->PatchCullingState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.CTE)]
        public void OP_CTE() => GpuState->ColorTestState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.JUMP)]
        public void OP_JUMP() => GpuDisplayList.JumpRelativeOffset((uint) (Params24 & ~3));

        [GpuInstructionAttribute(GpuOpCodes.END)]
        public void OP_END()
        {
            GpuDisplayList.Done = true;
            GpuDisplayList.GpuProcessor.GpuImpl.End(GpuState);
        }

        [GpuInstructionAttribute(GpuOpCodes.FINISH)]
        public void OP_FINISH()
        {
            GpuDisplayList.GpuProcessor.GpuImpl.Finish(GpuDisplayList.GpuStateStructPointer);
            GpuDisplayList.DoFinish(Pc, Params24, ExecuteNow: true);
        }

        //[GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.CALL)]
        public void OP_CALL() => GpuDisplayList.CallRelativeOffset((uint) (Params24 & ~3));

        //[GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.RET)]
        public void OP_RET() => GpuDisplayList.Ret();

        // void sceGuSignal(int signal, int behavior);
        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.SIGNAL)]
        public void OP_SIGNAL()
        {
            var signal = Extract(0, 16);
            var behaviour = (SignalBehavior) Extract(16, 8);

            Console.Out.WriteLineColored(ConsoleColor.Green, "OP_SIGNAL: {0}, {1}", signal, behaviour);

            switch (behaviour)
            {
                case SignalBehavior.PSP_GE_SIGNAL_NONE:
                    break;
                case SignalBehavior.PSP_GE_SIGNAL_HANDLER_CONTINUE:
                case SignalBehavior.PSP_GE_SIGNAL_HANDLER_PAUSE:
                case SignalBehavior.PSP_GE_SIGNAL_HANDLER_SUSPEND:
                    var next = GpuDisplayList.ReadInstructionAndMoveNext();
                    if (next.OpCode != GpuOpCodes.END)
                    {
                        throw new NotImplementedException("Error! Next Signal not an END! : " + next.OpCode);
                    }
                    break;
                default:
                    throw new NotImplementedException($"Not implemented {behaviour}");
            }

            GpuDisplayList.DoSignal(Pc, signal, behaviour, ExecuteNow: true);
        }

        [GpuInstructionAttribute(GpuOpCodes.FGE)]
        public void OP_FGE() => GpuState->FogState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.FCOL)]
        public void OP_FCOL() => GpuState->FogState.Color.SetRGB_A1(Params24);

        [GpuInstructionAttribute(GpuOpCodes.FFAR)]
        public void OP_FFAR() => GpuState->FogState.End = Float1;

        [GpuInstructionAttribute(GpuOpCodes.FDIST)]
        public void OP_FDIST() => GpuState->FogState.Dist = Float1;

        //string LightArrayOperation(string type, string code, int step = 1) { return ArrayOperation(type, 0, 3, code, step); }
        //string LightArrayOperationStep3(string type, string code) { return LightArrayOperation(type, code, 3); }

        [GpuInstructionAttribute(GpuOpCodes.LTE)]
        public void OP_LTE() => GpuState->LightingState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.ALC)]
        public void OP_ALC() => GpuState->LightingState.AmbientLightColor.SetRgb(Params24);

        [GpuInstructionAttribute(GpuOpCodes.ALA)]
        public void OP_ALA() => GpuState->LightingState.AmbientLightColor.SetA(Params24);

        [GpuInstructionAttribute(GpuOpCodes.LMODE)]
        public void OP_LMODE() => GpuState->LightingState.LightModel = (LightModelEnum) Param8(0);

        LightStateStruct* GetLigth(int index) => &((&GpuState->LightingState.Light0)[index]);

        private void _OP_LTE(int index) => GetLigth(index)->Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.LTE0)]
        public void OP_LTE0() => _OP_LTE(0);

        [GpuInstructionAttribute(GpuOpCodes.LTE1)]
        public void OP_LTE1() => _OP_LTE(1);

        [GpuInstructionAttribute(GpuOpCodes.LTE2)]
        public void OP_LTE2() => _OP_LTE(2);

        [GpuInstructionAttribute(GpuOpCodes.LTE3)]
        public void OP_LTE3() => _OP_LTE(3);

        private void _OP_LXP(int index) => GetLigth(index)->Position.X = Float1;
        private void _OP_LYP(int index) => GetLigth(index)->Position.Y = Float1;
        private void _OP_LZP(int index) => GetLigth(index)->Position.Z = Float1;

        [GpuInstructionAttribute(GpuOpCodes.LXP0)]
        public void OP_LXP0() => _OP_LXP(0);

        [GpuInstructionAttribute(GpuOpCodes.LXP1)]
        public void OP_LXP1() => _OP_LXP(1);

        [GpuInstructionAttribute(GpuOpCodes.LXP2)]
        public void OP_LXP2() => _OP_LXP(2);

        [GpuInstructionAttribute(GpuOpCodes.LXP3)]
        public void OP_LXP3() => _OP_LXP(3);

        [GpuInstructionAttribute(GpuOpCodes.LYP0)]
        public void OP_LYP0() => _OP_LYP(0);

        [GpuInstructionAttribute(GpuOpCodes.LYP1)]
        public void OP_LYP1() => _OP_LYP(1);

        [GpuInstructionAttribute(GpuOpCodes.LYP2)]
        public void OP_LYP2() => _OP_LYP(2);

        [GpuInstructionAttribute(GpuOpCodes.LYP3)]
        public void OP_LYP3() => _OP_LYP(3);

        [GpuInstructionAttribute(GpuOpCodes.LZP0)]
        public void OP_LZP0() => _OP_LZP(0);

        [GpuInstructionAttribute(GpuOpCodes.LZP1)]
        public void OP_LZP1() => _OP_LZP(1);

        [GpuInstructionAttribute(GpuOpCodes.LZP2)]
        public void OP_LZP2() => _OP_LZP(2);

        [GpuInstructionAttribute(GpuOpCodes.LZP3)]
        public void OP_LZP3() => _OP_LZP(3);

        private void _OP_LT(int index)
        {
            GetLigth(index)->Kind = (LightModelEnum) Param8(0);
            GetLigth(index)->Type = (LightTypeEnum) Param8(8);
            switch (GetLigth(index)->Type)
            {
                case LightTypeEnum.Directional:
                    GetLigth(index)->Position.W = 0;
                    break;
                case LightTypeEnum.PointLight:
                    GetLigth(index)->Position.W = 1;
                    GetLigth(index)->SpotCutoff = 180;
                    break;
                case LightTypeEnum.SpotLight:
                    GetLigth(index)->Position.W = 1;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [GpuInstructionAttribute(GpuOpCodes.LT0)]
        public void OP_LT0() => _OP_LT(0);

        [GpuInstructionAttribute(GpuOpCodes.LT1)]
        public void OP_LT1() => _OP_LT(1);

        [GpuInstructionAttribute(GpuOpCodes.LT2)]
        public void OP_LT2() => _OP_LT(2);

        [GpuInstructionAttribute(GpuOpCodes.LT3)]
        public void OP_LT3() => _OP_LT(3);

        private void _OP_LCA(int index) => GetLigth(index)->Attenuation.Constant = Float1;
        private void _OP_LLA(int index) => GetLigth(index)->Attenuation.Linear = Float1;
        private void _OP_LQA(int index) => GetLigth(index)->Attenuation.Quadratic = Float1;

        [GpuInstructionAttribute(GpuOpCodes.LCA0)]
        public void OP_LCA0() => _OP_LCA(0);

        [GpuInstructionAttribute(GpuOpCodes.LCA1)]
        public void OP_LCA1() => _OP_LCA(1);

        [GpuInstructionAttribute(GpuOpCodes.LCA2)]
        public void OP_LCA2() => _OP_LCA(2);

        [GpuInstructionAttribute(GpuOpCodes.LCA3)]
        public void OP_LCA3() => _OP_LCA(3);

        [GpuInstructionAttribute(GpuOpCodes.LLA0)]
        public void OP_LLA0() => _OP_LLA(0);

        [GpuInstructionAttribute(GpuOpCodes.LLA1)]
        public void OP_LLA1() => _OP_LLA(1);

        [GpuInstructionAttribute(GpuOpCodes.LLA2)]
        public void OP_LLA2() => _OP_LLA(2);

        [GpuInstructionAttribute(GpuOpCodes.LLA3)]
        public void OP_LLA3() => _OP_LLA(3);

        [GpuInstructionAttribute(GpuOpCodes.LQA0)]
        public void OP_LQA0() => _OP_LQA(0);

        [GpuInstructionAttribute(GpuOpCodes.LQA1)]
        public void OP_LQA1() => _OP_LQA(1);

        [GpuInstructionAttribute(GpuOpCodes.LQA2)]
        public void OP_LQA2() => _OP_LQA(2);

        [GpuInstructionAttribute(GpuOpCodes.LQA3)]
        public void OP_LQA3() => _OP_LQA(3);

        private void _OP_LXD(int index) => GetLigth(index)->SpotDirection.X = Float1;
        private void _OP_LYD(int index) => GetLigth(index)->SpotDirection.Y = Float1;
        private void _OP_LZD(int index) => GetLigth(index)->SpotDirection.Z = Float1;

        [GpuInstructionAttribute(GpuOpCodes.LXD0)]
        public void OP_LXD0() => _OP_LXD(0);

        [GpuInstructionAttribute(GpuOpCodes.LXD1)]
        public void OP_LXD1() => _OP_LXD(1);

        [GpuInstructionAttribute(GpuOpCodes.LXD2)]
        public void OP_LXD2() => _OP_LXD(2);

        [GpuInstructionAttribute(GpuOpCodes.LXD3)]
        public void OP_LXD3() => _OP_LXD(3);

        [GpuInstructionAttribute(GpuOpCodes.LYD0)]
        public void OP_LYD0() => _OP_LYD(0);

        [GpuInstructionAttribute(GpuOpCodes.LYD1)]
        public void OP_LYD1() => _OP_LYD(1);

        [GpuInstructionAttribute(GpuOpCodes.LYD2)]
        public void OP_LYD2() => _OP_LYD(2);

        [GpuInstructionAttribute(GpuOpCodes.LYD3)]
        public void OP_LYD3() => _OP_LYD(3);

        [GpuInstructionAttribute(GpuOpCodes.LZD0)]
        public void OP_LZD0() => _OP_LZD(0);

        [GpuInstructionAttribute(GpuOpCodes.LZD1)]
        public void OP_LZD1() => _OP_LZD(1);

        [GpuInstructionAttribute(GpuOpCodes.LZD2)]
        public void OP_LZD2() => _OP_LZD(2);

        [GpuInstructionAttribute(GpuOpCodes.LZD3)]
        public void OP_LZD3() => _OP_LZD(3);

        private void _OP_SPOTEXP(int index) => GetLigth(index)->SpotExponent = Float1;
        private void _OP_SPOTCUT(int index) => GetLigth(index)->SpotCutoff = Float1;

        [GpuInstructionAttribute(GpuOpCodes.SPOTEXP0)]
        public void OP_SPOTEXP0() => _OP_SPOTEXP(0);

        [GpuInstructionAttribute(GpuOpCodes.SPOTEXP1)]
        public void OP_SPOTEXP1() => _OP_SPOTEXP(1);

        [GpuInstructionAttribute(GpuOpCodes.SPOTEXP2)]
        public void OP_SPOTEXP2() => _OP_SPOTEXP(2);

        [GpuInstructionAttribute(GpuOpCodes.SPOTEXP3)]
        public void OP_SPOTEXP3() => _OP_SPOTEXP(3);

        [GpuInstructionAttribute(GpuOpCodes.SPOTCUT0)]
        public void OP_SPOTCUT0() => _OP_SPOTCUT(0);

        [GpuInstructionAttribute(GpuOpCodes.SPOTCUT1)]
        public void OP_SPOTCUT1() => _OP_SPOTCUT(1);

        [GpuInstructionAttribute(GpuOpCodes.SPOTCUT2)]
        public void OP_SPOTCUT2() => _OP_SPOTCUT(2);

        [GpuInstructionAttribute(GpuOpCodes.SPOTCUT3)]
        public void OP_SPOTCUT3() => _OP_SPOTCUT(3);

        private void _OP_ALC(int index) => GetLigth(index)->AmbientColor.SetRGB_A1(Params24);
        private void _OP_DLC(int index) => GetLigth(index)->DiffuseColor.SetRGB_A1(Params24);
        private void _OP_SLC(int index) => GetLigth(index)->SpecularColor.SetRGB_A1(Params24);

        [GpuInstructionAttribute(GpuOpCodes.ALC0)]
        public void OP_ALC0() => _OP_ALC(0);

        [GpuInstructionAttribute(GpuOpCodes.ALC1)]
        public void OP_ALC1() => _OP_ALC(1);

        [GpuInstructionAttribute(GpuOpCodes.ALC2)]
        public void OP_ALC2() => _OP_ALC(2);

        [GpuInstructionAttribute(GpuOpCodes.ALC3)]
        public void OP_ALC3() => _OP_ALC(3);

        [GpuInstructionAttribute(GpuOpCodes.DLC0)]
        public void OP_DLC0() => _OP_DLC(0);

        [GpuInstructionAttribute(GpuOpCodes.DLC1)]
        public void OP_DLC1() => _OP_DLC(1);

        [GpuInstructionAttribute(GpuOpCodes.DLC2)]
        public void OP_DLC2() => _OP_DLC(2);

        [GpuInstructionAttribute(GpuOpCodes.DLC3)]
        public void OP_DLC3() => _OP_DLC(3);

        [GpuInstructionAttribute(GpuOpCodes.SLC0)]
        public void OP_SLC0() => _OP_SLC(0);

        [GpuInstructionAttribute(GpuOpCodes.SLC1)]
        public void OP_SLC1() => _OP_SLC(1);

        [GpuInstructionAttribute(GpuOpCodes.SLC2)]
        public void OP_SLC2() => _OP_SLC(2);

        [GpuInstructionAttribute(GpuOpCodes.SLC3)]
        public void OP_SLC3() => _OP_SLC(3);

        [GpuInstructionAttribute(GpuOpCodes.SPOW)]
        public void OP_SPOW() => GpuState->LightingState.SpecularPower = Float1;

        [GpuInstructionAttribute(GpuOpCodes.VMS)]
        public void OP_VMS() => GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Reset(Params24);

        [GpuInstructionAttribute(GpuOpCodes.VIEW)]
        public void OP_VIEW() => GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Write(Float1);

        [GpuInstructionAttribute(GpuOpCodes.WMS)]
        public void OP_WMS() => GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Reset(Params24);

        [GpuInstructionAttribute(GpuOpCodes.WORLD)]
        public void OP_WORLD() => GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Write(Float1);

        [GpuInstructionAttribute(GpuOpCodes.PMS)]
        public void OP_PMS() => GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Reset(Params24);

        [GpuInstructionAttribute(GpuOpCodes.PROJ)]
        public void OP_PROJ() => GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Write(Float1);

        private SkinningStateStruct* SkinningState => &GpuDisplayList.GpuStateStructPointer->SkinningState;

        [GpuInstructionAttribute(GpuOpCodes.BOFS)]
        public void OP_BOFS() => SkinningState->CurrentBoneIndex = (int) Params24;

        [GpuInstructionAttribute(GpuOpCodes.BONE)]
        public void OP_BONE()
        {
            var boneMatrices = &SkinningState->BoneMatrix0;
            boneMatrices[SkinningState->CurrentBoneIndex / 12]
                .WriteAt(SkinningState->CurrentBoneIndex % 12, Float1);
            SkinningState->CurrentBoneIndex++;
        }

        private void _OP_MW(int index) => GpuState->MorphingState.MorphWeight[index] = Float1;

        [GpuInstructionAttribute(GpuOpCodes.MW0)]
        public void OP_MW0() => _OP_MW(0);

        [GpuInstructionAttribute(GpuOpCodes.MW1)]
        public void OP_MW1() => _OP_MW(1);

        [GpuInstructionAttribute(GpuOpCodes.MW2)]
        public void OP_MW2() => _OP_MW(2);

        [GpuInstructionAttribute(GpuOpCodes.MW3)]
        public void OP_MW3() => _OP_MW(3);

        [GpuInstructionAttribute(GpuOpCodes.MW4)]
        public void OP_MW4() => _OP_MW(4);

        [GpuInstructionAttribute(GpuOpCodes.MW5)]
        public void OP_MW5() => _OP_MW(5);

        [GpuInstructionAttribute(GpuOpCodes.MW6)]
        public void OP_MW6() => _OP_MW(6);

        [GpuInstructionAttribute(GpuOpCodes.MW7)]
        public void OP_MW7() => _OP_MW(7);

        [GpuInstructionAttribute(GpuOpCodes.NOP)]
        public void OP_NOP()
        {
        }

        [GpuInstructionAttribute(GpuOpCodes.BASE)]
        public void OP_BASE() => GpuState->BaseAddress = ((Params24 << 8) & 0xff000000);

        [GpuInstructionAttribute(GpuOpCodes.OFFSET_ADDR)]
        public void OP_OFFSET_ADDR() => GpuState->BaseOffset = (Params24 << 8);

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.ORIGIN_ADDR)]
        public void OP_ORIGIN_ADDR() => GpuState->BaseOffset = Pc;

        [GpuInstructionAttribute(GpuOpCodes.FBP)]
        public void OP_FBP() => GpuState->DrawBufferState.LowAddress = Params24;

        [GpuInstructionAttribute(GpuOpCodes.FBW)]
        public void OP_FBW()
        {
            GpuState->DrawBufferState.HighAddress = Param8(16);
            GpuState->DrawBufferState.Width = Param16(0);

            if (GpuState->DrawBufferState.Width == 0)
            {
                //Console.WriteLine("GpuState->DrawBufferState.Width == 0!");
                //GpuState->DrawBufferState.Width = 512;
            }

            //GpuDisplayList.GpuProcessor.GpuImpl.
            //gpu.markBufferOp(BufferOperation.LOAD, BufferType.COLOR);
            //Console.WriteLine("{0}", GpuState->DrawBufferState.Format);
        }

        [GpuInstructionAttribute(GpuOpCodes.PSM)]
        public void OP_PSM() => GpuState->DrawBufferState.Format = (GuPixelFormats) Param8(0);

        [GpuInstructionAttribute(GpuOpCodes.REGION1)]
        public void OP_REGION1()
        {
            var x1 = (short) BitUtils.Extract(Params24, 0, 10);
            var y1 = (short) BitUtils.Extract(Params24, 10, 10);
            GpuState->Viewport.RegionTopLeft.X = x1;
            GpuState->Viewport.RegionTopLeft.Y = y1;
        }

        //[GpuOpCodesNotImplemented]

        [GpuInstructionAttribute(GpuOpCodes.REGION2)]
        public void OP_REGION2()
        {
            var x2 = (short) BitUtils.Extract(Params24, 0, 10);
            var y2 = (short) BitUtils.Extract(Params24, 10, 10);
            GpuState->Viewport.RegionBottomRight.X = (short) (x2 + 1);
            GpuState->Viewport.RegionBottomRight.Y = (short) (y2 + 1);
        }

        [GpuInstructionAttribute(GpuOpCodes.SCISSOR1)]
        public void OP_SCISSOR1()
        {
            GpuState->ClipPlaneState.Scissor.Left = (short) BitUtils.Extract(Params24, 0, 10);
            GpuState->ClipPlaneState.Scissor.Top = (short) BitUtils.Extract(Params24, 10, 10);
        }


        [GpuInstructionAttribute(GpuOpCodes.SCISSOR2)]
        public void OP_SCISSOR2()
        {
            GpuState->ClipPlaneState.Scissor.Right = (short) BitUtils.Extract(Params24, 0, 10);
            GpuState->ClipPlaneState.Scissor.Bottom = (short) BitUtils.Extract(Params24, 10, 10);
        }

        [GpuInstructionAttribute(GpuOpCodes.XSCALE)]
        public void OP_XSCALE() => GpuState->Viewport.Scale.X = Float1 * 2;

        [GpuInstructionAttribute(GpuOpCodes.YSCALE)]
        public void OP_YSCALE() => GpuState->Viewport.Scale.Y = -Float1 * 2;

        [GpuInstructionAttribute(GpuOpCodes.ZSCALE)]
        public void OP_ZSCALE() => GpuState->Viewport.Scale.Z = Float1;

        [GpuInstructionAttribute(GpuOpCodes.XPOS)]
        public void OP_XPOS() => GpuState->Viewport.Position.X = Float1;

        [GpuInstructionAttribute(GpuOpCodes.YPOS)]
        public void OP_YPOS() => GpuState->Viewport.Position.Y = Float1;

        [GpuInstructionAttribute(GpuOpCodes.ZPOS)]
        public void OP_ZPOS() => GpuState->Viewport.Position.Z = BitUtils.ExtractUnsignedScaled(Params24, 0, 16, 1.0f);

        [GpuInstructionAttribute(GpuOpCodes.OFFSETX)]
        public void OP_OFFSETX() => GpuState->Offset.X = (short) BitUtils.Extract(Params24, 0, 4);

        [GpuInstructionAttribute(GpuOpCodes.OFFSETY)]
        public void OP_OFFSETY() => GpuState->Offset.Y = (short) BitUtils.Extract(Params24, 0, 4);

        [GpuInstructionAttribute(GpuOpCodes.FFACE)]
        public void OP_FFACE() => GpuState->BackfaceCullingState.FrontFaceDirection = (FrontFaceDirectionEnum) Params24;

        [GpuInstructionAttribute(GpuOpCodes.SHADE)]
        public void OP_SHADE() => GpuState->ShadeModel = (ShadingModelEnum) Params24;

        [GpuInstructionAttribute(GpuOpCodes.LOE)]
        public void OP_LOE() => GpuState->LogicalOperationState.Enabled = Bool1;

        [GpuInstructionAttribute(GpuOpCodes.LOP)]
        public void OP_LOP() => GpuState->LogicalOperationState.Operation = (LogicalOperationEnum) Param8(0);

        [GpuInstructionAttribute(GpuOpCodes.PSUB)]
        public void OP_PSUB()
        {
            GpuState->PatchState.DivS = Param8(0);
            GpuState->PatchState.DivT = Param8(8);
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.PPRIM)]
        public void OP_PPRIM()
        {
            //gpu.state.patch.type = command.extract!(PatchPrimitiveType, 0);
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.SPLINE)]
        public void OP_SPLINE()
        {
        }


        [GpuInstructionAttribute(GpuOpCodes.PFACE)]
        public void OP_PFACE() => GpuState->PatchCullingState.FaceFlag = (Params24 != 0);

        private TextureStateStruct* TextureState => &GpuState->TextureMappingState.TextureState;

        [GpuInstructionAttribute(GpuOpCodes.TME)]
        public void OP_TME() => GpuState->TextureMappingState.Enabled = Bool1;


        [GpuInstructionAttribute(GpuOpCodes.TMS)]
        public void OP_TMS() => GpuState->TextureMappingState.Matrix.Reset();


        [GpuInstructionAttribute(GpuOpCodes.TMATRIX)]
        public void OP_TMATRIX() => GpuState->TextureMappingState.Matrix.Write(Float1);

        [GpuInstructionAttribute(GpuOpCodes.TMODE)]
        public void OP_TMODE()
        {
            TextureState->Swizzled = (Param8(0) != 0);
            TextureState->MipmapShareClut = (Param8(8) != 0);
            TextureState->MipmapMaxLevel = Param8(16);
        }

        // Texture Pixel Storage Mode

        [GpuInstructionAttribute(GpuOpCodes.TPSM)]
        public void OP_TPSM() => TextureState->PixelFormat = (GuPixelFormats) Extract(0, 4);

        private TextureStateStruct.MipmapState* MipMapState(int index) => &(&TextureState->Mipmap0)[index];

        private void _OP_TBP(int index)
        {
            var mipMap = MipMapState(index);
            mipMap->Address = (mipMap->Address & 0xFF000000) | (Params24 & 0x00FFFFFF);
        }

        private void _OP_TBW(int index)
        {
            var mipMap = MipMapState(index);
            mipMap->BufferWidth = Param16(0);
            mipMap->Address = (mipMap->Address & 0x00FFFFFF) | ((uint) (Param8(16) << 24) & 0xFF000000);
        }

        [GpuInstructionAttribute(GpuOpCodes.TBP0)]
        public void OP_TBP0() => _OP_TBP(0);

        [GpuInstructionAttribute(GpuOpCodes.TBP1)]
        public void OP_TBP1() => _OP_TBP(1);

        [GpuInstructionAttribute(GpuOpCodes.TBP2)]
        public void OP_TBP2() => _OP_TBP(2);

        [GpuInstructionAttribute(GpuOpCodes.TBP3)]
        public void OP_TBP3() => _OP_TBP(3);

        [GpuInstructionAttribute(GpuOpCodes.TBP4)]
        public void OP_TBP4() => _OP_TBP(4);

        [GpuInstructionAttribute(GpuOpCodes.TBP5)]
        public void OP_TBP5() => _OP_TBP(5);

        [GpuInstructionAttribute(GpuOpCodes.TBP6)]
        public void OP_TBP6() => _OP_TBP(6);

        [GpuInstructionAttribute(GpuOpCodes.TBP7)]
        public void OP_TBP7() => _OP_TBP(7);

        [GpuInstructionAttribute(GpuOpCodes.TBW0)]
        public void OP_TBW0() => _OP_TBW(0);

        [GpuInstructionAttribute(GpuOpCodes.TBW1)]
        public void OP_TBW1() => _OP_TBW(1);

        [GpuInstructionAttribute(GpuOpCodes.TBW2)]
        public void OP_TBW2() => _OP_TBW(2);

        [GpuInstructionAttribute(GpuOpCodes.TBW3)]
        public void OP_TBW3() => _OP_TBW(3);

        [GpuInstructionAttribute(GpuOpCodes.TBW4)]
        public void OP_TBW4() => _OP_TBW(4);

        [GpuInstructionAttribute(GpuOpCodes.TBW5)]
        public void OP_TBW5() => _OP_TBW(5);

        [GpuInstructionAttribute(GpuOpCodes.TBW6)]
        public void OP_TBW6() => _OP_TBW(6);

        [GpuInstructionAttribute(GpuOpCodes.TBW7)]
        public void OP_TBW7() => _OP_TBW(7);

        private void _OP_TSIZE(int index)
        {
            // Astonishia Story is using normalArgument = 0x1804
            // -> use texture_height = 1 << 0x08 (and not 1 << 0x18)
            //        texture_width  = 1 << 0x04
            // The maximum texture size is 512x512: the exponent value must be [0..9]
            // Maybe a bit flag for something?

            var mipMap = MipMapState(index);
            var widthExp = (int) BitUtils.Extract(Params24, 0, 4);
            var heightExp = (int) BitUtils.Extract(Params24, 8, 4);
            var unknownFlag = BitUtils.Extract(Params24, 15, 1) != 0;
            if (unknownFlag)
            {
                Console.Error.WriteLine("_OP_TSIZE UnknownFlag : 0x{0:X}", Params24);
            }
            widthExp = Math.Min(widthExp, 9);
            heightExp = Math.Min(heightExp, 9);

            mipMap->TextureWidth = (ushort) (1 << widthExp);
            mipMap->TextureHeight = (ushort) (1 << heightExp);
        }

        [GpuInstructionAttribute(GpuOpCodes.TSIZE0)]
        public void OP_TSIZE0() => _OP_TSIZE(0);

        [GpuInstructionAttribute(GpuOpCodes.TSIZE1)]
        public void OP_TSIZE1() => _OP_TSIZE(1);

        [GpuInstructionAttribute(GpuOpCodes.TSIZE2)]
        public void OP_TSIZE2() => _OP_TSIZE(2);

        [GpuInstructionAttribute(GpuOpCodes.TSIZE3)]
        public void OP_TSIZE3() => _OP_TSIZE(3);

        [GpuInstructionAttribute(GpuOpCodes.TSIZE4)]
        public void OP_TSIZE4() => _OP_TSIZE(4);

        [GpuInstructionAttribute(GpuOpCodes.TSIZE5)]
        public void OP_TSIZE5() => _OP_TSIZE(5);

        [GpuInstructionAttribute(GpuOpCodes.TSIZE6)]
        public void OP_TSIZE6() => _OP_TSIZE(6);

        [GpuInstructionAttribute(GpuOpCodes.TSIZE7)]
        public void OP_TSIZE7() => _OP_TSIZE(7);

        [GpuInstructionAttribute(GpuOpCodes.TFLUSH)]
        public void OP_TFLUSH() => GpuDisplayList.GpuProcessor.GpuImpl.TextureFlush(GpuState);

        [GpuInstructionAttribute(GpuOpCodes.TSYNC)]
        public void OP_TSYNC() => GpuDisplayList.GpuProcessor.GpuImpl.TextureSync(GpuState);

        [GpuInstructionAttribute(GpuOpCodes.TFLT)]
        public void OP_TFLT()
        {
            TextureState->FilterMinification = (TextureFilter) Param8(0);
            TextureState->FilterMagnification = (TextureFilter) Param8(8);
        }

        [GpuInstructionAttribute(GpuOpCodes.TWRAP)]
        public void OP_TWRAP()
        {
            TextureState->WrapU = (WrapMode) Param8(0);
            TextureState->WrapV = (WrapMode) Param8(8);
        }

        [GpuInstructionAttribute(GpuOpCodes.TFUNC)]
        public void OP_TFUNC()
        {
            TextureState->Effect = (TextureEffect) Param8(0);
            TextureState->ColorComponent = (TextureColorComponent) Param8(8);
            TextureState->Fragment2X = (Param8(16) != 0);

            //Console.WriteLine(TextureState->Effect);
        }

        [GpuInstructionAttribute(GpuOpCodes.USCALE)]
        public void OP_USCALE() => GpuState->TextureMappingState.TextureState.ScaleU = Float1;

        [GpuInstructionAttribute(GpuOpCodes.VSCALE)]
        public void OP_VSCALE() => GpuState->TextureMappingState.TextureState.ScaleV = Float1;

        [GpuInstructionAttribute(GpuOpCodes.UOFFSET)]
        public void OP_UOFFSET() => GpuState->TextureMappingState.TextureState.OffsetU = Float1;

        [GpuInstructionAttribute(GpuOpCodes.VOFFSET)]
        public void OP_VOFFSET() => GpuState->TextureMappingState.TextureState.OffsetV = Float1;

        [GpuInstructionAttribute(GpuOpCodes.TEC)]
        public void OP_TEC() => GpuState->TextureMappingState.TextureEnviromentColor.SetRGB_A1(Params24);

        [GpuInstructionAttribute(GpuOpCodes.TEXTURE_ENV_MAP_MATRIX)]
        public void OP_TEXTURE_ENV_MAP_MATRIX()
        {
            GpuState->TextureMappingState.ShadeU = (short) BitUtils.Extract(Params24, 0, 2);
            GpuState->TextureMappingState.ShadeV = (short) BitUtils.Extract(Params24, 8, 2);
        }

        [GpuInstructionAttribute(GpuOpCodes.TMAP)]
        public void OP_TMAP()
        {
            GpuState->TextureMappingState.TextureMapMode = (TextureMapMode) Param8(0);
            GpuState->TextureMappingState.TextureProjectionMapMode = (TextureProjectionMapMode) Param8(8);
            GpuState->VertexState.Type.NormalCount = GpuState->TextureMappingState.GetTextureComponentsCount();
        }

        [GpuInstructionAttribute(GpuOpCodes.TBIAS)]
        public void OP_TBIAS()
        {
            GpuState->TextureMappingState.LevelMode = (TextureLevelMode) Param8(0);
            GpuState->TextureMappingState.MipmapBias = Param8(16) / 16.0f;
        }

        [GpuInstructionAttribute(GpuOpCodes.TSLOPE)]
        public void OP_TSLOPE() => GpuState->TextureMappingState.SlopeLevel = Float1;

        [GpuInstructionAttribute(GpuOpCodes.TRXSBP)]
        public void OP_TRXSBP() => GpuState->TextureTransferState.SourceAddress.Low24 = Params24;

        [GpuInstructionAttribute(GpuOpCodes.TRXSBW)]
        public void OP_TRXSBW()
        {
            GpuState->TextureTransferState.SourceAddress.High8 = Params24 << 8;
            GpuState->TextureTransferState.SourceLineWidth = (ushort) Extract(0, 16);
            GpuState->TextureTransferState.SourceX = 0;
            GpuState->TextureTransferState.SourceY = 0;
        }

        [GpuInstructionAttribute(GpuOpCodes.TRXSPOS)]
        public void OP_TRXSPOS()
        {
            GpuState->TextureTransferState.SourceX = (ushort) Extract(10 * 0, 10);
            GpuState->TextureTransferState.SourceY = (ushort) Extract(10 * 1, 10);
        }

        [GpuInstructionAttribute(GpuOpCodes.TRXDBP)]
        public void OP_TRXDBP() => GpuState->TextureTransferState.DestinationAddress.Low24 = Params24;

        [GpuInstructionAttribute(GpuOpCodes.TRXDBW)]
        public void OP_TRXDBW()
        {
            ref var textureTransferStateStruct = ref GpuState->TextureTransferState;
            textureTransferStateStruct.DestinationAddress.High8 = Params24 << 8;
            textureTransferStateStruct.DestinationLineWidth = (ushort) Extract(0, 16);
            textureTransferStateStruct.DestinationX = 0;
            textureTransferStateStruct.DestinationY = 0;
        }

        [GpuInstructionAttribute(GpuOpCodes.TRXDPOS)]
        public void OP_TRXDPOS()
        {
            GpuState->TextureTransferState.DestinationX = (ushort) Extract(10 * 0, 10);
            GpuState->TextureTransferState.DestinationY = (ushort) Extract(10 * 1, 10);
        }

        [GpuInstructionAttribute(GpuOpCodes.TRXSIZE)]
        public void OP_TRXSIZE()
        {
            GpuState->TextureTransferState.Width = (ushort) (Extract(10 * 0, 10) + 1);
            GpuState->TextureTransferState.Height = (ushort) (Extract(10 * 1, 10) + 1);
        }

        [GpuInstructionAttribute(GpuOpCodes.TRXKICK)]
        public void OP_TRXKICK()
        {
            GpuState->TextureTransferState.TexelSize = (TextureTransferStateStruct.TexelSizeEnum) Extract(0, 1);
            GpuDisplayList.GpuProcessor.GpuImpl.Transfer(GpuDisplayList.GpuStateStructPointer);
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x03)]
        public void OP_Unknown0x03()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x0D)]
        public void OP_Unknown0x0D()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x11)]
        public void OP_Unknown0x11()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x29)]
        public void OP_Unknown0x29()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x34)]
        public void OP_Unknown0x34()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x35)]
        public void OP_Unknown0x35()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x39)]
        public void OP_Unknown0x39()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x4E)]
        public void OP_Unknown0x4E()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x4F)]
        public void OP_Unknown0x4F()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x52)]
        public void OP_Unknown0x52()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x59)]
        public void OP_Unknown0x59()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0x5A)]
        public void OP_Unknown0x5A()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xB6)]
        public void OP_Unknown0xB6()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xB7)]
        public void OP_Unknown0xB7()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xD1)]
        public void OP_Unknown0xD1()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xED)]
        public void OP_Unknown0xED()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xEF)]
        public void OP_Unknown0xEF()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF0)]
        public void OP_Unknown0xF0()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF1)]
        public void OP_Unknown0xF1()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF2)]
        public void OP_Unknown0xF2()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF3)]
        public void OP_Unknown0xF3()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF4)]
        public void OP_Unknown0xF4()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF5)]
        public void OP_Unknown0xF5()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF6)]
        public void OP_Unknown0xF6()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF7)]
        public void OP_Unknown0xF7()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF8)]
        public void OP_Unknown0xF8()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xF9)]
        public void OP_Unknown0xF9()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xFA)]
        public void OP_Unknown0xFA()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xFB)]
        public void OP_Unknown0xFB()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xFC)]
        public void OP_Unknown0xFC()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xFD)]
        public void OP_Unknown0xFD()
        {
        }

        [GpuOpCodesNotImplemented]
        [GpuInstructionAttribute(GpuOpCodes.Unknown0xFE)]
        public void OP_Unknown0xFE()
        {
        }


        [GpuInstructionAttribute(GpuOpCodes.Dummy)]
        public void OP_Dummy()
        {
        }

        [GpuInstructionAttribute(GpuOpCodes.UNKNOWN)]
        public void OP_UNKNOWN() => Console.WriteLine("Unhandled GpuOpCode: {0} : {1:X}", OpCode, Params24);
    }
}
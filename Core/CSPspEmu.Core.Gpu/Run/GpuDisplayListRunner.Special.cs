using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
    {
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
        // ReSharper disable once UnusedMember.Global
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

        /// <summary>
        /// SCISSOR end (2)
        /// </summary>
        // ReSharper disable once UnusedMember.Global
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
    }
}
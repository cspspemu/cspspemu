using CSharpUtils;
using CSPspEmu.Core.Gpu.State;

using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
    {
        public void OP_NOP()
        {
        }

        public void OP_BASE() => GpuState->BaseAddress = ((Params24 << 8) & 0xff000000);
        public void OP_OFFSET_ADDR() => GpuState->BaseOffset = (Params24 << 8);

        [GpuOpCodesNotImplemented]
        public void OP_ORIGIN_ADDR() => GpuState->BaseOffset = Pc;
        public void OP_FBP() => GpuState->DrawBufferState.LowAddress = Params24;

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

        public void OP_PSM() => GpuState->DrawBufferState.Format = (GuPixelFormats) Param8(0);


        public void OP_REGION1()
        {
            var x1 = (short) BitUtils.Extract(Params24, 0, 10);
            var y1 = (short) BitUtils.Extract(Params24, 10, 10);
            GpuState->Viewport.RegionTopLeft.X = x1;
            GpuState->Viewport.RegionTopLeft.Y = y1;
        }

        //[GpuOpCodesNotImplemented]
        // ReSharper disable once UnusedMember.Global
        public void OP_REGION2()
        {
            var x2 = (short) BitUtils.Extract(Params24, 0, 10);
            var y2 = (short) BitUtils.Extract(Params24, 10, 10);
            GpuState->Viewport.RegionBottomRight.X = (short) (x2 + 1);
            GpuState->Viewport.RegionBottomRight.Y = (short) (y2 + 1);
        }

        public void OP_SCISSOR1()
        {
            GpuState->ClipPlaneState.Scissor.Left = (short) BitUtils.Extract(Params24, 0, 10);
            GpuState->ClipPlaneState.Scissor.Top = (short) BitUtils.Extract(Params24, 10, 10);
        }

        /// <summary>
        /// SCISSOR end (2)
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void OP_SCISSOR2()
        {
            GpuState->ClipPlaneState.Scissor.Right = (short) BitUtils.Extract(Params24, 0, 10);
            GpuState->ClipPlaneState.Scissor.Bottom = (short) BitUtils.Extract(Params24, 10, 10);
        }

        public void OP_XSCALE() => GpuState->Viewport.Scale.X = Float1 * 2;
        public void OP_YSCALE() => GpuState->Viewport.Scale.Y = -Float1 * 2;
        public void OP_ZSCALE() => GpuState->Viewport.Scale.Z = Float1;
        public void OP_XPOS() => GpuState->Viewport.Position.X = Float1;
        public void OP_YPOS() => GpuState->Viewport.Position.Y = Float1;
        public void OP_ZPOS() => GpuState->Viewport.Position.Z = BitUtils.ExtractUnsignedScaled(Params24, 0, 16, 1.0f);
        public void OP_OFFSETX() => GpuState->Offset.X = (short) BitUtils.Extract(Params24, 0, 4);
        public void OP_OFFSETY() => GpuState->Offset.Y = (short) BitUtils.Extract(Params24, 0, 4);
        public void OP_FFACE() => GpuState->BackfaceCullingState.FrontFaceDirection = (FrontFaceDirectionEnum) Params24;
        public void OP_SHADE() => GpuState->ShadeModel = (ShadingModelEnum) Params24;
        public void OP_LOE() => GpuState->LogicalOperationState.Enabled = Bool1;
        public void OP_LOP() => GpuState->LogicalOperationState.Operation = (LogicalOperationEnum) Param8(0);
    }
}
#define PRIM_BATCH

using CSharpPlatform;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using System;
using CSPspEmu.Core.Gpu.VertexReading;

namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
    {
        /**
         * Set the current clear-color
         *
         * @param color - Color to clear with
         **/
        // void sceGuClearColor(unsigned int color);

        /**
         * Set the current clear-depth
         *
         * @param depth - Set which depth to clear with (0x0000-0xffff)
         **/
        // void sceGuClearDepth(unsigned int depth);

        /**
         * Set the current stencil clear value
         *
         * @param stencil - Set which stencil value to clear with (0-255)
         **/
        // void sceGuClearStencil(unsigned int stencil);

        /**
         * Clear current drawbuffer
         *
         * Available clear-flags are (OR them together to get final clear-mode):
         *   - GU_COLOR_BUFFER_BIT   - Clears the color-buffer
         *   - GU_STENCIL_BUFFER_BIT - Clears the stencil-buffer
         *   - GU_DEPTH_BUFFER_BIT   - Clears the depth-buffer
         *
         * @param flags - Which part of the buffer to clear
         **/
        // void sceGuClear(int flags);

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

        /**
         * Draw array of vertices forming primitives
         *
         * Available primitive-types are:
         *   - GU_POINTS         - Single pixel points (1 vertex per primitive)
         *   - GU_LINES          - Single pixel lines (2 vertices per primitive)
         *   - GU_LINE_STRIP     - Single pixel line-strip (2 vertices for the first primitive, 1 for every following)
         *   - GU_TRIANGLES      - Filled triangles (3 vertices per primitive)
         *   - GU_TRIANGLE_STRIP - Filled triangles-strip (3 vertices for the first primitive, 1 for every following)
         *   - GU_TRIANGLE_FAN   - Filled triangle-fan (3 vertices for the first primitive, 1 for every following)
         *   - GU_SPRITES        - Filled blocks (2 vertices per primitive)
         *
         * The vertex-type decides how the vertices align and what kind of information they contain.
         * The following flags are ORed together to compose the final vertex format:
         *   - GU_TEXTURE_8BIT   - 8-bit texture coordinates
         *   - GU_TEXTURE_16BIT  - 16-bit texture coordinates
         *   - GU_TEXTURE_32BITF - 32-bit texture coordinates (float)
         *
         *   - GU_COLOR_5650     - 16-bit color (R5G6B5A0)
         *   - GU_COLOR_5551     - 16-bit color (R5G5B5A1)
         *   - GU_COLOR_4444     - 16-bit color (R4G4B4A4)
         *   - GU_COLOR_8888     - 32-bit color (R8G8B8A8)
         *
         *   - GU_NORMAL_8BIT    - 8-bit normals
         *   - GU_NORMAL_16BIT   - 16-bit normals
         *   - GU_NORMAL_32BITF  - 32-bit normals (float)
         *
         *   - GU_VERTEX_8BIT    - 8-bit vertex position
         *   - GU_VERTEX_16BIT   - 16-bit vertex position
         *   - GU_VERTEX_32BITF  - 32-bit vertex position (float)
         *
         *   - GU_WEIGHT_8BIT    - 8-bit weights
         *   - GU_WEIGHT_16BIT   - 16-bit weights
         *   - GU_WEIGHT_32BITF  - 32-bit weights (float)
         *
         *   - GU_INDEX_8BIT     - 8-bit vertex index
         *   - GU_INDEX_16BIT    - 16-bit vertex index
         *
         *   - GU_WEIGHTS(n)     - Number of weights (1-8)
         *   - GU_VERTICES(n)    - Number of vertices (1-8)
         *
         *   - GU_TRANSFORM_2D   - Coordinate is passed directly to the rasterizer
         *   - GU_TRANSFORM_3D   - Coordinate is transformed before passed to rasterizer
         *
         * @note Every vertex has to be aligned to the maxium size of all of its component.
         *
         * Vertex order:
         * [for vertices(1-8)]
         *     [weights (0-8)]
         *     [texture uv]
         *     [color]
         *     [normal]
         *     [vertex]
         * [/for]
         *
         * @par Example: Render 400 triangles, with floating-point texture coordinates, and floating-point position, no indices
         *
         * <code>
         *     sceGuDrawArray(GU_TRIANGLES, GU_TEXTURE_32BITF | GU_VERTEX_32BITF, 400 * 3, 0, vertices);
         * </code>
         *
         * @param prim     - What kind of primitives to render
         * @param vtype    - Vertex type to process
         * @param count    - How many vertices to process
         * @param indices  - Optional pointer to an index-list
         * @param vertices - Pointer to a vertex-list
         **/
        //void sceGuDrawArray(int prim, int vtype, int count, const void* indices, const void* vertices);

        // Vertex Type
        public void OP_VTYPE() => GpuState->VertexState.Type.Value = Params24;

        public void OP_RNORM() => GpuState->VertexState.Type.ReversedNormal = Bool1;
        public void OP_VADDR() => GpuState->VertexAddress = Params24;
        public void OP_IADDR() => GpuState->IndexAddress = Params24;

        /// <summary>
        /// Bezier Patch Kick
        /// </summary>
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
                    p.Position = Vector4f.Zero;
                    p.Normal = Vector4f.Zero;

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

            GpuDisplayList.GpuProcessor.GpuImpl.BeforeDraw(GpuDisplayList.GpuStateStructPointer);
            GpuDisplayList.GpuProcessor.GpuImpl.DrawCurvedSurface(GlobalGpuState, GpuDisplayList.GpuStateStructPointer,
                patch, uCount, vCount);
        }

        int _primCount;

        /// <summary>
        /// Primitive Kick - draw PRIMitive
        /// </summary>
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
			GpuDisplayList.GpuProcessor.GpuImpl.Prim(GlobalGpuState, GpuDisplayList.GpuStateStructPointer, PrimitiveType, VertexCount);
			GpuDisplayList.GpuProcessor.GpuImpl.PrimEnd(GlobalGpuState, GpuDisplayList.GpuStateStructPointer);
#endif
        }
    }
}
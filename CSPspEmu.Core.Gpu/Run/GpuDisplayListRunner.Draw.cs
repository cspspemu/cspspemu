using CSPspEmu.Core.Gpu.State;
using Mono.Simd;

namespace CSPspEmu.Core.Gpu.Run
{
	public unsafe sealed partial class GpuDisplayListRunner
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
				GpuState->ClearFlags = (ClearBufferSet)Param8(8);
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
		public void OP_VTYPE()
		{
			GpuState->VertexState.Type.Value = Params24;
			//gpu.state.vertexType.v  = command.extract!(uint, 0, 24);
			//writefln("VTYPE:%032b", command.param24);
			//writefln("     :%d", gpu.state.vertexType.position);
		}

		//[GpuOpCodesNotImplemented]
		// Reversed Normal
		public void OP_RNORM()
		{
			GpuState->VertexState.Type.ReversedNormal = Bool1;
		}


		// Vertex List (Base Address)
		public void OP_VADDR()
		{
			// + or |?
			GpuState->VertexAddress = (
				Params24
			);
		}

		// Index List (Base Address)
		public void OP_IADDR()
		{
			GpuState->IndexAddress = (
				Params24
			);
		}

		/// <summary>
		/// Bezier Patch Kick
		/// </summary>
		public void OP_BEZIER()
		{
			var UCount = Param8(0);
			var VCount = Param8(8);

			DrawBezier(UCount, VCount);
		}

		static private float[] BernsteinCoeff(float u)
		{
			float uPow2 = u * u;
			float uPow3 = uPow2 * u;
			float u1 = 1 - u;
			float u1Pow2 = u1 * u1;
			float u1Pow3 = u1Pow2 * u1;

			return new float[] {
				u1Pow3,
				3 * u * u1Pow2,
				3 * uPow2 * u1,
				uPow3,
			};
		}

		private void PointMultAdd(ref VertexInfo dest, ref VertexInfo src, float f)
		{
			dest.Position += src.Position * f;
			dest.Texture += src.Texture * f;
			dest.Color += src.Color * f;
			dest.Normal += src.Normal * f;
		}

		private VertexInfo[,] GetControlPoints(int UCount, int VCount)
		{
			var ControlPoints = new VertexInfo[UCount, VCount];

			var VertexPtr = (byte*)GpuDisplayList.GpuProcessor.Memory.PspAddressToPointerSafe(GlobalGpuState.GetAddressRelativeToBaseOffset(GpuState->VertexAddress), 0);
			var VertexReader = new VertexReader();
			VertexReader.SetVertexTypeStruct(GpuState->VertexState.Type, VertexPtr);

			for (int u = 0; u < UCount; u++)
			{
				for (int v = 0; v < VCount; v++)
				{
					ControlPoints[u, v] = VertexReader.ReadVertex(v * UCount + u);
					//Console.WriteLine("getControlPoints({0}, {1}) : {2}", u, v, controlPoints[u, v]);
				}
			}
			return ControlPoints;
		}

		private void DrawBezier(int UCount, int VCount)
		{
			var DivS = GpuState->PatchState.DivS;
			var DivT = GpuState->PatchState.DivT;

			if ((UCount - 1) % 3 != 0 || (VCount - 1) % 3 != 0)
			{
				Logger.Warning("Unsupported bezier parameters ucount=" + UCount + " vcount=" + VCount);
				return;
			}
			if (DivS <= 0 || DivT <= 0)
			{
				Logger.Warning("Unsupported bezier patches patch_div_s=" + DivS + " patch_div_t=" + DivT);
				return;
			}

			//initRendering();
			//boolean useTexture = context.vinfo.texture != 0 || context.textureFlag.isEnabled();
			//boolean useNormal = context.lightingFlag.isEnabled();

			var anchors = GetControlPoints(UCount, VCount);

			// Don't capture the ram if the vertex list is embedded in the display list. TODO handle stall_addr == 0 better
			// TODO may need to move inside the loop if indices are used, or find the largest index so we can calculate the size of the vertex list
			/*
			if (State.captureGeNextFrame && !isVertexBufferEmbedded()) {
				Logger.Info("Capture drawBezier");
				CaptureManager.captureRAM(context.vinfo.ptr_vertex, context.vinfo.vertexSize * ucount * vcount);
			}
			*/

			// Generate patch VertexState.
			var Patch = new VertexInfo[DivS + 1, DivT + 1];

			// Number of patches in the U and V directions
			int upcount = UCount / 3;
			int vpcount = VCount / 3;

			float[][] ucoeff = new float[DivS + 1][];

			for (int j = 0; j <= DivT; j++)
			{
				float vglobal = (float)j * vpcount / (float)DivT;

				int vpatch = (int)vglobal; // Patch number
				float v = vglobal - vpatch;
				if (j == DivT)
				{
					vpatch--;
					v = 1.0f;
				}
				float[] vcoeff = BernsteinCoeff(v);

				for (int i = 0; i <= DivS; i++)
				{
					float uglobal = (float)i * upcount / (float)DivS;
					int upatch = (int)uglobal;
					float u = uglobal - upatch;
					if (i == DivS)
					{
						upatch--;
						u = 1.0f;
					}
					ucoeff[i] = BernsteinCoeff(u);

					var p = default(VertexInfo);
					p.Position = Vector4f.Zero;
					p.Normal = Vector4f.Zero;

					for (int ii = 0; ii < 4; ++ii)
					{
						for (int jj = 0; jj < 4; ++jj)
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

					Patch[i, j] = p;

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

			GpuDisplayList.GpuProcessor.GpuImpl.DrawCurvedSurface(GlobalGpuState, GpuDisplayList.GpuStateStructPointer, Patch, UCount, VCount);
		}

		/// <summary>
		/// Primitive Kick - draw PRIMitive
		/// </summary>
		public void OP_PRIM()
		{
			var PrimitiveType = (GuPrimitiveType)Param8(16);
			var VertexCount = Param16(0);

			//Console.WriteLine("PRIM: {0}, {1}", PrimitiveType, VertexCount);

			GpuDisplayList.GpuProcessor.GpuImpl.Prim(GlobalGpuState, GpuDisplayList.GpuStateStructPointer, PrimitiveType, VertexCount);
		}
	}
}

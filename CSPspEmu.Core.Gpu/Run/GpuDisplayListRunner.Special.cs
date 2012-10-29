using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu.State.SubStates;

namespace CSPspEmu.Core.Gpu.Run
{
	public unsafe sealed partial class GpuDisplayListRunner
	{
		/**
		 * Set draw buffer parameters (and store in context for buffer-swap)
		 *
		 * Available pixel formats are:
		 *   - GU_PSM_5650
		 *   - GU_PSM_5551
		 *   - GU_PSM_4444
		 *   - GU_PSM_8888
		 *
		 * @par Example: Setup a standard 16-bit draw buffer
		 * @code
		 * sceGuDrawBuffer(GU_PSM_5551,(void*)0,512);
		 * @endcode
		 *
		 * @param psm - Pixel format to use for rendering (and display)
		 * @param fbp - VRAM pointer to where the draw buffer starts
		 * @param fbw - Frame buffer width (block aligned)
		 **/
		// void sceGuDrawBuffer(int psm, void* fbp, int fbw);

		public void OP_NOP()
		{
		}

		/// <summary>
		/// Base Address Register
		/// </summary>
		public void OP_BASE()
		{
			GlobalGpuState.Base = ((Params24 << 8) & 0xff000000);
			//GpuState->BaseAddress = (Params24 << 8);
		}

		/// <summary>
		/// 
		/// </summary>
		public void OP_OFFSET_ADDR()
		{
			GlobalGpuState.BaseOffset = (int)(Params24 << 8);
		}

		/// <summary>
		/// 
		/// </summary>
		[GpuOpCodesNotImplemented]
		public void OP_ORIGIN_ADDR()
		{
			GlobalGpuState.BaseOffset = (int)PC; // ?
		}

		/// <summary>
		/// Frame Buffer Pointer
		/// </summary>
		public void OP_FBP()
		{
			GpuState->DrawBufferState.LowAddress = Params24;
		}

		/// <summary>
		/// Frame Buffer Width
		/// </summary>
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

		/// <summary>
		/// frame buffer Pixel Storage Mode
		/// </summary>
		public void OP_PSM()
		{
			GpuState->DrawBufferState.Format = (GuPixelFormats)Param8(0);
		}

		// void drawRegion(int x, int y, int width, int height)
		// void sceGuDispBuffer(int width, int height, void* dispbp, int dispbw)
		//[GpuOpCodesNotImplemented]
		public void OP_REGION1()
		{
			var X1 = (short)BitUtils.Extract(Params24, 0, 10);
			var Y1 = (short)BitUtils.Extract(Params24, 10, 10);
			GpuState->Viewport.RegionTopLeft.X = X1;
			GpuState->Viewport.RegionTopLeft.Y = Y1;
		}

		//[GpuOpCodesNotImplemented]
		public void OP_REGION2()
		{
			var X2 = (short)BitUtils.Extract(Params24, 0, 10);
			var Y2 = (short)BitUtils.Extract(Params24, 10, 10);
			GpuState->Viewport.RegionBottomRight.X = X2;
			GpuState->Viewport.RegionBottomRight.Y = Y2;
		}

		/**
		 * Set what to scissor within the current framebuffer
		 *
		 * Note that scissoring is only performed if the custom scissoring is enabled (GU_SCISSOR_TEST)
		 *
		 * @param x - Left of scissor region
		 * @param y - Top of scissor region
		 * @param stopX - Right of scissor region
		 * @param stopY - Bottom of scissor region
		 **/
		// void sceGuScissor(int x, int y, int stopX, int stopY); // OP_SCISSOR1 + OP_SCISSOR2

		/// <summary>
		/// SCISSOR start (1)
		/// </summary>
		public void OP_SCISSOR1()
		{
			GpuState->ClipPlaneState.Scissor.Left = BitUtils.Extract(Params24, 0, 10);
			GpuState->ClipPlaneState.Scissor.Top = BitUtils.Extract(Params24, 10, 10);
		}

		/// <summary>
		/// SCISSOR end (2)
		/// </summary>
		public void OP_SCISSOR2()
		{
			GpuState->ClipPlaneState.Scissor.Right = BitUtils.Extract(Params24, 0, 10);
			GpuState->ClipPlaneState.Scissor.Bottom = BitUtils.Extract(Params24, 10, 10);
		}

		/**
		 * Set current viewport
		 *
		 * @par Example: Setup a viewport of size (480,272) with origo at (2048,2048)
		 * @code
		 * sceGuViewport(2048,2048,480,272);
		 * @endcode
		 *
		 * @param cx - Center for horizontal viewport
		 * @param cy - Center for vertical viewport
		 * @param width - Width of viewport
		 * @param height - Height of viewport
		 **/
		// void sceGuViewport(int cx, int cy, int width, int height); // OP_XSCALE + OP_YSCALE + OP_XPOS + OP_YPOS
		// sendCommandf(66,(float)(width>>1));
		// sendCommandf(67,(float)((-height)>>1));
		// sendCommandf(69,(float)cx);
		// sendCommandf(70,(float)cy);

		public void OP_XSCALE()
		{
			GpuState->Viewport.Scale.X = Float1 * 2;
			//Console.Error.WriteLine("OP_XSCALE: {0}", GpuState->Viewport.Scale.X);
		}
		public void OP_YSCALE()
		{
			GpuState->Viewport.Scale.Y = -Float1 * 2;
			//Console.Error.WriteLine("OP_YSCALE: {0}", GpuState->Viewport.Scale.Y);
		}
		public void OP_ZSCALE()
		{
			GpuState->Viewport.Scale.Z = Float1;
			//gpu.state.viewport.sz = command.extractFixedFloat!(0, 16);
		}

		public void OP_XPOS()
		{
			GpuState->Viewport.Position.X = Float1;
			//Console.Error.WriteLine("OP_XPOS: {0}", GpuState->Viewport.Position.X);
		}
		public void OP_YPOS()
		{
			GpuState->Viewport.Position.Y = Float1;
			//Console.Error.WriteLine("OP_YPOS: {0}", GpuState->Viewport.Position.Y);
		}
		//[GpuOpCodesNotImplemented]
		public void OP_ZPOS()
		{
			GpuState->Viewport.Position.Z = BitUtils.ExtractUnsignedScaled(Params24, 0, 16, 1.0f);
			//Console.Error.WriteLine("OP_ZPOS: {0}", GpuState->Viewport.Position.Z);
			//gpu.state.viewport.pz = command.extractFixedFloat!(0, 16);
		}

		//[GpuOpCodesNotImplemented]
		public void OP_OFFSETX()
		{
			GpuState->Offset.X = (short)BitUtils.Extract(Params24, 0, 4);
			//Console.Error.WriteLine("{0}", GpuState->Offset.X);
			//gpu.state.offsetX = command.extract!(uint, 0, 4);
		}
		//[GpuOpCodesNotImplemented]
		public void OP_OFFSETY()
		{
			GpuState->Offset.Y = (short)BitUtils.Extract(Params24, 0, 4);
			//gpu.state.offsetY = command.extract!(uint, 0, 4);
		}

		/**
		 * Set the current face-order (for culling)
		 *
		 * This only has effect when culling is enabled (GU_CULL_FACE)
		 *
		 * Culling order can be:
		 *   - GU_CW - Clockwise primitives are not culled
		 *   - GU_CCW - Counter-clockwise are not culled
		 *
		 * @param order - Which order to use
		 **/
		// void sceGuFrontFace(int order); // OP_FFACE
		public void OP_FFACE()
		{
			GpuState->BackfaceCullingState.FrontFaceDirection = (FrontFaceDirectionEnum)Params24;
			//gpu.state.frontFaceDirection = command.extractEnum!(FrontFaceDirection);
		}

		/**
		 * Set how primitives are shaded
		 *
		 * The available shading-methods are:
		 *   - GU_FLAT - Primitives are flatshaded, the last vertex-color takes effet
		 *   - GU_SMOOTH - Primtives are gouraud-shaded, all vertex-colors take effect
		 *
		 * @param mode - Which mode to use
		**/
		// void sceGuShadeModel(int mode); // OP_SHADE
		//[GpuOpCodesNotImplemented]
		public void OP_SHADE()
		{
			GpuState->ShadeModel = (ShadingModelEnum)Params24;
		}

		// Logical Operation Enable (GL_COLOR_LOGIC_OP)
		public void OP_LOE()
		{
			GpuState->LogicalOperationState.Enabled = Bool1;
		}

		/**
		 * Set color logical operation
		 *
		 * Available operations are:
		 *   - GU_CLEAR
		 *   - GU_AND
		 *   - GU_AND_REVERSE 
		 *   - GU_COPY
		 *   - GU_AND_INVERTED
		 *   - GU_NOOP
		 *   - GU_XOR
		 *   - GU_OR
		 *   - GU_NOR
		 *   - GU_EQUIV
		 *   - GU_INVERTED
		 *   - GU_OR_REVERSE
		 *   - GU_COPY_INVERTED
		 *   - GU_OR_INVERTED
		 *   - GU_NAND
		 *   - GU_SET
		 *
		 * This operation only has effect if GU_COLOR_LOGIC_OP is enabled.
		 *
		 * @param op - Operation to execute
		 **/
		// void sceGuLogicalOp(int op); // OP_LOP
		
		/// <summary>
		/// Logical Operation
		/// </summary>
		public void OP_LOP()
		{
			GpuState->LogicalOperationState.Operation = (LogicalOperationEnum)Param8(0);
			//gpu.state.logicalOperation.operation = command.extractEnum!(LogicalOperation);
		}

		/**
		 * Set virtual coordinate offset
		 *
		 * The PSP has a virtual coordinate-space of 4096x4096, this controls where rendering is performed
		 * 
		 * @par Example: Center the virtual coordinate range
		 * @code
		 * sceGuOffset(2048-(480/2),2048-(480/2));
		 * @endcode
		 *
		 * @param x - Offset (0-4095)
		 * @param y - Offset (0-4095)
		 */
		//void sceGuOffset(unsigned int x, unsigned int y); // OP_OFFSETX + OP_OFFSETY
	}
}
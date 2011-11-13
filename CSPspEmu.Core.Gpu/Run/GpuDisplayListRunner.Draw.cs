using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;
using OpenTK.Graphics.OpenGL;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
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
				GpuState[0].ClearFlags = (ClearBufferSet)Param8(8);
				GpuState[0].ClearingMode = true;
			}
			// Stop the clearing mode.
			else {
				GpuState[0].ClearingMode = false;
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
			GpuState[0].VertexState.Type.Value = Params24;
			//gpu.state.vertexType.v  = command.extract!(uint, 0, 24);
			//writefln("VTYPE:%032b", command.param24);
			//writefln("     :%d", gpu.state.vertexType.position);
		}

		// Vertex List (Base Address)
		public void OP_VADDR()
		{
			// + or |?
			GpuState[0].VertexAddress = (
				GpuDisplayList.GpuStateStructPointer[0].BaseAddress | Params24
			);
		}

		// Index List (Base Address)
		public void OP_IADDR()
		{
			GpuState[0].IndexAddress = (
				GpuDisplayList.GpuStateStructPointer[0].BaseAddress | Params24
			);
		}
	
		/*
		// http://en.wikipedia.org/wiki/Bernstein_polynomial
		float[4] bernsteinCoefficients(float u) {
    		static if (false) {
	    		float uPow1  = u;
				float uPow2  = uPow1 * uPow1;
				float uPow3  = uPow2 * uPow1;
	
				// Complementary.
				float u1Pow1 = 1 - u;
				float u1Pow2 = Pow1 * Pow1;
				float u1Pow3 = u1Pow2 * Pow1;
	
	    		float[4] ret = [
	    			(u1Pow3),
	    			(3 * uPow1 * u1Pow2),
	    			(3 * uPow2 * u1Pow1),
	    			(uPow3)
	    		];
			} else {
	    		float u0 = u - 0;
	    		float u1 = 1 - u;
	    		float[4] ret = [
	    			(u1 ^^ 3),
	    			(3 * (u0 ^^ 1) * (u1 ^^ 2)),
	    			(3 * (u1 ^^ 1) * (u0 ^^ 2)),
	    			(u0 ^^ 3)
	    		];
			}

			return ret;
		}
		*/

		// Bezier Patch Kick
		[GpuOpCodesNotImplemented]
		public void OP_BEZIER()
		{
		}
	
		// draw PRIMitive
		// Primitive Kick
		[GpuOpCodesNotImplemented]
		public void OP_PRIM()
		{
			/*
			auto primitiveType = command.extractEnum!(PrimitiveType, 16);
			auto vertexType    = gpu.state.vertexType;
			int  vertexSize    = vertexType.vertexSize;
			auto vertexCount   = command.param16;
			*/
			var PrimitiveType = (PrimitiveType)Param8(16);
			var VertexCount = Param16(0);

			GpuDisplayList.GpuProcessor.GpuImpl.Prim(GpuDisplayList.GpuStateStructPointer, PrimitiveType, VertexCount);
		}

		/**
		 * Image transfer using the GE
		 *
		 * @note Data must be aligned to 1 quad word (16 bytes)
		 *
		 * @par Example: Copy a fullscreen 32-bit image from RAM to VRAM
		 *
		 * <code>
		 *     sceGuCopyImage(GU_PSM_8888,0,0,480,272,512,pixels,0,0,512,(void*)(((unsigned int)framebuffer)+0x4000000));
		 * </code>
		 *
		 * @param psm    - Pixel format for buffer
		 * @param sx     - Source X
		 * @param sy     - Source Y
		 * @param width  - Image width
		 * @param height - Image height
		 * @param srcw   - Source buffer width (block aligned)
		 * @param src    - Source pointer
		 * @param dx     - Destination X
		 * @param dy     - Destination Y
		 * @param destw  - Destination buffer width (block aligned)
		 * @param dest   - Destination pointer
		 **/
		// void sceGuCopyImage(int psm, int sx, int sy, int width, int height, int srcw, void* src, int dx, int dy, int destw, void* dest);
		// sendCommandi(178/*OP_TRXSBP*/,((unsigned int)src) & 0xffffff);
		// sendCommandi(179/*OP_TRXSBW*/,((((unsigned int)src) & 0xff000000) >> 8)|srcw);
		// sendCommandi(235/*OP_TRXSPOS*/,(sy << 10)|sx);
		// sendCommandi(180/*OP_TRXDBP*/,((unsigned int)dest) & 0xffffff);
		// sendCommandi(181/*OP_TRXDBW*/,((((unsigned int)dest) & 0xff000000) >> 8)|destw);
		// sendCommandi(236/*OP_TRXDPOS*/,(dy << 10)|dx);
		// sendCommandi(238/*OP_TRXSIZE*/,((height-1) << 10)|(width-1));
		// sendCommandi(234/*OP_TRXKICK*/,(psm ^ 0x03) ? 0 : 1);

		/*struct TextureTransfer {
			uint srcAddress, dstAddress;
			ushort srcLineWidth, dstLineWidth;
			ushort srcX, srcY, dstX, dstY;
			ushort width, height;
		}*/
	
		// TRansfer X Source (Buffer Pointer/Width)/POSition
		[GpuOpCodesNotImplemented]
		public void OP_TRXSBP()
		{
			/*
			with (gpu.state.textureTransfer) {
				srcAddress = (srcAddress & 0xFF000000) | command.extract!(uint, 0, 24);
			}
			*/
		}

		[GpuOpCodesNotImplemented]
		public void OP_TRXSBW()
		{
			/*
			with (gpu.state.textureTransfer) {
				srcAddress = (srcAddress & 0x00FFFFFF) | (command.extract!(uint, 16, 8) << 24);
				srcLineWidth = command.extract!(ushort, 0, 16);
				srcX = srcY = 0;
			}
			*/
		}

		[GpuOpCodesNotImplemented]
		public void OP_TRXSPOS()
		{
			/*
			with (gpu.state.textureTransfer) {
				srcX = command.extract!(ushort,  0, 10);
				srcY = command.extract!(ushort, 10, 10);
			}
			*/
		}

		// TRansfer X Destination (Buffer Pointer/Width)/POSition
		[GpuOpCodesNotImplemented]
		public void OP_TRXDBP()
		{
			/*
			with (gpu.state.textureTransfer) {
				dstAddress = (dstAddress & 0xFF000000) | command.extract!(uint, 0, 24);
			}
			*/
		}

		[GpuOpCodesNotImplemented]
		public void OP_TRXDBW()
		{
			/*
			with (gpu.state.textureTransfer) {
				dstAddress = (dstAddress & 0x00FFFFFF) | (command.extract!(uint, 16, 8) << 24);
				dstLineWidth = command.extract!(ushort, 0, 16);
				dstX = dstY = 0;
			}
			*/
		}

		[GpuOpCodesNotImplemented]
		public void OP_TRXDPOS()
		{
			/*
			with (gpu.state.textureTransfer) {
				dstX = command.extract!(ushort,  0, 10);
				dstY = command.extract!(ushort, 10, 10);
			}
			*/
		}

		// TRansfer X SIZE
		[GpuOpCodesNotImplemented]
		public void OP_TRXSIZE()
		{
			/*
			with (gpu.state.textureTransfer) {
				width  = cast(ushort)(command.extract!(ushort,  0, 10) + 1);
				height = cast(ushort)(command.extract!(ushort, 10, 10) + 1);
			}
			*/
		}

		// TRansfer X KICK
		[GpuOpCodesNotImplemented]
		public void OP_TRXKICK()
		{
		}

	}
}

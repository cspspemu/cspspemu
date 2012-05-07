using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		/**
		  * Draw bezier surface
		  *
		  * @param vtype    - Vertex type, look at sceGuDrawArray() for vertex definition
		  * @param ucount   - Number of vertices used in the U direction
		  * @param vcount   - Number of vertices used in the V direction
		  * @param indices  - Pointer to index buffer
		  * @param vertices - Pointer to vertex buffer
		**/
		//void sceGuDrawBezier(int vtype, int ucount, int vcount, const void* indices, const void* vertices);

		/**
		  * Set dividing for patches (beziers and splines)
		  *
		  * @param ulevel - Number of division on u direction
		  * @param vlevel - Number of division on v direction
		**/
		//void sceGuPatchDivide(unsigned int ulevel, unsigned int vlevel);

		//void sceGuPatchFrontFace(unsigned int a0);

		/**
		  * Set primitive for patches (beziers and splines)
		  *
		  * @param prim - Desired primitive type (GU_POINTS | GU_LINE_STRIP | GU_TRIANGLE_STRIP)
		**/
		//void sceGuPatchPrim(int prim);

		//void sceGuDrawSpline(int vtype, int ucount, int vcount, int uedge, int vedge, const void* indices, const void* vertices);

		[GpuOpCodesNotImplemented]
		public void OP_PSUB()
		{
			/*
			gpu.state.patch.div_s = command.extract!(ubyte,  0, 8); 
			gpu.state.patch.div_t = command.extract!(ubyte,  8, 8);
			//gpu.logWarning("OP_PSUB(%f, %f)", params[0], params[1]); 
			*/
		}

		[GpuOpCodesNotImplemented]
		public void OP_PPRIM()
		{
			//gpu.state.patch.type = command.extract!(PatchPrimitiveType, 0);
		}

		[GpuOpCodesNotImplemented]
		public void OP_SPLINE()
		{
			/*
			auto sp_ucount = command.extract!(uint,  0, 8); 
			auto sp_vcount = command.extract!(uint,  8, 8);
			auto sp_utype  = command.extract!(uint, 16, 2);
			auto sp_vtype  = command.extract!(uint, 18, 2);
			gpu.logWarning("OP_SPLINE(%d, %d, %d, %d)", sp_ucount, sp_vcount, sp_utype, sp_vtype);
			*/
		}

		public void OP_PFACE()
		{
			GpuState->PatchCullingState.FaceFlag = (Params24 != 0);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		/**
		 * Set transform matrices
		 *
		 * Available matrices are:
		 *   - GU_PROJECTION - View->Projection matrix
		 *   - GU_VIEW - World->View matrix
		 *   - GU_MODEL - Model->World matrix
		 *   - GU_TEXTURE - Texture matrix
		 *
		 * @param type - Which matrix-type to set
		 * @param matrix - Matrix to load
		 **/
		// void sceGuSetMatrix(int type, const ScePspFMatrix4* matrix);

		public void OP_VMS()
		{
			GpuDisplayList.GpuStateStructPointer[0].VertexState.ViewMatrix.Reset();
		}
		public void OP_VIEW()
		{
			GpuDisplayList.GpuStateStructPointer[0].VertexState.ViewMatrix.Write(Float1);
		}

		public void OP_WMS()
		{
			GpuDisplayList.GpuStateStructPointer[0].VertexState.WorldMatrix.Reset();
		}
		public void OP_WORLD()
		{
			Console.WriteLine("{0:X}, {1}", Params24, Float1);
			GpuDisplayList.GpuStateStructPointer[0].VertexState.WorldMatrix.Write(Float1);
		}

		public void OP_PMS()
		{
			GpuDisplayList.GpuStateStructPointer[0].VertexState.ProjectionMatrix.Reset();
		}
		public void OP_PROJ()
		{
			GpuDisplayList.GpuStateStructPointer[0].VertexState.ProjectionMatrix.Write(Float1);
		}

		/**
		  * Specify skinning matrix entry
		  *
		  * To enable vertex skinning, pass GU_WEIGHTS(n), where n is between
		  * 1-8, and pass available GU_WEIGHT_??? declaration. This will change
		  * the amount of weights passed in the vertex araay, and by setting the skinning,
		  * matrices, you will multiply each vertex every weight and vertex passed.
		  *
		  * Please see sceGuDrawArray() for vertex format information.
		  *
		  * @param index - Skinning matrix index (0-7)
		  * @param matrix - Matrix to set
		**/
		//void sceGuBoneMatrix(unsigned int index, const ScePspFMatrix4* matrix);
		// @TODO : @FIX: @HACK : it defines the position in the matrixes. So we will do a hack there until fixed.
		// http://svn.ps2dev.org/filedetails.php?repname=psp&path=%2Ftrunk%2Fpspsdk%2Fsrc%2Fgu%2FsceGuBoneMatrix.c
		[GpuOpCodesNotImplemented]
		public void OP_BOFS()
		{
			//gpu.state.boneMatrixIndex = command.param16 / 12;
			//gpu.state.boneMatrix[gpu.state.boneMatrixIndex].reset(Matrix.WriteMode.M4x3);
		}

		[GpuOpCodesNotImplemented]
		public void OP_BONE()
		{
			//gpu.state.boneMatrix[gpu.state.boneMatrixIndex].write(command.float1);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State.SubStates;

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
			GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Reset();
		}
		public void OP_VIEW()
		{
			GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Write(Float1);
		}

		public void OP_WMS()
		{
			GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Reset();
		}
		public void OP_WORLD()
		{
			//Console.WriteLine("{0:X}, {1}", Params24, Float1);
			GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Write(Float1);
		}

		public void OP_PMS()
		{
			GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Reset();
		}
		public void OP_PROJ()
		{
			GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Write(Float1);
		}

		private SkinningStateStruct* SkinningState
		{
			get
			{
				return &GpuDisplayList.GpuStateStructPointer->SkinningState;
			}
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
		// @TODO : @FIX: @HACK : it defines the position in the matrixes not the index of the matrix. So we will do a hack there until fixed.
		// http://svn.ps2dev.org/filedetails.php?repname=psp&path=%2Ftrunk%2Fpspsdk%2Fsrc%2Fgu%2FsceGuBoneMatrix.c
		public void OP_BOFS()
		{
			SkinningState->CurrentBoneMatrixIndex = Params24 / 12;
			var BoneMatrices = &SkinningState->BoneMatrix0;
			BoneMatrices[SkinningState->CurrentBoneMatrixIndex].Reset();
		}

		public void OP_BONE()
		{
			var BoneMatrices = &SkinningState->BoneMatrix0;
			BoneMatrices[SkinningState->CurrentBoneMatrixIndex].Write(Float1);
		}
	}
}

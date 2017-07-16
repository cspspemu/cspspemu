using CSPspEmu.Core.Gpu.State.SubStates;

namespace CSPspEmu.Core.Gpu.Run
{
    public sealed unsafe partial class GpuDisplayListRunner
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

        // ReSharper disable once UnusedMember.Global
        public void OP_VMS()
        {
            var startIndex = Params24;
            GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Reset(startIndex);
        }

        // ReSharper disable once UnusedMember.Global
        public void OP_VIEW()
        {
            GpuDisplayList.GpuStateStructPointer->VertexState.ViewMatrix.Write(Float1);
        }

        // ReSharper disable once UnusedMember.Global
        public void OP_WMS()
        {
            var startIndex = Params24;
            GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Reset(startIndex);
        }

        // ReSharper disable once UnusedMember.Global
        public void OP_WORLD()
        {
            //Console.WriteLine("{0:X}, {1}", Params24, Float1);
            //Console.WriteLine(Float1);
            GpuDisplayList.GpuStateStructPointer->VertexState.WorldMatrix.Write(Float1);
        }

        // ReSharper disable once UnusedMember.Global
        public void OP_PMS()
        {
            var startIndex = Params24;
            GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Reset(startIndex);
        }

        // ReSharper disable once UnusedMember.Global
        public void OP_PROJ()
        {
            //Console.WriteLine("PROJ: 0x{0:X}, {1}", Params24, Float1);
            GpuDisplayList.GpuStateStructPointer->VertexState.ProjectionMatrix.Write(Float1);
        }

        private SkinningStateStruct* SkinningState => &GpuDisplayList.GpuStateStructPointer->SkinningState;

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
        // ReSharper disable once UnusedMember.Global
        public void OP_BOFS()
        {
            SkinningState->CurrentBoneIndex = (int) Params24;
            //SkinningState->CurrentBoneMatrixIndex = Params24 / 12;
            //uint StartIndex = Params24 % 12;
            //var BoneMatrices = &SkinningState->BoneMatrix0;
            //BoneMatrices[SkinningState->CurrentBoneMatrixIndex].Reset(StartIndex);
        }

        // ReSharper disable once UnusedMember.Global
        public void OP_BONE()
        {
            var BoneMatrices = &SkinningState->BoneMatrix0;
            //Console.WriteLine("{0}.{1} -> {2}", SkinningState->CurrentBoneIndex / 12, SkinningState->CurrentBoneIndex % 12, Float1);
            BoneMatrices[SkinningState->CurrentBoneIndex / 12].WriteAt(SkinningState->CurrentBoneIndex % 12, Float1);
            SkinningState->CurrentBoneIndex++;
        }
    }
}
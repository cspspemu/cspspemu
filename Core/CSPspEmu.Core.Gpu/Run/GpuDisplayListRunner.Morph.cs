namespace CSPspEmu.Core.Gpu.Run
{
    public unsafe sealed partial class GpuDisplayListRunner
    {
        /**
         * Specify morph weight entry
         *
         * To enable vertex morphing, pass GU_VERTICES(n), where n is between
         * 1-8. This will change the amount of vertices passed in the vertex array,
         * and by setting the morph weights for every vertex entry in the array,
         * you can blend between them.
         *
         * Please see sceGuDrawArray() for vertex format information.
         *
         * @param index  - Morph weight index (0-7)
         * @param weight - Weight to set
        **/
        // void sceGuMorphWeight(int index, float weight);
        /*
        mixin (ArrayOperation("OP_MW_n", 0, 7, q{
            gpu.state.morphWeights[Index] = command.float1;
        }));
        */

        private void _OP_MW(int index)
        {
            (&GpuState->MorphingState.MorphWeight0)[index] = Float1;
        }

        // ReSharper disable once UnusedMember.Global
        public void OP_MW0() => _OP_MW(0);

        // ReSharper disable once UnusedMember.Global
        public void OP_MW1() => _OP_MW(1);

        // ReSharper disable once UnusedMember.Global
        public void OP_MW2() => _OP_MW(2);

        // ReSharper disable once UnusedMember.Global
        public void OP_MW3() => _OP_MW(3);

        // ReSharper disable once UnusedMember.Global
        public void OP_MW4() => _OP_MW(4);

        // ReSharper disable once UnusedMember.Global
        public void OP_MW5() => _OP_MW(5);

        // ReSharper disable once UnusedMember.Global
        public void OP_MW6() => _OP_MW(6);

        // ReSharper disable once UnusedMember.Global
        public void OP_MW7() => _OP_MW(7);
    }
}
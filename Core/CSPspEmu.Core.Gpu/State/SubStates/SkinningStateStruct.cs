using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SkinningStateStruct
    {
        public int CurrentBoneIndex;

        public GpuMatrix4x3Struct BoneMatrix0,
            BoneMatrix1,
            BoneMatrix2,
            BoneMatrix3,
            BoneMatrix4,
            BoneMatrix5,
            BoneMatrix6,
            BoneMatrix7;
    }
}
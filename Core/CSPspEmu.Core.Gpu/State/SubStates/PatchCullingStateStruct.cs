using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PatchCullingStateStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Enabled;

        public bool FaceFlag;
    }
}
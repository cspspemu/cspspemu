using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DepthTestStateStruct
    {
        /// <summary>
        /// depth (Z) Test Enable (GL_DEPTH_TEST)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// TestFunction.GU_ALWAYS
        /// </summary>
        public TestFunctionEnum Function;

        /// <summary>
        /// 0.0 - 1.0
        /// </summary>
        public float RangeNear;

        /// <summary>
        /// 0.0 - 1.0
        /// </summary>
        public float RangeFar;

        /// <summary>
        /// 
        /// </summary>
        public ushort Mask;
    }
}
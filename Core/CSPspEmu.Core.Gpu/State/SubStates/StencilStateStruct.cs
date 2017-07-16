using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StencilStateStruct
    {
        /// <summary>
        /// Stencil Test Enable (GL_STENCIL_TEST)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// 
        /// </summary>
        public TestFunctionEnum Function;

        /// <summary>
        /// 
        /// </summary>
        public byte FunctionRef;

        /// <summary>
        /// 0xFF
        /// </summary>
        public byte FunctionMask;

        /// <summary>
        /// 
        /// </summary>
        public StencilOperationEnum OperationFail;

        /// <summary>
        /// 
        /// </summary>
        public StencilOperationEnum OperationZFail;

        /// <summary>
        /// 
        /// </summary>
        public StencilOperationEnum OperationZPass;
    }
}
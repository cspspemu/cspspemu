using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LogicalOperationStateStruct
    {
        /// <summary>
        /// Logical Operation Enable (GL_COLOR_LOGIC_OP)
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// LogicalOperation.GU_COPY
        /// </summary>
        public LogicalOperationEnum Operation;
    }
}
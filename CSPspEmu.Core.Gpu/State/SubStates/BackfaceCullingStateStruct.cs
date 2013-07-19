using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BackfaceCullingStateStruct
	{
		/// <summary>
		/// Backface Culling Enable (GL_CULL_FACE)
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public FrontFaceDirectionEnum FrontFaceDirection;
	}
}

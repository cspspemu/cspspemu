using CSPspEmu.Core.Types;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ColorTestStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public OutputPixel Ref;

		/// <summary>
		/// 
		/// </summary>
		public OutputPixel Mask;

		/// <summary>
		/// 
		/// </summary>
		public ColorTestFunctionEnum Function;
	}
}

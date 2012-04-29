using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public enum FrontFaceDirectionEnum { CounterClockWise = 0, ClockWise = 1 }

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

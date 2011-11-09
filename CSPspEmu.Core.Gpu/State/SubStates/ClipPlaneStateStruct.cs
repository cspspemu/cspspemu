
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public struct ClipPlaneStateStruct
	{
		/// <summary>
		/// Clip Plane Enable (GL_CLIP_PLANE0)
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public GpuRectStruct Scissor;
	}
}

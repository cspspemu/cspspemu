using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
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

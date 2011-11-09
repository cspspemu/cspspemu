using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State
{
	unsafe public struct GpuMatrixStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public fixed float Values[4 * 4];
	}
}

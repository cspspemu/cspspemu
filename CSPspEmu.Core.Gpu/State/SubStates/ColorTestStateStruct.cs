using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public struct ColorTestStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// [0xFF, 0xFF, 0xFF, 0xFF]
		/// </summary>
		public byte MaskRed;

		/// <summary>
		/// 
		/// </summary>
		public byte MaskGreen;

		/// <summary>
		/// 
		/// </summary>
		public byte MaskBlue;

		/// <summary>
		/// 
		/// </summary>
		public byte MaskAlpha;
	}
}

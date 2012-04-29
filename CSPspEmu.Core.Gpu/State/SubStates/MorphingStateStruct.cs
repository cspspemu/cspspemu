using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	unsafe public struct MorphingStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public float MorphWeight0;

		/// <summary>
		/// 
		/// </summary>
		public float MorphWeight1;

		/// <summary>
		/// 
		/// </summary>
		public float MorphWeight2;

		/// <summary>
		/// 
		/// </summary>
		public float MorphWeight3;

		/// <summary>
		/// 
		/// </summary>
		public float MorphWeight4;

		/// <summary>
		/// 
		/// </summary>
		public float MorphWeight5;

		/// <summary>
		/// 
		/// </summary>
		public float MorphWeight6;

		/// <summary>
		/// 
		/// </summary>
		public float MorphWeight7;
	}
}

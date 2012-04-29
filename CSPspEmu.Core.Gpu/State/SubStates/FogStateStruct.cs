using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FogStateStruct
	{
		/// <summary>
		/// FOG Enable (GL_FOG)
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct Color;

		/// <summary>
		/// 
		/// </summary>
		public float Dist;

		/// <summary>
		/// 
		/// </summary>
		public float End;

		/// <summary>
		/// Default Value: 0.1
		/// </summary>
		public float Density;

		/// <summary>
		/// 
		/// </summary>
		public int Mode;

		/// <summary>
		/// 
		/// </summary>
		public int Hint;
	}
}

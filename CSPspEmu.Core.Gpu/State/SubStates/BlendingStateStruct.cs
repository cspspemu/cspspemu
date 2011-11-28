using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public struct BlendingStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public BlendingOpEnum Equation;

		/// <summary>
		/// 
		/// </summary>
		public BlendingFactor FunctionSource;

		/// <summary>
		/// 
		/// </summary>
		public BlendingFactor FunctionDestination;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct FixColorSource;

		/// <summary>
		/// 
		/// </summary>
		public ColorfStruct FixColorDestination;

		/// <summary>
		/// 
		/// </summary>
		public byte ColorMaskR;

		/// <summary>
		/// 
		/// </summary>
		public byte ColorMaskG;

		/// <summary>
		/// 
		/// </summary>
		public byte ColorMaskB;

		/// <summary>
		/// 
		/// </summary>
		public byte ColorMaskA;
	}
}

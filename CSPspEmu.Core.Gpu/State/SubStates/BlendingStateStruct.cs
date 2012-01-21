using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Utils;

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
		public GuBlendingFactorSource FunctionSource;

		/// <summary>
		/// 
		/// </summary>
		public GuBlendingFactorDestination FunctionDestination;

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
		public PixelFormatDecoder.OutputPixel ColorMask;
	}
}

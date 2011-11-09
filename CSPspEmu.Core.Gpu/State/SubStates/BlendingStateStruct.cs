using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public struct BlendingStateStruct
	{
		public enum BlendingOpEnum
		{
			Add = 0,
			Substract = 1,
			ReverseSubstract = 2,
			Min = 3,
			Max = 4,
			Abs = 5,
		}

		public enum BlendingFactor
		{
			// Source
			GU_SRC_COLOR = 0, GU_ONE_MINUS_SRC_COLOR = 1, GU_SRC_ALPHA = 2, GU_ONE_MINUS_SRC_ALPHA = 3,
			// Dest
			GU_DST_COLOR = 0, GU_ONE_MINUS_DST_COLOR = 1, GU_DST_ALPHA = 4, GU_ONE_MINUS_DST_ALPHA = 5,
			// Both?
			GU_FIX = 10
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public int Equation;

		/// <summary>
		/// 
		/// </summary>
		public int FunctionSource;

		/// <summary>
		/// 
		/// </summary>
		public int FunctionDestination;
	}
}

using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Dynarec
{
	/// <summary>
	/// Class that represents a PSP ALLEGREX function converted into a .NET IL function.
	/// </summary>
	public class DynarecFunction
	{
		/// <summary>
		/// Delegate to execute this function.
		/// </summary>
		public Action<CpuThreadState> Delegate;

		/// <summary>
		/// A list of functions that have embedded this function.
		/// </summary>
		public List<DynarecFunction> InlinedAtFunctions;
	}
}

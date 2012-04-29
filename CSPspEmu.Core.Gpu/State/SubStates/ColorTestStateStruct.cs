using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Utils;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public enum ColorTestFunctionEnum : byte
	{
		GU_NEVER,
		GU_ALWAYS,
		GU_EQUAL,
		GU_NOTEQUAL,
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	unsafe public struct ColorTestStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public OutputPixel Ref;

		/// <summary>
		/// 
		/// </summary>
		public OutputPixel Mask;

		/// <summary>
		/// 
		/// </summary>
		public ColorTestFunctionEnum Function;
	}
}

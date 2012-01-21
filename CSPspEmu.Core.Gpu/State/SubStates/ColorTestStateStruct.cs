using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Utils;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public enum ColorTestFunctionEnum : byte
	{
		GU_NEVER,
		GU_ALWAYS,
		GU_EQUAL,
		GU_NOTEQUAL,
	}

	unsafe public struct ColorTestStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// 
		/// </summary>
		public PixelFormatDecoder.OutputPixel Ref;

		/// <summary>
		/// 
		/// </summary>
		public PixelFormatDecoder.OutputPixel Mask;

		/// <summary>
		/// 
		/// </summary>
		public ColorTestFunctionEnum Function;
	}
}

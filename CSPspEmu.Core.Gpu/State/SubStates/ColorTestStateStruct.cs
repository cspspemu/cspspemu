using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	public enum ColorTestFunctionEnum : byte
	{
		GU_NEVER,
		GU_ALWAYS,
		GU_EQUAL,
		GU_NOTEQUAL,
	}

	public struct ColorTestStateStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// [0xFF, 0xFF, 0xFF, 0xFF]
		/// </summary>
		public byte RefRed;

		/// <summary>
		/// 
		/// </summary>
		public byte RefGreen;

		/// <summary>
		/// 
		/// </summary>
		public byte RefBlue;

		/// <summary>
		/// 
		/// </summary>
		public byte RefAlpha;

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

		/// <summary>
		/// 
		/// </summary>
		public ColorTestFunctionEnum TestFunction;
	}
}

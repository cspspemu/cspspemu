
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State
{
	public struct GpuRectStruct
	{
		/// <summary>
		/// 
		/// </summary>
		public uint Left;

		/// <summary>
		/// 
		/// </summary>
		public uint Top;

		/// <summary>
		/// 
		/// </summary>
		public uint Right;

		/// <summary>
		/// 
		/// </summary>
		public uint Bottom;

		/// <summary>
		/// 
		/// </summary>
		public bool IsFull
		{
			get
			{
				return (Left <= 0 && Top <= 0) && (Right >= 480 && Bottom >= 272);
			}
		}
	}
}

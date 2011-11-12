using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State
{
	unsafe public struct GpuMatrix4x4Struct
	{
		/// <summary>
		/// 
		/// </summary>
		public fixed float Values[4 * 4];

		/// <summary>
		/// 
		/// </summary>
		public uint Index;

		/// <summary>
		/// 
		/// </summary>
		internal void Reset()
		{
			Index = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		internal void Write(float Value)
		{
			fixed (float* ValuesPtr = Values)
			{
				ValuesPtr[Index++] = Value;
			}
		}
	}

	unsafe public struct GpuMatrix4x3Struct
	{
		/// <summary>
		/// 
		/// </summary>
		public fixed float Values[4 * 3];

		/// <summary>
		/// 
		/// </summary>
		public uint Index;

		/// <summary>
		/// 
		/// </summary>
		internal void Reset()
		{
			Index = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		internal void Write(float Value)
		{
			fixed (float* ValuesPtr = Values)
			{
			ValuesPtr[Index++] = Value;
			}
		}
	}
}

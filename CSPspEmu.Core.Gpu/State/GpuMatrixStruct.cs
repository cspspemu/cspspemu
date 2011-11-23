using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State
{
	unsafe public struct GpuMatrix4x4Struct
	{
		readonly static public int[] Indexes = new int[] {
			0, 1, 2, 3,
			4, 5, 6, 7,
			8, 9, 10, 11,
			12, 13, 14, 15
		};

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

		public void Dump()
		{
			fixed (float* ValuesPtr = Values)
			{
				Console.WriteLine("----------------------");
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						Console.Write("{0}, ", ValuesPtr[y * 4 + x]);
					}
					Console.WriteLine("");
				}
				Console.WriteLine("----------------------");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		internal void Write(float Value)
		{
			if (Index < 16)
			{
				fixed (float* ValuesPtr = Values)
				{
					ValuesPtr[Indexes[Index++]] = Value;
				}
			}
		}
	}

	unsafe public struct GpuMatrix4x3Struct
	{
		readonly static public int[] Indexes = new int[] {
			0, 1, 2,
			4, 5, 6,
			8, 9, 10,
			12, 13, 14
		};

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
			fixed (float* ValuesPtr = Values)
			{
				ValuesPtr[15] = 1;
			}
			Index = 0;
		}

		public void Dump()
		{
			fixed (float* ValuesPtr = Values)
			{
				Console.WriteLine("----------------------");
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						Console.Write("{0}, ", ValuesPtr[y * 4 + x]);
					}
					Console.WriteLine("");
				}
				Console.WriteLine("----------------------");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		internal void Write(float Value)
		{
			if (Index < 16)
			{
				fixed (float* ValuesPtr = Values)
				{
					ValuesPtr[Indexes[Index++]] = Value;
				}
			}
		}
	}
}

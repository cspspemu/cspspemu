using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
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
			//if (Index < Indexes.Length)
			{
				fixed (float* ValuesPtr = Values)
				{
					ValuesPtr[Indexes[Index++ % Indexes.Length]] = Value;
				}
			}
		}

		public void SetIdentity()
		{
			fixed (float* ValuesPtr = Values)
			{
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						ValuesPtr[x + y * 4] = (x == y) ? 1 : 0;
					}
				}
			}
		}

		public Matrix4 Matrix4
		{
			get
			{
				fixed (float* ValuesPtr = Values)
				{
					var Matrix4 = new Matrix4(
						ValuesPtr[0], ValuesPtr[1], ValuesPtr[2], ValuesPtr[3],
						ValuesPtr[4], ValuesPtr[5], ValuesPtr[6], ValuesPtr[7],
						ValuesPtr[8], ValuesPtr[9], ValuesPtr[10], ValuesPtr[11],
						ValuesPtr[12], ValuesPtr[13], ValuesPtr[14], ValuesPtr[15]
					);
					//Matrix4.Transpose();
					return Matrix4;
				}
			}
		}

	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
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
			//if (Index < Indexes.Length)
			{
				fixed (float* ValuesPtr = Values)
				{
					ValuesPtr[Indexes[Index++ % Indexes.Length]] = Value;
				}
			}
		}

		public Matrix4 Matrix4
		{
			get
			{
				fixed (float* ValuesPtr = Values)
				{
					var Matrix4 = new Matrix4(
						ValuesPtr[0], ValuesPtr[1], ValuesPtr[2], ValuesPtr[3],
						ValuesPtr[4], ValuesPtr[5], ValuesPtr[6], ValuesPtr[7],
						ValuesPtr[8], ValuesPtr[9], ValuesPtr[10], ValuesPtr[11],
						ValuesPtr[12], ValuesPtr[13], ValuesPtr[14], ValuesPtr[15]
					);
					//Matrix4.Transpose();
					return Matrix4;
				}
			}
		}
	}
}

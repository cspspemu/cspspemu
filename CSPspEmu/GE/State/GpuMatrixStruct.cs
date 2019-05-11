using System;
using System.Runtime.InteropServices;
using CSharpPlatform;

namespace CSPspEmu.Core.Gpu.State
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct GpuMatrix4X4Struct
    {
        public static readonly int[] Indexes =
        {
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
        internal void Reset(uint index = 0) => Index = index;

        public void Dump()
        {
            fixed (float* valuesPtr = Values)
            {
                Console.WriteLine("----------------------");
                for (var y = 0; y < 4; y++)
                {
                    for (var x = 0; x < 4; x++)
                    {
                        Console.Write("{0}, ", valuesPtr[y * 4 + x]);
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("----------------------");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        internal void Write(float value)
        {
            //if (Index < Indexes.Length)
            {
                fixed (float* valuesPtr = Values)
                {
                    valuesPtr[Indexes[Index++ % Indexes.Length]] = value;
                }
            }
        }

        public void SetIdentity()
        {
            fixed (float* valuesPtr = Values)
            {
                for (var y = 0; y < 4; y++)
                {
                    for (var x = 0; x < 4; x++)
                    {
                        valuesPtr[x + y * 4] = (x == y) ? 1 : 0;
                    }
                }
            }
        }

        public Matrix4F Matrix4
        {
            get
            {
                fixed (float* valuesPtr = Values)
                {
                    var Matrix4 = Matrix4F.Create(
                        valuesPtr[0], valuesPtr[1], valuesPtr[2], valuesPtr[3],
                        valuesPtr[4], valuesPtr[5], valuesPtr[6], valuesPtr[7],
                        valuesPtr[8], valuesPtr[9], valuesPtr[10], valuesPtr[11],
                        valuesPtr[12], valuesPtr[13], valuesPtr[14], valuesPtr[15]
                    );
                    //Matrix4.Transpose();
                    return Matrix4;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct GpuMatrix4X3Struct
    {
        public static readonly int[] Indexes = new int[]
        {
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
        internal void Reset(uint index = 0)
        {
            fixed (float* valuesPtr = Values)
            {
                valuesPtr[15] = 1.0f;
            }
            Index = index;
        }

        public void Dump()
        {
            fixed (float* valuesPtr = Values)
            {
                Console.WriteLine("----------------------");
                for (var y = 0; y < 4; y++)
                {
                    for (var x = 0; x < 4; x++)
                    {
                        Console.Write("{0}, ", valuesPtr[y * 4 + x]);
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("----------------------");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        internal void Write(float value)
        {
            //if (Index < Indexes.Length)
            {
                fixed (float* valuesPtr = Values)
                {
                    valuesPtr[Indexes[Index++ % Indexes.Length]] = value;
                }
            }
        }

        internal void WriteAt(int index, float value)
        {
            //if (Index < Indexes.Length)
            {
                fixed (float* valuesPtr = Values)
                {
                    valuesPtr[Indexes[index]] = value;
                }
            }
        }

        public Matrix4F Matrix4
        {
            get
            {
                fixed (float* valuesPtr = Values)
                {
                    var matrix4 = Matrix4F.Create(
#if false
						ValuesPtr[0], ValuesPtr[1], ValuesPtr[2], ValuesPtr[3],
						ValuesPtr[4], ValuesPtr[5], ValuesPtr[6], ValuesPtr[7],
						ValuesPtr[8], ValuesPtr[9], ValuesPtr[10], ValuesPtr[11],
						ValuesPtr[12], ValuesPtr[13], ValuesPtr[14], ValuesPtr[15]
#else
                        valuesPtr[0], valuesPtr[1], valuesPtr[2], 0,
                        valuesPtr[4], valuesPtr[5], valuesPtr[6], 0,
                        valuesPtr[8], valuesPtr[9], valuesPtr[10], 0,
                        valuesPtr[12], valuesPtr[13], valuesPtr[14], 1.0f
#endif
                    );
                    //Matrix4.Transpose();
                    return matrix4;
                }
            }
        }

        public void SetLastColumn()
        {
            fixed (float* valuesPtr = Values)
            {
                valuesPtr[3] = 0.0f;
                valuesPtr[7] = 0.0f;
                valuesPtr[11] = 0.0f;
                valuesPtr[15] = 1.0f;
            }
        }

        public void SetPosition(int column, int row, float value)
        {
            fixed (float* valuesPtr = Values)
            {
                valuesPtr[row * 4 + column] = value;
            }
        }

        public void LoadIdentity()
        {
            for (var row = 0; row < 4; row++)
            {
                for (var column = 0; column < 4; column++)
                {
                    SetPosition(column, row, (column == row) ? 1f : 0f);
                }
            }
        }
    }
}
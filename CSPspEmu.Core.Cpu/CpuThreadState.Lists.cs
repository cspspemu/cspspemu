using CSharpUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu
{
	public unsafe partial class CpuThreadState
	{
		public struct FCR31
		{
			public enum TypeEnum : uint
			{
				Rint = 0,
				Cast = 1,
				Ceil = 2,
				Floor = 3,
			}

			private uint _Value;

			public uint Value
			{
				get
				{
					_Value = BitUtils.Insert(_Value, 0, 2, (uint)RM);
					_Value = BitUtils.Insert(_Value, 23, 1, (uint)(CC ? 1 : 0));
					_Value = BitUtils.Insert(_Value, 24, 1, (uint)(FS ? 1 : 0));
					return _Value;
				}
				set
				{
					_Value = value;
					CC = (BitUtils.Extract(value, 23, 1) != 0);
					FS = (BitUtils.Extract(value, 24, 1) != 0);
					RM = (TypeEnum)BitUtils.Extract(value, 0, 2);
				}
			}

			public TypeEnum RM;
			public bool CC;
			public bool FS;
		}

		public class GprList
		{
			public CpuThreadState Processor;

			public int this[int Index]
			{
				get { fixed (uint* PTR = &Processor.GPR0) return (int)PTR[Index]; }
				set { if (Index == 0) return; fixed (uint* PTR = &Processor.GPR0) PTR[Index] = (uint)value; }
			}
		}

		public class C0rList
		{
			public CpuThreadState Processor;

			public uint this[int Index]
			{
				get { fixed (uint* PTR = &Processor.C0R0) return (uint)PTR[Index]; }
				set { fixed (uint* PTR = &Processor.C0R0) PTR[Index] = (uint)value; }
			}
		}

		public class FprList
		{
			public CpuThreadState Processor;

			public float this[int Index]
			{
				get { fixed (float* PTR = &Processor.FPR0) return PTR[Index]; }
				set { fixed (float* PTR = &Processor.FPR0) PTR[Index] = value; }
			}
		}

		public class FprListInteger
		{
			public CpuThreadState Processor;

			public int this[int Index]
			{
				get { fixed (float* PTR = &Processor.FPR0) return ((int*)PTR)[Index]; }
				set { fixed (float* PTR = &Processor.FPR0) ((int*)PTR)[Index] = value; }
			}
		}

		public class VfprList
		{
			public CpuThreadState Processor;

			public float this[int Index]
			{
				get { fixed (float* PTR = &Processor.VFR0) return PTR[Index]; }
				set { fixed (float* PTR = &Processor.VFR0) PTR[Index] = value; }
			}

			public float this[int Matrix, int Column, int Row]
			{
				get { return this[VfpuUtils.GetCellIndex(Matrix, Column, Row)]; }
				set { this[VfpuUtils.GetCellIndex(Matrix, Column, Row)] = value; }
			}

			public float[] this[uint Size, string Name]
			{
				get { return VfpuUtils.GetIndices(Size, Name).Select(Item => this[Item]).ToArray(); }
				set { var Indices = VfpuUtils.GetIndices(Size, Name); for (int n = 0; n < value.Length; n++) this[Indices[n]] = value[n]; }
			}

			public void ClearAll(float Value = 0f)
			{
				for (int n = 0; n < 128; n++) this[n] = Value;
			}
		}
	}
}

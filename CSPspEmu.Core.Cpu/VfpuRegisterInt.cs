using CSharpUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu
{
	public struct VfpuRegisterInt
	{
		public uint Value;

		static public implicit operator uint(VfpuRegisterInt Value)
		{
			return Value.Value;
		}

		static public implicit operator VfpuRegisterInt(uint Value)
		{
			return new VfpuRegisterInt() { Value = Value };
		}

		// LINES (Rows or Columns)
		public uint RC_LINE { get { return BitUtils.Extract(Value, 0, 2); } set { BitUtils.Insert(ref Value, 0, 2, value); } }
		public uint RC_MATRIX { get { return BitUtils.Extract(Value, 2, 3); } set { BitUtils.Insert(ref Value, 2, 3, value); } }
		public uint RC_ROW_COLUMN { get { return BitUtils.Extract(Value, 5, 1); } set { BitUtils.Insert(ref Value, 5, 1, value); } }
		public uint RC_OFFSET { get { return BitUtils.Extract(Value, 6, 1) * 2; } set { BitUtils.Insert(ref Value, 6, 1, value / 2); } }

		// SINGLE
		public uint S_ROW { get { return BitUtils.Extract(Value, 0, 2); } set { BitUtils.Insert(ref Value, 0, 2, value); } }
		public uint S_MATRIX { get { return BitUtils.Extract(Value, 2, 3); } set { BitUtils.Insert(ref Value, 2, 3, value); } }
		public uint S_COLUMN { get { return BitUtils.Extract(Value, 5, 2); } set { BitUtils.Insert(ref Value, 5, 2, value); } }
	}
}

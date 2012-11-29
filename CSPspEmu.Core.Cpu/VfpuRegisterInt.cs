using CSharpUtils;

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

		private void set(int Offset, int Count, uint SetValue)
		{
			this.Value = BitUtils.Insert(this.Value, Offset, Count, SetValue);
		}

		private uint get(int Offset, int Count)
		{
			return BitUtils.Extract(this.Value, Offset, Count);
		}

		// MATRIX (Normal or transposed)
		public uint M_ROW { get { return get(0, 2); } set { set(0, 2, value); } }
		public uint M_MATRIX { get { return get(2, 3); } set { set(2, 3, value); } }
		public uint M_COLUMN { get { return get(6, 1) * 2; } set { set(6, 1, value / 2); } }
		public uint M_TRANSPOSED { get { return get(5, 1); } set { set(5, 1, value); } }

		// LINES (Rows or Columns)
		public uint RC_LINE { get { return get(0, 2); } set { set(0, 2, value); } }
		public uint RC_MATRIX { get { return get(2, 3); } set { set(2, 3, value); } }
		public uint RC_ROW_COLUMN { get { return get(5, 1); } set { set(5, 1, value); } }
		public uint RC_OFFSET { get { return get(6, 1) * 2; } set { set(6, 1, value / 2); } }

		// SINGLE
		public uint S_ROW { get { return get(0, 2); } set { set(0, 2, value); } }
		public uint S_MATRIX { get { return get(2, 3); } set { set(2, 3, value); } }
		public uint S_COLUMN { get { return get(5, 2); } set { set(5, 2, value); } }
	}
}

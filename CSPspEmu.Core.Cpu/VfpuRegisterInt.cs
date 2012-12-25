using CSharpUtils;
namespace CSPspEmu.Core.Cpu
{
	public struct VfpuRegisterInt
	{
		public uint Value;

		static public implicit operator int(VfpuRegisterInt Value)
		{
			return (int)Value.Value;
		}

		static public implicit operator VfpuRegisterInt(int Value)
		{
			return new VfpuRegisterInt() { Value = (uint)Value };
		}

		static public implicit operator uint(VfpuRegisterInt Value)
		{
			return Value.Value;
		}

		static public implicit operator VfpuRegisterInt(uint Value)
		{
			return new VfpuRegisterInt() { Value = Value };
		}

		private void set(int Offset, int Count, int SetValue)
		{
			this.Value = BitUtils.Insert(this.Value, Offset, Count, (uint)SetValue);
		}

		private int get(int Offset, int Count)
		{
			return (int)BitUtils.Extract(this.Value, Offset, Count);
		}

		// MATRIX (Normal or transposed)
		public int M_ROW { get { return get(0, 2); } set { set(0, 2, value); } }
		public int M_MATRIX { get { return get(2, 3); } set { set(2, 3, value); } }
		public int M_COLUMN { get { return get(6, 1) * 2; } set { set(6, 1, value / 2); } }
		public int M_TRANSPOSED { get { return get(5, 1); } set { set(5, 1, value); } }

		// LINES (Rows or Columns)
		public int RC_LINE { get { return get(0, 2); } set { set(0, 2, value); } }
		public int RC_MATRIX { get { return get(2, 3); } set { set(2, 3, value); } }
		public int RC_ROW_COLUMN { get { return get(5, 1); } set { set(5, 1, value); } }
		public int RC_OFFSET { get { return get(6, 1); } set { set(6, 1, value); } }

		// SINGLE
		public int S_COLUMN { get { return get(0, 2); } set { set(0, 2, value); } }
		public int S_MATRIX { get { return get(2, 3); } set { set(2, 3, value); } }
		public int S_ROW { get { return get(5, 2); } set { set(5, 2, value); } }
	}
}

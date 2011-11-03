using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu
{
	public struct Instruction
	{
		public uint Value;

		private void set(int Offset, int Count, uint SetValue)
		{
			uint Mask = (uint)((1 << Count) - 1);
			this.Value &= ~(Mask << Offset);
			this.Value |= (SetValue & Mask) << Offset;
		}


		private uint get(int Offset, int Count)
		{
			return (uint)((Value >> Offset) & ((1 << Count) - 1));
		}

		private int get_s(int Offset, int Count)
		{
			uint SignBit = (uint)(1 << (Offset + (Count - 1)));
			int _Value = (int)((Value >> Offset) & ((1 << Count) - 1));
			if ((_Value & SignBit) != 0)
			{
				throw (new NotImplementedException());
			}
			return _Value;
		}

		public uint OP1 { get { return get(26, 6); } set { set(26, 6, value); } }
		public uint OP2 { get { return get(0, 6); } set { set(0, 6, value); } }

		// Type Register.
		public int RD { get { return (int)get(11 + 5 * 0, 5); } set { set(11 + 5 * 0, 5, (uint)value); } }
		public int RT { get { return (int)get(11 + 5 * 1, 5); } set { set(11 + 5 * 1, 5, (uint)value); } }
		public int RS { get { return (int)get(11 + 5 * 2, 5); } set { set(11 + 5 * 2, 5, (uint)value); } }

		// Type Float Register.
		public uint FD { get { return get(6 + 5 * 0, 5); } set { set(6 + 5 * 0, 5, (uint)value); } }
		public uint FS { get { return get(6 + 5 * 1, 5); } set { set(6 + 5 * 1, 5, (uint)value); } }
		public uint FT { get { return get(6 + 5 * 2, 5); } set { set(6 + 5 * 2, 5, (uint)value); } }

		// Type Immediate (Unsigned).
		public int IMM { get { return (int)(short)(ushort)get(0, 16); } set { set(0, 16, (uint)value); } }
		public uint IMMU { get { return get(0, 16); } set { set(0, 16, (uint)value); } }

		// JUMP 26 bits.
		public uint JUMP { get { return get(0, 26); } }

		// CODE
		public uint CODE { get { return get(6, 20); } set { set(6, 20, value); } }

		public uint GetBranchAddress(uint PC)
		{
			return (uint)(PC + IMM * 4);
		}

		// Immediate 7 bits.
		// VFPU
		/*
		~ bitslice!("v", int , "IMM7", 0, 7)
		// // SVQ(111110:rs:vt5:imm14:0:vt1)
		~ bitslice!("v", uint, "VT1", 0, 1)
		~ bitslice!("v", uint, "VT2", 0, 2)
		~ bitslice!("v", int , "IMM14", 2, 14)
		~ bitslice!("v", uint, "VT5", 16, 5)
		~ bitslice!("v", uint, "IMM5", 16, 5)

		~ bitslice!("v", uint, "IMM4",  0, 4)

		~ bitslice!("v", uint, "IMM3",  16, 3)

		~ bitslice!("v", uint, "VD",  0, 7)
		~ bitslice!("v", uint, "ONE", 7, 1)
		~ bitslice!("v", uint, "VS",  8, 7)
		~ bitslice!("v", uint, "TWO", 15, 1)
		~ bitslice!("v", uint, "VT",  16, 7)

		// C1CR
		~ bitslice!("v", uint, "C1CR", 6 + 5 * 1, 5)

		// LSB/MSB
		~ bitslice!("v", uint, "LSB", 6 + 5 * 0, 5)
		~ bitslice!("v", uint, "MSB", 6 + 5 * 1, 5)
		*/
	}
}

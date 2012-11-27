using CSharpUtils;
using CSPspEmu.Core.Memory;
using System;

namespace CSPspEmu.Core.Cpu
{
	public struct Instruction
	{
		public uint Value;

		public static float HalfFloatToFloat(int imm16)
		{
			int s = (imm16 >> 15) & 0x00000001; // Sign
			int e = (imm16 >> 10) & 0x0000001f; // Exponent
			int f = (imm16 >> 0) & 0x000003ff;  // Fraction

			// Need to handle 0x7C00 INF and 0xFC00 -INF?
			if (e == 0)
			{
				// Need to handle +-0 case f==0 or f=0x8000?
				if (f == 0)
				{
					// Plus or minus zero
					return MathFloat.ReinterpretIntAsFloat(s << 31);
				}
				// Denormalized number -- renormalize it
				while ((f & 0x00000400) == 0)
				{
					f <<= 1;
					e -= 1;
				}
				e += 1;
				f &= ~0x00000400;
			}
			else if (e == 31)
			{
				if (f == 0)
				{
					// Inf
					return MathFloat.ReinterpretIntAsFloat((s << 31) | 0x7f800000);
				}
				// NaN
				return MathFloat.ReinterpretIntAsFloat((s << 31) | 0x7f800000 | (f << 13));
			}

			e = e + (127 - 15);
			f = f << 13;

			return MathFloat.ReinterpretIntAsFloat((s << 31) | (e << 23) | f);
		}

		public uint GetJumpAddress(uint CurrentPC)
		{
			//Console.WriteLine("{0:X8} - ", CurrentPC);
			//var Result = (uint)(CurrentPC & ~PspMemory.MemoryMask) + (JUMP << 2);
			var Result = (uint)(CurrentPC & ~0x0FFFFFFF) + (JUMP << 2);
			if (!PspMemory.IsAddressValid(Result))
			{
				Console.Error.WriteLine("ERROR: Generating an invalid JumpAddress 0x{0:X8} from CurrentPC=0x{1:X8} with JUMP=0x{2:X8}", Result, CurrentPC, JUMP << 2);
				//throw (new Exception(String.Format("Generating an invalid JumpAddress 0x{0:X8} from CurrentPC=0x{1:X8} with JUMP=0x{2:X8}", Result, CurrentPC, JUMP << 2)));
			}
			//Console.WriteLine("CurrentPC: 0x{0:X8} - 0x{1:X8} - 0x{2:X8} - 0x{3:X8}", CurrentPC, (CurrentPC & ~PspMemory.MemoryMask), JUMP << 2, Result);
			return Result;
		}

		/*
		private static float HalfFloatToFloat(ushort ShortValue)
		{
			uint Value = 0;
			var Significand = BitUtils.Extract(ShortValue, 0, 10);
			var Exponent    = BitUtils.Extract(ShortValue, 10, 5);
			var Sign        = BitUtils.Extract(ShortValue, 15, 1);
			BitUtils.Insert(ref Value, 0, 23, Significand);
			BitUtils.Insert(ref Value, 23, 8, Exponent);
			//BitUtils.Insert(ref Value, 13, 10, Significand);
			//BitUtils.Insert(ref Value, 26, 5, Exponent);
			BitUtils.Insert(ref Value, 31, 1, Exponent);
			//Console.Error.WriteLine("%032b".Sprintf(Value));
			float ValueFloat = MathFloat.ReinterpretUIntAsFloat(Value);
			Console.Error.WriteLine(ValueFloat);
			return ValueFloat;
		}
		*/

		private void set(int Offset, int Count, uint SetValue)
		{
			this.Value = BitUtils.Insert(this.Value, Offset, Count, SetValue);
		}

		private uint get(int Offset, int Count)
		{
			return BitUtils.Extract(this.Value, Offset, Count);
		}

		private int get_s(int Offset, int Count)
		{
			return BitUtils.ExtractSigned(this.Value, Offset, Count);
		}

		public uint OP1 { get { return get(26, 6); } set { set(26, 6, value); } }
		public uint OP2 { get { return get(0, 6); } set { set(0, 6, value); } }

		// Type Register.
		public int RD { get { return (int)get(11 + 5 * 0, 5); } set { set(11 + 5 * 0, 5, (uint)value); } }
		public int RT { get { return (int)get(11 + 5 * 1, 5); } set { set(11 + 5 * 1, 5, (uint)value); } }
		public int RS { get { return (int)get(11 + 5 * 2, 5); } set { set(11 + 5 * 2, 5, (uint)value); } }

		// Type Float Register.
		public int FD { get { return (int)get(6 + 5 * 0, 5); } set { set(6 + 5 * 0, 5, (uint)value); } }
		public int FS { get { return (int)get(6 + 5 * 1, 5); } set { set(6 + 5 * 1, 5, (uint)value); } }
		public int FT { get { return (int)get(6 + 5 * 2, 5); } set { set(6 + 5 * 2, 5, (uint)value); } }

		// Type Immediate (Unsigned).
		public int IMM { get { return (int)(short)(ushort)get(0, 16); } set { set(0, 16, (uint)value); } }
		public uint IMMU { get { return get(0, 16); } set { set(0, 16, (uint)value); } }

		public uint HIGH16 { get { return get(16, 16); } set { set(16, 16, (uint)value); } }

		// JUMP 26 bits.
		public uint JUMP { get { return get(0, 26); } set { set(0, 26, value); } }

		/// <summary>
		/// Instruction's JUMP raw value multiplied * 4. Creating the real address.
		/// </summary>
		public uint JUMP_Real { get { return JUMP * 4; } set { JUMP = value / 4; } }

		// CODE
		public uint CODE { get { return get(6, 20); } set { set(6, 20, value); } }

		public uint POS { get { return LSB; } set { LSB = value; } }
		public uint SIZE_E { get { return MSB + 1; } set { MSB = value - 1; } }
		public uint SIZE_I { get { return MSB - LSB + 1; } set { MSB = LSB + value - 1; } }

		public uint LSB { get { return get(6 + 5 * 0, 5); } set { set(6 + 5 * 0, 5, value); } }
		public uint MSB { get { return get(6 + 5 * 1, 5); } set { set(6 + 5 * 1, 5, value); } }

		public uint GetBranchAddress(uint PC)
		{
			return (uint)(PC + 4 + IMM * 4);
		}

		public uint VD { get { return get(0, 7); } set { set(0, 7, value); } }
		public uint ONE { get { return get(7, 1); } set { set(7, 1, value); } }
		public uint VS { get { return get(8, 7); } set { set(8, 7, value); } }
		public uint TWO { get { return get(15, 1); } set { set(15, 1, value); } }
		public uint VT { get { return get(16, 7); } set { set(16, 7, value); } }

		public uint ONE_TWO {
			get { return 1 + 1 * ONE + 2 * TWO; }
			set { ONE = (((value - 1) >> 0) & 1); TWO = (((value - 1) >> 1) & 1); }
		}

		public static implicit operator Instruction(uint Value)
		{
			return new Instruction()
			{
				Value = Value,
			};
		}

		public static implicit operator uint(Instruction Instruction)
		{
			return Instruction.Value;
		}

		// Immediate 7 bits.
		// VFPU
		/*
		~ bitslice!("v", int , "IMM7", 0, 7)
		// // SVQ(111110:rs:vt5:imm14:0:vt1)
		~ bitslice!("v", uint, "VT1", 0, 1)
		~ bitslice!("v", uint, "VT2", 0, 2)
		~ bitslice!("v", uint, "IMM5", 16, 5)

		~ bitslice!("v", uint, "IMM4",  0, 4)

		~ bitslice!("v", uint, "IMM3",  16, 3)

		// C1CR
		~ bitslice!("v", uint, "C1CR", 6 + 5 * 1, 5)

		// LSB/MSB
		~ bitslice!("v", uint, "LSB", 6 + 5 * 0, 5)
		~ bitslice!("v", uint, "MSB", 6 + 5 * 1, 5)
		*/

		/// <summary>
		/// @TODO: Signed or unsigned?
		/// </summary>
		public int IMM14 { get { return get_s(2, 14); } set { set(2, 14, (uint)value); } }
		public uint IMM5 { get { return get(16, 5); } set { set(16, 5, value); } }
		public uint IMM3 { get { return get(16, 3); } set { set(16, 3, value); } }
		public uint IMM7 { get { return get(0, 7); } set { set(0, 7, value); } }
		public uint IMM4 { get { return get(0, 4); } set { set(0, 4, value); } }

		public uint VT1 { get { return get(0, 1); } set { set(0, 1, value); } }
		public uint VT2 { get { return get(0, 2); } set { set(0, 2, value); } }

		public uint VT5 { get { return get(16, 5); } set { set(16, 5, value); } }

		public uint VT5_1 { get { return VT5 | (VT1 << 5); } }
		public uint VT5_2 { get { return VT5 | (VT2 << 5); } }

		public float IMM_HF
		{
			get
			{
				return HalfFloatToFloat(IMM);
			}
		}
	}
}

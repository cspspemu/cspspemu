using CSharpUtils;
using CSPspEmu.Core.Cpu.VFpu;
using CSPspEmu.Core.Memory;
using System;

namespace CSPspEmu.Core.Cpu
{
	public struct Instruction
	{
		public uint Value;

		public uint GetJumpAddress(IPspMemoryInfo MemoryInfo, uint CurrentPC) { return (uint)(CurrentPC & ~0x0FFFFFFF) + (JUMP_Bits << 2); }
		private void set(int Offset, int Count, uint SetValue) { this.Value = BitUtils.Insert(this.Value, Offset, Count, SetValue); }
		private uint get(int Offset, int Count) { return BitUtils.Extract(this.Value, Offset, Count); }
		private int get_s(int Offset, int Count) { return BitUtils.ExtractSigned(this.Value, Offset, Count); }

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
		public uint JUMP_Bits { get { return get(0, 26); } set { set(0, 26, value); } }

		/// <summary>
		/// Instruction's JUMP raw value multiplied * 4. Creating the real address.
		/// </summary>
		public uint JUMP_Real { get { return JUMP_Bits * 4; } set { JUMP_Bits = value / 4; } }

		public uint CODE { get { return get(6, 20); } set { set(6, 20, value); } }

		public uint POS { get { return LSB; } set { LSB = value; } }
		public uint SIZE_E { get { return MSB + 1; } set { MSB = value - 1; } }
		public uint SIZE_I { get { return MSB - LSB + 1; } set { MSB = LSB + value - 1; } }

		public uint LSB { get { return get(6 + 5 * 0, 5); } set { set(6 + 5 * 0, 5, value); } }
		public uint MSB { get { return get(6 + 5 * 1, 5); } set { set(6 + 5 * 1, 5, value); } }
		public uint C1CR { get { return get(6 + 5 * 1, 5); } set { set(6 + 5 * 1, 5, value); } }

		public uint GetBranchAddress(uint PC) { return (uint)(PC + 4 + IMM * 4); }

		public uint ONE { get { return get(7, 1); } set { set(7, 1, value); } }
		public uint TWO { get { return get(15, 1); } set { set(15, 1, value); } }
		public int ONE_TWO { get { return (int)(1 + 1 * ONE + 2 * TWO); } set { ONE = ((((uint)value - 1) >> 0) & 1); TWO = ((((uint)value - 1) >> 1) & 1); } }

		public VfpuRegisterInt VD { get { return get(0, 7); } set { set(0, 7, value); } }
		public VfpuRegisterInt VS { get { return get(8, 7); } set { set(8, 7, value); } }
		public VfpuRegisterInt VT { get { return get(16, 7); } set { set(16, 7, value); } }
		public VfpuRegisterInt VT5_1 { get { return VT5 | (VT1 << 5); } set { VT5 = value; VT1 = ((uint)value >> 5); } }

		// @TODO: Signed or unsigned?
		public int IMM14 { get { return get_s(2, 14); } set { set(2, 14, (uint)value); } }

		public uint IMM5 { get { return get(16, 5); } set { set(16, 5, value); } }
		public uint IMM3 { get { return get(16, 3); } set { set(16, 3, value); } }
		public uint IMM7 { get { return get(0, 7); } set { set(0, 7, value); } }
		public uint IMM4 { get { return get(0, 4); } set { set(0, 4, value); } }
		public uint VT1 { get { return get(0, 1); } set { set(0, 1, value); } }
		public uint VT2 { get { return get(0, 2); } set { set(0, 2, value); } }
		public uint VT5 { get { return get(16, 5); } set { set(16, 5, value); } }
		public uint VT5_2 { get { return VT5 | (VT2 << 5); } }
		public float IMM_HF { get { return HalfFloat.ToFloat(IMM); } }

		public static implicit operator Instruction(uint Value) { return new Instruction() { Value = Value, }; }
		public static implicit operator uint(Instruction Instruction) { return Instruction.Value; }
	}
}

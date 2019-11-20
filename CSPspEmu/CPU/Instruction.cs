using CSharpUtils;
using CSPspEmu.Core.Cpu.VFpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu
{
    public struct Instruction
    {
        public uint Value;

        public uint GetJumpAddress(IPspMemoryInfo memoryInfo, uint currentPc) =>
            (uint) (currentPc & ~0x0FFFFFFF) + (JumpBits << 2);

        private void Set(int offset, int count, uint setValue) =>
            Value = BitUtils.Insert(Value, offset, count, setValue);

        private uint Get(int offset, int count) => BitUtils.Extract(Value, offset, count);

        private int get_s(int offset, int count) => BitUtils.ExtractSigned(Value, offset, count);

        public uint Op1
        {
            get => Get(26, 6);
            set => Set(26, 6, value);
        }

        public uint Op2
        {
            get => Get(0, 6);
            set => Set(0, 6, value);
        }

        // Type Register.
        public int Rd
        {
            get => (int) Get(11 + 5 * 0, 5);
            set => Set(11 + 5 * 0, 5, (uint) value);
        }

        public int Rt
        {
            get => (int) Get(11 + 5 * 1, 5);
            set => Set(11 + 5 * 1, 5, (uint) value);
        }

        public int Rs
        {
            get => (int) Get(11 + 5 * 2, 5);
            set => Set(11 + 5 * 2, 5, (uint) value);
        }

        // Type Float Register.
        public int Fd
        {
            get => (int) Get(6 + 5 * 0, 5);
            set => Set(6 + 5 * 0, 5, (uint) value);
        }

        public int Fs
        {
            get => (int) Get(6 + 5 * 1, 5);
            set => Set(6 + 5 * 1, 5, (uint) value);
        }

        public int Ft
        {
            get => (int) Get(6 + 5 * 2, 5);
            set => Set(6 + 5 * 2, 5, (uint) value);
        }

        // Type Immediate (Unsigned).
        public int Imm
        {
            get => (short) (ushort) Get(0, 16);
            set => Set(0, 16, (uint) value);
        }

        public uint Immu
        {
            get => Get(0, 16);
            set => Set(0, 16, value);
        }

        public uint High16
        {
            get => Get(16, 16);
            set => Set(16, 16, value);
        }

        // JUMP 26 bits.
        public uint JumpBits
        {
            get => Get(0, 26);
            set => Set(0, 26, value);
        }

        /// <summary>
        /// Instruction's JUMP raw value multiplied * 4. Creating the real address.
        /// </summary>
        public uint JumpReal
        {
            get => JumpBits * 4;
            set => JumpBits = value / 4;
        }

        public uint Code
        {
            get => Get(6, 20);
            set => Set(6, 20, value);
        }

        public uint Pos
        {
            get => Lsb;
            set => Lsb = value;
        }

        public uint SizeE
        {
            get => Msb + 1;
            set => Msb = value - 1;
        }

        public uint SizeI
        {
            get => Msb - Lsb + 1;
            set => Msb = Lsb + value - 1;
        }

        public uint Lsb
        {
            get => Get(6 + 5 * 0, 5);
            set => Set(6 + 5 * 0, 5, value);
        }

        public uint Msb
        {
            get => Get(6 + 5 * 1, 5);
            set => Set(6 + 5 * 1, 5, value);
        }

        public uint C1Cr
        {
            get => Get(6 + 5 * 1, 5);
            set => Set(6 + 5 * 1, 5, value);
        }

        public uint GetBranchAddress(uint pc) => (uint) (pc + 4 + Imm * 4);

        public uint One
        {
            get => Get(7, 1);
            set => Set(7, 1, value);
        }

        public uint Two
        {
            get => Get(15, 1);
            set => Set(15, 1, value);
        }

        public int OneTwo
        {
            get => (int) (1 + 1 * One + 2 * Two);
            set
            {
                One = (((uint) value - 1) >> 0) & 1;
                Two = (((uint) value - 1) >> 1) & 1;
            }
        }

        public VfpuRegisterInt Vd
        {
            get => Get(0, 7);
            set => Set(0, 7, value);
        }

        public VfpuRegisterInt Vs
        {
            get => Get(8, 7);
            set => Set(8, 7, value);
        }

        public VfpuRegisterInt Vt
        {
            get => Get(16, 7);
            set => Set(16, 7, value);
        }

        public VfpuRegisterInt Vt51
        {
            get => Vt5 | (Vt1 << 5);
            set
            {
                Vt5 = value;
                Vt1 = (uint) value >> 5;
            }
        }

        // @TODO: Signed or unsigned?
        public int Imm14
        {
            get => get_s(2, 14);
            set => Set(2, 14, (uint) value);
        }
        //public int IMM14 { get { return (int)get(2, 14); } set { set(2, 14, (uint)value); } }

        public uint Imm8
        {
            get => Get(16, 8);
            set => Set(16, 8, value);
        }

        public uint Imm5
        {
            get => Get(16, 5);
            set => Set(16, 5, value);
        }

        public uint Imm3
        {
            get => Get(18, 3);
            set => Set(18, 3, value);
        }

        public uint Imm7
        {
            get => Get(0, 7);
            set => Set(0, 7, value);
        }

        public uint Imm4
        {
            get => Get(0, 4);
            set => Set(0, 4, value);
        }

        public uint Vt1
        {
            get => Get(0, 1);
            set => Set(0, 1, value);
        }

        public uint Vt2
        {
            get => Get(0, 2);
            set => Set(0, 2, value);
        }

        public uint Vt5
        {
            get => Get(16, 5);
            set => Set(16, 5, value);
        }

        public uint Vt52 => Vt5 | (Vt2 << 5);

        public float ImmHf => HalfFloat.ToFloat(Imm);

        public static implicit operator Instruction(uint value) => new Instruction {Value = value,};

        public static implicit operator uint(Instruction instruction) => instruction.Value;
    }
}
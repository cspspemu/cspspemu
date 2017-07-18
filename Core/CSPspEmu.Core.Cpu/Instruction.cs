using CSharpUtils;
using CSPspEmu.Core.Cpu.VFpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu
{
    public struct Instruction
    {
        public uint Value;

        public uint GetJumpAddress(IPspMemoryInfo memoryInfo, uint currentPc) => (uint) (currentPc & ~0x0FFFFFFF) + (JUMP_Bits << 2);

        private void Set(int offset, int count, uint setValue) => Value = BitUtils.Insert(Value, offset, count, setValue);

        private uint Get(int offset, int count) => BitUtils.Extract(Value, offset, count);

        private int get_s(int offset, int count) => BitUtils.ExtractSigned(Value, offset, count);

        public uint OP1
        {
            get => Get(26, 6);
            set => Set(26, 6, value);
        }

        public uint OP2
        {
            get => Get(0, 6);
            set => Set(0, 6, value);
        }

        // Type Register.
        public int RD
        {
            get => (int) Get(11 + 5 * 0, 5);
            set => Set(11 + 5 * 0, 5, (uint) value);
        }

        public int RT
        {
            get => (int) Get(11 + 5 * 1, 5);
            set => Set(11 + 5 * 1, 5, (uint) value);
        }

        public int RS
        {
            get => (int) Get(11 + 5 * 2, 5);
            set => Set(11 + 5 * 2, 5, (uint) value);
        }

        // Type Float Register.
        public int FD
        {
            get => (int) Get(6 + 5 * 0, 5);
            set => Set(6 + 5 * 0, 5, (uint) value);
        }

        public int FS
        {
            get => (int) Get(6 + 5 * 1, 5);
            set => Set(6 + 5 * 1, 5, (uint) value);
        }

        public int FT
        {
            get => (int) Get(6 + 5 * 2, 5);
            set => Set(6 + 5 * 2, 5, (uint) value);
        }

        // Type Immediate (Unsigned).
        public int IMM
        {
            get => (short) (ushort) Get(0, 16);
            set => Set(0, 16, (uint) value);
        }

        public uint IMMU
        {
            get => Get(0, 16);
            set => Set(0, 16, value);
        }

        public uint HIGH16
        {
            get => Get(16, 16);
            set => Set(16, 16, value);
        }

        // JUMP 26 bits.
        public uint JUMP_Bits
        {
            get => Get(0, 26);
            set => Set(0, 26, value);
        }

        /// <summary>
        /// Instruction's JUMP raw value multiplied * 4. Creating the real address.
        /// </summary>
        public uint JUMP_Real
        {
            get => JUMP_Bits * 4;
            set => JUMP_Bits = value / 4;
        }

        public uint CODE
        {
            get => Get(6, 20);
            set => Set(6, 20, value);
        }

        public uint POS
        {
            get => LSB;
            set => LSB = value;
        }

        public uint SIZE_E
        {
            get => MSB + 1;
            set => MSB = value - 1;
        }

        public uint SIZE_I
        {
            get => MSB - LSB + 1;
            set => MSB = LSB + value - 1;
        }

        public uint LSB
        {
            get => Get(6 + 5 * 0, 5);
            set => Set(6 + 5 * 0, 5, value);
        }

        public uint MSB
        {
            get => Get(6 + 5 * 1, 5);
            set => Set(6 + 5 * 1, 5, value);
        }

        public uint C1CR
        {
            get => Get(6 + 5 * 1, 5);
            set => Set(6 + 5 * 1, 5, value);
        }

        public uint GetBranchAddress(uint pc) => (uint) (pc + 4 + IMM * 4);

        public uint ONE
        {
            get => Get(7, 1);
            set => Set(7, 1, value);
        }

        public uint TWO
        {
            get => Get(15, 1);
            set => Set(15, 1, value);
        }

        public int ONE_TWO
        {
            get => (int) (1 + 1 * ONE + 2 * TWO);
            set
            {
                ONE = ((((uint) value - 1) >> 0) & 1);
                TWO = ((((uint) value - 1) >> 1) & 1);
            }
        }

        public VfpuRegisterInt VD
        {
            get => Get(0, 7);
            set => Set(0, 7, value);
        }

        public VfpuRegisterInt VS
        {
            get => Get(8, 7);
            set => Set(8, 7, value);
        }

        public VfpuRegisterInt VT
        {
            get => Get(16, 7);
            set => Set(16, 7, value);
        }

        public VfpuRegisterInt VT5_1
        {
            get => VT5 | (VT1 << 5);
            set
            {
                VT5 = value;
                VT1 = ((uint) value >> 5);
            }
        }

        // @TODO: Signed or unsigned?
        public int IMM14
        {
            get => get_s(2, 14);
            set => Set(2, 14, (uint) value);
        }
        //public int IMM14 { get { return (int)get(2, 14); } set { set(2, 14, (uint)value); } }

        public uint IMM8
        {
            get => Get(16, 8);
            set => Set(16, 8, value);
        }

        public uint IMM5
        {
            get => Get(16, 5);
            set => Set(16, 5, value);
        }

        public uint IMM3
        {
            get => Get(18, 3);
            set => Set(18, 3, value);
        }

        public uint IMM7
        {
            get => Get(0, 7);
            set => Set(0, 7, value);
        }

        public uint IMM4
        {
            get => Get(0, 4);
            set => Set(0, 4, value);
        }

        public uint VT1
        {
            get => Get(0, 1);
            set => Set(0, 1, value);
        }

        public uint VT2
        {
            get => Get(0, 2);
            set => Set(0, 2, value);
        }

        public uint VT5
        {
            get => Get(16, 5);
            set => Set(16, 5, value);
        }

        public uint VT5_2 => VT5 | (VT2 << 5);

        public float IMM_HF => HalfFloat.ToFloat(IMM);

        public static implicit operator Instruction(uint Value) => new Instruction {Value = Value,};

        public static implicit operator uint(Instruction Instruction) => Instruction.Value;
    }
}
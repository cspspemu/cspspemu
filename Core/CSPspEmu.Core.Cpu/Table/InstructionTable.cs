using System.Collections.Generic;
using System.Linq;

namespace CSPspEmu.Core.Cpu.Table
{
    public sealed class InstructionTable
    {
        private const AddressType AddrTypeNone = AddressType.None;
        private const AddressType AddrType16 = AddressType.T16;
        private const AddressType AddrType26 = AddressType.T26;
        private const AddressType AddrTypeReg = AddressType.Reg;

        private const InstructionType InstrTypePsp = InstructionType.Psp;
        private const InstructionType InstrTypeB = InstructionType.B;
        private const InstructionType InstrTypeLikely = InstructionType.Likely;
        private const InstructionType InstrTypeJal = InstructionType.Jal;
        private const InstructionType InstrTypeJump = InstructionType.Jump;
        private const InstructionType InstrTypeSyscall = InstructionType.Syscall;

        private static InstructionInfo Id(string name, string binaryEncoding, string asmEncoding,
            AddressType addressType, InstructionType instructionType)
        {
            return new InstructionInfo()
            {
                Name = name,
                BinaryEncoding = binaryEncoding,
                AsmEncoding = asmEncoding,
                AddressType = addressType,
                InstructionType = instructionType
            };
        }

        private static string Vm(string binaryEncoding) => binaryEncoding;

        public static IEnumerable<InstructionInfo> All => new InstructionInfo[] { }
            .Union(Alu)
            .Union(Bcu)
            .Union(Bcu)
            .Union(Cop0)
            .Union(Fpu)
            .Union(Lsu)
            .Union(Special)
            .Union(Vfpu)
            .Union(VfpuBranch);

        public static IEnumerable<InstructionInfo> AllBranches => new InstructionInfo[] { }
            .Union(Bcu)
            .Union(VfpuBranch);

        public static InstructionInfo Unknown = Id(InstructionNames.Unknown, Vm("111111:11111:11111:11111:11111:111111"), "",
            AddrTypeNone, 0);

        private static InstructionInfo[] _alu;

        public static InstructionInfo[] Alu => _alu ?? (_alu = new[]
        {
            // Arithmetic operations.
            Id(InstructionNames.Add, Vm("000000:rs:rt:rd:00000:100000"), "%d, %s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Addu, Vm("000000:rs:rt:rd:00000:100001"), "%d, %s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Addi, Vm("001000:rs:rt:imm16"), "%t, %s, %i", AddrTypeNone, 0),
            Id(InstructionNames.Addiu, Vm("001001:rs:rt:imm16"), "%t, %s, %i", AddrTypeNone, 0),
            Id(InstructionNames.Sub, Vm("000000:rs:rt:rd:00000:100010"), "%d, %s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Subu, Vm("000000:rs:rt:rd:00000:100011"), "%d, %s, %t", AddrTypeNone, 0),

            // Logical Operations.
            Id(InstructionNames.And, Vm("000000:rs:rt:rd:00000:100100"), "%d, %s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Andi, Vm("001100:rs:rt:imm16"), "%t, %s, %I", AddrTypeNone, 0),
            Id(InstructionNames.Nor, Vm("000000:rs:rt:rd:00000:100111"), "%d, %s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Or, Vm("000000:rs:rt:rd:00000:100101"), "%d, %s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Ori, Vm("001101:rs:rt:imm16"), "%t, %s, %I", AddrTypeNone, 0),
            Id(InstructionNames.Xor, Vm("000000:rs:rt:rd:00000:100110"), "%d, %s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Xori, Vm("001110:rs:rt:imm16"), "%t, %s, %I", AddrTypeNone, 0),

            // Shift Left/Right Logical/Arithmethic (Variable).
            Id(InstructionNames.Sll, Vm("000000:00000:rt:rd:sa:000000"), "%d, %t, %a", AddrTypeNone, 0),
            Id(InstructionNames.Sllv, Vm("000000:rs:rt:rd:00000:000100"), "%d, %t, %s", AddrTypeNone, 0),
            Id(InstructionNames.Sra, Vm("000000:00000:rt:rd:sa:000011"), "%d, %t, %a", AddrTypeNone, 0),
            Id(InstructionNames.Srav, Vm("000000:rs:rt:rd:00000:000111"), "%d, %t, %s", AddrTypeNone, 0),
            Id(InstructionNames.Srl, Vm("000000:00000:rt:rd:sa:000010"), "%d, %t, %a", AddrTypeNone, 0),
            Id(InstructionNames.Srlv, Vm("000000:rs:rt:rd:00000:000110"), "%d, %t, %s", AddrTypeNone, 0),
            Id(InstructionNames.Rotr, Vm("000000:00001:rt:rd:sa:000010"), "%d, %t, %a", AddrTypeNone, 0),
            Id(InstructionNames.Rotrv, Vm("000000:rs:rt:rd:00001:000110"), "%d, %t, %s", AddrTypeNone, 0),

            // Set Less Than (Immediate) (Unsigned).
            Id(InstructionNames.Slt, Vm("000000:rs:rt:rd:00000:101010"), "%d, %s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Slti, Vm("001010:rs:rt:imm16"), "%t, %s, %i", AddrTypeNone, 0),
            Id(InstructionNames.Sltu, Vm("000000:rs:rt:rd:00000:101011"), "%d, %s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Sltiu, Vm("001011:rs:rt:imm16"), "%t, %s, %i", AddrTypeNone, 0),

            // Load Upper Immediate.
            Id(InstructionNames.Lui, Vm("001111:00000:rt:imm16"), "%t, %I", AddrTypeNone, 0),

            // Sign Extend Byte/Half word.
            Id(InstructionNames.Seb, Vm("011111:00000:rt:rd:10000:100000"), "%d, %t", AddrTypeNone, 0),
            Id(InstructionNames.Seh, Vm("011111:00000:rt:rd:11000:100000"), "%d, %t", AddrTypeNone, 0),

            // BIT REVerse.
            Id(InstructionNames.Bitrev, Vm("011111:00000:rt:rd:10100:100000"), "%d, %t", AddrTypeNone, InstrTypePsp),

            // MAXimum/MINimum.
            Id(InstructionNames.Max, Vm("000000:rs:rt:rd:00000:101100"), "%d, %s, %t", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Min, Vm("000000:rs:rt:rd:00000:101101"), "%d, %s, %t", AddrTypeNone, InstrTypePsp),

            // DIVide (Unsigned).
            Id(InstructionNames.Div, Vm("000000:rs:rt:00000:00000:011010"), "%s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Divu, Vm("000000:rs:rt:00000:00000:011011"), "%s, %t", AddrTypeNone, 0),

            // MULTiply (Unsigned).
            Id(InstructionNames.Mult, Vm("000000:rs:rt:00000:00000:011000"), "%s, %t", AddrTypeNone, 0),
            Id(InstructionNames.Multu, Vm("000000:rs:rt:00000:00000:011001"), "%s, %t", AddrTypeNone, 0),

            // Multiply ADD/SUBstract (Unsigned).
            Id(InstructionNames.Madd, Vm("000000:rs:rt:00000:00000:011100"), "%s, %t", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Maddu, Vm("000000:rs:rt:00000:00000:011101"), "%s, %t", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Msub, Vm("000000:rs:rt:00000:00000:101110"), "%s, %t", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Msubu, Vm("000000:rs:rt:00000:00000:101111"), "%s, %t", AddrTypeNone, InstrTypePsp),

            // Move To/From HI/LO.
            Id(InstructionNames.Mfhi, Vm("000000:00000:00000:rd:00000:010000"), "%d", AddrTypeNone, 0),
            Id(InstructionNames.Mflo, Vm("000000:00000:00000:rd:00000:010010"), "%d", AddrTypeNone, 0),
            Id(InstructionNames.Mthi, Vm("000000:rs:00000:00000:00000:010001"), "%s", AddrTypeNone, 0),
            Id(InstructionNames.Mtlo, Vm("000000:rs:00000:00000:00000:010011"), "%s", AddrTypeNone, 0),

            // Move if Zero/Non zero.
            Id(InstructionNames.Movz, Vm("000000:rs:rt:rd:00000:001010"), "%d, %s, %t", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Movn, Vm("000000:rs:rt:rd:00000:001011"), "%d, %s, %t", AddrTypeNone, InstrTypePsp),

            // EXTract/INSert.
            Id(InstructionNames.Ext, Vm("011111:rs:rt:msb:lsb:000000"), "%t, %s, %a, %ne", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Ins, Vm("011111:rs:rt:msb:lsb:000100"), "%t, %s, %a, %ni", AddrTypeNone, InstrTypePsp),

            // Count Leading Ones/Zeros in word.
            Id(InstructionNames.Clz, Vm("000000:rs:00000:rd:00000:010110"), "%d, %s", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Clo, Vm("000000:rs:00000:rd:00000:010111"), "%d, %s", AddrTypeNone, InstrTypePsp),

            // Word Swap Bytes Within Halfwords/Words.
            Id(InstructionNames.Wsbh, Vm("011111:00000:rt:rd:00010:100000"), "%d, %t", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Wsbw, Vm("011111:00000:rt:rd:00011:100000"), "%d, %t", AddrTypeNone, InstrTypePsp),
        });

        private static InstructionInfo[] _bcu;

        public static InstructionInfo[] Bcu => _bcu ?? (_bcu = new[]
        {
            // Branch on EQuals (Likely).
            Id(InstructionNames.Beq, Vm("000100:rs:rt:imm16"), "%s, %t, %O", AddrType16, InstrTypeB),
            Id(InstructionNames.Beql, Vm("010100:rs:rt:imm16"), "%s, %t, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),

            // Branch on Greater Equal Zero (And Link) (Likely).
            Id(InstructionNames.Bgez, Vm("000001:rs:00001:imm16"), "%s, %O", AddrType16, InstrTypeB),
            Id(InstructionNames.Bgezl, Vm("000001:rs:00011:imm16"), "%s, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),
            Id(InstructionNames.Bgezal, Vm("000001:rs:10001:imm16"), "%s, %O", AddrType16, InstrTypeJal),
            Id(InstructionNames.Bgezall, Vm("000001:rs:10011:imm16"), "%s, %O", AddrType16,
                InstrTypeJal | InstrTypeLikely),

            // Branch on Less Than Zero (And Link) (Likely).
            Id(InstructionNames.Bltz, Vm("000001:rs:00000:imm16"), "%s, %O", AddrType16, InstrTypeB),
            Id(InstructionNames.Bltzl, Vm("000001:rs:00010:imm16"), "%s, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),
            Id(InstructionNames.Bltzal, Vm("000001:rs:10000:imm16"), "%s, %O", AddrType16, InstrTypeJal),
            Id(InstructionNames.Bltzall, Vm("000001:rs:10010:imm16"), "%s, %O", AddrType16,
                InstrTypeJal | InstrTypeLikely),

            // Branch on Less Or Equals than Zero (Likely).
            Id(InstructionNames.Blez, Vm("000110:rs:00000:imm16"), "%s, %O", AddrType16, InstrTypeB),
            Id(InstructionNames.Blezl, Vm("010110:rs:00000:imm16"), "%s, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),

            // Branch on Great Than Zero (Likely).
            Id(InstructionNames.Bgtz, Vm("000111:rs:00000:imm16"), "%s, %O", AddrType16, InstrTypeB),
            Id(InstructionNames.Bgtzl, Vm("010111:rs:00000:imm16"), "%s, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),

            // Branch on Not Equals (Likely).
            Id(InstructionNames.Bne, Vm("000101:rs:rt:imm16"), "%s, %t, %O", AddrType16, InstrTypeB),
            Id(InstructionNames.Bnel, Vm("010101:rs:rt:imm16"), "%s, %t, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),

            // Jump (And Link) (Register).
            Id(InstructionNames.J, Vm("000010:imm26"), "%j", AddrType26, InstrTypeJump),
            Id(InstructionNames.Jr, Vm("000000:rs:00000:00000:00000:001000"), "%J", AddrTypeReg, InstrTypeJump),
            Id(InstructionNames.Jalr, Vm("000000:rs:00000:rd:00000:001001"), "%J, %d", AddrTypeReg, InstrTypeJal),
            Id(InstructionNames.Jal, Vm("000011:imm26"), "%j", AddrType26, InstrTypeJal),

            // Branch on C1 False/True (Likely).
            Id(InstructionNames.Bc1F, Vm("010001:01000:00000:imm16"), "%O", AddrType16, InstrTypeB),
            Id(InstructionNames.Bc1T, Vm("010001:01000:00001:imm16"), "%O", AddrType16, InstrTypeB),
            Id(InstructionNames.Bc1Fl, Vm("010001:01000:00010:imm16"), "%O", AddrType16, InstrTypeB),
            Id(InstructionNames.Bc1Tl, Vm("010001:01000:00011:imm16"), "%O", AddrType16, InstrTypeB),
        });

        private static InstructionInfo[] _lsu;

        public static InstructionInfo[] Lsu => _lsu ?? (_lsu = new[]
        {
            // Load Byte/Half word/Word (Left/Right/Unsigned).
            Id(InstructionNames.Lb, Vm("100000:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Lh, Vm("100001:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Lw, Vm("100011:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Lwl, Vm("100010:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Lwr, Vm("100110:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Lbu, Vm("100100:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Lhu, Vm("100101:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),

            // Store Byte/Half word/Word (Left/Right).
            Id(InstructionNames.Sb, Vm("101000:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Sh, Vm("101001:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Sw, Vm("101011:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Swl, Vm("101010:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Swr, Vm("101110:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),

            // Load Linked word.
            // Store Conditional word.
            Id(InstructionNames.Ll, Vm("110000:rs:rt:imm16"), "%t, %O", AddrTypeNone, 0),
            Id(InstructionNames.Sc, Vm("111000:rs:rt:imm16"), "%t, %O", AddrTypeNone, 0),

            // Load Word to Cop1 floating point.
            // Store Word from Cop1 floating point.
            Id(InstructionNames.Lwc1, Vm("110001:rs:ft:imm16"), "%T, %i(%s)", AddrTypeNone, 0),
            Id(InstructionNames.Swc1, Vm("111001:rs:ft:imm16"), "%T, %i(%s)", AddrTypeNone, 0),
        });

        private static InstructionInfo[] _fpu;

        public static InstructionInfo[] Fpu => _fpu ?? (_fpu = new[]
        {
            // Binary Floating Point Unit Operations
            Id(InstructionNames.AddS, Vm("010001:10000:ft:fs:fd:000000"), "%D, %S, %T", AddrTypeNone, 0),
            Id(InstructionNames.SubS, Vm("010001:10000:ft:fs:fd:000001"), "%D, %S, %T", AddrTypeNone, 0),
            Id(InstructionNames.MulS, Vm("010001:10000:ft:fs:fd:000010"), "%D, %S, %T", AddrTypeNone, 0),
            Id(InstructionNames.DivS, Vm("010001:10000:ft:fs:fd:000011"), "%D, %S, %T", AddrTypeNone, 0),

            // Unary Floating Point Unit Operations
            Id(InstructionNames.SqrtS, Vm("010001:10000:00000:fs:fd:000100"), "%D, %S", AddrTypeNone, 0),
            Id(InstructionNames.AbsS, Vm("010001:10000:00000:fs:fd:000101"), "%D, %S", AddrTypeNone, 0),
            Id(InstructionNames.MovS, Vm("010001:10000:00000:fs:fd:000110"), "%D, %S", AddrTypeNone, 0),
            Id(InstructionNames.NegS, Vm("010001:10000:00000:fs:fd:000111"), "%D, %S", AddrTypeNone, 0),
            Id(InstructionNames.RoundWS, Vm("010001:10000:00000:fs:fd:001100"), "%D, %S", AddrTypeNone, 0),
            Id(InstructionNames.TruncWS, Vm("010001:10000:00000:fs:fd:001101"), "%D, %S", AddrTypeNone, 0),
            Id(InstructionNames.CeilWS, Vm("010001:10000:00000:fs:fd:001110"), "%D, %S", AddrTypeNone, 0),
            Id(InstructionNames.FloorWS, Vm("010001:10000:00000:fs:fd:001111"), "%D, %S", AddrTypeNone, 0),

            // Convert
            Id(InstructionNames.CvtSW, Vm("010001:10100:00000:fs:fd:100000"), "%D, %S", AddrTypeNone, 0),
            Id(InstructionNames.CvtWS, Vm("010001:10000:00000:fs:fd:100100"), "%D, %S", AddrTypeNone, 0),

            // Move float point registers
            Id(InstructionNames.Mfc1, Vm("010001:00000:rt:c1dr:00000:000000"), "%t, %S", AddrTypeNone, 0),
            Id(InstructionNames.Mtc1, Vm("010001:00100:rt:c1dr:00000:000000"), "%t, %S", AddrTypeNone, 0),
            // CFC1 -- move Control word from/to floating point (C1)
            Id(InstructionNames.Cfc1, Vm("010001:00010:rt:c1cr:00000:000000"), "%t, %p", AddrTypeNone, 0),
            Id(InstructionNames.Ctc1, Vm("010001:00110:rt:c1cr:00000:000000"), "%t, %p", AddrTypeNone, 0),

            // Compare <condition> Single.
            Id(InstructionNames.CFS, Vm("010001:10000:ft:fs:00000:11:0000"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CUnS, Vm("010001:10000:ft:fs:00000:11:0001"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CEqS, Vm("010001:10000:ft:fs:00000:11:0010"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CUeqS, Vm("010001:10000:ft:fs:00000:11:0011"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.COltS, Vm("010001:10000:ft:fs:00000:11:0100"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CUltS, Vm("010001:10000:ft:fs:00000:11:0101"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.COleS, Vm("010001:10000:ft:fs:00000:11:0110"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CUleS, Vm("010001:10000:ft:fs:00000:11:0111"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CSfS, Vm("010001:10000:ft:fs:00000:11:1000"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CNgleS, Vm("010001:10000:ft:fs:00000:11:1001"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CSeqS, Vm("010001:10000:ft:fs:00000:11:1010"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CNglS, Vm("010001:10000:ft:fs:00000:11:1011"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CLtS, Vm("010001:10000:ft:fs:00000:11:1100"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CNgeS, Vm("010001:10000:ft:fs:00000:11:1101"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CLeS, Vm("010001:10000:ft:fs:00000:11:1110"), "%S, %T", AddrTypeNone, 0),
            Id(InstructionNames.CNgtS, Vm("010001:10000:ft:fs:00000:11:1111"), "%S, %T", AddrTypeNone, 0),
        });

        private static InstructionInfo[] _special;

        public static InstructionInfo[] Special => _special ?? (_special = new[]
        {
            // Syscall
            Id(InstructionNames.Syscall, Vm("000000:imm20:001100"), "%C", AddrTypeNone, InstrTypeSyscall),

            Id(InstructionNames.Cache, Vm("101111--------------------------"), "%k, %o", AddrTypeNone, 0),
            Id(InstructionNames.Sync, Vm("000000:00000:00000:00000:00000:001111"), "", AddrTypeNone, 0),

            Id(InstructionNames.Break, Vm("000000:imm20:001101"), "%c", AddrTypeNone, 0),
            Id(InstructionNames.Dbreak, Vm("011100:00000:00000:00000:00000:111111"), "", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Halt, Vm("011100:00000:00000:00000:00000:000000"), "", AddrTypeNone, InstrTypePsp),

            // (D?/Exception) RETurn
            Id(InstructionNames.Dret, Vm("011100:00000:00000:00000:00000:111110"), "", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Eret, Vm("010000:10000:00000:00000:00000:011000"), "", AddrTypeNone, 0),

            // Move (From/To) IC
            Id(InstructionNames.Mfic, Vm("011100:rt:00000:00000:00000:100100"), "%t, %p", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Mtic, Vm("011100:rt:00000:00000:00000:100110"), "%t, %p", AddrTypeNone, InstrTypePsp),

            // Move (From/To) DR
            Id(InstructionNames.Mfdr, Vm("011100:00000:----------:00000:111101"), "%t, %r", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Mtdr, Vm("011100:00100:----------:00000:111101"), "%t, %r", AddrTypeNone,
                InstrTypePsp),
        });

        private static InstructionInfo[] _cop0;

        public static InstructionInfo[] Cop0 => _cop0 ?? (_cop0 = new[]
        {
            // C? (From/To) Cop0
            Id(InstructionNames.Cfc0, Vm("010000:00010:----------:00000:000000"), "%t, %p", AddrTypeNone,
                InstrTypePsp), // CFC0(010000:00010:rt:c0cr:00000:000000)
            Id(InstructionNames.Ctc0, Vm("010000:00110:----------:00000:000000"), "%t, %p", AddrTypeNone,
                InstrTypePsp), // CTC0(010000:00110:rt:c0cr:00000:000000)

            // Move (From/To) Cop0
            Id(InstructionNames.Mfc0, Vm("010000:00000:----------:00000:000000"), "%t, %0", AddrTypeNone,
                0), // MFC0(010000:00000:rt:c0dr:00000:000000)
            Id(InstructionNames.Mtc0, Vm("010000:00100:----------:00000:000000"), "%t, %0", AddrTypeNone,
                0), // MTC0(010000:00100:rt:c0dr:00000:000000)
        });

        private static InstructionInfo[] _vfpu;

        public static InstructionInfo[] Vfpu => _vfpu ?? (_vfpu = new[]
        {
            // Move From/to Vfpu (C?).
            Id(InstructionNames.Mfv, Vm("010010:00:011:rt:0:0000000:0:vd"), "%t, %zs", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Mfvc, Vm("010010:00:011:rt:0:0000000:1:vd"), "%t, %2d", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Mtv, Vm("010010:00:111:rt:0:0000000:0:vd"), "%t, %zs", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Mtvc, Vm("010010:00:111:rt:0:0000000:1:vd"), "%t, %2d", AddrTypeNone, InstrTypePsp),

            // Load/Store Vfpu (Left/Right).
            Id(InstructionNames.LvS, Vm("110010:rs:vt5:imm14:vt2"), "%Xs, %Y", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.LvQ, Vm("110110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.LvlQ, Vm("110101:rs:vt5:imm14:0:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.LvrQ, Vm("110101:rs:vt5:imm14:1:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.SvQ, Vm("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),

            // Vfpu DOT product
            // Vfpu SCaLe/ROTate
            Id(InstructionNames.Vdot, Vm("011001:001:vt:two:vs:one:vd"), "%zs, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vscl, Vm("011001:010:vt:two:vs:one:vd"), "%zp, %yp, %xs", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vsge, Vm("011011:110:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            //Id(InstructionNames.vslt,        VM("011011:100:vt:two:vs:one:vd"), "%zp, %yp, %xp", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
            Id(InstructionNames.Vslt, Vm("011011:111:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone,
                InstrTypePsp), // FIXED 2013-07-14

            // ROTate
            Id(InstructionNames.Vrot, Vm("111100:111:01:imm5:two:vs:one:vd"), "%zp, %ys, %vr", AddrTypeNone,
                InstrTypePsp),

            // Vfpu ZERO/ONE
            Id(InstructionNames.Vzero, Vm("110100:00:000:0:0110:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vone, Vm("110100:00:000:0:0111:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),

            // Vfpu MOVe/SiGN/Reverse SQuare root/COSine/Arc SINe/LOG2
            Id(InstructionNames.Vmov, Vm("110100:00:000:0:0000:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vabs, Vm("110100:00:000:0:0001:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vneg, Vm("110100:00:000:0:0010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vocp, Vm("110100:00:010:0:0100:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vsgn, Vm("110100:00:010:0:1010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vrcp, Vm("110100:00:000:1:0000:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vrsq, Vm("110100:00:000:1:0001:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vsin, Vm("110100:00:000:1:0010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vcos, Vm("110100:00:000:1:0011:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vexp2, Vm("110100:00:000:1:0100:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vlog2, Vm("110100:00:000:1:0101:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vsqrt, Vm("110100:00:000:1:0110:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vasin, Vm("110100:00:000:1:0111:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vnrcp, Vm("110100:00:000:1:1000:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vnsin, Vm("110100:00:000:1:1010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vrexp2, Vm("110100:00:000:1:1100:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vsat0, Vm("110100:00:000:0:0100:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vsat1, Vm("110100:00:000:0:0101:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),

            // Vfpu ConSTant
            Id(InstructionNames.Vcst, Vm("110100:00:011:imm5:two:0000000:one:vd"), "%zp, %vk", AddrTypeNone,
                InstrTypePsp),

            // Vfpu Matrix MULtiplication
            Id(InstructionNames.Vmmul, Vm("111100:000:vt:two:vs:one:vd"), "%zm, %tym, %xm", AddrTypeNone,
                InstrTypePsp),

            // -
            Id(InstructionNames.Vhdp, Vm("011001:100:vt:two:vs:one:vd"), "%zs, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.VcrsT, Vm("011001:101:vt:1:vs:0:vd"), "%zt, %yt, %xt", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.VcrspT, Vm("111100:101:vt:1:vs:0:vd"), "%zt, %yt, %xt", AddrTypeNone, InstrTypePsp),

            // Vfpu Integer to(2) Color
            Id(InstructionNames.Vi2C, Vm("110100:00:001:11:101:two:vs:one:vd"), "%zs, %yq", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vi2Uc, Vm("110100:00:001:11:100:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            // -
            Id(InstructionNames.Vtfm2, Vm("111100:001:vt:0:vs:1:vd"), "%zp, %ym, %xp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vtfm3, Vm("111100:010:vt:1:vs:0:vd"), "%zt, %yn, %xt", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vtfm4, Vm("111100:011:vt:1:vs:1:vd"), "%zq, %yo, %xq", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Vhtfm2, Vm("111100:001:vt:0:vs:0:vd"), "%zp, %ym, %xp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vhtfm3, Vm("111100:010:vt:0:vs:1:vd"), "%zt, %yn, %xt", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vhtfm4, Vm("111100:011:vt:1:vs:0:vd"), "%zq, %yo, %xq", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vsrt3, Vm("110100:00:010:01000:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vfad, Vm("110100:00:010:00110:two:vs:one:vd"), "%zp, %yp", AddrTypeNone, InstrTypePsp),

            // Vfpu MINimum/MAXium/ADD/SUB/DIV/MUL
            Id(InstructionNames.Vmin, Vm("011011:010:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vmax, Vm("011011:011:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vadd, Vm("011000:000:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vsub, Vm("011000:001:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vdiv, Vm("011000:111:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vmul, Vm("011001:000:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),

            // Vfpu (Matrix) IDenTity
            Id(InstructionNames.Vidt, Vm("110100:00:000:0:0011:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vmidt, Vm("111100:111:00:00011:two:0000000:one:vd"), "%zm", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Viim, Vm("110111:11:0:vd:imm16"), "%xs, %vi", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Vmmov, Vm("111100:111:00:00000:two:vs:one:vd"), "%zm, %ym", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vmzero, Vm("111100:111:00:00110:two:0000000:one:vd"), "%zm", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vmone, Vm("111100:111:00:00111:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vnop, Vm("111111:1111111111:00000:00000000000"), "", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vsync, Vm("111111:1111111111:00000:01100100000"), "", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vflush, Vm("111111:1111111111:00000:10000001101"), "", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Vpfxd, Vm("110111:10:------------:mskw:mskz:msky:mskx:satw:satz:saty:satx"),
                "[%vp4, %vp5, %vp6, %vp7]", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vpfxs,
                Vm(
                    "110111:00:----:negw:negz:negy:negx:cstw:cstz:csty:cstx:absw:absz:absy:absx:swzw:swzz:swzy:swzx"),
                "[%vp0, %vp1, %vp2, %vp3]", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vpfxt,
                Vm(
                    "110111:01:----:negw:negz:negy:negx:cstw:cstz:csty:cstx:absw:absz:absy:absx:swzw:swzz:swzy:swzx"),
                "[%vp0, %vp1, %vp2, %vp3]", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Vdet, Vm("011001:110:vt:two:vs:one:vd"), "%zs, %yp, %xp", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Vrnds, Vm("110100:00:001:00:000:two:vs:one:0000000"), "%ys", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vrndi, Vm("110100:00:001:00:001:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vrndf1, Vm("110100:00:001:00:010:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vrndf2, Vm("110100:00:001:00:011:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vcmp, Vm("011011:000:vt:two:vs:one:000:imm4"), "%Zn, %yp, %xp", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vcmovf, Vm("110100:10:101:01:imm3:two:vs:one:vd"), "%zp, %yp, %v3", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vcmovt, Vm("110100:10:101:00:imm3:two:vs:one:vd"), "%zp, %yp, %v3", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vavg, Vm("110100:00:010:00111:two:vs:one:vd"), "%zp, %yp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vf2Id, Vm("110100:10:011:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vf2In, Vm("110100:10:000:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vf2Iu, Vm("110100:10:010:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vf2Iz, Vm("110100:10:001:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vi2F, Vm("110100:10:100:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vscmp, Vm("011011:101:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vmscl, Vm("111100:100:vt:two:vs:one:vd"), "%zm, %ym, %xs", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Vt4444Q, Vm("110100:00:010:11001:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vt5551Q, Vm("110100:00:010:11010:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vt5650Q, Vm("110100:00:010:11011:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vmfvc, Vm("110100:00:010:10000:1:imm7:0:vd"), "%zs, %2s", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vmtvc, Vm("110100:00:010:10001:0:vs:1:imm7"), "%2d, %ys", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Mfvme, Vm("011010--------------------------"), "%t, %i", AddrTypeNone, 0),
            Id(InstructionNames.Mtvme, Vm("101100--------------------------"), "%t, %i", AddrTypeNone, 0),

            Id(InstructionNames.SvS, Vm("111010:rs:vt5:imm14:vt2"), "%Xs, %Y", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Vfim, Vm("110111:11:1:vt:imm16"), "%xs, %vh", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.SvlQ, Vm("111101:rs:vt5:imm14:0:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.SvrQ, Vm("111101:rs:vt5:imm14:1:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Vbfy1, Vm("110100:00:010:00010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vbfy2, Vm("110100:00:010:00011:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vf2H, Vm("110100:00:001:10:010:two:vs:one:vd"), "%zs, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vh2F, Vm("110100:00:001:10:011:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vi2S, Vm("110100:00:001:11:111:two:vs:one:vd"), "%zs, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vi2Us, Vm("110100:00:001:11:110:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vlgb, Vm("110100:00:001:10:111:two:vs:one:vd"), "%zs, %ys", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vqmul, Vm("111100:101:vt:1:vs:1:vd"), "%zq, %yq, %xq", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vs2I, Vm("110100:00:001:11:011:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),

            // Working on it.

            //"110100:00:001:11:000:1000000010000001"
            Id(InstructionNames.Vc2I, Vm("110100:00:001:11:001:two:vs:one:vd"), "%zs, %ys, %xs", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vuc2I, Vm("110100:00:001:11:000:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),


            Id(InstructionNames.Vsbn, Vm("011000:010:vt:two:vs:one:vd"), "%zs, %ys, %xs", AddrTypeNone, InstrTypePsp),

            Id(InstructionNames.Vsbz, Vm("110100:00:001:10110:two:vs:one:vd"), "%zs, %ys", AddrTypeNone, InstrTypePsp),
            Id(InstructionNames.Vsocp, Vm("110100:00:010:00101:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vsrt1, Vm("110100:00:010:00000:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vsrt2, Vm("110100:00:010:00001:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vsrt4, Vm("110100:00:010:01001:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id(InstructionNames.Vus2I, Vm("110100:00:001:11010:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),

            Id(InstructionNames.Vwbn, Vm("110100:11:imm8:two:vs:one:vd"), "%zs, %xs, %I", AddrTypeNone, InstrTypePsp),
            //Id(InstructionNames.vwb.q,       VM("111110------------------------1-"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
        });

        private static InstructionInfo[] _vfpuBranch;

        public static InstructionInfo[] VfpuBranch => _vfpuBranch ?? (_vfpuBranch = new[]
        {
            Id(InstructionNames.Bvf, Vm("010010:01:000:imm3:00:imm16"), "%Zc, %O", AddrType16,
                InstrTypePsp | InstrTypeB),
            Id(InstructionNames.Bvt, Vm("010010:01:000:imm3:01:imm16"), "%Zc, %O", AddrType16,
                InstrTypePsp | InstrTypeB),
            Id(InstructionNames.Bvfl, Vm("010010:01:000:imm3:10:imm16"), "%Zc, %O", AddrType16,
                InstrTypePsp | InstrTypeB | InstrTypeLikely),
            Id(InstructionNames.Bvtl, Vm("010010:01:000:imm3:11:imm16"), "%Zc, %O", AddrType16,
                InstrTypePsp | InstrTypeB | InstrTypeLikely),
        });
    }
}
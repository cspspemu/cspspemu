using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Cpu.Emitter;
using SafeILGenerator.Ast.Nodes;

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

        public static IEnumerable<InstructionInfo> ALL => new InstructionInfo[] { }
            .Union(ALU)
            .Union(BCU)
            .Union(BCU)
            .Union(COP0)
            .Union(FPU)
            .Union(LSU)
            .Union(SPECIAL)
            .Union(VFPU)
            .Union(VFPU_BRANCH);

        public static IEnumerable<InstructionInfo> ALL_BRANCHES => new InstructionInfo[] { }
            .Union(BCU)
            .Union(VFPU_BRANCH);

        public static InstructionInfo Unknown = Id("unknwon", Vm("111111:11111:11111:11111:11111:111111"), "",
            AddrTypeNone, 0);

        private static InstructionInfo[] _ALU;

        public static InstructionInfo[] ALU => _ALU ?? (_ALU = new[]
        {
            // Arithmetic operations.
            Id("add", Vm("000000:rs:rt:rd:00000:100000"), "%d, %s, %t", AddrTypeNone, 0),
            Id("addu", Vm("000000:rs:rt:rd:00000:100001"), "%d, %s, %t", AddrTypeNone, 0),
            Id("addi", Vm("001000:rs:rt:imm16"), "%t, %s, %i", AddrTypeNone, 0),
            Id("addiu", Vm("001001:rs:rt:imm16"), "%t, %s, %i", AddrTypeNone, 0),
            Id("sub", Vm("000000:rs:rt:rd:00000:100010"), "%d, %s, %t", AddrTypeNone, 0),
            Id("subu", Vm("000000:rs:rt:rd:00000:100011"), "%d, %s, %t", AddrTypeNone, 0),

            // Logical Operations.
            Id("and", Vm("000000:rs:rt:rd:00000:100100"), "%d, %s, %t", AddrTypeNone, 0),
            Id("andi", Vm("001100:rs:rt:imm16"), "%t, %s, %I", AddrTypeNone, 0),
            Id("nor", Vm("000000:rs:rt:rd:00000:100111"), "%d, %s, %t", AddrTypeNone, 0),
            Id("or", Vm("000000:rs:rt:rd:00000:100101"), "%d, %s, %t", AddrTypeNone, 0),
            Id("ori", Vm("001101:rs:rt:imm16"), "%t, %s, %I", AddrTypeNone, 0),
            Id("xor", Vm("000000:rs:rt:rd:00000:100110"), "%d, %s, %t", AddrTypeNone, 0),
            Id("xori", Vm("001110:rs:rt:imm16"), "%t, %s, %I", AddrTypeNone, 0),

            // Shift Left/Right Logical/Arithmethic (Variable).
            Id("sll", Vm("000000:00000:rt:rd:sa:000000"), "%d, %t, %a", AddrTypeNone, 0),
            Id("sllv", Vm("000000:rs:rt:rd:00000:000100"), "%d, %t, %s", AddrTypeNone, 0),
            Id("sra", Vm("000000:00000:rt:rd:sa:000011"), "%d, %t, %a", AddrTypeNone, 0),
            Id("srav", Vm("000000:rs:rt:rd:00000:000111"), "%d, %t, %s", AddrTypeNone, 0),
            Id("srl", Vm("000000:00000:rt:rd:sa:000010"), "%d, %t, %a", AddrTypeNone, 0),
            Id("srlv", Vm("000000:rs:rt:rd:00000:000110"), "%d, %t, %s", AddrTypeNone, 0),
            Id("rotr", Vm("000000:00001:rt:rd:sa:000010"), "%d, %t, %a", AddrTypeNone, 0),
            Id("rotrv", Vm("000000:rs:rt:rd:00001:000110"), "%d, %t, %s", AddrTypeNone, 0),

            // Set Less Than (Immediate) (Unsigned).
            Id("slt", Vm("000000:rs:rt:rd:00000:101010"), "%d, %s, %t", AddrTypeNone, 0),
            Id("slti", Vm("001010:rs:rt:imm16"), "%t, %s, %i", AddrTypeNone, 0),
            Id("sltu", Vm("000000:rs:rt:rd:00000:101011"), "%d, %s, %t", AddrTypeNone, 0),
            Id("sltiu", Vm("001011:rs:rt:imm16"), "%t, %s, %i", AddrTypeNone, 0),

            // Load Upper Immediate.
            Id("lui", Vm("001111:00000:rt:imm16"), "%t, %I", AddrTypeNone, 0),

            // Sign Extend Byte/Half word.
            Id("seb", Vm("011111:00000:rt:rd:10000:100000"), "%d, %t", AddrTypeNone, 0),
            Id("seh", Vm("011111:00000:rt:rd:11000:100000"), "%d, %t", AddrTypeNone, 0),

            // BIT REVerse.
            Id("bitrev", Vm("011111:00000:rt:rd:10100:100000"), "%d, %t", AddrTypeNone, InstrTypePsp),

            // MAXimum/MINimum.
            Id("max", Vm("000000:rs:rt:rd:00000:101100"), "%d, %s, %t", AddrTypeNone, InstrTypePsp),
            Id("min", Vm("000000:rs:rt:rd:00000:101101"), "%d, %s, %t", AddrTypeNone, InstrTypePsp),

            // DIVide (Unsigned).
            Id("div", Vm("000000:rs:rt:00000:00000:011010"), "%s, %t", AddrTypeNone, 0),
            Id("divu", Vm("000000:rs:rt:00000:00000:011011"), "%s, %t", AddrTypeNone, 0),

            // MULTiply (Unsigned).
            Id("mult", Vm("000000:rs:rt:00000:00000:011000"), "%s, %t", AddrTypeNone, 0),
            Id("multu", Vm("000000:rs:rt:00000:00000:011001"), "%s, %t", AddrTypeNone, 0),

            // Multiply ADD/SUBstract (Unsigned).
            Id("madd", Vm("000000:rs:rt:00000:00000:011100"), "%s, %t", AddrTypeNone, InstrTypePsp),
            Id("maddu", Vm("000000:rs:rt:00000:00000:011101"), "%s, %t", AddrTypeNone, InstrTypePsp),
            Id("msub", Vm("000000:rs:rt:00000:00000:101110"), "%s, %t", AddrTypeNone, InstrTypePsp),
            Id("msubu", Vm("000000:rs:rt:00000:00000:101111"), "%s, %t", AddrTypeNone, InstrTypePsp),

            // Move To/From HI/LO.
            Id("mfhi", Vm("000000:00000:00000:rd:00000:010000"), "%d", AddrTypeNone, 0),
            Id("mflo", Vm("000000:00000:00000:rd:00000:010010"), "%d", AddrTypeNone, 0),
            Id("mthi", Vm("000000:rs:00000:00000:00000:010001"), "%s", AddrTypeNone, 0),
            Id("mtlo", Vm("000000:rs:00000:00000:00000:010011"), "%s", AddrTypeNone, 0),

            // Move if Zero/Non zero.
            Id("movz", Vm("000000:rs:rt:rd:00000:001010"), "%d, %s, %t", AddrTypeNone, InstrTypePsp),
            Id("movn", Vm("000000:rs:rt:rd:00000:001011"), "%d, %s, %t", AddrTypeNone, InstrTypePsp),

            // EXTract/INSert.
            Id("ext", Vm("011111:rs:rt:msb:lsb:000000"), "%t, %s, %a, %ne", AddrTypeNone, InstrTypePsp),
            Id("ins", Vm("011111:rs:rt:msb:lsb:000100"), "%t, %s, %a, %ni", AddrTypeNone, InstrTypePsp),

            // Count Leading Ones/Zeros in word.
            Id("clz", Vm("000000:rs:00000:rd:00000:010110"), "%d, %s", AddrTypeNone, InstrTypePsp),
            Id("clo", Vm("000000:rs:00000:rd:00000:010111"), "%d, %s", AddrTypeNone, InstrTypePsp),

            // Word Swap Bytes Within Halfwords/Words.
            Id("wsbh", Vm("011111:00000:rt:rd:00010:100000"), "%d, %t", AddrTypeNone, InstrTypePsp),
            Id("wsbw", Vm("011111:00000:rt:rd:00011:100000"), "%d, %t", AddrTypeNone, InstrTypePsp),
        });

        private static InstructionInfo[] _BCU;

        public static InstructionInfo[] BCU => _BCU ?? (_BCU = new[]
        {
            // Branch on EQuals (Likely).
            Id("beq", Vm("000100:rs:rt:imm16"), "%s, %t, %O", AddrType16, InstrTypeB),
            Id("beql", Vm("010100:rs:rt:imm16"), "%s, %t, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),

            // Branch on Greater Equal Zero (And Link) (Likely).
            Id("bgez", Vm("000001:rs:00001:imm16"), "%s, %O", AddrType16, InstrTypeB),
            Id("bgezl", Vm("000001:rs:00011:imm16"), "%s, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),
            Id("bgezal", Vm("000001:rs:10001:imm16"), "%s, %O", AddrType16, InstrTypeJal),
            Id("bgezall", Vm("000001:rs:10011:imm16"), "%s, %O", AddrType16,
                InstrTypeJal | InstrTypeLikely),

            // Branch on Less Than Zero (And Link) (Likely).
            Id("bltz", Vm("000001:rs:00000:imm16"), "%s, %O", AddrType16, InstrTypeB),
            Id("bltzl", Vm("000001:rs:00010:imm16"), "%s, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),
            Id("bltzal", Vm("000001:rs:10000:imm16"), "%s, %O", AddrType16, InstrTypeJal),
            Id("bltzall", Vm("000001:rs:10010:imm16"), "%s, %O", AddrType16,
                InstrTypeJal | InstrTypeLikely),

            // Branch on Less Or Equals than Zero (Likely).
            Id("blez", Vm("000110:rs:00000:imm16"), "%s, %O", AddrType16, InstrTypeB),
            Id("blezl", Vm("010110:rs:00000:imm16"), "%s, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),

            // Branch on Great Than Zero (Likely).
            Id("bgtz", Vm("000111:rs:00000:imm16"), "%s, %O", AddrType16, InstrTypeB),
            Id("bgtzl", Vm("010111:rs:00000:imm16"), "%s, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),

            // Branch on Not Equals (Likely).
            Id("bne", Vm("000101:rs:rt:imm16"), "%s, %t, %O", AddrType16, InstrTypeB),
            Id("bnel", Vm("010101:rs:rt:imm16"), "%s, %t, %O", AddrType16,
                InstrTypeB | InstrTypeLikely),

            // Jump (And Link) (Register).
            Id("j", Vm("000010:imm26"), "%j", AddrType26, InstrTypeJump),
            Id("jr", Vm("000000:rs:00000:00000:00000:001000"), "%J", AddrTypeReg, InstrTypeJump),
            Id("jalr", Vm("000000:rs:00000:rd:00000:001001"), "%J, %d", AddrTypeReg, InstrTypeJal),
            Id("jal", Vm("000011:imm26"), "%j", AddrType26, InstrTypeJal),

            // Branch on C1 False/True (Likely).
            Id("bc1f", Vm("010001:01000:00000:imm16"), "%O", AddrType16, InstrTypeB),
            Id("bc1t", Vm("010001:01000:00001:imm16"), "%O", AddrType16, InstrTypeB),
            Id("bc1fl", Vm("010001:01000:00010:imm16"), "%O", AddrType16, InstrTypeB),
            Id("bc1tl", Vm("010001:01000:00011:imm16"), "%O", AddrType16, InstrTypeB),
        });

        private static InstructionInfo[] _LSU;

        public static InstructionInfo[] LSU => _LSU ?? (_LSU = new[]
        {
            // Load Byte/Half word/Word (Left/Right/Unsigned).
            Id("lb", Vm("100000:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("lh", Vm("100001:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("lw", Vm("100011:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("lwl", Vm("100010:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("lwr", Vm("100110:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("lbu", Vm("100100:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("lhu", Vm("100101:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),

            // Store Byte/Half word/Word (Left/Right).
            Id("sb", Vm("101000:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("sh", Vm("101001:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("sw", Vm("101011:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("swl", Vm("101010:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),
            Id("swr", Vm("101110:rs:rt:imm16"), "%t, %i(%s)", AddrTypeNone, 0),

            // Load Linked word.
            // Store Conditional word.
            Id("ll", Vm("110000:rs:rt:imm16"), "%t, %O", AddrTypeNone, 0),
            Id("sc", Vm("111000:rs:rt:imm16"), "%t, %O", AddrTypeNone, 0),

            // Load Word to Cop1 floating point.
            // Store Word from Cop1 floating point.
            Id("lwc1", Vm("110001:rs:ft:imm16"), "%T, %i(%s)", AddrTypeNone, 0),
            Id("swc1", Vm("111001:rs:ft:imm16"), "%T, %i(%s)", AddrTypeNone, 0),
        });

        public static InstructionInfo[] _FPU;

        public static InstructionInfo[] FPU => _FPU ?? (_FPU = new InstructionInfo[]
        {
            // Binary Floating Point Unit Operations
            Id("add.s", Vm("010001:10000:ft:fs:fd:000000"), "%D, %S, %T", AddrTypeNone, 0),
            Id("sub.s", Vm("010001:10000:ft:fs:fd:000001"), "%D, %S, %T", AddrTypeNone, 0),
            Id("mul.s", Vm("010001:10000:ft:fs:fd:000010"), "%D, %S, %T", AddrTypeNone, 0),
            Id("div.s", Vm("010001:10000:ft:fs:fd:000011"), "%D, %S, %T", AddrTypeNone, 0),

            // Unary Floating Point Unit Operations
            Id("sqrt.s", Vm("010001:10000:00000:fs:fd:000100"), "%D, %S", AddrTypeNone, 0),
            Id("abs.s", Vm("010001:10000:00000:fs:fd:000101"), "%D, %S", AddrTypeNone, 0),
            Id("mov.s", Vm("010001:10000:00000:fs:fd:000110"), "%D, %S", AddrTypeNone, 0),
            Id("neg.s", Vm("010001:10000:00000:fs:fd:000111"), "%D, %S", AddrTypeNone, 0),
            Id("round.w.s", Vm("010001:10000:00000:fs:fd:001100"), "%D, %S", AddrTypeNone, 0),
            Id("trunc.w.s", Vm("010001:10000:00000:fs:fd:001101"), "%D, %S", AddrTypeNone, 0),
            Id("ceil.w.s", Vm("010001:10000:00000:fs:fd:001110"), "%D, %S", AddrTypeNone, 0),
            Id("floor.w.s", Vm("010001:10000:00000:fs:fd:001111"), "%D, %S", AddrTypeNone, 0),

            // Convert
            Id("cvt.s.w", Vm("010001:10100:00000:fs:fd:100000"), "%D, %S", AddrTypeNone, 0),
            Id("cvt.w.s", Vm("010001:10000:00000:fs:fd:100100"), "%D, %S", AddrTypeNone, 0),

            // Move float point registers
            Id("mfc1", Vm("010001:00000:rt:c1dr:00000:000000"), "%t, %S", AddrTypeNone, 0),
            Id("mtc1", Vm("010001:00100:rt:c1dr:00000:000000"), "%t, %S", AddrTypeNone, 0),
            // CFC1 -- move Control word from/to floating point (C1)
            Id("cfc1", Vm("010001:00010:rt:c1cr:00000:000000"), "%t, %p", AddrTypeNone, 0),
            Id("ctc1", Vm("010001:00110:rt:c1cr:00000:000000"), "%t, %p", AddrTypeNone, 0),

            // Compare <condition> Single.
            Id("c.f.s", Vm("010001:10000:ft:fs:00000:11:0000"), "%S, %T", AddrTypeNone, 0),
            Id("c.un.s", Vm("010001:10000:ft:fs:00000:11:0001"), "%S, %T", AddrTypeNone, 0),
            Id("c.eq.s", Vm("010001:10000:ft:fs:00000:11:0010"), "%S, %T", AddrTypeNone, 0),
            Id("c.ueq.s", Vm("010001:10000:ft:fs:00000:11:0011"), "%S, %T", AddrTypeNone, 0),
            Id("c.olt.s", Vm("010001:10000:ft:fs:00000:11:0100"), "%S, %T", AddrTypeNone, 0),
            Id("c.ult.s", Vm("010001:10000:ft:fs:00000:11:0101"), "%S, %T", AddrTypeNone, 0),
            Id("c.ole.s", Vm("010001:10000:ft:fs:00000:11:0110"), "%S, %T", AddrTypeNone, 0),
            Id("c.ule.s", Vm("010001:10000:ft:fs:00000:11:0111"), "%S, %T", AddrTypeNone, 0),
            Id("c.sf.s", Vm("010001:10000:ft:fs:00000:11:1000"), "%S, %T", AddrTypeNone, 0),
            Id("c.ngle.s", Vm("010001:10000:ft:fs:00000:11:1001"), "%S, %T", AddrTypeNone, 0),
            Id("c.seq.s", Vm("010001:10000:ft:fs:00000:11:1010"), "%S, %T", AddrTypeNone, 0),
            Id("c.ngl.s", Vm("010001:10000:ft:fs:00000:11:1011"), "%S, %T", AddrTypeNone, 0),
            Id("c.lt.s", Vm("010001:10000:ft:fs:00000:11:1100"), "%S, %T", AddrTypeNone, 0),
            Id("c.nge.s", Vm("010001:10000:ft:fs:00000:11:1101"), "%S, %T", AddrTypeNone, 0),
            Id("c.le.s", Vm("010001:10000:ft:fs:00000:11:1110"), "%S, %T", AddrTypeNone, 0),
            Id("c.ngt.s", Vm("010001:10000:ft:fs:00000:11:1111"), "%S, %T", AddrTypeNone, 0),
        });

        private static InstructionInfo[] _SPECIAL;

        public static InstructionInfo[] SPECIAL => _SPECIAL ?? (_SPECIAL = new[]
        {
            // Syscall
            Id("syscall", Vm("000000:imm20:001100"), "%C", AddrTypeNone, InstrTypeSyscall),

            Id("cache", Vm("101111--------------------------"), "%k, %o", AddrTypeNone, 0),
            Id("sync", Vm("000000:00000:00000:00000:00000:001111"), "", AddrTypeNone, 0),

            Id("break", Vm("000000:imm20:001101"), "%c", AddrTypeNone, 0),
            Id("dbreak", Vm("011100:00000:00000:00000:00000:111111"), "", AddrTypeNone, InstrTypePsp),
            Id("halt", Vm("011100:00000:00000:00000:00000:000000"), "", AddrTypeNone, InstrTypePsp),

            // (D?/Exception) RETurn
            Id("dret", Vm("011100:00000:00000:00000:00000:111110"), "", AddrTypeNone, InstrTypePsp),
            Id("eret", Vm("010000:10000:00000:00000:00000:011000"), "", AddrTypeNone, 0),

            // Move (From/To) IC
            Id("mfic", Vm("011100:rt:00000:00000:00000:100100"), "%t, %p", AddrTypeNone, InstrTypePsp),
            Id("mtic", Vm("011100:rt:00000:00000:00000:100110"), "%t, %p", AddrTypeNone, InstrTypePsp),

            // Move (From/To) DR
            Id("mfdr", Vm("011100:00000:----------:00000:111101"), "%t, %r", AddrTypeNone,
                InstrTypePsp),
            Id("mtdr", Vm("011100:00100:----------:00000:111101"), "%t, %r", AddrTypeNone,
                InstrTypePsp),
        });

        public static InstructionInfo[] _COP0;

        public static InstructionInfo[] COP0 => _COP0 ?? (_COP0 = new[]
        {
            // C? (From/To) Cop0
            Id("cfc0", Vm("010000:00010:----------:00000:000000"), "%t, %p", AddrTypeNone,
                InstrTypePsp), // CFC0(010000:00010:rt:c0cr:00000:000000)
            Id("ctc0", Vm("010000:00110:----------:00000:000000"), "%t, %p", AddrTypeNone,
                InstrTypePsp), // CTC0(010000:00110:rt:c0cr:00000:000000)

            // Move (From/To) Cop0
            Id("mfc0", Vm("010000:00000:----------:00000:000000"), "%t, %0", AddrTypeNone,
                0), // MFC0(010000:00000:rt:c0dr:00000:000000)
            Id("mtc0", Vm("010000:00100:----------:00000:000000"), "%t, %0", AddrTypeNone,
                0), // MTC0(010000:00100:rt:c0dr:00000:000000)
        });

        private static InstructionInfo[] _VFPU;

        public static InstructionInfo[] VFPU => _VFPU ?? (_VFPU = new[]
        {
            // Move From/to Vfpu (C?).
            Id("mfv", Vm("010010:00:011:rt:0:0000000:0:vd"), "%t, %zs", AddrTypeNone, InstrTypePsp),
            Id("mfvc", Vm("010010:00:011:rt:0:0000000:1:vd"), "%t, %2d", AddrTypeNone, InstrTypePsp),
            Id("mtv", Vm("010010:00:111:rt:0:0000000:0:vd"), "%t, %zs", AddrTypeNone, InstrTypePsp),
            Id("mtvc", Vm("010010:00:111:rt:0:0000000:1:vd"), "%t, %2d", AddrTypeNone, InstrTypePsp),

            // Load/Store Vfpu (Left/Right).
            Id("lv.s", Vm("110010:rs:vt5:imm14:vt2"), "%Xs, %Y", AddrTypeNone, InstrTypePsp),
            Id("lv.q", Vm("110110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),
            Id("lvl.q", Vm("110101:rs:vt5:imm14:0:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),
            Id("lvr.q", Vm("110101:rs:vt5:imm14:1:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),
            Id("sv.q", Vm("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),

            // Vfpu DOT product
            // Vfpu SCaLe/ROTate
            Id("vdot", Vm("011001:001:vt:two:vs:one:vd"), "%zs, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id("vscl", Vm("011001:010:vt:two:vs:one:vd"), "%zp, %yp, %xs", AddrTypeNone, InstrTypePsp),
            Id("vsge", Vm("011011:110:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            //ID("vslt",        VM("011011:100:vt:two:vs:one:vd"), "%zp, %yp, %xp", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
            Id("vslt", Vm("011011:111:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone,
                InstrTypePsp), // FIXED 2013-07-14

            // ROTate
            Id("vrot", Vm("111100:111:01:imm5:two:vs:one:vd"), "%zp, %ys, %vr", AddrTypeNone,
                InstrTypePsp),

            // Vfpu ZERO/ONE
            Id("vzero", Vm("110100:00:000:0:0110:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),
            Id("vone", Vm("110100:00:000:0:0111:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),

            // Vfpu MOVe/SiGN/Reverse SQuare root/COSine/Arc SINe/LOG2
            Id("vmov", Vm("110100:00:000:0:0000:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vabs", Vm("110100:00:000:0:0001:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vneg", Vm("110100:00:000:0:0010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vocp", Vm("110100:00:010:0:0100:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vsgn", Vm("110100:00:010:0:1010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vrcp", Vm("110100:00:000:1:0000:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vrsq", Vm("110100:00:000:1:0001:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vsin", Vm("110100:00:000:1:0010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vcos", Vm("110100:00:000:1:0011:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vexp2", Vm("110100:00:000:1:0100:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vlog2", Vm("110100:00:000:1:0101:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vsqrt", Vm("110100:00:000:1:0110:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vasin", Vm("110100:00:000:1:0111:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vnrcp", Vm("110100:00:000:1:1000:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vnsin", Vm("110100:00:000:1:1010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vrexp2", Vm("110100:00:000:1:1100:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),

            Id("vsat0", Vm("110100:00:000:0:0100:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vsat1", Vm("110100:00:000:0:0101:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),

            // Vfpu ConSTant
            Id("vcst", Vm("110100:00:011:imm5:two:0000000:one:vd"), "%zp, %vk", AddrTypeNone,
                InstrTypePsp),

            // Vfpu Matrix MULtiplication
            Id("vmmul", Vm("111100:000:vt:two:vs:one:vd"), "%zm, %tym, %xm", AddrTypeNone,
                InstrTypePsp),

            // -
            Id("vhdp", Vm("011001:100:vt:two:vs:one:vd"), "%zs, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id("vcrs.t", Vm("011001:101:vt:1:vs:0:vd"), "%zt, %yt, %xt", AddrTypeNone, InstrTypePsp),
            Id("vcrsp.t", Vm("111100:101:vt:1:vs:0:vd"), "%zt, %yt, %xt", AddrTypeNone, InstrTypePsp),

            // Vfpu Integer to(2) Color
            Id("vi2c", Vm("110100:00:001:11:101:two:vs:one:vd"), "%zs, %yq", AddrTypeNone,
                InstrTypePsp),
            Id("vi2uc", Vm("110100:00:001:11:100:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            // -
            Id("vtfm2", Vm("111100:001:vt:0:vs:1:vd"), "%zp, %ym, %xp", AddrTypeNone, InstrTypePsp),
            Id("vtfm3", Vm("111100:010:vt:1:vs:0:vd"), "%zt, %yn, %xt", AddrTypeNone, InstrTypePsp),
            Id("vtfm4", Vm("111100:011:vt:1:vs:1:vd"), "%zq, %yo, %xq", AddrTypeNone, InstrTypePsp),

            Id("vhtfm2", Vm("111100:001:vt:0:vs:0:vd"), "%zp, %ym, %xp", AddrTypeNone,
                InstrTypePsp),
            Id("vhtfm3", Vm("111100:010:vt:0:vs:1:vd"), "%zt, %yn, %xt", AddrTypeNone,
                InstrTypePsp),
            Id("vhtfm4", Vm("111100:011:vt:1:vs:0:vd"), "%zq, %yo, %xq", AddrTypeNone,
                InstrTypePsp),

            Id("vsrt3", Vm("110100:00:010:01000:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            Id("vfad", Vm("110100:00:010:00110:two:vs:one:vd"), "%zp, %yp", AddrTypeNone, InstrTypePsp),

            // Vfpu MINimum/MAXium/ADD/SUB/DIV/MUL
            Id("vmin", Vm("011011:010:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id("vmax", Vm("011011:011:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id("vadd", Vm("011000:000:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id("vsub", Vm("011000:001:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id("vdiv", Vm("011000:111:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id("vmul", Vm("011001:000:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),

            // Vfpu (Matrix) IDenTity
            Id("vidt", Vm("110100:00:000:0:0011:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),
            Id("vmidt", Vm("111100:111:00:00011:two:0000000:one:vd"), "%zm", AddrTypeNone,
                InstrTypePsp),

            Id("viim", Vm("110111:11:0:vd:imm16"), "%xs, %vi", AddrTypeNone, InstrTypePsp),

            Id("vmmov", Vm("111100:111:00:00000:two:vs:one:vd"), "%zm, %ym", AddrTypeNone,
                InstrTypePsp),
            Id("vmzero", Vm("111100:111:00:00110:two:0000000:one:vd"), "%zm", AddrTypeNone,
                InstrTypePsp),
            Id("vmone", Vm("111100:111:00:00111:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),

            Id("vnop", Vm("111111:1111111111:00000:00000000000"), "", AddrTypeNone, InstrTypePsp),
            Id("vsync", Vm("111111:1111111111:00000:01100100000"), "", AddrTypeNone, InstrTypePsp),
            Id("vflush", Vm("111111:1111111111:00000:10000001101"), "", AddrTypeNone, InstrTypePsp),

            Id("vpfxd", Vm("110111:10:------------:mskw:mskz:msky:mskx:satw:satz:saty:satx"),
                "[%vp4, %vp5, %vp6, %vp7]", AddrTypeNone, InstrTypePsp),
            Id("vpfxs",
                Vm(
                    "110111:00:----:negw:negz:negy:negx:cstw:cstz:csty:cstx:absw:absz:absy:absx:swzw:swzz:swzy:swzx"),
                "[%vp0, %vp1, %vp2, %vp3]", AddrTypeNone, InstrTypePsp),
            Id("vpfxt",
                Vm(
                    "110111:01:----:negw:negz:negy:negx:cstw:cstz:csty:cstx:absw:absz:absy:absx:swzw:swzz:swzy:swzx"),
                "[%vp0, %vp1, %vp2, %vp3]", AddrTypeNone, InstrTypePsp),

            Id("vdet", Vm("011001:110:vt:two:vs:one:vd"), "%zs, %yp, %xp", AddrTypeNone, InstrTypePsp),

            Id("vrnds", Vm("110100:00:001:00:000:two:vs:one:0000000"), "%ys", AddrTypeNone,
                InstrTypePsp),
            Id("vrndi", Vm("110100:00:001:00:001:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),
            Id("vrndf1", Vm("110100:00:001:00:010:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),
            Id("vrndf2", Vm("110100:00:001:00:011:two:0000000:one:vd"), "%zp", AddrTypeNone,
                InstrTypePsp),

            Id("vcmp", Vm("011011:000:vt:two:vs:one:000:imm4"), "%Zn, %yp, %xp", AddrTypeNone,
                InstrTypePsp),

            Id("vcmovf", Vm("110100:10:101:01:imm3:two:vs:one:vd"), "%zp, %yp, %v3", AddrTypeNone,
                InstrTypePsp),
            Id("vcmovt", Vm("110100:10:101:00:imm3:two:vs:one:vd"), "%zp, %yp, %v3", AddrTypeNone,
                InstrTypePsp),

            Id("vavg", Vm("110100:00:010:00111:two:vs:one:vd"), "%zp, %yp", AddrTypeNone, InstrTypePsp),
            Id("vf2id", Vm("110100:10:011:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),
            Id("vf2in", Vm("110100:10:000:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),
            Id("vf2iu", Vm("110100:10:010:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),
            Id("vf2iz", Vm("110100:10:001:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),
            Id("vi2f", Vm("110100:10:100:imm5:two:vs:one:vd"), "%zp, %yp, %v5", AddrTypeNone,
                InstrTypePsp),

            Id("vscmp", Vm("011011:101:vt:two:vs:one:vd"), "%zp, %yp, %xp", AddrTypeNone, InstrTypePsp),
            Id("vmscl", Vm("111100:100:vt:two:vs:one:vd"), "%zm, %ym, %xs", AddrTypeNone, InstrTypePsp),

            Id("vt4444.q", Vm("110100:00:010:11001:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id("vt5551.q", Vm("110100:00:010:11010:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id("vt5650.q", Vm("110100:00:010:11011:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            Id("vmfvc", Vm("110100:00:010:10000:1:imm7:0:vd"), "%zs, %2s", AddrTypeNone, InstrTypePsp),
            Id("vmtvc", Vm("110100:00:010:10001:0:vs:1:imm7"), "%2d, %ys", AddrTypeNone, InstrTypePsp),

            Id("mfvme", Vm("011010--------------------------"), "%t, %i", AddrTypeNone, 0),
            Id("mtvme", Vm("101100--------------------------"), "%t, %i", AddrTypeNone, 0),

            Id("sv.s", Vm("111010:rs:vt5:imm14:vt2"), "%Xs, %Y", AddrTypeNone, InstrTypePsp),

            Id("vfim", Vm("110111:11:1:vt:imm16"), "%xs, %vh", AddrTypeNone, InstrTypePsp),

            Id("svl.q", Vm("111101:rs:vt5:imm14:0:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),
            Id("svr.q", Vm("111101:rs:vt5:imm14:1:vt1"), "%Xq, %Y", AddrTypeNone, InstrTypePsp),

            Id("vbfy1", Vm("110100:00:010:00010:two:vs:one:vd"), "%zp, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vbfy2", Vm("110100:00:010:00011:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            Id("vf2h", Vm("110100:00:001:10:010:two:vs:one:vd"), "%zs, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vh2f", Vm("110100:00:001:10:011:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),

            Id("vi2s", Vm("110100:00:001:11:111:two:vs:one:vd"), "%zs, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vi2us", Vm("110100:00:001:11:110:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),

            Id("vlgb", Vm("110100:00:001:10:111:two:vs:one:vd"), "%zs, %ys", AddrTypeNone,
                InstrTypePsp),
            Id("vqmul", Vm("111100:101:vt:1:vs:1:vd"), "%zq, %yq, %xq", AddrTypeNone, InstrTypePsp),
            Id("vs2i", Vm("110100:00:001:11:011:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),

            // Working on it.

            //"110100:00:001:11:000:1000000010000001"
            Id("vc2i", Vm("110100:00:001:11:001:two:vs:one:vd"), "%zs, %ys, %xs", AddrTypeNone,
                InstrTypePsp),
            Id("vuc2i", Vm("110100:00:001:11:000:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),


            Id("vsbn", Vm("011000:010:vt:two:vs:one:vd"), "%zs, %ys, %xs", AddrTypeNone, InstrTypePsp),

            Id("vsbz", Vm("110100:00:001:10110:two:vs:one:vd"), "%zs, %ys", AddrTypeNone, InstrTypePsp),
            Id("vsocp", Vm("110100:00:010:00101:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),
            Id("vsrt1", Vm("110100:00:010:00000:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id("vsrt2", Vm("110100:00:010:00001:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id("vsrt4", Vm("110100:00:010:01001:two:vs:one:vd"), "%zq, %yq", AddrTypeNone,
                InstrTypePsp),
            Id("vus2i", Vm("110100:00:001:11010:two:vs:one:vd"), "%zq, %yp", AddrTypeNone,
                InstrTypePsp),

            Id("vwbn", Vm("110100:11:imm8:two:vs:one:vd"), "%zs, %xs, %I", AddrTypeNone, InstrTypePsp),
            //ID("vwb.q",       VM("111110------------------------1-"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
        });

        public static InstructionInfo[] _VFPU_BRANCH;

        public static InstructionInfo[] VFPU_BRANCH => _VFPU_BRANCH ?? (_VFPU_BRANCH = new[]
        {
            Id("bvf", Vm("010010:01:000:imm3:00:imm16"), "%Zc, %O", AddrType16,
                InstrTypePsp | InstrTypeB),
            Id("bvt", Vm("010010:01:000:imm3:01:imm16"), "%Zc, %O", AddrType16,
                InstrTypePsp | InstrTypeB),
            Id("bvfl", Vm("010010:01:000:imm3:10:imm16"), "%Zc, %O", AddrType16,
                InstrTypePsp | InstrTypeB | InstrTypeLikely),
            Id("bvtl", Vm("010010:01:000:imm3:11:imm16"), "%Zc, %O", AddrType16,
                InstrTypePsp | InstrTypeB | InstrTypeLikely),
        });
    }
}
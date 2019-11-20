using System;

namespace CSPspEmu.Core.Cpu.Table
{
    public sealed class InstructionInfo : Attribute
    {
        /// Name of the instruction.
        /// Example: add
        public string Name;

        /// Mask extracted from BinaryEncoding
        public uint Mask;

        /// Value extracted from BinaryEncoding
        public uint Value;

        /// Example: 000000:rs:rt:rd:00000:100000
        private string _binaryEncoding;

        /// Example: %d, %s, %t
        public string AsmEncoding;
        public AddressType AddressType;
        public InstructionType InstructionType;
        private void ParseBinaryEncoding(string encoding)
        {
            Value = Mask = 0;

            foreach (var part in encoding.Split(':'))
            {
                if (part[0] == '-' || part[0] == '0' || part[0] == '1')
                {
                    foreach (var Char in part)
                    {
                        Mask <<= 1;
                        Value <<= 1;
                        switch (Char)
                        {
                            case '0':
                                Mask |= 1;
                                Value |= 0;
                                break;
                            case '1':
                                Mask |= 1;
                                Value |= 1;
                                break;
                            case '-':
                                Mask |= 0;
                                Value |= 0;
                                break;
                        }
                    }
                }
                else
                {
                    var displacement = part switch
                    {
                        "cstw" => 1,
                        "cstz" => 1,
                        "csty" => 1,
                        "cstx" => 1,
                        "absw" => 1,
                        "absz" => 1,
                        "absy" => 1,
                        "absx" => 1,
                        "mskw" => 1,
                        "mskz" => 1,
                        "msky" => 1,
                        "mskx" => 1,
                        "negw" => 1,
                        "negz" => 1,
                        "negy" => 1,
                        "negx" => 1,
                        "one" => 1,
                        "two" => 1,
                        "vt1" => 1,
                        "vt2" => 2,
                        "satw" => 2,
                        "satz" => 2,
                        "saty" => 2,
                        "satx" => 2,
                        "swzw" => 2,
                        "swzz" => 2,
                        "swzy" => 2,
                        "swzx" => 2,
                        "imm3" => 3,
                        "imm4" => 4,
                        "fcond" => 4,
                        "c0dr" => 5,
                        "c0cr" => 5,
                        "c1dr" => 5,
                        "c1cr" => 5,
                        "imm5" => 5,
                        "vt5" => 5,
                        "rs" => 5,
                        "rd" => 5,
                        "rt" => 5,
                        "sa" => 5,
                        "lsb" => 5,
                        "msb" => 5,
                        "fs" => 5,
                        "fd" => 5,
                        "ft" => 5,
                        "vs" => 7,
                        "vt" => 7,
                        "vd" => 7,
                        "imm7" => 7,
                        "imm8" => 8,
                        "imm14" => 14,
                        "imm16" => 16,
                        "imm20" => 20,
                        "imm26" => 26,
                        _ => throw new Exception($"Unknown part '{part}'")
                    };

                    Mask <<= displacement;
                    Value <<= displacement;
                }
            }
        }

        public string BinaryEncoding
        {
            set => ParseBinaryEncoding(_binaryEncoding = value);
            get => _binaryEncoding;
        }

        public override string ToString() => $"InstructionInfo({Name} {AsmEncoding})";
    }
}
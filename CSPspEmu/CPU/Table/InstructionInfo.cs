using System;

namespace CSPspEmu.Core.Cpu.Table
{
    public sealed class InstructionInfo : Attribute
    {
        /// <summary>
        /// Name of the instruction.
        /// Example: add
        /// </summary>
        public string Name;

        /// <summary>
        /// Mask extracted from BinaryEncoding
        /// </summary>
        public uint Mask;

        /// <summary>
        /// Value extracted from BinaryEncoding
        /// </summary>
        public uint Value;

        /// <summary>
        /// Example: 000000:rs:rt:rd:00000:100000
        /// </summary>
        private string _binaryEncoding;

        /// <summary>
        /// Example: %d, %s, %t
        /// </summary>
        public string AsmEncoding;

        /// <summary>
        /// 
        /// </summary>
        public AddressType AddressType;

        /// <summary>
        /// 
        /// </summary>
        public InstructionType InstructionType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encoding"></param>
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
                    int displacement;

                    switch (part)
                    {
                        case "cstw":
                        case "cstz":
                        case "csty":
                        case "cstx":
                        case "absw":
                        case "absz":
                        case "absy":
                        case "absx":
                        case "mskw":
                        case "mskz":
                        case "msky":
                        case "mskx":
                        case "negw":
                        case "negz":
                        case "negy":
                        case "negx":
                        case "one":
                        case "two":
                        case "vt1":
                            displacement = 1;
                            break;
                        case "vt2":
                        case "satw":
                        case "satz":
                        case "saty":
                        case "satx":
                        case "swzw":
                        case "swzz":
                        case "swzy":
                        case "swzx":
                            displacement = 2;
                            break;
                        case "imm3":
                            displacement = 3;
                            break;
                        case "imm4":
                        case "fcond":
                            displacement = 4;
                            break;
                        case "c0dr":
                        case "c0cr":
                        case "c1dr":
                        case "c1cr":
                        case "imm5":
                        case "vt5":
                        case "rs":
                        case "rd":
                        case "rt":
                        case "sa":
                        case "lsb":
                        case "msb":
                        case "fs":
                        case "fd":
                        case "ft":
                            displacement = 5;
                            break;
                        case "vs":
                        case "vt":
                        case "vd":
                        case "imm7":
                            displacement = 7;
                            break;
                        case "imm8":
                            displacement = 8;
                            break;
                        case "imm14":
                            displacement = 14;
                            break;
                        case "imm16":
                            displacement = 16;
                            break;
                        case "imm20":
                            displacement = 20;
                            break;
                        case "imm26":
                            displacement = 26;
                            break;
                        default:
                            throw new Exception("Unknown part '" + part + "'");
                    }

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
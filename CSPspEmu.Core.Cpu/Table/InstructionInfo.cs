using System;

namespace CSPspEmu.Core.Cpu.Table
{
	public enum AddressType
	{
		None = 0,
		T16 = 1,
		T26 = 2,
		Reg = 3,
	}

	[Flags]
	public enum InstructionType
	{
		None    = 0x00,
		B       = (1 << 0),
		Jump    = (1 << 1),
		Jal     = (1 << 2) | (1 << 3),
		Likely  = (1 << 4),
		Psp     = (1 << 8),
		Syscall = (1 << 16),
	}

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
		private string _BinaryEncoding;

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
		/// <param name="Encoding"></param>
		private void ParseBinaryEncoding(string Encoding)
		{
			Value = Mask = 0;

			foreach (var Part in Encoding.Split(':'))
			{
				if (Part[0] == '-' || Part[0] == '0' || Part[0] == '1')
				{
					foreach (var Char in Part)
					{
						Mask <<= 1; Value <<= 1;
						switch (Char)
						{
							case '0': Mask |= 1; Value |= 0; break;
							case '1': Mask |= 1; Value |= 1; break;
							case '-': Mask |= 0; Value |= 0; break;
						}
					}
				}
				else
				{
					int Displacement = 0;

					switch (Part)
					{
						case "cstw": case "cstz": case "csty": case "cstx":
						case "absw": case "absz": case "absy": case "absx":
						case "mskw": case "mskz": case "msky": case "mskx":
						case "negw": case "negz": case "negy": case "negx":
						case "one": case "two":
						case "vt1":
							Displacement = 1;
						break;
						case "vt2":
						case "satw": case "satz": case "saty": case "satx":
						case "swzw": case "swzz": case "swzy": case "swzx":
							Displacement = 2;
						break;
						case "imm3":
							Displacement = 3;
						break;
						case "imm4": case "fcond":
							Displacement = 4;
						break;
						case "c0dr": case "c0cr": case "c1dr": case "c1cr": case "imm5": case "vt5":
						case "rs": case "rd": case "rt": case "sa": case "lsb": case "msb": case "fs": case "fd": case "ft":
							Displacement = 5;
						break;
						case "vs": case "vt": case "vd": case "imm7":
							Displacement = 7;
						break;
						case "imm8" : Displacement = 8 ; break;
						case "imm14": Displacement = 14; break;
						case "imm16": Displacement = 16; break;
						case "imm20": Displacement = 20; break;
						case "imm26": Displacement = 26; break;
						default:
							throw(new Exception("Unknown part '" + Part + "'"));
					}

					Mask <<= Displacement; Value <<= Displacement;
				}
			}
		}

		public string BinaryEncoding
		{
			set
			{
				_BinaryEncoding = value;
				ParseBinaryEncoding(_BinaryEncoding);
			}
			get
			{
				return _BinaryEncoding;
			}
		}
	}
}

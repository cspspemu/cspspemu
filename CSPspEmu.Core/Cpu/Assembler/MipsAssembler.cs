using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Table;
using System.Reflection.Emit;
using System.IO;
using System.Text.RegularExpressions;
using CSharpUtils.Streams;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Cpu.Assembler
{
	public class MipsAssembler
	{
		protected Stream OutputStream;
		protected BinaryWriter BinaryWriter;
		protected Dictionary<String, InstructionInfo> Instructions;

		public MipsAssembler(Stream OutputStream)
		{
			this.Instructions = InstructionTable.ALL.ToDictionary((InstructionInfo) => InstructionInfo.Name);
			this.OutputStream = OutputStream;
			this.BinaryWriter = new BinaryWriter(this.OutputStream);
		}

		static public IEnumerable<String> Tokenize(String Line)
		{
			var Matches = new Regex(@"(-\d+|[%\w]+|\S)", RegexOptions.Compiled).Matches(Line);
			var Ret = new String[Matches.Count];
			for (int n = 0; n < Matches.Count; n++) Ret[n] = Matches[n].Value;
			return Ret;
		}

		static public List<Tuple<String, String>> MatchFormat(String Format, String Line)
		{
			var Matches = new List<Tuple<String, String>>();

			var FormatChunks = new Queue<String>(Tokenize(Format));
			var LineChunks = new Queue<String>(Tokenize(Line));

			while (FormatChunks.Count > 0)
			{
				var CurrentFormat = FormatChunks.Dequeue();
				var CurrentLine = LineChunks.Dequeue();

				switch (CurrentFormat)
				{
					case ",":
						if (CurrentLine != ",") throw (new InvalidDataException());
						break;
					default:
						Matches.Add(new Tuple<String, String>(CurrentFormat, CurrentLine));
						break;
				}
			}

			return Matches;
		}

		static public Dictionary<String, String> MatchFormatDictionary(String Format, String Line)
		{
			var Dictionary = new Dictionary<String, String>();
			foreach (var Pair in MatchFormat(Format, Line))
			{
				Dictionary[Pair.Item1] = Pair.Item2;
			}
			return Dictionary;
		}


		static public int ParseRegisterName(String RegisterName)
		{
			if (RegisterName[0] == 'r')
			{
				return Convert.ToInt32(RegisterName.Substring(1));
			}
			throw(new InvalidDataException());
		}

		public int ParseIntegerConstant(String Value)
		{
			if (Value.Substr(0, 1) == "-") return -ParseIntegerConstant(Value.Substr(1));
			if (Value.Substr(0, 2) == "0x") return Convert.ToInt32(Value.Substr(2), 16);
			if (Value.Substr(0, 2) == "0b") return Convert.ToInt32(Value.Substr(2), 2);
			return Convert.ToInt32(Value, 10);
		}

		public Instruction AssembleInstruction(String Line)
		{
			return AssembleInstructions(Line)[0];
		}

		public Instruction[] AssembleInstructions(String Line)
		{
			Line = Line.Trim();
			if (Line.Length == 0) return new Instruction[] {};
			var LineTokens = Line.Split(new char[] { ' ', '\t' }, 2);
			var InstructionName = LineTokens[0].ToLower();
			InstructionInfo InstructionInfo;
			if (Instructions.TryGetValue(InstructionName, out InstructionInfo))
			{
				var Instruction = new Instruction()
				{
					Value = InstructionInfo.Value & InstructionInfo.Mask,
				};
				var Matches = MatchFormat(InstructionInfo.AsmEncoding, LineTokens[1]);
				foreach (var Match in Matches)
				{
					switch (Match.Item1)
					{
						case "%d": Instruction.RD = ParseRegisterName(Match.Item2); break;
						case "%s": Instruction.RS = ParseRegisterName(Match.Item2); break;
						case "%t": Instruction.RT = ParseRegisterName(Match.Item2); break;
						case "%C": Instruction.CODE = (uint)ParseIntegerConstant(Match.Item2); break;
						case "%i": Instruction.IMM = ParseIntegerConstant(Match.Item2); break;
						case "%I": Instruction.IMMU = (uint)ParseIntegerConstant(Match.Item2); break;
						default: throw (new InvalidDataException("Unknown format '" + Match.Item1 + "'"));
					}
				}
				return new Instruction[] { Instruction };
			}
			else
			{
				switch (InstructionName)
				{
					case "li":
					{
						var Info = MatchFormatDictionary("%d, %i", LineTokens[1]);
						var DestReg = Info["%d"];
						var Value = ParseIntegerConstant(Info["%i"]);
						// Needs LUI
						if ((short)Value != Value)
						{
							var List = new List<Instruction>();
							List.AddRange(AssembleInstructions("lui " + DestReg + ", " + ((Value >> 16) & 0xFFFF)));
							List.AddRange(AssembleInstructions("ori " + DestReg + ", " + DestReg + ", " + (Value & 0xFFFF)));
							//Console.WriteLine(List.ToJson());
							return List.ToArray();
						}
						else
						{
							return AssembleInstructions("addi " + DestReg + ", r0, " + Value);
						}
					}
					default:
						throw (new InvalidOperationException("Unknown instruction type '" + InstructionName + "'"));
				}
			}
		}

		public void Assemble(String Lines)
		{
			foreach (var Line in Lines.Split(new char[] { '\n' }).Select(Str => Str.Trim()).Where(Str => Str.Length > 0))
			{
				var Instructions = AssembleInstructions(Line);
				foreach (var Instruction in Instructions)
				{
					BinaryWriter.Write(Instruction.Value);
				}
			}
		}
	}
}

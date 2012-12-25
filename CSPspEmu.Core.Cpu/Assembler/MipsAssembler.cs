using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.Core.Cpu.VFpu;
using CSPspEmu.Core.Memory;
using CSharpUtils;
using CSharpUtils.Streams;
using CSharpUtils.Arrays;
//using CSPspEmu.Core.Memory;
//using CSPspEmu.Core.Utils;
//using CSharpUtils.Arrays;
//using CSharpUtils.Streams;
//using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Assembler
{
	public class AssemblerResult
	{
		public IArray<Instruction> Instructions;
		public Dictionary<String, uint> Labels = new Dictionary<String, uint>();
		public List<AssemblerPatch> Patches = new List<AssemblerPatch>();
	}

	public enum AssemblerPatchType
	{
		REL_16 = 0,
		ABS_26 = 1,
		ABS_32 = 2,
	}

	public class AssemblerPatch
	{
		public uint Address;
		public AssemblerPatchType Type;
		public String LabelName;
	}

	public unsafe partial class MipsAssembler
	{
		protected Stream OutputStream;
		protected BinaryWriter BinaryWriter;
		protected BinaryReader BinaryReader;
		protected Dictionary<String, InstructionInfo> Instructions;

		public static AssemblerResult StaticAssembleInstructions(string Program)
		{
			var Memory = new MemoryStream();
			var Result = new AssemblerResult()
			{
				Instructions = new StreamStructArrayWrapper<Instruction>(Memory),
			};
			var MipsAssembler = new MipsAssembler(Memory);
			MipsAssembler.Assemble(Program, Result);
			return Result;
		}

		public MipsAssembler(Stream OutputStream)
		{
			this.Instructions = InstructionTable.ALL.ToDictionary((InstructionInfo) => InstructionInfo.Name);
			this.OutputStream = OutputStream;
			this.BinaryWriter = new BinaryWriter(this.OutputStream);
			this.BinaryReader = new BinaryReader(this.OutputStream);
		}

		public static uint ParseVfprRotate(string Format)
		{
			//return 0;
			var Parts = Format.Trim('[', ']').Split(',').Select(Item => Item.Trim()).ToArray();
			uint imm5 = 0;
			int CosIndex = -1;
			int SinIndex = -1;
			bool NegatedSin = false;
			for (int Index = 0; Index < Parts.Length; Index++)
			{
				var Part = Parts[Index];
				switch (Part)
				{
					case "c":
						if (CosIndex != -1) throw (new Exception("Can't put cosine twice"));
						CosIndex = Index;
						break;
					case "-s": 
					case "s":
						if (SinIndex != -1) throw (new Exception("Can't put sine twice"));
						SinIndex = Index;
						if (Part == "-s") NegatedSin = true;
						break;
					case "0":
						break;
					default:
						throw(new NotImplementedException(Part));
				}
			}

			if (CosIndex == -1) throw(new Exception("Didn't set cosine"));
			if (SinIndex == -1) throw (new Exception("Didn't set sine"));

			BitUtils.Insert(ref imm5, 0, 2, (uint)CosIndex);
			BitUtils.Insert(ref imm5, 2, 2, (uint)SinIndex);
			BitUtils.Insert(ref imm5, 4, 1, NegatedSin ? 1U : 0U);
			//Console.WriteLine(Format);
			//throw (new NotImplementedException("ParseVfprRotate"));
			return imm5;
		}

		public static void ParseAndUpdateVfprDestinationPrefix(int Index, string RegisterName, ref VfpuDestinationPrefix VfpuPrefix)
		{
			switch (RegisterName)
			{
				case "m": 
				case "M": VfpuPrefix.DestinationMask(Index, true); break;
				case "0:1": VfpuPrefix.DestinationMask(Index, false); VfpuPrefix.DestinationSaturation(Index, 1); break;
				case "-1:1": VfpuPrefix.DestinationMask(Index, false); VfpuPrefix.DestinationSaturation(Index, 3); break;
				default: throw (new NotImplementedException(String.Format("Invalid RegisterName {0}", RegisterName)));
			}
		}

		public static void ParseAndUpdateVfprSourceTargetPrefix(int Index, string RegisterName, ref VfpuPrefix VfpuPrefix)
		{
			int SetIndex = Index;
			bool IsConstant;

			RegisterName = RegisterName.Replace(" ", "");
			
			if (RegisterName.StartsWith("-"))
			{
				RegisterName = RegisterName.Substr(1);
				VfpuPrefix.SourceNegate(Index, true);
			}

			if (RegisterName.StartsWith("|") && RegisterName.EndsWith("|"))
			{
				RegisterName = RegisterName.Substr(1, -1);
				VfpuPrefix.SourceAbsolute(Index, true);
			}

			switch (RegisterName)
			{
				case "x": IsConstant = false; SetIndex = 0; break;
				case "y": IsConstant = false; SetIndex = 1; break;
				case "z": IsConstant = false; SetIndex = 2; break;
				case "w": IsConstant = false; SetIndex = 3; break;
				case "3": IsConstant = true; SetIndex = 0; VfpuPrefix.SourceAbsolute(Index, true); break;
				case "0": IsConstant = true; SetIndex = 0; VfpuPrefix.SourceAbsolute(Index, false); break;
				case "1/3": IsConstant = true; SetIndex = 1; VfpuPrefix.SourceAbsolute(Index, true); break;
				case "1": IsConstant = true; SetIndex = 1; VfpuPrefix.SourceAbsolute(Index, false); break;
				case "1/4": IsConstant = true; SetIndex = 2; VfpuPrefix.SourceAbsolute(Index, true); break;
				case "2": IsConstant = true; SetIndex = 2; VfpuPrefix.SourceAbsolute(Index, false); break;
				case "1/6": IsConstant = true; SetIndex = 3; VfpuPrefix.SourceAbsolute(Index, true); break;
				case "1/2": IsConstant = true; SetIndex = 3; VfpuPrefix.SourceAbsolute(Index, false); break;
				default: throw(new NotImplementedException(String.Format("Invalid RegisterName {0}", RegisterName)));
			}

			VfpuPrefix.SourceConstant(Index, IsConstant);
			VfpuPrefix.SourceIndex(Index, (uint)SetIndex);
		}

		public static uint ParseVfprConstantName(string RegisterName)
		{
			return (uint)VfpuUtils.VfpuConstantsIndices[RegisterName];
		}

		public class ParseVfprOffsetInfo
		{
			public int Offset;
			public int RS;
		}

		public static ParseVfprOffsetInfo ParseVfprOffset(int VfpuSize, string Str)
		{
			var Parts = Str.Split('+');
			return new ParseVfprOffsetInfo()
			{
				Offset = (Parts.Length > 1) ? ParseIntegerConstant(Parts.First()) : 0,
				RS = ParseGprName(Parts.Last()),
			};
		}

		public static int ParseVfprName(int VfpuSize, string RegisterName)
		{
			return VfpuUtils.VfpuRegisterInfo.Parse(VfpuSize, RegisterName).Index;
		}

		public static int ParseFprName(string RegisterName)
		{
			if (RegisterName[0] == 'f')
			{
				return Convert.ToInt32(RegisterName.Substr(1));
			}
			throw (new InvalidDataException());
		}

		public static int ParseGprName(string RegisterName)
		{
			if (RegisterName[0] == 'r')
			{
				return Convert.ToInt32(RegisterName.Substr(1));
			}
			throw(new InvalidDataException("Invalid Register Name '" + RegisterName + "'"));
		}

		public static int ParseIntegerConstant(String Value)
		{
			return NumberUtils.ParseIntegerConstant(Value);
		}

		public Instruction AssembleInstruction(String Line)
		{
			return AssembleInstructions(Line)[0];
		}

		public Instruction[] AssembleInstructions(String Line)
		{
			uint PC = 0;
			return AssembleInstructions(ref PC, Line, null);
		}

		public Instruction[] AssembleInstructions(ref uint PC, String Line, List<AssemblerPatch> Patches)
		{
			Line = Line.Trim();
			if (Line.Length == 0) return new Instruction[] {};
			string InstructionSuffix = "";
			int VfpuSize = 0;
			var LineTokens = Line.Split(new char[] { ' ', '\t' }, 2);
			var InstructionName = LineTokens[0].ToLower();
			InstructionInfo InstructionInfo;

			if (InstructionName.EndsWith(".s")) VfpuSize = 1;
			if (InstructionName.EndsWith(".p")) VfpuSize = 2;
			if (InstructionName.EndsWith(".t")) VfpuSize = 3;
			if (InstructionName.EndsWith(".q")) VfpuSize = 4;

			if (!Instructions.ContainsKey(InstructionName))
			{
				// Vfpu instruction with suffix.
				if (VfpuSize > 0)
				{
					InstructionSuffix = InstructionName.Substr(-2);
					InstructionName = InstructionName.Substr(0, -2);
				}
			}

			if (Instructions.TryGetValue(InstructionName, out InstructionInfo))
			{
				var Instruction = new Instruction()
				{
					Value = InstructionInfo.Value & InstructionInfo.Mask,
				};

				if (VfpuSize > 0)
				{
					Instruction.ONE_TWO = VfpuSize;
				}

				var Matches = Matcher(InstructionInfo.AsmEncoding, (LineTokens.Length > 1) ? LineTokens[1] : "");
				foreach (var Match in Matches)
				{
					var Key = Match.Key;
					var Value = Match.Value;

					switch (Key)
					{
						// VFPU
						// Vector registers
						case "%zs": 
						case "%zp":
						case "%zt":
						case "%zq":
						case "%zm":
							Instruction.VD = ParseVfprName(VfpuSize, Value);
							break;
						case "%ys": 
						case "%yp":
						case "%yt":
						case "%yq":
						case "%ym":
						case "%yn":
						case "%tym":
							Instruction.VS = ParseVfprName(VfpuSize, Value);
							if (Key == "%tym")
							{
								var Reg = Instruction.VS;
								Reg.M_TRANSPOSED = (Reg.M_TRANSPOSED != 0) ? 0 : 1;
								Instruction.VS = Reg;
							}
							break;
						case "%xs": 
						case "%xp":
						case "%xt":
						case "%xq":
						case "%xm":
							Instruction.VT = ParseVfprName(VfpuSize, Value); break;
						case "%vk": Instruction.IMM5 = ParseVfprConstantName(Value); break;

						case "%vr": Instruction.IMM5 = ParseVfprRotate(Value); break;

						//case "%zm": throw(new NotImplementedException("zm"));

						// sv.q %Xq, %Y
						case "%Xq": Instruction.VT5_1 = ParseVfprName(VfpuSize, Value); break;
						case "%Y":
							{
								var Info = ParseVfprOffset(VfpuSize, Value);
								if ((Info.Offset % 4) != 0) throw(new Exception("Offset must be multiple of 4"));
								Instruction.IMM14 = Info.Offset / 4;
								Instruction.RS = Info.RS;
							}
							break;

						// VFPU: prefixes (source/target)
						case "%vp0":
						case "%vp1":
						case "%vp2":
						case "%vp3":
							{
								int Index = int.Parse(Key.Substr(-1));
								VfpuPrefix VfpuPrefix = Instruction.Value;
								ParseAndUpdateVfprSourceTargetPrefix(Index, Value, ref VfpuPrefix);
								Instruction.Value = VfpuPrefix;
							}
							break;
						// VFPU: prefixes (destination)
						case "%vp4":
						case "%vp5":
						case "%vp6":
						case "%vp7":
							{
								int Index = int.Parse(Key.Substr(-1)) - 4;
								VfpuDestinationPrefix VfpuPrefix = Instruction.Value;
								ParseAndUpdateVfprDestinationPrefix(Index, Value, ref VfpuPrefix);
								Instruction.Value = VfpuPrefix;
							}
							break;

						//case "%xs": Instruction.VD = ParseVfprName(VfpuSize, Value); break;

						// FPU
						case "%S": Instruction.FS = ParseFprName(Value); break;
						case "%D": Instruction.FD = ParseFprName(Value); break;
						case "%T": Instruction.FT = ParseFprName(Value); break;

						// CPU
						case "%J":
						case "%s": Instruction.RS = ParseGprName(Value); break;
						case "%d": Instruction.RD = ParseGprName(Value); break;
						case "%t": Instruction.RT = ParseGprName(Value); break;

						case "%a": Instruction.POS = (uint)ParseIntegerConstant(Value); break;
						case "%ne": Instruction.SIZE_E = (uint)ParseIntegerConstant(Value); break;
						case "%ni": Instruction.SIZE_I = (uint)ParseIntegerConstant(Value); break;

						case "%p": Instruction.RD = (int)ParseIntegerConstant(Value); break;

						case "%c":
						case "%C": Instruction.CODE = (uint)ParseIntegerConstant(Value); break;
						case "%vi":
						case "%i": Instruction.IMM = ParseIntegerConstant(Value); break;
						case "%I": Instruction.IMMU = (uint)ParseIntegerConstant(Value); break;

						case "%j": Patches.Add(new AssemblerPatch() { Address = PC, LabelName = Value, Type = AssemblerPatchType.ABS_26 }); break;
						case "%O": Patches.Add(new AssemblerPatch() { Address = PC, LabelName = Value, Type = AssemblerPatchType.REL_16 }); break;

						default: throw (new InvalidDataException("Unknown format '" + Key + "' <-- (" + InstructionInfo.AsmEncoding + ")"));
					}
				}
				/*
				if ((InstructionInfo.InstructionType & InstructionType.B) != 0)
				{
					//Patches.Add(new Patch() { Address = PC, LabelName =  });
				}
				else if ((InstructionInfo.InstructionType & InstructionType.Jump) != 0)
				{
				}
				*/
				PC += 4;
				return new Instruction[] { Instruction };
			}
			else
			{
				switch (InstructionName)
				{
					case "nop":
					{
						//return AssembleInstructions(ref PC, "sll r0, r0, r0");
						return AssembleInstructions(ref PC, "and r0, r0, r0", Patches);
					}
					case "b":
					{
						var Info = Matcher("%O", LineTokens[1]);
						return AssembleInstructions(ref PC, "beq r0, r0, " + Info["%O"], Patches);
					}
					case "li":
					{
						var Info = Matcher("%d, %i", LineTokens[1]);
						var DestReg = Info["%d"];
						var Value = ParseIntegerConstant(Info["%i"]);
						// Needs LUI
						if ((short)Value != Value)
						{
							var List = new List<Instruction>();
							List.AddRange(AssembleInstructions(ref PC, "lui " + DestReg + ", " + ((Value >> 16) & 0xFFFF), Patches));
							List.AddRange(AssembleInstructions(ref PC, "ori " + DestReg + ", " + DestReg + ", " + (Value & 0xFFFF), Patches));
							//Console.WriteLine(List.ToJson());
							return List.ToArray();
						}
						else
						{
							return AssembleInstructions(ref PC, "addi " + DestReg + ", r0, " + Value, Patches);
						}
					}
					default:
						throw (new InvalidOperationException("Unknown instruction type '" + InstructionName + "'"));
				}
			}
		}

		/* Format codes
		 * %d - Rd
		 * %t - Rt
		 * %s - Rs
		 * %i - 16bit signed immediate
		 * %I - 16bit unsigned immediate (always printed in hex)
		 * %o - 16bit signed offset (rs base)
		 * %O - 16bit signed offset (PC relative)
		 * %j - 26bit absolute offset
		 * %J - Register jump
		 * %a - SA
		 * %0 - Cop0 register
		 * %1 - Cop1 register
		 * %2? - Cop2 register (? is (s, d))
		 * %p - General cop (i.e. numbered) register
		 * %n? - ins/ext size, ? (e, i)
		 * %r - Debug register
		 * %k - Cache function
		 * %D - Fd
		 * %T - Ft
		 * %S - Fs
		 * %x? - Vt (? is (s/scalar, p/pair, t/triple, q/quad, m/matrix pair, n/matrix triple, o/matrix quad)
		 * %y? - Vs
		 * %z? - Vd
		 * %X? - Vo (? is (s, q))
		 * %Y - VFPU offset
		 * %Z? - VFPU condition code/name (? is (c, n))
		 * %v? - VFPU immediate, ? (3, 5, 8, k, i, h, r, p? (? is (0, 1, 2, 3, 4, 5, 6, 7)))
		 * %c - code (for break)
		 * %C - code (for syscall)
		 * %? - Indicates vmmul special exception
		 */

		public void Assemble(String Lines, AssemblerResult AssemblerResult = null)
		{
			if (AssemblerResult == null) AssemblerResult = new AssemblerResult();

			var Labels = AssemblerResult.Labels;
			var Patches = AssemblerResult.Patches;

			foreach (var Line in Lines.Split(new char[] { '\n' }).Select(Str => Str.Trim()).Where(Str => Str.Length > 0))
			{
				// Strip comments.
				var Parts = Line.Split(new string[] { ";", "#" }, 2, StringSplitOptions.None);
				var RealLine = Parts[0].Trim();

				// Directive
				if (Line[0] == '.')
				{
					var LineTokens = Line.Split(new char[] { ' ', '\t' }, 2);
					switch (LineTokens[0])
					{
						case ".code":
							OutputStream.Position = ParseIntegerConstant(LineTokens[1]);
							break;
						default:
							throw (new NotImplementedException("Unsupported directive '" + LineTokens[0] + "'"));
					}
				}
				else
				{
					// Label
					if (RealLine.EndsWith(":"))
					{
						Labels[RealLine.Substr(0, -1).Trim()] = (uint)OutputStream.Position;
					}
					// Instruction
					else
					{
						uint PC = (uint)OutputStream.Position;
						var Instructions = AssembleInstructions(ref PC, RealLine, Patches);
						foreach (var Instruction in Instructions)
						{
							BinaryWriter.Write(Instruction.Value);
						}
					}
				}
			}

			foreach (var Patch in Patches)
			{
				if (Labels.ContainsKey(Patch.LabelName))
				{
					var LabelAddress = Labels[Patch.LabelName];
					Instruction Instruction;

					OutputStream.Position = Patch.Address;
					Instruction = (Instruction)BinaryReader.ReadUInt32();
					{
						switch (Patch.Type)
						{
							case AssemblerPatchType.REL_16:
								Instruction.IMM = ((int)LabelAddress - (int)Patch.Address - 4) / 4;
								break;
							case AssemblerPatchType.ABS_26:
								Console.Write("0x{0:X} : {1}", (LabelAddress & PspMemory.MemoryMask) / 4, Patch.LabelName);
								Instruction.JUMP = (LabelAddress & PspMemory.MemoryMask) / 4;
								break;
							case AssemblerPatchType.ABS_32:
								Instruction.Value = LabelAddress;
								break;
						}
					}
					OutputStream.Position = Patch.Address; BinaryWriter.Write(Instruction.Value);
				}
				else
				{
					throw(new KeyNotFoundException("Can't find label '" + Patch.LabelName + "'"));
				}
			}
		}
	}
}

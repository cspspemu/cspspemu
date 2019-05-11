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

namespace CSPspEmu.Core.Cpu.Assembler
{
    public class AssemblerResult
    {
        public IArray<Instruction> Instructions;
        public Dictionary<string, uint> Labels = new Dictionary<string, uint>();
        public List<AssemblerPatch> Patches = new List<AssemblerPatch>();
    }

    public enum AssemblerPatchType
    {
        Rel16 = 0,
        Abs26 = 1,
        Abs32 = 2,
    }

    public class AssemblerPatch
    {
        public uint Address;
        public AssemblerPatchType Type;
        public string LabelName;
    }

    public partial class MipsAssembler
    {
        protected Stream OutputStream;
        protected BinaryWriter BinaryWriter;
        protected BinaryReader BinaryReader;
        protected Dictionary<string, InstructionInfo> Instructions;

        public static AssemblerResult StaticAssembleInstructions(string program)
        {
            var memory = new MemoryStream();
            var result = new AssemblerResult
            {
                Instructions = new StreamStructArrayWrapper<Instruction>(memory),
            };
            var mipsAssembler = new MipsAssembler(memory);
            mipsAssembler.Assemble(program, result);
            return result;
        }

        public MipsAssembler(Stream outputStream)
        {
            Instructions = InstructionTable.All.ToDictionary(instructionInfo => instructionInfo.Name);
            OutputStream = outputStream;
            BinaryWriter = new BinaryWriter(OutputStream);
            BinaryReader = new BinaryReader(OutputStream);
        }

        public static uint ParseVfprRotate(string format)
        {
            //return 0;
            var parts = format.Trim('[', ']').Split(',').Select(item => item.Trim()).ToArray();
            uint imm5 = 0;
            var cosIndex = -1;
            var sinIndex = -1;
            var negatedSin = false;
            for (var index = 0; index < parts.Length; index++)
            {
                var part = parts[index];
                switch (part)
                {
                    case "c":
                        if (cosIndex != -1) throw new Exception("Can't put cosine twice");
                        cosIndex = index;
                        break;
                    case "-s":
                    case "s":
                        if (sinIndex != -1) throw new Exception("Can't put sine twice");
                        sinIndex = index;
                        if (part == "-s") negatedSin = true;
                        break;
                    case "0":
                        break;
                    default:
                        throw new NotImplementedException(part);
                }
            }

            if (cosIndex == -1) throw(new Exception("Didn't set cosine"));
            if (sinIndex == -1) throw (new Exception("Didn't set sine"));

            BitUtils.Insert(ref imm5, 0, 2, (uint) cosIndex);
            BitUtils.Insert(ref imm5, 2, 2, (uint) sinIndex);
            BitUtils.Insert(ref imm5, 4, 1, negatedSin ? 1U : 0U);
            //Console.WriteLine(Format);
            //throw (new NotImplementedException("ParseVfprRotate"));
            return imm5;
        }

        public static void ParseAndUpdateVfprDestinationPrefix(int index, string registerName,
            ref VfpuDestinationPrefix vfpuPrefix)
        {
            switch (registerName)
            {
                case "m":
                case "M":
                    vfpuPrefix.DestinationMask(index, true);
                    break;
                case "0:1":
                    vfpuPrefix.DestinationMask(index, false);
                    vfpuPrefix.DestinationSaturation(index, 1);
                    break;
                case "-1:1":
                    vfpuPrefix.DestinationMask(index, false);
                    vfpuPrefix.DestinationSaturation(index, 3);
                    break;
                default: throw new NotImplementedException($"Invalid RegisterName {registerName}");
            }
        }

        public static void ParseAndUpdateVfprSourceTargetPrefix(int index, string registerName,
            ref VfpuPrefix vfpuPrefix)
        {
            int setIndex;
            bool isConstant;

            registerName = registerName.Replace(" ", "");

            if (registerName.StartsWith("-"))
            {
                registerName = registerName.Substr(1);
                vfpuPrefix.SourceNegate(index, true);
            }

            if (registerName.StartsWith("|") && registerName.EndsWith("|"))
            {
                registerName = registerName.Substr(1, -1);
                vfpuPrefix.SourceAbsolute(index, true);
            }

            switch (registerName)
            {
                case "x":
                    isConstant = false;
                    setIndex = 0;
                    break;
                case "y":
                    isConstant = false;
                    setIndex = 1;
                    break;
                case "z":
                    isConstant = false;
                    setIndex = 2;
                    break;
                case "w":
                    isConstant = false;
                    setIndex = 3;
                    break;
                case "3":
                    isConstant = true;
                    setIndex = 0;
                    vfpuPrefix.SourceAbsolute(index, true);
                    break;
                case "0":
                    isConstant = true;
                    setIndex = 0;
                    vfpuPrefix.SourceAbsolute(index, false);
                    break;
                case "1/3":
                    isConstant = true;
                    setIndex = 1;
                    vfpuPrefix.SourceAbsolute(index, true);
                    break;
                case "1":
                    isConstant = true;
                    setIndex = 1;
                    vfpuPrefix.SourceAbsolute(index, false);
                    break;
                case "1/4":
                    isConstant = true;
                    setIndex = 2;
                    vfpuPrefix.SourceAbsolute(index, true);
                    break;
                case "2":
                    isConstant = true;
                    setIndex = 2;
                    vfpuPrefix.SourceAbsolute(index, false);
                    break;
                case "1/6":
                    isConstant = true;
                    setIndex = 3;
                    vfpuPrefix.SourceAbsolute(index, true);
                    break;
                case "1/2":
                    isConstant = true;
                    setIndex = 3;
                    vfpuPrefix.SourceAbsolute(index, false);
                    break;
                default: throw new NotImplementedException($"Invalid RegisterName {registerName}");
            }

            vfpuPrefix.SourceConstant(index, isConstant);
            vfpuPrefix.SourceIndex(index, (uint) setIndex);
        }

        public static uint ParseVfprConstantName(string registerName)
        {
            return (uint) VfpuConstants.GetConstantIndexByName(registerName);
        }

        public class ParseVfprOffsetInfo
        {
            public int Offset;
            public int Rs;
        }

        public static ParseVfprOffsetInfo ParseVfprOffset(int vfpuSize, string str)
        {
            var parts = str.Split('+');
            return new ParseVfprOffsetInfo()
            {
                Offset = (parts.Length > 1) ? ParseIntegerConstant(parts.First()) : 0,
                Rs = ParseGprName(parts.Last()),
            };
        }

        public static int ParseVfprName(int vfpuSize, string registerName)
        {
            return VfpuRegisterInfo.Parse(vfpuSize, registerName).RegisterIndex;
        }

        public static int ParseFprName(string registerName)
        {
            if (registerName[0] == 'f')
                return Convert.ToInt32(registerName.Substr(1));
            throw new InvalidDataException();
        }

        public static int ParseGprName(string registerName)
        {
            if (registerName[0] == 'r')
            {
                return Convert.ToInt32(registerName.Substr(1));
            }
            throw new InvalidDataException($"Invalid Register Name \'{registerName}\'");
        }

        public static int ParseIntegerConstant(string value)
        {
            return NumberUtils.ParseIntegerConstant(value);
        }

        public Instruction AssembleInstruction(string line)
        {
            return AssembleInstructions(line)[0];
        }

        public Instruction[] AssembleInstructions(string line)
        {
            uint pc = 0;
            return AssembleInstructions(ref pc, line, null);
        }

        public Instruction[] AssembleInstructions(ref uint pc, string line, List<AssemblerPatch> patches)
        {
            line = line.Trim();
            if (line.Length == 0) return new Instruction[] { };
            int vfpuSize = 0;
            var lineTokens = line.Split(new[] {' ', '\t'}, 2);
            var instructionName = lineTokens[0].ToLower();
            InstructionInfo instructionInfo;

            if (instructionName.EndsWith(".s")) vfpuSize = 1;
            if (instructionName.EndsWith(".p")) vfpuSize = 2;
            if (instructionName.EndsWith(".t")) vfpuSize = 3;
            if (instructionName.EndsWith(".q")) vfpuSize = 4;

            if (!Instructions.ContainsKey(instructionName))
            {
                // Vfpu instruction with suffix.
                if (vfpuSize > 0)
                {
                    instructionName.Substr(-2);
                    instructionName = instructionName.Substr(0, -2);
                }
            }

            if (Instructions.TryGetValue(instructionName, out instructionInfo))
            {
                var instruction = new Instruction()
                {
                    Value = instructionInfo.Value & instructionInfo.Mask,
                };

                if (vfpuSize > 0)
                {
                    instruction.OneTwo = vfpuSize;
                }

                var matches = Matcher(instructionInfo.AsmEncoding, (lineTokens.Length > 1) ? lineTokens[1] : "");
                foreach (var match in matches)
                {
                    var key = match.Key;
                    var value = match.Value;

                    switch (key)
                    {
                        // VFPU
                        // Vector registers
                        case "%zs":
                        case "%zp":
                        case "%zt":
                        case "%zq":
                        case "%zm":
                            instruction.Vd = ParseVfprName(vfpuSize, value);
                            break;
                        case "%ys":
                        case "%yp":
                        case "%yt":
                        case "%yq":
                        case "%ym":
                        case "%yn":
                        case "%tym":
                            if (key == "%tym")
                            {
                                value = ((value[0] == 'M') ? 'E' : 'M') + value.Substring(1);
                            }
                            instruction.Vs = ParseVfprName(vfpuSize, value);
                            break;
                        case "%xs":
                        case "%xp":
                        case "%xt":
                        case "%xq":
                        case "%xm":
                            instruction.Vt = ParseVfprName(vfpuSize, value);
                            break;
                        case "%vk":
                            instruction.Imm5 = ParseVfprConstantName(value);
                            break;

                        case "%vr":
                            instruction.Imm5 = ParseVfprRotate(value);
                            break;

                        //case "%zm": throw(new NotImplementedException("zm"));

                        // sv.q %Xq, %Y
                        case "%Xq":
                            instruction.Vt51 = ParseVfprName(vfpuSize, value);
                            break;
                        case "%Y":
                        {
                            var info = ParseVfprOffset(vfpuSize, value);
                            if ((info.Offset % 4) != 0) throw(new Exception("Offset must be multiple of 4"));
                            instruction.Imm14 = info.Offset / 4;
                            instruction.Rs = info.Rs;
                        }
                            break;

                        // VFPU: prefixes (source/target)
                        case "%vp0":
                        case "%vp1":
                        case "%vp2":
                        case "%vp3":
                        {
                            var index = int.Parse(key.Substr(-1));
                            VfpuPrefix vfpuPrefix = instruction.Value;
                            ParseAndUpdateVfprSourceTargetPrefix(index, value, ref vfpuPrefix);
                            instruction.Value = vfpuPrefix;
                        }
                            break;
                        // VFPU: prefixes (destination)
                        case "%vp4":
                        case "%vp5":
                        case "%vp6":
                        case "%vp7":
                        {
                            var index = int.Parse(key.Substr(-1)) - 4;
                            VfpuDestinationPrefix vfpuPrefix = instruction.Value;
                            ParseAndUpdateVfprDestinationPrefix(index, value, ref vfpuPrefix);
                            instruction.Value = vfpuPrefix;
                        }
                            break;

                        //case "%xs": Instruction.VD = ParseVfprName(VfpuSize, Value); break;

                        // FPU
                        case "%S":
                            instruction.Fs = ParseFprName(value);
                            break;
                        case "%D":
                            instruction.Fd = ParseFprName(value);
                            break;
                        case "%T":
                            instruction.Ft = ParseFprName(value);
                            break;

                        // CPU
                        case "%J":
                        case "%s":
                            instruction.Rs = ParseGprName(value);
                            break;
                        case "%d":
                            instruction.Rd = ParseGprName(value);
                            break;
                        case "%t":
                            instruction.Rt = ParseGprName(value);
                            break;

                        case "%a":
                            instruction.Pos = (uint) ParseIntegerConstant(value);
                            break;
                        case "%ne":
                            instruction.SizeE = (uint) ParseIntegerConstant(value);
                            break;
                        case "%ni":
                            instruction.SizeI = (uint) ParseIntegerConstant(value);
                            break;

                        case "%p":
                            instruction.Rd = ParseIntegerConstant(value);
                            break;

                        case "%c":
                        case "%C":
                            instruction.Code = (uint) ParseIntegerConstant(value);
                            break;
                        case "%vi":
                        case "%i":
                            instruction.Imm = ParseIntegerConstant(value);
                            break;
                        case "%I":
                            instruction.Immu = (uint) ParseIntegerConstant(value);
                            break;

                        case "%j":
                            patches.Add(new AssemblerPatch()
                            {
                                Address = pc,
                                LabelName = value,
                                Type = AssemblerPatchType.Abs26
                            });
                            break;
                        case "%O":
                            patches.Add(new AssemblerPatch()
                            {
                                Address = pc,
                                LabelName = value,
                                Type = AssemblerPatchType.Rel16
                            });
                            break;

                        default:
                            throw new InvalidDataException(
                                $"Unknown format \'{key}\' <-- ({instructionInfo.AsmEncoding})"
                            );
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
                pc += 4;
                return new[] {instruction};
            }
            else
            {
                switch (instructionName)
                {
                    case "nop":
                    {
                        //return AssembleInstructions(ref PC, "sll r0, r0, r0");
                        return AssembleInstructions(ref pc, "and r0, r0, r0", patches);
                    }
                    case "b":
                    {
                        var info = Matcher("%O", lineTokens[1]);
                        return AssembleInstructions(ref pc, $"beq r0, r0, {info["%O"]}", patches);
                    }
                    case "li":
                    {
                        var info = Matcher("%d, %i", lineTokens[1]);
                        var destReg = info["%d"];
                        var value = ParseIntegerConstant(info["%i"]);
                        // Needs LUI
                        if ((short) value != value)
                        {
                            var list = new List<Instruction>();
                            list.AddRange(AssembleInstructions(ref pc,
                                "lui " + destReg + ", " + ((value >> 16) & 0xFFFF), patches));
                            list.AddRange(AssembleInstructions(ref pc,
                                "ori " + destReg + ", " + destReg + ", " + (value & 0xFFFF), patches));
                            //Console.WriteLine(List.ToJson());
                            return list.ToArray();
                        }
                        else
                        {
                            return AssembleInstructions(ref pc, "addi " + destReg + ", r0, " + value, patches);
                        }
                    }
                    default:
                        throw (new InvalidOperationException("Unknown instruction type '" + instructionName + "'"));
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

        public void Assemble(string lines, AssemblerResult assemblerResult = null)
        {
            if (assemblerResult == null) assemblerResult = new AssemblerResult();

            var labels = assemblerResult.Labels;
            var patches = assemblerResult.Patches;

            foreach (var line in lines.Split('\n').Select(str => str.Trim()).Where(str => str.Length > 0))
            {
                // Strip comments.
                var parts = line.Split(new[] {";", "#"}, 2, StringSplitOptions.None);
                var realLine = parts[0].Trim();

                // Directive
                if (line[0] == '.')
                {
                    var lineTokens = line.Split(new[] {' ', '\t'}, 2);
                    switch (lineTokens[0])
                    {
                        case ".code":
                            OutputStream.Position = ParseIntegerConstant(lineTokens[1]);
                            break;
                        default:
                            throw (new NotImplementedException("Unsupported directive '" + lineTokens[0] + "'"));
                    }
                }
                else
                {
                    // Label
                    if (realLine.EndsWith(":"))
                    {
                        labels[realLine.Substr(0, -1).Trim()] = (uint) OutputStream.Position;
                    }
                    // Instruction
                    else
                    {
                        var pc = (uint) OutputStream.Position;
                        var instructions = AssembleInstructions(ref pc, realLine, patches);
                        foreach (var instruction in instructions)
                        {
                            BinaryWriter.Write(instruction.Value);
                        }
                    }
                }
            }

            foreach (var patch in patches)
            {
                if (!labels.ContainsKey(patch.LabelName))
                {
                    throw new KeyNotFoundException($"Can't find label '{patch.LabelName}'");
                }

                var labelAddress = labels[patch.LabelName];

                OutputStream.Position = patch.Address;
                var instruction = (Instruction) BinaryReader.ReadUInt32();
                {
                    switch (patch.Type)
                    {
                        case AssemblerPatchType.Rel16:
                            instruction.Imm = ((int) labelAddress - (int) patch.Address - 4) / 4;
                            break;
                        case AssemblerPatchType.Abs26:
                            instruction.JumpBits = (labelAddress & PspMemory.MemoryMask) / 4;
                            Console.Write("0x{0:X} : {1}", instruction.JumpBits, patch.LabelName);
                            break;
                        case AssemblerPatchType.Abs32:
                            instruction.Value = labelAddress;
                            break;
                    }
                }
                OutputStream.Position = patch.Address;
                BinaryWriter.Write(instruction.Value);
            }
        }
    }
}
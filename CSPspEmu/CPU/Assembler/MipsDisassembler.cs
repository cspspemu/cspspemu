using System;
using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Cpu.Switch;
using CSPspEmu.Core.Cpu.Table;
using CSPspEmu.Core.Memory;
using SafeILGenerator.Ast;

namespace CSPspEmu.Core.Cpu.Assembler
{
    public class MipsDisassembler
    {
        public static readonly MipsDisassembler Methods = new MipsDisassembler();

        public struct Result
        {
            public uint InstructionPc;
            public Instruction Instruction;
            public InstructionInfo InstructionInfo;
            public IPspMemoryInfo MemoryInfo;

            public static string GprIndexToRegisterName(int registerIndex) => $"r{registerIndex}";
            public static string FprIndexToRegisterName(int registerIndex) => $"f{registerIndex}";

            public static readonly Dictionary<string, Func<Result, string>> Opcodes =
                new Dictionary<string, Func<Result, string>>
                {
                    //return (uint)(PC & ~PspMemory.MemoryMask) | (Instruction.JUMP << 2);
                    {
                        "O",
                        result => $"0x{result.Instruction.GetBranchAddress(result.InstructionPc):X8}"
                    },
                    {"J", result => GprIndexToRegisterName(result.Instruction.Rs)},
                    {
                        "j",
                        result => $"0x{result.Instruction.GetJumpAddress(result.MemoryInfo, result.InstructionPc):X8}"
                    },

                    {"s", result => GprIndexToRegisterName(result.Instruction.Rs)},
                    {"d", result => GprIndexToRegisterName(result.Instruction.Rd)},
                    {"t", result => GprIndexToRegisterName(result.Instruction.Rt)},

                    {"S", result => FprIndexToRegisterName(result.Instruction.Fs)},
                    {"D", result => FprIndexToRegisterName(result.Instruction.Fd)},
                    {"T", result => FprIndexToRegisterName(result.Instruction.Ft)},

                    {"C", result => $"{result.Instruction.Code}"},
                    {"a", result => result.Instruction.Pos.ToString()},
                    {"i", result => result.Instruction.Imm.ToString()},
                    {"I", result => $"0x{result.Instruction.Immu:X4}"},
                };

            public string AssemblyLine
            {
                get
                {
                    var parameters = "";
                    var encoding = InstructionInfo.AsmEncoding;
                    for (var n = 0; n < encoding.Length; n++)
                    {
                        var c = encoding[n];
                        if (c == '%')
                        {
                            var found = false;
                            for (var match = 2; match >= 0; match--)
                            {
                                var part = encoding.Substr(n + 1, match);
                                if (!Opcodes.ContainsKey(part)) continue;
                                parameters += Opcodes[part](this);
                                n += part.Length;
                                found = true;
                                break;
                            }
                            if (found) continue;
                            //Parameters += Char;
                        }
                        parameters += c;
                    }

                    return $"{InstructionInfo.Name} {parameters}";
                }
            }

            public override string ToString() => AssemblyLine;
        }

        protected static Func<uint, MipsDisassembler, Result> ProcessCallback;

        protected static InstructionInfo[] InstructionLookup;

        private static AstGenerator ast = AstGenerator.Instance;

        public Result Disassemble(uint pc, Instruction instruction)
        {
            if (ProcessCallback == null)
            {
                var dictionary = new Dictionary<InstructionInfo, int>();

                InstructionLookup = InstructionTable.All.ToArray();
                for (var n = 0; n < InstructionLookup.Length; n++) dictionary[InstructionLookup[n]] = n;

                ProcessCallback = EmitLookupGenerator.GenerateSwitch<Func<uint, MipsDisassembler, Result>>("",
                    InstructionTable.All, instructionInfo => ast.Return(ast.CallStatic(
                        (Func<uint, int, Result>) _InternalHandle,
                        ast.Argument<uint>(0),
                        (instructionInfo != null) ? dictionary[instructionInfo] : -1
                    )));
            }

            var result = ProcessCallback(instruction, this);
            if (result.InstructionInfo == null)
            {
                Console.Error.WriteLine(
                    $"Instruction at 0x{pc:X8} with data 0x{(uint) instruction:X8} didn't generate a value");
                result.InstructionInfo = InstructionTable.Unknown;
            }
            result.InstructionPc = pc;
            return result;
        }

        public static Result _InternalHandle(uint data, int index) => new Result()
        {
            MemoryInfo = DefaultMemoryInfo.Instance,
            Instruction = data,
            InstructionInfo = (index != -1) ? InstructionLookup[index] : null,
        };
    }
}
using CSPspEmu.Core.Cpu.Assembler;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Cpu.Dynarec.Ast
{
    public class AstNodeStmPspInstruction : AstNodeStm
    {
        public MipsDisassembler.Result DisassembledResult;
        public AstNodeStm Statement;

        public AstNodeStmPspInstruction(MipsDisassembler.Result DisassembledResult, AstNodeStm Statement)
        {
            this.DisassembledResult = DisassembledResult;
            this.Statement = Statement;
        }

        public override void TransformNodes(TransformNodesDelegate Transformer)
        {
            Transformer.Ref(ref Statement);
        }

        public override Dictionary<string, string> Info
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"Address", String.Format("0x{0:X8}", DisassembledResult.InstructionPC)},
                    {"Instruction", DisassembledResult.ToString()},
                };
            }
        }
    }
}
using CSPspEmu.Core.Cpu.Assembler;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}

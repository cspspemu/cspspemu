using System;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System.Runtime.CompilerServices;
using System.IO;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe sealed partial class CpuEmitter
	{
		[Inject]
		private CpuProcessor CpuProcessor;
	
		[Inject]
		private PspMemory Memory;

		private MipsMethodEmitter MipsMethodEmitter;
		private IInstructionReader InstructionReader;
		private Instruction Instruction;
		private uint PC;

		public int BranchCount = 0;

		static private AstMipsGenerator ast = AstMipsGenerator.Instance;

		public CpuEmitter(InjectContext InjectContext, MipsMethodEmitter MipsMethodEmitter, IInstructionReader InstructionReader)
		{
			InjectContext.InjectDependencesTo(this);
			this.MipsMethodEmitter = MipsMethodEmitter;
			this.InstructionReader = InstructionReader;
		}

		public Instruction LoadAT(uint PC)
		{
			this.PC = PC;
			return this.Instruction = InstructionReader[PC];
		}

		private int ONE_TWO { get { return Instruction.ONE_TWO; } }

		private int RT { get { return Instruction.RT; } }
		private int RD { get { return Instruction.RD; } }
		private int RS { get { return Instruction.RS; } }
		private int IMM { get { return Instruction.IMM; } }
		private uint IMMU { get { return Instruction.IMMU; } }

		private int FT { get { return Instruction.FT; } }
		private int FD { get { return Instruction.FD; } }
		private int FS { get { return Instruction.FS; } }

		private AstNodeExpr IMM_s() { return ast.Immediate(IMM); }
		private AstNodeExpr IMM_u() { return ast.Immediate((uint)(ushort)IMM); }
		private AstNodeExpr IMM_uex() { return ast.Immediate((uint)IMM); }

		private AstNodeExpr Address_RS_IMM14(int Offset = 0)
		{
			return ast.Cast<uint>(ast.Binary(ast.GPR_s(RS), "+", Instruction.IMM14 * 4 + Offset), false);
		}

		private AstNodeExpr Address_RS_IMM()
		{
			return ast.Cast<uint>(ast.Binary(ast.GPR_s(RS), "+", IMM_s()), false);
		}
	}
}

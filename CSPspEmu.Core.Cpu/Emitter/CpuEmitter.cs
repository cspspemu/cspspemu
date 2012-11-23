using System;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter : IAstGenerator
	{
		public CpuProcessor CpuProcessor;
		private MipsMethodEmitter MipsMethodEmitter;
		private IInstructionReader InstructionReader;
		public Instruction Instruction { private set; get; }
		public uint PC { private set; get; }

		public Instruction LoadAT(uint PC)
		{
			this.PC = PC;
			return this.Instruction = InstructionReader[PC];
		}

		public int RT { get { return Instruction.RT; } }
		public int RD { get { return Instruction.RD; } }
		public int RS { get { return Instruction.RS; } }
		public int IMM { get { return Instruction.IMM; } }
		public uint IMMU { get { return Instruction.IMMU; } }

		public int FT { get { return Instruction.FT; } }
		public int FD { get { return Instruction.FD; } }
		public int FS { get { return Instruction.FS; } }

		public CpuEmitter(MipsMethodEmitter MipsMethodEmitter, IInstructionReader InstructionReader, CpuProcessor CpuProcessor)
		{
			this.MipsMethodEmitter = MipsMethodEmitter;
			this.InstructionReader = InstructionReader;
			this.CpuProcessor = CpuProcessor;
		}

		// AST utilities
		private AstNodeExprArgument GpuThreadStateArgument() { return this.Argument<CpuThreadState>(0, "CpuThreadState"); }
		private AstNodeExprLValue GPR(int Index) { if (Index == 0) throw (new Exception("Can't get reference to GPR0")); return this.FieldAccess(this.GpuThreadStateArgument(), "GPR" + Index); }
		private AstNodeExpr GPR_s(int Index) { if (Index == 0) return this.Immediate((int)0); return this.Cast<int>(GPR(Index)); }
		private AstNodeExpr GPR_u(int Index) { if (Index == 0) return this.Immediate((uint)0); return this.Cast<uint>(GPR(Index)); }
		private AstNodeExpr IMM_s() { return this.Immediate(IMM); }
		private AstNodeExpr IMM_u() { return this.Immediate((uint)(ushort)IMM); }
		private AstNodeExpr IMM_uex() { return this.Immediate((uint)IMM); }
		private AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return this.Assign(GPR(Index), this.Cast<uint>(Expr)); }
		private void GenerateAssignGPR(int Index, AstNodeExpr Expr) { MipsMethodEmitter.GenerateIL(AssignGPR(Index, Expr)); }

	}
}

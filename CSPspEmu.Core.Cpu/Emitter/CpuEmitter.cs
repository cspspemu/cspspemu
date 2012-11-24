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
		private AstNodeExprArgument CpuThreadStateArgument() { return this.Argument<CpuThreadState>(0, "CpuThreadState"); }
		private AstNodeExprLValue REG(string RegName) { return this.FieldAccess(this.CpuThreadStateArgument(), RegName); }
		private AstNodeExprLValue GPR(int Index) { if (Index == 0) throw (new Exception("Can't get reference to GPR0")); return REG("GPR" + Index); }
		private AstNodeExprLValue FPR(int Index) { return REG("FPR" + Index); }
		private AstNodeExprLValue FPR_I(int Index) { return this.Indirect(this.Cast(typeof(int*), this.GetAddress(REG("FPR" + Index)), Explicit: false)); }
		private AstNodeExpr GPR_s(int Index) { if (Index == 0) return this.Immediate((int)0); return this.Cast<int>(GPR(Index)); }
		private AstNodeExpr GPR_sl(int Index) { return this.Cast<long>(GPR_s(Index)); }
		private AstNodeExpr GPR_u(int Index) { if (Index == 0) return this.Immediate((uint)0); return this.Cast<uint>(GPR(Index)); }
		private AstNodeExpr GPR_ul(int Index) { return this.Cast<ulong>(GPR_u(Index)); }
		private AstNodeExpr IMM_s() { return this.Immediate(IMM); }
		private AstNodeExpr IMM_u() { return this.Immediate((uint)(ushort)IMM); }
		private AstNodeExpr IMM_uex() { return this.Immediate((uint)IMM); }
		private AstNodeStm AssignGPR(int Index, AstNodeExpr Expr) { if (Index == 0) return new AstNodeStmEmpty(); return this.Assign(GPR(Index), this.Cast<uint>(Expr)); }

		private void GenerateAssignREG(AstNodeExprLValue Reg, AstNodeExpr Expr) { MipsMethodEmitter.GenerateIL(this.Assign(Reg, Expr)); }
		private void GenerateAssignGPR(int Index, AstNodeExpr Expr) { MipsMethodEmitter.GenerateIL(AssignGPR(Index, Expr)); }
		private void GenerateAssignFPR_F(int Index, AstNodeExpr Expr)
		{
			MipsMethodEmitter.GenerateIL(this.Assign(FPR(Index), Expr));
		}

		private void GenerateAssignFPR_I(int Index, AstNodeExpr Expr)
		{
			MipsMethodEmitter.GenerateIL(this.Assign(FPR_I(Index), Expr));
		}

		private AstNodeExpr HILO_sl() { return this.CallStatic((Func<CpuThreadState, long>)CpuEmitterUtils._get_hi_lo_impl, this.CpuThreadStateArgument()); }
		private AstNodeExpr HILO_ul() { return this.Cast<ulong>(HILO_sl()); }

		private void GenerateAssignHILO(AstNodeExpr Expr) {
			MipsMethodEmitter.GenerateIL(this.Statement(this.CallStatic(
				(Action<CpuThreadState, long>)CpuEmitterUtils._assign_hi_lo_impl,
				this.CpuThreadStateArgument(),
				this.Cast<long>(Expr)
			)));
		}

	}
}

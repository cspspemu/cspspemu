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
		public CpuProcessor CpuProcessor;

		private MipsMethodEmitter MipsMethodEmitter;

		private IInstructionReader InstructionReader;
		public Instruction Instruction { private set; get; }
		public uint PC { private set; get; }
		public PspMemory Memory { get { return CpuProcessor.Memory; } }

		static private AstMipsGenerator ast = AstMipsGenerator.Instance;

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

		public CpuEmitter(InjectContext InjectContext, MipsMethodEmitter MipsMethodEmitter, IInstructionReader InstructionReader)
		{
			InjectContext.InjectDependencesTo(this);
			this.MipsMethodEmitter = MipsMethodEmitter;
			this.InstructionReader = InstructionReader;
		}

		public AstNodeExpr IMM_s() { return ast.Immediate(IMM); }
		public AstNodeExpr IMM_u() { return ast.Immediate((uint)(ushort)IMM); }
		public AstNodeExpr IMM_uex() { return ast.Immediate((uint)IMM); }

		public AstNodeExpr HILO_sl() { return ast.CallStatic((Func<CpuThreadState, long>)CpuEmitterUtils._get_hi_lo_impl, ast.CpuThreadStateArgument()); }
		public AstNodeExpr HILO_ul() { return ast.Cast<ulong>(HILO_sl()); }

		public AstNodeExpr Address_RS_IMM14(int Offset = 0)
		{
			return ast.Cast<uint>(ast.Binary(ast.GPR_s(RS), "+", Instruction.IMM14 * 4 + Offset), false);
		}

		public AstNodeExpr Address_RS_IMM()
		{
			return ast.Cast<uint>(ast.Binary(ast.GPR_s(RS), "+", IMM_s()), false);
		}
	}
}

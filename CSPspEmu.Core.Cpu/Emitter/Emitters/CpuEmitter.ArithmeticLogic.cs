using System;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Arithmetic operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm add() { return ast.AssignGPR(RD, ast.GPR_s(RS) + ast.GPR_s(RT)); }
		public AstNodeStm addu() { return ast.AssignGPR(RD, ast.GPR_u(RS) + ast.GPR_u(RT)); }
		public AstNodeStm sub() { return ast.AssignGPR(RD, ast.GPR_s(RS) - ast.GPR_s(RT)); }
		public AstNodeStm subu() { return ast.AssignGPR(RD, ast.GPR_u(RS) - ast.GPR_u(RT)); }

		public AstNodeStm addi() { return ast.AssignGPR(RT, ast.GPR_s(RS) + IMM_s()); }
		public AstNodeStm addiu() { return ast.AssignGPR(RT, ast.GPR_s(RS) + IMM_s()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Logical Operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm and() { return ast.AssignGPR(RD, ast.GPR_u(RS) & ast.GPR_u(RT)); }
		public AstNodeStm or() { return ast.AssignGPR(RD, ast.GPR_u(RS) | ast.GPR_u(RT)); }
		public AstNodeStm xor() { return ast.AssignGPR(RD, ast.GPR_u(RS) ^ ast.GPR_u(RT)); }
		public AstNodeStm nor() { return ast.AssignGPR(RD, ~(ast.GPR_u(RS) | ast.GPR_u(RT))); }

		public AstNodeStm andi() { return ast.AssignGPR(RT, ast.GPR_u(RS) & IMM_u()); }
		public AstNodeStm ori() { return ast.AssignGPR(RT, ast.GPR_u(RS) | IMM_u()); }
		public AstNodeStm xori() { return ast.AssignGPR(RT, ast.GPR_u(RS) ^ IMM_u()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Shift Left/Right Logical/Arithmethic (Variable).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm sll() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_u(RT), "<<", ast.Immediate((uint)Instruction.POS))); }
		public AstNodeStm sra() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_s(RT), ">>", ast.Immediate((int)Instruction.POS))); }
		public AstNodeStm srl() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_u(RT), ">>", ast.Immediate((uint)Instruction.POS))); }
		public AstNodeStm rotr() { return ast.AssignGPR(RD, ast.CallStatic((Func<uint, int, uint>)CpuEmitterUtils._rotr_impl, ast.GPR_u(RT), ast.Immediate((int)Instruction.POS))); }

		public AstNodeStm sllv() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_u(RT), "<<", ast.GPR_u(RS))); }
		public AstNodeStm srav() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_s(RT), ">>", ast.GPR_s(RS))); }
		public AstNodeStm srlv() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_u(RT), ">>", ast.GPR_u(RS))); }
		public AstNodeStm rotrv() { return ast.AssignGPR(RD, ast.CallStatic((Func<uint, int, uint>)CpuEmitterUtils._rotr_impl, ast.GPR_u(RT), ast.GPR_s(RS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Set Less Than (Immediate) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm slt() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_s(RS), "<", ast.GPR_s(RT))); }
		public AstNodeStm sltu() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_u(RS), "<", ast.GPR_u(RT))); }
		public AstNodeStm slti() { return ast.AssignGPR(RT, ast.Binary(ast.GPR_s(RS), "<", IMM_s())); }
		public AstNodeStm sltiu() { return ast.AssignGPR(RT, ast.Binary(ast.GPR_u(RS), "<", IMM_uex())); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Upper Immediate.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm lui() { return ast.AssignGPR(RT, ast.Binary(IMM_u(), "<<", ast.Immediate((uint)16))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Sign Extend Byte/Half word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm seb() { return ast.AssignGPR(RD, ast.Cast<int>(ast.Cast<sbyte>(ast.GPR_u(RT)))); }
		public AstNodeStm seh() { return ast.AssignGPR(RD, ast.Cast<int>(ast.Cast<short>(ast.GPR_u(RT)))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// BIT REVerse.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm bitrev() { return ast.AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._bitrev_impl, ast.GPR_u(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MAXimum/MINimum.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm max() { return ast.AssignGPR(RD, ast.CallStatic((Func<int, int, int>)CpuEmitterUtils._max_impl, ast.GPR_s(RS), ast.GPR_s(RT))); }
		public AstNodeStm min() { return ast.AssignGPR(RD, ast.CallStatic((Func<int, int, int>)CpuEmitterUtils._min_impl, ast.GPR_s(RS), ast.GPR_s(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// DIVide (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm div() { return (ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._div_impl, ast.CpuThreadStateArgument(), ast.GPR_s(RS), ast.GPR_s(RT)))); }
		public AstNodeStm divu() { return (ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._divu_impl, ast.CpuThreadStateArgument(), ast.GPR_u(RS), ast.GPR_u(RT)))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MULTiply (ADD/SUBstract) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mult() { return ast.AssignHILO(ast.GPR_sl(RS) * ast.GPR_sl(RT)); }
		public AstNodeStm multu() { return ast.AssignHILO(ast.GPR_ul(RS) * ast.GPR_ul(RT)); }
		public AstNodeStm madd() { return ast.AssignHILO(HILO_sl() + ast.GPR_sl(RS) * ast.GPR_sl(RT)); }
		public AstNodeStm maddu() { return ast.AssignHILO(HILO_ul() + ast.GPR_ul(RS) * ast.GPR_ul(RT)); }
		public AstNodeStm msub() { return ast.AssignHILO(HILO_sl() - ast.GPR_sl(RS) * ast.GPR_sl(RT)); }
		public AstNodeStm msubu() { return ast.AssignHILO(HILO_ul() - ast.GPR_ul(RS) * ast.GPR_ul(RT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move To/From HI/LO.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfhi() { return ast.AssignGPR(RD, ast.Cast<uint>(ast.REG("HI"))); }
		public AstNodeStm mflo() { return ast.AssignGPR(RD, ast.Cast<uint>(ast.REG("LO"))); }
		public AstNodeStm mthi() { return ast.AssignREG("HI", ast.GPR_s(RS)); }
		public AstNodeStm mtlo() { return ast.AssignREG("LO", ast.GPR_s(RS)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move if Zero/Non zero.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm movz() { return ast.If(ast.Binary(ast.GPR_s(RT), "==", 0), ast.Assign(ast.GPR(RD), ast.GPR_u(RS))); }
		public AstNodeStm movn() { return ast.If(ast.Binary(ast.GPR_s(RT), "!=", 0), ast.Assign(ast.GPR(RD), ast.GPR_u(RS))); }

		/// <summary>
		/// EXTract/INSert
		/// </summary>
		public AstNodeStm ext() { return ast.AssignGPR(RT, ast.CallStatic((Func<uint, int, int, uint>)CpuEmitterUtils._ext_impl, ast.GPR_u(RS), ast.Immediate((int)Instruction.POS), ast.Immediate((int)Instruction.SIZE_E))); }
		public AstNodeStm ins() { return ast.AssignGPR(RT, ast.CallStatic((Func<uint, uint, int, int, uint>)CpuEmitterUtils._ins_impl, ast.GPR_u(RT), ast.GPR_u(RS), ast.Immediate((int)Instruction.POS), ast.Immediate((int)Instruction.SIZE_I))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Count Leading Ones/Zeros in word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm clz() { return ast.AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._clz_impl, ast.GPR_u(RS))); }
		public AstNodeStm clo() { return ast.AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._clo_impl, ast.GPR_u(RS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Word Swap Bytes Within Halfwords/Words.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm wsbh() { return ast.AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._wsbh_impl, ast.GPR_u(RT))); }
		public AstNodeStm wsbw() { return ast.AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._wsbw_impl, ast.GPR_u(RT))); }
	}
}

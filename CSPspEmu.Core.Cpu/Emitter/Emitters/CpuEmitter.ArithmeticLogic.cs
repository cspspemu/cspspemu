using System;
using System.Runtime.CompilerServices;
using CSharpUtils;
using SafeILGenerator;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Arithmetic operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm add() { return AssignGPR(RD, GPR_s(RS) + GPR_s(RT)); }
		public AstNodeStm addu() { return AssignGPR(RD, GPR_u(RS) + GPR_u(RT)); }
		public AstNodeStm sub() { return AssignGPR(RD, GPR_s(RS) - GPR_s(RT)); }
		public AstNodeStm subu() { return AssignGPR(RD, GPR_u(RS) - GPR_u(RT)); }

		public AstNodeStm addi() { return AssignGPR(RT, GPR_s(RS) + IMM_s()); }
		public AstNodeStm addiu() { return AssignGPR(RT, GPR_s(RS) + IMM_s()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Logical Operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm and() { return AssignGPR(RD, GPR_u(RS) & GPR_u(RT)); }
		public AstNodeStm or() { return AssignGPR(RD, GPR_u(RS) | GPR_u(RT)); }
		public AstNodeStm xor() { return AssignGPR(RD, GPR_u(RS) ^ GPR_u(RT)); }
		public AstNodeStm nor() { return AssignGPR(RD, ~(GPR_u(RS) | GPR_u(RT))); }

		public AstNodeStm andi() { return AssignGPR(RT, GPR_u(RS) & IMM_u()); }
		public AstNodeStm ori() { return AssignGPR(RT, GPR_u(RS) | IMM_u()); }
		public AstNodeStm xori() { return AssignGPR(RT, GPR_u(RS) ^ IMM_u()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Shift Left/Right Logical/Arithmethic (Variable).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm sll() { return AssignGPR(RD, ast.Binary(GPR_u(RT), "<<", ast.Immediate((uint)Instruction.POS))); }
		public AstNodeStm sra() { return AssignGPR(RD, ast.Binary(GPR_s(RT), ">>", ast.Immediate((int)Instruction.POS))); }
		public AstNodeStm srl() { return AssignGPR(RD, ast.Binary(GPR_u(RT), ">>", ast.Immediate((uint)Instruction.POS))); }
		public AstNodeStm rotr() { return AssignGPR(RD, ast.CallStatic((Func<uint, int, uint>)CpuEmitterUtils._rotr_impl, GPR_u(RT), ast.Immediate((int)Instruction.POS))); }

		public AstNodeStm sllv() { return AssignGPR(RD, ast.Binary(GPR_u(RT), "<<", GPR_u(RS))); }
		public AstNodeStm srav() { return AssignGPR(RD, ast.Binary(GPR_s(RT), ">>", GPR_s(RS))); }
		public AstNodeStm srlv() { return AssignGPR(RD, ast.Binary(GPR_u(RT), ">>", GPR_u(RS))); }
		public AstNodeStm rotrv() { return AssignGPR(RD, ast.CallStatic((Func<uint, int, uint>)CpuEmitterUtils._rotr_impl, GPR_u(RT), GPR_s(RS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Set Less Than (Immediate) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm slt() { return AssignGPR(RD, ast.Binary(GPR_s(RS), "<", GPR_s(RT))); }
		public AstNodeStm sltu() { return AssignGPR(RD, ast.Binary(GPR_u(RS), "<", GPR_u(RT))); }
		public AstNodeStm slti() { return AssignGPR(RT, ast.Binary(GPR_s(RS), "<", IMM_s())); }
		public AstNodeStm sltiu() { return AssignGPR(RT, ast.Binary(GPR_u(RS), "<", IMM_uex())); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Upper Immediate.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm lui() { return AssignGPR(RT, ast.Binary(IMM_u(), "<<", ast.Immediate((uint)16))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Sign Extend Byte/Half word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm seb() { return AssignGPR(RD, ast.Cast<int>(ast.Cast<sbyte>(GPR_u(RT)))); }
		public AstNodeStm seh() { return AssignGPR(RD, ast.Cast<int>(ast.Cast<short>(GPR_u(RT)))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// BIT REVerse.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm bitrev() { return AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._bitrev_impl, GPR_u(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MAXimum/MINimum.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm max() { return AssignGPR(RD, ast.CallStatic((Func<int, int, int>)CpuEmitterUtils._max_impl, GPR_s(RS), GPR_s(RT))); }
		public AstNodeStm min() { return AssignGPR(RD, ast.CallStatic((Func<int, int, int>)CpuEmitterUtils._min_impl, GPR_s(RS), GPR_s(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// DIVide (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm div() { return (ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._div_impl, CpuThreadStateArgument(), GPR_s(RS), GPR_s(RT)))); }
		public AstNodeStm divu() { return (ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._divu_impl, CpuThreadStateArgument(), GPR_u(RS), GPR_u(RT)))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MULTiply (ADD/SUBstract) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mult() { return AssignHILO(GPR_sl(RS) * GPR_sl(RT)); }
		public AstNodeStm multu() { return AssignHILO(GPR_ul(RS) * GPR_ul(RT)); }
		public AstNodeStm madd() { return AssignHILO(HILO_sl() + GPR_sl(RS) * GPR_sl(RT)); }
		public AstNodeStm maddu() { return AssignHILO(HILO_ul() + GPR_ul(RS) * GPR_ul(RT)); }
		public AstNodeStm msub() { return AssignHILO(HILO_sl() - GPR_sl(RS) * GPR_sl(RT)); }
		public AstNodeStm msubu() { return AssignHILO(HILO_ul() - GPR_ul(RS) * GPR_ul(RT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move To/From HI/LO.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfhi() { return AssignGPR(RD, ast.Cast<uint>(REG("HI"))); }
		public AstNodeStm mflo() { return AssignGPR(RD, ast.Cast<uint>(REG("LO"))); }
		public AstNodeStm mthi() { return AssignREG("HI", GPR_s(RS)); }
		public AstNodeStm mtlo() { return AssignREG("LO", GPR_s(RS)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move if Zero/Non zero.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm movz() { return ast.IfElse(ast.Binary(GPR_s(RT), "==", 0), ast.Assign(GPR(RD), GPR_u(RS))); }
		public AstNodeStm movn() { return ast.IfElse(ast.Binary(GPR_s(RT), "!=", 0), ast.Assign(GPR(RD), GPR_u(RS))); }

		/// <summary>
		/// EXTract/INSert
		/// </summary>
		public AstNodeStm ext() { return AssignGPR(RT, ast.CallStatic((Func<uint, int, int, uint>)CpuEmitterUtils._ext_impl, GPR_u(RS), ast.Immediate((int)Instruction.POS), ast.Immediate((int)Instruction.SIZE_E))); }
		public AstNodeStm ins() { return AssignGPR(RT, ast.CallStatic((Func<uint, uint, int, int, uint>)CpuEmitterUtils._ins_impl, GPR_u(RT), GPR_u(RS), ast.Immediate((int)Instruction.POS), ast.Immediate((int)Instruction.SIZE_I))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Count Leading Ones/Zeros in word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm clz() { return AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._clz_impl, GPR_u(RS))); }
		public AstNodeStm clo() { return AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._clo_impl, GPR_u(RS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Word Swap Bytes Within Halfwords/Words.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm wsbh() { return AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._wsbh_impl, GPR_u(RT))); }
		public AstNodeStm wsbw() { return AssignGPR(RD, ast.CallStatic((Func<uint, uint>)CpuEmitterUtils._wsbw_impl, GPR_u(RT))); }
	}
}

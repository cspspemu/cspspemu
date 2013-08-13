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

		public AstNodeStm sllv() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_u(RT), "<<", ast.GPR_u(RS) & 31)); }
		public AstNodeStm srav() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_s(RT), ">>", ast.GPR_s(RS) & 31)); }
		public AstNodeStm srlv() { return ast.AssignGPR(RD, ast.Binary(ast.GPR_u(RT), ">>", ast.GPR_u(RS) & 31)); }
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
		public AstNodeStm div() { return (ast.Statement(ast.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._div_impl, ast.CpuThreadState, ast.GPR_s(RS), ast.GPR_s(RT)))); }
		public AstNodeStm divu() { return (ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._divu_impl, ast.CpuThreadState, ast.GPR_u(RS), ast.GPR_u(RT)))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MULTiply (ADD/SUBstract) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mult() { return ast.AssignHILO(ast.GPR_sl(RS) * ast.GPR_sl(RT)); }
		public AstNodeStm multu() { return ast.AssignHILO(ast.GPR_ul(RS) * ast.GPR_ul(RT)); }
		public AstNodeStm madd() { return ast.AssignHILO(ast.HILO_sl() + ast.GPR_sl(RS) * ast.GPR_sl(RT)); }
		public AstNodeStm maddu() { return ast.AssignHILO(ast.HILO_ul() + ast.GPR_ul(RS) * ast.GPR_ul(RT)); }
		public AstNodeStm msub() { return ast.AssignHILO(ast.HILO_sl() - ast.GPR_sl(RS) * ast.GPR_sl(RT)); }
		public AstNodeStm msubu() { return ast.AssignHILO(ast.HILO_ul() - ast.GPR_ul(RS) * ast.GPR_ul(RT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move To/From HI/LO.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfhi() { return ast.AssignGPR(RD, ast.Cast<uint>(ast.HI())); }
		public AstNodeStm mflo() { return ast.AssignGPR(RD, ast.Cast<uint>(ast.LO())); }
		public AstNodeStm mthi() { return ast.AssignHI(ast.GPR_s(RS)); }
		public AstNodeStm mtlo() { return ast.AssignLO(ast.GPR_s(RS)); }

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

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move Control (From/To) Cop0
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm cfc0() { Console.WriteLine("Unimplemented cfc0 : {0}, {1}", RT, RD); return ast.Statement(); }
		public AstNodeStm ctc0() { Console.WriteLine("Unimplemented ctc0 : {0}, {1}", RT, RD); return ast.Statement(); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (From/To) Cop0
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfc0() { return ast.AssignGPR(RT, ast.C0R(RD)); }
		public AstNodeStm mtc0() { return ast.AssignC0R(RD, ast.GPR(RT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Byte/Half word/Word (Left/Right/Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm lb() { return ast.AssignGPR(RT, ast.MemoryGetValue<sbyte>(Memory, Address_RS_IMM())); }
		public AstNodeStm lbu() { return ast.AssignGPR(RT, ast.MemoryGetValue<byte>(Memory, Address_RS_IMM())); }
		public AstNodeStm lh() { return ast.AssignGPR(RT, ast.MemoryGetValue<short>(Memory, Address_RS_IMM())); }
		public AstNodeStm lhu() { return ast.AssignGPR(RT, ast.MemoryGetValue<ushort>(Memory, Address_RS_IMM())); }
		public AstNodeStm lw() { return ast.AssignGPR(RT, ast.MemoryGetValue<int>(Memory, Address_RS_IMM())); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Store Byte/Half word/Word (Left/Right).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm sb() { return ast.MemorySetValue<byte>(Memory, Address_RS_IMM(), ast.GPR_u(RT)); }
		public AstNodeStm sh() { return ast.MemorySetValue<ushort>(Memory, Address_RS_IMM(), ast.GPR_u(RT)); }
		public AstNodeStm sw() { return ast.MemorySetValue<uint>(Memory, Address_RS_IMM(), ast.GPR_u(RT)); }

		public AstNodeStm lwl() { return ast.AssignGPR(RT, ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwl_exec, ast.CpuThreadState, ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT))); }
		public AstNodeStm lwr() { return ast.AssignGPR(RT, ast.CallStatic((Func<CpuThreadState, uint, int, uint, uint>)CpuEmitterUtils._lwr_exec, ast.CpuThreadState, ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT))); }

		public AstNodeStm swl() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swl_exec, ast.CpuThreadState, ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT))); }
		public AstNodeStm swr() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, int, uint>)CpuEmitterUtils._swr_exec, ast.CpuThreadState, ast.GPR_u(RS), IMM_s(), ast.GPR_u(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Linked word.
		// Store Conditional word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm ll() { return lw();  }
		public AstNodeStm sc() { return ast.Statements(sw(), ast.AssignGPR(RT, 1)); }

		public string SpecialName = "";

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Syscall
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm syscall()
		{
			if (Instruction.CODE == SyscallInfo.NativeCallSyscallCode)
			{
				var DelegateId = Memory.Read4(PC + 4);
				var SyscallInfoInfo = CpuProcessor.RegisteredNativeSyscallMethods[DelegateId];
				SpecialName = SyscallInfoInfo.FunctionEntryName;

				var Statements = ast.StatementsInline(
					ast.Assign(ast.PC(), PC),
					ast.Comment(SyscallInfoInfo.Name),
					ast.GetTickCall(true)
				);

				if (_DynarecConfig.FunctionCallWithStaticReferences)
				{
					Statements.AddStatement(ast.Statement(ast.CallDelegate(SyscallInfoInfo.PoolItem.AstFieldAccess, ast.CpuThreadState)));
				}
				else
				{
					Statements.AddStatement(ast.Statement(ast.CallInstance(ast.CpuThreadState, (Action<uint>)CpuThreadState.Methods.SyscallNative, DelegateId)));
				}
				
				Statements.AddStatement(ast.Return());

				return Statements;
			}
			else
			{
				return ast.StatementsInline(
					ast.AssignPC(PC),
					ast.GetTickCall(true),
					ast.Statement(ast.CallInstance(ast.CpuThreadState, (Action<int>)CpuThreadState.Methods.Syscall, (int)Instruction.CODE))
				);
			}
		}
		public AstNodeStm cache() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._cache_impl, ast.CpuThreadState, PC, (uint)Instruction.Value)); }
		public AstNodeStm sync() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._sync_impl, ast.CpuThreadState, PC, (uint)Instruction.Value)); }
		public AstNodeStm _break() { return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._break_impl, ast.CpuThreadState, PC, (uint)Instruction.Value)); }
		public AstNodeStm dbreak() { throw (new NotImplementedException("dbreak")); }
		public AstNodeStm halt() { throw (new NotImplementedException("halt")); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// (D?/Exception) RETurn
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm dret() { throw (new NotImplementedException("dret")); }
		public AstNodeStm eret() { throw (new NotImplementedException("eret")); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (From/To) IC
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfic() { return ast.AssignGPR(RT, ast.IC()); }
		public AstNodeStm mtic() { return ast.AssignIC(ast.GPR_u(RT)); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move (From/To) DR
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm mfdr() { throw (new NotImplementedException("mfdr")); }
		public AstNodeStm mtdr() { throw (new NotImplementedException("mtdr")); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Unknown instruction
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public AstNodeStm unknown()
		{
			Console.Error.WriteLine("UNKNOWN INSTRUCTION: 0x{0:X8} : 0x{1:X8} at 0x{2:X8}", Instruction.Value, Instruction.Value, PC);
			//return _break();
			return ast.Statement();
		}
	}
}

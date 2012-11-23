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
		public void add() { GenerateAssignGPR(RD, GPR_s(RS) + GPR_s(RT)); }
		public void addu() { GenerateAssignGPR(RD, GPR_u(RS) + GPR_u(RT)); }
		public void sub() { GenerateAssignGPR(RD, GPR_s(RS) - GPR_s(RT)); }
		public void subu() { GenerateAssignGPR(RD, GPR_u(RS) - GPR_u(RT)); }

		public void addi() { GenerateAssignGPR(RT, GPR_s(RS) + IMM_s()); }
		public void addiu() { GenerateAssignGPR(RT, GPR_s(RS) + IMM_s()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Logical Operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void and() { GenerateAssignGPR(RD, GPR_u(RS) & GPR_u(RT)); }
		public void or() { GenerateAssignGPR(RD, GPR_u(RS) | GPR_u(RT)); }
		public void xor() { GenerateAssignGPR(RD, GPR_u(RS) ^ GPR_u(RT)); }
		public void nor() { GenerateAssignGPR(RD, ~(GPR_u(RS) | GPR_u(RT))); }

		public void andi() { GenerateAssignGPR(RT, GPR_u(RS) & IMM_u()); }
		public void ori() { GenerateAssignGPR(RT, GPR_u(RS) | IMM_u()); }
		public void xori() { GenerateAssignGPR(RT, GPR_u(RS) ^ IMM_u()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Shift Left/Right Logical/Arithmethic (Variable).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void sll() { GenerateAssignGPR(RD, this.Binary(GPR_u(RT), "<<", this.Immediate((uint)Instruction.POS)));  }
		public void sra() { GenerateAssignGPR(RD, this.Binary(GPR_s(RT), ">>", this.Immediate((int)Instruction.POS))); }
		public void srl() { GenerateAssignGPR(RD, this.Binary(GPR_u(RT), ">>", this.Immediate((uint)Instruction.POS))); }

		public void sllv() { GenerateAssignGPR(RD, this.Binary(GPR_u(RT), "<<", GPR_u(RS))); }
		public void srav() { GenerateAssignGPR(RD, this.Binary(GPR_s(RT), ">>", GPR_s(RS))); }
		public void srlv() { GenerateAssignGPR(RD, this.Binary(GPR_u(RT), ">>", GPR_u(RS))); }
		public void rotr() { GenerateAssignGPR(RD, this.CallStatic((Func<uint, int, uint>)CpuEmitterUtils._rotr_impl, GPR_u(RT), this.Immediate((int)Instruction.POS))); }
		public void rotrv() { GenerateAssignGPR(RD, this.CallStatic((Func<uint, int, uint>)CpuEmitterUtils._rotr_impl, GPR_u(RT), GPR_s(RS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Set Less Than (Immediate) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void slt() { GenerateAssignGPR(RD, this.Binary(GPR_s(RS), "<", GPR_s(RT))); }
		public void sltu() { GenerateAssignGPR(RD, this.Binary(GPR_u(RS), "<", GPR_u(RT))); }
		public void slti() { GenerateAssignGPR(RT, this.Binary(GPR_s(RS), "<", IMM_s())); }
		public void sltiu() { GenerateAssignGPR(RT, this.Binary(GPR_u(RS), "<", IMM_uex())); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Upper Immediate.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void lui() { GenerateAssignGPR(RT, this.Binary(IMM_u(), "<<", this.Immediate((uint)16))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Sign Extend Byte/Half word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void seb() { GenerateAssignGPR(RD, this.Cast<int>(this.Cast<sbyte>(GPR_u(RT)))); }
		public void seh() { GenerateAssignGPR(RD, this.Cast<int>(this.Cast<short>(GPR_u(RT)))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// BIT REVerse.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void bitrev() { GenerateAssignGPR(RD, this.CallStatic((Func<uint, uint>)CpuEmitterUtils._bitrev_impl, GPR_u(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MAXimum/MINimum.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void max() { GenerateAssignGPR(RD, this.CallStatic((Func<int, int, int>)CpuEmitterUtils._max_impl, GPR_s(RS), GPR_s(RT))); }
		public void min() { GenerateAssignGPR(RD, this.CallStatic((Func<int, int, int>)CpuEmitterUtils._min_impl, GPR_s(RS), GPR_s(RT))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// DIVide (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void div() { MipsMethodEmitter.GenerateIL(this.Statement(this.CallStatic((Action<CpuThreadState, int, int>)CpuEmitterUtils._div_impl, this.GpuThreadStateArgument(), GPR_s(RS), GPR_s(RT)))); }
		public void divu() { MipsMethodEmitter.GenerateIL(this.Statement(this.CallStatic((Action<CpuThreadState, uint, uint>)CpuEmitterUtils._divu_impl, this.GpuThreadStateArgument(), GPR_u(RS), GPR_u(RT)))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MULTiply (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void _mult_common_op<TConvertType>(bool Signed)
		{
			MipsMethodEmitter.LoadGPR_Signed(RS);
			//if (!Signed) SafeILGenerator.ConvertTo<uint>();
			SafeILGenerator.ConvertTo<TConvertType>();
			MipsMethodEmitter.LoadGPR_Signed(RT);
			//if (!Signed) SafeILGenerator.ConvertTo<uint>();
			SafeILGenerator.ConvertTo<TConvertType>();
			SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
		}

		public void _mult_common_op<TConvertType>(SafeBinaryOperator Operator)
		{
			MipsMethodEmitter.SaveHI_LO(() =>
			{
				MipsMethodEmitter.LoadHI_LO();
				SafeILGenerator.ConvertTo<TConvertType>();
				_mult_common_op<TConvertType>(Signed: true);
				SafeILGenerator.BinaryOperation(Operator);
			});
		}
		public void _mult_common<TConvertType>(bool Signed)
		{
			MipsMethodEmitter.SaveHI_LO(() =>
			{
				_mult_common_op<TConvertType>(Signed);
			});
		}

		public void mult() { _mult_common<long>(Signed : true); }
		public void multu() {
#if true
			_mult_common<ulong>(Signed: false);
#else
			MipsMethodEmiter.SaveHI_LO(() =>
			{
				MipsMethodEmiter.LoadGPR_Signed(RS);
				MipsMethodEmiter.LoadGPR_Signed(RT);
				SafeILGenerator.Call((Func<uint, uint, ulong>)CpuEmitter._multu);
				//_mult_common_op<TConvertType>(Signed);
			});
#endif
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Multiply ADD/SUBstract (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void madd() { _mult_common_op<long>(SafeBinaryOperator.AdditionSigned); }
		public void maddu() { _mult_common_op<ulong>(SafeBinaryOperator.AdditionSigned); }
		public void msub() { _mult_common_op<long>(SafeBinaryOperator.SubstractionSigned); }
		public void msubu() { _mult_common_op<ulong>(SafeBinaryOperator.SubstractionSigned); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move To/From HI/LO.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void mfhi() { MipsMethodEmitter.SaveGPR(RD, () => { MipsMethodEmitter.LoadHI(); }); }
		public void mflo() { MipsMethodEmitter.SaveGPR(RD, () => { MipsMethodEmitter.LoadLO(); }); }
		public void mthi() { MipsMethodEmitter.SaveHI(() => { MipsMethodEmitter.LoadGPR_Unsigned(RS); }); }
		public void mtlo() { MipsMethodEmitter.SaveLO(() => { MipsMethodEmitter.LoadGPR_Unsigned(RS); }); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move if Zero/Non zero.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void movz() { MipsMethodEmitter.GenerateIL(this.IfElse(this.Binary(this.GPR_s(RT), "==", 0), this.Assign(this.GPR(RD), this.GPR_u(RS)))); }
		public void movn() { MipsMethodEmitter.GenerateIL(this.IfElse(this.Binary(this.GPR_s(RT), "!=", 0), this.Assign(this.GPR(RD), this.GPR_u(RS)))); }

		/// <summary>
		/// EXTract/INSert
		/// </summary>
		public void ext() { GenerateAssignGPR(RT, this.CallStatic((Func<uint, int, int, uint>)CpuEmitterUtils._ext_impl, GPR_u(RS), this.Immediate((int)Instruction.POS), this.Immediate((int)Instruction.SIZE_E))); }
		public void ins()
		{
			// @CHECK!!
			GenerateAssignGPR(RT, this.CallStatic((Func<uint, uint, int, int, uint>)CpuEmitterUtils._ins_impl, GPR_u(RT), GPR_u(RS), this.Immediate((int)Instruction.POS), this.Immediate((int)Instruction.SIZE_E))); 
			//MipsMethodEmitter.SaveGPR(RT, () =>
			//{
			//	MipsMethodEmitter.LoadGPR_Unsigned(RT);
			//	MipsMethodEmitter.LoadGPR_Unsigned(RS);
			//	SafeILGenerator.Push((int)Instruction.POS);
			//	SafeILGenerator.Push((int)Instruction.SIZE_I);
			//	SafeILGenerator.Call((Func<uint, uint, int, int, uint>)CpuEmitter._ins_impl);
			//});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Count Leading Ones/Zeros in word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void clz() { GenerateAssignGPR(RD, this.CallStatic((Func<uint, uint>)CpuEmitterUtils._clz_impl, GPR_u(RS))); }
		public void clo() { GenerateAssignGPR(RD, this.CallStatic((Func<uint, uint>)CpuEmitterUtils._clo_impl, GPR_u(RS))); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Word Swap Bytes Within Halfwords/Words.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void wsbh() { GenerateAssignGPR(RD, this.CallStatic((Func<uint, uint>)CpuEmitterUtils._wsbh_impl, GPR_u(RT))); }
		public void wsbw() { GenerateAssignGPR(RD, this.CallStatic((Func<uint, uint>)CpuEmitterUtils._wsbw_impl, GPR_u(RT))); }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Arithmetic operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void add() { MipsMethodEmiter.OP_3REG_Signed(RD, RS, RT, OpCodes.Add); }
		public void addu() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, OpCodes.Add); }
		public void sub() { MipsMethodEmiter.OP_3REG_Signed(RD, RS, RT, OpCodes.Sub);  }
		public void subu() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, OpCodes.Sub);  }

		public void addi() { MipsMethodEmiter.OP_2REG_IMM_Signed(RT, RS, (short)IMM, OpCodes.Add); }
		public void addiu() { MipsMethodEmiter.OP_2REG_IMM_Signed(RT, RS, (short)IMM, OpCodes.Add);  }


		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Logical Operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void and() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, OpCodes.And); }
		public void or() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, OpCodes.Or); }
		public void xor() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, OpCodes.Xor); }
		public void nor() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, OpCodes.Or, OpCodes.Not); }

		public void andi() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RT, RS, IMMU, OpCodes.And); }
		public void ori() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RT, RS, IMMU, OpCodes.Or); }
		public void xori() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RT, RS, IMMU, OpCodes.Xor); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Shift Left/Right Logical/Arithmethic (Variable).
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void sll() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RD, RT, Instruction.POS, OpCodes.Shl); }
		public void sra() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RD, RT, Instruction.POS, OpCodes.Shr); }
		public void srl() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RD, RT, Instruction.POS, OpCodes.Shr_Un); }

		public void sllv() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RT, RS, OpCodes.Shl); }
		public void srav() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RT, RS, OpCodes.Shr); }
		public void srlv() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RT, RS, OpCodes.Shr_Un); }
		public void rotr() { throw(new NotImplementedException()); }
		public void rotrv() { throw(new NotImplementedException()); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Set Less Than (Immediate) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void slt() { MipsMethodEmiter.OP_3REG_Signed(RD, RS, RT, OpCodes.Clt);  }
		public void sltu() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, OpCodes.Clt_Un);  }

		public void slti() { MipsMethodEmiter.OP_2REG_IMM_Signed(RT, RS, (short)Instruction.IMM, OpCodes.Clt); }
		public void sltiu() {
			//Console.WriteLine("SLTIU: {0} : {1}", Instruction.IMM, (uint)Instruction.IMM);
			MipsMethodEmiter.OP_2REG_IMM_Unsigned(RT, RS, (uint)Instruction.IMM, OpCodes.Clt_Un);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Upper Immediate.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void lui()
		{
			MipsMethodEmiter.SET(RT, IMMU << 16);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Sign Extend Byte/Half word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void seb() {
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Conv_I1);
				//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Conv_I4);
			});
		}
		public void seh() {
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Conv_I2);
				//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Conv_I4);
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// BIT REVerse.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void bitrev() 
		{
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("bitrev_impl"));
			});
			//throw (new NotImplementedException());
		}
		static public uint bitrev_impl(uint v)
		{
			v = ((v >> 1) & 0x55555555) | ((v & 0x55555555) << 1); // swap odd and even bits
			v = ((v >> 2) & 0x33333333) | ((v & 0x33333333) << 2); // swap consecutive pairs
			v = ((v >> 4) & 0x0F0F0F0F) | ((v & 0x0F0F0F0F) << 4); // swap nibbles ... 
			v = ((v >> 8) & 0x00FF00FF) | ((v & 0x00FF00FF) << 8); // swap bytes
			v = (v >> 16) | (v << 16); // swap 2-byte long pairs
			return v;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MAXimum/MINimum.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		private void _max_min(OpCode BranchOpCode)
		{
			var LabelIf = MipsMethodEmiter.ILGenerator.DefineLabel();
			var LabelElse = MipsMethodEmiter.ILGenerator.DefineLabel();
			var LabelEnd = MipsMethodEmiter.ILGenerator.DefineLabel();

			MipsMethodEmiter.LoadGPR_Signed(RS);
			MipsMethodEmiter.LoadGPR_Signed(RT);
			MipsMethodEmiter.ILGenerator.Emit(BranchOpCode, LabelElse);

			// IF
			MipsMethodEmiter.ILGenerator.MarkLabel(LabelIf);
			MipsMethodEmiter.SET_REG(RD, RS);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Br, LabelEnd);

			// ELSE
			MipsMethodEmiter.ILGenerator.MarkLabel(LabelElse);
			MipsMethodEmiter.SET_REG(RD, RT);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Br, LabelEnd);

			// END
			MipsMethodEmiter.ILGenerator.MarkLabel(LabelEnd);
		}

		public void max() { _max_min(OpCodes.Blt); }
		public void min() { _max_min(OpCodes.Bgt); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// DIVide (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		unsafe static public void _div_impl(CpuThreadState CpuThreadState, int Left, int Right)
		{
			CpuThreadState.LO = Left / Right;
			CpuThreadState.HI = Left % Right;
		}

		unsafe static public void _divu_impl(CpuThreadState CpuThreadState, uint Left, uint Right)
		{
			CpuThreadState.LO = (int)(Left / Right);
			CpuThreadState.HI = (int)(Left % Right);
		}

		public void div() {
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			MipsMethodEmiter.LoadGPR_Signed(RS);
			MipsMethodEmiter.LoadGPR_Signed(RT);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_div_impl"));
		}
		public void divu() {
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			MipsMethodEmiter.LoadGPR_Unsigned(RS);
			MipsMethodEmiter.LoadGPR_Unsigned(RT);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_divu_impl"));
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MULTiply (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void _mult_common_op(OpCode ConvOp)
		{
			MipsMethodEmiter.LoadGPR_Signed(RS);
			MipsMethodEmiter.ILGenerator.Emit(ConvOp);
			MipsMethodEmiter.LoadGPR_Signed(RT);
			MipsMethodEmiter.ILGenerator.Emit(ConvOp);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Mul);
		}

		public void _mult_common(OpCode ConvOp)
		{
			MipsMethodEmiter.SaveHI_LO(() =>
			{
				_mult_common_op(ConvOp);
			});
		}

		public void _mult_common_op(OpCode ConvOp, params OpCode[] OpCodes)
		{
			MipsMethodEmiter.SaveHI_LO(() =>
			{
				MipsMethodEmiter.LoadHI_LO();
				MipsMethodEmiter.ILGenerator.Emit(ConvOp);
				_mult_common_op(ConvOp);
				foreach (var OpCode in OpCodes)
				{
					MipsMethodEmiter.ILGenerator.Emit(OpCode);
				}
			});
		}

		public void mult() { _mult_common(OpCodes.Conv_I8); }
		public void multu() { _mult_common(OpCodes.Conv_U8); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Multiply ADD/SUBstract (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void madd() { _mult_common_op(OpCodes.Conv_I8, OpCodes.Add); }
		public void maddu() { _mult_common_op(OpCodes.Conv_U8, OpCodes.Add); }
		public void msub() { _mult_common_op(OpCodes.Conv_I8, OpCodes.Sub); }
		public void msubu() { _mult_common_op(OpCodes.Conv_U8, OpCodes.Sub); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move To/From HI/LO.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void mfhi() { MipsMethodEmiter.SaveGPR(RD, () => { MipsMethodEmiter.LoadHI(); }); }
		public void mflo() { MipsMethodEmiter.SaveGPR(RD, () => { MipsMethodEmiter.LoadLO(); }); }
		public void mthi() { MipsMethodEmiter.SaveHI(() => { MipsMethodEmiter.LoadGPR_Unsigned(RS); }); }
		public void mtlo() { MipsMethodEmiter.SaveLO(() => { MipsMethodEmiter.LoadGPR_Unsigned(RS); }); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move if Zero/Non zero.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		private void _movzn(OpCode OpCode)
		{
			var SkipMoveLabel = MipsMethodEmiter.ILGenerator.DefineLabel();
			MipsMethodEmiter.LoadGPR_Unsigned(RT);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4_0);
			MipsMethodEmiter.ILGenerator.Emit(OpCode, SkipMoveLabel);
			MipsMethodEmiter.SET_REG(RD, RS);
			MipsMethodEmiter.ILGenerator.MarkLabel(SkipMoveLabel);
		}

		public void movz() {
			_movzn(OpCodes.Bne_Un);
		}
		public void movn() {
			_movzn(OpCodes.Beq);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// EXTract/INSert.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		static public uint _ext_impl(uint Data, int Pos, int Size)
		{
			return BitUtils.Extract(Data, Pos, Size);
		}

		static public uint _ins_impl(uint InitialData, uint Data, int Pos, int Size)
		{
			return BitUtils.Insert(InitialData, Pos, Size, Data);
		}

		public void ext()
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, Instruction.POS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, Instruction.SIZE_E);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_ext_impl"));
			});
		}
		public void ins()
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, Instruction.POS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, Instruction.SIZE_I);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_ins_impl"));
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Count Leading Ones/Zeros in word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		static public uint _clo_impl(uint x)
		{
			uint ret = 0;
			while ((x & 0x80000000) != 0) { x <<= 1; ret++; }
			return ret;
		}
		static public uint _clz_impl(uint x)
		{
			return _clo_impl(~x);
		}

		public void clz()
		{
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_clz_impl"));
			});
		}
		public void clo()
		{
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_clo_impl"));
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Word Swap Bytes Within Halfwords/Words.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		uint _wsbh_impl(uint v)
		{
			// swap bytes
			return ((v & 0xFF00FF00) >> 8) | ((v & 0x00FF00FF) << 8);
		}
		uint _wsbw_impl(uint v)
		{
			// BSWAP
			return (
				((v & 0xFF000000) >> 24) |
				((v & 0x00FF0000) >> 8) |
				((v & 0x0000FF00) << 8) |
				((v & 0x000000FF) << 24)
			);
		}

		public void wsbh()
		{
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_wsbh_impl"));
			});
		}
		public void wsbw()
		{
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_wsbw_impl"));
			});
		}
	}
}

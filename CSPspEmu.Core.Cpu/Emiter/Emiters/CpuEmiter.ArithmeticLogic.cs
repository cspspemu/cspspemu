using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using CSharpUtils;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Arithmetic operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void add() { MipsMethodEmiter.OP_3REG_Signed(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); }); }
		public void addu() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); }); }
		public void sub() { MipsMethodEmiter.OP_3REG_Signed(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); }); }
		public void subu() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); }); }

		public void addi() {
			SafeILGenerator.Comment("addi");
			MipsMethodEmiter.OP_2REG_IMM_Signed(RT, RS, (short)IMM, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); });
		}
		public void addiu() {
			SafeILGenerator.Comment("addu");
			MipsMethodEmiter.OP_2REG_IMM_Signed(RT, RS, (short)IMM, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); });
		}


		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Logical Operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void and() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.And); }); }
		public void or() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or); }); }
		public void xor() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Xor); }); }
		public void nor() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or); SafeILGenerator.UnaryOperation(SafeUnaryOperator.Not); }); }

		public void andi() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RT, RS, IMMU, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.And); }); }
		public void ori() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RT, RS, IMMU, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or); }); }
		public void xori() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RT, RS, IMMU, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Xor); }); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Shift Left/Right Logical/Arithmethic (Variable).
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void sll() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RD, RT, Instruction.POS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftLeft); }); }
		public void sra() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RD, RT, Instruction.POS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftRightSigned); }); }
		public void srl() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RD, RT, Instruction.POS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftRightUnsigned); }); }

		static public uint _rotr(uint Value, int Offset)
		{
			return (Value >> Offset) | (Value << (32 - Offset));
		}

		public void sllv() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RT, RS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftLeft); }); }
		public void srav() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RT, RS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftRightSigned); }); }
		public void srlv() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RT, RS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftRightUnsigned); }); }
		public void rotr() {
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				SafeILGenerator.Push((int)Instruction.POS);
				
				MipsMethodEmiter.CallMethod((Func<uint, int, uint>)CpuEmiter._rotr);
			});
			//$rd = ROTR($rt, $ps);
		}
		public void rotrv() {
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.CallMethod((Func<uint, int, uint>)CpuEmiter._rotr);
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Set Less Than (Immediate) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void slt() { MipsMethodEmiter.OP_3REG_Signed(RD, RS, RT, () => { SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanSigned); }); }
		public void sltu() { MipsMethodEmiter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanUnsigned); }); }

		public void slti() { MipsMethodEmiter.OP_2REG_IMM_Signed(RT, RS, (short)Instruction.IMM, () => { SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanSigned); }); }
		public void sltiu() { MipsMethodEmiter.OP_2REG_IMM_Unsigned(RT, RS, (ushort)Instruction.IMM, () => { SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanUnsigned); }); }
		//Console.WriteLine("SLTIU: {0} : {1}", Instruction.IMM, (uint)Instruction.IMM);

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
				SafeILGenerator.ConvertTo<sbyte>();
				SafeILGenerator.ConvertTo<int>();
			});
		}
		public void seh() {
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				SafeILGenerator.ConvertTo<short>();
				SafeILGenerator.ConvertTo<int>();
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
				SafeILGenerator.Call((Func<uint, uint>)CpuEmiter._bitrev_impl);
			});
			//throw (new NotImplementedException());
		}
		static public uint _bitrev_impl(uint v)
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
		private void _max_min(SafeBinaryComparison Comparison)
		{
			var LabelIf = SafeILGenerator.DefineLabel("If");
			var LabelElse = SafeILGenerator.DefineLabel("Else");
			var LabelEnd = SafeILGenerator.DefineLabel("End");

			MipsMethodEmiter.LoadGPR_Signed(RS);
			MipsMethodEmiter.LoadGPR_Signed(RT);
			SafeILGenerator.BranchBinaryComparison(Comparison, LabelElse);

			// IF
			LabelIf.Mark();
			MipsMethodEmiter.SET_REG(RD, RS);
			SafeILGenerator.BranchAlways(LabelEnd);

			// ELSE
			LabelElse.Mark();
			MipsMethodEmiter.SET_REG(RD, RT);
			SafeILGenerator.BranchAlways(LabelEnd);

			// END
			LabelEnd.Mark();
		}

		unsafe static public int _max_impl(int Left, int Right) { return (Left > Right) ? Left : Right; }
		unsafe static public int _min_impl(int Left, int Right) { return (Left < Right) ? Left : Right; }

		public void max()
		{
#if true
			_max_min(SafeBinaryComparison.LessThanSigned);
#else
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				SafeILGenerator.Call((Func<int, int, int>)CpuEmiter._max_impl);
			});
#endif
		}
		public void min() {
#if true
			_max_min(SafeBinaryComparison.GreaterThanSigned);
#else
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				SafeILGenerator.Call((Func<int, int, int>)CpuEmiter._min_impl);
			});
#endif
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// DIVide (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		unsafe static public void _div_impl(CpuThreadState CpuThreadState, int Left, int Right)
		{
			if (Right == 0)
			{
				CpuThreadState.LO = 0;
				CpuThreadState.HI = 0;
			}
			else
			{
				CpuThreadState.LO = Left / Right;
				CpuThreadState.HI = Left % Right;
			}
		}

		unsafe static public void _divu_impl(CpuThreadState CpuThreadState, uint Left, uint Right)
		{
			if (Right == 0)
			{
				CpuThreadState.LO = 0;
				CpuThreadState.HI = 0;
			}
			else
			{
				CpuThreadState.LO = (int)(Left / Right);
				CpuThreadState.HI = (int)(Left % Right);
			}
		}

		public void div() {
			SafeILGenerator.Comment("DIV r" + RS + ", r" + RT);
			SafeILGenerator.LoadArgument0CpuThreadState();
			MipsMethodEmiter.LoadGPR_Signed(RS);
			MipsMethodEmiter.LoadGPR_Signed(RT);
			SafeILGenerator.Call((Action<CpuThreadState, int, int>)CpuEmiter._div_impl);
		}
		public void divu() {
			SafeILGenerator.Comment("DIVU r" + RS + ", r" + RT);
			SafeILGenerator.LoadArgument0CpuThreadState();
			MipsMethodEmiter.LoadGPR_Unsigned(RS);
			MipsMethodEmiter.LoadGPR_Unsigned(RT);
			SafeILGenerator.Call((Action<CpuThreadState, uint, uint>)CpuEmiter._divu_impl);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// MULTiply (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void _mult_common_op<TConvertType>(bool Signed)
		{
			MipsMethodEmiter.LoadGPR_Signed(RS);
			//if (!Signed) SafeILGenerator.ConvertTo<uint>();
			SafeILGenerator.ConvertTo<TConvertType>();
			MipsMethodEmiter.LoadGPR_Signed(RT);
			//if (!Signed) SafeILGenerator.ConvertTo<uint>();
			SafeILGenerator.ConvertTo<TConvertType>();
			SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
		}

		public void _mult_common_op<TConvertType>(SafeBinaryOperator Operator)
		{
			MipsMethodEmiter.SaveHI_LO(() =>
			{
				MipsMethodEmiter.LoadHI_LO();
				SafeILGenerator.ConvertTo<TConvertType>();
				_mult_common_op<TConvertType>(Signed: true);
				SafeILGenerator.BinaryOperation(Operator);
			});
		}
		public void _mult_common<TConvertType>(bool Signed)
		{
			MipsMethodEmiter.SaveHI_LO(() =>
			{
				_mult_common_op<TConvertType>(Signed);
			});
		}

		static public ulong _multu(uint Left, uint Right)
		{
			return (ulong)Left * (ulong)Right;
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
				SafeILGenerator.Call((Func<uint, uint, ulong>)CpuEmiter._multu);
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
		public void mfhi() { MipsMethodEmiter.SaveGPR(RD, () => { MipsMethodEmiter.LoadHI(); }); }
		public void mflo() { MipsMethodEmiter.SaveGPR(RD, () => { MipsMethodEmiter.LoadLO(); }); }
		public void mthi() { MipsMethodEmiter.SaveHI(() => { MipsMethodEmiter.LoadGPR_Unsigned(RS); }); }
		public void mtlo() { MipsMethodEmiter.SaveLO(() => { MipsMethodEmiter.LoadGPR_Unsigned(RS); }); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move if Zero/Non zero.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		private void _movzn(SafeBinaryComparison Comparison)
		{
			var SkipMoveLabel = SafeILGenerator.DefineLabel("SkipMoveLabel");
			MipsMethodEmiter.LoadGPR_Unsigned(RT);
			SafeILGenerator.Push((int)0);
			SafeILGenerator.BranchBinaryComparison(Comparison, SkipMoveLabel);
			MipsMethodEmiter.SET_REG(RD, RS);
			SkipMoveLabel.Mark();
		}

		public void movz() {
			_movzn(SafeBinaryComparison.NotEquals);
		}
		public void movn() {
			_movzn(SafeBinaryComparison.Equals);
		}

		static public uint _ext_impl(uint Data, int Pos, int Size)
		{
			return BitUtils.Extract(Data, Pos, Size);
		}

		static public uint _ins_impl(uint InitialData, uint Data, int Pos, int Size)
		{
			return BitUtils.Insert(InitialData, Pos, Size, Data);
		}

		/// <summary>
		/// EXTract
		/// </summary>
		public void ext()
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Push((int)Instruction.POS);
				SafeILGenerator.Push((int)Instruction.SIZE_E);
				SafeILGenerator.Call((Func<uint, int, int, uint>)CpuEmiter._ext_impl);
			});
		}

		/// <summary>
		/// INSert
		/// </summary>
		public void ins()
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Push((int)Instruction.POS);
				SafeILGenerator.Push((int)Instruction.SIZE_I);
				SafeILGenerator.Call((Func<uint, uint, int, int, uint>)CpuEmiter._ins_impl);
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Count Leading Ones/Zeros in word.
		/////////////////////////////////////////////////////////////////////////////////////////////////

		// http://aggregate.org/MAGIC/
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
				SafeILGenerator.Call((Func<uint, uint>)CpuEmiter._clz_impl);
			});
		}
		public void clo()
		{
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Call((Func<uint, uint>)CpuEmiter._clo_impl);
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Word Swap Bytes Within Halfwords/Words.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		static public uint _wsbh_impl(uint v)
		{
			// swap bytes
			return ((v & 0xFF00FF00) >> 8) | ((v & 0x00FF00FF) << 8);
		}
		static public uint _wsbw_impl(uint v)
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
				SafeILGenerator.Call((Func<uint, uint>)CpuEmiter._wsbh_impl);
			});
		}
		public void wsbw()
		{
			MipsMethodEmiter.SaveGPR(RD, () =>
			{
				MipsMethodEmiter.LoadGPR_Unsigned(RT);
				SafeILGenerator.Call((Func<uint, uint>)CpuEmiter._wsbw_impl);
			});
		}
	}
}

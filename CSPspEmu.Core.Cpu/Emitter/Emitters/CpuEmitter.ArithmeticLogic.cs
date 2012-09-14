using System;
using System.Runtime.CompilerServices;
using CSharpUtils;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Arithmetic operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void add() { MipsMethodEmitter.OP_3REG_Signed(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); }); }
		public void addu() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); }); }
		public void sub() { MipsMethodEmitter.OP_3REG_Signed(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); }); }
		public void subu() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); }); }

		public void addi() {
			SafeILGenerator.Comment("addi");
			MipsMethodEmitter.OP_2REG_IMM_Signed(RT, RS, (short)IMM, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); });
		}
		public void addiu() {
			SafeILGenerator.Comment("addu");
			MipsMethodEmitter.OP_2REG_IMM_Signed(RT, RS, (short)IMM, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); });
		}


		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Logical Operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void and() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.And); }); }
		public void or() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or); }); }
		public void xor() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Xor); }); }
		public void nor() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or); SafeILGenerator.UnaryOperation(SafeUnaryOperator.Not); }); }

		public void andi() { MipsMethodEmitter.OP_2REG_IMM_Unsigned(RT, RS, IMMU, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.And); }); }
		public void ori() { MipsMethodEmitter.OP_2REG_IMM_Unsigned(RT, RS, IMMU, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Or); }); }
		public void xori() { MipsMethodEmitter.OP_2REG_IMM_Unsigned(RT, RS, IMMU, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.Xor); }); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Shift Left/Right Logical/Arithmethic (Variable).
		/////////////////////////////////////////////////////////////////////////////////////////////////

		public void sll() { MipsMethodEmitter.OP_2REG_IMM_Unsigned(RD, RT, Instruction.POS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftLeft); }); }
		public void sra() { MipsMethodEmitter.OP_2REG_IMM_Unsigned(RD, RT, Instruction.POS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftRightSigned); }); }
		public void srl() { MipsMethodEmitter.OP_2REG_IMM_Unsigned(RD, RT, Instruction.POS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftRightUnsigned); }); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _rotr(uint Value, int Offset) { return (Value >> Offset) | (Value << (32 - Offset)); }

		public void sllv() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RT, RS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftLeft); }); }
		public void srav() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RT, RS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftRightSigned); }); }
		public void srlv() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RT, RS, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftRightUnsigned); }); }
		public void rotr() {
			MipsMethodEmitter.SaveGPR(RD, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RT);
				SafeILGenerator.Push((int)Instruction.POS);
				
				MipsMethodEmitter.CallMethod((Func<uint, int, uint>)CpuEmitter._rotr);
			});
			//$rd = ROTR($rt, $ps);
		}
		public void rotrv() {
			MipsMethodEmitter.SaveGPR(RD, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RT);
				MipsMethodEmitter.LoadGPR_Unsigned(RS);
				MipsMethodEmitter.CallMethod((Func<uint, int, uint>)CpuEmitter._rotr);
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Set Less Than (Immediate) (Unsigned).
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void slt() { MipsMethodEmitter.OP_3REG_Signed(RD, RS, RT, () => { SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanSigned); }); }
		public void sltu() { MipsMethodEmitter.OP_3REG_Unsigned(RD, RS, RT, () => { SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanUnsigned); }); }

		public void slti() { MipsMethodEmitter.OP_2REG_IMM_Signed(RT, RS, (short)Instruction.IMM, () => { SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanSigned); }); }

		[PspTested]
		// It is (uint) and it is correct.
		public void sltiu() { MipsMethodEmitter.OP_2REG_IMM_Unsigned(RT, RS, (uint)Instruction.IMM, () => { SafeILGenerator.CompareBinary(SafeBinaryComparison.LessThanUnsigned); }); }
		//Console.WriteLine("SLTIU: {0} : {1}", Instruction.IMM, (uint)Instruction.IMM);

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Load Upper Immediate.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void lui()
		{
			MipsMethodEmitter.SET(RT, IMMU << 16);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Sign Extend Byte/Half word.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void seb() {
			MipsMethodEmitter.SaveGPR(RD, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RT);
				SafeILGenerator.ConvertTo<sbyte>();
				SafeILGenerator.ConvertTo<int>();
			});
		}
		public void seh() {
			MipsMethodEmitter.SaveGPR(RD, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RT);
				SafeILGenerator.ConvertTo<short>();
				SafeILGenerator.ConvertTo<int>();
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// BIT REVerse.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		public void bitrev() 
		{
			MipsMethodEmitter.SaveGPR(RD, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RT);
				SafeILGenerator.Call((Func<uint, uint>)CpuEmitter._bitrev_impl);
			});
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _bitrev_impl(uint v)
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

			MipsMethodEmitter.LoadGPR_Signed(RS);
			MipsMethodEmitter.LoadGPR_Signed(RT);
			SafeILGenerator.BranchBinaryComparison(Comparison, LabelElse);

			// IF
			LabelIf.Mark();
			MipsMethodEmitter.SET_REG(RD, RS);
			SafeILGenerator.BranchAlways(LabelEnd);

			// ELSE
			LabelElse.Mark();
			MipsMethodEmitter.SET_REG(RD, RT);
			SafeILGenerator.BranchAlways(LabelEnd);

			// END
			LabelEnd.Mark();
		}

		public unsafe static int _max_impl(int Left, int Right) { return (Left > Right) ? Left : Right; }
		public unsafe static int _min_impl(int Left, int Right) { return (Left < Right) ? Left : Right; }

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
		public unsafe static void _div_impl(CpuThreadState CpuThreadState, int Left, int Right)
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

		public unsafe static void _divu_impl(CpuThreadState CpuThreadState, uint Left, uint Right)
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
			MipsMethodEmitter.LoadGPR_Signed(RS);
			MipsMethodEmitter.LoadGPR_Signed(RT);
			SafeILGenerator.Call((Action<CpuThreadState, int, int>)CpuEmitter._div_impl);
		}
		public void divu() {
			SafeILGenerator.Comment("DIVU r" + RS + ", r" + RT);
			SafeILGenerator.LoadArgument0CpuThreadState();
			MipsMethodEmitter.LoadGPR_Unsigned(RS);
			MipsMethodEmitter.LoadGPR_Unsigned(RT);
			SafeILGenerator.Call((Action<CpuThreadState, uint, uint>)CpuEmitter._divu_impl);
		}

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong _multu(uint Left, uint Right)
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
		public void mfhi() { MipsMethodEmitter.SaveGPR(RD, () => { MipsMethodEmitter.LoadHI(); }); }
		public void mflo() { MipsMethodEmitter.SaveGPR(RD, () => { MipsMethodEmitter.LoadLO(); }); }
		public void mthi() { MipsMethodEmitter.SaveHI(() => { MipsMethodEmitter.LoadGPR_Unsigned(RS); }); }
		public void mtlo() { MipsMethodEmitter.SaveLO(() => { MipsMethodEmitter.LoadGPR_Unsigned(RS); }); }

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Move if Zero/Non zero.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		private void _movzn(SafeBinaryComparison Comparison)
		{
			var SkipMoveLabel = SafeILGenerator.DefineLabel("SkipMoveLabel");
			MipsMethodEmitter.LoadGPR_Unsigned(RT);
			SafeILGenerator.Push((int)0);
			SafeILGenerator.BranchBinaryComparison(Comparison, SkipMoveLabel);
			MipsMethodEmitter.SET_REG(RD, RS);
			SkipMoveLabel.Mark();
		}

		public void movz() {
			_movzn(SafeBinaryComparison.NotEquals);
		}
		public void movn() {
			_movzn(SafeBinaryComparison.Equals);
		}

		public static uint _ext_impl(uint Data, int Pos, int Size)
		{
			return BitUtils.Extract(Data, Pos, Size);
		}

		public static uint _ins_impl(uint InitialData, uint Data, int Pos, int Size)
		{
			return BitUtils.Insert(InitialData, Pos, Size, Data);
		}

		/// <summary>
		/// EXTract
		/// </summary>
		public void ext()
		{
			MipsMethodEmitter.SaveGPR(RT, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Push((int)Instruction.POS);
				SafeILGenerator.Push((int)Instruction.SIZE_E);
				SafeILGenerator.Call((Func<uint, int, int, uint>)CpuEmitter._ext_impl);
			});
		}

		/// <summary>
		/// INSert
		/// </summary>
		public void ins()
		{
			MipsMethodEmitter.SaveGPR(RT, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RT);
				MipsMethodEmitter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Push((int)Instruction.POS);
				SafeILGenerator.Push((int)Instruction.SIZE_I);
				SafeILGenerator.Call((Func<uint, uint, int, int, uint>)CpuEmitter._ins_impl);
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Count Leading Ones/Zeros in word.
		/////////////////////////////////////////////////////////////////////////////////////////////////

		// http://aggregate.org/MAGIC/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _clo_impl(uint x)
		{
			uint ret = 0;
			while ((x & 0x80000000) != 0) { x <<= 1; ret++; }
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _clz_impl(uint x)
		{
			return _clo_impl(~x);
		}

		public void clz()
		{
			MipsMethodEmitter.SaveGPR(RD, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Call((Func<uint, uint>)CpuEmitter._clz_impl);
			});
		}
		public void clo()
		{
			MipsMethodEmitter.SaveGPR(RD, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RS);
				SafeILGenerator.Call((Func<uint, uint>)CpuEmitter._clo_impl);
			});
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Word Swap Bytes Within Halfwords/Words.
		/////////////////////////////////////////////////////////////////////////////////////////////////
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _wsbh_impl(uint v)
		{
			// swap bytes
			return ((v & 0xFF00FF00) >> 8) | ((v & 0x00FF00FF) << 8);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _wsbw_impl(uint v)
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
			MipsMethodEmitter.SaveGPR(RD, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RT);
				SafeILGenerator.Call((Func<uint, uint>)CpuEmitter._wsbh_impl);
			});
		}
		public void wsbw()
		{
			MipsMethodEmitter.SaveGPR(RD, () =>
			{
				MipsMethodEmitter.LoadGPR_Unsigned(RT);
				SafeILGenerator.Call((Func<uint, uint>)CpuEmitter._wsbw_impl);
			});
		}
	}
}

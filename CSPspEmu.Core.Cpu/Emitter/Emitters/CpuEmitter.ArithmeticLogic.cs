using System;
using System.Runtime.CompilerServices;
using CSharpUtils;
using SafeILGenerator;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter : IAstGenerator
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////
		// Arithmetic operations.
		/////////////////////////////////////////////////////////////////////////////////////////////////

		private AstNodeExprLValue GPR(int Index) {
			if (Index == 0) throw(new Exception("Can't get reference to GPR0"));
			return this.FieldAccess(this.Argument<CpuThreadState>(0, "CpuThreadState"), "GPR" + Index);
		}
		private AstNodeExpr GPR_s(int Index) { if (Index == 0) return this.Immediate((int)0); return this.Cast<int>(GPR(Index)); }
		private AstNodeExpr GPR_u(int Index) { if (Index == 0) return this.Immediate((uint)0); return this.Cast<uint>(GPR(Index)); }

		private AstNodeExpr IMM_s() { return this.Immediate(IMM); }
		private AstNodeExpr IMM_u() { return this.Immediate((uint)(ushort)IMM); }

		private AstNodeStm AssignGPR(int Index, AstNodeExpr Expr)
		{
			if (Index == 0)
			{
				return new AstNodeStmEmpty();
			}
			else
			{
				return this.Assign(GPR(Index), this.Cast<uint>(Expr));
			}
		}

		private void GenerateAssignGPR(int Index, AstNodeExpr Expr)
		{
			MipsMethodEmitter.GenerateIL(AssignGPR(Index, Expr));
		}

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _rotr(uint Value, int Offset) { return (Value >> Offset) | (Value << (32 - Offset)); }

		public void sllv() { GenerateAssignGPR(RD, this.Binary(GPR_u(RT), "<<", GPR_u(RS))); }
		public void srav() { GenerateAssignGPR(RD, this.Binary(GPR_s(RT), ">>", GPR_s(RS))); }
		public void srlv() { GenerateAssignGPR(RD, this.Binary(GPR_u(RT), ">>", GPR_u(RS))); }
		public void rotr() { GenerateAssignGPR(RD, this.CallStatic((Func<uint, int, uint>)CpuEmitter._rotr, GPR_u(RT), this.Immediate((int)Instruction.POS))); }
		public void rotrv() { GenerateAssignGPR(RD, this.CallStatic((Func<uint, int, uint>)CpuEmitter._rotr, GPR_u(RT), GPR_s(RS))); }

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
			GenerateAssignGPR(RT, this.Binary(IMM_u(), "<<", this.Immediate((uint)16)));
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
				SafeILGenerator.Call((Func<int, int, int>)CpuEmitter._max_impl);
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
				SafeILGenerator.Call((Func<int, int, int>)CpuEmitter._min_impl);
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

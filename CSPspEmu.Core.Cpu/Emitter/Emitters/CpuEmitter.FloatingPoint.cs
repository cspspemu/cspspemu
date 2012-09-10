using System;
using CSharpUtils;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		/*
		public static float _mul_s_impl(float a, float b)
		{
			//Console.WriteLine("MUL: {0} * {1} = {2}", a, b, a * b);
			return a * b;
		}

		public static float _div_s_impl(CpuThreadState CpuThreadState, float a, float b)
		{
			//Console.WriteLine("{0}", CpuThreadState.FPR[2]);
			//Console.WriteLine("DIV: {0} / {1} = {2}", a, b, a / b);
			return a / b;
		}
		*/

		// Binary Floating Point Unit Operations
		public void add_s() { MipsMethodEmiter.OP_3REG_F(FD, FS, FT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); }); }
		public void sub_s() { MipsMethodEmiter.OP_3REG_F(FD, FS, FT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.SubstractionSigned); }); }
		public void mul_s() { MipsMethodEmiter.OP_3REG_F(FD, FS, FT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned); }); }
		public void div_s() { MipsMethodEmiter.OP_3REG_F(FD, FS, FT, () => { SafeILGenerator.BinaryOperation(SafeBinaryOperator.DivideSigned); }); }

		// Unary Floating Point Unit Operations
		public void sqrt_s() { MipsMethodEmiter.OP_2REG_F(FD, FS, () => { SafeILGenerator.Call((Func<float, float>)MathFloat.Sqrt); }); }
		public void abs_s() { MipsMethodEmiter.OP_2REG_F(FD, FS, () => { SafeILGenerator.Call((Func<float, float>)MathFloat.Abs); }); }
		public void mov_s() { MipsMethodEmiter.OP_2REG_F(FD, FS, () => { }); }
		public void neg_s() { MipsMethodEmiter.OP_2REG_F(FD, FS, () => { SafeILGenerator.UnaryOperation(SafeUnaryOperator.Negate); }); }
		public void trunc_w_s()
		{
			MipsMethodEmiter.SaveFPR_I(FD, () =>
			{
				MipsMethodEmiter.LoadFPR(FS);
				MipsMethodEmiter.CallMethod((Func<float, int>)MathFloat.Cast);
			});
		}
		public void round_w_s()
		{
			MipsMethodEmiter.SaveFPR_I(FD, () =>
			{
				MipsMethodEmiter.LoadFPR(FS);
				MipsMethodEmiter.CallMethod((Func<float, int>)MathFloat.Round);
			});
		}
		public void ceil_w_s()
		{
			MipsMethodEmiter.SaveFPR_I(FD, () =>
			{
				MipsMethodEmiter.LoadFPR(FS);
				MipsMethodEmiter.CallMethod((Func<float, int>)MathFloat.Ceil);
			});
		}
		public void floor_w_s()
		{
			MipsMethodEmiter.SaveFPR_I(FD, () =>
			{
				MipsMethodEmiter.LoadFPR(FS);
				MipsMethodEmiter.CallMethod((Func<float, int>)MathFloat.Floor);
			});
		}

		/// <summary>
		/// Convert FS register (stored as an int) to float and stores the result on FD.
		/// </summary>
		public void cvt_s_w()
		{
			MipsMethodEmiter.SaveFPR(FD, () =>
			{
				MipsMethodEmiter.LoadFPR_I(FS);
				SafeILGenerator.ConvertTo<float>();
			});
		}

		public static void _cvt_w_s_impl(CpuThreadState CpuThreadState, int FD, int FS)
		{
			//Console.WriteLine("_cvt_w_s_impl: {0}", CpuThreadState.FPR[FS]);
			switch (CpuThreadState.Fcr31.RM)
			{
				case CpuThreadState.FCR31.TypeEnum.Rint: CpuThreadState.FPR_I[FD] = (int)MathFloat.Rint(CpuThreadState.FPR[FS]); break;
				case CpuThreadState.FCR31.TypeEnum.Cast: CpuThreadState.FPR_I[FD] = (int)CpuThreadState.FPR[FS]; break;
				case CpuThreadState.FCR31.TypeEnum.Ceil: CpuThreadState.FPR_I[FD] = (int)MathFloat.Ceil(CpuThreadState.FPR[FS]); break;
				case CpuThreadState.FCR31.TypeEnum.Floor: CpuThreadState.FPR_I[FD] = (int)MathFloat.Floor(CpuThreadState.FPR[FS]); break;
			}
		}

		/// <summary>
		/// Floating-Point Convert to Word Fixed-Point
		/// </summary>
		public void cvt_w_s()
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Push((int)FD);
			SafeILGenerator.Push((int)FS);
			SafeILGenerator.Call((Action<CpuThreadState, int, int>)CpuEmitter._cvt_w_s_impl);
		}

		// Move (from/to) float point registers (reinterpreted)
		public void mfc1() {
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter.LoadFPR(FS);
				SafeILGenerator.Call((Func<float, int>)MathFloat.ReinterpretFloatAsInt);
			});
		}
		public void mtc1() {
			MipsMethodEmiter.SaveFPR(FS, () =>
			{
				MipsMethodEmiter.LoadGPR_Signed(RT);
				SafeILGenerator.Call((Func<int, float>)MathFloat.ReinterpretIntAsFloat);
			});
		}
		// CFC1 -- move Control word from/to floating point (C1)
		public static void _cfc1_impl(CpuThreadState CpuThreadState, int RD, int RT)
		{
			switch (RD)
			{
				case 0: // readonly?
					throw(new NotImplementedException());
				case 31:
					CpuThreadState.GPR[RT] = (int)CpuThreadState.Fcr31.Value;
					break;
				default: throw(new Exception(String.Format("Unsupported CFC1(%d)", RD)));
			}
		}

		public static void _ctc1_impl(CpuThreadState CpuThreadState, int RD, int RT)
		{
			switch (RD)
			{
				case 31:
					CpuThreadState.Fcr31.Value = (uint)CpuThreadState.GPR[RT];
					break;
				default: throw (new Exception(String.Format("Unsupported CFC1(%d)", RD)));
			}
		}

		public void cfc1()
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Push((int)RD);
			SafeILGenerator.Push((int)RT);
			SafeILGenerator.Call((Action<CpuThreadState, int, int>)CpuEmitter._cfc1_impl);
		}
		public void ctc1()
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Push((int)RD);
			SafeILGenerator.Push((int)RT);
			SafeILGenerator.Call((Action<CpuThreadState, int, int>)CpuEmitter._ctc1_impl);
		}

		public static bool _comp_impl(float s, float t, bool fc_unordererd, bool fc_equal, bool fc_less, bool fc_inv_qnan)
		{
			if (float.IsNaN(s) || float.IsNaN(t))
			{
				return fc_unordererd;
			}
			//bool cc = false;
			//if (fc_equal) cc = cc || (s == t);
			//if (fc_less) cc = cc || (s < t);
			//return cc;
			bool equal = (fc_equal) && (s == t);
			bool less = (fc_less) && (s < t);

			return less || equal;
		}

		/// <summary>
		/// Compare (condition) Single_
		/// </summary>
		/// <param name="fc02"></param>
		/// <param name="fc3"></param>
		private void _comp(int fc02, int fc3)
		{
			bool fc_unordererd = ((fc02 & 1) != 0);
			bool fc_equal = ((fc02 & 2) != 0);
			bool fc_less = ((fc02 & 4) != 0);
			bool fc_inv_qnan = (fc3 != 0); // TODO -- Only used for detecting invalid operations?

			MipsMethodEmiter.SaveFCR31_CC(() =>
			{
				MipsMethodEmiter.LoadFPR(FS);
				MipsMethodEmiter.LoadFPR(FT);
				SafeILGenerator.Push((int)(fc_unordererd ? 1 : 0));
				SafeILGenerator.Push((int)(fc_equal ? 1 : 0));
				SafeILGenerator.Push((int)(fc_less ? 1 : 0));
				SafeILGenerator.Push((int)(fc_inv_qnan ? 1 : 0));
				SafeILGenerator.Call((Func<float, float, bool, bool, bool, bool, bool>)CpuEmitter._comp_impl);
			});
		}

		/// <summary>
		/// Compare False Single
		/// This predicate is always False it never has a True Result
		/// </summary>
		public void c_f_s() { _comp(0, 0); }

		/// <summary>
		/// Compare UNordered Single
		/// </summary>
		public void c_un_s() { _comp(1, 0); }

		/// <summary>
		/// Compare EQual Single
		/// </summary>
		public void c_eq_s() { _comp(2, 0); }

		/// <summary>
		/// Compare Unordered or EQual Single
		/// </summary>
		public void c_ueq_s() { _comp(3, 0); }

		/// <summary>
		/// Compare Ordered or Less Than Single
		/// </summary>
		public void c_olt_s() { _comp(4, 0); }

		/// <summary>
		/// Compare Unordered or Less Than Single
		/// </summary>
		public void c_ult_s() { _comp(5, 0); }

		/// <summary>
		/// Compare Ordered or Less than or Equal Single
		/// </summary>
		public void c_ole_s() { _comp(6, 0); }

		/// <summary>
		/// Compare Unordered or Less than or Equal Single
		/// </summary>
		public void c_ule_s() { _comp(7, 0); }
		
		/// <summary>
		/// Compare Signaling False Single
		/// This predicate always False
		/// </summary>
		public void c_sf_s() { _comp(0, 1); }

		/// <summary>
		/// Compare Non Greater Than or Less than or Equal Single
		/// </summary>
		public void c_ngle_s() { _comp(1, 1); }

		/// <summary>
		/// Compare Signaling Equal Single
		/// </summary>
		public void c_seq_s() { _comp(2, 1); }

		/// <summary>
		/// Compare Not Greater than or Less than Single
		/// </summary>
		public void c_ngl_s() { _comp(3, 1); }

		/// <summary>
		/// Compare Less Than Single
		/// </summary>
		public void c_lt_s() { _comp(4, 1); }

		/// <summary>
		/// Compare Not Greater than or Equal Single
		/// </summary>
		public void c_nge_s() { _comp(5, 1); }

		/// <summary>
		/// Compare Less than or Equal Single
		/// </summary>
		public void c_le_s() { _comp(6, 1); }

		/// <summary>
		/// Compare Not Greater Than Single
		/// </summary>
		public void c_ngt_s() { _comp(7, 1); }
	}
}

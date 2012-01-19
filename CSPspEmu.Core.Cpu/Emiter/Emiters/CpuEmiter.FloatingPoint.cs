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
		/*
		static public float _mul_s_impl(float a, float b)
		{
			//Console.WriteLine("MUL: {0} * {1} = {2}", a, b, a * b);
			return a * b;
		}

		static public float _div_s_impl(CpuThreadState CpuThreadState, float a, float b)
		{
			//Console.WriteLine("{0}", CpuThreadState.FPR[2]);
			//Console.WriteLine("DIV: {0} / {1} = {2}", a, b, a / b);
			return a / b;
		}
		*/

		// Binary Floating Point Unit Operations
		public void add_s() { MipsMethodEmiter.OP_3REG_F(FD, FS, FT, OpCodes.Add); }
		public void sub_s() { MipsMethodEmiter.OP_3REG_F(FD, FS, FT, OpCodes.Sub); }
		public void mul_s() { MipsMethodEmiter.OP_3REG_F(FD, FS, FT, OpCodes.Mul); }
		public void div_s() { MipsMethodEmiter.OP_3REG_F(FD, FS, FT, OpCodes.Div); }

		// Unary Floating Point Unit Operations
		public void sqrt_s() {
			MipsMethodEmiter.OP_2REG_F(FD, FS, () => {
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("sqrt_s_impl"));
			});
		}
		static public float sqrt_s_impl(float v)
		{
			return (float)Math.Sqrt((float)v);
		}

		public void abs_s() {
			MipsMethodEmiter.OP_2REG_F(FD, FS, () =>
			{
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("abs_s_impl"));
			});
		}
		static public float abs_s_impl(float v)
		{
			return (v >= 0) ? v : -v;
		}

		public void mov_s() { MipsMethodEmiter.OP_2REG_F(FD, FS, () => { }); }
		public void neg_s() { MipsMethodEmiter.OP_2REG_F(FD, FS, () => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Neg); }); }
		public void trunc_w_s()
		{
			floor_w_s();
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

		static public void _cvt_s_w_impl()
		{
		}

		/// <summary>
		/// Convert FS register (stored as an int) to float and stores the result on FD.
		/// </summary>
		public void cvt_s_w()
		{
			MipsMethodEmiter.SaveFPR(FD, () =>
			{
				MipsMethodEmiter.LoadFPR(FS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(MathFloat).GetMethod("ReinterpretFloatAsInt"));
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Conv_R4);
			});
		}

		static public void _cvt_w_s_impl(CpuThreadState CpuThreadState, int FD, int FS)
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

		public void cvt_w_s()
		{
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, FD);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, FS);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_cvt_w_s_impl"));
			//throw(new NotImplementedException());
		}

		// Move (from/to) float point registers (reinterpreted)
		public void mfc1() {
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter.LoadFPR(FS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(MathFloat).GetMethod("ReinterpretFloatAsInt"));
			});
		}
		public void mtc1() {
			MipsMethodEmiter.SaveFPR(FS, () =>
			{
				MipsMethodEmiter.LoadGPR_Signed(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(MathFloat).GetMethod("ReinterpretIntAsFloat"));
			});
		}
		// CFC1 -- move Control word from/to floating point (C1)
		static public void _cfc1_impl(CpuThreadState CpuThreadState, int RD, int RT)
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

		static public void _ctc1_impl(CpuThreadState CpuThreadState, int RD, int RT)
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
			//throw(new NotImplementedException());
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, RD);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, RT);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_cfc1_impl"));
		}
		public void ctc1()
		{
			//throw (new NotImplementedException());
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, RD);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, RT);
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_ctc1_impl"));
		}

		static public void _comp_impl(CpuThreadState CpuThreadState, float s, float t, bool fc_unordererd, bool fc_equal, bool fc_less, bool fc_inv_qnan)
		{
			if (float.IsNaN(s) || float.IsNaN(t))
			{
				CpuThreadState.Fcr31.CC = fc_unordererd;
			}
			else
			{
				bool cc = false;
				if (fc_equal) cc = cc || (s == t);
				if (fc_less) cc = cc || (s < t);
				CpuThreadState.Fcr31.CC = cc;
			}
		}

		private void _comp(int fc02, int fc3)
		{
			bool fc_unordererd = ((fc02 & 1) != 0);
			bool fc_equal = ((fc02 & 2) != 0);
			bool fc_less = ((fc02 & 4) != 0);
			bool fc_inv_qnan = (fc3 != 0); // @TODO? -- Only used for detecting invalid operations?

			//throw(new NotImplementedException());

			//MipsMethodEmiter.SaveFCR31_C(() =>
			{
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
				MipsMethodEmiter.LoadFPR(FS);
				MipsMethodEmiter.LoadFPR(FT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, fc_unordererd ? 1 : 0);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, fc_equal ? 1 : 0);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, fc_less ? 1 : 0);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, fc_inv_qnan ? 1 : 0);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(CpuEmiter).GetMethod("_comp_impl"));
			}
			//);
		}

		// Compare <condition> Single_
		public void c_f_s() { _comp(0, 0); }
		public void c_un_s() { _comp(1, 0); }
		public void c_eq_s() { _comp(2, 0); }
		public void c_ueq_s() { _comp(3, 0); }
		public void c_olt_s() { _comp(4, 0); }
		public void c_ult_s() { _comp(5, 0); }
		public void c_ole_s() { _comp(6, 0); }
		public void c_ule_s() { _comp(7, 0); }
		public void c_sf_s() { _comp(0, 1); }
		public void c_ngle_s() { _comp(1, 1); }
		public void c_seq_s() { _comp(2, 1); }
		public void c_ngl_s() { _comp(3, 1); }
		public void c_lt_s() { _comp(4, 1); }
		public void c_nge_s() { _comp(5, 1); }
		public void c_le_s() { _comp(6, 1); }
		public void c_ngt_s() { _comp(7, 1); }
	}
}

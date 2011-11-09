using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
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
		public void round_w_s() { throw (new NotImplementedException()); }
		public void trunc_w_s() { throw (new NotImplementedException()); }
		public void ceil_w_s() { throw (new NotImplementedException()); }
		public void floor_w_s() { throw (new NotImplementedException()); }

		// Convert
		public void cvt_s_w() { throw (new NotImplementedException()); }
		public void cvt_w_s() { throw (new NotImplementedException()); }

		// Move float point registers
		public void mfc1() {
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				MipsMethodEmiter.LoadFPR(FS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Conv_I4);
			});
		}
		public void mtc1() {
			MipsMethodEmiter.SaveFPR(FS, () =>
			{
				MipsMethodEmiter.LoadGPR(RT);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Conv_R4);
			});
		}
		// CFC1 -- move Control word from/to floating point (C1)
		public void cfc1() { throw (new NotImplementedException()); }
		public void ctc1() { throw (new NotImplementedException()); }

		// Compare <condition> Single_
		public void c_f_s() { throw (new NotImplementedException()); }
		public void c_un_s() { throw (new NotImplementedException()); }
		public void c_eq_s() { throw (new NotImplementedException()); }
		public void c_ueq_s() { throw (new NotImplementedException()); }
		public void c_olt_s() { throw (new NotImplementedException()); }
		public void c_ult_s() { throw (new NotImplementedException()); }
		public void c_ole_s() { throw (new NotImplementedException()); }
		public void c_ule_s() { throw (new NotImplementedException()); }
		public void c_sf_s() { throw (new NotImplementedException()); }
		public void c_ngle_s() { throw (new NotImplementedException()); }
		public void c_seq_s() { throw (new NotImplementedException()); }
		public void c_ngl_s() { throw (new NotImplementedException()); }
		public void c_lt_s() { throw (new NotImplementedException()); }
		public void c_nge_s() { throw (new NotImplementedException()); }
		public void c_le_s() { throw (new NotImplementedException()); }
		public void c_ngt_s() { throw (new NotImplementedException()); }
	}
}

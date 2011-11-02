using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter.Emiters
{
	public partial class CpuEmiter
	{
		// Binary Floating Point Unit Operations
		public void add_s() { throw (new NotImplementedException()); }
		public void sub_s() { throw (new NotImplementedException()); }
		public void mul_s() { throw (new NotImplementedException()); }
		public void div_s() { throw (new NotImplementedException()); }

		// Unary Floating Point Unit Operations
		public void sqrt_s() { throw (new NotImplementedException()); }
		public void abs_s() { throw (new NotImplementedException()); }
		public void mov_s() { throw (new NotImplementedException()); }
		public void neg_s() { throw (new NotImplementedException()); }
		public void round_w_s() { throw (new NotImplementedException()); }
		public void trunc_w_s() { throw (new NotImplementedException()); }
		public void ceil_w_s() { throw (new NotImplementedException()); }
		public void floor_w_s() { throw (new NotImplementedException()); }

		// Convert
		public void cvt_s_w() { throw (new NotImplementedException()); }
		public void cvt_w_s() { throw (new NotImplementedException()); }

		// Move float point registers
		public void mfc1() { throw (new NotImplementedException()); }
		public void cfc1() { throw (new NotImplementedException()); }
		public void mtc1() { throw (new NotImplementedException()); }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		// Move From/to Vfpu (C?)_
		public void mfv() { throw (new NotImplementedException()); }
		public void mfvc() { throw (new NotImplementedException()); }
		public void mtv() { throw (new NotImplementedException()); }
		public void mtvc() { throw (new NotImplementedException()); }

		// Load/Store Vfpu (Left/Right)_
		public void lv_s() { throw (new NotImplementedException()); }
		public void lv_q() { throw (new NotImplementedException()); }
		public void lvl_q() { throw (new NotImplementedException()); }
		public void lvr_q() { throw (new NotImplementedException()); }
		public void sv_q() { throw (new NotImplementedException()); }

		// Vfpu DOT product
		// Vfpu SCaLe/ROTate
		public void vdot() { throw (new NotImplementedException()); }
		public void vscl() { throw (new NotImplementedException()); }
		public void vslt() { throw (new NotImplementedException()); }
		public void vsge() { throw (new NotImplementedException()); }

		// ROTate
		public void vrot() { throw (new NotImplementedException()); }

		// Vfpu ZERO/ONE
		public void vzero() { throw (new NotImplementedException()); }
		public void vone() { throw (new NotImplementedException()); }

		// Vfpu MOVe/SiGN/Reverse SQuare root/COSine/Arc SINe/LOG2
		public void vmov() { throw (new NotImplementedException()); }
		public void vabs() { throw (new NotImplementedException()); }
		public void vneg() { throw (new NotImplementedException()); }
		public void vocp() { throw (new NotImplementedException()); }
		public void vsgn() { throw (new NotImplementedException()); }
		public void vrcp() { throw (new NotImplementedException()); }
		public void vrsq() { throw (new NotImplementedException()); }
		public void vsin() { throw (new NotImplementedException()); }
		public void vcos() { throw (new NotImplementedException()); }
		public void vexp2() { throw (new NotImplementedException()); }
		public void vlog2() { throw (new NotImplementedException()); }
		public void vsqrt() { throw (new NotImplementedException()); }
		public void vasin() { throw (new NotImplementedException()); }
		public void vnrcp() { throw (new NotImplementedException()); }
		public void vnsin() { throw (new NotImplementedException()); }
		public void vrexp2() { throw (new NotImplementedException()); }

		public void vsat0() { throw (new NotImplementedException()); }
		public void vsat1() { throw (new NotImplementedException()); }

		// Vfpu ConSTant
		public void vcst() { throw (new NotImplementedException()); }

		// Vfpu Matrix MULtiplication
		public void vmmul() { throw (new NotImplementedException()); }

		// -
		public void vhdp() { throw (new NotImplementedException()); }
		public void vcrs_t() { throw (new NotImplementedException()); }
		public void vcrsp_t() { throw (new NotImplementedException()); }

		// Vfpu Integer to(2) Color
		public void vi2c() { throw (new NotImplementedException()); }
		public void vi2uc() { throw (new NotImplementedException()); }

		// -
		public void vtfm2() { throw (new NotImplementedException()); }
		public void vtfm3() { throw (new NotImplementedException()); }
		public void vtfm4() { throw (new NotImplementedException()); }

		public void vhtfm2() { throw (new NotImplementedException()); }
		public void vhtfm3() { throw (new NotImplementedException()); }
		public void vhtfm4() { throw (new NotImplementedException()); }

		public void vsrt3() { throw (new NotImplementedException()); }

		public void vfad() { throw (new NotImplementedException()); }

		// Vfpu MINimum/MAXium/ADD/SUB/DIV/MUL
		public void vmin() { throw (new NotImplementedException()); }
		public void vmax() { throw (new NotImplementedException()); }
		public void vadd() { throw (new NotImplementedException()); }
		public void vsub() { throw (new NotImplementedException()); }
		public void vdiv() { throw (new NotImplementedException()); }
		public void vmul() { throw (new NotImplementedException()); }

		// Vfpu (Matrix) IDenTity
		public void vidt() { throw (new NotImplementedException()); }
		public void vmidt() { throw (new NotImplementedException()); }

		public void viim() { throw (new NotImplementedException()); }

		public void vmmov() { throw (new NotImplementedException()); }
		public void vmzero() { throw (new NotImplementedException()); }
		public void vmone() { throw (new NotImplementedException()); }

		public void vnop() { throw (new NotImplementedException()); }
		public void vsync() { throw (new NotImplementedException()); }
		public void vflush() { throw (new NotImplementedException()); }

		public void vpfxd() { throw (new NotImplementedException()); }
		public void vpfxs() { throw (new NotImplementedException()); }
		public void vpfxt() { throw (new NotImplementedException()); }

		public void vdet() { throw (new NotImplementedException()); }

		public void vrnds() { throw (new NotImplementedException()); }
		public void vrndi() { throw (new NotImplementedException()); }
		public void vrndf1() { throw (new NotImplementedException()); }
		public void vrndf2() { throw (new NotImplementedException()); }

		public void vcmp() { throw (new NotImplementedException()); }

		public void vcmovf() { throw (new NotImplementedException()); }
		public void vcmovt() { throw (new NotImplementedException()); }

		////////////////////////////
		/// Not implemented yet!
		////////////////////////////

		public void vavg() { throw (new NotImplementedException()); }
		public void vf2id() { throw (new NotImplementedException()); }
		public void vf2in() { throw (new NotImplementedException()); }
		public void vf2iu() { throw (new NotImplementedException()); }
		public void vf2iz() { throw (new NotImplementedException()); }
		public void vi2f() { throw (new NotImplementedException()); }

		public void vscmp() { throw (new NotImplementedException()); }
		public void vmscl() { throw (new NotImplementedException()); }

		public void vt4444_q() { throw (new NotImplementedException()); }
		public void vt5551_q() { throw (new NotImplementedException()); }
		public void vt5650_q() { throw (new NotImplementedException()); }

		public void vmfvc() { throw (new NotImplementedException()); }
		public void vmtvc() { throw (new NotImplementedException()); }

		public void mfvme() { throw (new NotImplementedException()); }
		public void mtvme() { throw (new NotImplementedException()); }

		public void sv_s() { throw (new NotImplementedException()); }

		public void vfim() { throw (new NotImplementedException()); }

		public void svl_q() { throw (new NotImplementedException()); }
		public void svr_q() { throw (new NotImplementedException()); }

		public void vbfy1() { throw (new NotImplementedException()); }
		public void vbfy2() { throw (new NotImplementedException()); }

		public void vf2h() { throw (new NotImplementedException()); }
		public void vh2f() { throw (new NotImplementedException()); }

		public void vi2s() { throw (new NotImplementedException()); }
		public void vi2us() { throw (new NotImplementedException()); }

		public void vlgb() { throw (new NotImplementedException()); }
		public void vqmul() { throw (new NotImplementedException()); }
		public void vs2i() { throw (new NotImplementedException()); }
		public void vsbn() { throw (new NotImplementedException()); }

		public void vsbz() { throw (new NotImplementedException()); }
		public void vsocp() { throw (new NotImplementedException()); }
		public void vsrt1() { throw (new NotImplementedException()); }
		public void vsrt2() { throw (new NotImplementedException()); }
		public void vsrt4() { throw (new NotImplementedException()); }
		public void vus2i() { throw (new NotImplementedException()); }

		public void vwbn() { throw (new NotImplementedException()); }
		//public void vwb_q() { throw(new NotImplementedException()); }
	}
}

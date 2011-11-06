using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		private void _getmemptr(Action Action)
		{
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldarg_0);
			Action();
			MipsMethodEmiter.ILGenerator.Emit(OpCodes.Call, typeof(Processor).GetMethod("GetMemoryPtr"));
		}

		private void _load(Action Action)
		{
			MipsMethodEmiter.SaveGPR(RT, () =>
			{
				_getmemptr(() =>
				{
					MipsMethodEmiter.LoadGPR(RS);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				});
				Action();
			});
		}

		private void _save(Action Action)
		{
			_getmemptr(() =>
			{
				MipsMethodEmiter.LoadGPR(RS);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, IMM);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
			});
			MipsMethodEmiter.LoadGPR(RT);
			Action();
		}

		// Load Byte/Half word/Word (Left/Right/Unsigned).
		public void lb() { _load(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I1); }); }
		public void lh() { _load(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I2); }); }
		public void lw() { _load(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_I4); }); }
		public void lwl() { throw (new NotImplementedException()); }
		public void lwr() { throw (new NotImplementedException()); }
		public void lbu() { _load(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_U1); }); }
		public void lhu() { _load(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldind_U2); }); }

		// Store Byte/Half word/Word (Left/Right).
		public void sb() { _save(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Stind_I1); }); }
		public void sh() { _save(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Stind_I2); }); }
		public void sw() { _save(() => { MipsMethodEmiter.ILGenerator.Emit(OpCodes.Stind_I4); }); }
		public void swl() { throw (new NotImplementedException()); }
		public void swr() { throw (new NotImplementedException()); }

		// Load Linked word.
		// Store Conditional word.
		public void ll() { throw (new NotImplementedException()); }
		public void sc() { throw (new NotImplementedException()); }

		// Load Word to Cop1 floating point.
		// Store Word from Cop1 floating point.
		public void lwc1() { throw (new NotImplementedException()); }
		public void swc1() { throw (new NotImplementedException()); }
	}
}

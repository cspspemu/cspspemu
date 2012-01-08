using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		static public uint _vc2i_impl(uint Value)
		{
			Value |= (Value >> 16);
			Value |= (Value >> 8);
			Value |= (Value >> 4);
			Value |= 0x00808080;

			// 0x01010101
			// (((n      ) & 0xFF) * 0x01010101) >>> 1;

			return Value;
		}

		public void vuc2i()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 4) throw (new NotImplementedException());
			VectorOperationSaveVd(VectorSize, (Index) =>
			{
				Load_VS(0, 1, AsInteger: true);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, (3 - Index) * 4);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Shl);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0xF0000000);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
				MipsMethodEmiter.CallMethod(typeof(CpuEmiter), "_vc2i_impl");
			}, AsInteger: true);
		}

		public void vc2i()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 4) throw (new NotImplementedException());
			VectorOperationSaveVd(VectorSize, (Index) =>
			{
				Load_VS(0, 1, AsInteger: true);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, (3 - Index) * 8);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Shl);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0xFF000000);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
			}, AsInteger: true);
		}

		public void vs2i()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize > 2) throw(new NotImplementedException());
			VectorOperationSaveVd(VectorSize * 2, (Index) =>
			{
				Load_VS((Index / 2), VectorSize, AsInteger : true);
				if ((Index % 2) == 0)
				{
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 16);
					MipsMethodEmiter.ILGenerator.Emit(OpCodes.Shl);
				}
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, 0xFFFF0000);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.And);
			}, AsInteger: true);
		}

		public void vi2f()
		{
			VectorOperationSaveVd(Index =>
			{
				Load_VS(Index, AsInteger: true);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Conv_R4);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, -(int)Instruction.IMM5);
				MipsMethodEmiter.CallMethod(typeof(MathFloat), "Scalb");
			});
		}


		// Vfpu Integer to(2) Color?
		public void vi2c() { throw (new NotImplementedException("")); }
		public void vi2uc() { throw (new NotImplementedException("")); }

		public void vf2id() { throw (new NotImplementedException("")); }
		public void vf2in() { throw (new NotImplementedException("")); }
		public void vf2iu() { throw (new NotImplementedException("")); }
		public void vf2iz() { throw (new NotImplementedException("")); }
		public void vf2h() { throw (new NotImplementedException("")); }
		public void vh2f() { throw (new NotImplementedException("")); }
		public void vi2s() { throw (new NotImplementedException("")); }
		public void vi2us() { throw (new NotImplementedException("")); }
	}
}

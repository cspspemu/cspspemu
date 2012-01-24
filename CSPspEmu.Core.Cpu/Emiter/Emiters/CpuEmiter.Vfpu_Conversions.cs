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
				MipsMethodEmiter.CallMethod((Func<uint, uint>)CpuEmiter._vc2i_impl);
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
				MipsMethodEmiter.CallMethod((Func<float, int, float>)MathFloat.Scalb);
			});
		}


		// Vfpu Integer to(2) Color?
		public void vi2c() { throw (new NotImplementedException("")); }

		static public uint _vi2uc(int x, int y, int z, int w)
		{
			return (0
				| (uint)((x < 0) ? 0 : ((x >> 23) << 0))
				| (uint)((y < 0) ? 0 : ((y >> 23) << 8))
				| (uint)((z < 0) ? 0 : ((z >> 23) << 16))
				| (uint)((w < 0) ? 0 : ((w >> 23) << 24))
			);
		}

		public void vi2uc() {
			var VectorSize = Instruction.ONE_TWO;
			Save_VD(Index: 0, VectorSize: 1, Action: () =>
			{
				Load_VS(0, VectorSize, AsInteger: true);
				Load_VS(1, VectorSize, AsInteger: true);
				Load_VS(2, VectorSize, AsInteger: true);
				Load_VS(3, VectorSize, AsInteger: true);
				//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				//MipsMethodEmiter.ILGenerator.Emit(OpCodes.Add);
				MipsMethodEmiter.CallMethod((Func<int, int, int, int, uint>)_vi2uc);
			}, AsInteger: true);
		}

		public void vf2id() { throw (new NotImplementedException("")); }
		public void vf2in() { throw (new NotImplementedException("")); }
		public void vf2iu() { throw (new NotImplementedException("")); }

		static public float _vf2iz(float Value, int imm5)
		{
			float ScalabValue = MathFloat.Scalb(Value, imm5);
			return (Value >= 0) ? (int)MathFloat.Floor(ScalabValue) : (int)MathFloat.Ceil(ScalabValue);
		}

		public void vf2iz() {
			var Imm5 = Instruction.IMM5;
			VectorOperationSaveVd(Index =>
			{
				Load_VS(Index);
				MipsMethodEmiter.ILGenerator.Emit(OpCodes.Ldc_I4, Imm5);
				MipsMethodEmiter.CallMethod((Func<float, int, float>)(CpuEmiter._vf2iz));
			});
		}
		public void vf2h() { throw (new NotImplementedException("")); }
		public void vh2f() { throw (new NotImplementedException("")); }
		public void vi2s() { throw (new NotImplementedException("")); }
		public void vi2us() { throw (new NotImplementedException("")); }
	}
}

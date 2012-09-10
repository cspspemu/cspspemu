using System;
using CSharpUtils;
using Codegen;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		public static uint _vc2i_impl(uint Value)
		{
			Value |= (Value >> 16);
			Value |= (Value >> 8);
			Value |= (Value >> 4);
			Value |= 0x00808080;

			// 0x01010101
			// (((n      ) & 0xFF) * 0x01010101) >>> 1;

			return Value;
		}

		public static uint _vi2c_impl(uint x, uint y, uint z, uint w)
		{
			return
				((x >> 24) << 0 ) |
				((y >> 24) << 8 ) |
				((z >> 24) << 16) |
				((w >> 24) << 24) |
			0;
		}

		public void vuc2i()
		{
			var VectorSize = Instruction.ONE_TWO;
			//if (VectorSize != 4) throw (new NotImplementedException());
			//if (VectorSize != 1) throw (new NotImplementedException());
			
			//VectorOperationSaveVd(VectorSize, (Index) =>
			VectorOperationSaveVd(4, (Index) =>
			{
				Load_VS(0, 1, AsInteger: true);
				SafeILGenerator.Push((int)((3 - Index) * 4));
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftLeft);
				SafeILGenerator.Push(unchecked((int)0xF0000000));
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.And);
				MipsMethodEmiter.CallMethod((Func<uint, uint>)CpuEmitter._vc2i_impl);
			}, AsInteger: true);
		}

		public void vc2i()
		{
			var VectorSize = Instruction.ONE_TWO;
			//if (VectorSize != 4) throw (new NotImplementedException());
			//if (VectorSize != 1) throw (new NotImplementedException());

			//VectorOperationSaveVd(VectorSize, (Index) =>
			VectorOperationSaveVd(4, (Index) =>
			{
				Load_VS(0, 1, AsInteger: true);
				SafeILGenerator.Push((int)((3 - Index) * 8));
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftLeft);
				SafeILGenerator.Push(unchecked((int)0xFF000000));
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.And);
			}, AsInteger: true);
		}

		// Vfpu Integer to(2) Color?
		public void vi2c()
		{
			var VectorSize = Instruction.ONE_TWO;
			if (VectorSize != 4) throw (new NotImplementedException(""));

			VectorOperationSaveVd(1, (Index) =>
			{
				Load_VS(0, 4, AsInteger: true);
				Load_VS(1, 4, AsInteger: true);
				Load_VS(2, 4, AsInteger: true);
				Load_VS(3, 4, AsInteger: true);
				SafeILGenerator.Call((Func<uint, uint, uint, uint, uint>)_vi2c_impl);
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
					SafeILGenerator.Push((int)16);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftLeft);
				}
				SafeILGenerator.Push(unchecked((int)0xFFFF0000));
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.And);
			}, AsInteger: true);
		}

		public static uint _vi2uc(int x, int y, int z, int w)
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
				MipsMethodEmiter.CallMethod((Func<int, int, int, int, uint>)_vi2uc);
			}, AsInteger: true);
		}

		public void vi2f()
		{
			VectorOperationSaveVd(Index =>
			{
				Load_VS(Index, AsInteger: true);
				SafeILGenerator.ConvertTo<float>();
				SafeILGenerator.Push(-(int)Instruction.IMM5);
				MipsMethodEmiter.CallMethod((Func<float, int, float>)MathFloat.Scalb);
			});
		}

		public void vf2id() {
			var VectorSize = Instruction.ONE_TWO;
			var Imm5 = Instruction.IMM5;
			VectorOperationSaveVd(VectorSize, Index =>
			{
				Load_VS(Index, VectorSize);
				SafeILGenerator.Push((int)Imm5);
				MipsMethodEmiter.CallMethod((Func<float, int, float>)MathFloat.Scalb);
				MipsMethodEmiter.CallMethod((Func<float, int>)MathFloat.Floor);
			}, AsInteger: true);
		}

		public void vf2in()
		{
			var VectorSize = Instruction.ONE_TWO;
			var Imm5 = Instruction.IMM5;
			VectorOperationSaveVd(VectorSize, Index =>
			{
				Load_VS(Index, VectorSize);
				SafeILGenerator.Push((int)Imm5);
				MipsMethodEmiter.CallMethod((Func<float, int, float>)MathFloat.Scalb);
				MipsMethodEmiter.CallMethod((Func<float, int>)MathFloat.Round);
			}, AsInteger: true);
		}

		public void vf2iu()
		{
			var VectorSize = Instruction.ONE_TWO;
			var Imm5 = Instruction.IMM5;
			VectorOperationSaveVd(VectorSize, Index =>
			{
				Load_VS(Index, VectorSize);
				SafeILGenerator.Push((int)Imm5);
				MipsMethodEmiter.CallMethod((Func<float, int, float>)MathFloat.Scalb);
				MipsMethodEmiter.CallMethod((Func<float, int>)MathFloat.Ceil);
			}, AsInteger: true);
		}

		public static float _vf2iz(float Value, int imm5)
		{
			float ScalabValue = MathFloat.Scalb(Value, imm5);
			return (Value >= 0) ? (int)MathFloat.Floor(ScalabValue) : (int)MathFloat.Ceil(ScalabValue);
		}

		public void vf2iz() {
			var Imm5 = Instruction.IMM5;
			VectorOperationSaveVd(Index =>
			{
				Load_VS(Index);
				SafeILGenerator.Push((int)Imm5);
				MipsMethodEmiter.CallMethod((Func<float, int, float>)(CpuEmitter._vf2iz));
			});
		}

		public static uint _vi2s(uint v1, uint v2)
		{
			return (
				((v1 >> 16) << 0) |
				((v2 >> 16) << 16)
			);
		}

		public void vi2s()
		{
			var VectorSize = VectorSizeOneTwo;
			Save_VD(0, VectorSize, () =>
			{
				Load_VS(0);
				Load_VS(1);
				MipsMethodEmiter.CallMethod((Func<uint, uint, uint>)(CpuEmitter._vi2s));
			}, AsInteger: true);
			if (VectorSize == 4)
			{
				Save_VD(1, VectorSize, () =>
				{
					Load_VS(2);
					Load_VS(3);
					MipsMethodEmiter.CallMethod((Func<uint, uint, uint>)(CpuEmitter._vi2s));
				}, AsInteger: true);
			}
		}

		public void vf2h() { throw (new NotImplementedException("")); }
		public void vh2f() { throw (new NotImplementedException("")); }

		public static int _vi2us(int x, int y)
		{
			return (
				((x < 0) ? 0 : ((x >> 15) << 0)) |
				((y < 0) ? 0 : ((y >> 15) << 16))
			);
		}

		public void vi2us()
		{
			var VectorSize = VectorSizeOneTwo;
			VectorOperationSaveVd(VectorSize / 2, (Index) =>
			{
				Load_VS(Index * 2 + 0, AsInteger: true);
				Load_VS(Index * 2 + 1, AsInteger: true);
				MipsMethodEmiter.CallMethod((Func<int, int, int>)(CpuEmitter._vi2us));
			}, AsInteger: true);
		}
	}
}

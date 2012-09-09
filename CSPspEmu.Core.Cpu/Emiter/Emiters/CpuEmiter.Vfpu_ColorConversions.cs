using System;

namespace CSPspEmu.Core.Cpu.Emiter
{
    public sealed partial class CpuEmiter
	{
		public static uint _vt4444_step(uint i0, uint i1)
		{
			uint o0 = 0;
			o0 |= ((i0 >> 4) & 15) << 0;
			o0 |= ((i0 >> 12) & 15) << 4;
			o0 |= ((i0 >> 20) & 15) << 8;
			o0 |= ((i0 >> 28) & 15) << 12;
			o0 |= ((i1 >> 4) & 15) << 16;
			o0 |= ((i1 >> 12) & 15) << 20;
			o0 |= ((i1 >> 20) & 15) << 24;
			o0 |= ((i1 >> 28) & 15) << 28;
			return o0;
		}

		public static uint _vt5551_step(uint i0, uint i1)
		{
			uint o0 = 0;
			o0 |= ((i0 >> 3) & 31) << 0;
			o0 |= ((i0 >> 11) & 31) << 5;
			o0 |= ((i0 >> 19) & 31) << 10;
			o0 |= ((i0 >> 31) & 1) << 15;
			o0 |= ((i1 >> 3) & 31) << 16;
			o0 |= ((i1 >> 11) & 31) << 21;
			o0 |= ((i1 >> 19) & 31) << 26;
			o0 |= ((i1 >> 31) & 1) << 31;
			return o0;
		}

		public static uint _vt5650_step(uint i0, uint i1)
		{
			uint o0 = 0;
			o0 |= ((i0 >> 3) & 31) << 0;
			o0 |= ((i0 >> 10) & 63) << 5;
			o0 |= ((i0 >> 19) & 31) << 11;
			o0 |= ((i1 >> 3) & 31) << 16;
			o0 |= ((i1 >> 10) & 63) << 21;
			o0 |= ((i1 >> 19) & 31) << 27;
			return o0;
		}

		public void vt4444_q()
		{
			VectorOperationSaveVd(2, (Index) =>
			{
				Load_VS(0 + Index * 2, AsInteger: true);
				Load_VS(1 + Index * 2, AsInteger: true);
				SafeILGenerator.Call((Func<uint, uint, uint>)_vt4444_step);
			}, AsInteger: true);
		}

		public void vt5551_q()
		{
			VectorOperationSaveVd(2, (Index) =>
			{
				Load_VS(0 + Index * 2, AsInteger: true);
				Load_VS(1 + Index * 2, AsInteger: true);
				SafeILGenerator.Call((Func<uint, uint, uint>)_vt5551_step);
			}, AsInteger: true);
		}

		public void vt5650_q()
		{
			VectorOperationSaveVd(2, (Index) =>
			{
				Load_VS(0 + Index * 2, AsInteger: true);
				Load_VS(1 + Index * 2, AsInteger: true);
				SafeILGenerator.Call((Func<uint, uint, uint>)_vt5650_step);
			}, AsInteger: true);
		}
	}
}

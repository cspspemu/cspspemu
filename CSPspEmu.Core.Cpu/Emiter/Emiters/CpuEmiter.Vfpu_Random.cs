using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace CSPspEmu.Core.Cpu.Emiter
{
	sealed public partial class CpuEmiter
	{
		static public void _vrnds(CpuThreadState CpuThreadState, int Seed)
		{
			CpuThreadState.Random = new Random(Seed);
		}

		static public int _vrndi(CpuThreadState CpuThreadState)
		{
			byte[] Data = new byte[4];
			CpuThreadState.Random.NextBytes(Data);
			return BitConverter.ToInt32(Data, 0);
		}

		static public float _vrndf1(CpuThreadState CpuThreadState)
		{
			return (float)(CpuThreadState.Random.NextDouble() * 2.0f);
		}

		static public float _vrndf2(CpuThreadState CpuThreadState)
		{
			return (float)(CpuThreadState.Random.NextDouble() * 4.0f);
		}

		/// <summary>
		/// Seed
		/// </summary>
		public void vrnds() {
			SafeILGenerator.LoadArgument0CpuThreadState();
			Load_VS(0, true);
			MipsMethodEmiter.CallMethod((Action<CpuThreadState, int>)_vrnds);
		}

		/// <summary>
		/// -2^31 <= value < 2^31 
		/// </summary>
		public void vrndi()
		{
			var VectorSize = Instruction.ONE_TWO;
			Save_VD(Index: 0, VectorSize: VectorSize, Action: () =>
			{
				for (int n = 0; n < VectorSize; n++)
				{
					SafeILGenerator.LoadArgument0CpuThreadState();
					MipsMethodEmiter.CallMethod((Func<CpuThreadState, int>)_vrndi);
				}
			}, AsInteger: true);
		}

		/// <summary>
		/// 0.0 <= value < 2.0.
		/// </summary>
		public void vrndf1() {
			var VectorSize = Instruction.ONE_TWO;
			Save_VD(Index: 0, VectorSize: VectorSize, Action: () =>
			{
				for (int n = 0; n < VectorSize; n++)
				{
					SafeILGenerator.LoadArgument0CpuThreadState();
					MipsMethodEmiter.CallMethod((Func<CpuThreadState, float>)_vrndf1);
				}
			}, AsInteger: false);
		}

		/// <summary>
		/// 0.0 <= value < 4.0 (max = 3.999979)
		/// </summary>
		public void vrndf2() {
			var VectorSize = Instruction.ONE_TWO;
			Save_VD(Index: 0, VectorSize: VectorSize, Action: () =>
			{
				for (int n = 0; n < VectorSize; n++)
				{
					SafeILGenerator.LoadArgument0CpuThreadState();
					MipsMethodEmiter.CallMethod((Func<CpuThreadState, float>)_vrndf2);
				}
			}, AsInteger: false);

		}
	}
}

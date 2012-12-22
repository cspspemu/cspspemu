using SafeILGenerator.Ast.Nodes;
using System;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		public static void _vrnds(CpuThreadState CpuThreadState, int Seed)
		{
			CpuThreadState.Random = new Random(Seed);
		}

		public static int _vrndi(CpuThreadState CpuThreadState)
		{
			byte[] Data = new byte[4];
			CpuThreadState.Random.NextBytes(Data);
			return BitConverter.ToInt32(Data, 0);
		}

		public static float _vrndf1(CpuThreadState CpuThreadState)
		{
			return (float)(CpuThreadState.Random.NextDouble() * 2.0f);
		}

		public static float _vrndf2(CpuThreadState CpuThreadState)
		{
			return (float)(CpuThreadState.Random.NextDouble() * 4.0f);
		}

		/// <summary>
		/// Seed
		/// </summary>
		public AstNodeStm vrnds()
		{
			return AstNotImplemented("vrnds");
			//SafeILGenerator.LoadArgument0CpuThreadState();
			//Load_VS(0, true);
			//MipsMethodEmitter.CallMethod((Action<CpuThreadState, int>)_vrnds);
		}

		/// <summary>
		/// -2^31 &lt;= value &lt; 2^31 
		/// </summary>
		public AstNodeStm vrndi()
		{
			return AstNotImplemented("vrndi");
			//var VectorSize = Instruction.ONE_TWO;
			//Save_VD(Index: 0, VectorSize: VectorSize, Action: () =>
			//{
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		SafeILGenerator.LoadArgument0CpuThreadState();
			//		MipsMethodEmitter.CallMethod((Func<CpuThreadState, int>)_vrndi);
			//	}
			//}, AsInteger: true);
		}

        // 0.0 <= value < 2.0.
		/// <summary>
		/// 0.0 &lt;= value &lt; 2.0.
		/// </summary>
		public AstNodeStm vrndf1()
		{
			return AstNotImplemented("vrndf1");
			//var VectorSize = Instruction.ONE_TWO;
			//Save_VD(Index: 0, VectorSize: VectorSize, Action: () =>
			//{
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		SafeILGenerator.LoadArgument0CpuThreadState();
			//		MipsMethodEmitter.CallMethod((Func<CpuThreadState, float>)_vrndf1);
			//	}
			//}, AsInteger: false);
		}

        // 0.0 <= value < 4.0 (max = 3.999979)
		/// <summary>
		/// 0.0 &lt;= value &lt; 4.0 (max = 3.999979)
		/// </summary>
		public AstNodeStm vrndf2()
		{
			return AstNotImplemented("vrndf2");
			//var VectorSize = Instruction.ONE_TWO;
			//Save_VD(Index: 0, VectorSize: VectorSize, Action: () =>
			//{
			//	for (int n = 0; n < VectorSize; n++)
			//	{
			//		SafeILGenerator.LoadArgument0CpuThreadState();
			//		MipsMethodEmitter.CallMethod((Func<CpuThreadState, float>)_vrndf2);
			//	}
			//}, AsInteger: false);
		}
	}
}

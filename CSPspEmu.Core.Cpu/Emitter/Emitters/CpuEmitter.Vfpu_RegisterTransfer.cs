using System;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
	{
		public void vmfvc() { throw (new NotImplementedException("")); }
		public void vmtvc() { throw (new NotImplementedException("")); }

		/// <summary>
		/// ID("mtv",         VM("010010:00:111:rt:0:0000000:0:vd"), "%t, %zs", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public void mtv()
		{
			/*
			Console.Error.WriteLine("MTV:{0}", Instruction.VD);
			_VfpuLoadVectorWithIndexPointer(Instruction.VD, 0, 1);
			MipsMethodEmiter.LoadGPR_Signed(Instruction.RT);
			MipsMethodEmiter.CallMethod(typeof(MathFloat), "ReinterpretIntAsFloat");
			SafeILGenerator.StoreIndirect<float>();
			*/

			VectorOperationSaveVd(1, (Index) =>
			{
				MipsMethodEmitter.LoadGPR_Signed(RT);
				MipsMethodEmitter.CallMethod((Func<int, float>)MathFloat.ReinterpretIntAsFloat);
			});
		}
		public void mtvc() { throw (new NotImplementedException("mtvc")); }


		public static uint _mfvc_impl(CpuThreadState CpuThreadState, VfpuControlRegistersEnum VfpuControlRegister)
		{
			switch (VfpuControlRegister)
			{
				case VfpuControlRegistersEnum.VFPU_PFXS: return CpuThreadState.PrefixSource.Value;
				case VfpuControlRegistersEnum.VFPU_PFXT: return CpuThreadState.PrefixTarget.Value;
				case VfpuControlRegistersEnum.VFPU_PFXD: return CpuThreadState.PrefixDestination.Value;
				case VfpuControlRegistersEnum.VFPU_CC: return CpuThreadState.VFR_CC_Value;
				case VfpuControlRegistersEnum.VFPU_RCX0: return (uint)MathFloat.ReinterpretFloatAsInt((float)(new Random().NextDouble()));
				case VfpuControlRegistersEnum.VFPU_RCX1:
				case VfpuControlRegistersEnum.VFPU_RCX2:
				case VfpuControlRegistersEnum.VFPU_RCX3:
				case VfpuControlRegistersEnum.VFPU_RCX4:
				case VfpuControlRegistersEnum.VFPU_RCX5:
				case VfpuControlRegistersEnum.VFPU_RCX6:
				case VfpuControlRegistersEnum.VFPU_RCX7:
					return (uint)MathFloat.ReinterpretFloatAsInt(1.0f);
				default:
					throw (new NotImplementedException("_mfvc_impl: " + VfpuControlRegister));
			}
			//return 0;
		}

		/// <summary>
		/// Copies a vfpu control register into a general purpose register.
		/// </summary>
		public void mfvc()
		{
			MipsMethodEmitter.SaveGPR(RT, () =>
			{
				SafeILGenerator.LoadArgument0CpuThreadState();
				SafeILGenerator.Push((int)(Instruction.IMM7 + 128));
				MipsMethodEmitter.CallMethod((Func<CpuThreadState, VfpuControlRegistersEnum, uint>)CpuEmitter._mfvc_impl);
			});
		}

		// Move From/to Vfpu (C?)_
		public void mfv()
		{
			MipsMethodEmitter.SaveGPR_F(RT, () =>
			{
				Load_VD(0, 1);
			});
		}
	}
}

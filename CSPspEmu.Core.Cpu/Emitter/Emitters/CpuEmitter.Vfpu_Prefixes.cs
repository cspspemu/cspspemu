using System;
using CSharpUtils;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
	{
		public static void _vpfxd_impl(CpuThreadState CpuThreadState, uint Value)
		{
			CpuThreadState.PrefixDestination.Value = Value;
		}

		public static void _vpfxs_impl(CpuThreadState CpuThreadState, uint Value)
		{
			CpuThreadState.PrefixSource.Value = Value;
		}

		public static void _vpfxt_impl(CpuThreadState CpuThreadState, uint Value)
		{
			CpuThreadState.PrefixTarget.Value = Value;
		}

		public void vpfxd()
		{
			PrefixDestination.EnableAndSetValueAndPc(Instruction.Value, PC);
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Push((uint)Instruction.Value);
			MipsMethodEmiter.CallMethod((Action<CpuThreadState, uint>)_vpfxd_impl);
		}

		public void vpfxs()
		{
			PrefixSource.EnableAndSetValueAndPc(Instruction.Value, PC);
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Push((uint)Instruction.Value);
			MipsMethodEmiter.CallMethod((Action<CpuThreadState, uint>)_vpfxs_impl);
		}

		public void vpfxt()
		{
			PrefixTarget.EnableAndSetValueAndPc(Instruction.Value, PC);
			SafeILGenerator.LoadArgument0CpuThreadState();
			SafeILGenerator.Push((uint)Instruction.Value);
			MipsMethodEmiter.CallMethod((Action<CpuThreadState, uint>)_vpfxt_impl);
		}
	}

	public struct VfpuPrefix
	{
		public uint DeclaredPC;
		public uint UsedPC;
		public uint Value;
		public bool Enabled;
		public int UsedCount;

		// swz(xyzw)
		public uint SourceIndex(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (0 + i * 2)) & 3;
			return BitUtils.Extract(Value, 0 + i * 2, 2);
		}

		// abs(xyzw)
		public bool SourceAbsolute(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (8 + i * 1)) & 1;
			return BitUtils.Extract(Value, 8 + i * 1, 1) != 0;
		}

		// cst(xyzw)
		public bool SourceConstant(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (12 + i * 1)) & 1;
			return BitUtils.Extract(Value, 12 + i * 1, 1) != 0;
		}

		// neg(xyzw)
		public bool SourceNegate(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (16 + i * 1)) & 1;
			return BitUtils.Extract(Value, 16 + i * 1, 1) != 0;
		}

		// sat(xyzw)
		public uint DestinationSaturation(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (0 + i * 2)) & 3;
			return BitUtils.Extract(Value, 0 + i * 2, 2);
		}

		// msk(xyzw)
		public bool DestinationMask(int i)
		{
			//assert(i >= 0 && i < 4);
			//return (value >> (8 + i * 1)) & 1;
			return BitUtils.Extract(Value, 8 + i * 1, 1) != 0;
		}

		public void EnableAndSetValueAndPc(uint Value, uint PC)
		{
			this.Enabled = true;
			this.Value = Value;
			this.DeclaredPC = PC;
			this.UsedCount = 0;
		}

		public override string ToString()
		{
			return String.Format(
				"VfpuPrefix(Enabled={0}, UsedPC=0x{1:X}, DeclaredPC=0x{2:X})",
				Enabled, UsedPC, DeclaredPC
			);
		}
	}

}

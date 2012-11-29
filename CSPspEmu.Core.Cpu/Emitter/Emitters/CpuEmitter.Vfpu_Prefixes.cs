using System;
using SafeILGenerator.Ast.Nodes;

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

		public AstNodeStm vpfxd()
		{
			PrefixDestination.EnableAndSetValueAndPc(Instruction.Value, PC);
			return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint>)_vpfxd_impl, CpuThreadStateArgument(), Instruction.Value));
		}

		public AstNodeStm vpfxs()
		{
			PrefixSource.EnableAndSetValueAndPc(Instruction.Value, PC);
			return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint>)_vpfxs_impl, CpuThreadStateArgument(), Instruction.Value));
		}

		public AstNodeStm vpfxt()
		{
			PrefixTarget.EnableAndSetValueAndPc(Instruction.Value, PC);
			return ast.Statement(ast.CallStatic((Action<CpuThreadState, uint>)_vpfxt_impl, CpuThreadStateArgument(), Instruction.Value));
		}
	}
}

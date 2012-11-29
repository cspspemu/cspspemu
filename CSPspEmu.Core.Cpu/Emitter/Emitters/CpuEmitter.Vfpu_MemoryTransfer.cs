using System;
using SafeILGenerator;
using SafeILGenerator.Ast.Nodes;
using System.Linq;
using CSPspEmu.Core.Cpu.VFpu;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe sealed partial class CpuEmitter
	{
		// Load/Store Vfpu (Left/Right)_
		public AstNodeStm lv_s()
		{
			return AstNotImplemented();
			////return;
			//uint VT = Instruction.VT5 | (Instruction.VT2 << 5);
			//uint Column = (VT >> 5) & 3;
			//uint Matrix = (VT >> 2) & 7;
			//uint Row = (VT >> 0) & 3;
			//
			//SaveVprField(CalcVprRegisterIndex(Matrix, Column, Row), () =>
			//{
			//	_load_memory_imm14_index(0);
			//	SafeILGenerator.LoadIndirect<float>();
			//});
		}

		// ID("lv.q",        VM("110110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		public AstNodeStm lv_q()
		{
			int VectorSize = 4;

			return ast.Statements(VfpuUtils.XRange(0, VectorSize).Select(Index =>
				AstSaveVfpuReg(Instruction.VT5_1, Index, (uint)VectorSize, ref PrefixNone, AstMemoryGetValue<float>(Memory, Address_RS_IMM14(Index * 4)))
			));
		}

		public static void _lvl_svl_q(CpuThreadState CpuThreadState, uint m, uint i, uint address, bool dir, bool save)
		{
			uint k = 3 - ((address >> 2) & 3);
			address &= unchecked((uint)~0xF);
			
			for (uint j = k; j < 4; ++j)
			{
				fixed (float* VFPR = &CpuThreadState.VFR0)
				{
					float* ptr;
					if (dir)
					{
						ptr = &VFPR[VfpuUtils.GetCellIndex(m, i, j)];
					}
					else
					{
						ptr = &VFPR[VfpuUtils.GetCellIndex(m, j, i)];
					}

					if (save)
					{
						*(float*)CpuThreadState.GetMemoryPtr(address) = *ptr;
					}
					else
					{
						*ptr = *(float*)CpuThreadState.GetMemoryPtr(address);
					}
				}
				address += 4;
			}
		}

		public static void _lvr_svr_q(CpuThreadState CpuThreadState, uint m, uint i, uint address, bool dir, bool save)
		{
			uint k = 4 - ((address >> 2) & 3);
			
			for (uint j = 0; j < k; ++j)
			{
				fixed (float* VFPR = &CpuThreadState.VFR0)
				{
					float* ptr;
					if (dir)
					{
						ptr = &VFPR[VfpuUtils.GetCellIndex(m, i, j)];
					}
					else
					{
						ptr = &VFPR[VfpuUtils.GetCellIndex(m, j, i)];
					}
					if (save)
					{
						*(float*)CpuThreadState.GetMemoryPtr(address) = *ptr;
					}
					else
					{
						*ptr = *(float*)CpuThreadState.GetMemoryPtr(address);
					}
				}
				address += 4;
			}
		}

		private AstNodeStm lv_sv_l_r_q(bool left, bool save)
		{
			VfpuRegisterInt Register = Instruction.VT5_1;
			var MethodInfo = left
				? (Action<CpuThreadState, uint, uint, uint, bool, bool>)CpuEmitter._lvl_svl_q
				: (Action<CpuThreadState, uint, uint, uint, bool, bool>)CpuEmitter._lvr_svr_q
			;
			return ast.Statement(ast.CallStatic(
				MethodInfo,
				CpuThreadStateArgument(),
				Register.RC_MATRIX,
				Register.RC_LINE,
				Address_RS_IMM14(0),
				(Register.RC_ROW_COLUMN != 0),
				save
			));
		}

		public AstNodeStm lvl_q()
		{
			return lv_sv_l_r_q(left: true, save: false);
		}

		public AstNodeStm lvr_q()
		{
			return lv_sv_l_r_q(left: false, save: false);
		}

		public AstNodeStm sv_s()
		{
			return AstNotImplemented();
			//uint VT = Instruction.VT5 | (Instruction.VT2 << 5);
			//uint Column = (VT >> 5) & 3;
			//uint Matrix = (VT >> 2) & 7;
			//uint Row = (VT >> 0) & 3;
			//
			//_load_memory_imm14_index(0);
			//{
			//	LoadVprFieldPtr(CalcVprRegisterIndex(Matrix, Column, Row));
			//	SafeILGenerator.LoadIndirect<float>();
			//}
			//SafeILGenerator.StoreIndirect<float>();
		}

		/// <summary>
		/// ID("sv.q",        VM("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public AstNodeStm sv_q()
		{
			int VectorSize = 4;
			
			return ast.Statements(VfpuUtils.XRange(0, VectorSize).Select(Index =>
				AstMemorySetValue<float>(Memory, Address_RS_IMM14(Index * 4), AstLoadVfpuReg(Instruction.VT5_1, Index, (uint)VectorSize, ref PrefixNone))
			));
		}

		public AstNodeStm svl_q()
		{
			return lv_sv_l_r_q(left: true, save: true);
		}

		public AstNodeStm svr_q()
		{
			return lv_sv_l_r_q(left: false, save: true);
		}
	}
}

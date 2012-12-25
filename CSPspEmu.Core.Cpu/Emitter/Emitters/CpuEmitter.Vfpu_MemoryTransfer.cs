using System;
using SafeILGenerator.Ast.Nodes;
using System.Linq;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;
using System.Diagnostics;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe sealed partial class CpuEmitter
	{
		// Load/Store Vfpu (Left/Right)_
		public AstNodeStm lv_s()
		{
			return AstNotImplemented("lv_s");
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

			var Dest = _Vector(VT5_1, VFloat, VectorSize);

			return Dest.SetVector((Index) =>
				AstMemoryGetValue<float>(Memory, Address_RS_IMM14(Index * 4))
			);
		}

		/// <summary>
		/// ID("sv.q",        VM("111110:rs:vt5:imm14:0:vt1"), "%Xq, %Y", ADDR_TYPE_NONE, INSTR_TYPE_PSP),
		/// </summary>
		public AstNodeStm sv_q()
		{
			int VectorSize = 4;

			var Dest = _Vector(VT5_1, VFloat, VectorSize);

			return ast.Statements(Enumerable.Range(0, VectorSize).Select(Index =>
				AstMemorySetValue<float>(Memory, Address_RS_IMM14(Index * 4), Dest[Index])
			));
		}

		public static void _lvl_svl_q(CpuThreadState CpuThreadState, int m, int i, uint address, bool dir, bool save)
		{
			//Console.Error.WriteLine("++++++++++++++");

			int k = (int)(3 - ((address >> 2) & 3));
			address &= unchecked((uint)~0xF);

			fixed (float* VFPR = &CpuThreadState.VFR0)
			for (int j = k; j < 4; j++, address += 4)
			{
				float* ptr;
				var memory = (float*)CpuThreadState.GetMemoryPtr(address);
				var vfpr_register = VfpuUtils.GetIndexCell(m, dir ? j : i, dir ? i : j);
				ptr = &VFPR[vfpr_register];

				//Console.Error.WriteLine("_lvl_svl_q({0}): {1:X8}: {2:X8} {3} {4:X8}", j, memory_address, *(int*)ptr, save ? "<-" : "->", *(int*)memory);

				LanguageUtils.Transfer(
					ref *memory,
					ref *ptr,
					save
				);

				//Console.Error.WriteLine("_lvl_svl_q({0}): {1:X8}: {2:X8} {3} {4:X8}", j, memory_address, *(int*)ptr, save ? "<-" : "->", *(int*)memory);
			}

			//Console.Error.WriteLine("--------------");
		}

		public static void _lvr_svr_q(CpuThreadState CpuThreadState, int m, int i, uint address, bool dir, bool save)
		{
			//Console.Error.WriteLine("++++++++++++++");

			int k = (int)(4 - ((address >> 2) & 3));
			address &= unchecked((uint)~0xF);

			fixed (float* VFPR = &CpuThreadState.VFR0)
			for (int j = 0; j < k; j++, address += 4)
			{
				float* ptr;
				var memory_address = address;
				var memory = (float*)CpuThreadState.GetMemoryPtr(memory_address);
				var vfpr_register = VfpuUtils.GetIndexCell(m, dir ? j : i, dir ? i : j);
				ptr = &VFPR[vfpr_register];

				//Console.Error.WriteLine("_lvl_svr_q({0}): {1:X8}: {2:X8} {3} {4:X8}", j, memory_address, *(int*)ptr, save ? "<-" : "->", *(int*)memory);
				
				LanguageUtils.Transfer(
					ref *memory,
					ref *ptr,
					save
				);
			}

			//Console.Error.WriteLine("--------------");
		}

		private AstNodeStm lv_sv_l_r_q(bool left, bool save)
		{
			VfpuRegisterInt Register = Instruction.VT5_1;
			var MethodInfo = left
				? (Action<CpuThreadState, int, int, uint, bool, bool>)CpuEmitter._lvl_svl_q
				: (Action<CpuThreadState, int, int, uint, bool, bool>)CpuEmitter._lvr_svr_q
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
			return AstNotImplemented("sv_s");
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

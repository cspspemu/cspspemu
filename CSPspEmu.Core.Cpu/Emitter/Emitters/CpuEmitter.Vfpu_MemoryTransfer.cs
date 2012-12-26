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

		public static void _lvl_svl_q(CpuThreadState CpuThreadState, bool Save, float* r0, float* r1, float* r2, float* r3, uint Address)
		{
			//Console.Error.WriteLine("+LLLLLLLLLLLLL {0:X8}", Address);

			int k = (int)(3 - ((Address >> 2) & 3));
			Address &= unchecked((uint)~0xF);

			float*[] registers = new float*[] { r0, r1, r2, r3 };

			fixed (float* VFPR = &CpuThreadState.VFR0)
			for (int j = k; j < 4; j++, Address += 4)
			{
				float* ptr = registers[j];
				var memory_address = Address;
				var memory = (float*)CpuThreadState.GetMemoryPtr(memory_address);

				//Console.Error.WriteLine("_lvl_svl_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);

				LanguageUtils.Transfer(
					ref *memory,
					ref *ptr,
					Save
				);

				//Console.Error.WriteLine("_lvl_svl_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);
			}

			//Console.Error.WriteLine("--------------");
		}

		public static void _lvr_svr_q(CpuThreadState CpuThreadState, bool Save, float* r0, float* r1, float* r2, float* r3, uint Address)
		{
			//Console.Error.WriteLine("+RRRRRRRRRRRRR {0:X8}", Address);

			int k = (int)(4 - ((Address >> 2) & 3));
			//Address &= unchecked((uint)~0xF);

			float*[] registers = new float*[] { r0, r1, r2, r3 };

			fixed (float* VFPR = &CpuThreadState.VFR0)
				for (int j = 0; j < k; j++, Address += 4)
			{
				float* ptr = registers[j];
				var memory_address = Address;
				var memory = (float*)CpuThreadState.GetMemoryPtr(memory_address);

				//Console.Error.WriteLine("_lvl_svr_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);
				
				LanguageUtils.Transfer(
					ref *memory,
					ref *ptr,
					Save
				);

				//Console.Error.WriteLine("_lvl_svr_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);
			}

			//Console.Error.WriteLine("--------------");
		}

		delegate void Delegate_lvl_svl_q(CpuThreadState CpuThreadState, bool Save, float* r0, float* r1, float* r2, float* r3, uint Address);

		private AstNodeStm lv_sv_l_r_q(bool left, bool save)
		{
			VfpuRegisterInt Register = Instruction.VT5_1;
			var MethodInfo = left
				? (Delegate_lvl_svl_q)CpuEmitter._lvl_svl_q
				: (Delegate_lvl_svl_q)CpuEmitter._lvr_svr_q
			;

			var VT5 = _Vector(VT5_1, VFloat, 4);

			return ast.Statement(ast.CallStatic(
				MethodInfo,
				CpuThreadStateArgument(),
				save,
				ast.GetAddress(VT5.GetIndexRef(0)),
				ast.GetAddress(VT5.GetIndexRef(1)),
				ast.GetAddress(VT5.GetIndexRef(2)),
				ast.GetAddress(VT5.GetIndexRef(3)),
				Address_RS_IMM14(0)
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

		public AstNodeStm lv_s()
		{
			return _Cell(VT5_2).Set(AstMemoryGetValue<float>(Memory, Address_RS_IMM14()));
		}

		public AstNodeStm sv_s()
		{
			return AstMemorySetValue<float>(Memory, Address_RS_IMM14(), _Cell(VT5_2).Get());
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

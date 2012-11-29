using System;
using System.Collections.Generic;
using CSharpUtils;
using SafeILGenerator;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;

namespace CSPspEmu.Core.Cpu.Emitter
{
	// http://forums.ps2dev.org/viewtopic.php?t=6929 
	// http://wiki.fx-world.org/doku.php?do=index
	// http://mrmrice.fx-world.org/vfpu.html
	// http://hitmen.c02.at/files/yapspd/psp_doc/chap4.html
	// pspgl_codegen.h
	// 
	/**
	 * Before you begin messing with the vfpu, you need to do one thing in your project:
	 * PSP_MAIN_THREAD_ATTR(PSP_THREAD_ATTR_VFPU);
	 * Almost all psp applications define this in the projects main c file. It sets a value that tells the psp how to handle your applications thread
	 * in case the kernel needs to switch to another thread and back to yours. You need to add PSP_THREAD_ATTR_VFPU to this so the psp's kernel will
	 * properly save/restore the vfpu state on thread switch, otherwise bad things might happen if another thread uses the vfpu and stomps on whatever was in there.
	 *
	 * Before diving into the more exciting bits, first you need to know how the VFPU registers are configured.
	 * The vfpu contains 128 32-bit floating point registers (same format as the float type in C).
	 * These registers can be accessed individually or in groups of 2, 3, 4, 9 or 16 in one instruction.
	 * They are organized as 8 blocks of registers, 16 per block.When you write code to access these registers, there is a naming convention you must use.
	 * 
	 * Every register name has 4 characters: Xbcr
	 * 
	 * X can be one of:
	 *   M - this identifies a matrix block of 4, 9 or 16 registers
	 *   E - this identifies a transposed matrix block of 4, 9 or 16 registers
	 *   C - this identifies a column of 2, 3 or 4 registers
	 *   R - this identifies a row of 2, 3, or 4 registers
	 *   S - this identifies a single register
	 *
	 * b can be one of:
	 *   0 - register block 0
	 *   1 - register block 1
	 *   2 - register block 2
	 *   3 - register block 3
	 *   4 - register block 4
	 *   5 - register block 5
	 *   6 - register block 6
	 *   7 - register block 7
	 *
	 * c can be one of:
	 *   0 - column 0
	 *   1 - column 1
	 *   2 - column 2
	 *   3 - column 3
	 *
	 * r can be one of:
	 *   0 - row 0
	 *   1 - row 1
	 *   2 - row 2
	 *   3 - row 3
	 *
	 * So for example, the register name S132 would be a single register in column 3, row 2 in register block 1.
	 * M500 would be a matrix of registers in register block 5.
	 *
	 * Almost every vfpu instruction will end with one of the following extensions:
	 *   .s - instruction works on a single register
	 *   .p - instruction works on a 2 register vector or 2x2 matrix
	 *   .t - instruction works on a 3 register vector or 3x3 matrix
	 *   .q - instruction works on a 4 register vector or 4x4 matrix
	 * 
	 * http://wiki.fx-world.org/doku.php?id=general:vfpu_registers
	 *
	 * This is something you need to know about how to transfer data in or out of the vfpu. First lets show the instructions used to load/store data from the vfpu:
	 *   lv.s (load 1 vfpu reg from unaligned memory)
	 *   lv.q (load 4 vfpu regs from 16 byte aligned memory)
	 *   sv.s (write 1 vfpu reg to unaligned memory)
	 *   sv.q (write 4 vfpu regs to 16 byte aligned memory)
	 *
	 * There are limitations with these instructions. You can only transfer to or from column or row registers in the vfpu.
	 *
	 * You can also load values into the vfpu from a MIPS register, this will work with all single registers:
	 *   mtv (move MIPS register to vfpu register)
	 *   mfv (move from vfpu register to MIPS register)
	 *
	 * There are 2 instructions, ulv.q and usv.q, that perform unaligned ran transfers to/from the vfpu. These have been found to be faulty so it is not recommended to use them.
	 *
	 * The vfpu performs a few trig functions, but they dont behave like the normal C functions we are used to.
	 * Normally we would pass in the angle in radians from -pi/2 to +pi/2, but the vfpu wants the input value in the range of -1 to 1.
	 *
	**/

	/**
	   The VFPU contains 32 registers (128bits each, 4x32bits).

	   VFPU Registers can get accessed as Matrices, Vectors or single words.
	   All registers are overlayed and enumerated in 3 digits (Matrix/Column/Row):

		M000 | C000   C010   C020   C030	M100 | C100   C110   C120   C130
		-----+--------------------------	-----+--------------------------
		R000 | S000   S010   S020   S030	R100 | S100   S110   S120   S130
		R001 | S001   S011   S021   S031	R101 | S101   S111   S121   S131
		R002 | S002   S012   S022   S032	R102 | S102   S112   S122   S132
		R003 | S003   S013   S023   S033	R103 | S103   S113   S123   S133

	  same for matrices starting at M200 - M700.
	  Subvectors can get addressed as singles/pairs/triplets/quads.
	  Submatrices can get addressed 2x2 pairs, 3x3 triplets or 4x4 quads.

	  So Q_C010 specifies the Quad Column starting at S010, T_C011 the triple Column starting at S011.
	*/
	public unsafe sealed partial class CpuEmitter
	{
		private void _call_debug_vfpu()
		{
			throw(new NotImplementedException());
			//MipsMethodEmitter.CallMethodWithCpuThreadStateAsFirstArgument(this.GetType(), "_debug_vfpu");
		}

		public static void _debug_vfpu(CpuThreadState CpuThreadState)
		{
			Console.Error.WriteLine("");
			Console.Error.WriteLine("VPU DEBUG:");
			fixed (float* FPR = &CpuThreadState.VFR0)
			{
				int Index = 0;
				for (int Matrix = 0; Matrix < 8; Matrix++)
				{
					Console.Error.WriteLine("Matrix {0}: ", Matrix);
					for (int Row = 0; Row < 4; Row++)
					{
						for (int Column = 0; Column < 4; Column++)
						{
							Console.Error.Write("{0},", FPR[Index]);
							Index++;
						}
						Console.Error.WriteLine("");
					}
					Console.Error.WriteLine("");
				}
			}
		}

		private void _load_memory_imm14_index(uint Index)
		{
			throw(new NotImplementedException());
			//MipsMethodEmitter._getmemptr(() =>
			//{
			//	MipsMethodEmitter.LoadGPR_Unsigned(RS);
			//	SafeILGenerator.Push((int)(Instruction.IMM14 * 4 + Index * 4));
			//	SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			//}, Safe: true, CanBeNull: false);
		}

		public VfpuPrefix PrefixNone;
		public VfpuPrefix PrefixSource;
		public VfpuPrefix PrefixTarget;
		public VfpuDestinationPrefix PrefixDestinationNone;
		public VfpuDestinationPrefix PrefixDestination;

		private void CheckPrefixUsage(ref VfpuPrefix Prefix, bool Debug = true)
		{
			// Disable the prefix once it have been used.
			if (Prefix.Enabled)
			{
				if (Prefix.UsedCount > 0 && Prefix.UsedPC != PC)
				{
					Prefix.Enabled = false;
				}
			}
		}

		private void CheckPrefixUsage(ref VfpuDestinationPrefix Prefix, bool Debug = true)
		{
			// Disable the prefix once it have been used.
			if (Prefix.Enabled)
			{
				if (Prefix.UsedCount > 0 && Prefix.UsedPC != PC)
				{
					Prefix.Enabled = false;
				}
			}
		}

		/*
		private void VfpuLoad_Register(uint Register, int Index, uint VectorSize, ref VfpuPrefix Prefix, bool Debug = false, bool AsInteger = false)
		{
			//Console.Error.WriteLine("{0:X}", PC);
			CheckPrefixUsage(ref Prefix);
			//Console.Error.WriteLine("PREFIX [1]!" + Index);

			if (Prefix.Enabled)
			{
				//Console.Error.WriteLine("PREFIX [2]!" + Index);
				Prefix.UsedPC = PC;
				Prefix.UsedCount++;

				// Constant.
				if (Prefix.SourceConstant(Index))
				{
					float Value = 0.0f;
					switch (Prefix.SourceIndex(Index))
					{
						case 0: Value = Prefix.SourceAbsolute(Index) ? (3.0f) : (0.0f); break;
						case 1: Value = Prefix.SourceAbsolute(Index) ? (1.0f / 3.0f) : (1.0f); break;
						case 2: Value = Prefix.SourceAbsolute(Index) ? (1.0f / 4.0f) : (2.0f); break;
						case 3: Value = Prefix.SourceAbsolute(Index) ? (1.0f / 6.0f) : (0.5f); break;
						default: throw(new InvalidOperationException());
					}
					//Console.Error.WriteLine("VALUE:: " + Value);
					if (AsInteger)
					{
						SafeILGenerator.Push((int)MathFloat.ReinterpretFloatAsInt(Value));
					}
					else
					{
						SafeILGenerator.Push((float)Value);
					}
				}
				// Value.
				else
				{
					_VfpuLoadVectorWithIndexPointer(Register, (uint)Prefix.SourceIndex(Index), VectorSize, Debug);
					if (AsInteger)
					{
						SafeILGenerator.LoadIndirect<int>();
					}
					else
					{
						SafeILGenerator.LoadIndirect<float>();
					}
				}

				if (Prefix.SourceAbsolute(Index))
				{
					//MipsMethodEmiter.ILGenerator.Emit(OpCodes);
					MipsMethodEmitter.CallMethod((Func<float, float>)MathFloat.Abs);
				}
				if (Prefix.SourceNegate(Index))
				{
					SafeILGenerator.UnaryOperation(SafeUnaryOperator.Negate);
				}
			}
			else
			{
				_VfpuLoadVectorWithIndexPointer(Register, (uint)Index, VectorSize, Debug);
				if (AsInteger)
				{
					SafeILGenerator.LoadIndirect<int>();
				}
				else
				{
					SafeILGenerator.LoadIndirect<float>();
				}

			}
		}
		*/

		/*
		if (enabled && prefix.enabled) {
			foreach (i, value; src) {
				if (prefix.mask(i)) continue;

				switch (prefix.saturation(i)) {
					case 1: value = clamp!(float)(value,  0.0, 1.0); break;
					case 3: value = clamp!(float)(value, -1.0, 1.0); break;
					default: break;
				}

				*dst[i] = value;
			}
			prefix.enabled = false;
		} else {
			foreach (i, value; src) *dst[i] = value;
		}
		*/

		/*
		private void Load_VCC(uint Index)
		{
			MipsMethodEmitter.LoadFieldPtr(typeof(CpuThreadState).GetField("VFR_CC_" + Index));
			SafeILGenerator.LoadIndirect<sbyte>();
		}

		private void Save_VCC(int Index, Action Action)
		{
			MipsMethodEmitter.LoadFieldPtr(typeof(CpuThreadState).GetField("VFR_CC_" + Index));
			{
				Action();
			}
			SafeILGenerator.StoreIndirect<sbyte>();
		}

		private void VfpuSave_Register(uint Register, int Index, uint VectorSize, VfpuDestinationPrefix Prefix, Action Action, bool Debug = false, bool AsInteger = false)
		{
			CheckPrefixUsage(ref Prefix);
			_VfpuLoadVectorWithIndexPointer(Register, (uint)Index, VectorSize, Debug);
			{
				Action();
				if (Prefix.Enabled)
				{
					if (!Prefix.DestinationMask(Index))
					{
						float Min = 0, Max = 0;
						bool DoClamp = false;
						switch (Prefix.DestinationSaturation(Index))
						{
							case 1: DoClamp = true; Min = 0.0f; Max = 1.0f; break;
							case 3: DoClamp = true; Min = -1.0f; Max = 1.0f; break;
							default: break;
						}
						if (DoClamp)
						{
							if (AsInteger)
							{
								SafeILGenerator.Push((int)Min);
								SafeILGenerator.Push((int)Max);
								MipsMethodEmitter.CallMethod((Func<int, int, int, int>)MathFloat.ClampInt);
							}
							else
							{
								SafeILGenerator.Push((float)Min);
								SafeILGenerator.Push((float)Max);
								MipsMethodEmitter.CallMethod((Func<float, float, float, float>)MathFloat.Clamp);
							}
						}
					}
				}
			}
			if (AsInteger)
			{
				SafeILGenerator.StoreIndirect<int>();
			}
			else
			{
				SafeILGenerator.StoreIndirect<float>();
			}
		}

		// VfpuPrefix VfpuPrefix, 

		private void Load_VS(int Index, bool AsInteger = false)
		{
			Load_VS(Index, VectorSizeOneTwo, AsInteger: AsInteger);
		}

		private void Load_VS(int Index, uint VectorSize, int RegisterOffset = 0, bool Debug = false, bool AsInteger = false)
		{
			VfpuLoad_Register((uint)(Instruction.VS + RegisterOffset), Index, VectorSize, ref PrefixSource, Debug, AsInteger: AsInteger);
		}

		private void Load_VT(int Index, bool AsInteger = false)
		{
			Load_VT(Index, VectorSizeOneTwo, AsInteger: AsInteger);
		}

		private void Load_VT(int Index, uint VectorSize, int RegisterOffset = 0, bool Debug = false, bool AsInteger = false)
		{
			VfpuLoad_Register((uint)(Instruction.VT + RegisterOffset), Index, VectorSize, ref PrefixTarget, Debug, AsInteger: AsInteger);
		}

		private void Load_VS_VT(int Index, bool AsInteger = false)
		{
			Load_VS(Index, AsInteger: AsInteger);
			Load_VT(Index, AsInteger: AsInteger);
		}

		private void Load_VS_VT(int Index, uint VectorSize, bool AsInteger = false)
		{
			Load_VS(Index, VectorSize: VectorSize, AsInteger: AsInteger);
			Load_VT(Index, VectorSize: VectorSize, AsInteger: AsInteger);
		}

		private void Load_VS_VT(int Index, uint VectorSize, int RegisterOffset = 0, bool Debug = false, bool AsInteger = false)
		{
			Load_VS(Index, VectorSize, RegisterOffset, Debug, AsInteger: AsInteger);
			Load_VT(Index, VectorSize, RegisterOffset, Debug, AsInteger: AsInteger);
		}

		private void Load_VD(int Index, uint VectorSize, int RegisterOffset = 0, bool Debug = false, bool AsInteger = false)
		{
			//Load_Register(Instruction.VD + RegisterOffset, Index, VectorSize, PrefixNone, Debug);
			VfpuLoad_Register((uint)(Instruction.VD + RegisterOffset), Index, VectorSize, ref PrefixNone, Debug, AsInteger: AsInteger);
		}

		private void Save_VD(int Index, uint VectorSize, int RegisterOffset, Action Action, bool Debug = false, bool AsInteger = false)
		{
			VfpuSave_Register((uint)(Instruction.VD + RegisterOffset), Index, VectorSize, PrefixDestination, Action, Debug, AsInteger: AsInteger);
		}

		private void Save_VD(int Index, uint VectorSize, Action Action, bool Debug = false, bool AsInteger = false)
		{
			VfpuSave_Register((uint)(Instruction.VD), Index, VectorSize, PrefixDestination, Action, Debug, AsInteger: AsInteger);
		}

		private void Save_VT(int Index, uint VectorSize, int RegisterOffset, Action Action, bool Debug = false, bool AsInteger = false)
		{
			VfpuSave_Register((uint)(Instruction.VT + RegisterOffset), Index, VectorSize, PrefixDestinationNone, Action, Debug, AsInteger: AsInteger);
		}

		private void Save_VT(int Index, uint VectorSize, Action Action, bool Debug = false, bool AsInteger = false)
		{
			VfpuSave_Register((uint)(Instruction.VT), Index, VectorSize, PrefixDestinationNone, Action, Debug, AsInteger: AsInteger);
		}
		*/

		/*
		static IEnumerable<int> XRange(int Start, int End)
		{
			for (int Value = Start; Value < End; Value++)
			{
				yield return Value;
			}
		}

	    static IEnumerable<int> XRange(uint Count)
		{
			return XRange(0, (int)Count);
		}


		public enum LineType
		{
			None = 0,
			Row = 1,
			Column = 2,
		}
		*/

		// S000 <-- S(Matrix)(Column)(Row)
		/*
		private void _VfpuLoadVectorWithIndexPointer(uint Register, uint Index, uint Size, bool Debug = false)
		{
			uint Line = BitUtils.Extract(Register, 0, 2); // 0-3
			uint Matrix = BitUtils.Extract(Register, 2, 3); // 0-7
			LineType LineType = LineType.None;
			uint Row = 0;
			uint Column = 0;

			if (Size == 1)
			{
				Column = Line;
				Row = (Register >> 5) & 3;
			}
			else
			{
				uint Offset = (Register & 64) >> (int)(3 + Size);
				LineType = ((Register & 32) == 0) ? LineType.Column : LineType.Row;

				if (LineType == LineType.Row)
				{
					Column = Offset + Index;
					Row = Line;
				}
				else
				{
					Column = Line;
					Row = Offset + Index;
				}
			}

			uint RegisterIndex = Matrix * 16 + Row * 4 + Column;

			if (Debug)
			{
				char C = 'S';
				if (LineType != LineType.None)
				{
					C = (LineType == LineType.Row) ? 'R' : 'C';
				}
				Console.Error.WriteLine(
					"_VfpuLoadVectorWithIndexPointer(R={0},I={1},S={2}): " +
					"{9}{3}{4}{5} Index({1}) :: Matrix={3}, Column={4}, Row={5}, Type={6}, " +
					"RegisterIndex={7}, Line={8}",
					Register, Index, Size,
					Matrix, Column, Row, LineType,
					RegisterIndex,
					Line, C
				);
			}

			//if (Reg == null) throw(new InvalidOperationException("Invalid Vfpu register"));
			LoadVprFieldPtr(RegisterIndex);
		}

	    static uint CalcVprRegisterIndex(uint Matrix, uint Column, uint Row)
		{
			return Matrix * 16 + Column * 4 + Row;
		}

		private void SaveVprField(uint RegisterIndex, Action LoadValueCallback)
		{
			LoadVprFieldPtr(RegisterIndex);
			{
				LoadValueCallback();
			}
			SafeILGenerator.StoreIndirect<float>();
		}

		private void LoadVprFieldPtr(uint RegisterIndex)
		{
			if (RegisterIndex < 0 || RegisterIndex > 128) throw(new InvalidCastException("Invalid VFR Register '" + RegisterIndex + "'"));
			try
			{
				var FieldInfo = typeof(CpuThreadState).GetField("VFR" + RegisterIndex);
				MipsMethodEmitter.LoadFieldPtr(FieldInfo);
			}
			catch (Exception Exception)
			{
				throw (new Exception("Can't load VFR register", Exception));
			}
		}

		private void _VectorOperation_N_Registers(uint VectorSize, int InputCount, Action<uint> Action)
		{
			foreach (var Index in XRange(VectorSize))
			{
				Save_VD(Index, VectorSize, () =>
				{
					if (InputCount >= 1) Load_VS(Index, VectorSize);
					if (InputCount >= 2) Load_VT(Index, VectorSize);
					Action((uint)Index);
				});
			}
		}
		*/

		/*
		private uint VectorSizeOneTwo
		{
			get
			{
				return Instruction.ONE_TWO;
			}
		}

		private void VectorOperationSaveVd(Action<int> Action, Action AccumulateAction = null, bool AsInteger = false)
		{
			VectorOperationSaveVd(VectorSizeOneTwo, Action, AccumulateAction, AsInteger: AsInteger);
		}

		private void VectorOperationSaveVd(uint VectorSize, Action<int> Action, Action AccumulateAction = null, bool AsInteger = false)
		{
			foreach (var Index in XRange(VectorSize))
			{
				Save_VD(Index, VectorSize, () =>
				{
					Action((int)Index);
				}, AsInteger: AsInteger);
			}
			if (AccumulateAction != null) AccumulateAction();
		}

		public static float LogFloatResult(float Value, CpuThreadState CpuThreadState)
		{
			Console.Error.WriteLine("LogFloatResult: {0}", Value);
			//CpuThreadState.DumpVfpuRegisters(Console.Error);
			return Value;
		}

		private void EmitLogFloatResult(bool Return = true)
		{
			SafeILGenerator.LoadArgument0CpuThreadState();
			MipsMethodEmitter.CallMethod((Func<float, CpuThreadState, float>)CpuEmitter.LogFloatResult);
			if (!Return)
			{
				SafeILGenerator.Pop();
			}
		}

		private void VectorOperationSaveAggregatedVd(uint VectorSize, Action StartAction, Action<int, Action<int>> IterationAction, Action EndAction)
		{
			SaveVprField(Instruction.VD, () =>
			{
				StartAction();
				foreach (var Index in XRange(VectorSize))
				{
					Action<int> Load = InputCount =>
					{
						if (InputCount >= 1) Load_VS(Index, VectorSize);
						if (InputCount >= 2) Load_VT(Index, VectorSize);
					};
					IterationAction(Index, Load);
				}
				EndAction();
			}
			);
		}
		*/

		private AstNodeExprLValue VFR(int Index)
		{
			return REG("VFR" + Index);
		}

		//private void VfpuLoad_Register(uint Register, int Index, uint VectorSize, ref VfpuPrefix Prefix, bool Debug = false, bool AsInteger = false)

		private AstNodeExpr AstLoadVfpuReg(uint Register, int Index, uint VectorSize, ref VfpuPrefix Prefix, bool AsInteger = false)
		{
			CheckPrefixUsage(ref Prefix);

			var RegisterIndices = VfpuUtils.GetIndices(VectorSize, VfpuUtils.RegisterType.Vector, Register);
			AstNodeExpr AstNodeExpr;

			if (Prefix.Enabled)
			{
				//Console.Error.WriteLine("PREFIX [2]!" + Index);
				Prefix.UsedPC = PC;
				Prefix.UsedCount++;

				// Constant.
				if (Prefix.SourceConstant(Index))
				{
					float Value = 0.0f;
					switch (Prefix.SourceIndex(Index))
					{
						case 0: Value = Prefix.SourceAbsolute(Index) ? (3.0f) : (0.0f); break;
						case 1: Value = Prefix.SourceAbsolute(Index) ? (1.0f / 3.0f) : (1.0f); break;
						case 2: Value = Prefix.SourceAbsolute(Index) ? (1.0f / 4.0f) : (2.0f); break;
						case 3: Value = Prefix.SourceAbsolute(Index) ? (1.0f / 6.0f) : (0.5f); break;
						default: throw (new InvalidOperationException());
					}

					if (AsInteger)
					{
						AstNodeExpr = ast.Immediate((int)MathFloat.ReinterpretFloatAsInt(Value));
					}
					else
					{
						AstNodeExpr = ast.Immediate((float)Value);
					}
				}
				// Value.
				else
				{
					if (AsInteger)
					{
						throw(new NotImplementedException());
					}
					else
					{
						AstNodeExpr = VFR(RegisterIndices[Prefix.SourceIndex(Index)]);
					}
					if (Prefix.SourceAbsolute(Index)) AstNodeExpr = ast.CallStatic((Func<float, float>)MathFloat.Abs, AstNodeExpr);
				}

				if (Prefix.SourceNegate(Index)) AstNodeExpr = -AstNodeExpr;
			}
			else
			{
				
				AstNodeExpr = VFR(RegisterIndices[Index]);
			}

			return AstNodeExpr;
		}

		private AstNodeExpr AstLoadVs(int Index, bool AsInteger = false)
		{
			var VectorSize = Instruction.ONE_TWO;
			return AstLoadVfpuReg(Instruction.VS, Index, VectorSize, ref PrefixSource, AsInteger);
		}

		private AstNodeExpr AstLoadVt(int Index, bool AsInteger = false)
		{
			var VectorSize = Instruction.ONE_TWO;
			return AstLoadVfpuReg(Instruction.VT, Index, VectorSize, ref PrefixTarget, AsInteger);
		}

		private AstNodeExpr PrefixVdTransform(int Index, AstNodeExpr AstNodeExpr, bool AsInteger)
		{
			CheckPrefixUsage(ref PrefixTarget);

			//Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAA ({0}, {1})", PrefixTarget.Enabled, PrefixDestination.DestinationMask(Index));

			if (PrefixDestination.Enabled)
			{
				if (!PrefixDestination.DestinationMask(Index))
				{
					//Console.WriteLine("BBBBBBBBBBBBBBB ({0})", PrefixDestination.DestinationSaturation(Index));
					float Min = 0, Max = 0;
					bool DoClamp = false;
					switch (PrefixDestination.DestinationSaturation(Index))
					{
						case 1: DoClamp = true; Min = 0.0f; Max = 1.0f; break;
						case 3: DoClamp = true; Min = -1.0f; Max = 1.0f; break;
						default: break;
					}
					if (DoClamp)
					{
						if (AsInteger)
						{
							AstNodeExpr = ast.CallStatic((Func<int, int, int, int>)MathFloat.ClampInt, AstNodeExpr, (int)Min, (int)Max);
						}
						else
						{
							AstNodeExpr = ast.CallStatic((Func<float, float, float, float>)MathFloat.Clamp, AstNodeExpr, (float)Min, (float)Max);
						}
					}
				}
			}

			return AstNodeExpr;
		}

		private AstNodeStm AstVfpuStoreVd(uint Size, Func<int, AstNodeExpr> Generator, bool AsInteger = false)
		{
			int[] RegisterIndices = VfpuUtils.GetIndices(Size, VfpuUtils.RegisterType.Vector, Instruction.VD);
			var Items = new List<AstNodeStm>();
			for (int Index = 0; Index < Size; Index++)
			{
				Items.Add(ast.Assign(VFR(RegisterIndices[Index]), PrefixVdTransform(Index, Generator(Index), AsInteger)));
			}
			return ast.Statements(Items.ToArray());
		}

		private AstNodeStm AstVfpuStoreVd(Func<int, AstNodeExpr> Generator, bool AsInteger = false)
		{
			return AstVfpuStoreVd(Instruction.ONE_TWO, Generator, AsInteger);
		}

		private AstNodeExpr AstVfpuLoadRegMatrixElement(uint Size, uint Register, int Column, int Row)
		{
			return VFR(VfpuUtils.GetIndicesMatrix(Size, Register)[Column, Row]);
		}

		private AstNodeExpr AstVfpuLoadRegMatrixElement(uint Register, int Column, int Row)
		{
			return VFR(VfpuUtils.GetIndicesMatrix(Instruction.ONE_TWO, Register)[Column, Row]);
		}

		private AstNodeStm AstVfpuStoreVdMatrix(Func<int, int, AstNodeExpr> Generator, bool AsInteger = false)
		{
			var Size = Instruction.ONE_TWO;
			var Items = new List<AstNodeStm>();

			var Register = (VfpuRegisterInt)Instruction.VD;
			var RegisterIndices = VfpuUtils.GetIndicesMatrix(Size, Instruction.VD);

			for (int Row = 0; Row < Size; Row++)
			{
				for (int Column = 0; Column < Size; Column++)
				{
					Items.Add(ast.Assign(VFR(RegisterIndices[Column, Row]), PrefixVdTransform(Column, Generator(Column, Row), AsInteger)));
				}
			}
			return ast.Statements(Items.ToArray());
		}

		private AstNodeStm AstSaveVfpuReg(uint Register, int Index, uint VectorSize, ref VfpuPrefix Prefix, AstNodeExpr ValueExpr, bool AsInteger = false)
		{
			int[] RegisterIndices = VfpuUtils.GetIndices(VectorSize, VfpuUtils.RegisterType.Vector, Register);
			return ast.Assign(VFR(RegisterIndices[Index]), PrefixVdTransform(Index, ValueExpr, AsInteger));
		}
	}
}

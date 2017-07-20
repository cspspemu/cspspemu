//#define CHECK_VFPU_REGISTER_SET

using System;
using System.Collections.Generic;
using SafeILGenerator.Ast.Nodes;
using CSPspEmu.Core.Cpu.VFpu;
using CSharpUtils;
using System.Linq;
using System.Runtime.InteropServices;
using SafeILGenerator.Ast;
using System.Runtime.CompilerServices;

namespace CSPspEmu.Core.Cpu.Emitter
{
    // http://forums.ps2dev.org/viewtopic.php?t=6929 
    // http://wiki.fx-world.org/doku.php?do=index
    // http://mrmrice.fx-world.org/vfpu.html
    // http://hitmen.c02.at/files/yapspd/psp_doc/chap4.html
    // pspgl_codegen.h
    // 
    // *
    //  Before you begin messing with the vfpu, you need to do one thing in your project:
    //  PSP_MAIN_THREAD_ATTR(PSP_THREAD_ATTR_VFPU);
    //  Almost all psp applications define this in the projects main c file. It sets a value that tells the psp how to handle your applications thread
    //  in case the kernel needs to switch to another thread and back to yours. You need to add PSP_THREAD_ATTR_VFPU to this so the psp's kernel will
    //  properly save/restore the vfpu state on thread switch, otherwise bad things might happen if another thread uses the vfpu and stomps on whatever was in there.
    // 
    //  Before diving into the more exciting bits, first you need to know how the VFPU registers are configured.
    //  The vfpu contains 128 32-bit floating point registers (same format as the float type in C).
    //  These registers can be accessed individually or in groups of 2, 3, 4, 9 or 16 in one instruction.
    //  They are organized as 8 blocks of registers, 16 per block.When you write code to access these registers, there is a naming convention you must use.
    //  
    //  Every register name has 4 characters: Xbcr
    //  
    //  X can be one of:
    //    M - this identifies a matrix block of 4, 9 or 16 registers
    //    E - this identifies a transposed matrix block of 4, 9 or 16 registers
    //    C - this identifies a column of 2, 3 or 4 registers
    //    R - this identifies a row of 2, 3, or 4 registers
    //    S - this identifies a single register
    // 
    //  b can be one of:
    //    0 - register block 0
    //    1 - register block 1
    //    2 - register block 2
    //    3 - register block 3
    //    4 - register block 4
    //    5 - register block 5
    //    6 - register block 6
    //    7 - register block 7
    // 
    //  c can be one of:
    //    0 - column 0
    //    1 - column 1
    //    2 - column 2
    //    3 - column 3
    // 
    //  r can be one of:
    //    0 - row 0
    //    1 - row 1
    //    2 - row 2
    //    3 - row 3
    // 
    //  So for example, the register name S132 would be a single register in column 3, row 2 in register block 1.
    //  M500 would be a matrix of registers in register block 5.
    // 
    //  Almost every vfpu instruction will end with one of the following extensions:
    //    .s - instruction works on a single register
    //    .p - instruction works on a 2 register vector or 2x2 matrix
    //    .t - instruction works on a 3 register vector or 3x3 matrix
    //    .q - instruction works on a 4 register vector or 4x4 matrix
    //  
    //  http://wiki.fx-world.org/doku.php?id=general:vfpu_registers
    // 
    //  This is something you need to know about how to transfer data in or out of the vfpu. First lets show the instructions used to load/store data from the vfpu:
    //    lv.s (load 1 vfpu reg from unaligned memory)
    //    lv.q (load 4 vfpu regs from 16 byte aligned memory)
    //    sv.s (write 1 vfpu reg to unaligned memory)
    //    sv.q (write 4 vfpu regs to 16 byte aligned memory)
    // 
    //  There are limitations with these instructions. You can only transfer to or from column or row registers in the vfpu.
    // 
    //  You can also load values into the vfpu from a MIPS register, this will work with all single registers:
    //    mtv (move MIPS register to vfpu register)
    //    mfv (move from vfpu register to MIPS register)
    // 
    //  There are 2 instructions, ulv.q and usv.q, that perform unaligned ran transfers to/from the vfpu. These have been found to be faulty so it is not recommended to use them.
    // 
    //  The vfpu performs a few trig functions, but they dont behave like the normal C functions we are used to.
    //  Normally we would pass in the angle in radians from -pi/2 to +pi/2, but the vfpu wants the input value in the range of -1 to 1.
    // 

    //
    //   The VFPU contains 32 registers (128bits each, 4x32bits).
    //
    //   VFPU Registers can get accessed as Matrices, Vectors or single words.
    //   All registers are overlayed and enumerated in 3 digits (Matrix/Column/Row):
    //
    //	M000 | C000   C010   C020   C030	M100 | C100   C110   C120   C130
    //	-----+--------------------------	-----+--------------------------
    //	R000 | S000   S010   S020   S030	R100 | S100   S110   S120   S130
    //	R001 | S001   S011   S021   S031	R101 | S101   S111   S121   S131
    //	R002 | S002   S012   S022   S032	R102 | S102   S112   S122   S132
    //	R003 | S003   S013   S023   S033	R103 | S103   S113   S123   S133
    //
    //  same for matrices starting at M200 - M700.
    //  Subvectors can get addressed as singles/pairs/triplets/quads.
    //  Submatrices can get addressed 2x2 pairs, 3x3 triplets or 4x4 quads.
    //
    //  So Q_C010 specifies the Quad Column starting at S010, T_C011 the triple Column starting at S011.
    //
    public sealed unsafe partial class CpuEmitter
    {
        private void _call_debug_vfpu()
        {
            throw (new NotImplementedException("_call_debug_vfpu"));
            //MipsMethodEmitter.CallMethodWithCpuThreadStateAsFirstArgument(this.GetType(), "_debug_vfpu");
        }

        public static void _debug_vfpu(CpuThreadState cpuThreadState)
        {
            Console.Error.WriteLine("");
            Console.Error.WriteLine("VPU DEBUG:");
            fixed (float* fpr = &cpuThreadState.Vfr0)
            {
                var index = 0;
                for (var matrix = 0; matrix < 8; matrix++)
                {
                    Console.Error.WriteLine("Matrix {0}: ", matrix);
                    for (var row = 0; row < 4; row++)
                    {
                        for (var column = 0; column < 4; column++)
                        {
                            Console.Error.Write("{0},", fpr[index]);
                            index++;
                        }
                        Console.Error.WriteLine("");
                    }
                    Console.Error.WriteLine("");
                }
            }
        }

        private void _load_memory_imm14_index(uint index)
        {
            throw (new NotImplementedException("_load_memory_imm14_index"));
            //MipsMethodEmitter._getmemptr(() =>
            //{
            //	MipsMethodEmitter.LoadGPR_Unsigned(RS);
            //	SafeILGenerator.Push((int)(Instruction.IMM14 * 4 + Index * 4));
            //	SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
            //}, Safe: true, CanBeNull: false);
        }

        public VfpuPrefix PrefixNone = new VfpuPrefix();
        public VfpuPrefix PrefixSource = new VfpuPrefix();
        public VfpuPrefix PrefixTarget = new VfpuPrefix();
        public VfpuDestinationPrefix PrefixDestinationNone = new VfpuDestinationPrefix();
        public VfpuDestinationPrefix PrefixDestination = new VfpuDestinationPrefix();

        internal abstract class VfpuRuntimeRegister
        {
            protected uint Pc;
            protected Instruction Instruction;
            protected VReg VReg;
            protected VType VType;
            protected int VectorSize;
            protected Dictionary<int, AstLocal> Locals = new Dictionary<int, AstLocal>();
            private static readonly AstMipsGenerator Ast = AstMipsGenerator.Instance;

            protected AstNodeExprLocal GetLocal(int index)
            {
                if (!Locals.ContainsKey(index)) Locals[index] = AstLocal.Create(GetVTypeType(), "LocalVFPR" + index);
                return Ast.Local(Locals[index]);
            }

            protected VfpuRuntimeRegister(CpuEmitter cpuEmitter, VReg vReg, VType vType, int vectorSize)
            {
                Pc = cpuEmitter._pc;
                Instruction = cpuEmitter._instruction;
                VReg = vReg;
                VType = vType;
                VectorSize = (vectorSize == 0) ? Instruction.OneTwo : vectorSize;
            }

            private Type GetVTypeType()
            {
                switch (VType)
                {
                    case VType.VFloat: return typeof(float);
                    case VType.VInt: return typeof(int);
                    case VType.VuInt: return typeof(uint);
                    default: throw (new InvalidCastException("Invalid VType " + VType));
                }
            }

            protected AstNodeExprLValue _GetVRegRef(int regIndex)
            {
                var toType = GetVTypeType();
                return toType == typeof(float) ? Ast.Vfr(regIndex) : Ast.Reinterpret(toType, Ast.Vfr(regIndex));
            }

            protected AstNodeExpr GetRegApplyPrefix(int[] indices, int regIndex, int prefixIndex)
            {
                var prefix = VReg.VfpuPrefix;
                prefix.CheckPrefixUsage(Pc);

                //Console.WriteLine("[Get] {0:X8}:: {1}", PC, Prefix.Enabled);
                //Console.WriteLine("GetRegApplyPrefix: {0}, {1}", Indices.Length, RegIndex);

                AstNodeExpr astNodeExpr = _GetVRegRef(indices[regIndex]);

                if (prefix.Enabled && prefix.IsValidIndex(prefixIndex))
                {
                    // Constant.
                    if (prefix.SourceConstant(prefixIndex))
                    {
                        float value = 0.0f;
                        var sourceIndex = prefix.SourceIndex(prefixIndex);
                        switch (sourceIndex)
                        {
                            case 0:
                                value = prefix.SourceAbsolute(prefixIndex) ? (3.0f) : (0.0f);
                                break;
                            case 1:
                                value = prefix.SourceAbsolute(prefixIndex) ? (1.0f / 3.0f) : (1.0f);
                                break;
                            case 2:
                                value = prefix.SourceAbsolute(prefixIndex) ? (1.0f / 4.0f) : (2.0f);
                                break;
                            case 3:
                                value = prefix.SourceAbsolute(prefixIndex) ? (1.0f / 6.0f) : (0.5f);
                                break;
                            default: throw (new InvalidOperationException("Invalid SourceIndex : " + sourceIndex));
                        }

                        astNodeExpr = Ast.Cast(GetVTypeType(), value);
                    }
                    // Value.
                    else
                    {
                        astNodeExpr = _GetVRegRef(indices[(int) prefix.SourceIndex(prefixIndex)]);
                        if (prefix.SourceAbsolute(prefixIndex))
                            astNodeExpr = Ast.CallStatic((Func<float, float>) MathFloat.Abs, astNodeExpr);
                    }

                    if (prefix.SourceNegate(prefixIndex)) astNodeExpr = Ast.Unary("-", astNodeExpr);
                }

                return astNodeExpr;
            }

            protected AstNodeStm SetRegApplyPrefix(int regIndex, int prefixIndex, AstNodeExpr astNodeExpr)
            {
                if (astNodeExpr == null) return null;

                var prefixDestination = VReg.VfpuDestinationPrefix;
                prefixDestination.CheckPrefixUsage(Pc);

                //Console.WriteLine("[Set] {0:X8}:: {1}", PC, PrefixDestination.Enabled);

                if (prefixDestination.Enabled && prefixDestination.IsValidIndex(prefixIndex))
                {
                    // It is masked. It won't write the value.
                    float max = 0;
                    float min = 0;
                    if (prefixDestination.DestinationMask(prefixIndex))
                    {
                        //return ast.Statement();
                        astNodeExpr = _GetVRegRef(regIndex);
                    }
                    else
                    {
                        var doClamp = false;
                        switch (prefixDestination.DestinationSaturation(prefixIndex))
                        {
                            case 1:
                                doClamp = true;
                                min = 0.0f;
                                max = 1.0f;
                                break;
                            case 3:
                                doClamp = true;
                                min = -1.0f;
                                max = 1.0f;
                                break;
                        }

                        if (doClamp)
                        {
                            if (VType == CpuEmitter.VType.VFloat)
                            {
                                astNodeExpr = Ast.CallStatic((Func<float, float, float, float>) MathFloat.Clamp,
                                    astNodeExpr, (float) min, (float) max);
                            }
                            else
                            {
                                astNodeExpr = Ast.Cast(GetVTypeType(),
                                    Ast.CallStatic((Func<int, int, int, int>) MathFloat.ClampInt, astNodeExpr,
                                        (int) min, (int) max));
                            }
                        }
                    }
                }

                //Console.Error.WriteLine("PrefixIndex:{0}", PrefixIndex);
                return Ast.Assign(GetLocal(regIndex), Ast.Cast(GetVTypeType(), astNodeExpr));
            }

            protected AstNodeStm SetRegApplyPrefix2(int regIndex, int prefixIndex, uint pc, string calledFrom)
            {
                return Ast.Statements(
#if CHECK_VFPU_REGISTER_SET
					ast.Statement(ast.CallStatic((Action<string, int, uint, float>)CheckVfpuRegister, CalledFrom, RegIndex, PC, ast.Cast<float>(GetLocal(RegIndex)))),
#endif
                    Ast.Assign(_GetVRegRef(regIndex), GetLocal(regIndex))
                );
            }

            public static void CheckVfpuRegister(string opcode, int regIndex, uint pc, float value)
            {
                //if (float.IsNaN(Value))
                {
                    Console.WriteLine("VFPU_SET:{0:X4}:{1}:VR{2}:{3:X8}:{4},", pc, opcode, regIndex,
                        MathFloat.ReinterpretFloatAsUInt(value), value);
                }
            }
        }

        internal sealed class VfpuCell : VfpuRuntimeRegister
        {
            private int _index;

            public VfpuCell(CpuEmitter cpuEmitter, VReg vReg, VType vType)
                : base(cpuEmitter, vReg, vType, 1)
            {
                _index = VfpuUtils.GetIndexCell(vReg.Reg);
            }

            public AstNodeExpr Get() => GetRegApplyPrefix(new[] {_index}, 0, 0);

            public AstNodeStm Set(AstNodeExpr value, uint pc, [CallerMemberName] string calledFrom = "") =>
                _ast.Statements(
                    SetRegApplyPrefix(_index, 0, value),
                    SetRegApplyPrefix2(_index, 0, pc, calledFrom)
                );
        }

        internal sealed class VfpuVector : VfpuRuntimeRegister
        {
            private int[] _indices;

            public VfpuVector(CpuEmitter cpuEmitter, VReg vReg, VType vType, int vectorSize)
                : base(cpuEmitter, vReg, vType, vectorSize)
            {
                _indices = VfpuUtils.GetIndicesVector(VectorSize, vReg.Reg);
            }

            public AstNodeExpr this[int index] => Get(index);
            public AstNodeExprLValue GetIndexRef(int index) => _GetVRegRef(_indices[index]);
            public AstNodeExpr Get(int index) => GetRegApplyPrefix(_indices, index, index);
            public AstNodeStm Set(int index, AstNodeExpr value) => SetRegApplyPrefix(_indices[index], index, value);

            public AstNodeStm Set2(int index, uint pc, [CallerMemberName] string calledFrom = "") =>
                SetRegApplyPrefix2(this._indices[index], index, pc, calledFrom);

            public AstNodeStm SetVector(Func<int, AstNodeExpr> generator, uint pc,
                [CallerMemberName] string calledFrom = "")
            {
                return _ast.Statements(
                    _ast.Statements(Enumerable.Range(0, VectorSize).Select(index => Set(index, generator(index)))
                        .Where(statement => statement != null)),
                    _ast.StatementsInline(Enumerable.Range(0, VectorSize)
                        .Select(index => Set2(index, pc, calledFrom)).Where(statement => statement != null))
                );
            }
        }

        internal sealed class VFpuVectorRef
        {
            private AstNodeExprLValue[] _refs;

            public VFpuVectorRef(params AstNodeExprLValue[] refs) => _refs = refs;

            public int VectorSize => _refs.Length;

            public AstNodeStm SetVector(Func<int, AstNodeExpr> generator)
            {
                return _ast.Statements(Enumerable.Range(0, VectorSize)
                    .Select(index => _ast.Assign(this[index], generator(index))));
            }

            public AstNodeExprLValue this[int index] => _refs[index];

            public static VFpuVectorRef Generate(int vectorSize, Func<int, AstNodeExprLValue> callback)
            {
                return new VFpuVectorRef(Enumerable.Range(0, vectorSize).Select(callback).ToArray());
            }
        }

        internal sealed class VfpuMatrix : VfpuRuntimeRegister
        {
            private int[,] _indices;

            public VfpuMatrix(CpuEmitter cpuEmitter, VReg vReg, VType vType, int vectorSize)
                : base(cpuEmitter, vReg, vType, vectorSize)
            {
                _indices = VfpuUtils.GetIndicesMatrix(this.VectorSize, vReg.Reg);
            }

            public AstNodeExpr this[int column, int row] => Get(column, row);

            public AstNodeExprLValue GetIndexRef(int column, int row) => _GetVRegRef(_indices[column, row]);

            private AstNodeExpr Get(int column, int row) => _GetVRegRef(this._indices[column, row]);

            private int GetPrefixIndex(int column, int row)
            {
                //return 0;
                return -1;
                //return Row;
                //return Row * 4 + Column;
                //return Column;
            }

            public AstNodeStm Set(int column, int row, AstNodeExpr value)
            {
                return SetRegApplyPrefix(_indices[column, row], GetPrefixIndex(column, row), value);
            }

            public AstNodeStm Set2(int column, int row, uint pc, [CallerMemberName] string calledFrom = "")
            {
                return SetRegApplyPrefix2(_indices[column, row], GetPrefixIndex(column, row), pc, calledFrom);
            }

            public AstNodeStm SetMatrix(Func<int, int, AstNodeExpr> generator, uint pc,
                [CallerMemberName] string calledFrom = "")
            {
                var statements = new List<AstNodeStm>();

                for (var row = 0; row < VectorSize; row++)
                for (var column = 0; column < VectorSize; column++)
                {
                    statements.Add(Set(column, row, generator(column, row)));
                }

                for (var row = 0; row < VectorSize; row++)
                for (var column = 0; column < VectorSize; column++) statements.Add(Set2(column, row, pc, calledFrom));
                return _ast.Statements(statements);
            }
        }

        public class VReg
        {
            public VfpuRegisterInt Reg;
            public VfpuDestinationPrefix VfpuDestinationPrefix;
            public VfpuPrefix VfpuPrefix;
        }

        public enum VType
        {
            VFloat,
            VInt,
            VuInt,
        }

        private VType VInt => VType.VInt;
        private VType VuInt => VType.VuInt;
        private VType VFloat => VType.VFloat;

        private VReg Vd => new VReg
        {
            Reg = _instruction.Vd,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestination
        };

        private VReg Vs => new VReg
        {
            Reg = _instruction.Vs,
            VfpuPrefix = PrefixSource,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg Vt => new VReg
        {
            Reg = _instruction.Vt,
            VfpuPrefix = PrefixTarget,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg Vt51 => new VReg
        {
            Reg = _instruction.Vt51,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg Vt52 => new VReg
        {
            Reg = _instruction.Vt52,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg VdNoPrefix => new VReg
        {
            Reg = _instruction.Vd,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg VsNoPrefix => new VReg
        {
            Reg = _instruction.Vs,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VReg VtNoPrefix => new VReg
        {
            Reg = _instruction.Vt,
            VfpuPrefix = PrefixNone,
            VfpuDestinationPrefix = PrefixDestinationNone
        };

        private VFpuVectorRef _MemoryVectorIMM14<TType>(int vectorSize)
        {
            var elementSize = Marshal.SizeOf(typeof(TType));
            return VFpuVectorRef.Generate(vectorSize,
                index => _ast.MemoryGetPointerRef<TType>(_memory, Address_RS_IMM14(index * elementSize)));
        }

        private VfpuCell _Cell(VReg vReg, VType vType = VType.VFloat) => new VfpuCell(this, vReg, vType);

        private VfpuVector _Vector(VReg vReg, VType vType = VType.VFloat, int size = 0) => new VfpuVector(this, vReg, vType, size);

        private VfpuMatrix _Matrix(VReg vReg, VType vType = VType.VFloat, int size = 0) => new VfpuMatrix(this, vReg, vType, size);

        private VfpuMatrix Mat(VReg vReg, VType vType = VType.VFloat, int size = 0) => new VfpuMatrix(this, vReg, vType, size);

        private VfpuVector Vec(VReg vReg, VType vType = VType.VFloat, int size = 0) => new VfpuVector(this, vReg, vType, size);
        private VfpuCell Cel(VReg vReg, VType vType = VType.VFloat) => new VfpuCell(this, vReg, vType);
        private VfpuMatrix MatVs => Mat(Vs);
        private VfpuMatrix MatVd => Mat(Vd);
        private VfpuMatrix MatVt => Mat(Vt);
        private VfpuVector VecVs => Vec(Vs);
        private VfpuVector VecVd => Vec(Vd);
        private VfpuVector VecVt => Vec(Vt);
        private VfpuVector VecVsI => Vec(Vs, VType.VInt);
        private VfpuVector VecVdI => Vec(Vd, VType.VInt);
        private VfpuVector VecVtI => Vec(Vt, VType.VInt);
        private VfpuVector VecVsU => Vec(Vs, VType.VuInt);
        private VfpuVector VecVdU => Vec(Vd, VType.VuInt);
        private VfpuVector VecVtU => Vec(Vt, VType.VuInt);
        private VfpuCell CelVs => Cel(Vs);
        private VfpuCell CelVd => Cel(Vd);
        private VfpuCell CelVt => Cel(Vt);
        private VfpuCell CelVtNoPrefix => Cel(VtNoPrefix);
        private VfpuCell CelVsI => Cel(Vs, VType.VInt);
        private VfpuCell CelVdI => Cel(Vd, VType.VInt);
        private VfpuCell CelVtI => Cel(Vt, VType.VInt);
        private VfpuCell CelVtINoPrefix => Cel(VtNoPrefix, VType.VInt);
        private VfpuCell CelVsU => Cel(Vs, VType.VuInt);
        private VfpuCell CelVdU => Cel(Vd, VType.VuInt);
        private VfpuCell CelVtU => Cel(Vt, VType.VuInt);
        private VfpuCell CelVtUNoPrefix => Cel(VtNoPrefix, VType.VuInt);
        private AstNodeExpr _Aggregate(AstNodeExpr first, Func<AstNodeExpr, int, AstNodeExpr> callback) => _Aggregate(first, OneTwo, callback);
        private AstNodeStmContainer _List(Func<int, AstNodeStm> callback) => _List(OneTwo, callback);
        private AstNodeStmContainer _List(int vectorSize, Func<int, AstNodeStm> callback)
        {
            var statements = _ast.Statements();
            for (var index = 0; index < vectorSize; index++)
                statements.AddStatement(callback(index));
            return statements;
        }

        private AstNodeExpr _Aggregate(AstNodeExpr first, int vectorSize, Func<AstNodeExpr, int, AstNodeExpr> callback)
        {
            var value = first;
            for (var index = 0; index < vectorSize; index++)
                value = callback(value, index);
            return value;
        }
    }
}
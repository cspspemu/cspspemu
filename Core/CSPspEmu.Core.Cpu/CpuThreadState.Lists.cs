using CSharpUtils;
using CSPspEmu.Core.Cpu.VFpu;
using System;
using System.Linq;

namespace CSPspEmu.Core.Cpu
{
    public unsafe partial class CpuThreadState
    {
        public struct FCR31
        {
            public enum TypeEnum : uint
            {
                Rint = 0,
                Cast = 1,
                Ceil = 2,
                Floor = 3,
            }

            private uint _Value;

            public uint Value
            {
                get
                {
                    _Value = BitUtils.Insert(_Value, 0, 2, (uint) RM);
                    _Value = BitUtils.Insert(_Value, 23, 1, (uint) (CC ? 1 : 0));
                    _Value = BitUtils.Insert(_Value, 24, 1, (uint) (FS ? 1 : 0));
                    return _Value;
                }
                set
                {
                    _Value = value;
                    CC = (BitUtils.Extract(value, 23, 1) != 0);
                    FS = (BitUtils.Extract(value, 24, 1) != 0);
                    RM = (TypeEnum) BitUtils.Extract(value, 0, 2);
                }
            }

            public TypeEnum RM;
            public bool CC;
            public bool FS;
        }

        public class GprList
        {
            public CpuThreadState CpuThreadState;

            private int NameToIndex(string name) => Array.IndexOf(RegisterMnemonicNames, name);

            public int this[string name]
            {
                get => this[NameToIndex(name)];
                set => this[NameToIndex(name)] = value;
            }

            public int this[int index]
            {
                get
                {
                    fixed (uint* ptr = &CpuThreadState.GPR0) return (int) ptr[index];
                }
                set
                {
                    if (index == 0) return;
                    fixed (uint* ptr = &CpuThreadState.GPR0) ptr[index] = (uint) value;
                }
            }
        }

        public class C0rList
        {
            public CpuThreadState CpuThreadState;

            public uint this[int index]
            {
                get
                {
                    fixed (uint* ptr = &CpuThreadState.C0R0) return ptr[index];
                }
                set
                {
                    fixed (uint* ptr = &CpuThreadState.C0R0) ptr[index] = value;
                }
            }
        }

        public class FprList
        {
            public CpuThreadState CpuThreadState;

            public float this[int index]
            {
                get
                {
                    fixed (float* ptr = &CpuThreadState.FPR0) return ptr[index];
                }
                set
                {
                    fixed (float* ptr = &CpuThreadState.FPR0) ptr[index] = value;
                }
            }
        }

        public class FprListInteger
        {
            public CpuThreadState CpuThreadState;

            public int this[int index]
            {
                get
                {
                    fixed (float* ptr = &CpuThreadState.FPR0) return ((int*) ptr)[index];
                }
                set
                {
                    fixed (float* ptr = &CpuThreadState.FPR0) ((int*) ptr)[index] = value;
                }
            }
        }

        public class VfprList
        {
            public CpuThreadState CpuThreadState;

            public float this[int index]
            {
                get
                {
                    fixed (float* ptr = &CpuThreadState.VFR0) return ptr[index];
                }
                set
                {
                    fixed (float* ptr = &CpuThreadState.VFR0) ptr[index] = value;
                }
            }

            public float this[int matrix, int column, int row]
            {
                get => this[VfpuUtils.GetIndexCell(matrix, column, row)];
                set => this[VfpuUtils.GetIndexCell(matrix, column, row)] = value;
            }

            public float[] this[string nameWithSufix]
            {
                get { return VfpuUtils.GetIndices(nameWithSufix).Select(item => this[item]).ToArray(); }
                set
                {
                    var indices = VfpuUtils.GetIndices(nameWithSufix);
                    for (var n = 0; n < value.Length; n++) this[indices[n]] = value[n];
                }
            }

            public float[] this[int size, string name]
            {
                get { return VfpuUtils.GetIndices(size, name).Select(item => this[item]).ToArray(); }
                set
                {
                    var indices = VfpuUtils.GetIndices(size, name);
                    for (var n = 0; n < value.Length; n++) this[indices[n]] = value[n];
                }
            }

            public void ClearAll(float value = 0f)
            {
                for (var n = 0; n < 128; n++) this[n] = value;
            }
        }
    }
}
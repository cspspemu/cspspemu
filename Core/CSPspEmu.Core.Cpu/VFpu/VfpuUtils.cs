using CSharpUtils;
using CSPspEmu.Core.Cpu.VFpu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSPspEmu.Core.Cpu.VFpu
{
    //public class VfpuRegister
    //{
    //	public int Index;
    //	public string Name { }
    //}

    public sealed class VfpuUtils
    {
        public static string GetRegisterName(int Matrix, int Column, int Row)
        {
            return "VFR" + GetIndexCell(Matrix, Column, Row);
        }

        //public static int GetIndexCell(int Matrix, int Column, int Row, bool Transposed)
        //{
        //	if (!Transposed)
        //	{
        //		return GetIndexCell(Matrix, Column, Row);
        //	}
        //	else
        //	{
        //		return GetIndexCell(Matrix, Row, Column);
        //	}
        //}

        public static int GetIndexCell(VfpuCell Cell)
        {
            return GetIndexCell(Cell.Matrix, Cell.Column, Cell.Row);
        }

        public static int GetIndexCell(int Matrix, int Column, int Row)
        {
            //public int S_COLUMN { get { return get(0, 2); } set { set(0, 2, value); } }
            //public int S_MATRIX { get { return get(2, 3); } set { set(2, 3, value); } }
            //public int S_ROW { get { return get(5, 2); } set { set(5, 2, value); } }

            if (Matrix < 0 || Matrix >= 8 || Column < 0 || Column >= 4 || Row < 0 || Row >= 4)
            {
                throw (new InvalidOperationException(String.Format("Matrix: {0}, Column: {1}, Row: {2}", Matrix, Column,
                    Row)));
            }

            //return Row * 16 + Matrix * 4 + Column;
            //return Matrix * 16 + Column * 4 + Row;
            return Matrix * 16 + Row * 4 + Column;
        }

        public static int[] GetIndices(VfpuRegisterInfo RegisterInfo)
        {
            return _GetIndices(RegisterInfo).ToArray();
        }

        public static int GetIndexCell(VfpuRegisterInt Register)
        {
            return GetIndexCell(VfpuRegisterInfo.FromVfpuRegisterInt(VfpuRegisterType.Cell, 1, Register));
        }

        public static int[] GetIndicesVector(int Size, VfpuRegisterInt Register)
        {
            return GetIndicesVector(VfpuRegisterInfo.FromVfpuRegisterInt(VfpuRegisterType.Vector, Size, Register));
        }

        public static int[,] GetIndicesMatrix(int Size, VfpuRegisterInt Register)
        {
            return GetIndicesMatrix(VfpuRegisterInfo.FromVfpuRegisterInt(VfpuRegisterType.Matrix, Size, Register));
        }

        public static int GetIndexCell(VfpuRegisterInfo RegisterInfo)
        {
            return GetIndexCell(RegisterInfo.GetCellOffset(0));
        }

        public static int[] GetIndicesVector(VfpuRegisterInfo RegisterInfo)
        {
            return Enumerable.Range(0, RegisterInfo.Size)
                .Select(Offset => GetIndexCell(RegisterInfo.GetCellOffset(Offset))).ToArray();
        }

        public static int[,] GetIndicesMatrix(VfpuRegisterInfo RegisterInfo)
        {
            var IndicesMatrix = new int[RegisterInfo.Size, RegisterInfo.Size];

            for (int Row = 0; Row < RegisterInfo.Size; Row++)
            {
                for (int Column = 0; Column < RegisterInfo.Size; Column++)
                {
                    IndicesMatrix[Column, Row] = (int) GetIndexCell(RegisterInfo.GetCellOffset(Column, Row));
                }
            }

            return IndicesMatrix;
        }

        private static int[] _GetIndices(VfpuRegisterInfo RegisterInfo)
        {
            switch (RegisterInfo.RegisterType)
            {
                case VfpuRegisterType.Cell: return new[] {(int) GetIndexCell(RegisterInfo)};
                case VfpuRegisterType.Vector: return GetIndicesVector(RegisterInfo);
                case VfpuRegisterType.Matrix: return GetIndicesMatrix(RegisterInfo).Compact();
                default:
                    throw (new NotImplementedException(String.Format("Invalid vfpu registry name {0}('{1}')",
                        RegisterInfo.RegisterIndex, RegisterInfo.Name)));
            }
        }

        public static int GetSizeBySuffix(string NameWithSufix)
        {
            if (NameWithSufix.EndsWith(".s")) return 1;
            if (NameWithSufix.EndsWith(".p")) return 2;
            if (NameWithSufix.EndsWith(".t")) return 3;
            if (NameWithSufix.EndsWith(".q")) return 4;
            throw (new Exception("Register doesn't have sufix"));
        }

        public static int[] GetIndices(string NameWithSufix)
        {
            return GetIndices(GetSizeBySuffix(NameWithSufix), NameWithSufix.Substr(0, -2));
        }

        public static int[] GetIndices(int Size, string Name)
        {
            return GetIndices(VfpuRegisterInfo.Parse(Size, Name));
        }
    }
}
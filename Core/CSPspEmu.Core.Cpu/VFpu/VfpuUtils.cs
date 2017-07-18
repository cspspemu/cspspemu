using System;
using System.Linq;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Cpu.VFpu
{
    //public class VfpuRegister
    //{
    //	public int Index;
    //	public string Name { }
    //}

    public sealed class VfpuUtils
    {
        public static string GetRegisterName(int matrix, int column, int row) =>
            "VFR" + GetIndexCell(matrix, column, row);

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

        public static int GetIndexCell(VfpuCell cell) => GetIndexCell(cell.Matrix, cell.Column, cell.Row);

        public static int GetIndexCell(int matrix, int column, int row)
        {
            //public int S_COLUMN { get { return get(0, 2); } set { set(0, 2, value); } }
            //public int S_MATRIX { get { return get(2, 3); } set { set(2, 3, value); } }
            //public int S_ROW { get { return get(5, 2); } set { set(5, 2, value); } }

            if (matrix < 0 || matrix >= 8 || column < 0 || column >= 4 || row < 0 || row >= 4)
            {
                throw new InvalidOperationException($"Matrix: {matrix}, Column: {column}, Row: {row}");
            }

            //return Row * 16 + Matrix * 4 + Column;
            //return Matrix * 16 + Column * 4 + Row;
            return matrix * 16 + row * 4 + column;
        }

        public static int[] GetIndices(VfpuRegisterInfo registerInfo) => _GetIndices(registerInfo).ToArray();

        public static int GetIndexCell(VfpuRegisterInt register) =>
            GetIndexCell(VfpuRegisterInfo.FromVfpuRegisterInt(VfpuRegisterType.Cell, 1, register));

        public static int[] GetIndicesVector(int size, VfpuRegisterInt register) =>
            GetIndicesVector(VfpuRegisterInfo.FromVfpuRegisterInt(VfpuRegisterType.Vector, size, register));

        public static int[,] GetIndicesMatrix(int size, VfpuRegisterInt register) =>
            GetIndicesMatrix(VfpuRegisterInfo.FromVfpuRegisterInt(VfpuRegisterType.Matrix, size, register));

        public static int GetIndexCell(VfpuRegisterInfo registerInfo) => GetIndexCell(registerInfo.GetCellOffset(0));

        public static int[] GetIndicesVector(VfpuRegisterInfo registerInfo) => Enumerable.Range(0, registerInfo.Size)
            .Select(offset => GetIndexCell(registerInfo.GetCellOffset(offset))).ToArray();

        public static int[,] GetIndicesMatrix(VfpuRegisterInfo registerInfo)
        {
            var indicesMatrix = new int[registerInfo.Size, registerInfo.Size];

            for (var row = 0; row < registerInfo.Size; row++)
            {
                for (var column = 0; column < registerInfo.Size; column++)
                {
                    indicesMatrix[column, row] = GetIndexCell(registerInfo.GetCellOffset(column, row));
                }
            }

            return indicesMatrix;
        }

        private static int[] _GetIndices(VfpuRegisterInfo registerInfo)
        {
            switch (registerInfo.RegisterType)
            {
                case VfpuRegisterType.Cell: return new[] {GetIndexCell(registerInfo)};
                case VfpuRegisterType.Vector: return GetIndicesVector(registerInfo);
                case VfpuRegisterType.Matrix: return GetIndicesMatrix(registerInfo).Compact();
                default:
                    throw (new NotImplementedException(string.Format("Invalid vfpu registry name {0}('{1}')",
                        registerInfo.RegisterIndex, registerInfo.Name)));
            }
        }

        public static int GetSizeBySuffix(string nameWithSufix)
        {
            if (nameWithSufix.EndsWith(".s")) return 1;
            if (nameWithSufix.EndsWith(".p")) return 2;
            if (nameWithSufix.EndsWith(".t")) return 3;
            if (nameWithSufix.EndsWith(".q")) return 4;
            throw (new Exception("Register doesn't have sufix"));
        }

        public static int[] GetIndices(string nameWithSufix) =>
            GetIndices(GetSizeBySuffix(nameWithSufix), nameWithSufix.Substr(0, -2));

        public static int[] GetIndices(int size, string name) => GetIndices(VfpuRegisterInfo.Parse(size, name));
    }
}
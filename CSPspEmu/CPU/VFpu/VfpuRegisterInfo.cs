using System;

namespace CSPspEmu.Core.Cpu.VFpu
{
    public sealed class VfpuRegisterInfo
    {
        public readonly int Size;
        public readonly char Type;
        public readonly int Matrix;
        public readonly int Column;
        public readonly int Row;

        public string Name => $"{NameWithoutSuffix}{Suffix}";

        public string NameWithoutSuffix => $"{Type}{Matrix}{Column}{Row}";

        public string Suffix
        {
            get
            {
                switch (Size)
                {
                    case 1: return ".s";
                    case 2: return ".p";
                    case 3: return ".t";
                    case 4: return ".q";
                    default: return ".?";
                }
            }
        }

        public bool Transposed => Type == 'R' || Type == 'E';

        public VfpuRegisterType RegisterType
        {
            get
            {
                switch (Type)
                {
                    case 'S': return VfpuRegisterType.Cell;
                    case 'C': return VfpuRegisterType.Vector;
                    case 'R': return VfpuRegisterType.Vector;
                    case 'M': return VfpuRegisterType.Matrix;
                    case 'E': return VfpuRegisterType.Matrix;
                    default: return VfpuRegisterType.Cell;
                }
            }
        }

        private void CheckInvalid(string message, bool check)
        {
            if (check) throw (new InvalidOperationException($"Invalid '{message}' : {Name}"));
        }

        public VfpuRegisterInfo(int size, char type, int matrix, int column, int row)
        {
            Size = size;
            Type = type;
            Matrix = matrix;
            Column = column;
            Row = row;

            //Console.Error.WriteLine("{0}", this.Name);

            CheckInvalid("Size", size < 1 || size > 4);
            CheckInvalid("Matrix", matrix < 0 || matrix >= 8);
            CheckInvalid("Row", row < 0 || row >= 4);
            CheckInvalid("Column", column < 0 || column >= 4);
            CheckInvalid("Type", type != 'S' && type != 'R' && type != 'C' && type != 'M' && type != 'E');
            CheckInvalid("Type+Size", type == 'S' && size != 1);
            CheckInvalid("Type+Size", type != 'S' && size == 1);
        }

        public static VfpuRegisterInfo Parse(int size, string name)
        {
            name = name.ToUpperInvariant();
            return new VfpuRegisterInfo(
                size,
                name[0],
                int.Parse(name.Substr(1, 1)),
                int.Parse(name.Substr(2, 1)),
                int.Parse(name.Substr(3, 1))
            );
        }

        public VfpuCell GetCellOffset(int offset) => GetCellOffset(0, offset);

        public VfpuCell GetCellOffset(int columnOffset, int rowOffset) => !Transposed
            ? new VfpuCell(Matrix, Column + columnOffset, Row + rowOffset)
            : new VfpuCell(Matrix, Column + rowOffset, Row + columnOffset);

        private void CheckRegisterType(VfpuRegisterType registerType)
        {
            if (RegisterType != registerType)
                throw new InvalidOperationException($"CheckRegisterType: {RegisterType} != {registerType}");
        }

        public static VfpuRegisterInfo FromVfpuRegisterInt(VfpuRegisterType type, int size, VfpuRegisterInt register)
        {
            if (type == VfpuRegisterType.Vector && size == 1) type = VfpuRegisterType.Cell;
            return Parse(size, VfpuConstants.GetRegisterNameByIndex(type, size, register.Value));
        }

        public VfpuRegisterInt RegisterIndex =>
            VfpuConstants.GetRegisterIndexByName(RegisterType, Size, NameWithoutSuffix);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.VFpu
{
    public sealed class VfpuRegisterInfo
    {
        public readonly int Size;
        public readonly char Type;
        public readonly int Matrix;
        public readonly int Column;
        public readonly int Row;

        public string Name
        {
            get { return String.Format("{0}{1}", NameWithoutSuffix, Suffix); }
        }

        public string NameWithoutSuffix
        {
            get { return String.Format("{0}{1}{2}{3}", Type, Matrix, Column, Row); }
        }

        public string Suffix
        {
            get
            {
                switch (Size)
                {
                    default:
                    case 1: return ".s";
                    case 2: return ".p";
                    case 3: return ".t";
                    case 4: return ".q";
                }
            }
        }

        public bool Transposed
        {
            get { return Type == 'R' || Type == 'E'; }
        }

        public VfpuRegisterType RegisterType
        {
            get
            {
                switch (Type)
                {
                    default:
                    case 'S': return VfpuRegisterType.Cell;
                    case 'C':
                    case 'R': return VfpuRegisterType.Vector;
                    case 'M':
                    case 'E': return VfpuRegisterType.Matrix;
                }
            }
        }

        private void CheckInvalid(string Message, bool Check)
        {
            if (Check) throw (new InvalidOperationException(String.Format("Invalid '{0}' : {1}", Message, this.Name)));
        }

        public VfpuRegisterInfo(int Size, char Type, int Matrix, int Column, int Row)
        {
            this.Size = Size;
            this.Type = Type;
            this.Matrix = Matrix;
            this.Column = Column;
            this.Row = Row;

            //Console.Error.WriteLine("{0}", this.Name);

            CheckInvalid("Size", Size < 1 || Size > 4);
            CheckInvalid("Matrix", Matrix < 0 || Matrix >= 8);
            CheckInvalid("Row", Row < 0 || Row >= 4);
            CheckInvalid("Column", Column < 0 || Column >= 4);
            CheckInvalid("Type", Type != 'S' && Type != 'R' && Type != 'C' && Type != 'M' && Type != 'E');
            CheckInvalid("Type+Size", Type == 'S' && Size != 1);
            CheckInvalid("Type+Size", Type != 'S' && Size == 1);
        }

        public static VfpuRegisterInfo Parse(int Size, string Name)
        {
            Name = Name.ToUpperInvariant();
            return new VfpuRegisterInfo(
                Size,
                Name[0],
                int.Parse(Name.Substr(1, 1)),
                int.Parse(Name.Substr(2, 1)),
                int.Parse(Name.Substr(3, 1))
            );
        }

        public VfpuCell GetCellOffset(int Offset)
        {
            return GetCellOffset(0, Offset);
        }

        public VfpuCell GetCellOffset(int ColumnOffset, int RowOffset)
        {
            return !Transposed
                ? new VfpuCell(Matrix, Column + ColumnOffset, Row + RowOffset)
                : new VfpuCell(Matrix, Column + RowOffset, Row + ColumnOffset);
        }

        private void CheckRegisterType(VfpuRegisterType RegisterType)
        {
            if (this.RegisterType != RegisterType)
            {
                throw (new InvalidOperationException(String.Format("CheckRegisterType: {0} != {1}", this.RegisterType,
                    RegisterType)));
            }
        }

        public static VfpuRegisterInfo FromVfpuRegisterInt(VfpuRegisterType Type, int Size, VfpuRegisterInt Register)
        {
            if (Type == VfpuRegisterType.Vector && Size == 1) Type = VfpuRegisterType.Cell;
            return Parse(Size, VfpuConstants.GetRegisterNameByIndex(Type, Size, Register.Value));
            throw (new NotImplementedException("FromVfpuRegisterInt"));
        }

        public VfpuRegisterInt RegisterIndex
        {
            get { return VfpuConstants.GetRegisterIndexByName(RegisterType, Size, NameWithoutSuffix); }
        }
    }
}
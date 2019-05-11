namespace CSPspEmu.Core.Cpu.VFpu
{
    public class VfpuCell
    {
        public readonly int Matrix;
        public readonly int Column;
        public readonly int Row;

        public VfpuCell(int matrix, int column, int row)
        {
            Matrix = matrix;
            Column = column;
            Row = row;
        }
    }
}
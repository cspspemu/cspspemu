using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.VFpu
{
    public class VfpuCell
    {
        public readonly int Matrix;
        public readonly int Column;
        public readonly int Row;

        public VfpuCell(int Matrix, int Column, int Row)
        {
            this.Matrix = Matrix;
            this.Column = Column;
            this.Row = Row;
        }
    }
}
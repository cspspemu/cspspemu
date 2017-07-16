using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////
        // bc1(f/t)(l): Branch on C1 (False/True) (Likely)
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public AstNodeStm bc1f()
        {
            return AssignBranchFlag(ast.Unary("!", ast.FCR31_CC()));
        }

        public AstNodeStm bc1fl()
        {
            return bc1f();
        }

        public AstNodeStm bc1t()
        {
            return AssignBranchFlag(ast.FCR31_CC());
        }

        public AstNodeStm bc1tl()
        {
            return bc1t();
        }
    }
}
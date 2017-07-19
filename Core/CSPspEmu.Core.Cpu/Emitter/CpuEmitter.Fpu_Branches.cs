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
        public AstNodeStm bc1f() => AssignBranchFlag(_ast.Unary("!", _ast.FCR31_CC()));

        public AstNodeStm bc1fl() => bc1f();
        public AstNodeStm bc1t() => AssignBranchFlag(_ast.FCR31_CC());
        public AstNodeStm bc1tl() => bc1t();
    }
}
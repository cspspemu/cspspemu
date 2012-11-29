using SafeILGenerator.Ast.Nodes;
using System;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
	{
		public AstNodeStm vnop() { return AstNotImplemented(); }
		public AstNodeStm vsync() { return AstNotImplemented(); }
		public AstNodeStm vflush() { return AstNotImplemented(); }

	}
}

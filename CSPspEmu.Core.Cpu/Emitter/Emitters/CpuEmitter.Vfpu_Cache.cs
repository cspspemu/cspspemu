using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
	{
		public AstNodeStm vnop() { return ast.AstNotImplemented("vnop"); }
		public AstNodeStm vsync() { return ast.AstNotImplemented("vsync"); }
		public AstNodeStm vflush() { return ast.AstNotImplemented("vflush"); }
	}
}

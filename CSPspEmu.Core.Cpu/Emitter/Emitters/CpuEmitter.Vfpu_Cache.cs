using SafeILGenerator.Ast.Nodes;

namespace CSPspEmu.Core.Cpu.Emitter
{
    public sealed partial class CpuEmitter
	{
		public AstNodeStm vnop() { return AstNotImplemented("vnop"); }
		public AstNodeStm vsync() { return AstNotImplemented("vsync"); }
		public AstNodeStm vflush() { return AstNotImplemented("vflush"); }
	}
}

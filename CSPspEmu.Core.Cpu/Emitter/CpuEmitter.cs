namespace CSPspEmu.Core.Cpu.Emitter
{
	public sealed partial class CpuEmitter
	{
		public CpuProcessor CpuProcessor;
		private MipsMethodEmitter MipsMethodEmitter;
		private IInstructionReader InstructionReader;
		public Instruction Instruction { private set; get; }
		public uint PC { private set; get; }

		public Instruction LoadAT(uint PC)
		{
			this.PC = PC;
			return this.Instruction = InstructionReader[PC];
		}

		public int RT { get { return Instruction.RT; } }
		public int RD { get { return Instruction.RD; } }
		public int RS { get { return Instruction.RS; } }
		public int IMM { get { return Instruction.IMM; } }
		public uint IMMU { get { return Instruction.IMMU; } }

		public int FT { get { return Instruction.FT; } }
		public int FD { get { return Instruction.FD; } }
		public int FS { get { return Instruction.FS; } }

		public CpuEmitter(MipsMethodEmitter MipsMethodEmitter, IInstructionReader InstructionReader, CpuProcessor CpuProcessor)
		{
			this.MipsMethodEmitter = MipsMethodEmitter;
			this.InstructionReader = InstructionReader;
			this.CpuProcessor = CpuProcessor;
		}
	}
}

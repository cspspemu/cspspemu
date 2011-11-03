using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CSPspEmu.Core.Cpu.Cpu
{
	public class InstructionReader
	{
		protected Stream Stream;
		protected BinaryReader BinaryReader;

		public InstructionReader(Stream Stream)
		{
			this.Stream = Stream;
			this.BinaryReader = new BinaryReader(Stream);
		}

		public Instruction this[uint Index]
		{
			get
			{
				Instruction Instruction;
				Stream.Position = Index;
				Instruction.Value = BinaryReader.ReadUInt32();
				return Instruction;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Cpu
{
	public class InstructionReader
	{
		protected Stream Stream;
		protected BinaryReader BinaryReader;
		protected BinaryWriter BinaryWriter;

		public InstructionReader(Stream Stream)
		{
			this.Stream = Stream;
			this.BinaryReader = new BinaryReader(Stream);
			this.BinaryWriter = new BinaryWriter(Stream);
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
			set
			{
				Stream.Position = Index;
				BinaryWriter.Write((uint)value.Value);
			}
		}
	}
}

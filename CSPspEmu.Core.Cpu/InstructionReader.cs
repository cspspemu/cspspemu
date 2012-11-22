using System.IO;
using CSharpUtils.Arrays;

namespace CSPspEmu.Core.Cpu
{
	public interface IInstructionReader
	{
		Instruction this[uint Index] { get; set; }
		uint EndPC { get; }
	}

	public class InstructionArrayReader : IInstructionReader
	{
		IArray<Instruction> Instructions;

		public InstructionArrayReader(IArray<Instruction> Instructions)
		{
			this.Instructions = Instructions;
		}

		public Instruction this[uint Index]
		{
			get
			{
				return this.Instructions[(int)(Index / 4)];
			}
			set
			{
				this.Instructions[(int)(Index / 4)] = value;
			}
		}

		public uint EndPC
		{
			get { return (uint)((Instructions.Length - 1) * 4); }
		}
	}

	public class InstructionStreamReader : IInstructionReader
	{
		protected Stream Stream;

		public InstructionStreamReader(Stream Stream)
		{
			this.Stream = Stream;
		}

		public Instruction this[uint Index]
		{
			get
			{
				var Instruction = default(Instruction);
				Stream.Position = Index;
				Instruction.Value = Stream.ReadStruct<uint>();
				return Instruction;
			}
			set
			{
				Stream.Position = Index;
				Stream.WriteStruct((uint)value.Value);
			}
		}


		public uint EndPC
		{
			get { return (uint)(Stream.Length - 4); }
		}
	}
}

using CSharpUtils.Arrays;
using System.IO;

namespace CSPspEmu.Core.Cpu
{
    public interface IInstructionReader
    {
        Instruction this[uint index] { get; set; }
        uint EndPc { get; }
    }

    public class InstructionArrayReader : IInstructionReader
    {
        private IArray<Instruction> _instructions;

        public InstructionArrayReader(IArray<Instruction> instructions)
        {
            _instructions = instructions;
        }

        public Instruction this[uint index]
        {
            get => _instructions[(int) (index / 4)];
            set => _instructions[(int) (index / 4)] = value;
        }

        public uint EndPc => (uint) ((_instructions.Length - 1) * 4);
    }

    public class InstructionStreamReader : IInstructionReader
    {
        protected Stream Stream;
        protected BinaryReader BinaryReader;
        protected BinaryWriter BinaryWriter;

        public InstructionStreamReader(Stream stream)
        {
            Stream = stream;
            BinaryReader = new BinaryReader(stream);
            BinaryWriter = new BinaryWriter(stream);
        }

        public Instruction this[uint index]
        {
            get
            {
                var instruction = default(Instruction);
                Stream.Position = index;
                instruction.Value = BinaryReader.ReadUInt32();
                return instruction;
            }
            set
            {
                Stream.Position = index;
                BinaryWriter.Write((uint) value.Value);
            }
        }


        public uint EndPc => (uint) (Stream.Length - 4);
    }
}
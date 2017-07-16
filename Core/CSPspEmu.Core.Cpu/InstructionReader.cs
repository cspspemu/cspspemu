using CSharpUtils.Arrays;
using System.IO;

namespace CSPspEmu.Core.Cpu
{
    public interface IInstructionReader
    {
        Instruction this[uint Index] { get; set; }
        uint EndPC { get; }
    }

    public class InstructionArrayReader : IInstructionReader
    {
        private IArray<Instruction> Instructions;

        public InstructionArrayReader(IArray<Instruction> Instructions)
        {
            this.Instructions = Instructions;
        }

        public Instruction this[uint Index]
        {
            get { return this.Instructions[(int) (Index / 4)]; }
            set { this.Instructions[(int) (Index / 4)] = value; }
        }

        public uint EndPC
        {
            get { return (uint) ((Instructions.Length - 1) * 4); }
        }
    }

    public class InstructionStreamReader : IInstructionReader
    {
        protected Stream Stream;
        protected BinaryReader BinaryReader;
        protected BinaryWriter BinaryWriter;

        public InstructionStreamReader(Stream Stream)
        {
            this.Stream = Stream;
            this.BinaryReader = new BinaryReader(Stream);
            this.BinaryWriter = new BinaryWriter(Stream);
        }

        public Instruction this[uint Index]
        {
            get
            {
                var Instruction = default(Instruction);
                Stream.Position = Index;
                Instruction.Value = BinaryReader.ReadUInt32();
                return Instruction;
            }
            set
            {
                Stream.Position = Index;
                BinaryWriter.Write((uint) value.Value);
            }
        }


        public uint EndPC
        {
            get { return (uint) (Stream.Length - 4); }
        }
    }
}
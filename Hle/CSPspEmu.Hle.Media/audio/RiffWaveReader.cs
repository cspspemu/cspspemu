using CSharpUtils.Streams;
using System;
using System.IO;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats.audio
{
    public class RiffWaveReader
    {
        public event Action<string, SliceStream> HandleChunk;

        public RiffWaveReader()
        {
        }

        public void Parse(Stream Stream)
        {
            ParseFile(Stream);
        }

        private void ParseFile(Stream Stream)
        {
            if (Stream.ReadString(4) != "RIFF") throw (new InvalidDataException("Not a RIFF File"));
            var RiffSize = new BinaryReader(Stream).ReadUInt32();
            var RiffStream = Stream.ReadStream(RiffSize);
            ParseRiff(RiffStream);
        }

        private void ParseRiff(Stream Stream)
        {
            if (Stream.ReadString(4) != "WAVE") throw (new InvalidDataException("Not a RIFF.WAVE File"));
            while (!Stream.Eof())
            {
                var ChunkType = Stream.ReadString(4);
                var ChunkSize = new BinaryReader(Stream).ReadUInt32();
                var ChunkStream = Stream.ReadStream(ChunkSize);
                HandleChunkInternal(ChunkType, ChunkStream);
            }
        }

        private void HandleChunkInternal(string ChunkType, SliceStream ChunkStream)
        {
            if (HandleChunk != null) HandleChunk(ChunkType, ChunkStream);
        }
    }
}
using System;
using System.Collections.Generic;
using CSharpUtils;
using CSharpUtils.Endian;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Hle.Formats.audio
{
    public interface ISoundDecoder
    {
        bool HasMore { get; }
        void Reset();
        StereoShortSoundSample GetNextSample();
        void SetLoopCount(int LoopCount);
    }

    /// <summary>
    /// Based on jpcsp. gid15 work.
    /// http://code.google.com/p/jpcsp/source/browse/trunk/src/jpcsp/sound/SampleSourceVAG.java?r=1995
    /// </summary>
    public sealed unsafe partial class Vag : ISoundDecoder
    {
        //public byte[] Data;
        //public StereoShortSoundSample[] DecodedSamples = new StereoShortSoundSample[0];

        private List<StereoShortSoundSample> DecodedSamples = new List<StereoShortSoundSample>();
        private Decoder SamplesDecoder;
        public bool SamplesDecoderEnd = false;

        bool ISoundDecoder.HasMore
        {
            get { return SamplesDecoder.HasMore; }
        }

        void ISoundDecoder.Reset()
        {
            SamplesDecoder.Reset();
        }

        void ISoundDecoder.SetLoopCount(int LoopCount)
        {
            SamplesDecoder.SetLoopCount(LoopCount);
        }

        StereoShortSoundSample ISoundDecoder.GetNextSample()
        {
            return SamplesDecoder.GetNextSample();
        }

        public StereoShortSoundSample[] GetAllDecodedSamples()
        {
            var Samples = new StereoShortSoundSample[SamplesCount];
            SamplesDecoder.Reset();
            for (int n = 0; n < SamplesCount; n++)
            {
                Samples[n] = SamplesDecoder.GetNextSample();
            }
            return Samples;
        }

        /*
        public bool DecodeSample()
        {
            if (!SamplesDecoderEnd)
            {
                if (SamplesDecoder.HasMore)
                {
                    DecodedSamples.Add(SamplesDecoder.Current);
                }
                else
                {
                }
                if (!SamplesDecoder.MoveNext()) SamplesDecoderEnd = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        */

        public int SamplesCount { get; protected set; }

        public Vag(byte* DataPointer, int DataLength)
        {
            //this.Data = Data;
            var Header = *(Header*) DataPointer;
            if (Header.Magic != 0)
            {
                Console.Error.WriteLine("Error VAG Magic: {0:X}", Header.Magic);
                throw (new NotImplementedException("Invalid VAG header"));
            }
            //var Hash = CSPspEmu.Core.Hashing.FastHash(DataPointer, DataLength);
            //Console.WriteLine("Header.SampleRate: {0}", Header.SampleRate);
            //Console.ReadKey();

            /*
            switch (Header.magic) {
                case "VAG":
                    blocks = cast(Block[])Data[0x30..$];
                break;
                case "\0\0\0":
                    blocks = cast(Block[])Data[0x10..$];
                break;
                default: throw(new Exception("Not a valid VAG File."));
            }
            */

            //ArrayUtils.HexDump(PointerUtils.PointerToByteArray(DataPointer, DataLength), 0xA0);

            SamplesCount = (DataLength - 0x10) * 56 / 16;
            SamplesDecoder = new Decoder((Block*) &DataPointer[0x10], (DataLength - 0x10) / 16);
            //SamplesDecoder = Decoder.DecodeBlocksStream(Blocks).GetEnumerator();

            //SaveToWav("output.wav");
        }

        public void SaveToWav(string FileName)
        {
            var WaveStream = new WaveStream();
            WaveStream.WriteWave(FileName, GetAllDecodedSamples());
        }

        internal sealed class Decoder
        {
            private const int CompressedBytesInBlock = 14;
            private const int DecompressedSamplesInBlock = CompressedBytesInBlock * 2; // 28

            private readonly short[] DecodedBlockSamples = new short[DecompressedSamplesInBlock];

            //private short History1 = 0, History2 = 0;
            private float Predict1, Predict2;

            private Block* BlockPointer;

            private int BlockTotalCount;

            //private int BlockIndex;
            private int SampleIndexInBlock;

            private int SampleIndexInBlock2;

            private bool ReachedEnd;

            //private StereoShortSoundSample CurrentSample;
            //private StereoShortSoundSample LastSample;
            private readonly Stack<State> LoopStack = new Stack<State>(1);

            private State CurrentState;
            private int CurrentLoopCount, TotalLoopCount;

            public struct State
            {
                public int BlockIndex;
                public int History1;
                public int History2;

                public State(int BlockIndex, int History1, int History2)
                {
                    this.BlockIndex = BlockIndex;
                    this.History1 = History1;
                    this.History2 = History2;
                }
            }

            public bool HasMore
            {
                get { return !ReachedEnd; }
            }

            public Decoder(Block* BlockPointer, int BlockTotalCount)
            {
                this.BlockPointer = BlockPointer;
                this.BlockTotalCount = BlockTotalCount;
                Reset();
            }

            public void Reset()
            {
                this.CurrentState = default(State);
                this.SampleIndexInBlock = 0;
                this.SampleIndexInBlock2 = 0;
                this.ReachedEnd = false;
                this.CurrentLoopCount = 0;
            }

            public void SetLoopCount(int LoopCount)
            {
                this.CurrentLoopCount = 0;
                this.TotalLoopCount = LoopCount;
            }

            private void SeekNextBlock()
            {
                if (this.ReachedEnd || this.CurrentState.BlockIndex >= BlockTotalCount)
                {
                    this.ReachedEnd = true;
                    return;
                }

                var Block = BlockPointer[this.CurrentState.BlockIndex++];
                switch (Block.Type)
                {
                    case Vag.Block.TypeEnum.LoopStart:
                    {
                        var CopyState = this.CurrentState;
                        CopyState.BlockIndex--;
                        LoopStack.Push(CopyState);
                    }
                        break;

                    case Vag.Block.TypeEnum.LoopEnd:
                    {
                        if (this.CurrentLoopCount++ < this.TotalLoopCount)
                        {
                            this.CurrentState = LoopStack.Pop();
                        }
                        else
                        {
                            LoopStack.Pop();
                        }
                    }
                        break;

                    case Vag.Block.TypeEnum.End:
                        this.ReachedEnd = true;
                        return;

                    //default:
                    //	//Console.Error.WriteLine("Not implemented: Vag.Block.Type: {0}", Block.Type);
                    //	break;
                }
                DecodeBlock(Block);
            }

            //private readonly int[] GetNextSampleStory = new int[4];

            public StereoShortSoundSample GetNextSample()
            {
                if (this.ReachedEnd) return default(StereoShortSoundSample);

                SampleIndexInBlock %= DecompressedSamplesInBlock;

                if (SampleIndexInBlock == 0)
                {
                    SeekNextBlock();
                }

                if (this.ReachedEnd) return default(StereoShortSoundSample);

                //Console.WriteLine("BlockIndex: {0}, SampleIndexInBlock: {1}, SampleIndexInBlock2: {2}", CurrentState.BlockIndex, SampleIndexInBlock, SampleIndexInBlock2);

                return new StereoShortSoundSample(DecodedBlockSamples[SampleIndexInBlock++]);
            }

            public void DecodeBlock(Block Block)
            {
                int SampleOffset = 0;
                int ShiftFactor = (int) BitUtils.Extract(Block.Modificator, 0, 4);
                int PredictIndex = (int) BitUtils.Extract(Block.Modificator, 4, 4) % Vag.VAG_f.Length;
                //if (predict_nr > VAG_f.length) predict_nr = 0; 

                // @TODO: maybe we can change << 12 >> shift_factor for "<< (12 - shift_factor)"
                // and move the substract outside the for. 

                Predict1 = VAG_f[PredictIndex * 2 + 0];
                Predict2 = VAG_f[PredictIndex * 2 + 1];

                //History1 = 0;
                //History2 = 0;

                // Mono 4-bit/28 Samples per block.
                for (int n = 0; n < CompressedBytesInBlock; n++)
                {
                    var DataByte = Block.Data[n];

                    DecodedBlockSamples[SampleOffset++] =
                        HandleSampleKeepHistory(((short) ((((uint) DataByte >> 0) & 0xF) << 12)) >> ShiftFactor);
                    DecodedBlockSamples[SampleOffset++] =
                        HandleSampleKeepHistory(((short) ((((uint) DataByte >> 4) & 0xF) << 12)) >> ShiftFactor);
                }
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            private short HandleSampleKeepHistory(int UnpackedSample)
            {
                short Sample = HandleSample(UnpackedSample);
                this.CurrentState.History2 = this.CurrentState.History1;
                this.CurrentState.History1 = Sample;
                return Sample;
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            private short HandleSample(int UnpackedSample)
            {
                int Sample = 0;
                Sample += UnpackedSample * 1;
                Sample += (int) (this.CurrentState.History1 * Predict1) / 64;
                Sample += (int) (this.CurrentState.History2 * Predict2) / 64;
                return (short) MathUtils.FastClamp(Sample, short.MinValue, short.MaxValue);
            }
        }

        internal static readonly int[] VAG_f =
        {
            0, 0,
            60, 0,
            115, -52,
            98, -55,
            122, -60,
        };

        public struct Header
        {
            public uint Magic;

            public UintBe VagVersion;
            public UintBe DataSize;
            public UintBe SampleRate;

            public fixed byte Name[16];
        }

        public struct Block
        {
            public enum TypeEnum : byte
            {
                LoopEnd = 3,
                LoopStart = 6,
                End = 7,
            }

            public byte Modificator;
            public TypeEnum Type;
            public fixed byte Data[14];
        }
    }
}
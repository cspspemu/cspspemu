using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Formats.audio
{
	/// <summary>
	/// Based on jpcsp. gid15 work.
	/// http://code.google.com/p/jpcsp/source/browse/trunk/src/jpcsp/sound/SampleSourceVAG.java?r=1995
	/// </summary>
	unsafe public partial class Vag
	{
		//public byte[] Data;
		//public StereoShortSoundSample[] DecodedSamples = new StereoShortSoundSample[0];

		protected List<StereoShortSoundSample> DecodedSamples = new List<StereoShortSoundSample>();
		protected IEnumerator<StereoShortSoundSample> SamplesDecoder;
		public bool SamplesDecoderEnd = false;

		public StereoShortSoundSample GetSampleAt(int Index)
		{
			while (Index >= DecodedSamples.Count)
			{
				if (!DecodeSample())
				{
					return new StereoShortSoundSample();
				}
			}
			return DecodedSamples[Index];
		}

		public StereoShortSoundSample[] GetAllDecodedSamples()
		{
			while (DecodeSample()) ;
			return DecodedSamples.ToArray();
		}

		public bool DecodeSample()
		{
			if (!SamplesDecoderEnd)
			{
				DecodedSamples.Add(SamplesDecoder.Current);
				if (!SamplesDecoder.MoveNext()) SamplesDecoderEnd = true;
				return true;
			}
			else
			{
				return false;
			}
		}

		public int SamplesCount { get; protected set; }

		public Vag()
		{
		}

		public void Load(byte[] Data)
		{
			fixed (byte* DataPointer = Data)
			{
				Load(DataPointer, Data.Length);
			}
		}

		public void Load(byte* DataPointer, int DataLength)
		{
			//this.Data = Data;
			var Header = *(Header*)DataPointer;
			if (Header.Magic != 0)
			{
				Console.Error.WriteLine("Error VAG Magic: {0:X}", Header.Magic);
				throw (new NotImplementedException("Invalid VAG header"));
			}
			var Hash = Hashing.FastHash(DataPointer, DataLength);

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

			var Blocks = PointerUtils.PointerToArray<Block>((Block*)&DataPointer[0x10], DataLength - 0x10);
			SamplesCount = Blocks.Length * 56 / 16;
			SamplesDecoder = Decoder.DecodeBlocksStream(Blocks).GetEnumerator();

			//SaveToWav("output.wav");
		}

		public void SaveToWav(String FileName)
		{
			var WaveStream = new WaveStream();
			WaveStream.WriteWave(FileName, GetAllDecodedSamples());
		}

		internal class Decoder
		{
			protected short[] Samples;
			protected int SampleOffset = 0;
			protected short History1 = 0, History2 = 0;
			protected float Predict1, Predict2;

			static public IEnumerable<StereoShortSoundSample> DecodeBlocksStream(Block[] Blocks)
			{
				var DecodedBlock = new short[28];
				var Decoder = new Decoder();
				int SamplesOffset = 0;
				bool Ended = false;
				var LastSample = default(StereoShortSoundSample);
				var CurrentSample = default(StereoShortSoundSample);
				for (int n = 0; !Ended && (n < Blocks.Length); n++)
				{
					var CurrentBlock = Blocks[n];
					SamplesOffset = 0;
					Decoder.DecodeBlock(CurrentBlock, DecodedBlock, ref SamplesOffset);
					switch (CurrentBlock.Type)
					{
						case Block.TypeEnum.None:
							// 16 bytes = 56 stereo 44100 samples
							foreach (var DecodedMonoSample in DecodedBlock)
							{
								CurrentSample = new StereoShortSoundSample(DecodedMonoSample, DecodedMonoSample);
								yield return StereoShortSoundSample.Mix(LastSample, CurrentSample);
								yield return CurrentSample;
								LastSample = CurrentSample;
							}
							break;
						case Block.TypeEnum.End:
							Ended = true;
							break;
					}
				}
			}

			static public StereoShortSoundSample[] DecodeAllBlocks(Block[] Blocks)
			{
				return DecodeBlocksStream(Blocks).ToArray();
			}

			public bool DecodeBlock(Block Block, short[] Samples, ref int SampleOffset)
			{
				this.Samples = Samples;
				this.SampleOffset = SampleOffset;

				int ShiftFactor = (int)BitUtils.Extract(Block.Modificator, 0, 4);
				int PredictIndex = (int)BitUtils.Extract(Block.Modificator, 4, 4) % Vag.VAG_f.Length;
				//if (predict_nr > VAG_f.length) predict_nr = 0; 

				if (Block.Type == Block.TypeEnum.End) return true;

				// @TODO: maybe we can change << 12 >> shift_factor for "<< (12 - shift_factor)"
				// and move the substract outside the for. 

				Predict1 = VAG_f[PredictIndex][0];
				Predict2 = VAG_f[PredictIndex][1];

				// Mono 4-bit/28 Samples per block.
				for (int n = 0; n < 14; n++)
				{
					var DataByte = Block.Data[n];

					PutSample(((short)(BitUtils.Extract(DataByte, 0, 4) << 12)) >> ShiftFactor);
					PutSample(((short)(BitUtils.Extract(DataByte, 4, 4) << 12)) >> ShiftFactor);
					//PutSample(SampleLeft);
					//PutSample(SampleRight);
				}

				SampleOffset = this.SampleOffset;

				return false;
			}

			protected short HandleSample(int UnpackedSample)
			{
				int Sample = 0;
				Sample += UnpackedSample * 1;
				Sample += (int)(History1 * Predict1);
				Sample += (int)(History2 * Predict2);
				return (short)MathUtils.Clamp<int>(Sample, short.MinValue, short.MaxValue); 
			}

			protected void PutSample(int UnpackedSample)
			{
				short Sample = HandleSample(UnpackedSample);
				History2 = History1;
				History1 = Sample;
				Samples[SampleOffset++] = Sample;
			}
		}
	}
}

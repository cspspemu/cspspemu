using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Audio;

namespace CSPspEmu.Hle.Formats.audio
{
	/// <summary>
	/// Based on jpcsp. gid15 work.
	/// http://code.google.com/p/jpcsp/source/browse/trunk/src/jpcsp/sound/SampleSourceVAG.java?r=1995
	/// </summary>
	unsafe public partial class Vag
	{
		//public byte[] Data;
		public short[] DecodedSamples;

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
			if (Header.Magic != 0) throw(new NotImplementedException("!"));
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
		
			//writefln("%s", this.header.toString);

			this.DecodedSamples = AudioMixer.Convert_Mono22050_Stereo44100(
				Decoder.DecodeBlocks(
					(Block*)&DataPointer[0x10],
					DataLength - 0x10
				)
			);

			SaveToWav("output.wav");
		}

		public void SaveToWav(String FileName)
		{
			var WaveStream = new WaveStream();
			WaveStream.WriteWave(FileName, DecodedSamples);
		}

		internal class Decoder
		{
			protected short[] Samples;
			protected int SampleOffset = 0;
			protected short History1 = 0, History2 = 0;
			protected float Predict1, Predict2;

			static public short[] DecodeBlocks(Block* Blocks, int BlockCount)
			{
				var Data = new short[28 * BlockCount];
				var Decoder = new Decoder();
				int SamplesOffset = 0;
				for (int n = 0; n < BlockCount; n++)
				{
					if (Decoder.DecodeBlock(Blocks[n], Data, ref SamplesOffset))
					{
						//Console.WriteLine("STOP: {0}/{1}", n, BlockCount);
						Data = Data.Slice(0, SamplesOffset);
						break;
					}
				}
				return Data;
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

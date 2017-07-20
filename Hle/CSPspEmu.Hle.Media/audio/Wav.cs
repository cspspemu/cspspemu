using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils.Streams;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Hle.Formats.audio
{
	public class WaveStream
	{
		protected Stream Stream;
		protected BinaryWriter BinaryWriter;

		public WaveStream()
		{
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		protected struct WaveFormat
		{
			/// <summary>
			/// 01 00       - For Uncompressed PCM (linear quntization)
			/// </summary>
			public ushort CompressionCode;
			
			/// <summary>
			/// 02 00       - Stereo
			/// </summary>
			public ushort NumberOfChannels;
			
			/// <summary>
			/// 44 AC 00 00 - 44100
			/// </summary>
			public uint SampleRate;
			
			/// <summary>
			/// Should be on uncompressed PCM : sampleRate * short.sizeof * numberOfChannels 
			/// </summary>
			public uint BytesPerSecond;
			
			/// <summary>
			/// short.sizeof * numberOfChannels
			/// </summary>
			public ushort BlockAlignment;
			
			/// <summary>
			/// ???
			/// </summary>
			public ushort BitsPerSample;

			/// <summary>
			/// 
			/// </summary>
			public ushort Padding;
		}

		protected void WriteChunk(string Name, Action Writer)
		{
			Stream.WriteStringz(Name, 4, Encoding.ASCII);
			BinaryWriter.Write((uint)0);
			var ChunkSizeStream = SliceStream.CreateWithLength(Stream, Stream.Position - 4, 4);
			var BackPosition = Stream.Position;
			{
				Writer();
			}
			var ChunkLength = Stream.Position - BackPosition;
			new BinaryWriter(ChunkSizeStream).Write((uint)ChunkLength);
		}

		public void WriteWave(string FileName, StereoShortSoundSample[] Samples)
		{
			using (var Stream = File.Open(FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			{
				WriteWave(Stream, Samples);
			}
		}

		public void WriteWave(Stream Stream, StereoShortSoundSample[] Samples)
		{
			this.Stream = Stream;
			this.BinaryWriter = new BinaryWriter(Stream);

			WriteChunk("RIFF", () =>
			{
				Stream.WriteStringz("WAVE", 4, Encoding.ASCII);
				WriteChunk("fmt ", () =>
				{
					Stream.WriteStruct(new WaveFormat()
					{
						CompressionCode = 1,
						SampleRate = 44100,
						NumberOfChannels = 2,
						BytesPerSecond = 44100 * sizeof(short) * 2,
						BlockAlignment = sizeof(short) * 2,
						BitsPerSample = 16,
						Padding = 0,
					});
				});
				WriteChunk("data", () =>
				{
					BinaryWriter.Write(PointerUtils.ArrayToByteArray(Samples));
					/*
					foreach (var Sample in Samples)
					{
						BinaryWriter.Write(Sample.Left);
						BinaryWriter.Write(Sample.Right);
					}
					*/
				});
			});
		}
	}
}

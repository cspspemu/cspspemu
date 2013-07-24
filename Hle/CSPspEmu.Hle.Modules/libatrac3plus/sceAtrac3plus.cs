using System;
using System.IO;
using System.Security.Cryptography;
using CSharpUtils;
using CSharpUtils.Arrays;
using CSharpUtils.Endian;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Formats.audio;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Modules.audio;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Formats.audio.At3;
using System.Runtime.InteropServices;
using CSharpUtils.Streams;

namespace CSPspEmu.Hle.Modules.libatrac3plus
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	public unsafe partial class sceAtrac3plus : HleModuleHost
	{
		static Logger Logger = Logger.GetLogger("sceAtrac3plus");

		[Inject]
		public sceAudio sceAudio;

		[Inject]
		public HleMemoryManager HleMemoryManager;

		[Inject]
		InjectContext InjectContext;

		public enum CodecType
		{
			PSP_MODE_AT_3_PLUS = 0x00001000,
			PSP_MODE_AT_3 = 0x00001001,
		}

		public struct OMAHeader
		{
			public uint_be   Magic;
			public ushort_be StructSize;
			public ushort_be Unknown0;
			public uint_be   Unknown1;
			public uint_be   Unknown2;
			public uint_be   Unknown3;
			public uint_be   Unknown4;
			public uint_be   Unknown5;
			public uint_be   Unknown6;

			// Must set from AT3.
			public uint   OmaInfo;

			private fixed byte Pad[60];

			public OMAHeader(uint omaInfo)
			{
				this.Magic = 0x45413301;
				this.StructSize = (ushort)sizeof(OMAHeader);
				this.Unknown0 = unchecked((ushort)(-1));
				this.Unknown1 = 0x00000000;
				this.Unknown2 = 0x010f5000;
				this.Unknown3 = 0x00040000;
				this.Unknown4 = 0x0000f5ce;
				this.Unknown5 = 0xd2929132;
				this.Unknown6 = 0x2480451c;
				this.OmaInfo = omaInfo;
			}

			static public void WriteOma(string FileName, CSPspEmu.Hle.Modules.libatrac3plus.sceAtrac3plus.Atrac.At3FormatStruct Format, Stream DataStream)
			{
				using (var Stream = File.Open(FileName, FileMode.Create, FileAccess.Write))
				{
					Stream.WriteStruct(new OMAHeader(Format.OmaInfo));
					Stream.WriteStream(DataStream);
				}
			}
		}

		[HleUidPoolClass(NotFoundError = SceKernelErrors.ERROR_ATRAC_BAD_ID, FirstItem = 0)]
		public class Atrac : IHleUidPoolClass
		{
			[Inject]
			protected HleMemoryManager HleMemoryManager;

			public At3FormatStruct Format;
			public FactStruct Fact;
			public SmplStruct Smpl;
			public LoopInfoStruct[] LoopInfoList;
			public int MaximumSamples
			{
				get
				{
					switch (CodecType) {
						case sceAtrac3plus.CodecType.PSP_MODE_AT_3_PLUS: return 0x800;
						case sceAtrac3plus.CodecType.PSP_MODE_AT_3: return 0x400;
						default: throw(new NotImplementedException());
					}
				}
			}
			public int BlockSize
			{
				get
				{
					return this.Format.BlockSize;
				}
			}
			public CodecType CodecType;
			public int NumberOfLoops;
			private int _DecodingOffset;
			public int DecodingOffset {
				get {
					return _DecodingOffset;
				}
				set
				{
					_DecodingOffset = value & ~0x7FF;
					DataStream.Position = _DecodingOffset * this.BlockSize / this.MaximumSamples;
				}
			}
			public SliceStream DataStream;
			//public IArray<StereoShortSoundSample> DecodedSamples;
			protected MaiAT3PlusFrameDecoder MaiAT3PlusFrameDecoder;

			public MemoryPartition PrimaryBuffer;
			public int PrimaryBufferReaded;

			public enum CompressionCode : ushort
			{
				Unknown = 0x0000,
				PcmUncompressed = 0x0001,
				MicrosoftAdpcm = 0x0002,
				ItuG711ALaw = 0x0006,
				ItuG711AmLaw = 0x0007,
				ImaAdpcm = 0x0011,
				ItuG723AdpcmYamaha = 0x0016,
				Gsm610 = 0x0031,
				ItuG721Adpcm = 0x0040,
				Mpeg = 0x0050,
				Atrac3 = 0x0270,
				Atrac3Plus = 0xFFFE,
				Experimental = 0xFFFF,
			}

			public struct WavFormatStruct
			{
				/// <summary>
				/// 01 00       - For Uncompressed PCM (linear quntization)
				/// </summary>
				public CompressionCode CompressionCode;

				/// <summary>
				/// 02 00       - Stereo
				/// </summary>
				public ushort AtracChannels;

				/// <summary>
				/// 44 AC 00 00 - 44100
				/// </summary>
				public int Bitrate;

				/// <summary>
				/// Should be on uncompressed PCM : sampleRate * short.sizeof * numberOfChannels
				/// </summary>
				public uint BytesPerSecond;

				/// <summary>
				/// short.sizeof * numberOfChannels
				/// </summary>
				public ushort BlockAlignment;

				static public StreamStructCachedArrayWrapper<StereoShortSoundSample> ParseWavData(Stream Stream)
				{
					StreamStructCachedArrayWrapper<StereoShortSoundSample> DecodedSamples = null;
					var RiffWaveReader = new RiffWaveReader();
					WavFormatStruct WavFormat;
					RiffWaveReader.HandleChunk += (ChunkType, ChunkStream) =>
					{
						switch (ChunkType)
						{
							case "fmt ":
								WavFormat = ChunkStream.ReadStruct<WavFormatStruct>();
								break;
							case "data":
								#if false
									DecodedSamples = new ArrayWrapper<StereoShortSoundSample>(PointerUtils.ByteArrayToArray<StereoShortSoundSample>(ChunkStream.ReadAll()));
								#else
									DecodedSamples = ChunkStream.ConvertToStreamStructCachedArrayWrapper<StereoShortSoundSample>(16 * 1024);
								#endif
								break;
							default:
								throw (new NotImplementedException(String.Format("Can't handle chunk '{0}'", ChunkType)));
						}
					};
					RiffWaveReader.Parse(Stream);
				
					//new WaveStream().WriteWave(@"c:\temp\3.wav", DecodedData);
				
					//Console.WriteLine("DecodedSamples: {0}", DecodedSamples.Length);
					//Console.WriteLine("EndSample: {0}", Fact.EndSample);
					//if (Fact.EndSample == 0)
					//{
					//	Fact.EndSample = DecodedSamples.Length / 2;
					//}
					//Console.ReadKey();

					return DecodedSamples;
				}
			}

			[StructLayout(LayoutKind.Explicit)]
			public struct At3FormatStruct
			{
				/// <summary>
				/// 01 00 - For Uncompressed PCM (linear quntization)
				/// FE FF - For AT3+
				/// </summary>
				[FieldOffset(0x0000)]
				public CompressionCode CompressionCode;

				/// <summary>
				/// 02 00       - Stereo
				/// </summary>
				[FieldOffset(0x0002)]
				public ushort AtracChannels;
				
				/// <summary>
				/// 44 AC 00 00 - 44100
				/// </summary>
				[FieldOffset(0x0004)]
				public uint Bitrate;
				
				/// <summary>
				/// Should be on uncompressed PCM : sampleRate * short.sizeof * numberOfChannels
				/// </summary>
				[FieldOffset(0x0008)]
				public uint AverageBytesPerSecond;
				
				/// <summary>
				/// short.sizeof * numberOfChannels
				/// </summary>
				[FieldOffset(0x000A)]
				public ushort BlockAlignment;
				
				/// <summary>
				/// ???
				/// </summary>
				[FieldOffset(0x000C)]
				public ushort BytesPerFrame;
				
				/// <summary>
				/// ???
				/// </summary>
				[FieldOffset(0x0010)]
				private fixed uint Unknown[6];

				/// <summary>
				/// 
				/// </summary>
				[FieldOffset(0x0028)]
				public uint OmaInfo;

				/// <summary>
				/// 
				/// </summary>
				[FieldOffset(0x0028)]
				private ushort_be _Unk2;

				/// <summary>
				/// 
				/// </summary>
				[FieldOffset(0x002A)]
				private ushort_be _BlockSize;

				/// <summary>
				/// 
				/// </summary>
				public int BlockSize
				{
					get
					{
						return (_BlockSize & 0x3FF) * 8 + 8;
					}
				}
			}

			public struct FactStruct
			{
				/// <summary>
				/// 
				/// </summary>
				public int EndSample;

				/// <summary>
				/// 
				/// </summary>
				public int SampleOffset;
			}

			/// <summary>
			/// Loop Info
			/// </summary>
			public struct SmplStruct
			{
				/// <summary>
				/// 0000 -
				/// </summary>
				private fixed uint Unknown[7];

				/// <summary>
				/// 001C -
				/// </summary>
				public uint LoopCount;

				/// <summary>
				/// 0020 - 
				/// </summary>
				private fixed uint Unknown2[1];
			}

			public struct LoopInfoStruct
			{
				/// <summary>
				/// 0000 -
				/// </summary>
				public uint CuePointID;
				
				/// <summary>
				/// 0004 -
				/// </summary>
				public uint Type;
				
				/// <summary>
				/// 0008 -
				/// </summary>
				public int StartSample;
				
				/// <summary>
				/// 000C -
				/// </summary>
				public int EndSample;
				
				/// <summary>
				/// 0010 -
				/// </summary>
				public uint Fraction;
				
				/// <summary>
				/// 0014 -
				/// </summary>
				public int PlayCount;
			}

			public Atrac(InjectContext InjectContext, CodecType CodecType)
			{
				InjectContext.InjectDependencesTo(this);

				PrimaryBuffer = HleMemoryManager.GetPartition(MemoryPartitions.User).Allocate(1024);

				this.CodecType = CodecType;
			}

			public Atrac(InjectContext InjectContext, byte* Data, int DataLength)
			{
				InjectContext.InjectDependencesTo(this);

				PrimaryBuffer = HleMemoryManager.GetPartition(MemoryPartitions.User).Allocate(1024);

				CodecType = CodecType.PSP_MODE_AT_3_PLUS;
				SetData(Data, DataLength);
			}

			public void SetData(byte* Data, int DataLength)
			{
				ParseAtracData(new UnmanagedMemoryStream(Data, DataLength));
				MaiAT3PlusFrameDecoder = new MaiAT3PlusFrameDecoder();
			}

			private void ParseAtracData(Stream Stream)
			{
				var RiffWaveReader = new RiffWaveReader();
				RiffWaveReader.HandleChunk += (ChunkType, ChunkStream) =>
				{
					switch (ChunkType)
					{
						case "fmt ":
							Format = ChunkStream.ReadStructPartially<At3FormatStruct>();
							break;
						case "fact":
							Fact = ChunkStream.ReadStructPartially<FactStruct>();
							break;
						case "smpl":
							// Loop info
							Smpl = ChunkStream.ReadStructPartially<SmplStruct>();
							LoopInfoList = ChunkStream.ReadStructVector<LoopInfoStruct>(Smpl.LoopCount);
							Console.WriteLine("AT3 smpl: {0}", Smpl.ToStringDefault());
							foreach (var LoopInfo in LoopInfoList) Console.WriteLine("Loop: {0}", LoopInfo.ToStringDefault());
							break;
						case "data":
							this.DataStream = ChunkStream;
							break;
						default:
							throw (new NotImplementedException(String.Format("Can't handle chunk '{0}'", ChunkType)));
					}
				};
				RiffWaveReader.Parse(Stream);
			}

			void IDisposable.Dispose()
			{
				Console.WriteLine("Atrac3+ Dispose");
			}

			public int EndSample
			{
				get
				{
					return Fact.EndSample;
				}
			}

			public bool DecodingReachedEnd
			{
				get
				{
					return RemainingFrames <= 0;
					//return DecodingOffset >= EndSample;
				}
			}

			/*
			public bool DecodeSample()
			{
			}
			*/

			public int GetNumberOfSamplesInNextFrame()
			{
				//Console.Error.WriteLine("*************** {0}, {1}, {2}", EndSample, DecodingOffset, EndSample - DecodingOffset);
				return Math.Min(this.MaximumSamples, EndSample - DecodingOffset);
			}

			public int RemainingFrames
			{
				get
				{
					return (int)(this.DataStream.Available() / this.BlockSize);
				}
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="SamplesOut"></param>
			/// <returns></returns>
			public int Decode(StereoShortSoundSample* SamplesOut)
			{
				//Console.Error.WriteLine("Decode");
				try
				{
					//int Channels = 2;

					//ToReadSamples /= 2;

					var BlockSize = this.Format.BlockSize;
					//this.Data
					int channels;
					short[] buf;

					int rc;

					if (this.DataStream.Available() < BlockSize)
					{
						Console.WriteLine("EndOfData {0} < {1} : {2}, {3}", this.DataStream.Available(), BlockSize, DecodingOffset, EndSample);
						return 0;
					}

					var Data = new byte[BlockSize];
					this.DataStream.Read(Data, 0, Data.Length);

					fixed (byte* DataPtr = Data)
					{
						if ((rc = this.MaiAT3PlusFrameDecoder.decodeFrame(DataPtr, BlockSize, out channels, out buf)) != 0)
						{
							Console.WriteLine("MaiAT3PlusFrameDecoder.decodeFrame: {0}", rc);
							return 0;
						}

						int DecodedSamples = this.MaximumSamples;
						int DecodedSamplesChannels = DecodedSamples * channels;
						_DecodingOffset += DecodedSamples;

						fixed (short* buf_ptr = buf)
						{
							for (int n = 0; n < DecodedSamplesChannels; n += channels)
							{
								SamplesOut->Left = buf_ptr[n + 0];
								SamplesOut->Right = buf_ptr[n + 1];
								SamplesOut++;
							}
						}

						return DecodedSamples;
					}
				}
				catch 
				{
					Console.Error.WriteLine("Error Atrac3.Decode");
					return 0;
				}
			}
		}

		/// <summary>
		/// Creates a new Atrac ID from the specified data
		/// </summary>
		/// <param name="DataPointer">The buffer holding the atrac3 data, including the RIFF/WAVE header.</param>
		/// <param name="DataLength">The size of the buffer pointed by buf</param>
		/// <returns>The new atrac ID, or less than 0 on error </returns>
		[HlePspFunction(NID = 0x7A20E7AF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public Atrac sceAtracSetDataAndGetID(byte* DataPointer, int DataLength)
		{
			//var Data = ArrayUtils.CreateArray<byte>(DataPointer, DataLength);
			return new Atrac(InjectContext, DataPointer, DataLength);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="OutputChannel"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB3B5D042, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetOutputChannel(CpuThreadState CpuThreadState, Atrac Atrac, out int OutputChannel)
		{
			OutputChannel = sceAudio.sceAudioChReserve(CpuThreadState, -1, Atrac.MaximumSamples, PspAudio.FormatEnum.Stereo);
			//Console.WriteLine("{0}", *OutputChannelPointer); Console.ReadKey();
			return 0;
		}

		/// <summary>
		/// Gets the bitrate.
		/// </summary>
		/// <param name="AtracId">The atracID</param>
		/// <param name="Bitrate">Pointer to a integer that receives the bitrate in kbps</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0xA554A158, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetBitrate(Atrac Atrac, out uint Bitrate)
		{
			//Bitrate = Atrac.Format.Bitrate;
			uint _AtracBitrate = (uint)((Atrac.Format.BytesPerFrame * 352800) / 1000);
			if (Atrac.CodecType == CodecType.PSP_MODE_AT_3_PLUS)
			{
				_AtracBitrate = ((_AtracBitrate >> 11) + 8) & 0xFFFFFFF0;
			}
			else
			{
				_AtracBitrate = (_AtracBitrate + 511) >> 10;
			}
			Bitrate = _AtracBitrate;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Atrac"></param>
		/// <param name="BufferPointer"></param>
		/// <param name="BufferSizeInBytes"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x0E2A73AB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetData(Atrac Atrac, byte* BufferPointer, int BufferSizeInBytes)
		{
			Atrac.SetData(BufferPointer, BufferSizeInBytes);
			return 0;
		}

		/// <summary>
		/// Gets the maximum number of samples of the atrac3 stream.
		/// </summary>
		/// <param name="Atrac">The atrac ID</param>
		/// <param name="MaxNumberOfSamples">Pointer to a integer that receives the maximum number of samples.</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0xD6A5F2F7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetMaxSample(Atrac Atrac, out int MaxNumberOfSamples)
		{
			MaxNumberOfSamples = Atrac.MaximumSamples;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="piLoopNum"></param>
		/// <param name="puiLoopStatus"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xFAA4F89B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetLoopStatus(Atrac Atrac, out int piLoopNum, out uint puiLoopStatus)
		{
			piLoopNum = 0;
			puiLoopStatus = 0;
			return 0;
		}

		/// <summary>
		/// Sets the number of loops for this atrac ID
		/// </summary>
		/// <param name="Atrac">The atracID</param>
		/// <param name="NumberOfLoops">
		///		The number of loops to set (0 means play it one time, 1 means play it twice, 2 means play it three times, ...)
		///		-1 means play it forever
		/// </param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x868120B5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetLoopNum(Atrac Atrac, int NumberOfLoops)
		{
			if (Atrac.Smpl.LoopCount == 0) throw(new SceKernelException(SceKernelErrors.ATRAC_ERROR_UNSET_PARAM));
			Atrac.NumberOfLoops = NumberOfLoops;
			return 0;
		}

		/// <summary>
		/// Gets the remaining (not decoded) number of frames
		/// </summary>
		/// <param name="Atrac">The atrac ID</param>
		/// <param name="RemainFramePointer">
		///		Pointer to a integer that receives either -1 if all at3 data is already on memory, 
		///		or the remaining (not decoded yet) frames at memory if not all at3 data is on memory 
		/// </param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x9AE849A7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetRemainFrame(Atrac Atrac, out int RemainFramePointer)
		{
			RemainFramePointer = Atrac.RemainingFrames;
			return 0;
		}

		/// <summary>
		/// Decode a frame of data. 
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="SamplesOut">pointer to a buffer that receives the decoded data of the current frame</param>
		/// <param name="DecodedSamples">pointer to a integer that receives the number of audio samples of the decoded frame</param>
		/// <param name="ReachedEnd">pointer to a integer that receives a boolean value indicating if the decoded frame is the last one</param>
		/// <param name="RemainingFramesToDecode">
		///		pointer to a integer that receives either -1 if all at3 data is already on memory, 
		///		or the remaining (not decoded yet) frames at memory if not all at3 data is on memory
		/// </param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x6A8C3CD5, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceAtracDecodeData(Atrac Atrac, StereoShortSoundSample* SamplesOut, int* DecodedSamples, int* ReachedEnd, int* RemainingFramesToDecode)
		{
			if (SamplesOut == null) return -1;
			int* Temp = stackalloc int[1];
			if (DecodedSamples == null) DecodedSamples = Temp;
			if (ReachedEnd == null) ReachedEnd = Temp;
			if (RemainingFramesToDecode == null) RemainingFramesToDecode = Temp;
			return _sceAtracDecodeData(Atrac, SamplesOut, out *DecodedSamples, out *ReachedEnd, out *RemainingFramesToDecode);
		}

		private int _sceAtracDecodeData(Atrac Atrac, StereoShortSoundSample* SamplesOut, out int DecodedSamples, out int ReachedEnd, out int RemainingFramesToDecode)
		{
			// Decode
			DecodedSamples = Atrac.Decode(SamplesOut);
			ReachedEnd = 0;
			RemainingFramesToDecode = Atrac.RemainingFrames;

			//Console.WriteLine("{0}/{1} -> {2} : {3}", Atrac.DecodingOffsetInSamples, Atrac.TotalSamples, DecodedSamples, Atrac.DecodingReachedEnd);


			if (Atrac.DecodingReachedEnd)
			{
				if (Atrac.NumberOfLoops == 0)
				{
					DecodedSamples = 0;
					ReachedEnd = 1;
					RemainingFramesToDecode = 0;
					Console.WriteLine("SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED)");
					throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED));
				}
				if (Atrac.NumberOfLoops > 0) Atrac.NumberOfLoops--;

				Atrac.DecodingOffset = (Atrac.LoopInfoList.Length > 0) ? Atrac.LoopInfoList[0].StartSample : 0;
			}

			//return Atrac.GetUidIndex(InjectContext);
			return 0;
		}

		/// <summary>
		/// It releases an atrac ID
		/// </summary>
		/// <param name="AtracId">The atrac ID to release</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x61EB33F5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracReleaseAtracID(Atrac Atrac)
		{
			Atrac.RemoveUid(InjectContext);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CodecType"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x780F88D1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public Atrac sceAtracGetAtracID(CodecType CodecType)
		{
			return new Atrac(InjectContext, CodecType);
		}

		/// <summary>
		/// Gets the number of samples of the next frame to be decoded.
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="NumberOfSamplesInNextFrame">Pointer to receives the number of samples of the next frame.</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x36FAABFB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetNextSample(Atrac Atrac, out int NumberOfSamplesInNextFrame)
		{
			NumberOfSamplesInNextFrame = 0;
			try
			{
				//Console.Error.WriteLine(Atrac.Format.ToStringDefault());
				NumberOfSamplesInNextFrame = Atrac.GetNumberOfSamplesInNextFrame();
				return 0;
			}
			catch (Exception Exception)
			{
				NumberOfSamplesInNextFrame = 1;
				Console.Error.WriteLine(Exception);
				return 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="ErrorResult"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE88F759B, FirmwareVersion = 150)]
		[HlePspNotImplemented(Notice = false)]
		public int sceAtracGetInternalErrorInfo(Atrac Atrac, out int ErrorResult)
		{
			ErrorResult = 0;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="WritePointerPointer">Pointer to where to read the atrac data</param>
		/// <param name="AvailableBytes">Number of bytes available at the writePointer location</param>
		/// <param name="ReadOffset">Offset where to seek into the atrac file before reading</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x5D268707, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetStreamDataInfo(Atrac Atrac, out PspPointer WritePointerPointer, out int AvailableBytes, out int ReadOffset)
		{
			//throw (new NotImplementedException());
			WritePointerPointer = Atrac.PrimaryBuffer.Low; // @FIXME!!
			AvailableBytes = Atrac.PrimaryBuffer.Size;
			ReadOffset = Atrac.PrimaryBufferReaded;

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="bytesToAdd">Number of bytes read into location given by sceAtracGetStreamDataInfo().</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x7DB31251, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracAddStreamData(Atrac Atrac, int bytesToAdd)
		{
			Atrac.PrimaryBufferReaded += bytesToAdd;
			//throw (new NotImplementedException());
			/*
			unimplemented();

			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);

			logInfo("sceAtracAddStreamData(%d, %d)", atracID, bytesToAdd);

			*/
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="puiPosition"></param>
		/// <param name="puiDataByte"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x83E85EA0, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetSecondBufferInfo(Atrac Atrac, out uint puiPosition, out uint puiDataByte)
		{
			//throw (new NotImplementedException());
			puiPosition = 0;
			puiDataByte = 0;

			throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_SECOND_BUFFER_NOT_NEEDED));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <returns>0 - not needed ; 1 - needed</returns>
		[HlePspFunction(NID = 0xECA32A99, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracIsSecondBufferNeeded(Atrac Atrac)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="pucSecondBufferAddr"></param>
		/// <param name="uiSecondBufferByte"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x83BF7AFD, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetSecondBuffer(Atrac Atrac, out byte pucSecondBufferAddr, uint uiSecondBufferByte)
		{
			throw (new NotImplementedException());
			/*
			unimplemented();
			//unimplemented();
			return 0;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="SamplePosition"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE23E3A35, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceAtracGetNextDecodePosition(Atrac Atrac, out int SamplePosition)
		{
			if (Atrac.DecodingReachedEnd) throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED));
			SamplePosition = Atrac.DecodingOffset;
			//Console.WriteLine("  {0}", SamplePosition);
			return 0;
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="AtracId"></param>
	    /// <param name="EndSamplePointer"></param>
	    /// <param name="LoopStartSamplePointer"></param>
	    /// <param name="LoopEndSamplePointer"></param>
	    /// <returns></returns>
	    [HlePspFunction(NID = 0xA2BBA8BE, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetSoundSample(Atrac Atrac, int* EndSamplePointer, int* LoopStartSamplePointer, int* LoopEndSamplePointer)
		{
			var HasLoops = (Atrac.LoopInfoList != null) && (Atrac.LoopInfoList.Length > 0);
			if (EndSamplePointer != null) *EndSamplePointer = Atrac.Fact.EndSample;
			if (LoopStartSamplePointer != null) *LoopStartSamplePointer = HasLoops ? Atrac.LoopInfoList[0].StartSample : -1;
			if (LoopEndSamplePointer != null) *LoopEndSamplePointer = HasLoops ? Atrac.LoopInfoList[0].EndSample : -1;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="uiSample"></param>
		/// <param name="pBufferInfo"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xCA3CA3D2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetBufferInfoForReseting(Atrac Atrac, uint uiSample, PspBufferInfo* pBufferInfo)
		{
			throw (new NotImplementedException());
			/*
			unimplemented();
			return 0;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="uiSample"></param>
		/// <param name="uiWriteByteFirstBuf"></param>
		/// <param name="uiWriteByteSecondBuf"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x644E5607, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracResetPlayPosition(Atrac Atrac, uint uiSample, uint uiWriteByteFirstBuf, uint uiWriteByteSecondBuf)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="uiSample"></param>
		/// <param name="BufferInfoAddr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x2DD3E298, FirmwareVersion = 250)]
		[HlePspNotImplemented]
		public int sceAtracGetBufferInfoForResetting(Atrac Atrac, uint uiSample, void* BufferInfoAddr)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Get Number of channels of the Atrac3 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="Channels"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x31668baa, FirmwareVersion = 250)]
		//[HlePspNotImplemented]
		public int sceAtracGetChannel(Atrac Atrac, out int Channels)
		{
			Channels = Atrac.Format.AtracChannels;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="at3Count"></param>
		/// <param name="at3plusCount"></param>
		[HlePspFunction(NID = 0x132F1ECA, FirmwareVersion = 250)]
		[HlePspNotImplemented]
		public int sceAtracReinit(int at3Count, int at3plusCount)
		{
			throw (new SceKernelException((SceKernelErrors)(-1)));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x5CF9D852, FirmwareVersion = 250)]
		[HlePspNotImplemented]
		public int sceAtracSetMOutHalfwayBuffer()
		{
			throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x0FAE370E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetHalfwayBufferAndGetID(uint HalfBufferPointer, uint ReadSize, uint HalfBufferSize)
		{
			//throw (new NotImplementedException());
			return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [HlePspFunction(NID = 0xD5C28CC0, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceAtracEndEntry()
        {
            return 0;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [HlePspFunction(NID = 0xD1F59FDB, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceAtracStartEntry()
        {
            return 0;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="halfBuffer"></param>
		/// <param name="readSize"></param>
		/// <param name="halfBufferSize"></param>
		/// <returns></returns>
        [HlePspFunction(NID = 0x9CD7DE03, FirmwareVersion = 250)]
        [HlePspNotImplemented]
		public int sceAtracSetMOutHalfwayBufferAndGetID(void* halfBuffer, uint readSize, uint halfBufferSize)
        {
			throw (new SceKernelException((SceKernelErrors)(-1)));
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="halfBuffer"></param>
		/// <param name="readSize"></param>
		/// <param name="halfBufferSize"></param>
		/// <returns></returns>
        [HlePspFunction(NID = 0x5DD66588, FirmwareVersion = 250)]
        [HlePspNotImplemented]
		public int sceAtracSetAA3HalfwayBufferAndGetID(void* halfBuffer, uint readSize, uint halfBufferSize)
		{
			throw (new SceKernelException((SceKernelErrors)(-1)));
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="bufferSize"></param>
		/// <param name="fileSize"></param>
		/// <param name="metadataSizeAddr"></param>
		/// <returns></returns>
        [HlePspFunction(NID = 0x5622B7C1, FirmwareVersion = 250)]
        [HlePspNotImplemented]
		public Atrac sceAtracSetAA3DataAndGetID(void* buffer, int bufferSize, int fileSize, uint metadataSizeAddr)
        {
			throw (new SceKernelException((SceKernelErrors)(-1)));
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Atrac"></param>
		/// <param name="halfBuffer"></param>
		/// <param name="readSize"></param>
		/// <param name="halfBufferSize"></param>
		/// <returns></returns>
        [HlePspFunction(NID = 0x3F6E26B5, FirmwareVersion = 150)]
        [HlePspNotImplemented]
		public int sceAtracSetHalfwayBuffer(Atrac Atrac, void* halfBuffer, uint readSize, uint halfBufferSize)
        {
			//throw (new NotImplementedException());
            return 0;
        }
	}
}

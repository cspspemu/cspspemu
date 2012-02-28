using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Modules.threadman;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Managers;
using System.IO;
using System.Runtime.InteropServices;
using AForge.Video.DirectShow.Internals;
using CSPspEmu.Media;
using CSharpUtils.Endian;
using CSPspEmu.Hle.Formats.audio;
using CSPspEmu.Hle.Modules.audio;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core;
using System.Security.Cryptography;
using CSPspEmu.Hle.Vfs.MemoryStick;
using CSPspEmu.Hle.Vfs.Local;
using System.Diagnostics;

namespace CSPspEmu.Hle.Modules.libatrac3plus
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceAtrac3plus : HleModuleHost
	{
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
		}

		public class Atrac : IDisposable
		{
			public byte[] Data;
			public At3FormatStruct Format;
			public FactStruct Fact;
			public SmplStruct Smpl;
			public int MaximumSamples
			{
				get
				{
					switch (CodecType) {
						case sceAtrac3plus.CodecType.PSP_MODE_AT_3_PLUS: return 2048;
						case sceAtrac3plus.CodecType.PSP_MODE_AT_3: return 1024;
						default: throw(new NotImplementedException());
					}
				}
			}
			public CodecType CodecType;
			public int NumberOfLoops;
			public int DecodingOffsetInSamples;
			public Stream DataStream;
			public short[] DecodedData;

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
				public uint Bitrate;

				/// <summary>
				/// Should be on uncompressed PCM : sampleRate * short.sizeof * numberOfChannels
				/// </summary>
				public uint BytesPerSecond;

				/// <summary>
				/// short.sizeof * numberOfChannels
				/// </summary>
				public ushort BlockAlignment;
			}

			public struct At3FormatStruct
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
				public uint Bitrate;
				
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
				public ushort BytesPerSample;
				
				/// <summary>
				/// ???
				/// </summary>
				public fixed uint Unknown[6];
				
				/// <summary>
				/// Information that will be copied to the OMA Header.
				/// </summary>
				public uint OmaInfo;
			}

			public struct FactStruct
			{
				public int EndSample;
				public int SampleOffset;
			}

			public struct SmplStruct
			{
			}

			HleState HleState;

			public Atrac(HleState HleState, CodecType CodecType)
			{
				this.HleState = HleState;
				this.CodecType = CodecType;
			}

			public Atrac(HleState HleState, byte[] Data)
			{
				this.HleState = HleState;
				CodecType = CodecType.PSP_MODE_AT_3_PLUS;
				SetData(Data);
			}

			public void SetData(byte[] Data)
			{
				this.Data = Data;

				var DataHash = SHA1.Create().ComputeHash(Data);

				//var Ms0Path = new DirectoryInfo(HleState.MemoryStickRootLocalFolder).FullName;
				var Ms0Path = new DirectoryInfo(ApplicationPaths.MemoryStickRootFolder).FullName;
				try { Directory.CreateDirectory(Ms0Path + "\\temp"); } catch { }

				var BaseFileName = Ms0Path + "\\temp\\" + BitConverter.ToString(DataHash);

				var OmaOutFileName = BaseFileName + ".oma";
				var WavOutFileName = BaseFileName + ".wav";

				if (!File.Exists(WavOutFileName))
				{
					//ArrayUtils.HexDump(Data, 1024);
					//Console.ReadKey();

					Debug.WriteLine("{0} -> {1}", OmaOutFileName, WavOutFileName);

					Debug.WriteLine("[a]");
					ParseAtracData(new MemoryStream(Data));
					{

						WriteOma(OmaOutFileName);
						Debug.WriteLine("[aa]");
						File.Delete(WavOutFileName);
						OmaWavConverter.convertOmaToWav(OmaOutFileName, WavOutFileName);
					}
					Debug.WriteLine("[b]");
				}
				try
				{
					ParseWavData(File.OpenRead(WavOutFileName));
				}
				catch
				{
					DecodedData = new short[0];
				}
				Debug.WriteLine("[c]");
			}

			private void ParseWavData(Stream Stream)
			{
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
							var Data = ChunkStream.ReadAll();
							DecodedData = new short[Data.Length / 2];
							Buffer.BlockCopy(Data, 0, DecodedData, 0, Data.Length);
							break;
						default:
							throw (new NotImplementedException(String.Format("Can't handle chunk '{0}'", ChunkType)));
					}
				};
				RiffWaveReader.Parse(Stream);

				//new WaveStream().WriteWave(@"c:\temp\3.wav", DecodedData);

				Console.WriteLine("DecodedSamples: {0}", DecodedData.Length);
				Console.WriteLine("EndSample: {0}", Fact.EndSample);
				//Console.ReadKey();
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

			public void WriteOma(string FileName)
			{
				using (var Stream = File.Open(FileName, FileMode.Create, FileAccess.Write))
				{
					Stream.WriteStruct(new OMAHeader(Format.OmaInfo));
					Stream.WriteStream(DataStream);
				}
			}

			void IDisposable.Dispose()
			{
			}

			public int TotalSamples
			{
				get
				{
					return DecodedData.Length / NumberOfChannels;
				}
			}

			public bool DecodingReachedEnd
			{
				get
				{
					return DecodingOffsetInSamples >= TotalSamples;
				}
			}

			public int NumberOfChannels
			{
				get
				{
					return 2;
				}
			}
			/*
			public bool DecodeSample()
			{
			}
			*/

			public int GetNumberOfSamplesInNextFrame()
			{
				return Math.Min(MaximumSamples, TotalSamples - DecodingOffsetInSamples);
			}

			//List<short> Temp = new List<short>();
			public int Decode(short* SamplesOut)
			{
				//int Channels = 2;

				//ToReadSamples /= 2;

				int StartDecodingOffsetInSamples = DecodingOffsetInSamples;

				for (int n = 0; n < MaximumSamples; n++)
				{
					if (DecodingReachedEnd)
					{
						break;
					}
					SamplesOut[n * NumberOfChannels + 0] = DecodedData[DecodingOffsetInSamples * NumberOfChannels + 0];
					SamplesOut[n * NumberOfChannels + 1] = DecodedData[DecodingOffsetInSamples * NumberOfChannels + 1];
					DecodingOffsetInSamples++;
				}
				/*
				if (Temp.Count > 90000)
				{
					new WaveStream().WriteWave(@"c:\temp\4.wav", Temp.ToArray());
					Console.ReadKey();
				}
				*/
				return DecodingOffsetInSamples - StartDecodingOffsetInSamples;
			}
		}

		HleUidPool<Atrac> AtracList = new HleUidPool<Atrac>();

		/// <summary>
		/// Creates a new Atrac ID from the specified data
		/// </summary>
		/// <param name="DataPointer">the buffer holding the atrac3 data, including the RIFF/WAVE header.</param>
		/// <param name="DataLength">the size of the buffer pointed by buf</param>
		/// <returns>The new atrac ID, or less than 0 on error </returns>
		[HlePspFunction(NID = 0x7A20E7AF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetDataAndGetID(byte* DataPointer, int DataLength)
		{
			var Data = ArrayUtils.CreateArray<byte>(DataPointer, DataLength);
			var Atrac = new Atrac(HleState, Data);
			var AtracId = AtracList.Create(Atrac);
			return AtracId;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="ChannelPointer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB3B5D042, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetOutputChannel(int AtracId, out int OutputChannel)
		{
			//throw(new NotImplementedException());
			var Atrac = AtracList.Get(AtracId);
			OutputChannel = HleState.ModuleManager.GetModule<sceAudio>().sceAudioChReserve(-1, Atrac.MaximumSamples, PspAudio.FormatEnum.Stereo);
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
		public int sceAtracGetBitrate(int AtracId, out uint Bitrate)
		{
			var Atrac = AtracList.Get(AtracId);
			Bitrate = Atrac.Format.Bitrate;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="BufferPointer"></param>
		/// <param name="BufferSizeInBytes"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x0E2A73AB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetData(int AtracId, byte* BufferPointer, int BufferSizeInBytes)
		{
			var Atrac = AtracList.Get(AtracId);
			Atrac.SetData(ArrayUtils.CreateArray<byte>(BufferPointer, BufferSizeInBytes));
			return 0;
		}

		/// <summary>
		/// Gets the maximum number of samples of the atrac3 stream.
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="MaxNumberOfSamples">Pointer to a integer that receives the maximum number of samples.</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0xD6A5F2F7, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceAtracGetMaxSample(int AtracId, out int MaxNumberOfSamples)
		{
			var Atrac = AtracList.Get(AtracId);
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
		public int sceAtracGetLoopStatus(int AtracId, out int piLoopNum, out uint puiLoopStatus)
		{
			throw (new NotImplementedException());
			/*
			unimplemented();
			return 0;
			*/
		}

		/// <summary>
		/// Sets the number of loops for this atrac ID
		/// </summary>
		/// <param name="AtracId">The atracID</param>
		/// <param name="NumberOfLoops">
		///		The number of loops to set (0 means play it one time, 1 means play it twice, 2 means play it three times, ...)
		///		-1 means play it forever
		/// </param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x868120B5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetLoopNum(int AtracId, int NumberOfLoops)
		{
			var Atrac = AtracList.Get(AtracId);
			Atrac.NumberOfLoops = NumberOfLoops;
			return 0;
		}

		/// <summary>
		/// Gets the remaining (not decoded) number of frames
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="RemainFramePointer">
		///		Pointer to a integer that receives either -1 if all at3 data is already on memory, 
		///		or the remaining (not decoded yet) frames at memory if not all at3 data is on memory 
		/// </param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x9AE849A7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetRemainFrame(int AtracId, out int RemainFramePointer)
		{
			var Atrac = AtracList.Get(AtracId);
			RemainFramePointer = -1;
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
		public int sceAtracDecodeData(int AtracId, short* SamplesOut, out int DecodedSamples, out int ReachedEnd, out int RemainingFramesToDecode)
		{
			var Atrac = AtracList.Get(AtracId);
			
			// Decode
			DecodedSamples = Atrac.Decode(SamplesOut);

			//Console.WriteLine("{0}/{1} -> {2} : {3}", Atrac.DecodingOffsetInSamples, Atrac.TotalSamples, DecodedSamples, Atrac.DecodingReachedEnd);
			
			RemainingFramesToDecode = -1;

			if (Atrac.DecodingReachedEnd)
			{
				ReachedEnd = -1;
				throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED));
			}
			else
			{
				ReachedEnd = 0;
				return 0;
			}
		}

		/// <summary>
		/// It releases an atrac ID
		/// </summary>
		/// <param name="AtracId">The atrac ID to release</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x61EB33F5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracReleaseAtracID(int AtracId)
		{
			AtracList.Remove(AtracId);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CodecType"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x780F88D1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetAtracID(CodecType CodecType)
		{
			var Atrac = new Atrac(HleState, CodecType);
			var AtracId = AtracList.Create(Atrac);
			return AtracId;
		}

		/// <summary>
		/// Gets the number of samples of the next frame to be decoded.
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="NumberOfSamplesInNextFrame">Pointer to receives the number of samples of the next frame.</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x36FAABFB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetNextSample(int AtracId, out int NumberOfSamplesInNextFrame)
		{
			var Atrac = AtracList.Get(AtracId);
			NumberOfSamplesInNextFrame = Atrac.GetNumberOfSamplesInNextFrame();
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="ErrorResult"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE88F759B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetInternalErrorInfo(int AtracId, out int ErrorResult)
		{
			ErrorResult = 0;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="writePointer">Pointer to where to read the atrac data</param>
		/// <param name="availableBytes">Number of bytes available at the writePointer location</param>
		/// <param name="readOffset">Offset where to seek into the atrac file before reading</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x5D268707, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetStreamDataInfo(int AtracId, uint* writePointerPointer /*u8** writePointer*/, out uint availableBytes, out uint readOffset)
		{
			//throw (new NotImplementedException());
			*writePointerPointer = 0; // @FIXME!!
			availableBytes = 0;
			readOffset     = 0;

			return -1;
			//throw(new NotImplementedException());
			/*
			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);
		
			unimplemented();
		
			*writePointer   = cast(u8*)atrac3Object.writeBufferGuestPtr; // @FIXME!!
			*availableBytes = cast(uint)atrac3Object.writeBufferSize;
			*readOffset     = atrac3Object.processor.dataOffset;
		
			logInfo(
				"sceAtracGetStreamDataInfo(atracID=%d, writePointer=%08X, availableBytes=%d, readOffset=%d)",
				atracID, cast(uint)*writePointer, *availableBytes, *readOffset
			);

			return 0;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="bytesToAdd">Number of bytes read into location given by sceAtracGetStreamDataInfo().</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x7DB31251, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracAddStreamData(int AtracId, int bytesToAdd)
		{
			throw (new NotImplementedException());
			/*
			unimplemented();

			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);

			logInfo("sceAtracAddStreamData(%d, %d)", atracID, bytesToAdd);

			return 0;
			*/
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
		public int sceAtracGetSecondBufferInfo(int AtracId, out uint puiPosition, out uint puiDataByte)
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
		/// <param name="pucSecondBufferAddr"></param>
		/// <param name="uiSecondBufferByte"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x83BF7AFD, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetSecondBuffer(int AtracId, out byte pucSecondBufferAddr, uint uiSecondBufferByte)
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
		/// <param name="SamplePositionPointer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE23E3A35, FirmwareVersion = 150)]
		public int sceAtracGetNextDecodePosition(int AtracId, out int SamplePositionPointer)
		{
			var Atrac = AtracList.Get(AtracId);
			SamplePositionPointer = Atrac.DecodingOffsetInSamples;
			if (Atrac.DecodingReachedEnd) throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED));
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="EndSamplePointer"></param>
		/// <param name="LoopStartSamplePointer"></param>
		/// <param name="piLoopEndSample"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA2BBA8BE, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetSoundSample(int AtracId, out int EndSamplePointer, out int LoopStartSamplePointer, out int LoopEndSamplePointer)
		{
			var Atrac = AtracList.Get(AtracId);
			{
				EndSamplePointer = Atrac.Fact.EndSample;
				LoopStartSamplePointer = -1;
				LoopEndSamplePointer = -1;
			}
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
		public int sceAtracGetBufferInfoForReseting(int AtracId, uint uiSample, PspBufferInfo* pBufferInfo)
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
		public int sceAtracResetPlayPosition(int AtracId, uint uiSample, uint uiWriteByteFirstBuf, uint uiWriteByteSecondBuf)
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
		/// <param name="BufferInfoAddr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x2DD3E298, FirmwareVersion = 250)]
		[HlePspNotImplemented]
		public int sceAtracGetBufferInfoForResetting(int AtracId, uint uiSample, void* BufferInfoAddr)
		{
			throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x31668baa, FirmwareVersion = 250)]
		[HlePspNotImplemented]
		public int sceAtracGetChannel()
		{
			throw (new NotImplementedException());
			return -1;
		}
	}
}

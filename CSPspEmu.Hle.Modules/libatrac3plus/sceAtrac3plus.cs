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

		public class Atrac : IDisposable
		{
			public byte[] Data;
			public FormatStruct Format;
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
			public int SamplesOffset;

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
				Atrac = 0xFFFE,
				Experimental = 0xFFFF,
			}

			public struct FormatStruct
			{
				public CompressionCode CompressionCode;
				public ushort AtracChannels;
				public uint Bitrate;
				public ushort BytesPerFrame;
				public ushort HiBytesPerFrame;
			}

			public struct FactStruct
			{
				public int EndSample;
				public int SampleOffset;
			}

			public struct SmplStruct
			{
			}

			public Atrac(CodecType CodecType)
			{
				this.CodecType = CodecType;
			}

			public Atrac(byte[] Data)
			{
				CodecType = CodecType.PSP_MODE_AT_3_PLUS;
				SetData(Data);
			}

			public void SetData(byte[] Data)
			{
				this.Data = Data;
				//ArrayUtils.HexDump(Data, 1024);
				//Console.ReadKey();
				Parse(new MemoryStream(Data));

				OmaWavConverter.convertOmaToWav(@"C:\temp\test.oma", @"C:\temp\test.wav");

				//FilterGraph.AddFilter();
				//FilterGraph.AddFilter();
				//Console.ReadKey();
			}

			private void Parse(Stream Stream)
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
					switch (ChunkType)
					{
						case "fmt ":
							Format = ChunkStream.ReadStruct<FormatStruct>();
							break;
						case "fact":
							Fact = ChunkStream.ReadStruct<FactStruct>();
							break;
						case "smpl":
							// Loop info
							Smpl = ChunkStream.ReadStruct<SmplStruct>();
							break;
						case "data":
							break;
						default:
							throw(new NotImplementedException(String.Format("Can't handle chunk '{0}'", ChunkType)));
					}
				}
			}

			void IDisposable.Dispose()
			{
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
			var Atrac = new Atrac(Data);
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
		public int sceAtracGetOutputChannel(int AtracId, int* OutputChannelPointer)
		{
			var Atrac = AtracList.Get(AtracId);
			*OutputChannelPointer = 2;
			return 0;
		}

		/// <summary>
		/// Gets the bitrate.
		/// </summary>
		/// <param name="AtracId">The atracID</param>
		/// <param name="OutputBitrate">Pointer to a integer that receives the bitrate in kbps</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0xA554A158, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetBitrate(int AtracId, uint* OutputBitrate)
		{
			var Atrac = AtracList.Get(AtracId);
			*OutputBitrate = Atrac.Format.Bitrate;
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
		/// <param name="MaxNumberOfSamplesPointer">Pointer to a integer that receives the maximum number of samples.</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0xD6A5F2F7, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceAtracGetMaxSample(int AtracId, int* MaxNumberOfSamplesPointer)
		{
			var Atrac = AtracList.Get(AtracId);
			*MaxNumberOfSamplesPointer = Atrac.MaximumSamples;
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
		public int sceAtracGetLoopStatus(int AtracId, int* piLoopNum, uint* puiLoopStatus)
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
		public int sceAtracGetRemainFrame(int AtracId, int* RemainFramePointer)
		{
			var Atrac = AtracList.Get(AtracId);
			*RemainFramePointer = -1;
			return 0;
		}

		/// <summary>
		/// Decode a frame of data. 
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="SamplesOut">pointer to a buffer that receives the decoded data of the current frame</param>
		/// <param name="DecodedSamplesPointer">pointer to a integer that receives the number of audio samples of the decoded frame</param>
		/// <param name="ReachedEndPointer">pointer to a integer that receives a boolean value indicating if the decoded frame is the last one</param>
		/// <param name="RemainingFramesToDecodePointer">
		///		pointer to a integer that receives either -1 if all at3 data is already on memory, 
		///		or the remaining (not decoded yet) frames at memory if not all at3 data is on memory
		/// </param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x6A8C3CD5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracDecodeData(int AtracId, ushort* SamplesOut, int* DecodedSamplesPointer, bool* ReachedEndPointer, int* RemainingFramesToDecodePointer)
		{
			var Atrac = AtracList.Get(AtracId);
			throw(new NotImplementedException());
			*DecodedSamplesPointer = 0;
			*ReachedEndPointer = true;
			*RemainingFramesToDecodePointer = -1;
			//throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED));
			return 0;

			/*
			//logInfo("Not implemented sceAtracDecodeData(%d)", atracID);
			//unimplemented_notice();
		
			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);
		
			int numSamples = atrac3Object.getMaxNumberOfSamples();
			int numShorts = numSamples * 2;
			int result = 0;
			bool endedStream;
		
			int samplesLeft = cast(int)(atrac3Object.samples.length - atrac3Object.samplesOffset * 2);
		
			if (samplesLeft <= numShorts) {
				endedStream = true;
			} else {
				endedStream = false;
			}
		
			int numReadedSamples = min(samplesLeft, numShorts);
		
			outSamples[0..numReadedSamples] = cast(u16[])atrac3Object.samples[atrac3Object.samplesOffset * 2..atrac3Object.samplesOffset * 2 + numReadedSamples]; 
			atrac3Object.samplesOffset += numReadedSamples / 2;
		
			*outN = numReadedSamples / 2;
			*outRemainFrame = -1;
		
			logTrace("sceAtracDecodeData(atracID=%d, outN=%d, outEnd=%d, outRemainFrame=%d, offset=%d)", atracID, *outN, *outEnd, *outRemainFrame, atrac3Object.samplesOffset);

			if (endedStream) {
				if (atrac3Object.nloops != 0) {
					endedStream = false;
					atrac3Object.samplesOffset = 0;
					logTrace("sceAtracDecodeData :: reset");
				}
				if (atrac3Object.nloops > 0) atrac3Object.nloops--;
			}
		
			if (endedStream) {
				*outEnd = -1;			
				result = SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED;
				logWarning("sceAtracDecodeData :: ended");
			} else {
				*outEnd = 0;
			}
		
			if (result == 0) {
				Thread.yield();
				Thread.sleep(dur!"usecs"(2300));
				//core.datetime.
				//std.date.
				//2300
			}

			return result;
			*/
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
			throw (new NotImplementedException());
			return 0;
			//throw(new NotImplementedException());
			/*
			unimplemented_notice();
			uniqueIdFactory.remove!Atrac3Object(atracID);
			return 0;
			*/
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
			var Atrac = new Atrac(CodecType);
			var AtracId = AtracList.Create(Atrac);
			return AtracId;
		}

		/// <summary>
		/// Gets the number of samples of the next frame to be decoded.
		/// </summary>
		/// <param name="AtracId">The atrac ID</param>
		/// <param name="outN">Pointer to receives the number of samples of the next frame.</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x36FAABFB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetNextSample(int AtracId, int* outN)
		{
			throw (new NotImplementedException());
			*outN = 0;
			return 0;
			//throw (new NotImplementedException());
			/*
			unimplemented_notice();
			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);

			*outN = atrac3Object.getMaxNumberOfSamples();
			logInfo("sceAtracGetNextSample(atracID=%d, outN=%d)", atracID, *outN);
			return 0;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AtracId"></param>
		/// <param name="piResult"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE88F759B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetInternalErrorInfo(int AtracId, int* piResult)
		{
			*piResult = 0;
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
		public int sceAtracGetStreamDataInfo(int AtracId, uint* writePointerPointer /*u8** writePointer*/, uint* availableBytes, uint* readOffset)
		{
			throw (new NotImplementedException());
			*writePointerPointer = 0; // @FIXME!!
			*availableBytes = 0;
			*readOffset     = 0;

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
		public int sceAtracGetSecondBufferInfo(int AtracId, uint* puiPosition, uint* puiDataByte)
		{
			//throw (new NotImplementedException());
			*puiPosition = 0;
			*puiDataByte = 0;

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
		public int sceAtracSetSecondBuffer(int AtracId, byte* pucSecondBufferAddr, uint uiSecondBufferByte)
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
		[HlePspNotImplemented]
		public int sceAtracGetNextDecodePosition(int AtracId, int* SamplePositionPointer)
		{
			var Atrac = AtracList.Get(AtracId);
			*SamplePositionPointer = Atrac.SamplesOffset;
			if (Atrac.SamplesOffset >= Atrac.Fact.EndSample) throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED));
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
		public int sceAtracGetSoundSample(int AtracId, int* EndSamplePointer, int* LoopStartSamplePointer, int* LoopEndSamplePointer)
		{
			var Atrac = AtracList.Get(AtracId);
			{
				*EndSamplePointer = Atrac.Fact.EndSample;
				*LoopStartSamplePointer = -1;
				*LoopEndSamplePointer = -1;
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

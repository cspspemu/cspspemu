using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules._unknownPrx
{
	unsafe public partial class sceMp3 : HleModuleHost
	{
		[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
		public struct SceMp3InitArg
		{
			/// <summary>
			/// Stream start position
			/// </summary>
			public uint mp3StreamStart;
			
			/// <summary>
			/// Unknown - set to 0
			/// </summary>
			public uint unk1;
			
			/// <summary>
			/// Stream end position
			/// </summary>
			public uint mp3StreamEnd;
			
			/// <summary>
			/// Unknown - set to 0
			/// </summary>
			public uint unk2;
			
			/// <summary>
			/// Pointer to a buffer to contain raw mp3 stream data (+1472 bytes workspace)
			/// </summary>
			public uint mp3BufPtr;
			
			/// <summary>
			/// Size of mp3Buf buffer (must be >= 8192)
			/// </summary>
			public int mp3BufSize;
			
			/// <summary>
			/// Pointer to decoded pcm samples buffer
			/// </summary>
			public uint pcmBufPtr;
			
			/// <summary>
			/// Size of pcmBuf buffer (must be >= 9216)
			/// </summary>
			public int pcmBufSize;
		}

		public enum Mp3StreamId : int
		{
		}

		/// <summary>
		/// sceMp3ReserveMp3Handle
		/// </summary>
		/// <param name="mp3args">Pointer to SceMp3InitArg structure</param>
		/// <returns>sceMp3 handle on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x07EC321A, FirmwareVersion = 150)]
		public int sceMp3ReserveMp3Handle(SceMp3InitArg* mp3args)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// sceMp3NotifyAddStreamData
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <param name="size">number of bytes added to the stream data buffer</param>
		/// <returns>0 if success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x0DB149F4, FirmwareVersion = 150)]
		public int sceMp3NotifyAddStreamData(Mp3StreamId mp3Stream, int size)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3ResetPlayPosition
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x2A368661, FirmwareVersion = 150)]
		public int sceMp3ResetPlayPosition(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3InitResource
		/// </summary>
		/// <returns>0 if success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x35750070, FirmwareVersion = 150)]
		public int sceMp3InitResource() {
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3TermResource
		/// </summary>
		/// <returns>0 if success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x3C2FA058, FirmwareVersion = 150)]
		public int sceMp3TermResource() {
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3SetLoopNum
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <param name="loopNbr">Number of loops</param>
		/// <returns>0 if success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x3CEF484F, FirmwareVersion = 150)]
		public int sceMp3SetLoopNum(Mp3StreamId mp3Stream, int loopNbr)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3Init
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>0 if success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x44E07129, FirmwareVersion = 150)]
		public int sceMp3Init(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3GetMp3ChannelNum
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>Number of channels of the mp3</returns>
		[HlePspFunction(NID = 0x7F696782, FirmwareVersion = 150)]
		public int sceMp3GetMp3ChannelNum(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3GetSamplingRate
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>Sampling rate of the mp3</returns>
		[HlePspFunction(NID = 0x8F450998, FirmwareVersion = 150)]
		public int sceMp3GetSamplingRate(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3GetInfoToAddStreamData
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <param name="mp3BufPtr">Pointer to stream data buffer</param>
		/// <param name="mp3BufToWritePtr">Space remaining in stream data buffer</param>
		/// <param name="mp3PosPtr">Position in source stream to start reading from</param>
		/// <returns>0 if success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xA703FE0F, FirmwareVersion = 150)]
		public int sceMp3GetInfoToAddStreamData(Mp3StreamId mp3Stream, uint mp3BufPtr, uint mp3BufToWritePtr, uint mp3PosPtr)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3Decode
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <param name="outPcmPtr">Pointer to destination pcm samples buffer</param>
		/// <returns>number of bytes in decoded pcm buffer, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xD021C0FB, FirmwareVersion = 150)]
		public int sceMp3Decode(Mp3StreamId mp3Stream, uint outPcmPtr)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3CheckStreamDataNeeded
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>1 if more stream data is needed, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xD0A56296, FirmwareVersion = 150)]
		public bool sceMp3CheckStreamDataNeeded(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3ReleaseMp3Handle
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>0 if success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xF5478233, FirmwareVersion = 150)]
		public int sceMp3ReleaseMp3Handle(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3GetSumDecodedSample
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>Number of decoded samples</returns>
		[HlePspFunction(NID = 0x354D27EA, FirmwareVersion = 150)]
		public int sceMp3GetSumDecodedSample(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3GetBitRate
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>Bitrate of the mp3</returns>
		[HlePspFunction(NID = 0x87677E40, FirmwareVersion = 150)]
		public int sceMp3GetBitRate(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3GetMaxOutputSample
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>Number of max samples to output</returns>
		[HlePspFunction(NID = 0x87C263D1, FirmwareVersion = 150)]
		public int sceMp3GetMaxOutputSample(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMp3GetLoopNum
		/// </summary>
		/// <param name="mp3Stream">sceMp3 handle</param>
		/// <returns>Number of loops</returns>
		[HlePspFunction(NID = 0xD8F54A51, FirmwareVersion = 150)]
		public int sceMp3GetLoopNum(Mp3StreamId mp3Stream)
		{
			throw (new NotImplementedException());
		}
	}
}

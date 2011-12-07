using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Modules.threadman;

namespace CSPspEmu.Hle.Modules.libatrac3plus
{
	unsafe public partial class sceAtrac3plus : HleModuleHost
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB3B5D042, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetOutputChannel()
		{
			throw(new NotImplementedException());
			/*
			unimplemented();
			return 0;
			*/
		}

		/// <summary>
		/// Gets the bitrate.
		/// </summary>
		/// <param name="atracID">The atracID</param>
		/// <param name="outBitrate">Pointer to a integer that receives the bitrate in kbps</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0xA554A158, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetBitrate(int atracID, int* outBitrate)
		{
			throw (new NotImplementedException());
			/*
			unimplemented_notice();

			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);

			*outBitrate = atrac3Object.processor.atrac3Format.sampleRate;

			return 0;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atracID"></param>
		/// <param name="bufferPtr"></param>
		/// <param name="bufferSizeInBytes"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x0E2A73AB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetData(int atracID, byte* bufferPtr, uint bufferSizeInBytes)
		{
			throw(new NotImplementedException());
			/*
			u8[] buffer = bufferPtr[0..bufferSizeInBytes];
			//unimplemented();
			unimplemented_notice();
			return 0;
			*/
		}

		/// <summary>
		/// Gets the maximum number of samples of the atrac3 stream.
		/// </summary>
		/// <param name="atracID">The atrac ID</param>
		/// <param name="outMax">Pointer to a integer that receives the maximum number of samples.</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0xD6A5F2F7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetMaxSample(int atracID, int* outMax)
		{
			throw (new NotImplementedException());
			/*
			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);
			*outMax = atrac3Object.getMaxNumberOfSamples();
			//unimplemented();
			logInfo("sceAtracGetMaxSample(atracID=%d, outMax=%d)", atracID, *outMax);
			return 0;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atracID"></param>
		/// <param name="piLoopNum"></param>
		/// <param name="puiLoopStatus"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xFAA4F89B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetLoopStatus(int atracID, int* piLoopNum, uint* puiLoopStatus)
		{
			throw (new NotImplementedException());
			/*
			unimplemented();
			return 0;
			*/
		}

		/// <summary>
		/// Creates a new Atrac ID from the specified data
		/// </summary>
		/// <param name="DataPointer">the buffer holding the atrac3 data, including the RIFF/WAVE header.</param>
		/// <param name="DataLength">the size of the buffer pointed by buf</param>
		/// <returns>The new atrac ID, or less than 0 on error </returns>
		[HlePspFunction(NID = 0x7A20E7AF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetDataAndGetID(byte* DataPointer, uint DataLength)
		{
			return 0;
			//throw(new NotImplementedException());
			/*
			unimplemented_notice();
			logWarning("Not implemented sceAtracSetDataAndGetID");
			Atrac3Object atrac3Object = new Atrac3Object((cast(ubyte*)buf)[0..bufsize]);

			return cast(int)uniqueIdFactory.add(atrac3Object);
			*/
		}

		/// <summary>
		/// Sets the number of loops for this atrac ID
		/// </summary>
		/// <param name="atracID">The atracID</param>
		/// <param name="nloops">
		///		The number of loops to set (0 means play it one time, 1 means play it twice, 2 means play it three times, ...)
		///		-1 means play it forever
		/// </param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x868120B5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetLoopNum(int atracID, int nloops)
		{
			//throw (new NotImplementedException());
			return 0;
			/*
			unimplemented_notice();
			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);
			atrac3Object.nloops = nloops;
			logInfo("sceAtracSetLoopNum(atracID=%d, nloops=%d)", atracID, nloops);
			return 0;
			*/
		}

		/// <summary>
		/// Gets the remaining (not decoded) number of frames
		/// </summary>
		/// <param name="atracID">The atrac ID</param>
		/// <param name="outRemainFrame">
		///		Pointer to a integer that receives either -1 if all at3 data is already on memory, 
		///		or the remaining (not decoded yet) frames at memory if not all at3 data is on memory 
		/// </param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x9AE849A7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetRemainFrame(int atracID, int* outRemainFrame)
		{
			*outRemainFrame = -1;
			return 0;
			//throw (new NotImplementedException());
			/*
			//unimplemented_notice();
			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);
			//logWarning("Not implemented sceAtracGetRemainFrame(%d, %s)", atracID, outRemainFrame);
			*outRemainFrame = -1;
			return 0;
			*/
		}

		/*
		Atrac3Object getAtrac3ObjectById(int atracID) {
			return uniqueIdFactory.get!Atrac3Object(atracID);
		}
		*/

		/// <summary>
		/// Decode a frame of data. 
		/// </summary>
		/// <param name="atracID">The atrac ID</param>
		/// <param name="outSamples">pointer to a buffer that receives the decoded data of the current frame</param>
		/// <param name="outN">pointer to a integer that receives the number of audio samples of the decoded frame</param>
		/// <param name="outEnd">pointer to a integer that receives a boolean value indicating if the decoded frame is the last one</param>
		/// <param name="outRemainFrame">
		///		pointer to a integer that receives either -1 if all at3 data is already on memory, 
		///		or the remaining (not decoded yet) frames at memory if not all at3 data is on memory
		/// </param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x6A8C3CD5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracDecodeData(int atracID, ushort* outSamples, int* outN, int* outEnd, int* outRemainFrame)
		{
			*outN = 0;
			*outEnd = -1;
			*outRemainFrame = -1;
			throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED));
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
		/// <param name="atracID">The atrac ID to release</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x61EB33F5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracReleaseAtracID(int atracID)
		{
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
		/// <param name="codecType"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x780F88D1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetAtracID(int codecType)
		{
			return 0;
			//throw(new NotImplementedException());
			/*
			ubyte[] data;
			logInfo("sceAtracGetAtracID(%d)", codecType);
			unimplemented_notice();
			return cast(int)uniqueIdFactory.add(new Atrac3Object(data));
			*/
		}

		/// <summary>
		/// Gets the number of samples of the next frame to be decoded.
		/// </summary>
		/// <param name="atracID">The atrac ID</param>
		/// <param name="outN">Pointer to receives the number of samples of the next frame.</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x36FAABFB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetNextSample(int atracID, int* outN)
		{
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
		/// <param name="atracID"></param>
		/// <param name="piResult"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE88F759B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetInternalErrorInfo(int atracID, int* piResult)
		{
			*piResult = 0;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atracID">The atrac ID</param>
		/// <param name="writePointer">Pointer to where to read the atrac data</param>
		/// <param name="availableBytes">Number of bytes available at the writePointer location</param>
		/// <param name="readOffset">Offset where to seek into the atrac file before reading</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x5D268707, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetStreamDataInfo(int atracID, uint writePointerPointer /*u8** writePointer*/, uint* availableBytes, uint* readOffset)
		{
			throw(new NotImplementedException());
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
		/// <param name="atracID">The atrac ID</param>
		/// <param name="bytesToAdd">Number of bytes read into location given by sceAtracGetStreamDataInfo().</param>
		/// <returns>Less than 0 on error, otherwise 0</returns>
		[HlePspFunction(NID = 0x7DB31251, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracAddStreamData(int atracID, int bytesToAdd)
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
		/// <param name="atracID"></param>
		/// <param name="puiPosition"></param>
		/// <param name="puiDataByte"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x83E85EA0, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetSecondBufferInfo(int atracID, uint* puiPosition, uint* puiDataByte)
		{
			*puiPosition = 0;
			*puiDataByte = 0;

			throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_SECOND_BUFFER_NOT_NEEDED));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atracID"></param>
		/// <param name="pucSecondBufferAddr"></param>
		/// <param name="uiSecondBufferByte"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x83BF7AFD, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracSetSecondBuffer(int atracID, byte* pucSecondBufferAddr, uint uiSecondBufferByte)
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
		/// <param name="atracID"></param>
		/// <param name="puiSamplePosition"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE23E3A35, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetNextDecodePosition(int atracID, uint* puiSamplePosition)
		{
			*puiSamplePosition = 0;
			return 0;
			//throw (new NotImplementedException());
			/*
			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);

			//unimplemented_notice();
			//*puiSamplePosition = atrac3Object.samplesOffset / 2;
			*puiSamplePosition = atrac3Object.samplesOffset;
			//*puiSamplePosition = 0;
			logInfo("sceAtracGetNextDecodePosition(atracID=%d, puiSamplePosition=%d)", atracID, *puiSamplePosition);

			if (atrac3Object.samplesOffset >= atrac3Object.processor.fact.atracEndSample)
			{
				return SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED;
			}
			else
			{
				return 0;
			}
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atracID"></param>
		/// <param name="piEndSample"></param>
		/// <param name="piLoopStartSample"></param>
		/// <param name="piLoopEndSample"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA2BBA8BE, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetSoundSample(int atracID, int* piEndSample, int* piLoopStartSample, int* piLoopEndSample)
		{
			*piEndSample = 0;
			*piLoopStartSample = 0;
			*piLoopEndSample = 0;
			//throw (new NotImplementedException());
			return 0;
			/*
			Atrac3Object atrac3Object = getAtrac3ObjectById(atracID);
			atrac3Object.writeBufferSize = atrac3Object.getMaxNumberOfSamples();
			atrac3Object.writeBufferGuestPtr = hleEmulatorState.memoryManager.malloc(atrac3Object.writeBufferSize);
			*piEndSample = atrac3Object.processor.fact.atracEndSample;
			if (atrac3Object.processor.loops.length > 0)
			{
				*piLoopStartSample = atrac3Object.processor.loops[0].startSample;
				*piLoopEndSample = atrac3Object.processor.loops[0].endSample;
			}
			else
			{
				*piLoopStartSample = -1;
				*piLoopEndSample = -1;
			}

			logInfo("sceAtracGetSoundSample(atracID=%d, piEndSample=%d, piLoopStartSample=%d, piLoopEndSample=%d)", atracID, *piEndSample, *piLoopStartSample, *piLoopEndSample);

			//unimplemented_notice();
			return 0;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atracID"></param>
		/// <param name="uiSample"></param>
		/// <param name="pBufferInfo"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xCA3CA3D2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracGetBufferInfoForReseting(int atracID, uint uiSample, PspBufferInfo* pBufferInfo)
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
		/// <param name="atracID"></param>
		/// <param name="uiSample"></param>
		/// <param name="uiWriteByteFirstBuf"></param>
		/// <param name="uiWriteByteSecondBuf"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x644E5607, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAtracResetPlayPosition(int atracID, uint uiSample, uint uiWriteByteFirstBuf, uint uiWriteByteSecondBuf)
		{
			throw (new NotImplementedException());
			/*
			unimplemented();
			return 0;
			*/
		}
	}
}

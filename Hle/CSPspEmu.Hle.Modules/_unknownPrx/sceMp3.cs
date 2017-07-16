using System;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;
using CSharpUtils;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules._unknownPrx
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public unsafe class sceMp3 : HleModuleHost
    {
        [Inject] InjectContext InjectContext;

        public struct SceMp3InitArg
        {
            /// <summary>
            /// Stream start position
            /// </summary>
            public PspPointer Mp3StreamStart;

            /// <summary>
            /// Unknown - set to 0
            /// </summary>
            public uint Unknown1;

            /// <summary>
            /// Stream end position
            /// </summary>
            public PspPointer Mp3StreamEnd;

            /// <summary>
            /// Unknown - set to 0
            /// </summary>
            public uint Unknown2;

            /// <summary>
            /// Pointer to a buffer to contain raw mp3 stream data (+1472 bytes workspace)
            /// </summary>
            public PspPointer Mp3BufferPointer;

            /// <summary>
            /// Size of mp3Buf buffer (must be >= 8192)
            /// </summary>
            public int Mp3BufferSize;

            /// <summary>
            /// Pointer to decoded pcm samples buffer
            /// </summary>
            public PspPointer PcmBufferPointer;

            /// <summary>
            /// Size of pcmBuf buffer (must be >= 9216)
            /// </summary>
            public int PcmBufferSize;
        }

        public enum Mp3StreamId : int
        {
        }

        [HleUidPoolClass(NotFoundError = (SceKernelErrors) (-1))]
        public class Mp3Stream : IDisposable, IHleUidPoolClass
        {
            public SceMp3InitArg* Mp3Arguments;
            public int SamplingRate;

            public Mp3Stream(SceMp3InitArg* Mp3Arguments)
            {
                this.Mp3Arguments = Mp3Arguments;
            }

            public void AddStreamData(byte[] Data)
            {
                Console.WriteLine("Not Implemented: Mp3Stream.AddStreamData");
            }

            void IDisposable.Dispose()
            {
            }
        }

        /// <summary>
        /// sceMp3ReserveMp3Handle
        /// </summary>
        /// <param name="Mp3Arguments">Pointer to an <see cref="SceMp3InitArg"/> structure</param>
        /// <returns>sceMp3 handle on success, less than 0 on error.</returns>
        [HlePspFunction(NID = 0x07EC321A, FirmwareVersion = 150)]
        public Mp3Stream sceMp3ReserveMp3Handle(SceMp3InitArg* Mp3Arguments)
        {
            return new Mp3Stream(Mp3Arguments);
        }

        /// <summary>
        /// sceMp3NotifyAddStreamData
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <param name="Size">Number of bytes added to the stream data buffer</param>
        /// <returns>0 if success, less than 0 on error.</returns>
        [HlePspFunction(NID = 0x0DB149F4, FirmwareVersion = 150)]
        public int sceMp3NotifyAddStreamData(Mp3Stream Mp3Stream, int Size)
        {
            Mp3Stream.AddStreamData(
                PointerUtils.PointerToByteArray(
                    (byte*) Mp3Stream.Mp3Arguments->Mp3BufferPointer.GetPointer(Memory, Size), Size)
            );
            return 0;
        }

        /// <summary>
        /// sceMp3ResetPlayPosition
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>Less than 0 on error</returns>
        [HlePspFunction(NID = 0x2A368661, FirmwareVersion = 150)]
        public int sceMp3ResetPlayPosition(Mp3Stream Mp3Stream)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3InitResource
        /// </summary>
        /// <returns>0 if success, less than 0 on error.</returns>
        [HlePspFunction(NID = 0x35750070, FirmwareVersion = 150)]
        public int sceMp3InitResource()
        {
            return 0;
        }

        /// <summary>
        /// sceMp3TermResource
        /// </summary>
        /// <returns>0 if success, less than 0 on error.</returns>
        [HlePspFunction(NID = 0x3C2FA058, FirmwareVersion = 150)]
        public int sceMp3TermResource()
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3SetLoopNum
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <param name="NumberOfLoops">Number of loops</param>
        /// <returns>0 if success, less than 0 on error.</returns>
        [HlePspFunction(NID = 0x3CEF484F, FirmwareVersion = 150)]
        public int sceMp3SetLoopNum(Mp3Stream Mp3Stream, int NumberOfLoops)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3Init
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>0 if success, less than 0 on error.</returns>
        [HlePspFunction(NID = 0x44E07129, FirmwareVersion = 150)]
        public int sceMp3Init(Mp3Stream Mp3Stream)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3GetMp3ChannelNum
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>Number of channels of the mp3</returns>
        [HlePspFunction(NID = 0x7F696782, FirmwareVersion = 150)]
        public int sceMp3GetMp3ChannelNum(Mp3Stream Mp3Stream)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3GetSamplingRate
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>Sampling rate of the mp3</returns>
        [HlePspFunction(NID = 0x8F450998, FirmwareVersion = 150)]
        public int sceMp3GetSamplingRate(Mp3Stream Mp3Stream)
        {
            return Mp3Stream.SamplingRate;
        }

        /// <summary>
        /// sceMp3GetInfoToAddStreamData
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <param name="Mp3BufferPointer">Pointer to stream data buffer</param>
        /// <param name="mp3BufToWritePtr">Space remaining in stream data buffer</param>
        /// <param name="mp3PosPtr">Position in source stream to start reading from</param>
        /// <returns>0 if success, less than 0 on error.</returns>
        [HlePspFunction(NID = 0xA703FE0F, FirmwareVersion = 150)]
        public int sceMp3GetInfoToAddStreamData(Mp3Stream Mp3Stream, uint Mp3BufferPointer, uint mp3BufToWritePtr,
            uint mp3PosPtr)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3Decode
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <param name="OutputPcmPointer">Pointer to destination pcm samples buffer</param>
        /// <returns>Number of bytes in decoded pcm buffer, less than 0 on error.</returns>
        [HlePspFunction(NID = 0xD021C0FB, FirmwareVersion = 150)]
        public int sceMp3Decode(Mp3Stream Mp3Stream, uint OutputPcmPointer)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3CheckStreamDataNeeded
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>1 if more stream data is needed, less than 0 on error.</returns>
        [HlePspFunction(NID = 0xD0A56296, FirmwareVersion = 150)]
        public bool sceMp3CheckStreamDataNeeded(Mp3Stream Mp3Stream)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3ReleaseMp3Handle
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>0 if success, less than 0 on error.</returns>
        [HlePspFunction(NID = 0xF5478233, FirmwareVersion = 150)]
        public int sceMp3ReleaseMp3Handle(Mp3Stream Mp3Stream)
        {
            Mp3Stream.RemoveUid(InjectContext);
            return 0;
        }

        /// <summary>
        /// sceMp3GetSumDecodedSample
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>Number of decoded samples</returns>
        [HlePspFunction(NID = 0x354D27EA, FirmwareVersion = 150)]
        public int sceMp3GetSumDecodedSample(Mp3Stream Mp3Stream)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3GetBitRate
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>Bitrate of the mp3</returns>
        [HlePspFunction(NID = 0x87677E40, FirmwareVersion = 150)]
        public int sceMp3GetBitRate(Mp3Stream Mp3Stream)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3GetMaxOutputSample
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>Number of max samples to output</returns>
        [HlePspFunction(NID = 0x87C263D1, FirmwareVersion = 150)]
        public int sceMp3GetMaxOutputSample(Mp3Stream Mp3Stream)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// sceMp3GetLoopNum
        /// </summary>
        /// <param name="Mp3Stream">sceMp3 handle</param>
        /// <returns>Number of loops</returns>
        [HlePspFunction(NID = 0xD8F54A51, FirmwareVersion = 150)]
        public int sceMp3GetLoopNum(Mp3Stream Mp3Stream)
        {
            throw (new NotImplementedException());
        }
    }
}
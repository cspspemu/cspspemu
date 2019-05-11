using cscodec.h264.player;
using CSPspEmu.Core.Memory;
using System;

namespace CSPspEmu.Hle.Modules.mpeg
{
    /// <summary>
    /// AVC: Advanced Video Coding
    /// </summary>
    /// <see cref="http://en.wikipedia.org/wiki/H.264/MPEG-4_AVC"/>
    public unsafe partial class sceMpeg
    {
        protected bool[] AbvEsBufAllocated = new bool[2];

        /// <summary>
        /// sceMpegAvcDecodeDetail
        /// </summary>
        /// <param name="SceMpegPointer">SceMpeg handle</param>
        /// <param name="AvcDecodeDetail">AvcDecodeDetail</param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0x0F6C18D7, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegAvcDecodeDetail(SceMpegPointer* SceMpegPointer, AvcDecodeDetailStruct* AvcDecodeDetail)
        {
            var Mpeg = GetMpeg(SceMpegPointer);
            var SceMpegData = GetSceMpegData(SceMpegPointer);

            //throw(new NotImplementedException());
            AvcDecodeDetail->AvcDecodeResult = 0;
            AvcDecodeDetail->VideoFrameCount = 0;
            AvcDecodeDetail->AvcDetailFrameWidth = SceMpegData->FrameWidth;
            AvcDecodeDetail->AvcDetailFrameHeight = 272;
            AvcDecodeDetail->FrameCropRectLeft = 0;
            AvcDecodeDetail->FrameCropRectRight = 0;
            AvcDecodeDetail->FrameCropRectTop = 0;
            AvcDecodeDetail->FrameCropRectBottom = 0;
            AvcDecodeDetail->AvcFrameStatus = SceMpegData->AvcFrameStatus;

            return 0;
        }

        /// <summary>
        /// sceMpegAvcDecodeFlush
        /// </summary>
        /// <param name="SceMpegPointer">SceMpeg handle</param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0x4571CC64, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegAvcDecodeFlush(SceMpegPointer* SceMpegPointer)
        {
            var Mpeg = GetMpeg(SceMpegPointer);
            var SceMpegData = GetSceMpegData(SceMpegPointer);

            Mpeg.AvcFlush();

            // Finish the Mpeg only if we are not at the start of a new video,
            // otherwise the analyzed video could be lost.
            if (SceMpegData->VideoFrameCount > 0 || SceMpegData->AudioFrameCount > 0)
            {
                _FinishMpeg(SceMpegData);
            }

            //throw(new NotImplementedException());
            return 0;
        }

        private void _FinishMpeg(SceMpeg* SceMpegData)
        {
            Console.WriteLine("_FinishMpeg");
        }

        /// <summary>
        /// Get Mpeg AVC Access Unit
        /// </summary>
        /// <param name="Mpeg">SceMpeg handle</param>
        /// <param name="StreamId">Associated stream</param>
        /// <param name="MpegAccessUnit">Will contain pointer to Au</param>
        /// <param name="DataAttributes">Unknown</param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0xFE246728, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceMpegGetAvcAu(SceMpegPointer* SceMpegPointer, StreamId StreamId, out SceMpegAu MpegAccessUnit,
            int* DataAttributes)
        {
            if (DataAttributes != null)
            {
                *DataAttributes = 1;
            }

            var Mpeg = GetMpeg(SceMpegPointer);
            if (!Mpeg.HasData) throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
            MpegAccessUnit = Mpeg.GetAvcAu(StreamId);
            return 0;
        }

        /// <summary>
        /// Allocates an elementary stream buffer.
        /// </summary>
        /// <param name="Mpeg"></param>
        /// <returns>
        ///		0 if error, else a ElementaryStream ID.
        /// </returns>
        [HlePspFunction(NID = 0xA780CF7E, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegMallocAvcEsBuf(SceMpegPointer* Mpeg)
        {
            for (int n = 0; n < 2; n++)
            {
                if (!AbvEsBufAllocated[n])
                {
                    AbvEsBufAllocated[n] = true;
                    return n + 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// sceMpegFreeAvcEsBuf
        /// </summary>
        /// <param name="Mpeg"></param>
        /// <param name="ElementaryStream">Value returned from <see cref="sceMpegMallocAvcEsBuf"/></param>
        [HlePspFunction(NID = 0xCEB870B1, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public uint sceMpegFreeAvcEsBuf(SceMpegPointer* Mpeg, int ElementaryStream)
        {
            AbvEsBufAllocated[ElementaryStream - 1] = false;
            return 0;
        }

        /// <summary>
        /// Sets the SceMpegAvcMode to a Mpeg
        /// </summary>
        /// <param name="SceMpegPointer">SceMpeg handle</param>
        /// <param name="Mode">Pointer to <see cref="SceMpegAvcMode"/> struct defining the decode mode (pixelformat)</param>
        /// <returns>0 if success.</returns>
        [HlePspFunction(NID = 0xA11C7026, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegAvcDecodeMode(SceMpegPointer* SceMpegPointer, SceMpegAvcMode* Mode)
        {
            var Mpeg = GetMpeg(SceMpegPointer);
            var SceMpegData = GetSceMpegData(SceMpegPointer);

            if (Mode != null)
            {
                switch (Mode->PixelFormat)
                {
                    case Core.Types.GuPixelFormats.Rgba5650:
                    case Core.Types.GuPixelFormats.Rgba5551:
                    case Core.Types.GuPixelFormats.Rgba8888:
                        SceMpegData->SceMpegAvcMode = *Mode;
                        break;
                    default:
                        throw (new Exception("Invalid PixelFormat in sceMpegAvcDecodeMode: " + Mode->Mode + ", " +
                                             Mode->PixelFormat));
                }
            }

            return 0;
        }

        /// <summary>
        /// sceMpegAvcDecode
        /// </summary>
        /// <param name="SceMpegPointer">SceMpeg handle</param>
        /// <param name="MpegAccessUnit">Video Access Unit</param>
        /// <param name="FrameWidth">Output buffer width, set to 512 if writing to framebuffer</param>
        /// <param name="OutputBufferPointer">Buffer that will contain the decoded frame</param>
        /// <param name="Init">Will be set to 0 on first call, then 1</param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0x0E3C2E9D, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceMpegAvcDecode(SceMpegPointer* SceMpegPointer, SceMpegAu* MpegAccessUnit, int FrameWidth,
            PspPointer* OutputBufferPointer, PspPointer* Init)
        {
            //if (*Init == 1)
            //{
            //	throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
            //}

            var SceMpegData = GetSceMpegData(SceMpegPointer);
            var Mpeg = GetMpeg(SceMpegPointer);

            // Dummy
            var VideoPacket = new VideoPacket();

            //Console.Error.WriteLine("0x{0:X}", PspMemory.PointerToPspAddress(OutputBuffer));
            //Console.WriteLine("{0:X8}", (*OutputBufferPointer).Address);
            Mpeg.AvcDecode(
                MpegAccessUnit,
                FrameWidth,
                SceMpegData->SceMpegAvcMode.PixelFormat,
                (*OutputBufferPointer)
            );

            SceMpegData->AvcFrameStatus = 1;
            //Init = SceMpegData->AvcFrameStatus;

            //throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Mpeg"></param>
        /// <param name="Mode"></param>
        /// <param name="Width">480</param>
        /// <param name="Height">272</param>
        /// <param name="Result">Where to store the result</param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x211A057C, FirmwareVersion = 150)]
        public int sceMpegAvcQueryYCbCrSize(SceMpegPointer* Mpeg, QueryYCbCrSizeModeEnum Mode, int Width, int Height,
            int* Result)
        {
            if ((Width & 15) != 0 || (Height & 15) != 0 || Width > 480 || Height > 272)
            {
                throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_INVALID_VALUE));
            }

            *Result = (Width / 2) * (Height / 2) * 6 + 128;
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Mpeg"></param>
        /// <param name="source_addr"></param>
        /// <param name="range_addr"></param>
        /// <param name="frameWidth"></param>
        /// <param name="dest_addr"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x31BD0272, FirmwareVersion = 150)]
        public int sceMpegAvcCsc(SceMpegPointer* Mpeg, int source_addr, int range_addr, int frameWidth, int dest_addr)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Mpeg"></param>
        /// <param name="mode"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="ycbcr_addr"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x67179B1B, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegAvcInitYCbCr(SceMpegPointer* Mpeg, int mode, int width, int height, int ycbcr_addr)
        {
            //throw (new NotImplementedException());
            //return -1;
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Mpeg"></param>
        /// <param name="au_addr"></param>
        /// <param name="buffer_addr"></param>
        /// <param name="init_addr"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0xF0EB1125, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegAvcDecodeYCbCr(SceMpegPointer* Mpeg, int au_addr, int buffer_addr, int init_addr)
        {
            //throw (new NotImplementedException());
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Mpeg"></param>
        /// <param name="buffer_addr"></param>
        /// <param name="status_addr"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0xF2930C9C, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegAvcDecodeStopYCbCr(SceMpegPointer* Mpeg, byte* OutputBuffer, out int Status)
        {
            Status = 0;
            //throw (new NotImplementedException());
            //return -1;
            return 0;
        }

        /// <summary>
        /// sceMpegAvcDecodeStop
        /// </summary>
        /// <param name="SceMpegPointer">SceMpeg handle</param>
        /// <param name="FrameWidth">Output buffer width, set to 512 if writing to framebuffer</param>
        /// <param name="OutputBuffer">Buffer that will contain the decoded frame</param>
        /// <param name="Status">Frame number</param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0x740FCCD1, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegAvcDecodeStop(SceMpegPointer* SceMpegPointer, int FrameWidth, byte* OutputBuffer,
            out int Status)
        {
            //throw(new NotImplementedException());
            var Mpeg = GetMpeg(SceMpegPointer);
            var SceMpegData = GetSceMpegData(SceMpegPointer);

            Mpeg.Stop();

            Status = 0;

            //throw(new NotImplementedException());
            return 0;
        }
    }
}
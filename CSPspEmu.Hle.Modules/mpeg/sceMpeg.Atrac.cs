using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.mpeg
{
	unsafe public partial class sceMpeg
	{
		/// <summary>
		/// sceMpegQueryAtracEsSize
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="ElementaryStreamSize">will contain size of Es</param>
		/// <param name="OutputSize">will contain size of decoded data</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xF8DCB679, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryAtracEsSize(SceMpeg* Mpeg, int* ElementaryStreamSize, int* OutputSize)
		{
			*ElementaryStreamSize = MPEG_ATRAC_ES_SIZE;
			*OutputSize = MPEG_ATRAC_ES_OUTPUT_SIZE;
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegGetAtracAu
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pStream">associated stream</param>
		/// <param name="MpegAccessUnit">will contain pointer to Au</param>
		/// <param name="Atrac3PlusPointer">Pointer to ATRAC3plus stream (from PSMF file).</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xE1CE83A7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegGetAtracAu(SceMpeg* Mpeg, StreamId StreamId, SceMpegAu* MpegAccessUnit, void* Atrac3PlusPointer)
		{
			CheckEnabledMpeg();

			//Mpeg->SceMpegData.

			throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
		}


		/// <summary>
		/// sceMpegAtracDecode
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="MpegAccessUnit">video Au</param>
		/// <param name="OutputBuffer">buffer that will contain the decoded frame</param>
		/// <param name="Init">set this to 1 on first call</param>
		/// <returns>
		///		0 if success.
		/// </returns>
		[HlePspFunction(NID = 0x800C44DF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAtracDecode(SceMpeg* Mpeg, SceMpegAu* MpegAccessUnit, byte* OutputBuffer, int Init)
		{
			CheckEnabledMpeg();

			throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_NO_DATA));
		}

	}
}

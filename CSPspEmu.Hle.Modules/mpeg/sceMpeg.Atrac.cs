using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.mpeg
{
	unsafe public partial class sceMpeg
	{
		/// <summary>
		/// sceMpegInitAu
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pEsBuffer">prevously allocated Es buffer</param>
		/// <param name="pAu">will contain pointer to Au</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x167AFD9E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegInitAu(SceMpeg* Mpeg, void* pEsBuffer, SceMpegAu* pAu)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegQueryAtracEsSize
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="iEsSize">will contain size of Es</param>
		/// <param name="iOutSize">will contain size of decoded data</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xF8DCB679, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryAtracEsSize(SceMpeg* Mpeg, int* iEsSize, int* iOutSize)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegGetAtracAu
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pStream">associated stream</param>
		/// <param name="pAu">will contain pointer to Au</param>
		/// <param name="pUnk">unknown</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xE1CE83A7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegGetAtracAu(SceMpeg* Mpeg, SceMpegStream* pStream, SceMpegAu* pAu, void* pUnk)
		{
			//throw(new NotImplementedException());
			return 0;
		}


		/// <summary>
		/// sceMpegAtracDecode
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pAu">video Au</param>
		/// <param name="pBuffer">buffer that will contain the decoded frame</param>
		/// <param name="iInit">set this to 1 on first call</param>
		/// <returns>
		///		0 if success.
		/// </returns>
		[HlePspFunction(NID = 0x800C44DF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAtracDecode(SceMpeg* Mpeg, SceMpegAu* pAu, void* pBuffer, int iInit)
		{
			//throw(new NotImplementedException());
			return -1;
		}

	}
}

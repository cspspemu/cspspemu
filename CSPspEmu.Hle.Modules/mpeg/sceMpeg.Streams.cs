using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.mpeg
{
	unsafe public partial class sceMpeg
	{

		/// <summary>
		/// sceMpegUnRegistStream
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pStream">pointer to stream</param>
		[HlePspFunction(NID = 0x591A4AA2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceMpegUnRegistStream(SceMpeg* Mpeg, SceMpegStream* pStream)
		{
			//throw(new NotImplementedException());
		}

		/// <summary>
		/// sceMpegRegistStream
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="iStreamID">stream id, 0 for video, 1 for audio</param>
		/// <param name="iUnk">unknown, set to 0</param>
		/// <returns>0 if error.</returns>
		[HlePspFunction(NID = 0x42560F23, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		//public SceMpegStream* sceMpegRegistStream(SceMpeg* Mpeg, int iStreamID, int iUnk)
		public uint sceMpegRegistStream(SceMpeg* Mpeg, StreamId iStreamID, int iUnk)
		{
			var SceMpegData = GetSceMpegData(Mpeg);

			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegQueryStreamOffset
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pBuffer">pointer to file header</param>
		/// <param name="iOffset">will contain stream offset in bytes, usually 2048</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x21FF80E4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryStreamOffset(SceMpeg* Mpeg, void* pBuffer, int* iOffset)
		{
			//throw(new NotImplementedException());
			*iOffset = 0;
			return 0;
		}

		/// <summary>
		/// sceMpegQueryStreamSize
		/// </summary>
		/// <param name="pBuffer">pointer to file header</param>
		/// <param name="iSize">will contain stream size in bytes</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x611E9E11, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryStreamSize(void* pBuffer, int* iSize)
		{
			//throw(new NotImplementedException());

			*iSize = 0;

			return 0;
		}


		/// <summary>
		/// sceMpegFlushAllStreams
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x707B7629, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegFlushAllStream(SceMpeg* Mpeg)
		{
			//throw(new NotImplementedException());
			return -1;
		}
	}
}

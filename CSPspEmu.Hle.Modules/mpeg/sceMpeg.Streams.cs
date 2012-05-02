using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Hle.Formats.video;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.mpeg
{
	/// <summary>
	/// 
	/// </summary>
	/// <see cref="http://en.wikipedia.org/wiki/MPEG_program_stream"/>
	unsafe public partial class sceMpeg
	{
		public class StreamInfo
		{
			public StreamId StreamId;
			public int StreamIndex;
		}

		HleUidPoolSpecial<StreamInfo, int> RegisteredStreams = new HleUidPoolSpecial<StreamInfo, int>(FirstId: 0x17);

		/// <summary>
		/// sceMpegUnRegistStream
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pStream">pointer to stream</param>
		[HlePspFunction(NID = 0x591A4AA2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceMpegUnRegistStream(SceMpegPointer* Mpeg, int StreamInfoId)
		{
			RegisteredStreams.Remove(StreamInfoId);
			//throw(new NotImplementedException());
		}

		/// <summary>
		/// sceMpegRegistStream
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="StreamId">stream id, 0 for video, 1 for audio</param>
		/// <param name="StreamIndex">unknown, set to 0</param>
		/// <returns>the id, 0 on error.</returns>
		[HlePspFunction(NID = 0x42560F23, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		//public SceMpegStream* sceMpegRegistStream(SceMpeg* Mpeg, int iStreamID, int iUnk)
		public int sceMpegRegistStream(SceMpegPointer* Mpeg, StreamId StreamId, int StreamIndex)
		{
			CheckEnabledMpeg();

			var StreamInfoId = RegisteredStreams.Create(new StreamInfo()
			{
				StreamId = StreamId,
				StreamIndex = StreamIndex,
			});
			//Console.WriteLine(iStreamID);
			//return 0;

			//var SceMpegData = GetSceMpegData(Mpeg);

			//throw(new NotImplementedException());
			return StreamInfoId;
		}

		/// <summary>
		/// sceMpegQueryStreamOffset
		/// </summary>
		/// <param name="MpegPointer">SceMpeg handle</param>
		/// <param name="PmfHeader">Pointer to file header</param>
		/// <param name="Offset">Will contain the stream offset in bytes, usually 2048</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x21FF80E4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryStreamOffset(SceMpegPointer* MpegPointer, byte* PmfHeader, out uint Offset)
		{
			var Pmf = new Pmf().Load(new MemoryStream(PointerUtils.PointerToByteArray(PmfHeader, 2048)));

			var SceMpeg = MpegPointer->GetSceMpeg(PspMemory);

			SceMpeg->StreamSize = (int)(uint)Pmf.Header.StreamSize;

			Offset = (uint)Pmf.Header.StreamOffset;
			return 0;
		}

		/// <summary>
		/// sceMpegQueryStreamSize
		/// </summary>
		/// <param name="PmfHeader">pointer to file header</param>
		/// <param name="Size">will contain stream size in bytes</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x611E9E11, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryStreamSize(byte* PmfHeader, out uint Size)
		{
			var Pmf = new Pmf().Load(new MemoryStream(PointerUtils.PointerToByteArray(PmfHeader, 2048)));
			Size = Pmf.Header.StreamSize;
			//*Size = 0;
			return 0;
		}


		/// <summary>
		/// sceMpegFlushAllStreams
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x707B7629, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegFlushAllStream(SceMpegPointer* Mpeg)
		{
			//throw(new NotImplementedException());
			return 0;
		}
	}
}

namespace CSPspEmu.Hle.Modules.mpeg
{
	public unsafe partial class sceMpeg
	{
		/// <summary>
		/// sceMpegQueryAtracEsSize
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="ElementaryStreamSize">Will contain size of Es</param>
		/// <param name="OutputSize">Will contain size of decoded data</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0xF8DCB679, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryAtracEsSize(SceMpegPointer* Mpeg, out int ElementaryStreamSize, out int OutputSize)
		{
			ElementaryStreamSize = MPEG_ATRAC_ES_SIZE;
			OutputSize = MPEG_ATRAC_ES_OUTPUT_SIZE;
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegGetAtracAu
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="StreamId">Associated stream</param>
		/// <param name="MpegAccessUnit">Will contain pointer to Au</param>
		/// <param name="Atrac3PlusPointer">Pointer to ATRAC3plus stream (from PSMF file).</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0xE1CE83A7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegGetAtracAu(SceMpegPointer* Mpeg, StreamId StreamId, SceMpegAu* MpegAccessUnit, void* Atrac3PlusPointer)
		{
			//Mpeg->SceMpegData.

			throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
		}


		/// <summary>
		/// sceMpegAtracDecode
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="MpegAccessUnit">Video Au</param>
		/// <param name="OutputBuffer">Buffer that will contain the decoded frame</param>
		/// <param name="Init">Set this to 1 on first call</param>
		/// <returns>
		///		0 if successful.
		/// </returns>
		[HlePspFunction(NID = 0x800C44DF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAtracDecode(SceMpegPointer* Mpeg, SceMpegAu* MpegAccessUnit, byte* OutputBuffer, int Init)
		{
			throw (new SceKernelException(SceKernelErrors.ERROR_ATRAC_NO_DATA));
		}

	}
}

namespace CSPspEmu.Hle.Modules.mpeg
{
	/// <summary>
	/// 
	/// </summary>
	/// <see cref="http://en.wikipedia.org/wiki/Presentation_and_access_units"/>
	public unsafe partial class sceMpeg
	{
		/// <summary>
		/// Initializes a Mpeg Access Unit from an ElementaryStreamBuffer.
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="ElementaryStreamBuffer">Prevously allocated Es buffer</param>
		/// <param name="MpegAccessUnit">Will contain pointer to Au</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0x167AFD9E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegInitAu(SceMpegPointer* Mpeg, int ElementaryStreamBuffer, SceMpegAu* MpegAccessUnit)
		{
			CheckEnabledMpeg();

			MpegAccessUnit->PresentationTimestampBe = unchecked((uint)0);
			MpegAccessUnit->PresentationTimestampLe = unchecked((uint)0);
			MpegAccessUnit->DecodeTimestampBe = unchecked((uint)0);
			MpegAccessUnit->DecodeTimestampLe = unchecked((uint)0);
			MpegAccessUnit->EsBuffer = ElementaryStreamBuffer;

			if (ElementaryStreamBuffer >= 1 && ElementaryStreamBuffer <= AbvEsBufAllocated.Length && AbvEsBufAllocated[ElementaryStreamBuffer - 1])
			{
				MpegAccessUnit->AuSize = MPEG_AVC_ES_SIZE;
			}
			else
			{
				MpegAccessUnit->AuSize = MPEG_ATRAC_ES_SIZE;
			}

			return 0;
		}
	}
}

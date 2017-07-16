namespace cscodec.av
{
	public class AVComponentDescriptor
	{
		public ushort plane = 2;            ///< which of the 4 planes contains the component

		/**
		 * Number of elements between 2 horizontally consecutive pixels minus 1.
		 * Elements are bits for bitstream formats, bytes otherwise.
		 */
		public ushort step_minus1 = 3;

		/**
		 * Number of elements before the component of the first pixel plus 1.
		 * Elements are bits for bitstream formats, bytes otherwise.
		 */
		public ushort offset_plus1 = 3;
		public ushort shift = 3;            ///< number of least significant bits that must be shifted away to get the value
		public ushort depth_minus1 = 4;            ///< number of bits in the component minus 1

		public AVComponentDescriptor(ushort _plane, ushort _step_minus1, ushort _offset_plus1, ushort _shift, ushort _depth_minus1)
		{
			this.plane = _plane;
			this.step_minus1 = _step_minus1;
			this.offset_plus1 = _offset_plus1;
			this.shift = _shift;
			this.depth_minus1 = _depth_minus1;
		}

	}
}
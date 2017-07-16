namespace cscodec.av
{
	public class AVPicture
	{
		/**
		 * four components are given, that's all.
		 * the last component is alpha
		 */
		public byte[][] data_base = new byte[4][];
		public int[] data_offset = new int[4];
		public int[] linesize = new int[4];       ///< number of bytes per line
	}
}
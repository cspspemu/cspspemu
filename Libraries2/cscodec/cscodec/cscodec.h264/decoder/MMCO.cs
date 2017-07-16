namespace cscodec.h264.decoder
{
	public class MMCO
	{
		public const int MMCO_END = 0;
		public const int MMCO_SHORT2UNUSED = 1;
		public const int MMCO_LONG2UNUSED = 2;
		public const int MMCO_SHORT2LONG = 3;
		public const int MMCO_SET_MAX_LONG = 4;
		public const int MMCO_RESET = 5;
		public const int MMCO_LONG = 6;

		public int opcode;
		public int short_pic_num;  ///< pic_num without wrapping (pic_num & max_pic_num)
		public int long_arg;       ///< index, pic_num, or num long refs depending on opcode
	}
}
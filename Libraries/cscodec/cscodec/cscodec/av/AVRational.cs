namespace cscodec.av
{
	public class AVRational
	{
		public int num; ///< numerator
		public int den; ///< denominator

		public AVRational(int _num, int _den)
		{
			this.num = _num;
			this.den = _den;
		}

		public AVRational(long _num, long _den)
		{
			this.num = (int)_num;
			this.den = (int)_den;
		}
	}
}
namespace CSPspEmu.Core.Audio
{
	public struct StereoShortSoundSample
	{
		public short Left;  // Left audio
		public short Right; // Right audio

		public StereoShortSoundSample(short Left, short Right)
		{
			this.Left = Left;
			this.Right = Right;
		}

		public static StereoShortSoundSample Mix(StereoShortSoundSample A, StereoShortSoundSample B)
		{
			return new StereoShortSoundSample((short)((A.Left + B.Left) / 2), (short)((A.Right + B.Right) / 2));
		}

		public static implicit operator StereoIntSoundSample(StereoShortSoundSample StereoShortSoundSample)
		{
			return new StereoIntSoundSample((int)StereoShortSoundSample.Left, (int)StereoShortSoundSample.Right);
		}


		public short MonoLeftRight {
			set
			{
				Left = value;
				Right = value;
			}
		}
	}

	public struct StereoIntSoundSample
	{
		public int Left;  // Left audio
		public int Right; // Right audio

		public StereoIntSoundSample(int Left, int Right)
		{
			this.Left = Left;
			this.Right = Right;
		}

		public static StereoIntSoundSample Mix(StereoIntSoundSample A, StereoIntSoundSample B)
		{
			return new StereoIntSoundSample((int)((A.Left + B.Left) / 2), (int)((A.Right + B.Right) / 2));
		}

		public static StereoIntSoundSample operator +(StereoIntSoundSample A, StereoIntSoundSample B)
		{
			return new StereoIntSoundSample((A.Left + B.Left), (A.Right + B.Right));
		}

		public static StereoIntSoundSample operator /(StereoIntSoundSample A, int Div)
		{
			return new StereoIntSoundSample((A.Left / Div), (A.Right / Div));
		}

		public static implicit operator StereoShortSoundSample(StereoIntSoundSample StereoIntSoundSample)
		{
			return new StereoShortSoundSample((short)StereoIntSoundSample.Left, (short)StereoIntSoundSample.Right);
		}

		public int MonoLeftRight
		{
			set
			{
				Left = value;
				Right = value;
			}
		}
	}
}

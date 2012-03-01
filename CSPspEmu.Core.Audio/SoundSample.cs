using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Audio
{
	public struct StereoShortSoundSample
	{
		public short Left;
		public short Right;

		public StereoShortSoundSample(short Left, short Right)
		{
			this.Left = Left;
			this.Right = Right;
		}

		static public StereoShortSoundSample Mix(StereoShortSoundSample A, StereoShortSoundSample B)
		{
			return new StereoShortSoundSample((short)((A.Left + B.Left) / 2), (short)((A.Right + B.Right) / 2));
		}

		static public implicit operator StereoIntSoundSample(StereoShortSoundSample StereoShortSoundSample)
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
		public int Left;
		public int Right;

		public StereoIntSoundSample(int Left, int Right)
		{
			this.Left = Left;
			this.Right = Right;
		}

		static public StereoIntSoundSample Mix(StereoIntSoundSample A, StereoIntSoundSample B)
		{
			return new StereoIntSoundSample((int)((A.Left + B.Left) / 2), (int)((A.Right + B.Right) / 2));
		}

		static public StereoIntSoundSample operator +(StereoIntSoundSample A, StereoIntSoundSample B)
		{
			return new StereoIntSoundSample((A.Left + B.Left), (A.Right + B.Right));
		}

		static public StereoIntSoundSample operator /(StereoIntSoundSample A, int Div)
		{
			return new StereoIntSoundSample((A.Left / Div), (A.Right / Div));
		}

		static public implicit operator StereoShortSoundSample(StereoIntSoundSample StereoIntSoundSample)
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

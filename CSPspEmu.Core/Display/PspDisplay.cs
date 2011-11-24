using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Rtc;

namespace CSPspEmu.Core.Display
{
	public class PspDisplay
	{
		public PspRtc HlePspRtc;

		public Info CurrentInfo = new Info()
		{
			Address = 0x04000000,
			BufferWidth = 512,
			PixelFormat = PixelFormats.RGBA_8888,
			Sync = SyncMode.Immediate,
			Mode = 0,
			Width = 480,
			Height = 272,
		};

		public enum SyncMode : int
		{
			Immediate = 0,
			NextFrame = 1,
		}

		public enum PixelFormats : int
		{
			RGB_565 = 0,
			RGBA_5551 = 1,
			RGBA_4444 = 2,
			RGBA_8888 = 3,
		}

		public struct Info {
			public uint Address;
			public int BufferWidth;
			public PixelFormats PixelFormat;
			public SyncMode Sync;
			public int Mode;
			public int Width;
			public int Height;
		}

		public PspDisplay(PspRtc HlePspRtc)
		{
			this.HlePspRtc = HlePspRtc;
		}

		public int VblankCount
		{
			get
			{
				//this.HlePspRtc.Elapsed
				return 0;
			}
		}
	}
}

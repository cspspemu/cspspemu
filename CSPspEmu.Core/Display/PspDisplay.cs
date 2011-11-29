using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Core.Threading.Synchronization;

namespace CSPspEmu.Core.Display
{
	public class PspDisplay : PspEmulatorComponent
	{
		public PspRtc HlePspRtc;

		public Info CurrentInfo = new Info()
		{
			Address = 0x04000000,
			BufferWidth = 512,
			PixelFormat = GuPixelFormats.RGBA_8888,
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

		public struct Info {
			public uint Address;
			public int BufferWidth;
			public GuPixelFormats PixelFormat;
			public SyncMode Sync;
			public int Mode;
			public int Width;
			public int Height;
		}

		public override void InitializeComponent()
		{
			this.HlePspRtc = PspEmulatorContext.GetInstance<PspRtc>();
		}

		public PspWaitEvent VBlankEvent = new PspWaitEvent();

		private int _VblankCount = 0;

		public int VblankCount
		{
			set
			{
				_VblankCount++;
			}
			get
			{
				//this.HlePspRtc.Elapsed
				return _VblankCount;
			}
		}
	}
}

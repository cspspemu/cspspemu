using System;
using System.Drawing;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Core.Threading.Synchronization;
using CSPspEmu.Core.Utils;

namespace CSPspEmu.Core.Display
{
	public class PspDisplay : PspEmulatorComponent
	{
		public const double ProcessedPixelsPerSecond = 9000000; // hz
		public const double CyclesPerPixel           = 1;
		public const double PixelsInARow             = 525;
		public const double VsyncRow                 = 272;
		public const double NumberOfRows             = 286;
	
		public const double HorizontalSyncHertz = (ProcessedPixelsPerSecond * CyclesPerPixel) / PixelsInARow;
		public const double VeritcalSyncHertz = HorizontalSyncHertz / NumberOfRows;

		[Inject]
		public PspRtc PspRtc;

		[Inject]
		public PspMemory Memory;

		public Info CurrentInfo = new Info()
		{
			FrameAddress = 0x04000000,
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

		public struct Info 
		{
			public uint FrameAddress;
			public int BufferWidth;
			public GuPixelFormats PixelFormat;
			public SyncMode Sync;
			public int Mode;
			public int Width;
			public int Height;

			public int BufferWidthHeightCount
			{
				get
				{
					return BufferWidth * Height;
				}
			}
		}

		public override void InitializeComponent()
		{
		}

		public void TriggerVBlankStart()
		{
			VBlankEvent.Signal();
			if (VBlankEventCall != null) VBlankEventCall();
			VblankCount++;
			IsVblank = true;
		}

		public void TriggerVBlankEnd()
		{
			IsVblank = false;
		}

		public PspWaitEvent VBlankEvent = new PspWaitEvent();
		public event Action VBlankEventCall;

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

		public unsafe Bitmap TakeScreenshot()
		{
			return new PspBitmap(
				CurrentInfo.PixelFormat,
				CurrentInfo.BufferWidth,
				CurrentInfo.Height,
				(byte*)Memory.PspAddressToPointerSafe(
					CurrentInfo.FrameAddress,
					PixelFormatDecoder.GetPixelsSize(CurrentInfo.PixelFormat, CurrentInfo.BufferWidth * CurrentInfo.Height)
				)
			).ToBitmap();
		}

		public bool IsVblank { get; protected set; }
	}
}

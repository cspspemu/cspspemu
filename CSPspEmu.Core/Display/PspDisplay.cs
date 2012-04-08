using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Core.Threading.Synchronization;
using CSharpUtils;
using CSharpUtils.Extensions;
using System.Drawing;
using CSPspEmu.Core.Utils;
using CSPspEmu.Core.Memory;
using System.Drawing.Imaging;

namespace CSPspEmu.Core.Display
{
	public class PspDisplay : PspEmulatorComponent
	{
		public const double processed_pixels_per_second = 9000000; // hz
		public const double cycles_per_pixel            = 1;
		public const double pixels_in_a_row             = 525;
		public const double vsync_row                   = 272;
		public const double number_of_rows              = 286;
	
		public const double hsync_hz = (processed_pixels_per_second * cycles_per_pixel) / pixels_in_a_row;
		public const double vsync_hz = hsync_hz / number_of_rows;

		public PspRtc PspRtc;
		public PspMemory Memory;

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

		public struct Info 
		{
			public uint Address;
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
			this.PspRtc = PspEmulatorContext.GetInstance<PspRtc>();
			this.Memory = PspEmulatorContext.GetInstance<PspMemory>();
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

		unsafe public Bitmap TakeScreenshot()
		{
			return new PspBitmap(
				CurrentInfo.PixelFormat,
				CurrentInfo.BufferWidth,
				CurrentInfo.Height,
				(byte*)Memory.PspAddressToPointerSafe(
					CurrentInfo.Address,
					PixelFormatDecoder.GetPixelsSize(CurrentInfo.PixelFormat, CurrentInfo.BufferWidth * CurrentInfo.Height)
				)
			).ToBitmap();
		}

		public bool IsVblank { get; protected set; }
	}
}

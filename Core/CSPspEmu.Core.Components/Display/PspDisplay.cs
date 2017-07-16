using System;
using System.Drawing;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Core.Threading.Synchronization;
using CSPspEmu.Core.Utils;
using CSPspEmu.Core.Types;
using System.Collections.Generic;

namespace CSPspEmu.Core.Display
{
	public class PspDisplay
	{
		public const double ProcessedPixelsPerSecond = 9000000; // hz
		public const double CyclesPerPixel           = 1;
		public const double PixelsInARow             = 525;
		public const double VsyncRow                 = 272;
		public const double NumberOfRows             = 286;
		public const float HCountPerVblank = 285.72f;


		public const double HorizontalSyncHertz = (ProcessedPixelsPerSecond * CyclesPerPixel) / PixelsInARow;
		public const double VerticalSyncHertz = HorizontalSyncHertz / NumberOfRows;

		[Inject]
		public PspRtc PspRtc;

		[Inject]
		public PspMemory Memory;

		private PspDisplay()
		{
		}

		public Info CurrentInfo = new Info()
		{
			Enabled = true,
			FrameAddress = 0x04000000,
			BufferWidth = 512,
			PixelFormat = GuPixelFormats.RGBA_8888,
			//Sync = SyncMode.Immediate,
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
			public bool Enabled;
			public bool PlayingVideo;
			public uint FrameAddress;
			public int BufferWidth;
			public GuPixelFormats PixelFormat;
			//public SyncMode Sync;
			public int Mode;
			public int Width;
			public int Height;

			public int BufferWidthHeightCount => BufferWidth * Height;
		}

		private DateTime _startDrawTime;

		static public event Action DrawEvent;
		public void TriggerDrawStart()
		{
			_startDrawTime = DateTime.UtcNow;
			if (DrawEvent != null) DrawEvent();
		}

		public int GetHCount()
		{
			var elaspedTime = DateTime.UtcNow - _startDrawTime;
			return (int)(elaspedTime.TotalSeconds / (1 / HorizontalSyncHertz));
		}

		static public event Action VBlankCallback;

		public void VBlankCallbackOnce(Action callback)
		{
			Action Callback2 = null;
			Callback2  = () =>
			{
				VBlankCallback -= Callback2;
				callback();
			};
			VBlankCallback += Callback2;
		}

		public void TriggerVBlankStart()
		{
			if (VBlankCallback != null) VBlankCallback();
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

		private int _vblankCount = 0;

		public int VblankCount
		{
			set
			{
				_vblankCount = value;
			}
			get
			{
				//this.HlePspRtc.Elapsed
				return _vblankCount;
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

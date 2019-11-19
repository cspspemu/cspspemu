using System;
using System.Drawing;
using CSPspEmu.Core.Components.Rtc;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Threading.Synchronization;
using CSPspEmu.Core.Types;
using CSPspEmu.Utils.Utils;

namespace CSPspEmu.Core.Components.Display
{
    public class PspDisplay
    {
        public const double ProcessedPixelsPerSecond = 9000000; // hz
        public const double CyclesPerPixel = 1;
        public const double PixelsInARow = 525;
        public const double VsyncRow = 272;
        public const double NumberOfRows = 286;
        public const float HCountPerVblank = 285.72f;
        public const int MaxBufferWidth = 512;
        public const int MaxBufferHeight = 272;
        public const int MaxBufferArea = MaxBufferWidth * MaxBufferHeight;
        public const int MaxVisibleWidth = 480;
        public const int MaxVisibleHeight = 272;
        public const int MaxVisibleArea = MaxVisibleWidth * MaxVisibleHeight;


        public const double HorizontalSyncHertz = (ProcessedPixelsPerSecond * CyclesPerPixel) / PixelsInARow;
        public const double VerticalSyncHertz = HorizontalSyncHertz / NumberOfRows;

        [Inject] PspRtc _pspRtc;

        [Inject] PspMemory _memory;

        private PspDisplay()
        {
        }

        public Info CurrentInfo = new Info()
        {
            Enabled = true,
            FrameAddress = 0x04000000,
            BufferWidth = 512,
            PixelFormat = GuPixelFormats.Rgba8888,
            //Sync = SyncMode.Immediate,
            Mode = 0,
            Width = 480,
            Height = 272,
        };

        public enum SyncMode
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

        public static event Action DrawEvent;

        public void TriggerDrawStart()
        {
            _startDrawTime = DateTime.UtcNow;
            DrawEvent?.Invoke();
        }

        public int GetHCount()
        {
            var elaspedTime = DateTime.UtcNow - _startDrawTime;
            return (int) (elaspedTime.TotalSeconds / (1 / HorizontalSyncHertz));
        }

        public static event Action VBlankCallback;

        public void VBlankCallbackOnce(Action callback)
        {
            void Action()
            {
                VBlankCallback -= Action;
                callback();
            }

            VBlankCallback += Action;
        }

        public void TriggerVBlankStart()
        {
            VBlankCallback?.Invoke();
            VBlankEvent.Signal();
            VBlankEventCall?.Invoke();
            VblankCount++;
            IsVblank = true;
        }

        public void TriggerVBlankEnd()
        {
            IsVblank = false;
        }

        public PspWaitEvent VBlankEvent = new PspWaitEvent();
        public event Action VBlankEventCall;

        private int _vblankCount;

        public int VblankCount
        {
            set => _vblankCount = value;
            get => _vblankCount;
        }

        public unsafe PspBitmap TakePspScreenshot()
        {
            return new PspBitmap(
                CurrentInfo.PixelFormat,
                CurrentInfo.BufferWidth,
                CurrentInfo.Height,
                (byte*) _memory.PspAddressToPointerSafe(
                    CurrentInfo.FrameAddress,
                    PixelFormatDecoder.GetPixelsSize(CurrentInfo.PixelFormat,
                        CurrentInfo.BufferWidth * CurrentInfo.Height)
                )
            );
        }

        public unsafe Bitmap TakeScreenshot() => TakePspScreenshot().ToBitmap();

        public bool IsVblank { get; protected set; }
    }
}
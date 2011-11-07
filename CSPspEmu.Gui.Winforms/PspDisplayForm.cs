using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSharpUtils.Extensions;
using CSharpUtils;
using CSPspEmu.Core;

namespace CSPspEmu.Gui.Winforms
{
	unsafe public partial class PspDisplayForm : Form
	{
		public Bitmap Buffer = new Bitmap(512, 272);
		public Graphics BufferGraphics;
		public AbstractPspMemory Memory;

		public PspDisplayForm(AbstractPspMemory Memory)
		{
			this.Memory = Memory;

			InitializeComponent();
			SetClientSizeCore(480, 272);
			MinimumSize = Size;
			MaximumSize = Size;
			
			CenterToScreen();
			BufferGraphics = Graphics.FromImage(Buffer);
			BufferGraphics.Clear(Color.Red);

			var Timer = new Timer();
			Timer.Interval = 1000 / 60;
			Timer.Tick += new EventHandler(Timer_Tick);
			Timer.Start();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
		}

		protected override void OnPaintBackground(PaintEventArgs PaintEventArgs)
		{
			Buffer.LockBitsUnlock(System.Drawing.Imaging.PixelFormat.Format32bppArgb, (BitmapData) =>
			{
				var BitmapDataPtr = (byte*)BitmapData.Scan0.ToPointer();
				var FrameBuffer = (byte*)Memory.PspAddressToPointer(AbstractPspMemory.FrameBufferOffset);
				var Count = 512 * 272;
				for (int n = 0; n < Count; n++)
				{
					BitmapDataPtr[n * 4 + 3] = 0xFF;
					BitmapDataPtr[n * 4 + 0] = FrameBuffer[n * 4 + 2];
					BitmapDataPtr[n * 4 + 1] = FrameBuffer[n * 4 + 1];
					BitmapDataPtr[n * 4 + 2] = FrameBuffer[n * 4 + 0];
				}
			});
			PaintEventArgs.Graphics.DrawImage(Buffer, new Point(0, 0));
			//base.OnPaintBackground(e);
		}

		void Timer_Tick(object sender, EventArgs e)
		{
			Refresh();
			//var g = Graphics.FromHdc(this.Handle);
			//g.DrawImage();
			//var Bitmap = new Bitmap();
			//Bitmap.LockBits(
		}
	}
}

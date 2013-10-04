using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
using CSPspEmu.Core.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace CSPspEmu.Gui.Winforms.Winforms
{
	unsafe public sealed class PspOpenglDisplayControl : UserControl, IGuiWindowInfo
	{
		//CommonGuiDisplayOpengl DisplayOpengl;

		public PspOpenglDisplayControl()
		{
			//this.CanFocus = false;
			//DisplayOpengl = new CommonGuiDisplayOpengl(PspDisplayForm.Singleton.IGuiExternalInterface, this);
		}

		public void ReDraw()
		{
			this.Refresh();
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			//base.OnPaintBackground(pevent);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);
			OnDrawFrame();
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			//this.Context.MakeCurrent();
		}

		private void PspOpenglDisplayControl_Load(object sender, EventArgs e)
		{

		}

		private byte[] OutputPixel = new byte[512 * 272 * 4];
		private Bitmap FramebufferBitmap = new Bitmap(512, 272);

		protected void OnDrawFrame()
		{
			var Size = new System.Drawing.Size(PspDisplayForm.Singleton.ClientSize.Width, PspDisplayForm.Singleton.ClientSize.Height - this.Top);
			var Top = PspDisplayForm.Singleton.MainMenuStripHeight;
			if (this.Top != Top) this.Top = Top;
			if (this.Size != Size) this.Size = Size;
			var DisplayConfig = PspDisplayForm.Singleton.PspDisplay.CurrentInfo;
			var Memory = PspDisplayForm.Singleton.Memory;
			try
			{
				var Graphics = this.CreateGraphics();

				fixed (byte* _OutputPixelPtr = OutputPixel)
				{
					OutputPixel* OutputPixelPtr = (OutputPixel*)_OutputPixelPtr;
					PixelFormatDecoder.Decode(DisplayConfig.PixelFormat, Memory.PspAddressToPointerSafe(DisplayConfig.FrameAddress), OutputPixelPtr, 512, 272);
					for (int n = 0; n < 512 * 272; n++) OutputPixelPtr[n].A = 0xFF;
				}

				FramebufferBitmap.SetChannelsDataInterleaved(OutputPixel, BitmapChannel.Red, BitmapChannel.Green, BitmapChannel.Blue, BitmapChannel.Alpha);
				Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
				Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
				Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
				Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				Graphics.DrawImage(FramebufferBitmap, new Rectangle(0, 0, Size.Width, Size.Height), new Rectangle(0, 0, 480, 272), GraphicsUnit.Pixel);
				
				//Graphics.FillRectangle(new SolidBrush(Color.Red), new Rectangle(0, 0, Size.Width, Size.Height));
				//PspMemory.FrameBufferOffset.
				//PspDisplayForm.Singleton.
				//DisplayOpengl.DrawVram(PspDisplayForm.Singleton.StoredConfig.EnableSmaa);
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLineColored(ConsoleColor.Red, "OnDrawFrame: {0}", Exception);
			}
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// PspOpenglDisplayControl
			// 
			//this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "PspOpenglDisplayControl";
			this.Size = new System.Drawing.Size(480, 272);
			//this.Load += new System.EventHandler(this.PspOpenglDisplayControl_Load);
			this.ResumeLayout(false);
		}

		public Bitmap GrabScreenshot()
		{
			return new Bitmap(480, 272);
		}

		bool IGuiWindowInfo.EnableRefreshing
		{
			get
			{
				return (PspDisplayForm.Singleton.WindowState != FormWindowState.Minimized) && PspDisplayForm.Singleton.EnableRefreshing;
			}
		}

		void IGuiWindowInfo.SwapBuffers()
		{
			//this.Context.SwapBuffers();
		}

		GuiRectangle IGuiWindowInfo.ClientRectangle
		{
			get { return new GuiRectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height); }
		}
	}
}

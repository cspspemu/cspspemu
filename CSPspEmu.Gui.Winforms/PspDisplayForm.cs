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
using System.Drawing.Imaging;
using CSPspEmu.Hle;
using System.Diagnostics;

namespace CSPspEmu.Gui.Winforms
{
	unsafe public partial class PspDisplayForm : Form
	{
		public Bitmap Buffer = new Bitmap(512, 272);
		public Graphics BufferGraphics;
		public PspMemory Memory;
		public PspDisplay PspDisplay;

		public PspDisplayForm(PspMemory Memory, PspDisplay PspDisplay)
		{
			this.Memory = Memory;
			this.PspDisplay = PspDisplay;

			InitializeComponent();
			DisplayScale = 2;

			BufferGraphics = Graphics.FromImage(Buffer);
			BufferGraphics.Clear(Color.Red);

			var Timer = new Timer();
			Timer.Interval = 1000 / 60;
			Timer.Tick += new EventHandler(Timer_Tick);
			Timer.Start();
		}

		private int _DisplayScale;

		public int DisplayScale
		{
			set
			{
				_DisplayScale = value;
				var InnerSize = new Size(480 * _DisplayScale, 272 * _DisplayScale);
				MinimumSize = new Size(1, 1);
				MaximumSize = new Size(2048, 2048);
				SetClientSizeCore(InnerSize.Width, InnerSize.Height);
				MinimumSize = MaximumSize = Size;
				CenterToScreen();

				xToolStripMenuItem1.Checked = (_DisplayScale == 1);
				xToolStripMenuItem2.Checked = (_DisplayScale == 2);
				xToolStripMenuItem3.Checked = (_DisplayScale == 3);
				xToolStripMenuItem4.Checked = (_DisplayScale == 4);
			}
			get
			{
				return _DisplayScale;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
		}

		protected override void OnPaintBackground(PaintEventArgs PaintEventArgs)
		{
			if (EnableRefreshing)
			{
				Buffer.LockBitsUnlock(System.Drawing.Imaging.PixelFormat.Format32bppArgb, (BitmapData) =>
				{
					var BitmapDataPtr = (byte*)BitmapData.Scan0.ToPointer();
					var Address = PspDisplay.CurrentInfo.Address;
					//Console.WriteLine("{0:X}", Address);
					var FrameBuffer = (byte*)Memory.PspAddressToPointer(Address);
					var Count = 512 * 272;
					for (int n = 0; n < Count; n++)
					{
						BitmapDataPtr[n * 4 + 3] = 0xFF;
						BitmapDataPtr[n * 4 + 0] = FrameBuffer[n * 4 + 2];
						BitmapDataPtr[n * 4 + 1] = FrameBuffer[n * 4 + 1];
						BitmapDataPtr[n * 4 + 2] = FrameBuffer[n * 4 + 0];
					}
				});
			}
			//Console.WriteLine(this.ClientRectangle);
			PaintEventArgs.Graphics.DrawImage(Buffer, this.ClientRectangle);
		}

		protected bool EnableRefreshing = true;

		void Timer_Tick(object sender, EventArgs e)
		{
			Refresh();
			//var g = Graphics.FromHdc(this.Handle);
			//g.DrawImage();
			//var Bitmap = new Bitmap();
			//Bitmap.LockBits(
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				EnableRefreshing = false;
				var SaveFileDialog = new SaveFileDialog();
				SaveFileDialog.Filter = "PNG|*.png|All Files|*.*";
				SaveFileDialog.FileName = "screenshot.png";
				SaveFileDialog.AddExtension = true;
				SaveFileDialog.DefaultExt = "png";
				if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					var Buffer2 = new Bitmap(480, 272);
					Graphics.FromImage(Buffer2).DrawImage(Buffer, Point.Empty);
					Buffer2.Save(SaveFileDialog.FileName, ImageFormat.Png);
				}
			}
			finally
			{
				EnableRefreshing = true;
			}
		}

		private void PspDisplayForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.D1)
			{
				DisplayScale = 1;
			}
			if (e.KeyCode == Keys.D2)
			{
				DisplayScale = 2;
			}
			if (e.KeyCode == Keys.D3)
			{
				DisplayScale = 3;
			}
			if (e.KeyCode == Keys.D4)
			{
				DisplayScale = 4;
			}

			Console.WriteLine(e);
		}

		private void frameSkippingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.PspDisplay.Vsync = frameSkippingToolStripMenuItem.Checked;
		}

		private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start("http://cspspemu.soywiz.com/");
		}

		private void xToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			DisplayScale = 1;
		}

		private void xToolStripMenuItem2_Click(object sender, EventArgs e)
		{
			DisplayScale = 2;
		}

		private void xToolStripMenuItem3_Click(object sender, EventArgs e)
		{
			DisplayScale = 3;
		}

		private void xToolStripMenuItem4_Click(object sender, EventArgs e)
		{
			DisplayScale = 4;
		}
	}
}

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
using System.Diagnostics;
using System.Drawing.Drawing2D;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Gui.Winforms
{
	unsafe public partial class PspDisplayForm : Form
	{
		public Bitmap Buffer = new Bitmap(512, 272);
		public Graphics BufferGraphics;
		public SceCtrlData SceCtrlData;
		public IGuiExternalInterface IGuiExternalInterface;

		public PspMemory Memory { get { return IGuiExternalInterface.GetMemory(); } }
		public PspDisplay PspDisplay { get { return IGuiExternalInterface.GetDisplay(); } }
		public PspController PspController { get { return IGuiExternalInterface.GetController(); } }
		

		public void SendControllerFrame()
		{
			this.PspController.InsertSceCtrlData(SceCtrlData);
		}

		public PspDisplayForm(IGuiExternalInterface IGuiExternalInterface)
		{
			this.IGuiExternalInterface = IGuiExternalInterface;

			InitializeComponent();
			DisplayScale = 1;

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
				var InnerSize = new Size(480 * _DisplayScale, 272 * _DisplayScale + menuStrip1.Height);
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
				try
				{
					Buffer.LockBitsUnlock(System.Drawing.Imaging.PixelFormat.Format32bppArgb, (BitmapData) =>
					{
						var BitmapDataPtr = (byte*)BitmapData.Scan0.ToPointer();
						var Address = PspDisplay.CurrentInfo.Address;
						//var Address = Memory.FrameBufferSegment.Low;
						//Console.WriteLine("{0:X}", Address);
						var FrameBuffer = (byte*)Memory.PspAddressToPointer(Address);
						var Count = 512 * 272;
						switch (PspDisplay.CurrentInfo.PixelFormat)
						{
							case PspDisplay.PixelFormats.RGBA_8888:
								for (int n = 0; n < Count; n++)
								{
									BitmapDataPtr[n * 4 + 3] = 0xFF;
									BitmapDataPtr[n * 4 + 0] = FrameBuffer[n * 4 + 2];
									BitmapDataPtr[n * 4 + 1] = FrameBuffer[n * 4 + 1];
									BitmapDataPtr[n * 4 + 2] = FrameBuffer[n * 4 + 0];
								}
								break;
							case PspDisplay.PixelFormats.RGBA_5551:
								for (int n = 0; n < Count; n++)
								{
									ushort Value = *(ushort*)&FrameBuffer[n * 2];
									BitmapDataPtr[n * 4 + 3] = 0xFF;
									BitmapDataPtr[n * 4 + 0] = (byte)Value.ExtractUnsignedScale(10, 5, 255);
									BitmapDataPtr[n * 4 + 1] = (byte)Value.ExtractUnsignedScale(5, 5, 255);
									BitmapDataPtr[n * 4 + 2] = (byte)Value.ExtractUnsignedScale(0, 5, 255);
								}
								break;
							default:
								//throw(new NotImplementedException("Not implemented PixelFormat '" + PspDisplay.CurrentInfo.PixelFormat + "'"));
								Console.Error.WriteLine("Not implemented PixelFormat '" + PspDisplay.CurrentInfo.PixelFormat + "'");
								break;
						}
					});
				}
				catch (Exception Exception)
				{
					Console.WriteLine(Exception);
				}
			}
			//Console.WriteLine(this.ClientRectangle);
			PaintEventArgs.Graphics.CompositingMode = CompositingMode.SourceCopy;
			PaintEventArgs.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
			PaintEventArgs.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			PaintEventArgs.Graphics.DrawImage(
				Buffer,
				new Rectangle(
					0, menuStrip1.Height,
					512 * DisplayScale, 272 * DisplayScale
				)
			);
			//PaintEventArgs.Graphics.DrawImageUnscaled(Buffer, new Point(0, menuStrip1.Height));
		}

		protected bool EnableRefreshing = true;

		void Timer_Tick(object sender, EventArgs e)
		{
			SendControllerFrame();
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

		protected void PauseResume(Action Action)
		{
			try
			{
				EnableRefreshing = false;
				IGuiExternalInterface.PauseResume(Action);
			}
			finally
			{
				EnableRefreshing = true;
			}
		}

		private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
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
			});
		}

		/*
		public void UpdateFlags(PspCtrlButtons PspCtrlButtons)
		{
			SceCtrlData.Buttons
		}
		*/

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

		private void dumpRamToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				var SaveFileDialog = new SaveFileDialog();
				SaveFileDialog.Filter = "DUMP|*.dump|All Files|*.*";
				SaveFileDialog.FileName = String.Format("memory-{0}.dump", (long)(DateTime.UtcNow - new DateTime(0)).TotalMilliseconds);
				SaveFileDialog.AddExtension = true;
				SaveFileDialog.DefaultExt = "dump";
				if (SaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					var Stream = SaveFileDialog.OpenFile();
					Stream.WriteStream(new PspMemoryStream(Memory).SliceWithBounds(Memory.MainSegment.Low, Memory.MainSegment.High - 1));
					Stream.Flush();
					Stream.Close();
				}
			});
		}

		private PspCtrlButtons GetButtonsFromKeys(Keys Key)
		{
			switch (Key)
			{
				case Keys.W: return PspCtrlButtons.Triangle;
				case Keys.S: return PspCtrlButtons.Cross;
				case Keys.A: return PspCtrlButtons.Square;
				case Keys.D: return PspCtrlButtons.Circle;
				case Keys.Q: return PspCtrlButtons.LeftTrigger;
				case Keys.E: return PspCtrlButtons.RightTrigger;
				case Keys.Up: return PspCtrlButtons.Up;
				case Keys.Return: return PspCtrlButtons.Start;
				case Keys.Space: return PspCtrlButtons.Select;
				case Keys.Right: return PspCtrlButtons.Right;
				case Keys.Down: return PspCtrlButtons.Down;
				case Keys.Left: return PspCtrlButtons.Left;
			}
			return PspCtrlButtons.None;
		}

		private void PspDisplayForm_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.D1: DisplayScale = 1; break;
				case Keys.D2: DisplayScale = 2; break;
				case Keys.D3: DisplayScale = 3; break;
				case Keys.D4: DisplayScale = 4; break;
			}

			SceCtrlData.UpdateButtons(GetButtonsFromKeys(e.KeyCode), true);
		}

		private void PspDisplayForm_KeyUp(object sender, KeyEventArgs e)
		{
			SceCtrlData.UpdateButtons(GetButtonsFromKeys(e.KeyCode), false);
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PauseResume(() =>
			{
				var OpenFileDialog = new OpenFileDialog();
				OpenFileDialog.Filter = "Compatible Formats (*.elf, *.pbp, *.iso, *.cso, *.dax)|*.elf;*.pbp;*.iso;*.cso;*.dax|All Files|*.*";
				if (OpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					IGuiExternalInterface.LoadFile(OpenFileDialog.FileName);
				}
			});
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new AboutForm().ShowDialog();
		}

		private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.Resume();
		}

		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.Pause();
		}
	}
}

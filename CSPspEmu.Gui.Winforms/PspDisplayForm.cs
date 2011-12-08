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
using CSPspEmu.Core.Controller;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Utils;

namespace CSPspEmu.Gui.Winforms
{
	unsafe public partial class PspDisplayForm : Form
	{
		/// <summary>
		/// 
		/// </summary>
		public Bitmap Buffer = new Bitmap(512, 272);

		/// <summary>
		/// 
		/// </summary>
		public Graphics BufferGraphics;

		/// <summary>
		/// 
		/// </summary>
		public SceCtrlData SceCtrlData;

		/// <summary>
		/// 
		/// </summary>
		public IGuiExternalInterface IGuiExternalInterface;

		/// <summary>
		/// 
		/// </summary>
		public PspMemory Memory { get { return IGuiExternalInterface.GetMemory(); } }

		/// <summary>
		/// 
		/// </summary>
		public PspDisplay PspDisplay { get { return IGuiExternalInterface.GetDisplay(); } }

		/// <summary>
		/// 
		/// </summary>
		public PspConfig PspConfig { get { return IGuiExternalInterface.GetConfig(); } }

		/// <summary>
		/// 
		/// </summary>
		public PspController PspController { get { return IGuiExternalInterface.GetController(); } }

		float AnalogX = 0.0f, AnalogY = 0.0f;

		public void SendControllerFrame()
		{
			if (IGuiExternalInterface.IsInitialized())
			{
				SceCtrlData.X = 0;
				SceCtrlData.Y = 0;

				bool AnalogXUpdated = false;
				bool AnalogYUpdated = false;
				if (AnalogUp) { AnalogY -= 0.1f; AnalogYUpdated = true; }
				if (AnalogDown) { AnalogY += 0.1f; AnalogYUpdated = true; }
				if (AnalogLeft) { AnalogX -= 0.1f; AnalogXUpdated = true; }
				if (AnalogRight) { AnalogX += 0.1f; AnalogXUpdated = true; }
				if (!AnalogXUpdated) AnalogX /= 2.0f;
				if (!AnalogYUpdated) AnalogY /= 2.0f;

				AnalogX = MathFloat.Clamp(AnalogX, -1.0f, 1.0f);
				AnalogY = MathFloat.Clamp(AnalogY, -1.0f, 1.0f);

				//Console.WriteLine("{0}, {1}", AnalogX, AnalogY);

				SceCtrlData.X = AnalogX;
				SceCtrlData.Y = AnalogY;

				this.PspController.InsertSceCtrlData(SceCtrlData);
			}
		}

		public PspDisplayForm(IGuiExternalInterface IGuiExternalInterface)
		{
			this.IGuiExternalInterface = IGuiExternalInterface;

			InitializeComponent();
			DisplayScale = 1;

			BufferGraphics = Graphics.FromImage(Buffer);
			//BufferGraphics.Clear(Color.Red);
			BufferGraphics.Clear(Color.Black);

			updateResumePause();
			updateDebugSyscalls();
			updateDebugGpu();

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

		public struct BGRA
		{
			public byte B, G, R, A;
		}

		protected override void OnPaintBackground(PaintEventArgs PaintEventArgs)
		{
			if (IGuiExternalInterface.IsInitialized())
			{
				if (EnableRefreshing)
				{
					try
					{
						Buffer.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
						{
							int Width = 512;
							int Height = 272;
							var Count = Width * Height;
							var BitmapDataDecode = new PixelFormatDecoder.OutputPixel[Count];
							fixed (PixelFormatDecoder.OutputPixel* BitmapDataDecodePtr = BitmapDataDecode)
							{
								var BitmapDataPtr = (BGRA*)BitmapData.Scan0.ToPointer();
								var Address = PspDisplay.CurrentInfo.Address;
								//var Address = Memory.FrameBufferSegment.Low;
								//Console.WriteLine("{0:X}", Address);
								var FrameBuffer = (byte*)Memory.PspAddressToPointer(Address);

								//var LastRow = (FrameBuffer + 512 * 260 * 4 + 4 * 10);
								//Console.WriteLine("{0},{1},{2},{3}", LastRow[0], LastRow[1], LastRow[2], LastRow[3]);

								if (FrameBuffer == null)
								{
									Console.Error.WriteLine("FrameBuffer == null");
								}
								else if (BitmapDataPtr == null)
								{
									Console.Error.WriteLine("BitmapDataPtr == null");
								}
								else
								{
									PixelFormatDecoder.Decode(
										PspDisplay.CurrentInfo.PixelFormat,
										(void*)FrameBuffer,
										BitmapDataDecodePtr,
										Width, Height
									);
								}

								// Converts the decoded data to Window's format.
								for (int n = 0; n < Count; n++)
								{
									BitmapDataPtr[n].R = BitmapDataDecodePtr[n].R;
									BitmapDataPtr[n].G = BitmapDataDecodePtr[n].G;
									BitmapDataPtr[n].B = BitmapDataDecodePtr[n].B;
									BitmapDataPtr[n].A = 0xFF;
								}
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
			else
			{
				var Buffer = new Bitmap(512, 272);
				var BufferGraphics = Graphics.FromImage(Buffer);
				BufferGraphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, Buffer.Width, Buffer.Height));
				//BufferGraphics.DrawString("Initializing...", new Font("Arial", 10), new SolidBrush(Color.White), new PointF(8, 8));
				PaintEventArgs.Graphics.DrawImage(Buffer, new Rectangle(0, menuStrip1.Height, 512 * DisplayScale, 272 * DisplayScale));
			}
		}

		protected bool EnableRefreshing = true;

		void Timer_Tick(object sender, EventArgs e)
		{
			SendControllerFrame();
			Refresh();

			if (IGuiExternalInterface.IsInitialized())
			{
				PspDisplay.VBlankEvent.Signal();
				PspDisplay.VblankCount++;
			}
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
			this.PspConfig.VerticalSynchronization = frameSkippingToolStripMenuItem.Checked;
		}

		private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://en.blog.cballesterosvelasco.es/search/label/pspemu/?rf=csp");
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

		//float AnalogX, AnalogY;
		bool AnalogUp = false;
		bool AnalogDown = false;
		bool AnalogLeft = false;
		bool AnalogRight = false;

		private void PspDisplayForm_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.D1: DisplayScale = 1; break;
				case Keys.D2: DisplayScale = 2; break;
				case Keys.D3: DisplayScale = 3; break;
				case Keys.D4: DisplayScale = 4; break;
				case Keys.F2: IGuiExternalInterface.ShowDebugInformation(); break;
			}

			switch (e.KeyCode)
			{
				case Keys.U: AnalogUp = true; break;
				case Keys.J: AnalogDown = true; break;
				case Keys.H: AnalogLeft = true; break;
				case Keys.K: AnalogRight = true; break;
			}

			SceCtrlData.UpdateButtons(GetButtonsFromKeys(e.KeyCode), true);
		}

		private void PspDisplayForm_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.U: AnalogUp = false; break;
				case Keys.J: AnalogDown = false; break;
				case Keys.H: AnalogLeft = false; break;
				case Keys.K: AnalogRight = false; break;
			}

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
			PauseResume(() =>
			{
				new AboutForm().ShowDialog();
			});
		}

		private void updateResumePause()
		{
			var Paused = IGuiExternalInterface.IsPaused();
			pauseToolStripMenuItem.Checked = Paused;
			resumeToolStripMenuItem.Checked = !Paused;
		}

		private void updateDebugSyscalls()
		{
			traceSyscallsToolStripMenuItem.Checked = PspConfig.DebugSyscalls;
		}

		private void updateDebugGpu()
		{
			traceUnimplementedGpuToolStripMenuItem.Checked = PspConfig.NoticeUnimplementedGpuCommands;
		}

		private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.Resume();
			updateResumePause();
		}

		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IGuiExternalInterface.Pause();
			updateResumePause();
		}

		private void indieGamesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://kawagames.com/?rf=csp");
		}

		private void blogcballesterosvelascoesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(@"http://en.blog.cballesterosvelasco.es/?rf=csp");
		}

		private void traceSyscallsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PspConfig.DebugSyscalls = !PspConfig.DebugSyscalls;
			updateDebugSyscalls();
		}

		private void traceUnimplementedGpuToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PspConfig.NoticeUnimplementedGpuCommands = !PspConfig.NoticeUnimplementedGpuCommands;
			updateDebugSyscalls();
		}
	}
}

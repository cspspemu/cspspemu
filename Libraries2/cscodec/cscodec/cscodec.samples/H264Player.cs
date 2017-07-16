using cscodec.h264.decoder;
using cscodec.h264.player;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace cscodec.h264.player
{
	public class H264Player
	{

		Form frame;
		//private PlayerFrame displayPanel;

		/**
		 * @param args
		 */
		[STAThread]
		public static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				var OpenFileDialog = new OpenFileDialog();
				OpenFileDialog.CheckFileExists = true;
				OpenFileDialog.Filter = "h264 files|*.h264;*.264|All Files|*.*";
				if (OpenFileDialog.ShowDialog() == DialogResult.OK)
				{
					args = new string[] { OpenFileDialog.FileName };
				}
				else
				{
					Console.WriteLine("Usage: H264Player <.h264 raw file>\n");
					return;
				}
			}

			// TODO Auto-generated method stub
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: H264Player <.h264 raw file>\n");
				return;
			}
			else
			{
				var H264Player = new H264Player();

				H264Player.frame = new Form()
				{
					Text = "cscodec.h264 Player",
					FormBorderStyle = FormBorderStyle.FixedDialog,
					MinimizeBox = false,
					StartPosition = FormStartPosition.CenterScreen,
				};
				//displayPanel = new PlayerFrame();

				//frame.getContentPane().add(displayPanel, BorderLayout.CENTER);

				// Finish setting up the frame, and show it.
				H264Player.frame.FormClosing += (s, e) =>
				{
					Environment.Exit(0);
				};

				H264Player.frame.HandleCreated += (s, e) =>
				{
					new Thread(() =>
					{
						H264Player.run(args[0]);
					}).Start();
				};

				Application.Run(H264Player.frame);

			} // if
		}

		public H264Player()
		{

		}

		public void run(string fileName)
		{
			Console.WriteLine("Playing " + fileName);
			playFile(fileName);
		}

		public static void CenterForm(Form theForm)
		{
			theForm.Location = new Point(
				Screen.PrimaryScreen.WorkingArea.Width / 2 - theForm.Width / 2,
				Screen.PrimaryScreen.WorkingArea.Height / 2 - theForm.Height / 2);
		}

		public bool playFile(string filename)
		{
			//using (Stream fin = File.OpenRead(filename))
			using (Stream fin = new MemoryStream(File.ReadAllBytes(filename)))
			using (FrameDecoder FrameDecoder = new FrameDecoder(fin))
			{
				try
				{
					while (true)
					{
						var picture = FrameDecoder.DecodeFrame();

						var Width = picture.imageWidthWOEdge;
						var Height = picture.imageHeightWOEdge;

						if (this.frame.ClientSize.Width < Width || this.frame.ClientSize.Height < Height)
						{
							this.frame.Invoke((Action)(() =>
							{
								this.frame.ClientSize = new Size(Width, Height);
								CenterForm(this.frame);
							}));
						}
						this.frame.CreateGraphics().DrawImage(FrameUtils.imageFromFrameWithoutEdges(picture, Width, Height), Point.Empty);
					}
				}
				catch (EndOfStreamException)
				{
				}
			}

			Console.WriteLine("Stop playing video.");

			return true;
		}
	}
}
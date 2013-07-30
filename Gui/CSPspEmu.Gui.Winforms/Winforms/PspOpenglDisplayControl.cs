using CSharpUtils;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Types;
using CSPspEmu.Core.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
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
	unsafe public sealed class PspOpenglDisplayControl : GLControl
	{
		public struct BGRA
		{
			public byte B, G, R, A;
		}


		public PspOpenglDisplayControl(GraphicsMode mode) : base(mode)
		{
			//this.CanFocus = false;
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			Context.SwapInterval = 0;
			MakeCurrent();
			GL.Enable(EnableCap.Texture2D);
		}

		private bool BindTexOpengl()
		{
			//Console.WriteLine("OpenglGpuImpl.FrameBufferTexture: {0}, {1}, {2}", OpenglGpuImpl.FrameBufferTexture, GL.IsTexture(OpenglGpuImpl.FrameBufferTexture), GL.IsTexture(2));
			var OpenglGpuImpl = (PspDisplayForm.Singleton.GpuProcessor.GpuImpl as OpenglGpuImpl);
			if (OpenglGpuImpl != null)
			{
				var DrawBuffer = OpenglGpuImpl.GetCurrentDrawBufferTexture(new OpenglGpuImpl.DrawBufferKey()
				{
					Address = PspDisplayForm.Singleton.PspDisplay.CurrentInfo.FrameAddress,
					//Width = PspDisplayForm.Singleton.PspDisplay.CurrentInfo.Width,
					//Height = PspDisplayForm.Singleton.PspDisplay.CurrentInfo.Height
				});

				var Texture = DrawBuffer.TextureColor;

				if (GL.IsTexture(Texture))
				{
					//GL.Enable(EnableCap.Texture2D);
					GL.BindTexture(TextureTarget.Texture2D, Texture);
					//Console.WriteLine(GL.GetError());
					TextureVerticalFlip = true;
					return true;
				}
				else
				{
					Console.WriteLine("Not shared contexts");
				}
			}
			return false;
		}

		int TexVram = -1;

		public Bitmap Buffer = new Bitmap(512, 272);
		public Graphics BufferGraphics;
		ulong LastHash = unchecked((ulong)-1);
		OutputPixel[] BitmapDataDecode = new OutputPixel[512 * 512];
		byte* OldFrameBuffer = (byte*)-1;
		bool TextureVerticalFlip;

		private void BindTexVram()
		{
			if (TexVram == -1)
			{
				TexVram = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, TexVram);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new uint[] { 0xFF000000 });
			}

			//Console.WriteLine(TexVram);

			GL.BindTexture(TextureTarget.Texture2D, TexVram);

			if (BufferGraphics == null)
			{
				BufferGraphics = Graphics.FromImage(Buffer);
				//BufferGraphics.Clear(Color.Red);
				BufferGraphics.Clear(Color.Black);
			}

			if (PspDisplayForm.Singleton.WindowState == FormWindowState.Minimized)
			{
				return;
			}

			if (!PspDisplayForm.Singleton.EnableRefreshing)
			{
				return;
			}

			try
			{
				int Width = 512;
				int Height = 272;
				var FrameAddress = PspDisplayForm.Singleton.PspDisplay.CurrentInfo.FrameAddress;
				byte* FrameBuffer = null;
				byte* DepthBuffer = null;
				try
				{
					FrameBuffer = (byte*)PspDisplayForm.Singleton.Memory.PspAddressToPointerSafe(
						FrameAddress,
						PixelFormatDecoder.GetPixelsSize(PspDisplayForm.Singleton.PspDisplay.CurrentInfo.PixelFormat, Width * Height)
					);
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
				}
				
				//Console.Error.WriteLine("FrameBuffer == 0x{0:X}!!", (long)FrameBuffer);
				
				if (FrameBuffer == null)
				{
					//Console.Error.WriteLine("FrameBuffer == null!!");
				}
				
				//Console.WriteLine("{0:X}", Address);
				
				var Hash = PixelFormatDecoder.Hash(
					PspDisplayForm.Singleton.PspDisplay.CurrentInfo.PixelFormat,
					(void*)FrameBuffer,
					Width, Height
				);
				
				if (Hash != LastHash)
				{
					LastHash = Hash;
					Buffer.LockBitsUnlock(System.Drawing.Imaging.PixelFormat.Format32bppArgb, (BitmapData) =>
					{
						var Count = Width * Height;
						fixed (OutputPixel* BitmapDataDecodePtr = BitmapDataDecode)
						{
							var BitmapDataPtr = (BGRA*)BitmapData.Scan0.ToPointer();
				
							//var LastRow = (FrameBuffer + 512 * 260 * 4 + 4 * 10);
							//Console.WriteLine("{0},{1},{2},{3}", LastRow[0], LastRow[1], LastRow[2], LastRow[3]);
				
							if (FrameBuffer == null)
							{
								if (OldFrameBuffer != null)
								{
									Console.Error.WriteLine("FrameBuffer == null");
								}
							}
							else if (BitmapDataPtr == null)
							{
								Console.Error.WriteLine("BitmapDataPtr == null");
							}
							else
							{
								PixelFormatDecoder.Decode(
									PspDisplayForm.Singleton.PspDisplay.CurrentInfo.PixelFormat,
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
				
							OldFrameBuffer = FrameBuffer;

							GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 512, 272, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new IntPtr(BitmapDataPtr));
							TextureVerticalFlip = false;
						}
					});
				}
				//else
				{
					//Console.WriteLine("Display not updated!");
				}
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			}
		}

		private void BindTex()
		{
			//BindTexVram(); return;

			if (PspDisplayForm.Singleton.PspDisplay.CurrentInfo.PlayingVideo)
			{
				BindTexVram();
			}
			else if (PspDisplayForm.Singleton.GpuProcessor.UsingGe)
			{
				if (!BindTexOpengl())
				{
					BindTexVram();
				}
			}
			else
			{
				BindTexVram();
			}
		}

		private void UnbindTex()
		{
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		private void BindUnbindTex(Action Callback)
		{
			BindTex();
			try
			{
				Callback();
			}
			finally
			{
				UnbindTex();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			this.Top = PspDisplayForm.Singleton.MainMenuStripHeight;
			this.Size = new System.Drawing.Size(PspDisplayForm.Singleton.ClientSize.Width, PspDisplayForm.Singleton.ClientSize.Height - this.Top);
			int RectWidth = 512;
			int RectHeight = 272;
			GL.Viewport(this.ClientRectangle);

			if (
				!PspDisplayForm.Singleton.IGuiExternalInterface.IsInitialized()
				|| (!PspDisplayForm.Singleton.PspDisplay.CurrentInfo.Enabled && !PspDisplayForm.Singleton.PspDisplay.CurrentInfo.PlayingVideo)
				//|| true
			)
			{
				GL.ClearColor(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
				GL.Clear(ClearBufferMask.ColorBufferBit);
				SwapBuffers();
				return;
			}

			GL.ClearColor(Color.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit);

			GL.MatrixMode(MatrixMode.Texture);
			GL.LoadIdentity();
			//GL.Rotate(90, new Vector3d(0, 0, 1));

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0, RectWidth , RectHeight, 0, 0, -0xFFFF);

			GL.Color3(Color.White);

			//UpdateTex();
			BindUnbindTex(() =>
			{
				float x0 = 0f, x1 = 1f * 480f / 512f;
				float y0 = 0f, y1 = 1f;

				if (TextureVerticalFlip) LanguageUtils.Swap(ref y0, ref y1);

				GL.Begin(BeginMode.Quads);
				{
					GL.TexCoord2(x0, y0); GL.Vertex2(0, 0);
					GL.TexCoord2(x1, y0); GL.Vertex2(RectWidth, 0);
					GL.TexCoord2(x1, y1); GL.Vertex2(RectWidth, RectHeight);
					GL.TexCoord2(x0, y1); GL.Vertex2(0, RectHeight);
				}
				GL.End();
				GL.Flush();
			});
			SwapBuffers();
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// PspOpenglDisplayControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "PspOpenglDisplayControl";
			this.Size = new System.Drawing.Size(480, 272);
			this.Load += new System.EventHandler(this.PspOpenglDisplayControl_Load);
			this.ResumeLayout(false);
		}

		private void PspOpenglDisplayControl_Load(object sender, EventArgs e)
		{

		}
	}
}

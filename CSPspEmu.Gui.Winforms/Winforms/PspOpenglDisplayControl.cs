using CSPspEmu.Core.Gpu.Impl.Opengl;
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
	public class PspOpenglDisplayControl : GLControl
	{
		public PspOpenglDisplayControl(GraphicsMode mode)
			: base(mode)
			{
				//this.CanFocus = false;
			}

			public override bool PreProcessMessage(ref Message msg)
			{
				//if (msg.LParam)
				//OnKeyDown
				//Keys
				var Key = (Keys)msg.WParam;

				switch (msg.Msg)
				{
					case 256: // WM_KEYDOWN
						PspDisplayForm.Singleton.DoKeyDown(Key);
						return false;
					case 257: // WM_KEYUP
						PspDisplayForm.Singleton.DoKeyUp(Key);
						return false;
				}
				//Console.WriteLine("{0} {1} {2} {3} :: {4}", msg.Msg, msg.LParam, msg.WParam, msg.Result, msg);
				return base.PreProcessMessage(ref msg);
			}

			protected override void OnCreateControl()
			{
				base.OnCreateControl();

				Context.SwapInterval = 0;
				MakeCurrent();
			}

			private void BindTex()
			{
				var OpenglGpuImpl = (PspDisplayForm.Singleton.GpuProcessor.GpuImpl as OpenglGpuImpl);
				if (OpenglGpuImpl != null)
				{
					//Console.WriteLine("OpenglGpuImpl.FrameBufferTexture: {0}, {1}, {2}", OpenglGpuImpl.FrameBufferTexture, GL.IsTexture(OpenglGpuImpl.FrameBufferTexture), GL.IsTexture(2));
					if (GL.IsTexture(OpenglGpuImpl.FrameBufferTexture))
					{
						GL.Enable(EnableCap.Texture2D);
						GL.BindTexture(TextureTarget.Texture2D, OpenglGpuImpl.FrameBufferTexture);
					}
				}
				
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				this.Top = PspDisplayForm.Singleton.MainMenuStripHeight - 1;
				this.Size = new System.Drawing.Size(PspDisplayForm.Singleton.ClientSize.Width, PspDisplayForm.Singleton.ClientSize.Height - this.Top);
				int RectWidth = 512;
				int RectHeight = 272;

				//OpenglGpuImpl.RenderGraphicsContext.Update(

				//OpenglGpuImpl.RenderGraphicsContext.MakeCurrent(this.WindowInfo);

				GL.Viewport(this.ClientRectangle);
				GL.ClearColor(Color.White);
				GL.Clear(ClearBufferMask.ColorBufferBit);

				GL.MatrixMode(MatrixMode.Texture);
				GL.LoadIdentity();
				//GL.Rotate(90, new Vector3d(0, 0, 1));

				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadIdentity();
				GL.Ortho(0, RectWidth, RectHeight, 0, 0, -0xFFFF);

				GL.Color3(Color.White);
				
				//UpdateTex();
				BindTex();

				GL.Begin(BeginMode.Quads);
				{
					GL.TexCoord2(0f, 1f); GL.Vertex2(0, 0);
					GL.TexCoord2(1f * 480f / 512f, 1f); GL.Vertex2(RectWidth, 0);
					GL.TexCoord2(1f * 480f / 512f, 0f); GL.Vertex2(RectWidth, RectHeight);
					GL.TexCoord2(0f, 0f); GL.Vertex2(0, RectHeight); 
				}
				GL.End();
				SwapBuffers();
			}
	}
}

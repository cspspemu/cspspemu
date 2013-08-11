using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
using CSPspEmu.Core.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Gui
{
	public class GuiRectangle
	{
		public int X, Y, Width, Height;

		public GuiRectangle(int x, int y, int width, int height)
		{
			this.X = x;
			this.Y = y;
			this.Width = width;
			this.Height = height;
		}
	}

	public interface IGuiWindowInfo
	{
		bool EnableRefreshing { get; }
		void SwapBuffers();
		GuiRectangle ClientRectangle { get; }
	}

	public struct BGRA
	{
		public byte B, G, R, A;
	}

	unsafe public class CommonGuiDisplayOpengl
	{
		IGuiExternalInterface IGuiExternalInterface;
		IGuiWindowInfo IGuiWindowInfo;

		internal InjectContext InjectContext { get { return IGuiExternalInterface.InjectContext; } }

		internal GpuProcessor GpuProcessor { get { return InjectContext.GetInstance<GpuProcessor>(); } }
		internal PspDisplay PspDisplay { get { return InjectContext.GetInstance<PspDisplay>(); } }
		internal PspMemory Memory { get { return InjectContext.GetInstance<PspMemory>(); } }

		GLTexture TexVram;
		public Bitmap Buffer = new Bitmap(512, 272);
		public Graphics BufferGraphics;
		ulong LastHash = unchecked((ulong)-1);
		OutputPixel[] BitmapDataDecode = new OutputPixel[512 * 512];
		byte* OldFrameBuffer = (byte*)-1;
		bool TextureVerticalFlip;

		public CommonGuiDisplayOpengl(IGuiExternalInterface IGuiExternalInterface, IGuiWindowInfo IGuiWindowInfo)
		{
			this.IGuiExternalInterface = IGuiExternalInterface;
			this.IGuiWindowInfo = IGuiWindowInfo;
		}
		
		private bool BindTexOpengl()
		{
			//Console.WriteLine("OpenglGpuImpl.FrameBufferTexture: {0}, {1}, {2}", OpenglGpuImpl.FrameBufferTexture, GL.IsTexture(OpenglGpuImpl.FrameBufferTexture), GL.IsTexture(2));
			var OpenglGpuImpl = (GpuProcessor.GpuImpl as OpenglGpuImpl);
			if (OpenglGpuImpl != null)
			{
				var DrawBuffer = OpenglGpuImpl.GetCurrentDrawBufferTexture(new OpenglGpuImpl.DrawBufferKey()
				{
					Address = PspDisplay.CurrentInfo.FrameAddress,
					//Width = PspDisplayForm.Singleton.PspDisplay.CurrentInfo.Width,
					//Height = PspDisplayForm.Singleton.PspDisplay.CurrentInfo.Height
				});

				//DrawBuffer.WaitUnbinded();
				var Texture = (uint)DrawBuffer.TextureColor;

				if (GL.glIsTexture(Texture))
				{
					//GL.Enable(EnableCap.Texture2D);
					GL.glBindTexture(GL.GL_TEXTURE_2D, Texture);
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

		private void BindTexVram()
		{
			if (TexVram == null)
			{
				TexVram = GLTexture.Create().SetFormat(TextureFormat.RGBA).SetSize(1, 1);
			}

			//Console.WriteLine(TexVram);

			TexVram.Bind();

			if (BufferGraphics == null)
			{
				BufferGraphics = Graphics.FromImage(Buffer);
				//BufferGraphics.Clear(Color.Red);
				BufferGraphics.Clear(Color.Black);
			}

			//if (PspDisplayForm.Singleton.WindowState == FormWindowState.Minimized)
			//{
			//	return;
			//}
			//
			//if (!PspDisplayForm.Singleton.EnableRefreshing)
			//{
			//	return;
			//}

			if (!IGuiWindowInfo.EnableRefreshing)
			{
				return;
			}

			try
			{
				int Width = 512;
				int Height = 272;
				var FrameAddress = PspDisplay.CurrentInfo.FrameAddress;
				byte* FrameBuffer = null;
				byte* DepthBuffer = null;
				try
				{
					FrameBuffer = (byte*)Memory.PspAddressToPointerSafe(
						FrameAddress,
						PixelFormatDecoder.GetPixelsSize(PspDisplay.CurrentInfo.PixelFormat, Width * Height)
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
					PspDisplay.CurrentInfo.PixelFormat,
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

							OldFrameBuffer = FrameBuffer;

							GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, 512, 272, 0, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, BitmapDataPtr);
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

			if (PspDisplay.CurrentInfo.PlayingVideo)
			{
				BindTexVram();
			}
			else if (GpuProcessor.UsingGe)
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
			GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
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

		GLShader Shader;
		GLAttribute AttributeVertexPosition;
		GLUniform UniformTexture;
		GLBuffer VertexBuffer;
		GLTexture TestTexture;

		private void Initialize()
		{
			Shader = new GLShader(
				"attribute vec4 vertexPosition; varying vec2 texCoord; void main() { gl_Position = vertexPosition; texCoord = vertexPosition.xy * 0.5 + 1.0; }",
				"uniform sampler2D texture; varying vec2 texCoord; void main() { gl_FragColor = texture2D(texture, texCoord); }"
			);

			var Vertices = new float[] {
				-1, -1, 0, 0,
				+1, -1, 0, 0,
				-1, +1, 0, 0,
				+1, +1, 0, 0,
			};

			VertexBuffer = GLBuffer.Create().SetData(Vertices);

			AttributeVertexPosition = Shader.GetAttribute("vertexPosition");
			UniformTexture = Shader.GetUniform("texture");
		}

		public void DrawVram()
		{
			if (Shader == null) Initialize();

			int RectWidth = 512;
			int RectHeight = 272;
			var Rectangle = IGuiWindowInfo.ClientRectangle;
			GL.glViewport(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);

			GL.glClearColor(0, 0, 0, 1);
			GL.glClear(GL.GL_COLOR_BUFFER_BIT);

			if (
				IGuiExternalInterface.IsInitialized()
				&& (PspDisplay.CurrentInfo.Enabled || PspDisplay.CurrentInfo.PlayingVideo)
			)
			{
				Shader.Use();

				if (TestTexture == null)
				{
					TestTexture = GLTexture.Create().SetFormat(TextureFormat.RGBA).SetSize(2, 2).SetData(new uint[] { 0xFF0000FF, 0xFF00FFFF, 0xFFFF00FF, 0xFFFFFFFF });
				}
				var RenderTarget = GLRenderTarget.Create(128, 128);
				RenderTarget.Bind();

				var TextureUnit = GLTextureUnit.CreateAtIndex(0);
				TextureUnit.MakeCurrent();
				GL.glEnable(GL.GL_TEXTURE_2D);
				UniformTexture.Set(TextureUnit);

				TestTexture.Bind();

				AttributeVertexPosition.SetData<float>(VertexBuffer, 4, 0);
				GL.glDrawArrays(GL.GL_TRIANGLE_STRIP, 0, 4);

				File.WriteAllBytes(@"c:\temp\out777.bin", RenderTarget.ReadPixels());

				//BindUnbindTex(() =>
				//{
				//	AttributeVertexPosition.SetData<float>(VertexBuffer, 4, 0);
				//	GL.glDrawArrays(GL.GL_TRIANGLE_STRIP, 0, 4);
				//});
			}

			IGuiWindowInfo.SwapBuffers();
		}
	}
}

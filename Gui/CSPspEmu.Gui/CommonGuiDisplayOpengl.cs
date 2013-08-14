using CSharpPlatform;
using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Gpu.Impl.Opengl.Modules;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
using CSPspEmu.Core.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
//using System.Drawing;
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

		private GLTexture GetTexOpengl()
		{
			//Console.WriteLine("OpenglGpuImpl.FrameBufferTexture: {0}, {1}, {2}", OpenglGpuImpl.FrameBufferTexture, GL.IsTexture(OpenglGpuImpl.FrameBufferTexture), GL.IsTexture(2));
			var OpenglGpuImpl = (GpuProcessor.GpuImpl as OpenglGpuImpl);
			if (OpenglGpuImpl != null)
			{
				var DrawBuffer = OpenglGpuImpl.RenderbufferManager.GetDrawBufferTexture(new DrawBufferKey()
				{
					Address = PspDisplay.CurrentInfo.FrameAddress,
					//Width = PspDisplayForm.Singleton.PspDisplay.CurrentInfo.Width,
					//Height = PspDisplayForm.Singleton.PspDisplay.CurrentInfo.Height
				});

				if (DrawBuffer == null)
				{
					return null;
				}
				else
				{
					//DrawBuffer.WaitUnbinded();
					//var Texture = (uint)DrawBuffer.RenderTarget.TextureColor.Texture;
					var Texture = (uint)DrawBuffer.RenderTarget.TextureColorBuffered.Texture;

					//return GLTexture.Wrap(Texture);

					if (GL.glIsTexture(Texture))
					{
						TextureVerticalFlip = false;
						//Console.Out.WriteLineColored(ConsoleColor.Red, "Texture: {0}", Texture);

						//var Data = new byte[512 * 2 * 272 * 2 * 4];
						//fixed (byte* DataPtr = Data)
						//{
						//	GL.glBindTexture(GL.GL_TEXTURE_2D, Texture);
						//	GL.glGetTexImage(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, GL.GL_RGBA, DataPtr);
						//}
						//File.WriteAllBytes(@"c:\temp\lol2.bin", Data);
						return GLTexture.Wrap(Texture);
					}
					else
					{
						Console.WriteLine("Not shared contexts");
					}
				}
			}

			return GLTexture.Wrap(0);
		}

		private GLTexture GetTexVram()
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

			if (IGuiWindowInfo.EnableRefreshing)
			{
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
									BitmapDataPtr[n].R = BitmapDataDecodePtr[n].B;
									BitmapDataPtr[n].G = BitmapDataDecodePtr[n].G;
									BitmapDataPtr[n].B = BitmapDataDecodePtr[n].R;
									BitmapDataPtr[n].A = 0xFF;
								}

								OldFrameBuffer = FrameBuffer;

								GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, 512, 272, 0, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, BitmapDataPtr);
								TextureVerticalFlip = true;
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
			return TexVram;
		}

		private GLTexture GetTex()
		{
			//BindTexVram(); return;

			if (!PspDisplay.CurrentInfo.PlayingVideo && GpuProcessor.UsingGe)
			{
				return GetTexOpengl();
			}
			
			return GetTexVram();
		}

		GLShader Shader;
		GLBuffer VertexBuffer;
		GLBuffer TexCoordsBuffer;
		//GLTexture TestTexture;
		public class ShaderInfoClass
		{
			public GLAttribute position;
			public GLAttribute texCoords;
			public GLUniform texture;
		}
		ShaderInfoClass ShaderInfo = new ShaderInfoClass();

		private void Initialize()
		{
			Shader = new GLShader(
				"attribute vec4 position; attribute vec4 texCoords; varying vec2 v_texCoord; void main() { gl_Position = position; v_texCoord = texCoords.xy; }",
				"uniform sampler2D texture; varying vec2 v_texCoord; void main() { gl_FragColor = texture2D(texture, v_texCoord); }"
			);

			VertexBuffer = GLBuffer.Create().SetData(CSharpPlatform.RectangleF.FromCoords(-1, -1, +1, +1).GetFloat2TriangleStripCoords());
			
			Shader.BindUniformsAndAttributes(ShaderInfo);

			// TestTexture = GLTexture.Create().SetFormat(TextureFormat.RGBA).SetSize(2, 2).SetData(new uint[] { 0xFF0000FF, 0xFF00FFFF, 0xFFFF00FF, 0xFFFFFFFF });
		}

		//GLTexture TestTexture;

		public void DrawVram()
		{
			if (Shader == null) Initialize();

			//int RectWidth = 512;
			//int RectHeight = 272;
			var Rectangle = IGuiWindowInfo.ClientRectangle;

			GL.glViewport(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
			GL.glClearColor(0, 0, 0, 1);
			GL.glClear(GL.GL_COLOR_BUFFER_BIT);
			//IGuiWindowInfo.SwapBuffers();
			//return;

			if (
				(PspDisplay.CurrentInfo.Enabled || PspDisplay.CurrentInfo.PlayingVideo)
			)
			{
				var Tex = GetTex();
				//Console.Out.WriteLineColored(ConsoleColor.Red, "{0}", Tex.Texture);
				if (Tex != null && Tex.Texture != 0)
				{
					Shader.Draw(GLGeometry.GL_TRIANGLE_STRIP, 4, () =>
					{
						var TextureRect = CSharpPlatform.RectangleF.FromCoords(0, 0, (float)PspDisplay.CurrentInfo.Width / 512f, (float)PspDisplay.CurrentInfo.Height / 272f);
						if (TextureVerticalFlip) TextureRect = TextureRect.VFlip();
						TexCoordsBuffer = GLBuffer.Create().SetData(TextureRect.GetFloat2TriangleStripCoords());

						ShaderInfo.texture.Set(GLTextureUnit.CreateAtIndex(0).SetFiltering(GLScaleFilter.Nearest).SetWrap(GLWrap.ClampToEdge).SetTexture(Tex));
						ShaderInfo.position.SetData<float>(VertexBuffer, 2);
						ShaderInfo.texCoords.SetData<float>(TexCoordsBuffer, 2);
					});
				}
			}

			IGuiWindowInfo.SwapBuffers();
		}
	}
}

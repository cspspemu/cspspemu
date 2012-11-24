using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLES;
using CSharpUtils;
using CSPspEmu.Core.Utils;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Gpu.Impl.OpenglEs
{
	public unsafe class TextureOpengles : Texture<GpuImplOpenglEs>
	{
		public uint TextureId { get; private set; }

		protected override void Init()
		{
			//this.GpuImpl.GraphicsContext.MakeCurrent(GpuImpl.WindowInfo);

			//lock (OpenglGpuImpl.GpuLock)
			{
				TextureId = GL.glGenTexture();
				var GlError = GL.glGetError();
				//Console.Error.WriteLine("GenTexture: {0} : Thread : {1} <- {2}", GlError, Thread.CurrentThread.ManagedThreadId, TextureId);
				if (GlError != GL.GL_NO_ERROR)
				{
					//TextureId = 0;
				}
			}
		}

		public override bool SetData(OutputPixel* Pixels, int TextureWidth, int TextureHeight)
		{
			//lock (OpenglGpuImpl.GpuLock)
			{
				//if (TextureId != 0)
				{
					this.Width = TextureWidth;
					this.Height = TextureHeight;

					Data = new OutputPixel[TextureWidth * TextureHeight];
					fixed (OutputPixel* DataPtr = Data)
					{
						PointerUtils.Memcpy((byte*)DataPtr, (byte*)Pixels, TextureWidth * TextureHeight * sizeof(OutputPixel));
					}

					Bind();
					GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, TextureWidth, TextureHeight, 0, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, Pixels);
					var GlError = GL.glGetError();
					GL.glFlush();

					if (GlError != GL.GL_NO_ERROR)
					{
						Console.Error.WriteLine("########## ERROR: TexImage2D: {0} : TexId:{1} : {2} : {3}x{4}", GlError, TextureId, new IntPtr(Pixels), TextureWidth, TextureHeight);
						TextureId = 0;
						Bind();
						return false;
					}
				}
			}
			return true;

			//glTexEnvf(GL_TEXTURE_ENV, GL_RGB_SCALE, 1.0); // 2.0 in scale_2x
			//GL.TexEnv(TextureEnvTarget.TextureEnv, GL_TEXTURE_ENV_MODE, TextureEnvModeTranslate[state.texture.effect]);
		}

		public override void Bind()
		{
			//lock (OpenglGpuImpl.GpuLock)
			{
				if (TextureId != 0)
				{
					//GL.Enable(EnableCap.Texture2D);
					GL.glBindTexture(GL.GL_TEXTURE_2D, TextureId);

					var GlError = GL.eglGetError();
					if (GlError != GL.GL_NO_ERROR)
					{
						//Console.Error.WriteLine("Bind: {0} : {1}", GlError, TextureId);
					}
				}
				else
				{
					//GL.Disable(EnableCap.Texture2D);
				}
			}
		}

		public override void Dispose()
		{
			if (TextureId != 0)
			{
				GL.glDeleteTexture(TextureId);
				TextureId = 0;
			}
		}
	}

	public class TextureCacheOpengles : TextureCache<GpuImplOpenglEs, TextureOpengles>
	{
		public TextureCacheOpengles(PspMemory PspMemory, GpuImplOpenglEs GpuImpl)
			: base (PspMemory, GpuImpl)
		{
		}
	}
}

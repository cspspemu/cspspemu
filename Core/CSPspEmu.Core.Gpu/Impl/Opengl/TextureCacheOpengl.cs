using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if OPENTK
using OpenTK.Graphics.OpenGL;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
#else
using MiniGL;
#endif

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public unsafe class TextureOpengl : Texture<OpenglGpuImpl>
	{
		public int TextureId { get; private set; }

		protected override void Init()
		{
			//this.GpuImpl.GraphicsContext.MakeCurrent(GpuImpl.WindowInfo);

			//lock (OpenglGpuImpl.GpuLock)
			{
				TextureId = GL.GenTexture();
				var GlError = GL.GetError();
				//Console.Error.WriteLine("GenTexture: {0} : Thread : {1} <- {2}", GlError, Thread.CurrentThread.ManagedThreadId, TextureId);
				if (GlError != ErrorCode.NoError)
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
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureWidth, TextureHeight, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888Reversed, new IntPtr(Pixels));
					var GlError = GL.GetError();
					//GL.Flush();

					if (GlError != ErrorCode.NoError)
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
					GL.BindTexture(TextureTarget.Texture2D, TextureId);

					var GlError = GL.GetError();
					if (GlError != ErrorCode.NoError)
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
				GL.DeleteTexture(TextureId);
				TextureId = 0;
			}
		}
	}

	public class TextureCacheOpengl : TextureCache<OpenglGpuImpl, TextureOpengl>
	{
		public TextureCacheOpengl(PspMemory PspMemory, OpenglGpuImpl GpuImpl)
			: base (PspMemory, GpuImpl)
		{
		}
	}
}

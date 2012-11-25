using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLES;
using CSharpUtils;
using CSPspEmu.Core.Utils;
using CSPspEmu.Core.Memory;
using System.Threading;

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
				GL.glGetError();
				TextureId = GL.glGenTexture();
				var GlError = GL.glGetError();
				//Console.Error.WriteLine("GenTexture: {0} : Thread : {1} <- {2}", GlError, Thread.CurrentThread.ManagedThreadId, TextureId);
				if (GlError != GL.GL_NO_ERROR)
				{
					//TextureId = 0;
					Console.Error.WriteLine("########## ERROR: glGenTexture: {0}('{1}') {2}", GlError, GL.glGetErrorString(GlError), TextureId);
				}
			}
		}

		public override bool SetData(OutputPixel* Pixels, int TextureWidth, int TextureHeight)
		{
			int Side = Math.Max(TextureWidth, TextureHeight);
			{
				//if (TextureId != 0)
				{
					this.Width = TextureWidth;
					this.Height = TextureHeight;

#if ALLOW_RECTANGULAR_TEXTURES
					int DataWidth = TextureWidth;
					int DataHeight = TextureHeight;
#else
					int DataWidth = Side;
					int DataHeight = Side;
#endif

					Data = new OutputPixel[DataWidth * DataHeight];

					fixed (OutputPixel* DataPtr = Data)
					{
						for (int Row = 0; Row < TextureHeight; Row++)
						{
							PointerUtils.Memcpy(
								(byte *)(DataPtr + Row * DataWidth),
								(byte *)(Pixels + Row * TextureWidth),
								TextureWidth * sizeof(OutputPixel)
							);
							PointerUtils.Memset(
								(byte*)(DataPtr + Row * DataWidth + TextureWidth),
								0,
								DataWidth - TextureWidth
							);

							for (int Column = 0; Column < DataWidth; Column++)
							{
								DataPtr[Column] = new OutputPixel()
								{
									R = DataPtr[Column].A,
									G = DataPtr[Column].G,
									B = DataPtr[Column].R,
									A = DataPtr[Column].R,
								};
							}

						}

						ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
						{
							Console.WriteLine("Trying to create texture: {0} ({1}x{2})", TextureId, DataWidth, DataHeight);
						});

						Bind();

						GL.glGetError();
						GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, DataWidth, DataHeight, 0, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, DataPtr);
						var GlError = GL.glGetError();

						if (GlError != GL.GL_NO_ERROR)
						{
							Console.Error.WriteLine("########## ERROR: glTexImage2D: {0}('{1}') : TexId:{2} : {3} : {4}x{5} {6}x{7}", GlError, GL.glGetErrorString(GlError), TextureId, new IntPtr(Pixels), TextureWidth, TextureHeight, DataWidth, DataHeight);
							TextureId = 0;
							Bind();
							return false;
						}

						//GL.glFlush();
					}
				}
			}
			return true;
		}

		public override void Bind()
		{
			//lock (OpenglGpuImpl.GpuLock)
			{
				if (TextureId != 0)
				{
					//GL.Enable(EnableCap.Texture2D);
					
					GL.glGetError();
					GL.glBindTexture(GL.GL_TEXTURE_2D, TextureId);
					var GlError = GL.glGetError();
					if (GlError != GL.GL_NO_ERROR)
					{
						Console.Error.WriteLine("########## ERROR: glBindTexture: {0}('{1}') : {2}", GlError, GL.glGetErrorString(GlError), TextureId);
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
				GL.glGetError();
				GL.glDeleteTexture(TextureId);
				var GlError = GL.glGetError();
				if (GlError != GL.GL_NO_ERROR)
				{
					Console.Error.WriteLine("########## ERROR: glDeleteTexture: {0}('{1}') : {2}", GlError, GL.glGetErrorString(GlError), TextureId);
				}

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

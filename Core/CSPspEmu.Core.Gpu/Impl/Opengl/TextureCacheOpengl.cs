using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public unsafe class TextureOpengl : Texture<OpenglGpuImpl>
	{
		public GLTexture Texture;
		public int TextureId { get { return (int)Texture.Texture; } }

		protected override void Init()
		{
			Texture = GLTexture.Create();
		}

		public override bool SetData(OutputPixel* Pixels, int TextureWidth, int TextureHeight)
		{
			this.Width = TextureWidth;
			this.Height = TextureHeight;

			Data = new OutputPixel[TextureWidth * TextureHeight];
			fixed (OutputPixel* DataPtr = Data)
			{
				PointerUtils.Memcpy((byte*)DataPtr, (byte*)Pixels, TextureWidth * TextureHeight * sizeof(OutputPixel));
			}

			Bind();
			Texture.SetFormat(TextureFormat.RGBA).SetSize(TextureWidth, TextureHeight).SetData(Pixels);

			return true;
		}

		public void SetData(byte[] Data, int Width, int Height)
		{
			fixed (byte* DataPtr = Data)
			{
				SetData((OutputPixel*)DataPtr, Width, Height);
			}
		}

		public override void Bind()
		{
			Texture.Bind();
		}

		public override void Dispose()
		{
			if (TextureId != 0)
			{
				Texture.Dispose();
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

using CSharpPlatform.GL.Utils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
    public class TextureHookInfo
    {
        public int Width;
        public int Height;
        public OutputPixel[] Data;
        public TextureCacheKey TextureCacheKey;
    }

    public class TextureOpengl : Texture<OpenglGpuImpl>
    {
        public GLTexture Texture;

        public int TextureId => (int) Texture.Texture;

        protected override void Init()
        {
            Texture = GLTexture.Create();
        }

        public override bool SetData(OutputPixel[] pixels, int textureWidth, int textureHeight)
        {
            Width = textureWidth;
            Height = textureHeight;

            Bind();
            Texture.SetFormat(TextureFormat.RGBA).SetSize(textureWidth, textureHeight).SetData(pixels);

            return true;
        }

        //public void SetData(byte[] Data, int Width, int Height)
        //{
        //	fixed (byte* DataPtr = Data)
        //	{
        //		SetData((OutputPixel*)DataPtr, Width, Height);
        //	}
        //}

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
        public TextureCacheOpengl(PspMemory pspMemory, OpenglGpuImpl gpuImpl, InjectContext injectContext)
            : base(pspMemory, gpuImpl, injectContext)
        {
        }
    }
}
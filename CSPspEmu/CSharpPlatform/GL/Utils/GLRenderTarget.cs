using System;
using System.Diagnostics.Contracts;

namespace CSharpPlatform.GL.Utils
{
    [Flags]
    public enum RenderTargetLayers
    {
        Color = (1 << 0),
        Depth = (1 << 1),
        Stencil = (1 << 2),
        All = Color | Depth | Stencil
    }

    public unsafe class GLRenderTarget : IDisposable
    {
        [ThreadStatic] public static GLRenderTarget Current = GLRenderTargetScreen.Default;

        protected uint FrameBufferId;
        public GLTexture TextureColor { get; private set; }
        public GLTexture TextureDepth { get; private set; }
        public GLRenderBuffer RenderBufferStencil { get; private set; }
        private int _Width;
        private int _Height;
        public RenderTargetLayers RenderTargetLayers { get; private set; }

        public virtual int Width => _Width;

        public virtual int Height => _Height;

        protected GLRenderTarget()
        {
        }

        /// <summary>
        /// Copies a RenderTarget from one to another
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        public static void CopyFromTo(GLRenderTarget From, GLRenderTarget To)
        {
            Contract.Assert(From != null);
            Contract.Assert(To != null);

            From.BindUnbind(() =>
            {
                To.TextureColor?.BindUnbind(() =>
                {
                    GL.glCopyTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA4, 0, 0, From.Width, From.Height, 0);
                });

                To.TextureDepth?.BindUnbind(() =>
                {
                    GL.glCopyTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_DEPTH_COMPONENT, 0, 0, From.Width,
                        From.Height, 0);
                });
            });
        }

        protected GLRenderTarget(int Width, int Height, RenderTargetLayers RenderTargetLayers)
        {
            if (Width == 0 || Height == 0)
                throw(new Exception($"Invalid GLRenderTarget size: {Width}x{Height}"));
            _Width = Width;
            _Height = Height;
            this.RenderTargetLayers = RenderTargetLayers;
            Initialize();
        }

        public static GLRenderTarget Create(int Width, int Height,
            RenderTargetLayers RenderTargetLayers = RenderTargetLayers.All)
        {
            return new GLRenderTarget(Width, Height, RenderTargetLayers);
        }

        private void Initialize()
        {
            fixed (uint* FrameBufferPtr = &FrameBufferId)
            {
                GL.glGenFramebuffers(1, FrameBufferPtr);
                if ((RenderTargetLayers & RenderTargetLayers.Color) != 0)
                    TextureColor = GLTexture.Create().SetFormat(TextureFormat.RGBA).SetSize(_Width, _Height);
                if ((RenderTargetLayers & RenderTargetLayers.Depth) != 0)
                    TextureDepth = GLTexture.Create().SetFormat(TextureFormat.DEPTH).SetSize(_Width, _Height);
                if ((RenderTargetLayers & RenderTargetLayers.Stencil) != 0)
                    RenderBufferStencil = new GLRenderBuffer(_Width, _Height, GL.GL_STENCIL_INDEX8);
            }
        }

        public void Dispose()
        {
            fixed (uint* FrameBufferPtr = &FrameBufferId)
            {
                GL.glDeleteFramebuffers(1, FrameBufferPtr);
                if ((RenderTargetLayers & RenderTargetLayers.Color) != 0)
                {
                    TextureColor.Dispose();
                }
                if ((RenderTargetLayers & RenderTargetLayers.Depth) != 0)
                {
                    TextureDepth.Dispose();
                }
                if ((RenderTargetLayers & RenderTargetLayers.Stencil) != 0)
                {
                    RenderBufferStencil.Dispose();
                }
            }
        }

        private void Unbind()
        {
        }

        public void BindUnbind(Action Action)
        {
            var OldFrameBuffer = GL.glGetInteger(GL.GL_FRAMEBUFFER_BINDING);
            Bind();
            try
            {
                Action();
            }
            finally
            {
                GL.glBindFramebuffer(GL.GL_FRAMEBUFFER, (uint) OldFrameBuffer);
            }
        }

        protected virtual void BindBuffers()
        {
            if ((RenderTargetLayers & RenderTargetLayers.Color) != 0)
            {
                GL.glFramebufferTexture2D(GL.GL_FRAMEBUFFER, GL.GL_COLOR_ATTACHMENT0, GL.GL_TEXTURE_2D,
                    TextureColor.Texture, 0);
            }
            if ((RenderTargetLayers & RenderTargetLayers.Depth) != 0)
            {
                GL.glFramebufferTexture2D(GL.GL_FRAMEBUFFER, GL.GL_DEPTH_ATTACHMENT, GL.GL_TEXTURE_2D,
                    TextureDepth.Texture, 0);
            }
            if ((RenderTargetLayers & RenderTargetLayers.Stencil) != 0)
            {
                //GL.glFramebufferRenderbuffer(GL.GL_FRAMEBUFFER, GL.GL_STENCIL_ATTACHMENT, GL.GL_RENDERBUFFER, RenderBufferStencil.Index);
            }

            int Status = GL.glCheckFramebufferStatus(GL.GL_FRAMEBUFFER);
            if (Status != GL.GL_FRAMEBUFFER_COMPLETE)
            {
                throw (new Exception(
                    $"Failed to bind FrameBuffer 0x{Status:X4} : {GL.GetConstantString(Status)}, {RenderTargetLayers}, {Width}x{Height}"));
            }
            GL.glViewport(0, 0, Width, Height);
            GL.glClearColor(0, 0, 0, 0);
            GL.glClear(GL.GL_COLOR_CLEAR_VALUE | GL.GL_DEPTH_CLEAR_VALUE | GL.GL_STENCIL_CLEAR_VALUE);
            GL.glFlush();
        }

        public GLRenderTarget Bind()
        {
            if (Current != this)
            {
                Current?.Unbind();
            }
            Current = this;
            GL.glBindFramebuffer(GL.GL_FRAMEBUFFER, FrameBufferId);
            {
                BindBuffers();
            }
            return this;
        }

        public byte[] ReadPixels()
        {
            var Data = new byte[Width * Height * 4];
            fixed (byte* DataPtr = Data)
            {
                GL.glReadPixels(0, 0, Width, Height, GL.GL_RGBA, GL.GL_UNSIGNED_BYTE, DataPtr);
            }
            return Data;
        }

        public override string ToString()
        {
            return $"GLRenderTarget({FrameBufferId}, Size({Width}x{Height}))";
        }

        public class GLRenderBuffer : IDisposable
        {
            public readonly int Width, Height;

            public uint Index => _Index;

            private uint _Index;

            public GLRenderBuffer(int Width, int Height, int Format)
            {
                this.Width = Width;
                this.Height = Height;
                fixed (uint* IndexPtr = &_Index)
                {
                    GL.glGenRenderbuffers(1, IndexPtr);
                    GL.glBindRenderbuffer(GL.GL_RENDERBUFFER, _Index);
                    GL.glRenderbufferStorage(GL.GL_RENDERBUFFER, Format, Width, Height);
                }
            }

            public void Dispose()
            {
                fixed (uint* IndexPtr = &_Index)
                {
                    GL.glDeleteRenderbuffers(1, IndexPtr);
                }
            }
        }
    }

    public class GLRenderTargetScreen : GLRenderTarget
    {
        public static GLRenderTargetScreen Default => new GLRenderTargetScreen();

        public override int Width => 64;

        public override int Height => 64;

        protected GLRenderTargetScreen()
        {
            FrameBufferId = 0;
        }

        protected override void BindBuffers()
        {
        }
    }
}
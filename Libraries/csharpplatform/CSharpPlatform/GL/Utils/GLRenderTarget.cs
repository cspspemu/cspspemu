using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.GL.Utils
{
    [Flags]
    public enum RenderTargetLayers
    {
        Color = (1 << 0),
        Depth = (1 << 1),
        Stencil = (1 << 2),
        All = Color | Depth | Stencil,
    }

    unsafe public class GLRenderTarget : IDisposable
    {
        [ThreadStatic] public static GLRenderTarget Current = GLRenderTargetScreen.Default;

        protected uint FrameBufferId;
        public GLTexture TextureColor { get; private set; }
        public GLTexture TextureDepth { get; private set; }
        public GLRenderBuffer RenderBufferStencil { get; private set; }
        private int _Width;
        private int _Height;
        public RenderTargetLayers RenderTargetLayers { get; private set; }

        virtual public int Width
        {
            get { return _Width; }
        }

        virtual public int Height
        {
            get { return _Height; }
        }

        protected GLRenderTarget()
        {
        }

        /// <summary>
        /// Copies a RenderTarget from one to another
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        static public void CopyFromTo(GLRenderTarget From, GLRenderTarget To)
        {
            Contract.Assert(From != null);
            Contract.Assert(To != null);

            From.BindUnbind(() =>
            {
                if (To.TextureColor != null)
                {
                    To.TextureColor.BindUnbind(() =>
                    {
                        GL.glCopyTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA4, 0, 0, From.Width, From.Height, 0);
                    });
                }

                if (To.TextureDepth != null)
                {
                    To.TextureDepth.BindUnbind(() =>
                    {
                        GL.glCopyTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_DEPTH_COMPONENT, 0, 0, From.Width,
                            From.Height, 0);
                    });
                }
            });
        }

        protected GLRenderTarget(int Width, int Height, RenderTargetLayers RenderTargetLayers)
        {
            if (Width == 0 || Height == 0)
                throw(new Exception(String.Format("Invalid GLRenderTarget size: {0}x{1}", Width, Height)));
            this._Width = Width;
            this._Height = Height;
            this.RenderTargetLayers = RenderTargetLayers;
            Initialize();
        }

        static public GLRenderTarget Create(int Width, int Height,
            RenderTargetLayers RenderTargetLayers = RenderTargetLayers.All)
        {
            return new GLRenderTarget(Width, Height, RenderTargetLayers);
        }

        private void Initialize()
        {
            fixed (uint* FrameBufferPtr = &FrameBufferId)
            {
                GL.glGenFramebuffers(1, FrameBufferPtr);
                if ((RenderTargetLayers & Utils.RenderTargetLayers.Color) != 0)
                    TextureColor = GLTexture.Create().SetFormat(TextureFormat.RGBA).SetSize(_Width, _Height);
                if ((RenderTargetLayers & Utils.RenderTargetLayers.Depth) != 0)
                    TextureDepth = GLTexture.Create().SetFormat(TextureFormat.DEPTH).SetSize(_Width, _Height);
                if ((RenderTargetLayers & Utils.RenderTargetLayers.Stencil) != 0)
                    RenderBufferStencil = new GLRenderBuffer(_Width, _Height, GL.GL_STENCIL_INDEX8);
            }
        }

        public void Dispose()
        {
            fixed (uint* FrameBufferPtr = &FrameBufferId)
            {
                GL.glDeleteFramebuffers(1, FrameBufferPtr);
                if ((RenderTargetLayers & Utils.RenderTargetLayers.Color) != 0)
                {
                    TextureColor.Dispose();
                }
                if ((RenderTargetLayers & Utils.RenderTargetLayers.Depth) != 0)
                {
                    TextureDepth.Dispose();
                }
                if ((RenderTargetLayers & Utils.RenderTargetLayers.Stencil) != 0)
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

        virtual protected void BindBuffers()
        {
            if ((RenderTargetLayers & Utils.RenderTargetLayers.Color) != 0)
            {
                GL.glFramebufferTexture2D(GL.GL_FRAMEBUFFER, GL.GL_COLOR_ATTACHMENT0, GL.GL_TEXTURE_2D,
                    TextureColor.Texture, 0);
            }
            if ((RenderTargetLayers & Utils.RenderTargetLayers.Depth) != 0)
            {
                GL.glFramebufferTexture2D(GL.GL_FRAMEBUFFER, GL.GL_DEPTH_ATTACHMENT, GL.GL_TEXTURE_2D,
                    TextureDepth.Texture, 0);
            }
            if ((RenderTargetLayers & Utils.RenderTargetLayers.Stencil) != 0)
            {
                //GL.glFramebufferRenderbuffer(GL.GL_FRAMEBUFFER, GL.GL_STENCIL_ATTACHMENT, GL.GL_RENDERBUFFER, RenderBufferStencil.Index);
            }

            int Status = GL.glCheckFramebufferStatus(GL.GL_FRAMEBUFFER);
            if (Status != GL.GL_FRAMEBUFFER_COMPLETE)
            {
                throw (new Exception(String.Format("Failed to bind FrameBuffer 0x{0:X4} : {1}, {2}, {3}x{4}", Status,
                    GL.GetConstantString(Status), RenderTargetLayers, Width, Height)));
            }
            GL.glViewport(0, 0, Width, Height);
            GL.glClearColor(0, 0, 0, 0);
            GL.glClear(GL.GL_COLOR_CLEAR_VALUE | GL.GL_DEPTH_CLEAR_VALUE | GL.GL_STENCIL_CLEAR_VALUE);
            GL.glFlush();
        }

        public GLRenderTarget Bind()
        {
            if (Current != this && Current != null)
            {
                Current.Unbind();
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
            return String.Format("GLRenderTarget({0}, Size({1}x{2}))", this.FrameBufferId, this.Width, this.Height);
        }

        unsafe public class GLRenderBuffer : IDisposable
        {
            public readonly int Width, Height;

            public uint Index
            {
                get { return _Index; }
            }

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

    unsafe public class GLRenderTargetScreen : GLRenderTarget
    {
        static public GLRenderTargetScreen Default
        {
            get { return new GLRenderTargetScreen(); }
        }

        public override int Width
        {
            get { return 64; }
        }

        public override int Height
        {
            get { return 64; }
        }

        protected GLRenderTargetScreen() : base()
        {
            this.FrameBufferId = 0;
        }

        protected override void BindBuffers()
        {
        }
    }
}
using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.Opengl.Modules
{
    public struct DrawBufferKey
    {
        public uint Address;
        //public int Width;
        //public int Height;
    }

    public unsafe class RenderbufferManager
    {
        //static int ViewportWidth = 512 * 2;
        //static int ViewportHeight = 272 * 2;
        //
        //static int ScreenWidth = 480 * 2;
        //static int ScreenHeight = 272 * 2;

        unsafe public class DrawBufferValue : IDisposable
        {
            public DrawBufferKey DrawBufferKey;
            public GLRenderTarget RenderTarget;
            public int Width, Height;
            private OpenglGpuImpl OpenglGpuImpl;
            private int CurrentScaleViewport;
            private bool MustUpdateRenderTarget = false;
            private int MustUpdateRenderTarget_ScaleViewport = 0;

            public DrawBufferValue(OpenglGpuImpl OpenglGpuImpl, DrawBufferKey DrawBufferKey)
            {
                this.OpenglGpuImpl = OpenglGpuImpl;
                this.DrawBufferKey = DrawBufferKey;

                OpenglGpuImpl.OnScaleViewport += UpdateTextures_External;
                UpdateTextures(OpenglGpuImpl.ScaleViewport);
            }

            private void UpdateTextures_External(int ScaleViewport)
            {
                MustUpdateRenderTarget = true;
                MustUpdateRenderTarget_ScaleViewport = ScaleViewport;
            }

            private void UpdateTextures(int ScaleViewport)
            {
                if (CurrentScaleViewport == ScaleViewport) return;
                CurrentScaleViewport = ScaleViewport;
                MustUpdateRenderTarget = false;

                if (RenderTarget != null) RenderTarget.Dispose();

                RenderTarget = GLRenderTarget.Create(
                    Width = 512 * ScaleViewport,
                    Height = 272 * ScaleViewport
                );
                //Console.WriteLine(OpenglContextFactory.Current);
                //Console.WriteLine(RenderTarget);
                //Console.ReadKey();
            }

            public bool Binded = false;
            public ManualResetEvent UnbindedEvent = new ManualResetEvent(false);

            public void WaitUnbinded()
            {
                UnbindedEvent.WaitOne();
            }

            public void Unbind()
            {
                if (Binded)
                {
                    //GL.Flush();
                    GLRenderTargetScreen.Default.Bind();
                    Binded = false;
                    UnbindedEvent.Set();
                }
            }

            public void Bind()
            {
                if (MustUpdateRenderTarget)
                {
                    Unbind();
                    UpdateTextures(MustUpdateRenderTarget_ScaleViewport);
                }
                if (!Binded)
                {
                    RenderTarget.Bind();
                    //GL.glViewport(0, 0, Width, Height);
                    Binded = true;
                    UnbindedEvent.Reset();
                }
            }

            public void Dispose()
            {
                Unbind();
                OpenglGpuImpl.OnScaleViewport -= UpdateTextures_External;
                RenderTarget.Dispose();
            }

            internal void WaitUnlocked()
            {
                Lock();
                Unlock();
            }

            internal void Lock()
            {
                Monitor.Enter(this);
            }

            internal void Unlock()
            {
                Monitor.Exit(this);
            }
        }

        private readonly Dictionary<DrawBufferKey, DrawBufferValue> DrawBufferTextures =
            new Dictionary<DrawBufferKey, DrawBufferValue>();

        public GLTexture TextureCacheGetAndBind(GpuStateStruct* GpuState)
        {
            if (_DynarecConfig.EnableRenderTarget)
            {
                var TextureMappingState = &GpuState->TextureMappingState;
                var ClutState = &TextureMappingState->ClutState;
                var TextureState = &TextureMappingState->TextureState;
                var Key = new DrawBufferKey()
                {
                    Address = TextureState->Mipmap0.Address,
                };

                if (DrawBufferTextures.ContainsKey(Key))
                {
                    return GetOrCreateDrawBufferTexture(Key).RenderTarget.TextureColor.Bind();
                }
            }

            var CurrentTexture = OpenglGpuImpl.TextureCache.Get(GpuState);
            CurrentTexture.Bind();
            return CurrentTexture.Texture;
        }

        //public int GetDrawTexture(DrawBufferKey Key)
        //{
        //	//Console.WriteLine("GetDrawTexture: {0}", GetCurrentDrawBufferTexture(Key).TextureColor);
        //	return GetCurrentDrawBufferTexture(Key).TextureColor;
        //}

        OpenglGpuImpl OpenglGpuImpl;

        public RenderbufferManager(OpenglGpuImpl OpenglGpuImpl)
        {
            this.OpenglGpuImpl = OpenglGpuImpl;
        }

        public DrawBufferValue GetDrawBufferTexture(DrawBufferKey Key)
        {
            var Value = DrawBufferTextures.GetOrDefault(Key, null);
            if (Value != null) Value.WaitUnlocked();
            return Value;
        }

        public void GetDrawBufferTextureAndLock(DrawBufferKey Key, Action<DrawBufferValue> Action)
        {
            var RenderBuffer = GetDrawBufferTexture(Key);
            if (RenderBuffer != null)
            {
                RenderBuffer.Lock();
                try
                {
                    Action(RenderBuffer);
                }
                finally
                {
                    RenderBuffer.Unlock();
                }
            }
            else
            {
                Action(null);
            }
        }

        public DrawBufferValue GetOrCreateDrawBufferTexture(DrawBufferKey Key)
        {
            if (!DrawBufferTextures.ContainsKey(Key)) DrawBufferTextures[Key] = new DrawBufferValue(OpenglGpuImpl, Key);
            return GetDrawBufferTexture(Key);
        }

        public void DrawVideo(uint FrameBufferAddress, OutputPixel* OutputPixel, int Width, int Height)
        {
            var DrawBuffer = GetOrCreateDrawBufferTexture(new DrawBufferKey()
            {
                Address = FrameBufferAddress,
            });
            DrawBuffer.Bind();
            //GL.DrawPixels(Width, Height, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new IntPtr(OutputPixel));
            DrawBuffer.Unbind();
            Console.WriteLine("DrawVideo: {0:X8}, {1}x{2}", FrameBufferAddress, Width, Height);
        }

        private uint CachedBindAddress;
        public DrawBufferValue CurrentDrawBuffer { get; private set; }

        public void BindCurrentDrawBufferTexture(GpuStateStruct* GpuState)
        {
            if (CachedBindAddress != GpuState->DrawBufferState.Address)
            {
                GL.glFlush();
                GL.glFinish();

                CachedBindAddress = GpuState->DrawBufferState.Address;
                var Key = new DrawBufferKey()
                {
                    Address = GpuState->DrawBufferState.Address,
                    //Width = (int)GpuState->DrawBufferState.Width,
                    //Height = (int)272,
                };
                if (CurrentDrawBuffer != null)
                {
                    CurrentDrawBuffer.Unbind();
                }
                CurrentDrawBuffer = GetOrCreateDrawBufferTexture(Key);
                CurrentDrawBuffer.Bind();
            }
        }
    }
}
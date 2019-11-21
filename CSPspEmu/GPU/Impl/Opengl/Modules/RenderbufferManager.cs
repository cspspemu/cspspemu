using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using CSharpUtils.Extensions;

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

        public class DrawBufferValue : IDisposable
        {
            public DrawBufferKey DrawBufferKey;
            public GLRenderTarget RenderTarget;
            public int Width, Height;
            private readonly OpenglGpuImpl _openglGpuImpl;
            private int _currentScaleViewport;
            private bool _mustUpdateRenderTarget;
            private int _mustUpdateRenderTargetScaleViewport;

            public DrawBufferValue(OpenglGpuImpl openglGpuImpl, DrawBufferKey drawBufferKey)
            {
                _openglGpuImpl = openglGpuImpl;
                DrawBufferKey = drawBufferKey;

                openglGpuImpl.OnScaleViewport += UpdateTextures_External;
                UpdateTextures(openglGpuImpl.ScaleViewport);
            }

            private void UpdateTextures_External(int scaleViewport)
            {
                _mustUpdateRenderTarget = true;
                _mustUpdateRenderTargetScaleViewport = scaleViewport;
            }

            private void UpdateTextures(int scaleViewport)
            {
                if (_currentScaleViewport == scaleViewport) return;
                _currentScaleViewport = scaleViewport;
                _mustUpdateRenderTarget = false;

                RenderTarget?.Dispose();

                RenderTarget = GLRenderTarget.Create(
                    Width = 512 * scaleViewport,
                    Height = 272 * scaleViewport
                );
                //Console.WriteLine(OpenglContextFactory.Current);
                //Console.WriteLine(RenderTarget);
                //Console.ReadKey();
            }

            public bool Binded;
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
                if (_mustUpdateRenderTarget)
                {
                    Unbind();
                    UpdateTextures(_mustUpdateRenderTargetScaleViewport);
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
                _openglGpuImpl.OnScaleViewport -= UpdateTextures_External;
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

        private readonly Dictionary<DrawBufferKey, DrawBufferValue> _drawBufferTextures =
            new Dictionary<DrawBufferKey, DrawBufferValue>();

        public GLTexture TextureCacheGetAndBind(GpuStateStruct gpuState)
        {
            if (DynarecConfig.EnableRenderTarget)
            {
                var textureMappingState = gpuState.TextureMappingState;
                var clutState = textureMappingState.ClutState;
                var textureState = textureMappingState.TextureState;
                var key = new DrawBufferKey()
                {
                    Address = textureState.Mipmap0.Address,
                };

                if (_drawBufferTextures.ContainsKey(key))
                {
                    return GetOrCreateDrawBufferTexture(key).RenderTarget.TextureColor.Bind();
                }
            }

            var currentTexture = OpenglGpuImpl.TextureCache.Get(gpuState);
            currentTexture.Bind();
            return currentTexture.Texture;
        }

        //public int GetDrawTexture(DrawBufferKey Key)
        //{
        //	//Console.WriteLine("GetDrawTexture: {0}", GetCurrentDrawBufferTexture(Key).TextureColor);
        //	return GetCurrentDrawBufferTexture(Key).TextureColor;
        //}

        OpenglGpuImpl OpenglGpuImpl;

        public RenderbufferManager(OpenglGpuImpl openglGpuImpl)
        {
            OpenglGpuImpl = openglGpuImpl;
        }

        public DrawBufferValue GetDrawBufferTexture(DrawBufferKey key)
        {
            var value = _drawBufferTextures.GetOrDefault(key, null);
            value?.WaitUnlocked();
            return value;
        }

        public void GetDrawBufferTextureAndLock(DrawBufferKey key, Action<DrawBufferValue> action)
        {
            var renderBuffer = GetDrawBufferTexture(key);
            if (renderBuffer != null)
            {
                renderBuffer.Lock();
                try
                {
                    action(renderBuffer);
                }
                finally
                {
                    renderBuffer.Unlock();
                }
            }
            else
            {
                action(null);
            }
        }

        public DrawBufferValue GetOrCreateDrawBufferTexture(DrawBufferKey key)
        {
            if (!_drawBufferTextures.ContainsKey(key))
                _drawBufferTextures[key] = new DrawBufferValue(OpenglGpuImpl, key);
            return GetDrawBufferTexture(key);
        }

        public void DrawVideo(uint frameBufferAddress, OutputPixel* outputPixel, int width, int height)
        {
            var drawBuffer = GetOrCreateDrawBufferTexture(new DrawBufferKey()
            {
                Address = frameBufferAddress,
            });
            drawBuffer.Bind();
            //GL.DrawPixels(Width, Height, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new IntPtr(OutputPixel));
            drawBuffer.Unbind();
            Console.WriteLine("DrawVideo: {0:X8}, {1}x{2}", frameBufferAddress, width, height);
        }

        private uint _cachedBindAddress;
        public DrawBufferValue CurrentDrawBuffer { get; private set; }

        public void BindCurrentDrawBufferTexture(GpuStateStruct gpuState)
        {
            if (_cachedBindAddress != gpuState.DrawBufferState.Address)
            {
                GL.glFlush();
                GL.glFinish();

                _cachedBindAddress = gpuState.DrawBufferState.Address;
                var key = new DrawBufferKey()
                {
                    Address = gpuState.DrawBufferState.Address,
                    //Width = (int)GpuState->DrawBufferState.Width,
                    //Height = (int)272,
                };
                CurrentDrawBuffer?.Unbind();
                CurrentDrawBuffer = GetOrCreateDrawBufferTexture(key);
                CurrentDrawBuffer.Bind();
            }
        }
    }
}
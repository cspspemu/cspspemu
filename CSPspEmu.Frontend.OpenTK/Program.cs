using System;
using System.IO;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Components.Controller;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Core.Components.Rtc;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
using CSPspEmu.Core.Types.Controller;
using CSPspEmu.Runner;
using CSPspEmu.Runner.Components.Display;
using CSPspEmu.Utils.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSPspEmu.Frontend
{
    unsafe class Game : GameWindow
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Game().Run();
            
        }

        private PspEmulator Emulator;

        public Game() : base(
            width: 1280, // initial width
            height: 720, // initial height
            mode: GraphicsMode.Default,
            title: "Title", // initial title
            options: GameWindowFlags.Default,
            device: DisplayDevice.Default,
            major: 1, minor: 1, // OpenGL major/minor version
            GraphicsContextFlags.ForwardCompatible
        )
        {
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            var bitmap = injector.GetInstance<PspDisplay>().TakePspScreenshot();
            fixed (OutputPixel* ptr = bitmap.GetPixels())
            {
                File.WriteAllBytes("/tmp/demo.png", new Span<byte>(ptr, bitmap.Width * bitmap.Height * 4).ToArray());
            }
        }

        SceCtrlData ctrlData = new SceCtrlData {Buttons = 0, Lx = 0, Ly = 0};

        private void OnKeyChange(KeyboardKeyEventArgs e, bool down)
        {
            var button = e.Key switch
            {
                Key.W => (PspCtrlButtons) PspCtrlButtons.Triangle,
                Key.A => (PspCtrlButtons) PspCtrlButtons.Square,
                Key.D => (PspCtrlButtons) PspCtrlButtons.Circle,
                Key.S => (PspCtrlButtons) PspCtrlButtons.Cross,
                _ => (PspCtrlButtons) 0
            };

            if (button != 0)
            {
                if (down)
                {
                    ctrlData.Buttons |= button;
                }
                else
                {
                    ctrlData.Buttons &= ~button;
                }
            }
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            OnKeyChange(e, true);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            OnKeyChange(e, false);
        }

        private InjectContext injector;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Console.WriteLine("LOAD");

            injector = PspInjectContext.CreateInjectContext(PspStoredConfig.Load(), false);

            Emulator = injector.GetInstance<PspEmulator>();
            Emulator.PspRunner = null;
            //Injector.InjectDependencesTo(Emulator);

            Emulator.CpuConfig.DebugSyscalls = true;
            Emulator.CpuConfig.TrackCallStack = true;
            //Emulator.LoadFile("../../../../deploy/cspspemu/demos/ortho.pbp");
            Emulator.LoadFile("../../../../deploy/cspspemu/demos/compilerPerf.pbp");

            var emulator = Emulator;
            var rtc = emulator.InjectContext.GetInstance<PspRtc>();
            var display = emulator.InjectContext.GetInstance<PspDisplay>();
            var displayComponent = emulator.InjectContext.GetInstance<DisplayComponentThread>();
            var memory = emulator.InjectContext.GetInstance<PspMemory>();
            var controller = emulator.InjectContext.GetInstance<PspController>();
            //displayComponent.triggerStuff = false;

            var lx = 0;
            var ly = 0;

            var pressingAnalogLeft = 0;
            var pressingAnalogRight = 0;
            var pressingAnalogUp = 0;
            var pressingAnalogDown = 0;

            display.VBlankEventCall += () =>
            {
                lx = (pressingAnalogLeft != 0) ? -pressingAnalogLeft : pressingAnalogRight;
                ly = (pressingAnalogUp != 0) ? -pressingAnalogUp : pressingAnalogDown;

                ctrlData.X = lx / 3f;
                ctrlData.Y = ly / 3f;
                ctrlData.TimeStamp = (uint) rtc.UnixTimeStampTS.Milliseconds;

                controller.InsertSceCtrlData(ctrlData);
                //SDL.SDL_RenderClear(renderer);

                //SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
                //SDL.SDL_RenderPresent(renderer);

                display.TriggerVBlankEnd();
            };

            /*
            displayComponent.Step(
                DrawStart: () => { display.TriggerDrawStart(); },
                VBlankStart: () => { display.TriggerVBlankStart(); },
                VBlankEnd: () =>
                {
                    lx = (pressingAnalogLeft != 0) ? -pressingAnalogLeft : pressingAnalogRight;
                    ly = (pressingAnalogUp != 0) ? -pressingAnalogUp : pressingAnalogDown;

                    ctrlData.X = lx / 3f;
                    ctrlData.Y = ly / 3f;
                    ctrlData.TimeStamp = (uint) rtc.UnixTimeStampTS.Milliseconds;

                    controller.InsertSceCtrlData(ctrlData);
                    //SDL.SDL_RenderClear(renderer);

                    //SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
                    //SDL.SDL_RenderPresent(renderer);

                    display.TriggerVBlankEnd();
                }
            );
            */

            //Emulator.CreateNewContextAndRemoveOldOne();

            /*
            Emulator.StartAndLoad("../../../../deploy/cspspemu/demos/ortho.pbp", emulator =>
            {
                var rtc = emulator.InjectContext.GetInstance<PspRtc>();
                var display = emulator.InjectContext.GetInstance<PspDisplay>();
                var displayComponent = emulator.InjectContext.GetInstance<DisplayComponentThread>();
                var memory = emulator.InjectContext.GetInstance<PspMemory>();
                var controller = emulator.InjectContext.GetInstance<PspController>();
                displayComponent.triggerStuff = false;

                var ctrlData = new SceCtrlData {Buttons = 0, Lx = 0, Ly = 0};

                var lx = 0;
                var ly = 0;

                var pressingAnalogLeft = 0;
                var pressingAnalogRight = 0;
                var pressingAnalogUp = 0;
                var pressingAnalogDown = 0;

                displayComponent.Step(
                    DrawStart: () => { display.TriggerDrawStart(); },
                    VBlankStart: () => { display.TriggerVBlankStart(); },
                    VBlankEnd: () =>
                    {
                        lx = (pressingAnalogLeft != 0) ? -pressingAnalogLeft : pressingAnalogRight;
                        ly = (pressingAnalogUp != 0) ? -pressingAnalogUp : pressingAnalogDown;

                        ctrlData.X = lx / 3f;
                        ctrlData.Y = ly / 3f;
                        ctrlData.TimeStamp = (uint) rtc.UnixTimeStampTS.Milliseconds;

                        controller.InsertSceCtrlData(ctrlData);
                        //SDL.SDL_RenderClear(renderer);

                        //SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
                        //SDL.SDL_RenderPresent(renderer);

                        display.TriggerVBlankEnd();
                    }
                );
                //emulator.
            });
            */

            //GL.create

        }
        int texture = -1;

        private void CheckError(string name)
        {
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine($"{name}: {error}");
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0.3f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (texture <= 0)
            {
                texture = GL.GenTexture();
            }

            GL.BindTexture(TextureTarget.Texture2D, texture);
            CheckError("BindTexture");
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            CheckError("TexParameter");
            var bitmap = injector.GetInstance<PspDisplay>().TakePspScreenshot();
            var pixelsArray = new OutputPixel[bitmap.Area];
            fixed(OutputPixel * pixels = pixelsArray)
            {
                PixelFormatDecoder.Decode(bitmap.GuPixelFormat, bitmap.Address, pixels, bitmap.Width, bitmap.Height);
                /*
                var data = new byte[512 * 272 * 4];
                for (int i = 0; i < 512 * 272 * 4; i++)
                {
                    data[i] = 0x10;
                    //pixels[i] = OutputPixel.FromRgba(255, 128, 128, 255);
                }
                Console.WriteLine($"{bitmap.Width}, {bitmap.Height}, {pixels[0]}");
                */
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, new IntPtr(pixels));
                CheckError("TexImage2D");
                GL.BindTexture(TextureTarget.Texture2D, 0);
                CheckError("BindTexture");
            }
            
            GL.Viewport(0, 0, Width, Height);
            CheckError("Viewport");

            GL.Enable(EnableCap.Texture2D);
            GL.MatrixMode(MatrixMode.Projection);
            CheckError("MatrixMode");
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            CheckError("BindTexture");

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1f, -1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(+1f, -1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(+1f, +1f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1f, +1f);

            GL.End();
            CheckError("End");


            //GL.BindTexture(TextureTarget.Texture2D, 0);
            SwapBuffers();
        }
    }
}
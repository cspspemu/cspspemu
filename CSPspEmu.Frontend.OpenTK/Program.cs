﻿using System;
using System.IO;
using CSPspEmu.Core.Components.Controller;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Core.Components.Rtc;
using CSPspEmu.Core.Types;
using CSPspEmu.Core.Types.Controller;
using CSPspEmu.Emulator.Simple;
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

        private SimplifiedPspEmulator SimplifiedPspEmulator;
        private PspEmulator Emulator => SimplifiedPspEmulator.Emulator;

        public Game() : base(
            width: 1280, // initial width
            height: 720, // initial height
            mode: GraphicsMode.Default,
            title: "PSP Emulator", // initial title
            options: GameWindowFlags.Default,
            device: DisplayDevice.Default,
            major: 1, minor: 1, // OpenGL major/minor version
            GraphicsContextFlags.ForwardCompatible
        )
        {
        }

        SceCtrlData ctrlData = new SceCtrlData {Buttons = 0, Lx = 0, Ly = 0};
        int lx = 0;
        int ly = 0;

        int pressingAnalogLeft = 0;
        int pressingAnalogRight = 0;
        int pressingAnalogUp = 0;
        int pressingAnalogDown = 0;


        private void OnKeyChange(KeyboardKeyEventArgs e, bool down)
        {
            var button = e.Key switch
            {
                Key.W => PspCtrlButtons.Triangle,
                Key.A => PspCtrlButtons.Square,
                Key.D => PspCtrlButtons.Circle,
                Key.S => PspCtrlButtons.Cross,
                Key.Q => PspCtrlButtons.LeftTrigger,
                Key.E => PspCtrlButtons.RightTrigger,
                Key.Up => PspCtrlButtons.Up,
                Key.Down => PspCtrlButtons.Down,
                Key.Left => PspCtrlButtons.Left,
                Key.Right => PspCtrlButtons.Right,
                Key.Space => PspCtrlButtons.Select,
                Key.Enter => PspCtrlButtons.Start,
                _ => (PspCtrlButtons) 0
            };

            if (e.Key == Key.F3)
            {
                var bitmap = SimplifiedPspEmulator.Display.TakePspScreenshot();
                fixed (OutputPixel* ptr = bitmap.GetPixels())
                {
                    File.WriteAllBytes("/tmp/cspspemu.screenshot.raw", new Span<byte>(ptr, bitmap.Width * bitmap.Height * 4).ToArray());
                }
            } 
            
            Console.WriteLine(button);

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
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Console.WriteLine("LOAD");

            SimplifiedPspEmulator = new SimplifiedPspEmulator();

            
            //SimplifiedPspEmulator.LoadAndStart("../../../../deploy/cspspemu/demos/ortho.pbp");
            SimplifiedPspEmulator.LoadAndStart("../../../../deploy/cspspemu/demos/compilerPerf.pbp");

            var rtc = SimplifiedPspEmulator.Rtc;
            var display = SimplifiedPspEmulator.Display;
            var controller = SimplifiedPspEmulator.Controller;

            /*
            display.VBlankEventCall += () =>
            {
            };
            */
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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
            CheckError("TexParameter");
            var bitmap = SimplifiedPspEmulator.Display.TakePspScreenshot();
            var pixelsArray = new OutputPixel[bitmap.Area];
            fixed (OutputPixel* pixels = pixelsArray)
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
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, new IntPtr(pixels));
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

            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex2(-1f, -1f);
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex2(+1f, -1f);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex2(+1f, +1f);
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex2(-1f, +1f);

            GL.End();
            CheckError("End");


            //GL.BindTexture(TextureTarget.Texture2D, 0);
            SwapBuffers();
            
            lx = pressingAnalogLeft != 0 ? -pressingAnalogLeft : pressingAnalogRight;
            ly = pressingAnalogUp != 0 ? -pressingAnalogUp : pressingAnalogDown;

            ctrlData.X = lx / 3f;
            ctrlData.Y = ly / 3f;
            ctrlData.TimeStamp = (uint) Emulator.InjectContext.GetInstance<PspRtc>().UnixTimeStampTS.Milliseconds;

            //Console.WriteLine("controller.InsertSceCtrlData(ctrlData)");
            Emulator.InjectContext.GetInstance<PspController>().InsertSceCtrlData(ctrlData);
                
            Emulator.InjectContext.GetInstance<PspDisplay>().TriggerVBlankEnd();

        }
    }
}
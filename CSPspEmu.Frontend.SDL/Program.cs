using System;
using System.Reflection.Emit;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu;
using CSPspEmu.Core.Components.Controller;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Core.Components.Rtc;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types.Controller;
using CSPspEmu.Runner.Components.Display;
using CSPspEmu.Utils;
using SDL2;

class Program
{
    static unsafe void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        using var pspEmulator = new PspEmulator();
        
        //pspEmulator.StartAndLoad("../deploy/cspspemu/demos/ortho.pbp", GuiRunner: (emulator) =>
        pspEmulator.StartAndLoad("../deploy/cspspemu/demos/compilerPerf.pbp", GuiRunner: (emulator) =>
            //pspEmulator.StartAndLoad("../deploy/cspspemu/demos/cubevfpu.prx", GuiRunner: (emulator) =>
            //pspEmulator.StartAndLoad("../deploy/cspspemu/demos/cwd.elf", GuiRunner: (emulator) =>
            //pspEmulator.StartAndLoad("../deploy/cspspemu/demos/fileio.pbp", GuiRunner: (emulator) =>
            //pspEmulator.StartAndLoad("../deploy/cspspemu/demos/fputest.pbp", GuiRunner: (emulator) =>
            //pspEmulator.StartAndLoad("../deploy/cspspemu/demos/malloctest.pbp", GuiRunner: (emulator) =>
            //pspEmulator.StartAndLoad("../deploy/cspspemu/demos/minifire.elf", GuiRunner: (emulator) =>
            //pspEmulator.StartAndLoad("../deploy/cspspemu/demos/lights.pbp", GuiRunner: (emulator) =>
            //pspEmulator.StartAndLoad("../deploy/cspspemu/demos/polyphonic.elf", GuiRunner: (emulator) =>
        {
            Console.WriteLine("Hello World!");
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) != 0)
            {
                Console.Error.WriteLine("Couldn't initialize SDL");
                return;
            }

            var window = SDL.SDL_CreateWindow(
                "",
                SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
                480 * 2, 272 * 2,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
            );
            SDL.SDL_SetWindowTitle(window, "C# PSP Emulator");
            var renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            /*
                var surface = SDL.SDL_CreateRGBSurface(0, 480, 272, 32, 0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000);

                SDL.SDL_LockSurface(surface);
                var surfaceInfo = (SDL.SDL_Surface*) surface.ToPointer();
                var pixels = (uint*) surfaceInfo->pixels.ToPointer();
                for (var n = 0; n < 480 * 272; n++)
                {
                    pixels[n] = 0x0000FF00;
                }
                SDL.SDL_UnlockSurface(surface);
                */

            //SDL.SDL_FillRect(SDL.SDL_Rect)
            var texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGBA8888,
                (int) SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, 480, 272);
            //var texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);

            //var surface = SDL.SDL_GetWindowSurface(window);
            //var renderer = SDL.SDL_CreateSoftwareRenderer(surface);
            var running = true;

            var rtc = emulator.InjectContext.GetInstance<PspRtc>();
            var display = emulator.InjectContext.GetInstance<PspDisplay>();
            var displayComponent = emulator.InjectContext.GetInstance<DisplayComponentThread>();
            var memory = emulator.InjectContext.GetInstance<PspMemory>();
            var controller = emulator.InjectContext.GetInstance<PspController>();
            displayComponent.triggerStuff = false;

            //var image = SDL_image.IMG_Load("icon0.png");
            //var texture = SDL.SDL_CreateTextureFromSurface(renderer, image);
            var ctrlData = new SceCtrlData();

            ctrlData.Buttons = 0;
            ctrlData.Lx = 0;
            ctrlData.Ly = 0;

            var lx = 0;
            var ly = 0;

            var pressingAnalogLeft = 0;
            var pressingAnalogRight = 0;
            var pressingAnalogUp = 0;
            var pressingAnalogDown = 0;

            PspCtrlButtons UpdatePressing(ref int value, bool pressing)
            {
                if (pressing)
                {
                    value++;
                }
                else
                {
                    value = 0;
                }

                return 0;
            }

            while (running)
            {
                while (SDL.SDL_PollEvent(out var e) != 0)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            running = false;
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                        case SDL.SDL_EventType.SDL_KEYUP:
                            var pressed = e.type == SDL.SDL_EventType.SDL_KEYDOWN;
                            var buttonMask = e.key.keysym.sym switch
                                {
                                SDL.SDL_Keycode.SDLK_a => PspCtrlButtons.Square,
                                SDL.SDL_Keycode.SDLK_w => PspCtrlButtons.Triangle,
                                SDL.SDL_Keycode.SDLK_d => PspCtrlButtons.Circle,
                                SDL.SDL_Keycode.SDLK_s => PspCtrlButtons.Cross,
                                SDL.SDL_Keycode.SDLK_SPACE => PspCtrlButtons.Select,
                                SDL.SDL_Keycode.SDLK_RETURN => PspCtrlButtons.Start,
                                SDL.SDL_Keycode.SDLK_UP => PspCtrlButtons.Up,
                                SDL.SDL_Keycode.SDLK_DOWN => PspCtrlButtons.Down,
                                SDL.SDL_Keycode.SDLK_LEFT => PspCtrlButtons.Left,
                                SDL.SDL_Keycode.SDLK_RIGHT => PspCtrlButtons.Right,
                                SDL.SDL_Keycode.SDLK_i => UpdatePressing(ref pressingAnalogUp, pressed),
                                SDL.SDL_Keycode.SDLK_k => UpdatePressing(ref pressingAnalogDown, pressed),
                                SDL.SDL_Keycode.SDLK_j => UpdatePressing(ref pressingAnalogLeft, pressed),
                                SDL.SDL_Keycode.SDLK_l => UpdatePressing(ref pressingAnalogRight, pressed)
                                };

                                
                            if (pressed)
                            {
                                ctrlData.Buttons |= buttonMask;
                            }
                            else
                            {
                                ctrlData.Buttons &= ~buttonMask;
                            }

                            break;
                    }
                }

                /*
                    SDL.SDL_SetRenderDrawColor(renderer, 0xFF, (byte)n, (byte)n, 0xFF);
                    n++;
                    SDL.SDL_RenderClear(renderer);
                    SDL.SDL_UpdateWindowSurface(window);
                    */

                {
                    //Console.WriteLine(display.CurrentInfo.FrameAddress);
                    var pixels2 = new uint[512 * 272];
                    var displayData = memory.Range<uint>(display.CurrentInfo.FrameAddress, 512 * 272);
                    for (var m = 0; m < 512 * 272; m++)
                    {
                        var color = displayData[m];
                        var r = BitUtils.Extract(color, 0, 8);
                        var g = BitUtils.Extract(color, 8, 8);
                        var b = BitUtils.Extract(color, 16, 8);
                        pixels2[m] = (r << 24) | (g << 16) | (b << 8) | 0xFF;
                    }

                    fixed (uint* pp = pixels2)
                    {
                        var rect = new SDL.SDL_Rect() {x = 0, y = 0, w = 480, h = 272};
                        SDL.SDL_UpdateTexture(texture, ref rect, new IntPtr(pp), 512 * 4);
                    }
                }
                displayComponent.Step(DrawStart: () =>
                {

                    display.TriggerDrawStart();                        
                }, VBlankStart: () =>
                {
                    display.TriggerVBlankStart();                        
                }, VBlankEnd: () =>
                {

                    lx = (pressingAnalogLeft != 0) ? -pressingAnalogLeft : pressingAnalogRight;
                    ly = (pressingAnalogUp != 0) ? -pressingAnalogUp : pressingAnalogDown;

                    ctrlData.X = lx / 3f;
                    ctrlData.Y = ly / 3f;
                    ctrlData.TimeStamp = (uint) rtc.UnixTimeStampTS.Milliseconds;

                    controller.InsertSceCtrlData(ctrlData);
                    //SDL.SDL_RenderClear(renderer);
                    SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
                    SDL.SDL_RenderPresent(renderer);

                    display.TriggerVBlankEnd();
                });
                //display.TriggerVBlankStart();

                //display.TriggerVBlankEnd();
            }
            //SDL.SDL_FreeSurface(image);

            SDL.SDL_DestroyTexture(texture);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        });
    }
}

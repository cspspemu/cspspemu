using System;
using SDL2;

namespace CSPspEmu.Gui.GtkSharp
{
    public class GtkProgram
    {
        /*
        public static void Main(string[] args)
        {
            Application.Init ();
 
            Window window = new Window ("helloworld");
            window.Show();
 
            Application.Run ();
            Thread.Sleep(TimeSpan.FromMilliseconds(10000));
        }
        
        public static void RunStart(IGuiExternalInterface IGuiExternalInterface)
        {
            Application.Init ();
 
            Window window = new Window ("helloworld");
            window.Show();
 
            Application.Run ();
            Thread.Sleep(TimeSpan.FromMilliseconds(10000));
        }
        */
        /*
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            app.Run(new MainWindow());
        }
        */
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) != 0)
            {
                Console.Error.WriteLine("Couldn't initialize SDL");
                return;
            }
            var window = SDL.SDL_CreateWindow("hello", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 480, 272, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            SDL.SDL_Event e;
            var running = true;
            while (running)
            {
                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            running = false;
                            break;
                    }
                }
            }
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
        
    }
}
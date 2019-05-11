using System;
using SDL2;

class Program
{
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

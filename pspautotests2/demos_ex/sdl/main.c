#include <pspkernel.h>
#include <pspthreadman.h>
#include <pspdebug.h>
#include <stdio.h>
#include <string.h>

PSP_MODULE_INFO("SDL TEST", 0, 1, 1);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER | THREAD_ATTR_VFPU);

#include <SDL/SDL.h>
#include <SDL/SDL_mixer.h>

void SDL_putpixel(SDL_Surface *surface, int x, int y, Uint32 color) {
	*((Uint32 *)(surface->pixels + y * surface->pitch + x * sizeof(Uint32))) = color;
}

int filter_events(const SDL_Event *event) {
	return 1;
}

int main(int argc, char *argv[]) {
	SDL_Surface* screen;
	//Mix_Music *music;
	int x, y;

	//SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO | SDL_INIT_JOYSTICK);
	SDL_Init(SDL_INIT_VIDEO | SDL_INIT_JOYSTICK);
	screen = SDL_SetVideoMode(480, 272, 32, SDL_HWSURFACE);
	for (y = 0; y < 272; y++) {
		for (x = 0; x < 480; x++) {
			SDL_putpixel(screen, x, y, 0xFFFFFFFF);
		}
	}
	
	//Mix_Init(MIX_INIT_OGG);
	/*
	Mix_OpenAudio(44100, MIX_DEFAULT_FORMAT, 2, 1024);
	music = Mix_LoadMUS("nature_in_motion.mod");
	Kprintf("music-type: %d\n", Mix_GetMusicType(music));
	Mix_PlayMusic(music, 10);
	*/
	
	//SDL_SetEventFilter(filter_events);
	
	//Kprintf("Loading...\n");
	//SDL_Surface *bmp = SDL_LoadBMP("test.bmp");
	//SDL_Rect rect = {0, 0, 480, 272};
	while (1) {
		//Kprintf("Frame...\n");
		SDL_Event event;
		while (SDL_PollEvent(&event))  {
			switch (event.type) {
				case SDL_QUIT: return -1;
				case SDL_JOYBUTTONUP:
				case SDL_JOYBUTTONDOWN:
					Kprintf("Button: %d, %d\n", event.jbutton.button, event.type);
				break;
				default:
					Kprintf("Other event %d\n", event.type);
				break;
			}
		}
		//SDL_BlitSurface(bmp, NULL, screen, &rect);
		//Kprintf("Blitted...\n");
		//rect.y += 16;
		SDL_Delay(1000);
	}

	return 0;
}
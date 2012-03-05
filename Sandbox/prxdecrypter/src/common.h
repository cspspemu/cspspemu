#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED

#include <pspdebug.h>
#include <pspdisplay.h>

// colours
#define RGB(r, g, b) (0xFF000000 | ((b)<<16) | ((g)<<8) | (r))
#define COLOR_BLACK      0x00000000L
#define COLOR_WHITE      0x00FFFFFFL
#define COLOR_BLUE       0x00FF0000L
#define COLOR_RED        0x000000FFL
#define COLOR_GREEN      0x0000FF00L
#define COLOR_GREY       0x00808080L

#endif

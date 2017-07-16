/*
 * Battlegrounds 3 - http://xfacter.wordpress.com
 * -----------------------------------------------------------------------
 * Licensed under the BSD license, see LICENSE for details.
 *
 * Copyright (c) 2009 Alex Wickes
 */

#include <common.h>

#include <stdio.h>
#include <math.h>
#include <pspgu.h>
#include <pspgum.h>
#include <pspkernel.h>

#include "../../libs/xlib/xlog.c"
#include "../../libs/xlib/xmath.c"
#include "../../libs/xlib/xlib.c"
#include "../../libs/xlib/xmem.c"
#include "../../libs/xlib/xctrl.c"
#include "../../libs/xlib/xtime.c"
#include "../../libs/xlib/xgraphics.c"
//#include "../../libs/xlib/xsound.c"
#include "../../libs/xlib/xtexture.c"
#include "../../libs/xlib/xtext.c"
#include "../../libs/xlib/xobj.c"
#include "../../libs/xlib/xheightmap.c"
//#include "../../libs/xlib/xini.c"
#include "../../libs/xlib/xparticle.c"
#include "../../libs/xlib/xbuffer.c"

#define FADE_TIME 2.0f
#define FADE_WAIT 2.0f

void bg3_draw_tex(xTexture* tex, int x, int y)
{
	if (tex == NULL) return;
	xTexDraw(tex, x, y, tex->width, tex->height, 0, 0, tex->width, tex->height);
}

int xMain()
{
	//x_auto_srand();

	//xTimeInit();
	//xCtrlInit();

	xGuInit();
	xGuPerspective(75.0f, 0.5f, 1000.0f);
	xGuEnable(X_DITHER_SMOOTH);
	//xGuEnable(X_PSEUDO_AA);
	sceGuColor(0xffffffff);
	xGuTexMode(GU_TFX_MODULATE, 1);
	sceGuEnable(GU_TEXTURE_2D);
	xGuTexFilter(X_BILINEAR);

	sceGuEnable(GU_LIGHTING);
	sceGuEnable(GU_LIGHT0);

	//xSoundInit(32);
	//xSound3dSpeedOfSound(100.0f);

	xTexture* logo = xTexLoadTGA("./test.tga", 0, 0);

	xTimeUpdate();
	xGuClear(0xffffff);
	sceGuDisable(GU_LIGHTING);
	sceGuDisable(GU_BLEND);
	bg3_draw_tex(logo, 0, 0);
	xGuFrameEnd();

	emulatorEmitScreenshot();
    return 0;
}

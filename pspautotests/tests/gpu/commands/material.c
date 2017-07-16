#include <pspdisplay.h>
#include <pspgu.h>
#include <pspgum.h>
#include <common.h>
#include <pspkernel.h>
#include "commands.h"

#define BUF_WIDTH 512
#define SCR_WIDTH 480
#define SCR_HEIGHT 272

unsigned int __attribute__((aligned(16))) list[262144];
unsigned int __attribute__((aligned(16))) clut[] = { 0xaaaaaaaa, 0xffffffff, 0x00000000 };
unsigned char __attribute__((aligned(16))) bgData[] = {
	2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
};
unsigned char __attribute__((aligned(16))) imageData[] = {
	0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
	1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
};

typedef struct
{
	unsigned short a, b;
	unsigned short x, y, z;
} Vertex;

typedef struct
{
	unsigned short a, b;
	unsigned long color;
	unsigned short x, y, z;
} VertexColor;

Vertex bg[2] = { {0, 0, 0, 0, 0}, {4, 4, 479, 272, 0} };
Vertex vertices[2] = { {0, 0, 10, 10, 0}, {4, 4, 470, 262, 0} };
VertexColor colorVertices[2] = { {0, 0, 0x77333388, 10, 10, 0}, {4, 4, 0x77338833, 470, 262, 0} };
VertexColor transparentColorVertices[2] = { {0, 0, 0x00333388, 10, 10, 0}, {4, 4, 0x00338833, 470, 262, 0} };

void nextBox(int *x, int *y)
{
	vertices[0].x = *x;
	vertices[0].y = *y;
	vertices[1].x = *x + 40;
	vertices[1].y = *y + 20;
	*x += 47;
	if (*x >= 470) {
		*x = 10;
		*y += 26;
	}
	sceKernelDcacheWritebackRange(vertices, sizeof(vertices));

	sceGuEnable(GU_TEXTURE_2D);
	sceGuTexMode(GU_PSM_T8, 0, 0, GU_FALSE);
	sceGuTexFunc(GU_TFX_DECAL, GU_TCC_RGBA);
	sceGuTexImage(0, 4, 4, 16, imageData);

	sceGuDrawArray(GU_SPRITES, GU_TEXTURE_16BIT | GU_VERTEX_16BIT | GU_TRANSFORM_2D, 2, NULL, vertices);
}

typedef enum HasColorMode
{
	HASCOLOR_RGBA = 0x00,
	HASCOLOR_RGB = 0x01,
	HASCOLOR_RGBA_TRANSPARENT = 0x10,
	HASCOLOR_RGB_TRANSPARENT = 0x11,
} HasColorMode;

void nextBoxHasColor(int *x, int *y, HasColorMode mode)
{
	VertexColor *sendVertices = mode & HASCOLOR_RGBA_TRANSPARENT ? transparentColorVertices : colorVertices;

	sendVertices[0].x = *x;
	sendVertices[0].y = *y;
	sendVertices[1].x = *x + 40;
	sendVertices[1].y = *y + 20;
	*x += 47;
	if (*x >= 470) {
		*x = 10;
		*y += 26;
	}
	sceKernelDcacheWritebackRange(sendVertices, sizeof(colorVertices));

	sceGuEnable(GU_TEXTURE_2D);
	sceGuTexMode(GU_PSM_T8, 0, 0, GU_FALSE);
	sceGuTexFunc(GU_TFX_DECAL, mode & HASCOLOR_RGB ? GU_TCC_RGB : GU_TCC_RGBA);
	sceGuTexImage(0, 4, 4, 16, imageData);

	sceGuDrawArray(GU_SPRITES, GU_COLOR_8888 | GU_TEXTURE_16BIT | GU_VERTEX_16BIT | GU_TRANSFORM_2D, 2, NULL, sendVertices);
}

void drawBG()
{
	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0xFF);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0x550055);
	sceGuEnable(GU_TEXTURE_2D);
	sceGuTexMode(GU_PSM_T8, 0, 0, GU_FALSE);
	sceGuTexFunc(GU_TFX_DECAL, GU_TCC_RGBA);
	sceGuTexImage(0, 4, 4, 16, bgData);

	sceGuDrawArray(GU_SPRITES, GU_TEXTURE_16BIT | GU_VERTEX_16BIT | GU_TRANSFORM_2D, 2, NULL, bg);
}

void draw()
{
	int x = 10, y = 10;

	sceDisplaySetMode(0, SCR_WIDTH, SCR_HEIGHT);
	sceGuStart(GU_DIRECT, list);
	sceGuClear(GU_COLOR_BUFFER_BIT);

	sceGuClutMode(GU_PSM_8888, 0, 0xFF, 0);
	sceGuClutLoad(1, clut);
	
	drawBG();

	// Reset things.
	sceGuSendCommandi(GE_CMD_LIGHTINGENABLE, 0);
	sceGuSendCommandi(GE_CMD_MATERIALALPHA, 0x00);
	sceGuSendCommandi(GE_CMD_AMBIENTCOLOR, 0x000000);
	sceGuSendCommandi(GE_CMD_AMBIENTALPHA, 0x00);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0);
	nextBox(&x, &y);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0xFF);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0xFF0000);
	nextBox(&x, &y);

	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0x00FF00);
	nextBox(&x, &y);
	
	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0x0000FF);
	nextBox(&x, &y);
	
	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0);
	nextBoxHasColor(&x, &y, HASCOLOR_RGBA);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, GU_AMBIENT);
	sceGuSendCommandi(GE_CMD_MATERIALALPHA, 0x00);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0xFF0000);
	sceGuSendCommandi(GE_CMD_AMBIENTCOLOR, 0x0000FF);
	sceGuSendCommandi(GE_CMD_AMBIENTALPHA, 0x00);
	sceGuSendCommandi(GE_CMD_LIGHTINGENABLE, 1);
	nextBoxHasColor(&x, &y, HASCOLOR_RGBA);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0xFF);
	sceGuSendCommandi(GE_CMD_MATERIALALPHA, 0x80);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0xFFFFFF);
	sceGuSendCommandi(GE_CMD_AMBIENTCOLOR, 0xFFFFFF);
	sceGuSendCommandi(GE_CMD_AMBIENTALPHA, 0x80);
	sceGuSendCommandi(GE_CMD_LIGHTINGENABLE, 1);
	nextBoxHasColor(&x, &y, HASCOLOR_RGBA);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0);
	nextBoxHasColor(&x, &y, HASCOLOR_RGBA_TRANSPARENT);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, GU_AMBIENT);
	sceGuSendCommandi(GE_CMD_MATERIALALPHA, 0x0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0xFF0000);
	sceGuSendCommandi(GE_CMD_AMBIENTCOLOR, 0x0000FF);
	sceGuSendCommandi(GE_CMD_AMBIENTALPHA, 0x0);
	sceGuSendCommandi(GE_CMD_LIGHTINGENABLE, 1);
	nextBoxHasColor(&x, &y, HASCOLOR_RGBA_TRANSPARENT);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0xFF);
	sceGuSendCommandi(GE_CMD_MATERIALALPHA, 0x10);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0xFFFFFF);
	sceGuSendCommandi(GE_CMD_AMBIENTCOLOR, 0xFFFFFF);
	sceGuSendCommandi(GE_CMD_AMBIENTALPHA, 0x10);
	sceGuSendCommandi(GE_CMD_LIGHTINGENABLE, 1);
	nextBoxHasColor(&x, &y, HASCOLOR_RGBA_TRANSPARENT);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0);
	nextBoxHasColor(&x, &y, HASCOLOR_RGB);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, GU_AMBIENT);
	sceGuSendCommandi(GE_CMD_MATERIALALPHA, 0x0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0xFF0000);
	sceGuSendCommandi(GE_CMD_AMBIENTCOLOR, 0x0000FF);
	sceGuSendCommandi(GE_CMD_AMBIENTALPHA, 0x0);
	sceGuSendCommandi(GE_CMD_LIGHTINGENABLE, 1);
	nextBoxHasColor(&x, &y, HASCOLOR_RGB);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0xFF);
	sceGuSendCommandi(GE_CMD_MATERIALALPHA, 0x10);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0xFFFFFF);
	sceGuSendCommandi(GE_CMD_AMBIENTCOLOR, 0xFFFFFF);
	sceGuSendCommandi(GE_CMD_AMBIENTALPHA, 0x10);
	sceGuSendCommandi(GE_CMD_LIGHTINGENABLE, 1);
	nextBoxHasColor(&x, &y, HASCOLOR_RGB);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0);
	nextBoxHasColor(&x, &y, HASCOLOR_RGB_TRANSPARENT);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, GU_AMBIENT);
	sceGuSendCommandi(GE_CMD_MATERIALALPHA, 0x0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0xFF0000);
	sceGuSendCommandi(GE_CMD_AMBIENTCOLOR, 0x0000FF);
	sceGuSendCommandi(GE_CMD_AMBIENTALPHA, 0x0);
	sceGuSendCommandi(GE_CMD_LIGHTINGENABLE, 1);
	nextBoxHasColor(&x, &y, HASCOLOR_RGB_TRANSPARENT);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0xFF);
	sceGuSendCommandi(GE_CMD_MATERIALALPHA, 0x10);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0xFFFFFF);
	sceGuSendCommandi(GE_CMD_AMBIENTCOLOR, 0xFFFFFF);
	sceGuSendCommandi(GE_CMD_AMBIENTALPHA, 0x10);
	sceGuSendCommandi(GE_CMD_LIGHTINGENABLE, 1);
	nextBoxHasColor(&x, &y, HASCOLOR_RGB_TRANSPARENT);

	sceGuSendCommandi(GE_CMD_MATERIALUPDATE, 0);
	sceGuSendCommandi(GE_CMD_MATERIALAMBIENT, 0);
	nextBox(&x, &y);

	/*ScePspFVector3 pos = {x, y, 0};
	sceGuLight(0, GU_DIRECTIONAL, GU_AMBIENT_AND_DIFFUSE, &pos);
	nextBox(&x, &y);

	pos.x = x; pos.y = y;
	sceGuLight(0, GU_DIRECTIONAL, GU_AMBIENT_AND_DIFFUSE, &pos);
	sceGuLightMode(GU_SINGLE_COLOR);
	sceGuLightColor(0, GU_AMBIENT, 0x00FF00);
	sceGuLightAtt(0, 1.0, 1.0, 1.0);
	nextBox(&x, &y);*/

	sceGuFinish();
	sceGuSync(GU_SYNC_LIST, GU_SYNC_WHAT_DONE);
	sceGuSync(0, 0);

	sceDisplayWaitVblankStart();
}

void init()
{
	void *fbp0 = 0;
 
	sceGuInit();
	sceGuStart(GU_DIRECT, list);
	sceGuDrawBuffer(GU_PSM_8888, fbp0, BUF_WIDTH);
	sceGuDispBuffer(SCR_WIDTH, SCR_HEIGHT, fbp0, BUF_WIDTH);
	sceGuScissor(0, 0, SCR_WIDTH, SCR_HEIGHT);
	sceGuEnable(GU_SCISSOR_TEST);
	sceGuFinish();
	sceGuSync(0, 0);
 
	sceDisplayWaitVblankStart();
	sceGuDisplay(1);
}

int main(int argc, char *argv[])
{
	int i;
	init();
	for (i = 0; i < 120; ++i) {
		draw();
	}

	emulatorEmitScreenshot();

	sceGuTerm();

	return 0;
}
#include <pspdisplay.h>
#include <pspgu.h>
#include <pspgum.h>
#include <common.h>

#define BUF_WIDTH 512
#define SCR_WIDTH 480
#define SCR_HEIGHT 272

unsigned int __attribute__((aligned(16))) list[262144];
unsigned int __attribute__((aligned(16))) clutWhite[] = { 0xFFFFFFFF, 0x00000000 };
unsigned char __attribute__((aligned(16))) imageData[16] = {0};

typedef struct
{
	unsigned short a, b;
	unsigned short x, y, z;
} Vertex;

Vertex vertices[2] = { {0, 0, 10, 10, 0}, {1, 1, 470, 262, 0} };

void draw()
{
	sceDisplaySetMode(0, SCR_WIDTH, SCR_HEIGHT);
	sceGuStart(GU_DIRECT, list);

	sceGuClear(GU_COLOR_BUFFER_BIT);
	sceGuEnable(GU_TEXTURE_2D);
	sceGuTexMode(GU_PSM_T8, 0, 0, GU_FALSE);
	sceGuTexFunc(GU_TFX_DECAL, GU_TCC_RGB);
	sceGuTexImage(0, 1, 1, 16, imageData);

	sceGuClutLoad(1, clutWhite);
	sceGuDrawArray(GU_SPRITES, GU_TEXTURE_16BIT | GU_VERTEX_16BIT | GU_TRANSFORM_2D, 2, NULL, vertices);

	sceGuFinish();
	sceGuSync(0, 0);

	sceDisplayWaitVblank();
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
	init();
	draw();

	emulatorEmitScreenshot();

	sceGuTerm();

	return 0;
}
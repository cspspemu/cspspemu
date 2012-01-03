#include <common.h>

#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspdebug.h>
#include <stdlib.h>
#include <stdio.h>
#include <math.h>
#include <string.h>
#include <time.h>

#include <pspgu.h>
#include <pspgum.h>
#include <pspdisplay.h>

static unsigned int __attribute__((aligned(16))) list[262144];

#define BUF_WIDTH (512)
#define SCR_WIDTH (480)
#define SCR_HEIGHT (272)
#define PIXEL_SIZE (4) /* change this if you change to another screenmode */
#define FRAME_SIZE (BUF_WIDTH * SCR_HEIGHT * PIXEL_SIZE)
#define ZBUF_SIZE (BUF_WIDTH * SCR_HEIGHT * 2) /* zbuffer seems to be 16-bit? */

void init() {
	printf("init()\n"); fflush(stdout);
	sceKernelDcacheWritebackAll();

	sceGuInit();
	sceGuStart(GU_DIRECT, list);

	sceGuDrawBuffer (GU_PSM_8888, (void*)FRAME_SIZE, BUF_WIDTH);
	//sceGuDispBuffer (SCR_WIDTH, SCR_HEIGHT, (void*)FRAME_SIZE, BUF_WIDTH);
	sceGuDepthBuffer((void *)(FRAME_SIZE * 2), BUF_WIDTH);
	sceGuOffset     (2048 - (SCR_WIDTH / 2),2048 - (SCR_HEIGHT / 2));
	sceGuViewport   (2048, 2048, SCR_WIDTH, SCR_HEIGHT);
	sceGuDepthRange (0xc350, 0x2710);
	sceGuScissor    (0, 0, SCR_WIDTH, SCR_HEIGHT);
	//sceGuEnable     (GU_SCISSOR_TEST);
	//sceGuFrontFace  (GU_CW);
	//sceGuClear      (GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);
	sceGuFinish     ();
	sceGuSync       (GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);

	//sceDisplayWaitVblankStart();
	//sceGuDisplay(GU_TRUE);
	printf("/init()\n"); fflush(stdout);
}

void gu_signal(int value) {
	printf("gu_signal(%08X)\n", (unsigned int)value); fflush(stdout);
}

void gu_finish(int value) {
	printf("gu_finish(%08X)\n", (unsigned int)value); fflush(stdout);
}

void testGuCallbacks() {
	//int cb_id;
	
	sceKernelDcacheWritebackAll();

	printf("sceGeEdramGetSize: %08X\n", (unsigned int)sceGeEdramGetSize()); fflush(stdout);
	printf("sceGeEdramGetAddr: %08X\n", (unsigned int)sceGeEdramGetAddr()); fflush(stdout);
	
	sceGuSetCallback(GU_CALLBACK_SIGNAL, gu_signal);
	sceGuSetCallback(GU_CALLBACK_FINISH, gu_finish);
	{
		sceGuStart(GU_DIRECT, list);
		{
			/*
			sceGuSignal(3, GU_BEHAVIOR_CONTINUE);
			sceGuSignal(4, GU_BEHAVIOR_SUSPEND);
			*/
			sceGuSignal(GU_BEHAVIOR_CONTINUE, 3);
			sceGuSignal(GU_BEHAVIOR_SUSPEND, 4);
		}
		sceGuFinish();
		sceGuSync(0, 0);
	}
	sceGuSetCallback(GU_CALLBACK_FINISH, NULL);
	sceGuSetCallback(GU_CALLBACK_SIGNAL, NULL);
	
	printf("End!\n"); fflush(stdout);
}

void testGeCallbacks() {
}

int main(int argc, char *argv[]) {
	printf("main\n"); fflush(stdout);
	init();
	testGuCallbacks();
	testGeCallbacks();
	return 0;
}

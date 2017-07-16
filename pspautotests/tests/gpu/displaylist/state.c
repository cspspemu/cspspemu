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
#include <pspsysmem.h>

static unsigned int __attribute__((aligned(16))) list[262144];

char buffer[65535];
char *bpos = &buffer[0];
int dlid[1024];
int dlidcount = 0;

//int sceKernelGetCompiledSdkVersion606(int version);

#define BUF_WIDTH (512)
#define SCR_WIDTH (480)
#define SCR_HEIGHT (272)
#define PIXEL_SIZE (4) /* change this if you change to another screenmode */
#define FRAME_SIZE (BUF_WIDTH * SCR_HEIGHT * PIXEL_SIZE)
#define ZBUF_SIZE (BUF_WIDTH * SCR_HEIGHT * 2) /* zbuffer seems to be 16-bit? */

char* status_str(int status) {
	if(status < 0 || status > 4)
		return "INVALID";
	char* str[] = {
	"DONE",
	"QUEUED",
	"DRAWING",
	"STALL",
	"BREAK",
	};
	return str[status];
}

void printListSync() {
	int i, status;
	status = sceGeDrawSync(1);
	bpos += sprintf(bpos, "sceGeDrawSync(1): 0x%x (%s)\n", status, status_str(status));
	for(i = 0; i < dlidcount; i++) {
		status = sceGeListSync(dlid[i], 1);
		if (dlid[i] < 0 && status == 0x80000100) {
			bpos += sprintf(bpos, "sceGeListSync(dl=%d, 1): Invalid list - 0x%x\n", i, dlid[i]);
		} else {
			bpos += sprintf(bpos, "sceGeListSync(dl=%d, 1): 0x%x (%s)\n", i, status, status_str(status));
		}
	}
}

void ge_signal(int value, void* arg) {
	int status = sceGeDrawSync(1);
	bpos += sprintf(bpos, "signal: sceGeDrawSync: %X %s\n", status, status_str(status));
}

void ge_finish(int value, void* arg) {
	int status = sceGeDrawSync(1);
	bpos += sprintf(bpos, "signal: sceGeDrawSync: %X %s\n", status, status_str(status));
}

int cbid, cbid2;

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
	
	PspGeCallbackData cbdata;
	cbdata.signal_func = ge_signal;
	cbdata.signal_arg  = NULL;
	cbdata.finish_func = ge_finish;
	cbdata.finish_arg  = NULL;	
	cbid = sceGeSetCallback(&cbdata);
}

unsigned int __attribute__((aligned(16))) dlist1[] = {
	0x00000000, // NOP, geman crashes if signal is first instruction
	0x0E010000, // SIGNAL + WAIT
	0x0C000000, // END
	0x00000000, // NOP
	0x0F000000, // FINISH
	0x0C000000, // END
};

void testGeCallbacks() {
	int res;
	dlid[dlidcount++] = sceGeListEnQueue(dlist1, 0, cbid, 0);
	dlid[dlidcount++] = sceGeListEnQueue(dlist1, dlist1, -1, 0);
	dlid[dlidcount++] = sceGeListEnQueue(dlist1, dlist1, -1, 0);
	printListSync();
	
	res = sceGeBreak(0, 0);
	bpos += sprintf(bpos, "sceGeBreak(0, ...): %d\n", dlid[1] == res);
	printListSync();
	
	res = sceGeBreak(0, 0);
	bpos += sprintf(bpos, "sceGeBreak(0, ...): 0x%08x\n", res);
	printListSync();

	res = sceGeContinue();
	bpos += sprintf(bpos, "sceGeContinue(): %d\n", res);
	printListSync();
	
	dlid[dlidcount++] = sceGeListEnQueueHead(dlist1, 0, -1, 0);
	bpos += sprintf(bpos, "sceGeListEnQueueHead(...)\n");
	printListSync();

	sceGeBreak(0, 0);
	dlid[dlidcount++] = sceGeListEnQueueHead(dlist1, dlist1, -1, 0);
	dlid[dlidcount++] = sceGeListEnQueueHead(dlist1, 0, -1, 0);
	bpos += sprintf(bpos, "sceGeBreak(0, 0) then sceGeListEnQueueHead(...)\n");
	printListSync();
	
	res = sceGeContinue();
	bpos += sprintf(bpos, "sceGeContinue(): %d\n", res);
	printListSync();
	
	res = sceGeListUpdateStallAddr(dlid[dlidcount - 2], 0);
	bpos += sprintf(bpos, "sceGeListUpdateStallAddr(%d): %d\n", dlidcount - 2, res);
	printListSync();
	
	res = sceGeBreak(1, 0);
	bpos += sprintf(bpos, "sceGeBreak(1, ...): %d\n", res);
	printListSync();

	dlidcount = 0;
	dlid[dlidcount++] = sceGeListEnQueue(dlist1, 0, -1, 0);
	dlid[dlidcount++] = sceGeListEnQueue(dlist1, 0, -1, 0);
	bpos += sprintf(bpos, "Enqueue 2 more\n");
	printListSync();
	
	sceGeListSync(dlid[dlidcount - 1], 0);
	bpos += sprintf(bpos, "After sceGeListSync with wait\n");
	printListSync();
	
	res = sceGeDrawSync(0);
	bpos += sprintf(bpos, "sceGeDrawSync(0): %d\n", res);
	printListSync();
	
	res = sceGeDrawSync(0);
	bpos += sprintf(bpos, "sceGeDrawSync(0): %d\n", res);

	printf("%s", buffer);
}

int main(int argc, char *argv[]) {
	//sceKernelGetCompiledSdkVersion606(0x6060000);
	printf("main\n"); fflush(stdout);
	init();
	testGeCallbacks();
	return 0;
}

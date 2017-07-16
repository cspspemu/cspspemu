#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <psprtc.h>

#include <stdlib.h>
#include <stdio.h>
#include <math.h>
#include <string.h>

#include <pspintrman.h>

// http://forums.ps2dev.org/viewtopic.php?t=5687
// @TODO! Fixme! In which thread should handlers be executed?

//#define eprintf(...) pspDebugScreenPrintf(__VA_ARGS__); printf(__VA_ARGS__);

void vblank_handler_counter(int no, int* counter) {
	*counter = *counter + 1;
}

void checkVblankInterruptHandler() {
	int counter = 0, last_counter = 0;
	int results[3], n;

	//pspDebugScreenInit();
	pspDebugScreenPrintf("Starting...\n");
	printf("Starting...\n");

	sceKernelRegisterSubIntrHandler(PSP_VBLANK_INT, 0, vblank_handler_counter, &counter);
	sceKernelDelayThread(80000);
	results[0] = counter;
	printf("NotEnabled: Counter:%s\n", (counter == 0) ? "zero" : "non-zero"); // 0. Not enabled yet.
	
	sceKernelEnableSubIntr(PSP_VBLANK_INT, 0);
	sceKernelDelayThread(160000);
	results[1] = counter;
	printf("Enabled (GreaterThan2): Counter:%s\n", (counter >= 2) ? "greater" : "no greater"); // n. Already enabled.

	sceKernelReleaseSubIntrHandler(PSP_VBLANK_INT, 0);
	last_counter = counter;
	sceKernelDelayThread(80000);
	results[2] = counter;
	printf("Disabled (NotChangedAfterDisabled): %s\n", (last_counter != counter) ? "changed" : "not changed"); // n. Disabled.
	
	for (n = 0; n < 3; n++) {
		//printf("Output %d:%d\n", n, results[n]);
		pspDebugScreenPrintf("%d\n", results[n]);
	}
}

/*
// Exit callback
int exitCallback(int arg1, int arg2, void *common) {
	sceKernelExitGame();
	return 0;
}

// Callback thread
int callbackThread(SceSize args, void *argp) {
	int cbid;

	cbid = sceKernelCreateCallback("Exit Callback", (void*) exitCallback, NULL);
	sceKernelRegisterExitCallback(cbid);
	sceKernelSleepThreadCB();

	return 0;
}

// Sets up the callback thread and returns its thread id
int setupCallbacks(void) {
	int thid = 0;

	thid = sceKernelCreateThread("update_thread", callbackThread, 0x11, 0xFA0, 0, 0);
	if (thid >= 0) {
		sceKernelStartThread(thid, 0, 0);
	}
	return thid;
}
*/

int main() {
	//setupCallbacks();
	checkVblankInterruptHandler();

	return 0;
}
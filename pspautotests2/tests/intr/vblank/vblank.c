#include <common.h>

#include <pspkernel.h>
#include <pspthreadman.h>
#include <pspdisplay.h>

SceUID sema = -1;
int vblankCalled = 0;
int mainThreadId = 0;
int called = 0;

int vblankCalledThread = -1;

void vblankCallback(int no, void *value) {
	//sceKernelSignalSema(sema, 1);
	/*
	if (!vblankCalled) {
		vblankCalled = 1;
	}
	*/
	vblankCalledThread = sceKernelGetThreadId();
	called = 1;
	*(uint *)value = 3;
	sceKernelSignalSema(sema, 1);
}

void basicUsage() {
	int value = 7;
	sema = sceKernelCreateSema("semaphore", 0, 0, 255, NULL);
	mainThreadId = sceKernelGetThreadId();

	//int cb = sceKernelCreateCallback("vblankCallback", vblankCallback, NULL);
	sceKernelRegisterSubIntrHandler(PSP_DISPLAY_SUBINT, 0, vblankCallback, &value);
	printf("beforeEnableVblankCallback\n");
	sceKernelEnableSubIntr(PSP_DISPLAY_SUBINT, 0);
	printf("afterEnableVblankCallback\n");
	
	sceKernelWaitSemaCB(sema, 1, NULL);
	//while (!vblankCalled) { sceKernelDelayThread(1000); }
	if (called) {
		printf("vblankCallback(%d):%d\n", *(int *)&value, (vblankCalledThread == mainThreadId));
	}
	
	sceKernelReleaseSubIntrHandler(PSP_DISPLAY_SUBINT, 0);
	//sceDisplayWaitVblank();
	
	printf("ended\n");
}

void vblank_counter(int no, int* counter) {
	*counter = *counter + 1;
}

void suspendUsage() {
	checkpointNext("sceKernelCpuSuspendIntr usage:");
	int counter;
	checkpoint("sceKernelRegisterSubIntrHandler 1: %08x", sceKernelRegisterSubIntrHandler(PSP_VBLANK_INT, 1, vblank_counter, &counter));

	counter = 0;
	int flag = sceKernelCpuSuspendIntr();
	checkpoint("sceKernelEnableSubIntr: %08x", sceKernelEnableSubIntr(PSP_VBLANK_INT, 1));
	checkpoint("sceKernelDelayThread: %08x", sceKernelDelayThread(300000));
	checkpoint("sceKernelDisableSubIntr: %08x", sceKernelDisableSubIntr(PSP_VBLANK_INT, 1));
	sceKernelCpuResumeIntr(flag);

	checkpoint("sceKernelDelayThread: %08x", sceKernelDelayThread(300000));
	checkpoint("Interrupts suspended: %d", counter);

	counter = 0;
	checkpoint("sceKernelRegisterSubIntrHandler 2: %08x", sceKernelRegisterSubIntrHandler(PSP_VBLANK_INT, 2, vblank_counter, &counter));
	checkpoint("sceKernelEnableSubIntr: %08x", sceKernelEnableSubIntr(PSP_VBLANK_INT, 2));
	checkpoint("sceKernelDelayThread: %08x", sceKernelDelayThread(300000));
	checkpoint("sceKernelDisableSubIntr: %08x", sceKernelDisableSubIntr(PSP_VBLANK_INT, 2));

	checkpoint("sceKernelDelayThread: %08x", sceKernelDelayThread(300000));
	checkpoint("Interrupts resumed: %d", counter);

	sceKernelReleaseSubIntrHandler(PSP_VBLANK_INT, 1);
	sceKernelReleaseSubIntrHandler(PSP_VBLANK_INT, 2);
}

int main(int argc, char** argv) {
	basicUsage();
	suspendUsage();
	
	return 0;
}
#include <common.h>

#include <pspkernel.h>
#include <pspctrl.h>
#include <psprtc.h>
#include <pspdisplay.h>

char schedulingLog[65536];
char *schedulingLogPos;

void schedf(const char *format, ...) {
	va_list args;
	va_start(args, format);
	schedulingLogPos += vsprintf(schedulingLogPos, format, args);
	// This is easier to debug in the emulator, but printf() reschedules on the real PSP.
	//vprintf(format, args);
	va_end(args);
}

void testGetCycle(const char *title, int *cyclep) {
	int result = sceCtrlGetSamplingCycle(cyclep);
	if (cyclep != NULL && cyclep != (int *) 0xDEADBEEF) {
		schedf("%s: %08X (%d)\n", title, result, *cyclep);
	} else {
		schedf("%s: %08X\n", title, result);
	}
}

void testSetCycle(const char *title, int cycle) {
	sceDisplayWaitVblank();
	int result = sceCtrlSetSamplingCycle(cycle);
	schedf("%s: %08X (%d)", title, result, cycle);

	int verify = 0xDEADBEEF;
	if (result >= 0) {
		sceCtrlGetSamplingCycle(&verify);
		if (cycle == verify) {
			schedf(" - OK\n");
		} else {
			schedf(" - mismatch %d vs %d\n", cycle, verify);
		}
	} else {
		sceCtrlGetSamplingCycle(&verify);
		schedf(" - still %d\n", verify);
	}
}

void testCycleLatch(int cycle) {
	SceCtrlLatch latch;

	// Ignore it this time, to reset it.
	sceCtrlReadLatch(&latch);
	sceDisplayWaitVblank();

	sceCtrlSetSamplingCycle(cycle);

	int vcountBefore = sceDisplayGetVcount();
	sceKernelDelayThread(166666);
	int vcountAfter = sceDisplayGetVcount();
	int after = sceCtrlReadLatch(&latch);

	// Keeping it approximate because timing is hard to get millisecond accurate.
	schedf("%d cycle: %dx\n", cycle, (after + 5) / (vcountAfter - vcountBefore));
}

int main(int argc, char *argv[]) {
	int result, cycle;

	schedulingLogPos = schedulingLog;

	schedf("sceCtrlGetSamplingCycle:\n");
	testGetCycle("Normal", &cycle);
	// Crashes.
	//testGetCycle("NULL", NULL);
	//testGetCycle("Normal", (int *) 0xDEADBEEF);

	testSetCycle("Zero", 0);
	testSetCycle("1", 1);
	testSetCycle("Negative", -1);
	testSetCycle("333", 333);
	testSetCycle("4444", 4444);
	testSetCycle("5554", 5554);
	testSetCycle("5555", 5555);
	testSetCycle("5556", 5556);
	testSetCycle("6666", 6666);
	testSetCycle("19999", 19999);
	testSetCycle("20000", 20000);
	testSetCycle("20001", 20001);
	testSetCycle("Zero", 0);
	
	testCycleLatch(0);
	testCycleLatch(5555);
	testCycleLatch(9999);
	testCycleLatch(16666);
	testCycleLatch(20000);

	printf("%s", schedulingLog);

	return 0;
}
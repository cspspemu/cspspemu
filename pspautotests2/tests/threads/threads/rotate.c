#include "shared.h"

int spinCount = 0;
int spinThread(SceSize argc, void *argv) {
	schedf("spinThread: start\n");

	for (spinCount = 0; spinCount < 10000; ++spinCount) {
		int result = sceKernelRotateThreadReadyQueue(0);
		if (result != 0) {
			schedf("spinThread: sceKernelRotateThreadReadyQueue returned: %08x\n", result);
			break;
		}
	}

	schedf("spinThread: end\n");
	return 0;
}

int spinCount2 = 0;
int spinThread2(SceSize argc, void *argv) {
	schedf("spinThread2: start\n");

	for (spinCount2 = 0; spinCount2 < 10000; ++spinCount2) {
		int result = sceKernelRotateThreadReadyQueue(0);
		if (result != 0) {
			schedf("spinThread2: sceKernelRotateThreadReadyQueue returned: %08x\n", result);
			break;
		}
	}

	schedf("spinThread2: end\n");
	return 0;
}

void testRotate(const char *title, int priority) {
	int result = sceKernelRotateThreadReadyQueue(priority);
	if (result == 0) {
		schedf("%s: OK\n", title);
	} else {
		schedf("%s: Failed (%08x)\n", title, result);
	}
}

int main(int argc, char *argv[]) {
	testRotate("Negative", -1);
	testRotate("Zero", 0);
	testRotate("Small", 1);
	testRotate("Priority 0x07", 7);
	testRotate("Priority 0x08", 8);
	testRotate("Priority 0x77", 0x77);
	testRotate("Priority 0x78", 0x78);

	SceUID thread = sceKernelCreateThread("spin", &spinThread, 0x20, 0x1000, 0, NULL);
	SceUID thread2 = sceKernelCreateThread("spin2", &spinThread2, 0x20, 0x1000, 0, NULL);

	schedf("main: starting spinThread\n");
	sceKernelStartThread(thread, 0, NULL);
	schedf("main: spinThread ready (%d)\n", spinCount);

	schedf("main: starting spinThread2\n");
	sceKernelStartThread(thread2, 0, NULL);
	schedf("main: spinThread2 ready (%d)\n", spinCount2);

	int result = sceKernelRotateThreadReadyQueue(0);
	schedf("main: rotated ready queue: %08x (%d, %d)\n", result, spinCount, spinCount2);

	sceKernelWaitThreadEnd(thread, NULL);
	sceKernelWaitThreadEnd(thread2, NULL);

	flushschedf();
	return 0;
}

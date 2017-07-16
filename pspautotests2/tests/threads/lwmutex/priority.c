#include "shared.h"

SceLwMutexWorkarea workarea;

static int threadFunction(SceSize argSize, void* argPointer) {
	int num = argPointer ? *((int*)argPointer) : 0;
	schedf("A%d\n", num);
	sceKernelLockLwMutexCB(&workarea, 1, NULL);
	schedf("B%d\n", num);
	sceKernelUnlockLwMutex(&workarea, 1);

	return 0;
}

void execPriorityTests(int attr, int deleteInstead) {
	SceUID threads[5];
	int test[5] = {1, 2, 3, 4, 5};
	int result;

	sceKernelCreateLwMutex(&workarea, "mutex1", attr, 1, NULL);

	int i;
	for (i = 0; i < 5; i++) {
		threads[i] = CREATE_PRIORITY_THREAD(threadFunction, 0x16 - i);
		sceKernelStartThread(threads[i], sizeof(int), (void*)&test[i]);
	}

	// This one intentionally is an invalid unlock.
	sceKernelDelayThread(1000);
	schedf("Unlocking...\n");
	result = sceKernelUnlockLwMutex(&workarea, 2);
	sceKernelDelayThread(5000);
	schedf("Unlocked 2? %08X\n", result);

	if (!deleteInstead)
	{
		sceKernelDelayThread(1000);
		schedf("Unlocking...\n");
		result = sceKernelUnlockLwMutex(&workarea, 1);
		sceKernelDelayThread(5000);
		schedf("Unlocked 1? %08X\n", result);
	}

	sceKernelDelayThread(1000);
	schedf("Delete: %08X\n", sceKernelDeleteLwMutex(&workarea));
	schedf("\n\n");
	flushschedf();
}

int main(int argc, char **argv) {
	execPriorityTests(0x000, 0);
	execPriorityTests(0x100, 0);
	execPriorityTests(0x000, 1);
	execPriorityTests(0x100, 1);
	return 0;
}
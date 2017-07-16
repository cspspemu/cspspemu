#include "shared.h"

SceUID sema;

static int threadFunction(int argSize, void* argPointer) {
	int num = argPointer ? *((int*)argPointer) : 0;
	printf("A%d\n", num);
	sceKernelWaitSemaCB(sema, 1, NULL);
	printf("B%d\n", num);

	return 0;
}

static void execPriorityTests(int attr) {
	SceUID threads[5];
	int test[5] = {1, 2, 3, 4, 5};

	sema = sceKernelCreateSema("sema1", attr, 0, 2, NULL);
	PRINT_SEMAPHORE(sema);

	int i;
	for (i = 0; i < 5; i++) {
		threads[i] = CREATE_PRIORITY_THREAD(threadFunction, 0x16 - i);
		sceKernelStartThread(threads[i], sizeof(int), (void*)&test[i]);
	}

	sceKernelDelayThread(10 * 1000);

	printf("---\n");
	PRINT_SEMAPHORE(sema);
	printf("---\n");
	sceKernelSignalSema(sema, 1);
	
	sceKernelDelayThread(10 * 1000);

	printf("---\n");
	PRINT_SEMAPHORE(sema);
	printf("---\n");

	sceKernelSignalSema(sema, 2);
	
	sceKernelDelayThread(10 * 1000);

	printf("---\n");
	PRINT_SEMAPHORE(sema);
	printf("---\n");

	sceKernelDeleteSema(sema);
	printf("\n\n");
}

int main(int argc, char **argv) {
	sema = sceKernelCreateSema("sema0", 0, 0, 2, NULL);
	// This is concerning: remove this line and the future PRINT_SEMAPHORE()s will print garbage?
	// Happens on a real PSP.
	PRINT_SEMAPHORE(sema);
	sceKernelDeleteSema(sema);

	execPriorityTests(0x000);
	execPriorityTests(0x100);
	return 0;
}
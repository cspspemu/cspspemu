#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

SceUID threads[5];
SceUID sema;
unsigned int test[5] = {0x0123, 0x4567, 0x89AB, 0xCDEF, 0x12345678};

static int threadFunction(int argSize, void* argPointer) {
	schedf("[1]:%d:%04X\n", argSize, argPointer ? *((unsigned int*)argPointer) : 0);
	sceKernelWaitSemaCB(sema, 1, NULL);

	schedf("[2]:%d:%04X\n", argSize, argPointer ? *((unsigned int*)argPointer) : 0);
	return 0;
}

static int threadFunction2(int argSize, void* argPointer) {
	*((unsigned int*)argPointer) = 3;
	return 0;
}

#define PRINT_SEMAPHORE(sema, info) schedf("Sema(Id=%d,Size=%d,Name='%s',Attr=%d,init=%d,cur=%d,max=%d,wait=%d)\n", (sema > 0) ? 1 : 0, info.size, info.name, info.attr, info.initCount, info.currentCount, info.maxCount, info.numWaitThreads);

int main(int argc, char **argv) {
	int result;
	int check_not_update_value = 7;
	SceKernelSemaInfo info;
	
	sema = sceKernelCreateSema("sema1", 0, 0, 2, NULL);
	
	sceKernelReferSemaStatus(sema, &info);
	PRINT_SEMAPHORE(sema, info);
	
	threads[0] = sceKernelCreateThread("Thread-0", (void *)&threadFunction, 0x12, 0x10000, 0, NULL);
	threads[1] = sceKernelCreateThread("Thread-1", (void *)&threadFunction, 0x12, 0x10000, 0, NULL);
	threads[2] = sceKernelCreateThread("Thread-2", (void *)&threadFunction, 0x12, 0x10000, 0, NULL);
	threads[3] = sceKernelCreateThread("Thread-3", (void *)&threadFunction, 0x12, 0x10000, 0, NULL);
	threads[4] = sceKernelCreateThread("Thread-4", (void *)&threadFunction2, 0x12, 0x10000, 0, NULL);
	
	schedf("VALUE-INVARIANT:%d\n", check_not_update_value);
	
	sceKernelStartThread(threads[0], 1, (void*)&test[1]);
	sceKernelStartThread(threads[1], 2, NULL);
	sceKernelStartThread(threads[2], 0, (void*)&test[0]);
	sceKernelStartThread(threads[3], sizeof(int), (void*)&test[4]);
	sceKernelStartThread(threads[4], sizeof(int), &check_not_update_value);

	sceKernelDelayThread(10 * 1000);
	
	schedf("---\n");
	sceKernelReferSemaStatus(sema, &info);
	PRINT_SEMAPHORE(sema, info);
	schedf("---\n");
	sceKernelSignalSema(sema, 1);
	
	sceKernelDelayThread(10 * 1000);

	schedf("---\n");
	sceKernelReferSemaStatus(sema, &info);
	PRINT_SEMAPHORE(sema, info);
	schedf("---\n");

	sceKernelSignalSema(sema, 1);
	
	sceKernelDelayThread(10 * 1000);

	schedf("---\n");
	sceKernelReferSemaStatus(sema, &info);
	PRINT_SEMAPHORE(sema, info);
	schedf("---\n");
	
	result = sceKernelDeleteSema(sema);
	schedf("%08X\n", result);
	result = sceKernelDeleteSema(sema);
	schedf("%08X\n", result);
	
	schedf("VALUE-INVARIANT:%d\n", check_not_update_value);
	flushschedf();
	
	return 0;
}
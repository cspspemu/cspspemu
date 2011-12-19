#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

SceUID threads[3];
SceUID sema;
int test[4] = {0x0123, 0x4567, 0x89AB, 0xCDEF};

static int threadFunction(int args, void* argp) {
	printf("[1]:%d:%04X\n", args, argp ? *((u32*)argp) : 0);
	sceKernelWaitSemaCB(sema, 1, NULL);
	printf("[2]:%d:%04X\n", args, argp ? *((u32*)argp) : 0);
	return 0;
}

#define PRINT_SEMAPHORE(sema, info) printf("Sema(Id=%d,Size=%d,Name='%s',Attr=%d,init=%d,cur=%d,max=%d,wait=%d)\n", sema, info.size, info.name, info.attr, info.initCount, info.currentCount, info.maxCount, info.numWaitThreads);

int main(int argc, char **argv) {
	int result;
	SceKernelSemaInfo info;

	pspDebugScreenInit();
	
	sema = sceKernelCreateSema("sema1", 0, 0, 2, NULL);
	
	sceKernelReferSemaStatus(sema, &info);
	PRINT_SEMAPHORE(sema, info);
	
	threads[0] = sceKernelCreateThread("Thread-0", (void *)&threadFunction, 0x12, 0x10000, 0, NULL);
	threads[1] = sceKernelCreateThread("Thread-1", (void *)&threadFunction, 0x12, 0x10000, 0, NULL);
	threads[2] = sceKernelCreateThread("Thread-2", (void *)&threadFunction, 0x12, 0x10000, 0, NULL);
	
	sceKernelStartThread(threads[0], 1, (void*)&test[1]);
	sceKernelStartThread(threads[1], 2, NULL);
	sceKernelStartThread(threads[2], 0, (void*)&test[0]);

	sceKernelDelayThread(10 * 1000);
	
	printf("---\n");
	sceKernelReferSemaStatus(sema, &info);
	PRINT_SEMAPHORE(sema, info);
	printf("---\n");
	
	sceKernelSignalSema(sema, 1);
	
	sceKernelDelayThread(10 * 1000);

	printf("---\n");
	sceKernelReferSemaStatus(sema, &info);
	PRINT_SEMAPHORE(sema, info);
	printf("---\n");

	sceKernelSignalSema(sema, 1);
	
	sceKernelDelayThread(10 * 1000);

	printf("---\n");
	sceKernelReferSemaStatus(sema, &info);
	PRINT_SEMAPHORE(sema, info);
	printf("---\n");
	
	result = sceKernelDeleteSema(sema);
	printf("%08X\n", result);
	result = sceKernelDeleteSema(sema);
	printf("%08X\n", result);
	
	return 0;
}
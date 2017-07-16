#include <common.h>

#include <pspkernel.h>
#include <stdio.h>
#include <string.h>
#include <math.h>

#define PSP_MUTEX_ATTR_FIFO 0
#define PSP_MUTEX_ATTR_PRIORITY 0x100
#define PSP_MUTEX_ATTR_ALLOW_RECURSIVE 0x200

int sceKernelCreateMutex(const char *name, uint attributes, int initial_count, void* options);
int sceKernelDeleteMutex(int mutexId);
int sceKernelLockMutex(int mutexId, int count, uint* timeout);
int sceKernelTryLockMutex(int mutexId, int count);
int sceKernelUnlockMutex(int mutexId, int count);

void simpleTest() {
	int mutexid = sceKernelCreateMutex("test", PSP_MUTEX_ATTR_FIFO, 0, 0);
	printf("sceKernelCreateMutex:0x%08X\n", mutexid < 0 ? mutexid : 1);
	{
		printf("sceKernelLockMutex:0x%08X\n", sceKernelLockMutex(mutexid, 1, NULL));
		printf("sceKernelUnlockMutex:0x%08X\n", sceKernelUnlockMutex(mutexid, 1));
		printf("sceKernelUnlockMutex:0x%08X\n", sceKernelUnlockMutex(mutexid, 1));
	}
	printf("sceKernelDeleteMutex:0x%08X\n", sceKernelDeleteMutex(mutexid));
	printf("sceKernelDeleteMutex:0x%08X\n", sceKernelDeleteMutex(mutexid));
}

void recursiveTest() {
	int mutexid = sceKernelCreateMutex("test", PSP_MUTEX_ATTR_FIFO | PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, 0);
	printf("sceKernelCreateMutex:0x%08X\n", mutexid < 0 ? mutexid : 1);
	{
		printf("[0]sceKernelLockMutex:0x%08X\n", sceKernelLockMutex(mutexid, 1, NULL));
		{
			printf("[1]sceKernelLockMutex:0x%08X\n", sceKernelLockMutex(mutexid, 1, NULL));
			{
				printf("[2]sceKernelLockMutex:0x%08X\n", sceKernelLockMutex(mutexid, 1, NULL));
				printf("[2]sceKernelUnlockMutex:0x%08X\n", sceKernelUnlockMutex(mutexid, 1));
			}
			printf("[1]sceKernelUnlockMutex:0x%08X\n", sceKernelUnlockMutex(mutexid, 1));
		}
		printf("[0]sceKernelUnlockMutex:0x%08X\n", sceKernelUnlockMutex(mutexid, 1));
	}
	printf("sceKernelDeleteMutex:0x%08X\n", sceKernelDeleteMutex(mutexid));
	printf("sceKernelDeleteMutex:0x%08X\n", sceKernelDeleteMutex(mutexid));
}

int mutexid;
int semaid;

static int multipleTestThread(int argsize, void* argdata) {
	int n = *(int *)argdata;
	printf("multipleTest_t%d:[0]\n", n);
	sceKernelWaitSema(semaid, 1, NULL);

	sceKernelLockMutex(mutexid, 1, NULL);
	{
		sceKernelLockMutex(mutexid, 1, NULL);
		{
			printf("multipleTest_t%d:[A]\n", n);
			{
				sceKernelDelayThread(3000);
			}
			printf("multipleTest_t%d:[B]\n", n);
		}
		sceKernelUnlockMutex(mutexid, 1);
	}
	sceKernelUnlockMutex(mutexid, 1);
	//printf("multipleTest_t%d:[3]\n", n);
	return 0;
}

#define N_THREADS 5
void multipleTest() {
	int threads[N_THREADS];
	int n;
	
	semaid = sceKernelCreateSema("sema1", 0, 0, 255, NULL);
	mutexid = sceKernelCreateMutex("mutex", PSP_MUTEX_ATTR_FIFO | PSP_MUTEX_ATTR_ALLOW_RECURSIVE, 0, 0);
	{
		for (n = 0; n < N_THREADS; n++) {
			sceKernelStartThread(threads[n] = sceKernelCreateThread("multipleTestThread", (void *)&multipleTestThread, 0x12, 0x10000, 0, NULL), sizeof(int), &n);
		}

		sceKernelSignalSema(semaid, N_THREADS);
		
		for (n = 0; n < N_THREADS; n++) {
			sceKernelWaitThreadEnd(threads[n], NULL);
		}
	}
	sceKernelDeleteMutex(mutexid);
}

int main(int argc, char *argv[]) {
	printf("simpleTest:\n"); simpleTest();
	printf("recursiveTest:\n"); recursiveTest();
	printf("multipleTest:\n"); multipleTest();
	return 0;
}

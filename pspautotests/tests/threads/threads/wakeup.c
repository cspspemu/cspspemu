/**
 * This feature is used in pspaudio library and probably in a lot of games.
 *
 * It checks also the correct behaviour of semaphores: sceKernelCreateSema, sceKernelSignalSema and sceKernelWaitSema.
 *
 * If new threads are not executed immediately, it would output: 2, 2, -1.
 * It's expected to output 0, 1, -1.
 */
#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

int sema;
int thid;

static int threadFunction1(int args, void* argp) {
	int n;
	for (n = 0; n < 5; n++) {
		printf("[1]:%d\n", n);
		sceKernelSignalSema(sema, 1);
		sceKernelSleepThreadCB();
	}
	
	printf("[1]\n");
	sceKernelSignalSema(sema, 1);
	
	return 0;
}

void testThreadsWakeup1() {
	int n;
	SceUInt timeout = 20 * 1000; // 20ms
	
	printf("------------------------\n");

	sema = sceKernelCreateSema("sema1", 0, 0, 1, NULL);
	sceKernelStartThread(thid = sceKernelCreateThread("Test Thread", (void *)&threadFunction1, 0x12, 0x10000, 0, NULL), 0, NULL);
	
	for (n = 0; n < 5; n++) {
		sceKernelWaitSemaCB(sema, 1, &timeout);
		printf("[0]:%d\n", n);
		sceKernelDelayThread(10 * 1000); // 10ms
		sceKernelWakeupThread(thid);
	}
	
	sceKernelWaitSemaCB(sema, 1, &timeout);
	sceKernelTerminateDeleteThread(thid);
	sceKernelDeleteSema(sema);
	
	printf("[0]\n");
}

static int threadFunction2(int args, void* argp) {
	sceKernelWaitSemaCB(sema, 1, NULL);

	printf("[1]\n");
	sceKernelSleepThreadCB();
	printf("[2]\n");
	sceKernelSleepThreadCB();
	printf("[3]\n");
	sceKernelSleepThreadCB();
	
	sceKernelDelayThread(10 * 1000);
	
	printf("[6]\n");
	
	sceKernelExitThread(777);
	
	printf("[NO]\n");
	
	return 888;
}

void testThreadsWakeup2() {
	printf("------------------------\n");

	sema = sceKernelCreateSema("sema1", 0, 0, 1, NULL);
	thid = sceKernelCreateThread("Test Thread", (void *)&threadFunction2, 0x12, 0x10000, 0, NULL);

	sceKernelStartThread(thid, 0, NULL);

	sceKernelWakeupThread(thid);
	sceKernelWakeupThread(thid);

	printf("[0]\n");
	sceKernelSignalSema(sema, 1);
	
	sceKernelDelayThread(10 * 1000);
	
	printf("[4]\n");
	
	sceKernelWakeupThread(thid);
	
	printf("[5]\n");
	
	sceKernelWaitThreadEndCB(thid, NULL);
	
	printf("[7]\n");
	
	printf("%d\n", sceKernelGetThreadExitStatus(thid));
	
	sceKernelDeleteThread(thid);
	sceKernelDeleteSema(sema);
}

/*
static int threadFunction3(int args, void* argp) {
	printf("[1]\n");
	sceKernelSleepThreadCB();
	printf("[2]\n");
	sceKernelSleepThreadCB();
	printf("[3]\n");
	sceKernelSleepThreadCB();
	
	sceKernelDelayThread(10 * 1000);
	
	printf("[5]\n");
	
	return 777;
}

void testThreadsWakeup3() {
	printf("------------------------\n");

	thid = sceKernelCreateThread("Test Thread", (void *)&threadFunction3, 0x12, 0x10000, 0, NULL);

	sceKernelWakeupThread(thid);
	sceKernelWakeupThread(thid);

	sceKernelDelayThread(1 * 1000);
	printf("[0]\n");
	
	sceKernelStartThread(thid, 0, NULL);
	
	sceKernelDelayThread(10 * 1000);
	
	printf("[4]\n");
	
	sceKernelWakeupThread(thid);
	
	sceKernelWaitThreadEndCB(thid, NULL);
	
	printf("%d\n", sceKernelGetThreadExitStatus(thid));
	
	sceKernelDeleteThread(thid);
}
*/

// test3 should check if sceKernelWakeupThread could be called before sceKernelStartThread.

int main(int argc, char **argv) {
	testThreadsWakeup1();
	testThreadsWakeup2();
	
	return 0;
}
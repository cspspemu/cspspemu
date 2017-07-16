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

static int semaphore = 0;

static int threadFunction(int args, void* argp) {
	int local_value = *(int *)argp;

	printf("%d, %d\n", args, local_value);

	sceKernelSignalSema(semaphore, 1);
	
	return 0;
}

void testThreads() {
	int result;
	int n;
	SceUID threads[3];

	// Create a semaphore for waiting both threads to execute.
	semaphore = sceKernelCreateSema("Semaphore", 0, 0, 2, NULL);
	
	for (n = 0; n < 2; n++) {
		// Create and start a new thread passing a stack local variable as parameter.
		// When sceKernelStartThread, thread is executed immediately, so in a while it has access
		// to the unmodified stack of the thread that created this one and can access n,
		// before it changes its value.
		threads[n] = sceKernelCreateThread("Test Thread", (void *)&threadFunction, 0x12, 0x10000, 0, NULL);
		sceKernelStartThread(
			threads[n],
			sizeof(n), &n
		);
	}

	// Wait until semaphore have been signaled two times (both new threads have been executed).
	sceKernelWaitSema(semaphore, 2, NULL);

	// After both threads have been executed, we will emit a -1 to check that semaphores work fine.
	printf("%d\n", -1);

	for (n = 0; n < 2; n++) {
		sceKernelDeleteThread(threads[n]);
	}

	sceKernelDeleteSema(semaphore);
}

static volatile int threadFlag[4];

static int threadEndedFunction1(int args, void* argp) {
	threadFlag[0] = 1;
	return 0;
}

static int threadEndedFunction2(int args, void* argp) {
	threadFlag[1] = 1;
	sceKernelExitThread(0);
	return 0;
}

static int threadEndedFunction3(int args, void* argp) {
	threadFlag[2] = 1;
	sceKernelDelayThread(10 * 1000);
	printf("Thread3.GoingToEnd\n");
	sceKernelExitThread(0);
	return 0;
}

void testThreadsEnded() {
	int thread1, thread2, thread3, thread4;
	
	// Thread1 will stop returning the function and sceKernelWaitThreadEnd will be executed after the thread have ended.
	thread1 = sceKernelCreateThread("threadEndedFunction1", (void *)&threadEndedFunction1, 0x12, 0x10000, 0, NULL);

	// Thread1 will stop with sceKernelExitThread and sceKernelWaitThreadEnd will be executed after the thread have ended.
	thread2 = sceKernelCreateThread("threadEndedFunction2", (void *)&threadEndedFunction2, 0x12, 0x10000, 0, NULL);

	// Thread3 will stop after a while so it will allow to execute sceKernelWaitThreadEnd before it ends.
	thread3 = sceKernelCreateThread("threadEndedFunction3", (void *)&threadEndedFunction3, 0x12, 0x10000, 0, NULL);
	
	// Thread4 won't start never, so sceKernelWaitThreadEnd can be executed before thread is started.
	thread4 = sceKernelCreateThread("threadEndedFunction4", NULL, 0x12, 0x10000, 0, NULL);

	int i;
	for (i = 0; i < 4; i++)
		threadFlag[i] = 0;

	sceKernelStartThread(thread1, 0, NULL);
	sceKernelStartThread(thread2, 0, NULL);
	sceKernelStartThread(thread3, 0, NULL);
	
	// This waits 5ms and supposes both threads (1 and 2) have ended. Thread 3 should have not ended. Thread 4 is not going to be started.
	sceKernelDelayThread(2 * 1000);

	printf("Threads.EndedExpected - start status: ");
	for (i = 0; i < 4; i++)
		printf("%d", threadFlag[i]);
	printf("\n");
	
	sceKernelWaitThreadEnd(thread1, NULL);
	printf("Thread1.Ended\n");
	sceKernelWaitThreadEnd(thread2, NULL);
	printf("Thread2.Ended\n");
	sceKernelWaitThreadEnd(thread3, NULL);
	printf("Thread3.Ended\n");
	sceKernelWaitThreadEnd(thread4, NULL);
	printf("Thread4.NotStartedSoEnded\n");
}

int main(int argc, char **argv) {
	testThreads();
	testThreadsEnded();
	
	return 0;
}
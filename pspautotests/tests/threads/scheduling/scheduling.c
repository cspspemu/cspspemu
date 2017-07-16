#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspthreadman.h>
#include <psploadexec.h>

SceUID threads[4];
SceUID sema;

int testSimpleScheduling_Thread(SceSize argc, void *argp) {
	unsigned int *args = (unsigned int *)argp;
	unsigned int thread_n = args[0];
	unsigned int delay = args[1];
	int count = 4;
	while (count--) {
		checkpoint("  * %08X @%d", thread_n, count);
		sceKernelDelayThread(delay);
	}
	sceKernelSignalSema(sema, 1);
	
	return 0;
}

void testSimpleScheduling(int delay) {
	unsigned int n;
	
	char temp[256];
	snprintf(temp, sizeof(temp), "testSimpleScheduling (%d):", delay);
	checkpointNext(temp);
	
	sema = sceKernelCreateSema("EndSema", 0, 0, 4, NULL);
	
	unsigned int args[2];
	for (n = 0; n < 3; n++) {
		threads[n] = sceKernelCreateThread("Thread-N", testSimpleScheduling_Thread, 0x18, 0x10000, 0, NULL);
		args[0] = n;
		args[1] = delay;
		sceKernelStartThread(threads[n], sizeof(args), args);
		checkpoint("  Started thread %d", n);
	}
	
	checkpoint("  Waiting");
	sceKernelWaitSemaCB(sema, 3, NULL);
	checkpoint("  Finished");

	for (n = 0; n < 3; n++) {
		sceKernelDeleteThread(threads[n]);
	}

	sceKernelDeleteSema(sema);
}

int testSimpleVblankScheduling_Thread(SceSize args, void *argp) {
	unsigned int thread_n = *(unsigned int *)argp;
	int count = 4;
	while (count--) {
		checkpoint("  %08X @%d", thread_n, count);
		sceDisplayWaitVblankStart();
	}
	sceKernelSignalSema(sema, 1);
	
	return 0;
}


void testSimpleVblankScheduling() {
	unsigned int n;
	
	checkpointNext("testSimpleVblankScheduling:");
	
	sema = sceKernelCreateSema("EndSema", 0, 0, 4, NULL);
	
	for (n = 0; n < 3; n++) {
		threads[n] = sceKernelCreateThread("Thread-N", testSimpleVblankScheduling_Thread, 0x18, 0x10000, 0, NULL);
		sceKernelStartThread(threads[n], 1, &n);
	}

	checkpoint("  Waiting");
	sceKernelWaitSemaCB(sema, 3, NULL);
	checkpoint("  Finished");
	
	for (n = 0; n < 3; n++) {
		sceKernelDeleteThread(threads[n]);
	}

	sceKernelDeleteSema(sema);
}

char buffer[10000];
char *msg;

int testNoThreadSwitchingWhenSuspendedInterrupts_sleepingThread(SceSize args, void *argp) {
	checkpoint("  Sleeping Thread: going to sleep");
	sceKernelSleepThread();
	checkpoint("  Sleeping Thread: that was a nice nap");
	return 0;
}

// http://code.google.com/p/jpcsp/source/detail?r=2253
void testNoThreadSwitchingWhenSuspendedInterrupts() {
	msg = buffer;
	strcpy(msg, "");
	
	checkpointNext("testNoThreadSwitchingWhenSuspendedInterrupts:");
	
	SceUID sleepingThid = sceKernelCreateThread(
		"Sleeping Thread",
		testNoThreadSwitchingWhenSuspendedInterrupts_sleepingThread,
		0x10,
		0x1000,
		0,
		0
	);
	sceKernelStartThread(sleepingThid, 0, 0);
	sceKernelDelayThread(100000);
	int intr = sceKernelCpuSuspendIntr();
	checkpoint("  sceKernelWakeupThread: %08x", sceKernelWakeupThread(sleepingThid));
	checkpoint("  Main Thread with disabled interrupts");
	sceKernelCpuResumeIntr(intr);
	checkpoint("  Main Thread with enabled interrupts");
	schedf("%s", msg);

	sceKernelDeleteThread(sleepingThid);
}

int main(int argc, char **argv) {
	testNoThreadSwitchingWhenSuspendedInterrupts();
	testSimpleScheduling(1);
	testSimpleScheduling(500);
	testSimpleVblankScheduling();

	return 0;
}


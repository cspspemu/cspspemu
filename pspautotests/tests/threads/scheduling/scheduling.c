#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspthreadman.h>
#include <psploadexec.h>

SceUID threads[4];
SceUID sema;

int testSimpleScheduling_Thread(SceSize args, void *argp) {
	unsigned int thread_n = *(unsigned int *)argp;
	int count = 4;
	while (count--) {
		printf("%08X\n", thread_n);
		sceKernelDelayThread(1);
	}
	sceKernelSignalSema(sema, 1);
	
	return 0;
}

void testSimpleScheduling() {
	unsigned int n;
	
	printf("testSimpleScheduling:\n");
	
	sema = sceKernelCreateSema("EndSema", 0, 0, 4, NULL);
	
	for (n = 0; n < 4; n++) {
		threads[n] = sceKernelCreateThread("Thread-N", testSimpleScheduling_Thread, 0x18, 0x10000, 0, NULL);
		sceKernelStartThread(threads[n], 1, &n);
	}
	
	sceKernelWaitSemaCB(sema, 4, NULL);

	for (n = 0; n < 4; n++) {
		sceKernelDeleteThread(threads[n]);
	}
}

int testSimpleVblankScheduling_Thread(SceSize args, void *argp) {
	unsigned int thread_n = *(unsigned int *)argp;
	int count = 4;
	while (count--) {
		printf("%08X\n", thread_n);
		sceDisplayWaitVblankStart();
	}
	sceKernelSignalSema(sema, 1);
	
	return 0;
}


void testSimpleVblankScheduling() {
	unsigned int n;
	
	printf("testSimpleVblankScheduling:\n");
	
	sema = sceKernelCreateSema("EndSema", 0, 0, 4, NULL);
	
	for (n = 0; n < 4; n++) {
		threads[n] = sceKernelCreateThread("Thread-N", testSimpleVblankScheduling_Thread, 0x18, 0x10000, 0, NULL);
		sceKernelStartThread(threads[n], 1, &n);
	}
	
	sceKernelWaitSemaCB(sema, 4, NULL);
	
	for (n = 0; n < 4; n++) {
		sceKernelDeleteThread(threads[n]);
	}
}

char buffer[10000];
char *msg;

int testNoThreadSwitchingWhenSuspendedInterrupts_sleepingThread(SceSize args, void *argp) {
	sceKernelSleepThread();
	strcat(msg, "Sleeping Thread\n");
	return 0;
}

// http://code.google.com/p/jpcsp/source/detail?r=2253
void testNoThreadSwitchingWhenSuspendedInterrupts() {
	msg = buffer;
	strcpy(msg, "");
	
	printf("testNoThreadSwitchingWhenSuspendedInterrupts:\n");
	
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
	sceKernelWakeupThread(sleepingThid);
	strcat(msg, "Main Thread with disabled interrupts\n");
	sceKernelCpuResumeIntr(intr);
	strcat(msg, "Main Thread with enabled interrupts\n");
	printf("%s", msg);
}

int main(int argc, char **argv) {
	testNoThreadSwitchingWhenSuspendedInterrupts();
	testSimpleScheduling();
	testSimpleVblankScheduling();

	return 0;
}


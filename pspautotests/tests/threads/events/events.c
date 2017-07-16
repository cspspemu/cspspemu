#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

int result;
SceUID evid;

void testEvents_thread1(int args, void* argp) {
	printf("[1]\n");
	sceKernelDelayThread(1000);
	printf("[2]\n");
	result = sceKernelSetEventFlag(evid, 4);
	//sceKernelDelayThread(1000);
	printf("[3]\n");
}

void testEvents() {
	SceUInt timeout = 1000;
	u32 outBits = -1;
	evid = sceKernelCreateEventFlag("test_event", PSP_EVENT_WAITMULTIPLE, 4 | 2, NULL);

	outBits = -1;
	result = sceKernelPollEventFlag(evid, 4 | 2, PSP_EVENT_WAITAND, &outBits);
	printf("event: %08X:%d\n", (uint)result, (int)outBits);

	outBits = -1;
	result = sceKernelPollEventFlag(evid, 8 | 2, PSP_EVENT_WAITAND, &outBits);
	printf("event: %08X:%d\n", (uint)result, (int)outBits);

	outBits = -1;
	result = sceKernelPollEventFlag(evid, 8 | 4, PSP_EVENT_WAITOR, &outBits);
	printf("event: %08X:%d\n", (uint)result, (int)outBits);
	result = sceKernelSetEventFlag(evid, 32 | 16);
	printf("event: %08X\n", (uint)result);
	result = sceKernelClearEventFlag(evid, ~4);
	printf("event: %08X\n", (uint)result);

	outBits = -1;
	result = sceKernelPollEventFlag(evid, 0xFFFFFFFC, PSP_EVENT_WAITOR, &outBits);
	printf("event: %08X:%d\n", (uint)result, (int)outBits);

	outBits = -1;
	result = sceKernelPollEventFlag(evid + 100, 8 | 2, PSP_EVENT_WAITAND, &outBits);
	printf("event: %08X:%d\n", (uint)result, (int)outBits);

	sceKernelStartThread(
		sceKernelCreateThread("Test Thread", (void *)&testEvents_thread1, 0x12, 0x10000, 0, NULL),
		0, NULL
	);

	outBits = -1;
	result = sceKernelWaitEventFlagCB(evid, 4, PSP_EVENT_WAITAND, &outBits, NULL);
	printf("event: %08X:%d\n", (uint)result, (int)outBits);
	
	result = sceKernelClearEventFlag(evid, ~(2 | 8));

	outBits = -1;
	result = sceKernelWaitEventFlagCB(evid, 2 | 8, PSP_EVENT_WAITAND, &outBits, &timeout);
	printf("event: %08X:%d\n", (uint)result, (int)outBits);

	result = sceKernelDeleteEventFlag(evid);
	printf("event: %08X\n", (uint)result);
	
	outBits = -1;
	result = sceKernelPollEventFlag(evid, 8 | 2, PSP_EVENT_WAITAND, &outBits);
	printf("event: %08X:%d\n", (uint)result, (int)outBits);
	
	// Test PSP_EVENT_WAITCLEARALL
	// Test PSP_EVENT_WAITCLEAR
	// Test callback handling
}

int main(int argc, char **argv) {
	testEvents();
	
	return 0;
}
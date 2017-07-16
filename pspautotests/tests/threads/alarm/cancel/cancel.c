#include "../sub_shared.h"
#include <limits.h>

volatile int alarmHandlerHits = 0;

void testCancel(const char *title, SceUID alarm) {
	int result = sceKernelCancelAlarm(alarm);
	printf("%s: %08X\n", title, result);
}

static SceUInt alarmHandler(void *common) {
	alarmHandlerHits++;
	if (common != NULL) {
		return *(int *) common;
	} else {
		return 0;
	}
}

int main(int argc, char **argv) {
	SceUID alarm = sceKernelSetAlarm(1000, alarmHandler, NULL);

	testCancel("Normal", alarm);
	testCancel("Deleted", alarm);
	testCancel("Invalid", 0xDEADBEEF);
	testCancel("NULL", 0);

	alarm = sceKernelSetAlarm(10000, alarmHandler, NULL);
	BASIC_SCHED_TEST("Normal",
		result = sceKernelCancelAlarm(alarm);
	);
	BASIC_SCHED_TEST("Deleted",
		result = sceKernelCancelAlarm(alarm);
	);

	alarm = sceKernelSetAlarm(100, alarmHandler, NULL);
	sceKernelDelayThread(1000);
	testCancel("Already ran", alarm);

	return 0;
}
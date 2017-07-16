#include "../sub_shared.h"
#include <limits.h>

volatile int alarmHandlerHits = 0;

inline void schedfRefer(const char *title, SceUID alarm, SceKernelAlarmInfo *info, SceSize size) {
	if (info != NULL && info != (SceKernelAlarmInfo *) 0xDEADBEEF) {
		memset(info, 0, sizeof(SceKernelAlarmInfo));
		info->size = size;
	}

	int result = sceKernelReferAlarmStatus(alarm, info);
	if (result == 0) {
		schedf("%s: OK (size=%d,schedule=%d,handler=%d,common=%d)\n", title, info->size, (*(u64 *) &info->schedule) > 0, info->handler != NULL, info->common != NULL);
	} else {
		schedf("%s: Invalid (%08X)\n", title, result);
	}
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
	SceKernelAlarmInfo info;

	schedfRefer("Normal", alarm, &info, sizeof(SceKernelAlarmInfo));
	// Crashes.
	//schedfRefer("NULL info", alarm, NULL, sizeof(SceKernelAlarmInfo));
	//schedfRefer("Invalid info", alarm, (SceKernelAlarmInfo *) 0xDEADBEEF, sizeof(SceKernelAlarmInfo));

	int sizes[] = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21};
	int i, n;
	for (i = 0, n = sizeof(sizes) / sizeof(sizes[0]); i < n; ++i) {
		char title[32];
		sprintf(title, "Size %d", i);
		schedfRefer(title, alarm, &info, i);
	}

	sceKernelCancelAlarm(alarm);
	flushschedf();

	schedfRefer("Deleted", alarm, &info, sizeof(SceKernelAlarmInfo));
	schedfRefer("Invalid", 0xDEADBEEF, &info, sizeof(SceKernelAlarmInfo));
	schedfRefer("NULL", 0, &info, sizeof(SceKernelAlarmInfo));
	flushschedf();

	alarm = sceKernelSetAlarm(10000, alarmHandler, NULL);
	BASIC_SCHED_TEST("Normal",
		result = sceKernelReferAlarmStatus(alarm, &info);
	);
	sceKernelCancelAlarm(alarm);

	BASIC_SCHED_TEST("Deleted",
		result = sceKernelReferAlarmStatus(alarm, &info);
	);

	alarm = sceKernelSetAlarm(100, alarmHandler, NULL);
	sceKernelDelayThread(1000);
	schedfRefer("Already ran", alarm, &info, sizeof(SceKernelAlarmInfo));

	return 0;
}
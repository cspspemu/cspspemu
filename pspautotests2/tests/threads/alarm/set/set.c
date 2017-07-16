#include "../sub_shared.h"
#include <limits.h>

volatile int alarmHandlerHits = 0;

void testSetAlarm(const char *title, SceUInt clock, SceKernelAlarmHandler handler, void *common) {
	s64 currentTime = sceKernelGetSystemTimeWide();

	alarmHandlerHits = 0;
	SceUID alarm = sceKernelSetAlarm(clock, handler, common);
	int hits = alarmHandlerHits;

	schedfAlarm(alarm, currentTime);
	if (alarm > 0) {
		sceKernelCancelAlarm(alarm);
	}

	printf("%s (hits=%d): ", title, hits);
	flushschedf();
}

void testSetClockAlarm(const char *title, u64 clock, SceKernelAlarmHandler handler, void *common) {
	SceKernelSysClock sysclock;
	*(u64 *) &sysclock = clock;
	
	s64 currentTime = sceKernelGetSystemTimeWide();

	alarmHandlerHits = 0;
	SceUID alarm = sceKernelSetSysClockAlarm(&sysclock, handler, common);
	int hits = alarmHandlerHits;

	schedfAlarm(alarm, currentTime);
	if (alarm > 0) {
		sceKernelCancelAlarm(alarm);
	}

	printf("%s (hits=%d): ", title, hits);
	flushschedf();
}

static SceUInt alarmHandler(void *common) {
	alarmHandlerHits++;
	if (common != NULL) {
		return *(int *) common;
	} else {
		return 0;
	}
}

void checkAlarmFrequency() {
	s64 currentTime = sceKernelGetSystemTimeWide();
	int interval = 1000;

	alarmHandlerHits = 0;
	SceUID alarm = sceKernelSetAlarm(1000, alarmHandler, &interval);
	sceKernelDelayThread(1000 * 10 + 200);
	int hits = alarmHandlerHits;

	schedfAlarm(alarm, currentTime);
	if (alarm > 0) {
		sceKernelCancelAlarm(alarm);
	}

	printf("Frequency: %d hits, ", hits);
	flushschedf();
}

void checkAlarmSysFrequency() {
	s64 currentTime = sceKernelGetSystemTimeWide();
	int interval = 1000;
	u64 clocks = 1000;

	alarmHandlerHits = 0;
	SceUID alarm = sceKernelSetSysClockAlarm((SceKernelSysClock *) &clocks, alarmHandler, &interval);
	sceKernelDelayThread(1000 * 10 + 200);
	int hits = alarmHandlerHits;

	schedfAlarm(alarm, currentTime);
	if (alarm > 0) {
		sceKernelCancelAlarm(alarm);
	}

	printf("SysClock frequency: %d hits, ", hits);
	flushschedf();
}

void checkMaxAlarms() {
	SceUID alarms[1024];
	int i, j;
	for (i = 0; i < 1024; i++) {
		alarms[i] = sceKernelSetAlarm(100000 + i, alarmHandler, NULL);
		if (alarms[i] < 0) {
			schedf("Failed at %d: %08X\n", i, alarms[i]);
			break;
		}
	}

	for (j = 0; j < i; j++) {
		sceKernelCancelAlarm(alarms[j]);
	}

	if (i == 1024) {
		schedf("Created 1024 alarms: OK\n");
	}
	flushschedf();
}

int main(int argc, char **argv) {
	testSetAlarm("Zero timer", 0, alarmHandler, NULL);
	testSetAlarm("100 timer", 100, alarmHandler, NULL);
	testSetAlarm("INT_MAX timer", INT_MAX, alarmHandler, NULL);
	testSetAlarm("NULL handler", 100, NULL, NULL);

	testSetClockAlarm("Zero sys timer", 0, alarmHandler, NULL);
	testSetClockAlarm("100 sys timer", 100, alarmHandler, NULL);
	testSetClockAlarm("INT_MAX sys timer", INT_MAX, alarmHandler, NULL);
	testSetClockAlarm("NULL handler sys", 100, NULL, NULL);

	// Crashes.
	//int result = sceKernelSetSysClockAlarm(NULL, alarmHandler, NULL);
	//printf("sceKernelSetSysClockAlarm NULL clocks: %08X\n", result);

	checkAlarmFrequency();
	checkAlarmSysFrequency();
	checkMaxAlarms();

	SceUID alarm;
	BASIC_SCHED_TEST("Normal",
		alarm = sceKernelSetAlarm(100000, &alarmHandler, NULL);
		result = alarm > 0 ? 1 : 0;
	);
	sceKernelCancelAlarm(alarm);
	BASIC_SCHED_TEST("NULL handler",
		result = sceKernelSetAlarm(100000, NULL, NULL);
	);

	return 0;
}
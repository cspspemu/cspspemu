#include <common.h>

#include "rtc_common.h"

/**
* Check that getCurrentTick works fine.
*
* @TODO: Currently sceKernelDelayThread only work with ms.
*        It should check that work with microseconds precission.
*/

void checkGetCurrentTick() {
	u64 tick0, tick1;
	int microseconds = 2 * 1000; // 2ms

	printf("Checking sceRtcGetCurrentTick\n");

	sceRtcGetCurrentTick(&tick0);
	{
		sceKernelDelayThread(microseconds);
	}
	sceRtcGetCurrentTick(&tick1);

	printf("%d\n", (tick1 - tick0) >= microseconds);
}

void checkGetCurrentClock() {
	printf("Checking sceRtcGetCurrentClock\n");

	pspTime pt_baseline;
	pspTime pt;

	do
	{
		sceRtcGetCurrentClock(&pt_baseline, 0);
		sceRtcGetCurrentClock(&pt, -60);
	}
	// Rollover is annoying.  We could test in a more complicated way, I guess.
	while (pt_baseline.minutes == 59 && pt_baseline.seconds == 59);

	if (pt.hour != pt_baseline.hour - 1 && !(pt.hour == 12 && pt_baseline.hour == 1))
		printf("-60 TZ: Failed, got time different by %d hours.\n", (pt_baseline.hour - pt.hour) % 12);
	else
		printf("-60 TZ: OK\n");

	checkPspTime(pt);

	// Crash.
	//printf("NULL, 0 TZ: %08x\n", sceRtcGetCurrentClock(NULL, 0));
	printf("0 TZ: %08x\n", sceRtcGetCurrentClock(&pt, 0));
	printf("+13 TZ: %08x\n", sceRtcGetCurrentClock(&pt, 13));
	printf("+60 TZ: %08x\n", sceRtcGetCurrentClock(&pt, 60));
	printf("-60 TZ: %08x\n", sceRtcGetCurrentClock(&pt, -60));
	printf("-600000 TZ: %08x\n", sceRtcGetCurrentClock(&pt, -600000));
	printf("INT_MAX TZ: %08x\n", sceRtcGetCurrentClock(&pt, INT_MAX));
	printf("-INT_MAX TZ: %08x\n", sceRtcGetCurrentClock(&pt, -INT_MAX));
}

void checkGetCurrentClockLocalTime() {
	printf("Checking sceRtcGetCurrentClockLocalTime\n");
	pspTime pt;

	// Crash.
	//printf("NULL: %08x\n", sceRtcGetCurrentClockLocalTime(NULL));
	// Not much to test here...
	printf("Normal: %08x\n", sceRtcGetCurrentClockLocalTime(&pt));

	checkPspTime(pt);
}

int main(int argc, char **argv) {
	checkGetCurrentTick();
	checkGetCurrentClock();
	checkGetCurrentClockLocalTime();
	return 0;
}

#include <common.h>

#include <pspkernel.h>
#include <psprtc.h>

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
	/*sceRtcGetCurrentClock */
}

void checkDaysInMonth() {
	printf("Checking sceRtcGetDaysInMonth\n");
	printf("sceRtcGetDaysInMonth:2010, 4\n");
	printf("%d\n", sceRtcGetDaysInMonth(2010, 4));
}

void checkDayOfWeek() {
	printf("Checking sceRtcGetDayOfWeek\n");
	printf("sceRtcGetDayOfWeek:2010, 4, 27\n");
	printf("%d\n", sceRtcGetDayOfWeek(2010, 4, 27));
}

int main(int argc, char **argv) {
	checkGetCurrentTick();
	checkDaysInMonth();
	checkDayOfWeek();
	checkGetCurrentClock();
	
	return 0;
}

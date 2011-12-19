#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

SceUID vtimer;

void testVTimerGetSimple() {
	SceUID vtimer1;

	vtimer1 = sceKernelCreateVTimer("VTIMER1", NULL);
	
	sceKernelDelayThread(10000);
	
	printf("%lld\n", sceKernelGetVTimerTimeWide(vtimer1) / 10000);

	sceKernelStartVTimer(vtimer1);

	sceKernelDelayThread(10000);
	printf("%lld\n", sceKernelGetVTimerTimeWide(vtimer1) / 10000);
	sceKernelStopVTimer(vtimer1);

	sceKernelStartVTimer(vtimer1);

	sceKernelDelayThread(10000);
	printf("%lld\n", sceKernelGetVTimerTimeWide(vtimer1) / 10000);
	sceKernelStopVTimer(vtimer1);

	printf("%lld\n", sceKernelGetVTimerTimeWide(vtimer1) / 10000);
	
	sceKernelCancelVTimerHandler(vtimer1);
}

SceUInt testVTimerHandler_TimerHandler(SceUID uid, SceKernelSysClock *elapsedScheduled, SceKernelSysClock *elapsedReal, void *common) {
	printf("%d: %08X%08X, %08X\n", (vtimer == uid), elapsedScheduled->hi, elapsedScheduled->low, (unsigned int)common);

	return 0;
}

void testVTimerHandler() {
	SceKernelSysClock time;
	
	vtimer = sceKernelCreateVTimer("VTIMER", NULL);
	
	sceKernelStartVTimer(vtimer);
	
	time.hi = 0;
	time.low = 1000;

	sceKernelSetVTimerHandler(vtimer, &time, testVTimerHandler_TimerHandler, NULL);
	
	sceKernelDelayThread(2000);
	
	sceKernelStopVTimer(vtimer);
	
	sceKernelCancelVTimerHandler(vtimer);
}

int main(int argc, char **argv) {
	testVTimerGetSimple();
	//testVTimerGetStatus();
	testVTimerHandler();

	return 0;
}
#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

SceUID vtimer;

void testVTimerGetSimple() {
	SceUID vtimer1;

	vtimer1 = sceKernelCreateVTimer("VTIMER1", NULL);
	printf("sceKernelCreateVTimer:%08X\n", vtimer1 > 0 ? 1 : vtimer1);
	
	sceKernelDelayThread(10000);
	
	printf("sceKernelGetVTimerTimeWide: %lld\n", sceKernelGetVTimerTimeWide(vtimer1) / 10000);

	printf("sceKernelStartVTimer:0x%08X\n", sceKernelStartVTimer(vtimer1));

	sceKernelDelayThread(10000);
	printf("sceKernelGetVTimerTimeWide: %lld\n", sceKernelGetVTimerTimeWide(vtimer1) / 10000);
	printf("sceKernelStopVTimer:0x%08X\n", sceKernelStopVTimer(vtimer1));

	printf("sceKernelStartVTimer:0x%08X\n", sceKernelStartVTimer(vtimer1));

	sceKernelDelayThread(10000);
	printf("sceKernelGetVTimerTimeWide: %lld\n", sceKernelGetVTimerTimeWide(vtimer1) / 10000);
	printf("sceKernelStopVTimer:0x%08X\n", sceKernelStopVTimer(vtimer1));
	printf("sceKernelStopVTimer:0x%08X\n", sceKernelStopVTimer(vtimer1));

	printf("sceKernelGetVTimerTimeWide: %lld\n", sceKernelGetVTimerTimeWide(vtimer1) / 10000);
	
	printf("sceKernelCancelVTimerHandler:0x%08X\n", sceKernelCancelVTimerHandler(vtimer1));
	printf("sceKernelDeleteVTimer:0x%08X\n", sceKernelDeleteVTimer(vtimer1));
}

char msg[4096];

SceUInt testVTimerHandler_TimerHandler(SceUID uid, SceKernelSysClock *elapsedScheduled, SceKernelSysClock *elapsedReal, void *common) {
	// Crash if printf (write to file) within timer.
	sprintf(msg,
		"testVTimerHandler_TimerHandler: %d: %08X%08X, %08X%08X, %08X\n",
		(vtimer == uid) ? 1 : 0,
		(uint)elapsedScheduled->hi, (uint)elapsedScheduled->low,
		(uint)elapsedReal->hi, (uint)elapsedReal->low,
		(uint)common
	);

	return 0;
}

void testVTimerHandler() {
	SceKernelSysClock time;

	vtimer = sceKernelCreateVTimer("VTIMER", NULL);
	printf("sceKernelCreateVTimer:%08X\n", vtimer > 0 ? 1 : vtimer);
	
	printf("sceKernelStartVTimer:%08X\n", sceKernelStartVTimer(vtimer));
	
	time.hi = 0;
	time.low = 1000;

	printf("sceKernelSetVTimerHandler:0x%08X\n", sceKernelSetVTimerHandler(vtimer, &time, &testVTimerHandler_TimerHandler, NULL));
	
	sceKernelDelayThread(5000);
	
	printf("sceKernelStopVTimer:0x%08X\n", sceKernelStopVTimer(vtimer));
	
	printf("sceKernelCancelVTimerHandler:0x%08X\n", sceKernelCancelVTimerHandler(vtimer));
	printf("%s", msg);
}

int main(int argc, char **argv) {
	printf("Start\n");
	{
		testVTimerGetSimple();
		//testVTimerGetStatus();
		testVTimerHandler();
	}
	printf("End\n");

	return 0;
}

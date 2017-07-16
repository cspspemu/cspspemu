#include <common.h>

#include <pspkernel.h>
#include <pspctrl.h>
#include <psprtc.h>

char schedulingLog[65536];
char *schedulingLogPos;

volatile int didPreempt = 0;

void schedf(const char *format, ...) {
	va_list args;
	va_start(args, format);
	schedulingLogPos += vsprintf(schedulingLogPos, format, args);
	// This is easier to debug in the emulator, but printf() reschedules on the real PSP.
	//vprintf(format, args);
	va_end(args);
}

int testThread(SceSize argc, void* argv)
{
	while (1)
	{
		sceKernelDelayThread(3);
		didPreempt = 1;
	}
	return 0;
}

int main(int argc, char *argv[]) {
	int result, rest, back;

	schedulingLogPos = schedulingLog;

	SceUID thread = sceKernelCreateThread("preempt", &testThread, sceKernelGetThreadCurrentPriority() - 1, 0x500, 0, NULL);
	sceKernelStartThread(thread, 0, 0);

	didPreempt = 0;
	result = sceCtrlGetIdleCancelThreshold(&rest, &back);
	schedf("sceCtrlGetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	rest = 0;
	back = 0;
	result = sceCtrlSetIdleCancelThreshold(rest, back);
	schedf("sceCtrlSetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	result = sceCtrlGetIdleCancelThreshold(&rest, &back);
	schedf("sceCtrlGetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	rest = -127;
	back = -127;
	result = sceCtrlSetIdleCancelThreshold(rest, back);
	schedf("sceCtrlSetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	rest = -65535;
	back = 65536;
	result = sceCtrlSetIdleCancelThreshold(rest, back);
	schedf("sceCtrlSetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	rest = 128;
	back = 128;
	result = sceCtrlSetIdleCancelThreshold(rest, back);
	schedf("sceCtrlSetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	rest = 0;
	back = -1;
	result = sceCtrlSetIdleCancelThreshold(rest, back);
	schedf("sceCtrlSetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	rest = -2;
	back = -2;
	result = sceCtrlSetIdleCancelThreshold(rest, back);
	schedf("sceCtrlSetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	rest = 129;
	back = 129;
	result = sceCtrlSetIdleCancelThreshold(rest, back);
	schedf("sceCtrlSetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	result = sceCtrlGetIdleCancelThreshold(&rest, &back);
	schedf("sceCtrlGetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	rest = 1;
	back = 129;
	result = sceCtrlSetIdleCancelThreshold(rest, back);
	schedf("sceCtrlSetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	result = sceCtrlGetIdleCancelThreshold(&rest, &back);
	schedf("sceCtrlGetIdleCancelThreshold: %08X (%d, %d) preempt:%d\n", result, rest, back, didPreempt);

	didPreempt = 0;
	result = sceCtrlGetIdleCancelThreshold(&rest, NULL);
	schedf("sceCtrlGetIdleCancelThreshold: %08X (%d, NULL) preempt:%d\n", result, rest, didPreempt);

	didPreempt = 0;
	result = sceCtrlGetIdleCancelThreshold(NULL, NULL);
	schedf("sceCtrlGetIdleCancelThreshold: %08X (NULL, NULL) preempt:%d\n", result, didPreempt);

	didPreempt = 0;
	result = sceCtrlGetIdleCancelThreshold((int *) 0xDEADBEEF, (int *) 0xDEADBEEF);
	schedf("sceCtrlGetIdleCancelThreshold: %08X (0xDEADBEEF, 0xDEADBEEF) preempt:%d\n", result, didPreempt);

	sceKernelTerminateDeleteThread(thread);

	printf("%s", schedulingLog);

	return 0;
}
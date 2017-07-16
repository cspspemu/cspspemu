#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspumd.h>
#include <psprtc.h>

static char schedulingLog[65536];
static char *schedulingLogPos;
static volatile int didPreempt = 0;
static volatile int needsCancel = 0;
static int testResult;
static int otherCB = 0;

#define TEST_NAMED_RES(name, func, args...) \
	didPreempt = 0; \
	sceKernelNotifyCallback(otherCB, 1); \
	testResult = func(args); \
	schedf("%s: %08X preempt:%d\n", name, testResult, didPreempt); \
	/* Give the drive time to do its thing. */ \
	sceKernelDelayThread(2000);

#define TEST_RES(func, args...) TEST_NAMED_RES(#func, func, args)

void schedf(const char *format, ...) {
	va_list args;
	va_start(args, format);
	schedulingLogPos += vsprintf(schedulingLogPos, format, args);
	// This is easier to debug in the emulator, but printf() reschedules on the real PSP.
	//vprintf(format, args);
	va_end(args);
}

int umdHandler(int unknown, int info, void *arg)
{
	schedf("umdHandler called: %08X, %08X, %08X\n", (uint)unknown, (uint)info, (uint)arg);
	return 0;
}

int umdHandler2(int unknown, int info, void *arg)
{
	schedf("umdHandler2 called: %08X, %08X, %08X\n", (uint)unknown, (uint)info, (uint)arg);
	return 0;
}

int otherHandler(int unknown, int info, void *arg)
{
	schedf("otherHandler called: %08X, %08X, %08X\n", (uint)unknown, (uint)info, (uint)arg);
	return 0;
}

int testThread(SceSize argc, void* argv)
{
	while (1)
	{
		sceKernelRotateThreadReadyQueue(sceKernelGetThreadCurrentPriority());
		didPreempt = 1;
		if (needsCancel)
		{
			needsCancel = 0;
			schedf("sceUmdCancelWaitDriveStat: %08x\n", sceUmdCancelWaitDriveStat());
		}
	}
	return 0;
}

int main(int argc, char **argv) {
	schedulingLogPos = schedulingLog;

	printf("WARNING: Test cannot run without a UMD in the PSP.\n\n");

	int param = 0x1234;
	SceUID cb = sceKernelCreateCallback("umdHandler", umdHandler, (void *)param);
	SceUID cb2 = sceKernelCreateCallback("umdHandler2", umdHandler2, (void *)param);
	otherCB = sceKernelCreateCallback("otherHandler", otherHandler, (void *)param);

	SceUID thread = sceKernelCreateThread("preempt", &testThread, sceKernelGetThreadCurrentPriority(), 0x500, 0, NULL);
	sceKernelStartThread(thread, 0, 0);

	TEST_RES(sceUmdRegisterUMDCallBack, cb);
	TEST_RES(sceUmdRegisterUMDCallBack, cb2);
	TEST_RES(sceUmdUnRegisterUMDCallBack, -1);
	TEST_RES(sceUmdCheckMedium);
	TEST_RES(sceUmdActivate, 1, "disc0:");
	TEST_RES(sceUmdWaitDriveStat, 0x02);
	TEST_RES(sceUmdDeactivate, 1, "disc0:");
	TEST_RES(sceKernelCheckCallback);
	TEST_RES(sceUmdDeactivate, 1, "disc0:");
	//TEST_RES(sceUmdGetDriveStat);
	TEST_RES(sceUmdWaitDriveStatCB, 0x20, 10000);
	TEST_RES(sceUmdDeactivate, 1, "disc0:");
	TEST_RES(sceUmdWaitDriveStatWithTimer, 0x20, 10000);
	TEST_RES(sceUmdActivate, 1, "disc0:");
	TEST_RES(sceUmdWaitDriveStatWithTimer, 0x20, 10000);
	TEST_RES(sceKernelCheckCallback);
	TEST_RES(sceUmdUnRegisterUMDCallBack, cb);
	schedf("sceUmdUnRegisterUMDCallBack: %s\n", sceUmdUnRegisterUMDCallBack(cb2) == cb2 ? "OK" : "Failed");
	TEST_RES(sceUmdUnRegisterUMDCallBack, cb2);
	TEST_RES(sceUmdDeactivate, 1, "disc0:");
	//TEST_RES(sceUmdGetDriveStat);
	TEST_RES(sceUmdWaitDriveStatCB, 0x20, 10000);
	//TEST_RES(sceUmdWaitDriveStatCB, 0x10, 10000);
	TEST_RES(sceUmdWaitDriveStatCB, 0xFF, 10000);
	TEST_RES(sceUmdWaitDriveStatCB, 0xFF, 10000);
	TEST_RES(sceUmdWaitDriveStatCB, 0xFF, 10000);
	TEST_RES(sceUmdWaitDriveStatCB, 0xFF, 10000);
	TEST_RES(sceUmdWaitDriveStatCB, 0xFF, 10000);

	needsCancel = 1;
	TEST_RES(sceUmdWaitDriveStat, 0x20);

	sceKernelTerminateDeleteThread(thread);
	sceKernelDelayThread(300);
	printf("%s", schedulingLog);

	return 0;
}

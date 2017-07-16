#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <pspumd.h>

char schedulingLog[65536];
char *schedulingLogPos;

int umdHandler(int unknown, int info, void *arg) {
	// Ignore the specific value, depends on whether a disc is inserted
	info = info ? 1 : 0;
	schedulingLogPos += sprintf(schedulingLogPos, "umdHandler called: %08X, %08X, %08X\n", (uint)unknown, (uint)info, (uint)arg);

	return 0;
}

int main(int argc, char **argv) {
	int umdCbCallbackId;
	int result;

	schedulingLogPos = schedulingLog;

	umdCbCallbackId = sceKernelCreateCallback("umdHandler", umdHandler, (void *)0x1234);
	
	result = sceUmdRegisterUMDCallBack(umdCbCallbackId);
	schedulingLogPos += sprintf(schedulingLogPos, "sceUmdRegisterUMDCallBack: %08X\n", result);
	result = sceUmdActivate(1, "disc0:");
	schedulingLogPos += sprintf(schedulingLogPos, "sceUmdActivate: %08X\n", result);
	result = sceKernelCheckCallback();
	schedulingLogPos += sprintf(schedulingLogPos, "sceKernelCheckCallback: %d\n", result);
	result = sceUmdDeactivate(1, "disc0:");
	schedulingLogPos += sprintf(schedulingLogPos, "sceUmdDeactivate: %08X\n", result);
	result = sceUmdWaitDriveStatCB(0xFF, 0);
	schedulingLogPos += sprintf(schedulingLogPos, "sceUmdWaitDriveStatCB: %08x\n", result);
	result = sceKernelCheckCallback();
	schedulingLogPos += sprintf(schedulingLogPos, "sceKernelCheckCallback: %d\n", result);
	result = sceUmdUnRegisterUMDCallBack(umdCbCallbackId);
	schedulingLogPos += sprintf(schedulingLogPos, "sceUmdUnRegisterUMDCallBack: %08X\n", result >= 0);
	result = sceUmdUnRegisterUMDCallBack(umdCbCallbackId);
	schedulingLogPos += sprintf(schedulingLogPos, "sceUmdUnRegisterUMDCallBack: %08X\n", result >= 0);

	printf("%s", schedulingLog);

	return 0;
}

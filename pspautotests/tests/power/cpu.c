#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>
#include <psppower.h>

int powerHandler(int unknown, int powerInfo, void *arg) {
	schedf("powerHandler called: %08X, %08X, %08X\n", (uint)unknown, (uint)powerInfo & 0x00000080, (uint)arg);

	return 0;
}

static uint testResult;
static float testResultf;

#define TEST_NAMED_RES(name, func, args...) \
	testResult = func(args); \
	schedf("%s: %08X\n", name, testResult);

#define TEST_NAMED_RESF(name, func, args...) \
	testResult = func(args); \
	schedf("%s: %f\n", name, testResultf);

#define TEST_NAMED_NOTZERO(name, func, args...) \
	testResult = func(args); \
	schedf("%s: %s\n", name, testResult != 0 ? "OK" : "Failed");

#define TEST_RES(func, args...) TEST_NAMED_RES(#func, func, args)
#define TEST_RESF(func, args...) TEST_NAMED_RESF(#func, func, args)
#define TEST_NOTZERO(func, args...) TEST_NAMED_NOTZERO(#func, func, args)

int main(int argc, char **argv) {
	int powerCbCallbackId = sceKernelCreateCallback("powerHandler", powerHandler, (void *)0x1234);
	scePowerRegisterCallback(-1, powerCbCallbackId);

	TEST_RES(sceKernelCheckCallback);

	TEST_RES(scePowerGetCpuClockFrequency);
	TEST_RES(scePowerGetCpuClockFrequencyInt);
	TEST_RESF(scePowerGetCpuClockFrequencyFloat);
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(scePowerSetCpuClockFrequency, 166);
	TEST_RES(sceKernelCheckCallback);

	TEST_RES(scePowerGetCpuClockFrequency);
	TEST_RES(scePowerGetCpuClockFrequencyInt);
	TEST_RESF(scePowerGetCpuClockFrequencyFloat);
	TEST_RES(sceKernelCheckCallback);

	return 0;
}

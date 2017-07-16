#include <common.h>

#include <pspthreadman.h>
#include <psppower.h>
#include <pspsuspend.h>

extern "C" int scePowerVolatileMemTryLock(int, void **, int *);
extern "C" int scePowerVolatileMemLock(int, void **, int *);
extern "C" int scePowerVolatileMemUnlock(int);

int testKernelUnlock(const char *title, int mode) {
	if (sceKernelVolatileMemLock(0, NULL, NULL) != 0) {
		printf("TEST FAILURE\n");
	}
	int result = sceKernelVolatileMemUnlock(mode);
	checkpoint("%s: %08x", title, result);
	if (result != 0) {
		sceKernelVolatileMemUnlock(0);
	}
	return result;
}

int testPowerUnlock(const char *title, int mode) {
	if (sceKernelVolatileMemLock(0, NULL, NULL) != 0) {
		printf("TEST FAILURE\n");
	}
	int result = scePowerVolatileMemUnlock(mode);
	checkpoint("%s: %08x", title, result);
	if (result != 0) {
		sceKernelVolatileMemUnlock(0);
	}
	return result;
}

extern "C" int main(int argc, char *argv[]) {
	void *ptr;
	int size;

	checkpointNext("Modes:");
	int modes[] = {-2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 0x10, 0x100, 0x1000, 0x7FFFFFFF};
	for (size_t i = 0; i < ARRAY_SIZE(modes); ++i) {
		char temp[32];
		snprintf(temp, 32, "  Mode %d", modes[i]);
		testKernelUnlock(temp, modes[i]);
	}

	checkpointNext("Modes (scePower):");
	for (size_t i = 0; i < ARRAY_SIZE(modes); ++i) {
		char temp[32];
		snprintf(temp, 32, "  Mode %d (scePower)", modes[i]);
		testPowerUnlock(temp, modes[i]);
	}

	checkpointNext("While not locked:");
	checkpoint("  Kernel: %08x", sceKernelVolatileMemUnlock(0));
	checkpoint("  Power: %08x", sceKernelVolatileMemUnlock(0));

	return 0;
}
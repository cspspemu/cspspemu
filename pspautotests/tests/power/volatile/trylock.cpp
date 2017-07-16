#include <common.h>

#include <pspthreadman.h>
#include <psppower.h>
#include <pspsuspend.h>
#include <pspintrman.h>

extern "C" int scePowerVolatileMemTryLock(int, void **, int *);
extern "C" int scePowerVolatileMemLock(int, void **, int *);
extern "C" int scePowerVolatileMemUnlock(int);

int testKernelTryLock(const char *title, int mode, bool withPtr = true, bool withSize = true) {
	void *ptr = (void *)0xDEAD1337;
	int size = 0x1337;
	int result = sceKernelVolatileMemTryLock(mode, withPtr ? &ptr : NULL, withSize ? &size : NULL);
	checkpoint("%s: %08x (%p, %08x)", title, result, ptr, size);
	if (result == 0) {
		sceKernelVolatileMemUnlock(0);
	}
	return result;
}

int testPowerTryLock(const char *title, int mode, bool withPtr = true, bool withSize = true) {
	void *ptr = (void *)0xDEAD1337;
	int size = 0x1337;
	int result = scePowerVolatileMemTryLock(mode, withPtr ? &ptr : NULL, withSize ? &size : NULL);
	checkpoint("%s: %08x (%p, %08x)", title, result, ptr, size);
	if (result == 0) {
		scePowerVolatileMemUnlock(0);
	}
	return result;
}

bool interruptRan = false;
extern "C" void interruptFunc(int no, void *arg) {
	if (interruptRan) {
		return;
	}
	interruptRan = true;

	testKernelTryLock("  While dispatch disabled", 0);
	testKernelTryLock("  While dispatch disabled (again)", 0);
	testPowerTryLock("  While dispatch disabled (scePower)", 0);
}

extern "C" int main(int argc, char *argv[]) {
	checkpointNext("Modes:");
	int modes[] = {-2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 0x10, 0x100, 0x1000, 0x7FFFFFFF};
	for (size_t i = 0; i < ARRAY_SIZE(modes); ++i) {
		char temp[32];
		snprintf(temp, 32, "  Mode %d", modes[i]);
		testKernelTryLock(temp, modes[i]);
	}

	checkpointNext("Output:");
	testKernelTryLock("  No ptr", 0, false, true);
	testKernelTryLock("  No size", 0, true, false);
	testKernelTryLock("  Neither", 0, false, false);

	checkpointNext("Double:");
	sceKernelVolatileMemTryLock(0, NULL, NULL);
	testKernelTryLock("  While locked", 0);
	sceKernelVolatileMemUnlock(0);
	testKernelTryLock("  After unlock", 0);

	checkpointNext("Modes (scePower):");
	for (size_t i = 0; i < ARRAY_SIZE(modes); ++i) {
		char temp[32];
		snprintf(temp, 32, "  Mode %d (scePower)", modes[i]);
		testPowerTryLock(temp, modes[i]);
	}

	checkpointNext("Output (scePower):");
	testPowerTryLock("  No ptr (scePower)", 0, false, true);
	testPowerTryLock("  No size (scePower)", 0, true, false);
	testPowerTryLock("  Neither (scePower)", 0, false, false);

	checkpointNext("Double (scePower):");
	sceKernelVolatileMemTryLock(0, NULL, NULL);
	testPowerTryLock("  While locked (scePower)", 0);
	sceKernelVolatileMemUnlock(0);
	testPowerTryLock("  After unlock (scePower)", 0);

	checkpointNext("Interrupts disabled:");
	int flag = sceKernelCpuSuspendIntr();
	testKernelTryLock("  While dispatch disabled", 0);
	testKernelTryLock("  While dispatch disabled (again)", 0);
	testPowerTryLock("  While dispatch disabled (scePower)", 0);
	sceKernelCpuResumeIntr(flag);
	checkpoint("  Unlock (outside): %08x", sceKernelVolatileMemUnlock(0));

	sceKernelVolatileMemLock(0, NULL, NULL);
	checkpointNext("Interrupts disabled (while locked):");
	flag = sceKernelCpuSuspendIntr();
	testKernelTryLock("  While dispatch disabled", 0);
	testKernelTryLock("  While dispatch disabled (again)", 0);
	testPowerTryLock("  While dispatch disabled (scePower)", 0);
	sceKernelCpuResumeIntr(flag);
	checkpoint("  Unlock (outside): %08x", sceKernelVolatileMemUnlock(0));

	interruptRan = false;
	checkpointNext("Inside interrupt:");
	sceKernelRegisterSubIntrHandler(PSP_VBLANK_INT, 1, (void *)interruptFunc, NULL);
	sceKernelEnableSubIntr(PSP_VBLANK_INT, 1);
	sceKernelDelayThread(30000);
	sceKernelDisableSubIntr(PSP_VBLANK_INT, 1);
	sceKernelReleaseSubIntrHandler(PSP_VBLANK_INT, 1);
	checkpoint("  Unlock (outside): %08x", sceKernelVolatileMemUnlock(0));
	
	interruptRan = false;
	sceKernelVolatileMemLock(0, NULL, NULL);
	checkpointNext("Inside interrupt (while locked):");
	sceKernelRegisterSubIntrHandler(PSP_VBLANK_INT, 1, (void *)interruptFunc, NULL);
	sceKernelEnableSubIntr(PSP_VBLANK_INT, 1);
	sceKernelDelayThread(30000);
	sceKernelDisableSubIntr(PSP_VBLANK_INT, 1);
	sceKernelReleaseSubIntrHandler(PSP_VBLANK_INT, 1);
	checkpoint("  Unlock (outside): %08x", sceKernelVolatileMemUnlock(0));

	return 0;
}
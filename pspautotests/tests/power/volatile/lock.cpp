#include <common.h>

#include <pspthreadman.h>
#include <psppower.h>
#include <pspsuspend.h>
#include <pspintrman.h>

extern "C" int scePowerVolatileMemTryLock(int, void **, int *);
extern "C" int scePowerVolatileMemLock(int, void **, int *);
extern "C" int scePowerVolatileMemUnlock(int);

int testKernelLock(const char *title, int mode, bool withPtr = true, bool withSize = true) {
	void *ptr = (void *)0xDEAD1337;
	int size = 0x1337;
	int result = sceKernelVolatileMemLock(mode, withPtr ? &ptr : NULL, withSize ? &size : NULL);
	checkpoint("%s: %08x (%p, %08x)", title, result, ptr, size);
	if (result == 0) {
		sceKernelVolatileMemUnlock(0);
	}
	return result;
}

int testPowerLock(const char *title, int mode, bool withPtr = true, bool withSize = true) {
	void *ptr = (void *)0xDEAD1337;
	int size = 0x1337;
	int result = scePowerVolatileMemLock(mode, withPtr ? &ptr : NULL, withSize ? &size : NULL);
	checkpoint("%s: %08x (%p, %08x)", title, result, ptr, size);
	if (result == 0) {
		scePowerVolatileMemUnlock(0);
	}
	return result;
}

struct VolatileWaitThread {
	VolatileWaitThread(const char *name, int prio = 0x60)
		: name_(name) {
		thread_ = sceKernelCreateThread(name, &run, prio, 0x1000, 0, NULL);
	}

	void start() {
		const void *arg[1] = { (void *)this };
		sceKernelStartThread(thread_, sizeof(arg), arg);
		sceKernelDelayThread(1000);
		checkpoint("  ** started %s", name_);
	}

	void stop() {
		if (thread_ >= 0) {
			if (sceKernelGetThreadExitStatus(thread_) != 0) {
				sceKernelDelayThread(1000);
				sceKernelTerminateDeleteThread(thread_);
				checkpoint("  ** stopped %s", name_);
			} else {
				sceKernelDeleteThread(thread_);
			}
		}
		thread_ = 0;
	}

	static int run(SceSize args, void *argp) {
		VolatileWaitThread *o = *(VolatileWaitThread **)argp;
		return o->wait();
	}

	virtual int wait() {
		checkpoint("ERROR: base wait() called.");
		return 1;
	}

	~VolatileWaitThread() {
		stop();
	}

	const char *name_;
	SceUID thread_;
};

struct KernelVolatileWaitThread : public VolatileWaitThread {
	KernelVolatileWaitThread(const char *name, int prio = 0x60)
		: VolatileWaitThread(name, prio) {
		start();
	}

	virtual int wait() {
		char msg[32];
		snprintf(msg, 32, "  * %s", name_);
		testKernelLock(msg, 0);
		return 0;
	}
};

struct PowerVolatileWaitThread : public VolatileWaitThread {
	PowerVolatileWaitThread(const char *name, int prio = 0x60)
		: VolatileWaitThread(name, prio) {
		start();
	}

	virtual int wait() {
		char msg[32];
		snprintf(msg, 32, "  * %s", name_);
		testPowerLock(msg, 0);
		return 0;
	}
};

bool interruptRan = false;
extern "C" void interruptFunc(int no, void *arg) {
	if (interruptRan) {
		return;
	}
	interruptRan = true;

	testKernelLock("  While dispatch disabled", 0);
	testKernelLock("  While dispatch disabled (again)", 0);
	testPowerLock("  While dispatch disabled (scePower)", 0);
}

extern "C" int main(int argc, char *argv[]) {
	checkpointNext("Modes:");
	int modes[] = {-2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 0x10, 0x100, 0x1000, 0x7FFFFFFF};
	for (size_t i = 0; i < ARRAY_SIZE(modes); ++i) {
		char temp[32];
		snprintf(temp, 32, "  Mode %d", modes[i]);
		testKernelLock(temp, modes[i]);
	}

	checkpointNext("Output:");
	testKernelLock("  No ptr", 0, false, true);
	testKernelLock("  No size", 0, true, false);
	testKernelLock("  Neither", 0, false, false);

	checkpointNext("Waits:");
	{
		sceKernelVolatileMemLock(0, NULL, NULL);
		KernelVolatileWaitThread wait1("waiting thread 1", 0x31);
		KernelVolatileWaitThread wait2("waiting thread 2", 0x33);
		KernelVolatileWaitThread wait3("waiting thread 3", 0x32);
		checkpoint("  With three waiting");
		sceKernelVolatileMemUnlock(0);
		sceKernelDelayThread(1000);
	}

	checkpointNext("Modes (scePower):");
	for (size_t i = 0; i < ARRAY_SIZE(modes); ++i) {
		char temp[32];
		snprintf(temp, 32, "  Mode %d (scePower)", modes[i]);
		testPowerLock(temp, modes[i]);
	}

	checkpointNext("Output (scePower):");
	testPowerLock("  No ptr (scePower)", 0, false, true);
	testPowerLock("  No size (scePower)", 0, true, false);
	testPowerLock("  Neither (scePower)", 0, false, false);

	checkpointNext("Waits (scePower):");
	{
		sceKernelVolatileMemLock(0, NULL, NULL);
		PowerVolatileWaitThread wait1("waiting thread 1", 0x31);
		PowerVolatileWaitThread wait2("waiting thread 2", 0x33);
		PowerVolatileWaitThread wait3("waiting thread 3", 0x32);
		checkpoint("  With three waiting");
		sceKernelVolatileMemUnlock(0);
		sceKernelDelayThread(1000);
	}

	checkpointNext("Interrupts disabled:");
	int flag = sceKernelCpuSuspendIntr();
	testKernelLock("  While dispatch disabled", 0);
	testKernelLock("  While dispatch disabled (again)", 0);
	testPowerLock("  While dispatch disabled (scePower)", 0);
	sceKernelCpuResumeIntr(flag);
	checkpoint("  Unlock (outside): %08x", sceKernelVolatileMemUnlock(0));

	sceKernelVolatileMemLock(0, NULL, NULL);
	checkpointNext("Interrupts disabled (while locked):");
	flag = sceKernelCpuSuspendIntr();
	testKernelLock("  While dispatch disabled", 0);
	testKernelLock("  While dispatch disabled (again)", 0);
	testPowerLock("  While dispatch disabled (scePower)", 0);
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
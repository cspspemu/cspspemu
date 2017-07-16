#include <common.h>

#include <pspthreadman.h>
#include <pspmodulemgr.h>

const static SceUInt NO_TIMEOUT = (SceUInt)-1337;

struct SceKernelFplOptParam2 {
	SceSize size;
	u32 alignment;
};

enum SceKernelFplAttr {
	PSP_FPL_ATTR_FIFO = 0x0000,
	PSP_FPL_ATTR_PRIORITY = 0x0100,
	PSP_FPL_ATTR_HIGHMEM = 0x4000,
	PSP_FPL_ATTR_KNOWN = PSP_FPL_ATTR_FIFO | PSP_FPL_ATTR_PRIORITY | PSP_FPL_ATTR_HIGHMEM,
};

inline void schedfFpl(SceKernelFplInfo &info) {
	schedf("FPL: OK (size=%d,name=%s,attr=%08x,blockSize=%08x,numBlocks=%08x,freeBlocks=%08x,wait=%d)\n", info.size, info.name, info.attr, info.blockSize, info.numBlocks, info.freeBlocks, info.numWaitThreads);
}

inline void schedfFpl(SceUID fpl) {
	if (fpl > 0) {
		SceKernelFplInfo info;
		info.size = sizeof(info);

		int result = sceKernelReferFplStatus(fpl, &info);
		if (result == 0) {
			schedfFpl(info);
		} else {
			schedf("FPL: Invalid (%08x)\n", result);
		}
	} else {
		schedf("FPL: Failed (%08x)\n", fpl);
	}
}

struct KernelObjectWaitThread {
	KernelObjectWaitThread(const char *name, SceUID uid, SceUInt timeout, int prio = 0x60)
		: name_(name), object_(uid), timeout_(timeout) {
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
		KernelObjectWaitThread *o = *(KernelObjectWaitThread **)argp;
		return o->wait();
	}

	virtual int wait() {
		checkpoint("ERROR: base wait() called.");
		return 1;
	}

	~KernelObjectWaitThread() {
		stop();
	}

	const char *name_;
	SceUID thread_;
	SceUID object_;
	SceUInt timeout_;
};

struct FplWaitThread : public KernelObjectWaitThread {
	FplWaitThread(const char *name, SceUID uid, SceUInt timeout, int hold_ms = 1000, int prio = 0x60)
		: KernelObjectWaitThread(name, uid, timeout, prio), hold_ms_(hold_ms) {
		start();
	}

	virtual int wait() {
		void *allocated = NULL;
		if (timeout_ == NO_TIMEOUT) {
			int result = sceKernelAllocateFpl(object_, &allocated, NULL);
			checkpoint("  ** %s got result: %08x, received = %s", name_, result, allocated == NULL ? "NULL" : "ptr");
		} else {
			int result = sceKernelAllocateFpl(object_, &allocated, &timeout_);
			checkpoint("  ** %s got result: %08x, received = %s, timeout = %dms remaining", name_, result, allocated == NULL ? "NULL" : "ptr", timeout_ / 1000);
		}
		if (allocated != NULL) {
			checkpoint("  ** %s delayed: %08x", name_, sceKernelDelayThread(hold_ms_));
			checkpoint("  ** %s freed: %08x", name_, sceKernelFreeFpl(object_, allocated));
		}
		return 0;
	}

	int hold_ms_;
};

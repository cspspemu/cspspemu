#include <common.h>

#include <pspthreadman.h>
#include <pspmodulemgr.h>

const static SceUInt NO_TIMEOUT = (SceUInt)-1337;

static inline SceUID sceKernelCreateMsgPipe(const char *name, int part, u32 attr, u32 size, void *opt) {
	return sceKernelCreateMsgPipe(name, part, attr, (void *)size, opt);
}

static inline void schedfMsgPipe(SceKernelMppInfo *info) {
	schedf("Msgpipe: OK (size=%d, name=%s, attr=%08x, buffer=%x, free=%x, sending=%d, receiving=%d)\n", info->size, info->name, info->attr, info->bufSize, info->freeSize, info->numSendWaitThreads, info->numReceiveWaitThreads);
}

static inline void schedfMsgPipe(SceUID msgpipe) {
	if (msgpipe >= 0) {
		SceKernelMppInfo info;
		info.size = sizeof(info);

		int result = sceKernelReferMsgPipeStatus(msgpipe, &info);
		if (result == 0) {
			schedfMsgPipe(&info);
		} else {
			schedf("Msgpipe: Invalid (%08x)\n", result);
		}
	} else {
		schedf("Msgpipe: Failed (%08x)\n", msgpipe);
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

struct MsgPipeReceiveWaitThread : public KernelObjectWaitThread {
	MsgPipeReceiveWaitThread(const char *name, SceUID uid, SceUInt timeout, u32 size = 256, int prio = 0x60)
		: KernelObjectWaitThread(name, uid, timeout, prio), size_(size) {
		start();
	}

	virtual int wait() {
		char msg[256];
		int received = 0x1337;
		if (timeout_ == NO_TIMEOUT) {
			int result = sceKernelReceiveMsgPipe(object_, msg, size_, 0, &received, NULL);
			checkpoint("  ** %s got result: %08x, received = %08x", name_, result, received);
		} else {
			int result = sceKernelReceiveMsgPipe(object_, msg, size_, 0, &received, &timeout_);
			checkpoint("  ** %s got result: %08x, received = %08x, timeout = %dms remaining", name_, result, received, timeout_ / 1000);
		}
		return 0;
	}

	u32 size_;
};

struct MsgPipeSendWaitThread : public KernelObjectWaitThread {
	MsgPipeSendWaitThread(const char *name, SceUID uid, SceUInt timeout, u32 size = 256, int prio = 0x60)
		: KernelObjectWaitThread(name, uid, timeout, prio), size_(size) {
		start();
	}

	virtual int wait() {
		char msg[256];
		int sent = 0x1337;
		if (timeout_ == NO_TIMEOUT) {
			int result = sceKernelSendMsgPipe(object_, msg, size_, 0, &sent, NULL);
			checkpoint("  ** %s got result: %08x, sent = %08x", name_, result, sent);
		} else {
			int result = sceKernelSendMsgPipe(object_, msg, size_, 0, &sent, &timeout_);
			checkpoint("  ** %s got result: %08x, sent = %08x, timeout = %dms remaining", name_, result, sent, timeout_ / 1000);
		}
		return 0;
	}

	u32 size_;
};

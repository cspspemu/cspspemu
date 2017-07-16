#include <common.h>

#include <pspthreadman.h>
#include <pspmodulemgr.h>

#include <assert.h>

const static SceUInt NO_TIMEOUT = (SceUInt)-1337;

inline void schedfCallback(SceKernelCallbackInfo &info) {
	schedf("Callback: OK (size=%d,name=%s,thread=%d,callback=%d,common=%08x,notifyCount=%08x,notifyArg=%d)\n", info.size, info.name, info.threadId == 0 ? 0 : 1, info.callback == 0 ? 0 : 1, info.common, info.notifyCount, info.notifyArg);
}

inline void schedfCallback(SceUID cb) {
	if (cb > 0) {
		SceKernelCallbackInfo info;
		info.size = sizeof(info);

		int result = sceKernelReferCallbackStatus(cb, &info);
		if (result == 0) {
			schedfCallback(info);
		} else {
			schedf("Callback: Invalid (%08x)\n", result);
		}
	} else {
		schedf("Callback: Failed (%08x)\n", cb);
	}
}

struct Callback {
	template <typename T>
	Callback(const char *name, SceKernelCallbackFunction func, T arg) : uid_(-1) {
		Create(name, func, arg);
	}

	~Callback() {
		if (uid_ >= 0) {
			Delete();
		}
	}

	int Delete() {
		int result = sceKernelDeleteCallback(uid_);
		uid_ = -1;
		return result;
	}

	template <typename T>
	int Create(const char *name, SceKernelCallbackFunction func, T arg) {
		assert(sizeof(arg) == 4);
		if (uid_ >= 0) {
			Delete();
		}
		uid_ = sceKernelCreateCallback(name, func, (void *)arg);
		if (uid_ < 0) {
			return uid_;
		}
		return 0;
	}

	operator SceUID() {
		return uid_;
	}

	SceUID uid_;
};

struct BasicThread {
	BasicThread(const char *name, int prio = 0x60)
		: name_(name) {
		thread_ = sceKernelCreateThread(name, &run, prio, 0x1000, 0, NULL);
	}

	void start() {
		const void *arg[1] = { (void *)this };
		sceKernelStartThread(thread_, sizeof(arg), arg);
		checkpoint("  ** started %s", name_);
	}

	void stop() {
		if (thread_ >= 0) {
			if (sceKernelGetThreadExitStatus(thread_) != 0) {
				sceKernelDelayThread(500);
				sceKernelTerminateDeleteThread(thread_);
				checkpoint("  ** stopped %s", name_);
			} else {
				sceKernelDeleteThread(thread_);
			}
		}
		thread_ = 0;
	}

	static int run(SceSize args, void *argp) {
		BasicThread *o = *(BasicThread **)argp;
		return o->execute();
	}

	virtual int execute() = 0;

	~BasicThread() {
		stop();
	}

	const char *name_;
	SceUID thread_;
};

struct CallbackSleeper : public BasicThread {
	CallbackSleeper(const char *name, int prio = 0x60, bool doStart = true)
		: BasicThread(name, prio), ret_(0) {
		if (doStart) {
			start();
		}
	}

	static int callback(int arg1, int arg2, void *arg) {
		CallbackSleeper *me = (CallbackSleeper *)arg;
		return me->hit(arg1, arg2);
	}

	virtual int hit(int arg1, int arg2) {
		checkpoint("  Callback hit on %s: %08x, %08x, returning %08x", name_, arg1, arg2, ret_);
		return ret_;
	}

	virtual int execute() {
		cb_ = sceKernelCreateCallback(name_, &CallbackSleeper::callback, (void *)this);
		checkpoint("  Beginning sleep on %s", name_);
		checkpoint("  Woke from sleep: %08x", sceKernelSleepThreadCB());
		return 0;
	}

	SceUID callbackID() {
		return cb_;
	}

	void wakeup() {
		sceKernelWakeupThread(thread_);
	}

	void setReturn(int ret) {
		ret_ = ret;
	}

	int ret_;
	SceUID cb_;
};

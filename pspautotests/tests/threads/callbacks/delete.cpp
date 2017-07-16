#include "shared.h"

int cbFunc(int arg1, int arg2, void *arg) {
	checkpoint(" * cbFunc hit: %08x, %08x, %08x", arg1, arg2, arg);
	return 0;
}

inline void testDelete(const char *title, SceUID cb) {
	int result = sceKernelDeleteCallback(cb);
	if (result == 0) {
		checkpoint("%s: OK", title);
	} else {
		checkpoint("%s: Failed (%08x)", title, result);
	}
}

struct CallbackDeleter : public BasicThread {
	CallbackDeleter(const char *name, SceUID cb, int prio = 0x60)
		: BasicThread(name, prio), cb_(cb) {
		start();
	}

	virtual int execute() {
		testDelete("  Delete on different thread", cb_);
		return 0;
	}

	SceUID cb_;
};

extern "C" int main(int argc, char *argv[]) {
	SceUID cb = sceKernelCreateCallback("delete", &cbFunc, NULL);

	checkpointNext("Objects:");
	testDelete("  Normal", cb);
	testDelete("  NULL", 0);
	testDelete("  Invalid", 0xDEADBEEF);
	testDelete("  Deleted", cb);

	checkpointNext("Different thread:");
	{
		cb = sceKernelCreateCallback("delete", &cbFunc, NULL);
		CallbackDeleter deleter("deleting thread", cb);
		sceKernelDelayThread(4000);
	}

	checkpointNext("Deleting during wait:");
	{
		CallbackSleeper waiter1("better priority sleeping thread", 0x10);
		CallbackSleeper waiter2("worse priority sleeping thread", 0x30);
		sceKernelDelayThread(1000);
		testDelete("  Delete better priority cb", waiter1.callbackID());
		testDelete("  Delete worse priority cb", waiter2.callbackID());
		sceKernelDelayThread(1000);
	}

	checkpointNext("Associated with a deleted thread:");
	{
		CallbackSleeper waiter("sleeping thread");
		sceKernelDelayThread(1000);
		waiter.wakeup();
		waiter.stop();

		sceKernelDelayThread(1000);
		testDelete("  Delete after thread delete", waiter.callbackID());
		sceKernelDelayThread(1000);
	}

	cb = sceKernelCreateCallback("delete", &cbFunc, NULL);
	checkpointNext("Delete while notified:");
	checkpoint("  Notify: %08x", sceKernelNotifyCallback(cb, 0x1337));
	testDelete("  Delete", cb);

	// And now just to double check - we don't get called, right?
	sceKernelDelayThreadCB(5000);

	return 0;
}

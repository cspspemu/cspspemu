#include "shared.h"

int cbFunc(int arg1, int arg2, void *arg) {
	checkpoint(" * cbFunc hit: %08x, %08x, %08x", arg1, arg2, arg);
	return 0;
}

inline void testCancel(const char *title, SceUID cb) {
	int result = sceKernelCancelCallback(cb);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: OK ", title);
		schedfCallback(cb);
	} else {
		checkpoint("%s: Failed (%08x)", title, result);
	}
}

extern "C" int main(int argc, char *argv[]) {
	Callback cb("cancel", &cbFunc, NULL);

	checkpointNext("Objects:");
	testCancel("  Normal", cb);
	testCancel("  NULL", 0);
	testCancel("  Invalid", 0xDEADBEEF);
	cb.Delete();
	testCancel("  Deleted", cb);

	cb.Create("notify", &cbFunc, NULL);
	checkpointNext("After notify:");
	sceKernelNotifyCallback(cb, 0x123);
	testCancel("  1 pending", cb);

	checkpoint("sceKernelDelayThreadCB: %08x", sceKernelDelayThreadCB(1000));

	checkpointNext("Different thread:");
	{
		CallbackSleeper waiter1("better priority sleeping thread", 0x10);
		CallbackSleeper waiter2("worse priority sleeping thread", 0x30);
		sceKernelDelayThread(1000);
		sceKernelNotifyCallback(waiter1.callbackID(), 0x1337);
		sceKernelNotifyCallback(waiter2.callbackID(), 0x1337);
		sceKernelCancelCallback(waiter1.callbackID());
		sceKernelCancelCallback(waiter2.callbackID());
		sceKernelDelayThread(1000);
	}

	return 0;
}
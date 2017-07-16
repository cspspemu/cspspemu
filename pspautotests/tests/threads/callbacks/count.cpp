#include "shared.h"

int cbFunc(int arg1, int arg2, void *arg) {
	checkpoint(" * cbFunc hit: %08x, %08x, %08x", arg1, arg2, arg);
	return 0;
}

inline void testGetCount(const char *title, SceUID cb) {
	int result = sceKernelGetCallbackCount(cb);
	if (result >= 0) {
		checkpoint(NULL);
		schedf("%s: OK (%d) ", title, result);
		schedfCallback(cb);
	} else {
		checkpoint("%s: Failed (%08x)", title, result);
	}
}

extern "C" int main(int argc, char *argv[]) {
	Callback cb1("count1", &cbFunc, NULL);
	Callback cb2("count2", &cbFunc, NULL);

	checkpointNext("Objects:");
	testGetCount("  Normal", cb1);
	testGetCount("  NULL", 0);
	testGetCount("  Invalid", 0xDEADBEEF);
	cb1.Delete();
	testGetCount("  Deleted", cb1);

	cb1.Create("count1", &cbFunc, NULL);
	checkpointNext("After notify:");
	sceKernelNotifyCallback(cb1, 0x123);
	testGetCount("  1 pending", cb1);
	sceKernelNotifyCallback(cb1, 0x123);
	sceKernelNotifyCallback(cb1, 0x123);
	testGetCount("  1 pending twice", cb1);
	sceKernelNotifyCallback(cb1, 0x123);
	sceKernelNotifyCallback(cb2, 0x123);
	testGetCount("  2 pending", cb1);

	checkpoint("sceKernelDelayThreadCB: %08x", sceKernelDelayThreadCB(1000));

	checkpointNext("Different thread:");
	{
		CallbackSleeper waiter1("better priority sleeping thread", 0x10);
		CallbackSleeper waiter2("worse priority sleeping thread", 0x30);
		sceKernelDelayThread(1000);
		sceKernelNotifyCallback(waiter1.callbackID(), 0x1337);
		sceKernelNotifyCallback(waiter2.callbackID(), 0x1337);
		testGetCount("  Better", waiter1.callbackID());
		testGetCount("  Worse", waiter2.callbackID());
		sceKernelDelayThread(1000);
	}

	return 0;
}
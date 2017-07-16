#include "shared.h"

int cbFunc(int arg1, int arg2, void *arg) {
	checkpoint(" * cbFunc hit: %08x, %08x, %08x", arg1, arg2, arg);
	return 0;
}

extern "C" int main(int argc, char *argv[]) {
	Callback cb1("check1", &cbFunc, 0xABC00001);
	Callback cb2("check2", &cbFunc, 0xABC00002);

	checkpointNext("Basics:");
	checkpoint("  Without pending: %08x", sceKernelCheckCallback());
	sceKernelNotifyCallback(cb1, 1);
	checkpoint("  With 1 pending: %08x", sceKernelCheckCallback());
	sceKernelNotifyCallback(cb1, 1);
	sceKernelNotifyCallback(cb2, 2);
	checkpoint("  With 2 pending: %08x", sceKernelCheckCallback());

	checkpointNext("Different thread:");
	{
		CallbackSleeper waiter1("better priority sleeping thread", 0x10);
		CallbackSleeper waiter2("worse priority sleeping thread", 0x30);
		sceKernelDelayThread(1000);
		sceKernelNotifyCallback(waiter1.callbackID(), 0x1337);
		sceKernelNotifyCallback(waiter2.callbackID(), 0x1337);
		checkpoint("sceKernelCheckCallback: %08x", sceKernelCheckCallback());
		sceKernelDelayThread(1000);
	}

	return 0;
}
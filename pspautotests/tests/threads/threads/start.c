#include "shared.h"

volatile int argLengthArgc = 0;
volatile void *argLengthArgv;
int argLengthFunc(SceSize argc, void *argv) {
	argLengthArgc = argc;
	argLengthArgv = argv;
	return 0;
}

int testFunc(SceSize argc, void *argv) {
	return 0;
}

SceKernelThreadInfo stackCheckInfo;
const char *stackCheckName;
int stackCheckFunc(SceSize argc, void *argv) {
	stackCheckInfo.size = sizeof(stackCheckInfo);
	sceKernelReferThreadStatus(0, &stackCheckInfo);

	if ((int)argv & 0xf) {
		schedf("    %s: ERROR: arg pointer not aligned.\n", stackCheckName);
	}

	u32 *stack = (u32 *) stackCheckInfo.stack;
	u32 *stackEnd = stack + stackCheckInfo.stackSize / 4;

	if (stack[0] != sceKernelGetThreadId()) {
		schedf("    %s: ERROR: stack should start with thread ID.\n", stackCheckName);
	}
	if (stackEnd[-1] != 0xFFFFFFFF || stackEnd[-2] != 0xFFFFFFFF) {
		schedf("    %s: WARNING: k0 laid out differently?\n", stackCheckName);
	}
	if (stackEnd[-14] != (u32)stack) {
		schedf("    %s: WARNING: stack pointer not correct in k0.\n", stackCheckName);
	}
	if (stackEnd[-16] != sceKernelGetThreadId()) {
		schedf("    %s: WARNING: thread id not correct in k0.\n", stackCheckName);
	}

	SceUID uid = sceKernelAllocPartitionMemory(2, "TEST", PSP_SMEM_Low, 0x100, NULL);
	if (stack < (u32 *)sceKernelGetBlockHeadAddr(uid)) {
		schedf("    %s: WARNING: stack allocated low.\n", stackCheckName);
	}
	sceKernelFreePartitionMemory(uid);

	if (stack[1] != 0xFFFFFFFF) {
		schedf("    %s: WARNING: stack not set to FF, instead: %08x.\n", stackCheckName, stack[1]);
	}

	return 0;
}

void testCheckStackLayout(const char *title, int argSize, u32 attr) {
	char argLengthTemp[0x1000];
	memset(argLengthTemp, 0xAB, sizeof(argLengthTemp));

	stackCheckName = title;
	SceUID stackCheckThread = sceKernelCreateThread("stackCheck", &stackCheckFunc, 0x10, 0x1000, attr, NULL);
	sceKernelStartThread(stackCheckThread, argSize, argLengthTemp);
	sceKernelWaitThreadEnd(stackCheckThread, NULL);

	checkpoint("%s", title);
}

int main(int argc, char *argv[]) {
	int i, result;
	SceKernelThreadInfo info;
	SceUID testThread = sceKernelCreateThread("test", &testFunc, sceKernelGetThreadCurrentPriority(), 0x1000, 0, NULL);
	SceUID deletedThread = sceKernelCreateThread("deleted", &testFunc, sceKernelGetThreadCurrentPriority(), 0x1000, 0, NULL);
	sceKernelDeleteThread(deletedThread);

	checkpointNext("Thread IDs:");
	checkpoint("  Normal: %08x", sceKernelStartThread(testThread, 0, NULL));
	checkpoint("  Twice: %08x", sceKernelStartThread(testThread, 0, NULL));
	checkpoint("  NULL: %08x", sceKernelStartThread(0, 0, NULL));
	checkpoint("  Current: %08x", sceKernelStartThread(sceKernelGetThreadId(), 0, NULL));
	checkpoint("  Deleted: %08x", sceKernelStartThread(deletedThread, 0, NULL));
	checkpoint("  Invalid: %08x", sceKernelStartThread(0xDEADBEEF, 0, NULL));

	checkpointNext("Argument length:");
	SceUID argLengthThread = sceKernelCreateThread("argLength", &argLengthFunc, sceKernelGetThreadCurrentPriority() - 1, 0x800, 0, NULL);
	char argLengthTemp[0x1000];
	void *argLengthStack;

	info.size = sizeof(info);
	sceKernelReferThreadStatus(argLengthThread, &info);
	argLengthStack = info.stack;

	result = sceKernelStartThread(argLengthThread, 8, NULL);
	sceKernelWaitThreadEnd(argLengthThread, NULL);
	char temp[256];
	if (argLengthArgv == 0)
		sprintf(temp, "NULL");
	else if (argLengthArgv == argLengthTemp)
		sprintf(temp, "original");
	else
		sprintf(temp, "stack+0x%x", (char *)argLengthArgv - (char *)argLengthStack);
	checkpoint("  With NULL ptr: %08x (%d, %s)", result, argLengthArgc, temp);

	// Note: larger than stack seems to crash the PSP...
	static int lengths[] = {-1, 0x80000000, 0, 1, 2, 3, 4, 5, 6, 7, 8, 80, 90, 0x600};
	for (i = 0; i < ARRAY_SIZE(lengths); ++i) {
		argLengthArgc = -1;
		argLengthArgv = NULL;
		result = sceKernelStartThread(argLengthThread, lengths[i], argLengthTemp);
		sceKernelWaitThreadEnd(argLengthThread, NULL);

		if (argLengthArgv == 0)
			sprintf(temp, "NULL");
		else if (argLengthArgv == argLengthTemp)
			sprintf(temp, "original");
		else
			sprintf(temp, "stack+0x%x", (char *)argLengthArgv - (char *)argLengthStack);
		checkpoint("  %d arg length: %08x (%d, %s)", lengths[i], result, argLengthArgc, temp);
	}

	// NULL crashes it, though...
	checkpointNext("Argument pointers:");
	void *argptrs[] = {argLengthTemp, (void *)0xDEADBEEF, (void *)0x80000000};
	for (i = 0; i < ARRAY_SIZE(argptrs); ++i) {
		argLengthArgc = -1;
		argLengthArgv = NULL;
		result = sceKernelStartThread(argLengthThread, 4, argptrs[i]);
		sceKernelWaitThreadEnd(argLengthThread, NULL);

		if (argLengthArgv == 0)
			sprintf(temp, "NULL");
		else if (argLengthArgv == argLengthTemp)
			sprintf(temp, "original");
		else
			sprintf(temp, "stack+0x%x", (char *)argLengthArgv - (char *)argLengthStack);
		checkpoint("  arg ptr #%d: %08x (%d, %s)", i, result, argLengthArgc, temp);
	}

	checkpointNext("Priorities:");
	static int priorities[] = {0x8, 0x1F, 0x20, 0x21, 0x40};
	for (i = 0; i < ARRAY_SIZE(priorities); ++i) {
		argLengthArgc = -1;
		sceKernelDeleteThread(argLengthThread);
		argLengthThread = sceKernelCreateThread("argLength", &argLengthFunc, priorities[i], 0x800, 0, NULL);
		result = sceKernelStartThread(argLengthThread, priorities[i], argLengthTemp);
		int afterStart = argLengthArgc;
		sceKernelWaitThreadEnd(argLengthThread, NULL);

		int priorityDiff = priorities[i] - sceKernelGetThreadCurrentPriority();
		checkpoint("  priority 0x%02x: %08x (current%s0x%02x, %s)", priorities[i], result, priorityDiff < 0 ? "-" : "+", priorityDiff < 0 ? -priorityDiff : priorityDiff, afterStart == priorities[i] ? "resched" : (argLengthArgc == priorities[i] ? "deferred" : "never"));
	}

	checkpointNext("Stack:");
	testCheckStackLayout("  No arg", 0, 0);
	testCheckStackLayout("  Odd arg", 1, 0);
	testCheckStackLayout("  Large arg", 0x600, 0);
	testCheckStackLayout("  Low stack", 0, 0x00400000);
	testCheckStackLayout("  Low stack without fill", 0, 0x00400000 | 0x00100000);

	return 0;
}
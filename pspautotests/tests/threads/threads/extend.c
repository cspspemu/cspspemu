#include "shared.h"

extern int sceKernelExtendThreadStack(SceSize size, int func(void *), void *param);

const char *extendTitle = NULL;
SceUID mainThreadID = 0;
void *mainThreadStack = NULL;
u32 mainThreadStackSize = 0;

void *getStackAddr(SceUID threadID) {
	SceKernelThreadInfo status;
	status.size = sizeof(status);
	sceKernelReferThreadStatus(threadID, &status);
	return status.stack;
}

u32 getStackSize(SceUID threadID) {
	SceKernelThreadInfo status;
	status.size = sizeof(status);
	sceKernelReferThreadStatus(threadID, &status);
	return status.stackSize;
}

int someThread(void *arg) {
	checkpoint("%s: %x (%s)", extendTitle, arg, sceKernelGetThreadId() == mainThreadID ? "main thread" : "new thread");
	checkpoint("%s:  stack %x -> %x (%s stack)", extendTitle, mainThreadStackSize, getStackSize(mainThreadID), mainThreadStack == getStackAddr(mainThreadID) ? "same" : "different");

	return 0x12345678;
}

int inceptionThread(void *arg) {
	checkpoint("%s: inceptionThread %x (%s)", extendTitle, arg, sceKernelGetThreadId() == mainThreadID ? "main thread" : "new thread");
	checkpoint("%s:  stack %x -> %x (%s stack)", extendTitle, mainThreadStackSize, getStackSize(mainThreadID), mainThreadStack == getStackAddr(mainThreadID) ? "same" : "different");
	return sceKernelExtendThreadStack(0x8000, &someThread, (void *) 0xC0DE1C71);
}

int dummyThread(void *arg) {
	return 7;
}

void testExtend(const char *title, SceSize size, int func(void *), u32 arg) {
	extendTitle = title;
	checkpoint("%s result: %08x", extendTitle, sceKernelExtendThreadStack(size, func, (void *)arg));
}

u32 *get_k0()
{
	u32 ret = 0;
	asm volatile (
		"addi %0, $k0, 0\n"
		: "=r"(ret)
	);
	
	return (u32 *)ret;
}

SceKernelThreadInfo stackCheckInfo;
const char *stackCheckName;
int stackCheckFunc(void *arg) {
	stackCheckInfo.size = sizeof(stackCheckInfo);
	sceKernelReferThreadStatus(0, &stackCheckInfo);

	u32 *stack = (u32 *) stackCheckInfo.stack;
	u32 *stackEnd = stack + stackCheckInfo.stackSize / 4;
	u32 *k0 = get_k0();

	if (stack[0] != sceKernelGetThreadId()) {
		schedf("    %s: ERROR: stack should start with thread ID.\n", stackCheckName);
	}
	if (k0 < stack || k0 > stackEnd) {
		schedf("    %s: WARNING: k0 not inside stack.\n", stackCheckName);
	} else {
		if (stackEnd[-1] != 0xFFFFFFFF || stackEnd[-2] != 0xFFFFFFFF) {
			schedf("    %s: WARNING: k0 laid out differently?\n", stackCheckName);
		}
		if (stackEnd[-14] != (u32)stack) {
			schedf("    %s: WARNING: stack pointer not correct in k0.\n", stackCheckName);
		}
		if (stackEnd[-16] != sceKernelGetThreadId()) {
			schedf("    %s: WARNING: thread id not correct in k0.\n", stackCheckName);
		}
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

void testCheckStackLayout(const char *title, SceSize size, u32 arg) {
	stackCheckName = title;
	sceKernelExtendThreadStack(size, &stackCheckFunc, (void *)arg);

	checkpoint("%s", title);
}

int main(int argc, char *argv[]) {
	int i;

	mainThreadID = sceKernelGetThreadId();
	mainThreadStack = getStackAddr(mainThreadID);
	mainThreadStackSize = getStackSize(mainThreadID);

	testExtend("Normal", 0x4000, &someThread, 0xC0DEBEEF);
	testExtend("Recursive", 0x4000, &inceptionThread, 0xC0DEBEEF);

	checkpointNext("Sizes:");
	const static int sizes[] = {0, -1, 1, 32, 64, 128, 256, 384, 512, 640, 768, 1024};
	for (i = 0; i < ARRAY_SIZE(sizes); ++i) {
		char temp[16];
		snprintf(temp, sizeof(temp), "  %d", sizes[i]);
		testExtend(temp, sizes[i], &someThread, 0xC0DEBEEF);
	}

	// Crashes.
	//testExtend("NULL entry", 0x4000, NULL, 0xC0DEBEEF);

	checkpointNext("Stack layout:");
	testCheckStackLayout("0x1000", 0x1000, 0x1000);
	testCheckStackLayout("0x100000", 0x100000, 0x100000);
}
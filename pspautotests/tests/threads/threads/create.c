#include "shared.h"

void testCreate(const char *title, const char *name, SceKernelThreadEntry entry, int prio, int stack, SceUInt attr, SceKernelThreadOptParam *opt) {
	SceUID result = sceKernelCreateThread(name, entry, prio, stack, attr, opt);
	if (result > 0) {
		checkpoint(NULL);
		schedf("%s: ", title);
		schedfThreadStatus(result);
		sceKernelDeleteThread(result);
	} else {
		checkpoint("%s: Failed (%08X)", title, result);
	}
}

static int testFunc(SceSize argc, void *argv) {
	return 0;
}

int main(int argc, char **argv) {
	int i;
	char temp[32];

	checkpointNext("Names:");
	testCreate("  Normal", "create", &testFunc, 0x20, 0x10000, 0, NULL);
	testCreate("  NULL name", NULL, &testFunc, 0x20, 0x10000, 0, NULL);
	testCreate("  Blank name", "", &testFunc, 0x20, 0x10000, 0, NULL);
	testCreate("  Long name", "1234567890123456789012345678901234567890123456789012345678901234", &testFunc, 0x20, 0x10000, 0, NULL);

	checkpointNext("Attributes:");
	u32 attrs[] = {1, 0x10, 0x50, 0x100, 0x200, 0x400, 0x600, 0x800, 0xF00, 0x1000, 0x2000, 0x4000, 0x8000, 0x6000, 0xF000, 0x10000, 0x20000, 0x40000, 0x80000, 0xF0000, 0x100000, 0x200000, 0x400000, 0x800000, 0xF00000, 0x1000000, 0x2000000, 0x4000000, 0x8000000, 0xF000000, 0x10000000, 0x20000000, 0x40000000, 0x80000000, 0xF0000000};
	for (i = 0; i < sizeof(attrs) / sizeof(attrs[0]); ++i) {
		sprintf(temp, "  0x%x attr", attrs[i]);
		testCreate(temp, "create", &testFunc, 0x20, 0x10000, attrs[i], NULL);
	}

	checkpointNext("Stack size:");
	testCreate("  Zero stack", "create", &testFunc, 0x20, 0, 0, NULL);
	int stacks[] = {-1, 1, -0x10000, 0x100, 0x1FF, 0x200, 0x1000, 0x100000, 0x1000000, 0x1000001, 0x20000000};
	for (i = 0; i < sizeof(stacks) / sizeof(stacks[0]); ++i) {
		sprintf(temp, "  0x%08x stack", stacks[i]);
		testCreate(temp, "create", &testFunc, 0x20, stacks[i], 0, NULL);
	}

	checkpointNext("Priorities:");
	int prios[] = {0, 0x06, 0x07, 0x08, 0x77, 0x78, -1};
	for (i = 0; i < sizeof(prios) / sizeof(prios[0]); ++i) {
		sprintf(temp, "  0x%02x priority", prios[i]);
		testCreate(temp, "create", &testFunc, prios[i], 0x200, 0, NULL);
	}

	checkpointNext("Entry:");
	testCreate("  Null entry", "create", NULL, 0x20, 0x10000, 0, NULL);
	testCreate("  Invalid entry", "create", (SceKernelThreadEntry) 0xDEADBEEF, 0x20, 0x10000, 0, NULL);

	// TODO: Options?

	checkpointNext(NULL);
	SceUID thread1 = sceKernelCreateThread("create", &testFunc, 0x20, 0x1000, 0, NULL);
	SceUID thread2 = sceKernelCreateThread("create", &testFunc, 0x20, 0x1000, 0, NULL);
	if (thread1 > 0 && thread2 > 0) {
		checkpoint("Two with same name: OK");
	} else {
		checkpoint("Two with same name: Failed (%X, %X)", thread1, thread2);
	}
	sceKernelDeleteThread(thread1);
	sceKernelDeleteThread(thread2);

	checkpointNext(NULL);
	SceUID thread;
	BASIC_SCHED_TEST("NULL name",
		thread = sceKernelCreateThread(NULL, &testFunc, 0x20, 0x1000, 0, NULL);
		result = thread > 0 ? 1 : thread;
	);
	BASIC_SCHED_TEST("Priority 0x70",
		thread = sceKernelCreateThread("create", &testFunc, 0x70, 0x1000, 0, NULL);
		result = thread > 0 ? 1 : thread;
	);
	sceKernelDeleteThread(thread);
	BASIC_SCHED_TEST("Priority 0x08",
		thread = sceKernelCreateThread("create", &testFunc, 0x08, 0x1000, 0, NULL);
		result = thread > 0 ? 1 : thread;
	);
	sceKernelDeleteThread(thread);

	// This also serves to see if stack space is committed on create.
	SceUID threads[1024];
	int result = 0;
	for (i = 0; i < 1024; ++i) {
		threads[i] = sceKernelCreateThread("create", &testFunc, 0x20, 0x0100000, 0, NULL);
		if (threads[i] < 0) {
			result = threads[i];
			break;
		}
	}

	if (result != 0) {
		schedf("Create 1024: Failed at %d (%08X)\n", i, result);
	} else {
		schedf("Create 1024: OK\n");
	}

	while (--i >= 0) {
		sceKernelDeleteThread(threads[i]);
	}

	return 0;
}
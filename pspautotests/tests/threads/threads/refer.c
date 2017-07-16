#define sceKernelReferThreadStatus sceKernelReferThreadStatus_OLD

#include <common.h>
#include <sysmem-imports.h>

#undef sceKernelReferThreadStatus

typedef struct SceKernelThreadInfo2 {
	SceSize size;
	char name[32];
	SceUInt attr;
	int status;
	SceKernelThreadEntry entry;
	void * stack;
	int stackSize;
	void * gpReg;
	int initPriority;
	int currentPriority;
	int waitType;
	SceUID waitId;
	int wakeupCount;
	int exitStatus;
	SceKernelSysClock runClocks;
	SceUInt intrPreemptCount;
	SceUInt threadPreemptCount;
	SceUInt releaseCount;
	u32 unk;
} SceKernelThreadInfo2;

int sceKernelReferThreadStatus(SceUID threadID, SceKernelThreadInfo2 *info);

void schedfThreadInfo(SceKernelThreadInfo2 *info, SceKernelThreadEntry entry) {
	// TODO: Figure out what unk is.
	schedf("Thread %s (attr=%08x, status=%d, entry=%d, stack=%d, stacksize=%x, gp=%d, priority init=%x, cur=%x, wait=%d for %d, wakeup=%d, exit=%08x, unk=%08x)\n", info->name, info->attr, info->status, info->entry == entry, info->stack != 0, info->stackSize, info->gpReg != 0, info->initPriority, info->currentPriority, info->waitType, info->waitId, info->wakeupCount, info->exitStatus, info->unk);
	// TODO: It should be possible to get these values correct, but it'll probably be slower...
	//schedf("(clocks=%lld, intr pre=%d, thread pre=%d, release=%d)\n", *(u64 *)&info->runClocks, info->intrPreemptCount, info->threadPreemptCount, info->releaseCount);
}

int delayFunc(SceSize argc, void *argv) {
	schedf("* delayFunc\n");

	sceKernelDelayThread(500);

	return 7;
}

int slumberFunc(SceSize argc, void *argv) {
	sceKernelSleepThread();
	schedf(" ** slumber woke up\n");
	return 8;
}

int suicideFunc(SceSize argc, void *argv) {
	sceKernelExitDeleteThread(9);
	return 9;
}

void runReferTests() {
	SceKernelThreadInfo2 info;
	int i;
	SceUID delayThread = sceKernelCreateThread("delay", &delayFunc, sceKernelGetThreadCurrentPriority(), 0x1000, PSP_THREAD_ATTR_VFPU, NULL);
	SceUID deletedThread = sceKernelCreateThread("deleted", &delayFunc, sceKernelGetThreadCurrentPriority(), 0x1000, 0, NULL);
	sceKernelDeleteThread(deletedThread);

	info.size = sizeof(info);

	checkpointNext("Thread IDs:");
	checkpoint("  NULL: %08x", sceKernelReferThreadStatus(0, &info));
	checkpoint("  Current: %08x", sceKernelReferThreadStatus(sceKernelGetThreadId(), &info));
	checkpoint("  Deleted: %08x", sceKernelReferThreadStatus(deletedThread, &info));
	checkpoint("  Invalid: %08x", sceKernelReferThreadStatus(0xDEADBEEF, &info));

	// Crashes.
	//checkpointNext("sceKernelReferThreadStatus info ptr:");
	//checkpoint("  NULL info: %08x", sceKernelReferThreadStatus(0, NULL));

	checkpointNext("Sizes:");
	int sizes[] = {-1, 0, 1, 2, 3, 4, 5, 8, 16, 32, 64, 80, 82, 108, 128, 1024};
	for (i = 0; i < ARRAY_SIZE(sizes); ++i) {
		memset(&info, 0xff, sizeof(info));
		info.size = sizes[i];
		int result = sceKernelReferThreadStatus(0, &info);
		checkpoint("  %d: %08x => %d (exit: %x)", sizes[i], result, info.size, info.exitStatus);
	}

	info.size = sizeof(info);
	sceKernelStartThread(delayThread, 0, NULL);
	sceKernelReferThreadStatus(delayThread, &info);
	checkpointNext("Values:");
	schedfThreadInfo(&info, &delayFunc);

	SceUID slumberThread = sceKernelCreateThread("slumber", &slumberFunc, sceKernelGetThreadCurrentPriority() - 1, 0x1000, 0, NULL);

	checkpoint("  slumber before start:");
	sceKernelReferThreadStatus(slumberThread, &info);
	schedfThreadInfo(&info, &slumberFunc);

	sceKernelStartThread(slumberThread, 0, NULL);
	checkpoint("  started slumber");

	checkpoint("  slumber after start:");
	sceKernelReferThreadStatus(slumberThread, &info);
	schedfThreadInfo(&info, &slumberFunc);

	sceKernelTerminateThread(slumberThread);
	checkpoint("  terminated slumber");

	checkpoint("  slumber after terminate:");
	sceKernelReferThreadStatus(slumberThread, &info);
	schedfThreadInfo(&info, &slumberFunc);

	sceKernelStartThread(slumberThread, 0, NULL);
	checkpoint("  started slumber");

	checkpoint("  slumber after start:");
	sceKernelReferThreadStatus(slumberThread, &info);
	schedfThreadInfo(&info, &slumberFunc);

	checkpoint("  woke slumber: %08x", sceKernelWakeupThread(slumberThread));

	checkpoint("  slumber after wake:");
	sceKernelReferThreadStatus(slumberThread, &info);
	schedfThreadInfo(&info, &slumberFunc);

	sceKernelTerminateDeleteThread(slumberThread);
	checkpoint("  terminated and deleted slumber");

	// TODO: Test more cases.

	flushschedf();
}

int main(int argc, char *argv[]) {
	sceKernelSetCompiledSdkVersion(0x2060010);
	schedf("Until 2.60:\n");
	runReferTests();

	sceKernelSetCompiledSdkVersion(0x2060011);
	schedf("\n\nAfter 2.60:\n");
	runReferTests();

	return 0;
}
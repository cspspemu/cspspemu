#include "../sub_shared.h"

SETUP_SCHED_TEST;

SceUID mbx;

void schedf(const char *format, ...) {
	va_list args;
	va_start(args, format);
	schedulingLogPos += vsprintf(schedulingLog + schedulingLogPos, format, args);
	va_end(args);
}

static int threadFunction(SceSize argSize, void *argPointer) {
	int num = argPointer ? *((int*)argPointer) : 0;

	TestMbxMessage *msg = (TestMbxMessage *) 0xDEADBEEF;

	schedf("A%d\n", num);
	int result = sceKernelReceiveMbxCB(mbx, (void **) &msg, NULL);
	schedf("B%d\n", num);

	if (result == 0)
	{
		schedf("OK (ptr=%s) ", mbxPtrStatus(msg, mbx, NULL));
		if (msg != 0 && (uint) msg != 0xDEADBEEF) {
			schedf("GOT: \"%s\" (next=%s, prio=%02x) ", msg->text, mbxPtrStatus(msg->header.next, mbx, msg), msg->header.msgPriority);
		}
		schedf("\n");
	}
	else
		schedf("Failed: %08X\n", result);

	return 0;
}

static void execPriorityTests(int attr, int changePriority) {
	printf("For attr %08X%s:\n", attr, changePriority ? " and changed priorities" : "");

	schedulingLogPos = 0;

	SceUID threads[7];
	int test[7] = {1, 2, 3, 4, 5, 6, 7};

	mbx = sceKernelCreateMbx("mbx1", attr, NULL);
	PRINT_MBX(mbx);

	sendMbx(mbx, 0x20);
	sendMbx(mbx, 0x10);

	int i;
	for (i = 0; i < 7; i++) {
		threads[i] = CREATE_PRIORITY_THREAD(threadFunction, 0x18 - i);
		sceKernelStartThread(threads[i], sizeof(int), (void*)&test[i]);
	}

	sceKernelDelayThread(10 * 1000);

	// What we're gonna do now is change the threads' priorities to see whether
	// priority at time of wait (already happened) or at time of send matters.
	if (changePriority) {
		for (i = 0; i < 7; i++) {
			sceKernelChangeThreadPriority(threads[i], 0x18 - 7 + i);
		}
		printf("Priorities reversed.  Have a nice day.\n");
	}

	schedf("---\n");
	PRINT_MBX(mbx);
	schedf("---\n");
	sendMbx(mbx, 0x15);

	sceKernelDelayThread(10 * 1000);

	schedf("---\n");
	PRINT_MBX(mbx);
	schedf("---\n");

	sendMbx(mbx, 0x20);
	sendMbx(mbx, 0x10);

	sceKernelDelayThread(10 * 1000);

	schedf("---\n");
	PRINT_MBX(mbx);
	schedf("---\n");

	sceKernelDeleteMbx(mbx);
	schedf("\n\n");

	printf("%s", schedulingLog);
}

int main(int argc, char **argv) {
	execPriorityTests(0, 0);
	execPriorityTests(PSP_MBX_ATTR_PRIORITY, 0);
	execPriorityTests(PSP_MBX_ATTR_MSG_PRIORITY, 0);
	execPriorityTests(PSP_MBX_ATTR_PRIORITY | PSP_MBX_ATTR_MSG_PRIORITY, 0);

	execPriorityTests(PSP_MBX_ATTR_PRIORITY, 1);
	return 0;
}
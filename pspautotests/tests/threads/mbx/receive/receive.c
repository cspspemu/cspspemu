#include "../sub_shared.h"

SETUP_SCHED_TEST;

void waitSimple(const char *title, SceUID mbx, int getMsg) {
	SceKernelMbxInfo mbxinfo;
	mbxinfo.size = sizeof(SceKernelMbxInfo);
	TestMbxMessage *msg = (TestMbxMessage*) 0xDEADBEEF;

	int result = sceKernelReceiveMbx(mbx, getMsg ? (void **) &msg : 0, NULL);
	int inforesult = sceKernelReferMbxStatus(mbx, &mbxinfo);

	if (result == 0) {
		printf("%s: OK (ptr=%s) ", title, mbxPtrStatusInfo(msg, &mbxinfo, NULL));

		if (msg != 0 && (uint) msg != 0xDEADBEEF) {
			printf("GOT: \"%s\" (next=%s, prio=%02x) ", msg->text, mbxPtrStatusInfo(msg->header.next, &mbxinfo, msg), msg->header.msgPriority);
		}

		printMbxInfo(inforesult, &mbxinfo);
	} else {
		printf("%s: Failed (%08X, ptr=%s)\n", title, result, mbxPtrStatusInfo(msg, &mbxinfo, NULL));
	}
}

void waitSimpleTimeout(const char *title, SceUID mbx, SceUInt timeout, int getMsg) {
	SceKernelMbxInfo mbxinfo;
	mbxinfo.size = sizeof(SceKernelMbxInfo);
	TestMbxMessage *msg = (TestMbxMessage*) 0xDEADBEEF;

	int result = sceKernelReceiveMbx(mbx, getMsg ? (void **) &msg : 0, &timeout);
	int inforesult = sceKernelReferMbxStatus(mbx, &mbxinfo);

	if (result == 0) {
		printf("%s: OK (%dms left, ptr=%s) ", title, timeout, mbxPtrStatusInfo(msg, &mbxinfo, NULL));

		if (msg != 0 && (uint) msg != 0xDEADBEEF) {
			printf("GOT: \"%s\" (next=%s, prio=%02x) ", msg->text, mbxPtrStatusInfo(msg->header.next, &mbxinfo, msg), msg->header.msgPriority);
		}

		printMbxInfo(inforesult, &mbxinfo);
	} else {
		printf("%s: Failed (%08X, %dms left, ptr=%s)\n", title, result, timeout, mbxPtrStatusInfo(msg, &mbxinfo, NULL));
	}
}

static int waitTestFunc(SceSize argSize, void* argPointer) {
	TestMbxMessage *msg = nextMbxMsg();

	sceKernelDelayThread(1000);
	SCHED_LOG(C, 2);
	msg->header.msgPriority = 0x20;
	schedulingResult = sceKernelSendMbx(*(SceUID *) argPointer, (void *) msg);
	SCHED_LOG(D, 2);
	return 0;
}

static int deleteMeFunc(SceSize argSize, void* argPointer) {
	SceUID mbx = *(SceUID*) argPointer;
	TestMbxMessage *msg = (TestMbxMessage*) 0xDEADBEEF;
	int result = sceKernelReceiveMbx(mbx, (void **) &msg, NULL);

	printf("After delete: %08X (ptr=%s) ", result, mbxPtrStatus(msg, mbx, NULL));
	if (msg != 0 && (uint) msg != 0xDEADBEEF) {
		printf("GOT: \"%s\" (next=%s, prio=%02x)", msg->text, mbxPtrStatus(msg->header.next, mbx, msg), msg->header.msgPriority);
	}
	printf("\n");
	return 0;
}

void runWaitTests(uint attr) {
	printf("For attr %08X:\n", attr);

	SceUID mbx = sceKernelCreateMbx("wait1", attr, NULL);

	// Crashes
	//sendMbx(mbx, 0x20);
	//waitSimple("Single no ptr", mbx, 0);
	sendMbx(mbx, 0x20);
	waitSimple("Single standard", mbx, 1);

	sendMbx(mbx, 0x15);
	sendMbx(mbx, 0x05);
	waitSimple("Multiple standard #1", mbx, 1);
	waitSimple("Multiple standard #2", mbx, 1);

	waitSimpleTimeout("Empty no ptr", mbx, 500, 0);
	waitSimpleTimeout("Empty standard", mbx, 500, 1);
	sendMbx(mbx, 0x20);
	waitSimpleTimeout("Single standard", mbx, 500, 1);

	sendMbx(mbx, 0x15);
	sendMbx(mbx, 0x05);
	waitSimpleTimeout("Multiple standard #1", mbx, 500, 1);
	waitSimpleTimeout("Multiple standard #2", mbx, 500, 1);
	waitSimpleTimeout("Multiple standard #3", mbx, 500, 1);

	sceKernelDeleteMbx(mbx);
}

int main(int argc, char **argv) {
	runWaitTests(0);
	runWaitTests(PSP_MBX_ATTR_PRIORITY);
	runWaitTests(PSP_MBX_ATTR_MSG_PRIORITY);
	runWaitTests(PSP_MBX_ATTR_PRIORITY | PSP_MBX_ATTR_MSG_PRIORITY);

	// Sent off thread.
	SceUID mbx = sceKernelCreateMbx("wait1", 0, NULL);
	TestMbxMessage *msg;

	schedulingLogPos = 0;
	schedulingPlacement = 1;
	SCHED_LOG(A, 1);
	SceUInt timeout = 5000;
	SceUID thread = sceKernelCreateThread("waitTest", &waitTestFunc, 0x12, 0x10000, 0, NULL);
	sceKernelStartThread(thread, sizeof(mbx), &mbx);
	SCHED_LOG(B, 1);
	int result = sceKernelReceiveMbx(mbx, (void **) &msg, &timeout);
	SCHED_LOG(E, 1);
	printf("Wait timeout: %s (thread=%08X, main=%08X, remaining=%d)\n", schedulingLog, schedulingResult, result, (timeout + 15) / 1000);

	sceKernelDeleteMbx(mbx);
	sceKernelWaitThreadEnd(thread, NULL);
	sceKernelDeleteThread(thread);

	SceUID deleteThread = CREATE_SIMPLE_THREAD(deleteMeFunc);
	mbx = sceKernelCreateMbx("wait1", 0, NULL);
	sceKernelStartThread(deleteThread, sizeof(mbx), &mbx);
	sceKernelDelayThread(500);
	sceKernelDeleteMbx(mbx);

	timeout = 50000;
	sceKernelWaitThreadEnd(deleteThread, &timeout);

	printf("Cancel: ");
	mbx = sceKernelCreateMbx("wait1", 0, NULL);
	sceKernelStartThread(deleteThread, sizeof(mbx), &mbx);
	sceKernelDelayThread(500);
	sceKernelCancelReceiveMbx(mbx, NULL);

	timeout = 50000;
	sceKernelWaitThreadEnd(deleteThread, &timeout);
	sceKernelDeleteMbx(mbx);
	sceKernelDeleteThread(deleteThread);

	waitSimple("NULL", 0, 0);
	waitSimple("Invalid", 0xDEADBEEF, 0);
	waitSimple("Deleted", mbx, 0);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelReceiveMbx(0, NULL, NULL);
	);
	BASIC_SCHED_TEST("Other timeout",
		timeout = 100;
		result = sceKernelReceiveMbx(mbx2, (void **) &msg, &timeout);
	);
	BASIC_SCHED_TEST("Same timeout",
		timeout = 100;
		result = sceKernelReceiveMbx(mbx1, (void **) &msg, &timeout);
	);
	BASIC_SCHED_TEST("Other timeout (5ms)",
		timeout = 5;
		result = sceKernelReceiveMbx(mbx2, (void **) &msg, &timeout);
	);
	BASIC_SCHED_TEST("Same timeout (5ms)",
		timeout = 5;
		result = sceKernelReceiveMbx(mbx2, (void **) &msg, &timeout);
	);

	return 0;
}
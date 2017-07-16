#include "../sub_shared.h"

SETUP_SCHED_TEST;

TestMbxMessage *sendTestMbx(const char *title, SceUID mbx, TestMbxMessage *msg) {
	int result = sceKernelSendMbx(mbx, msg);
	if (result == 0) {
		printf("%s: OK\n", title);
	} else {
		printf("%s: Failed (%X)\n", title, result);
	}
	PRINT_MBX(mbx);

	return msg;
}

TestMbxMessage *sendTest(const char *title, SceUID mbx) {
	TestMbxMessage *msg = nextMbxMsg();
	return sendTestMbx(title, mbx, msg);
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

int main(int argc, char **argv) {
	SceUID mbx = sceKernelCreateMbx("set", 0, NULL);
	PRINT_MBX(mbx);

	sendTest("Empty", mbx);

	TestMbxMessage *modified = sendTest("Modified", mbx);
	modified->header.next = (SceKernelMsgPacket *) modified;
	PRINT_MBX(mbx);

	waitSimpleTimeout("Receive tampered #1", mbx, 5000, 1);
	PRINT_MBX(mbx);
	waitSimpleTimeout("Receive tampered #2", mbx, 5000, 1);

	sceKernelDeleteMbx(mbx);

	mbx = sceKernelCreateMbx("set", 0, NULL);
	PRINT_MBX(mbx);

	sendTest("Empty", mbx);

	modified = sendTest("Modified B", mbx);
	modified->header.next = NULL;
	PRINT_MBX(mbx);

	waitSimpleTimeout("Receive tampered B #1", mbx, 5000, 1);
	waitSimpleTimeout("Receive tampered B #2", mbx, 5000, 1);

	sceKernelDeleteMbx(mbx);

	mbx = sceKernelCreateMbx("set", 0, NULL);
	PRINT_MBX(mbx);

	modified = sendTest("Modified C", mbx);

	// This is evil.
	TestMbxMessage *extra = nextMbxMsg();
	modified->header.next = (SceKernelMsgPacket *) extra;
	extra->header.next = (SceKernelMsgPacket *) modified;
	PRINT_MBX(mbx);

	waitSimpleTimeout("Receive tampered C #1", mbx, 5000, 1);
	waitSimpleTimeout("Receive tampered C #2", mbx, 5000, 1);

	sceKernelDeleteMbx(mbx);

	mbx = sceKernelCreateMbx("set", 0, NULL);

	TestMbxMessage *msg = nextMbxMsg();
	sendTestMbx("Send twice #1", mbx, msg);
	sendTestMbx("Send twice #2", mbx, msg);

	waitSimpleTimeout("Receive twice #1", mbx, 5000, 1);
	waitSimpleTimeout("Receive twice #2", mbx, 5000, 1);

	// Crashes.
	//sendTestMbx("Send NULL", mbx, NULL);

	sceKernelDeleteMbx(mbx);

	sendTest("NULL", 0);
	sendTest("Invalid", 0xDEADBEEF);
	sendTest("Deleted", mbx);

	msg = nextMbxMsg();
	BASIC_SCHED_TEST("NULL",
		result = sceKernelSendMbx(0, 0);
	);
	BASIC_SCHED_TEST("Other",
		result = sceKernelSendMbx(mbx2, &msg);
	);
	BASIC_SCHED_TEST("Same",
		result = sceKernelSendMbx(mbx1, &msg);
	);

	return 0;
}
#include "../sub_shared.h"

SETUP_SCHED_TEST;

void pollMbx(const char *title, SceUID mbx, int getMsg) {
	SceKernelMbxInfo mbxinfo;
	mbxinfo.size = sizeof(SceKernelMbxInfo);
	TestMbxMessage *msg = (TestMbxMessage*) 0xDEADBEEF;

	int result = sceKernelPollMbx(mbx, getMsg ? (void **) &msg : 0);
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

inline void sendMbxDummy(SceUID mbx, int prio) {
	TestMbxMessage *msg = nextMbxMsg();

	*((int *) &msg->header.msgPriority) = prio;
	sceKernelSendMbx(mbx, (void *) msg);
}

void runPollTests(uint attr) {
	printf("For attr %08X:\n", attr);

	SceUID mbx = sceKernelCreateMbx("poll1", attr, NULL);

	pollMbx("Empty no ptr", mbx, 0);
	pollMbx("Empty standard", mbx, 1);

	sendMbx(mbx, 0x01);
	// Crashes.
	//pollMbx("Single no ptr", mbx, 0);
	pollMbx("Single standard", mbx, 1);

	sendMbx(mbx, 0x15);
	sendMbx(mbx, 0x05);
	sendMbx(mbx, 0x10);
	pollMbx("Multiple standard #1", mbx, 1);
	pollMbx("Multiple standard #2", mbx, 1);
	pollMbx("Multiple standard #3", mbx, 1);
	pollMbx("Multiple standard #4", mbx, 1);

	sceKernelDeleteMbx(mbx);
}

int main(int argc, char **argv) {
	runPollTests(0);
	runPollTests(PSP_MBX_ATTR_PRIORITY);
	runPollTests(PSP_MBX_ATTR_MSG_PRIORITY);
	runPollTests(PSP_MBX_ATTR_PRIORITY | PSP_MBX_ATTR_MSG_PRIORITY);

	SceUID mbx = sceKernelCreateMbx("poll1", PSP_MBX_ATTR_MSG_PRIORITY, NULL);

	TestMbxMessage *sent = sendMbx(mbx, 0x15);
	sprintf(sent->text, "modify");
	pollMbx("Modified text", mbx, 1);

	TestMbxMessage *sent1 = sendMbx(mbx, 0x15);
	TestMbxMessage *sent2 = sendMbx(mbx, 0x05);
	TestMbxMessage *sent3 = sendMbx(mbx, 0x10);
	sent1->header.msgPriority = 0x10;
	sent3->header.msgPriority = 0x15;
	pollMbx("Modified priority #1", mbx, 1);
	pollMbx("Modified priority #2", mbx, 1);
	pollMbx("Modified priority #3", mbx, 1);
	pollMbx("Modified priority #4", mbx, 1);

	sendMbxDummy(mbx, 0x1510);
	sendMbxDummy(mbx, 0x0510);
	sendMbxDummy(mbx, 0x1010);
	pollMbx("uint16 priority #1", mbx, 1);
	pollMbx("uint16 priority #2", mbx, 1);
	pollMbx("uint16 priority #3", mbx, 1);
	pollMbx("uint16 priority #4", mbx, 1);

	sceKernelDeleteMbx(mbx);

	pollMbx("NULL", 0, 1);
	pollMbx("Invalid", 0xDEADBEEF, 1);
	pollMbx("Deleted", mbx, 1);

	TestMbxMessage *msg;
	BASIC_SCHED_TEST("NULL",
		result = sceKernelPollMbx(0, (void **) &msg);
	);
	BASIC_SCHED_TEST("Other",
		result = sceKernelPollMbx(mbx2, (void **) &msg);
	);
	BASIC_SCHED_TEST("Same",
		result = sceKernelPollMbx(mbx1, (void **) &msg);
	);

	return 0;
}
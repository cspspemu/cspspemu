#include "../sub_shared.h"

SETUP_SCHED_TEST;

#define REFER_TEST(title, mbx, mbxinfo) { \
	int result = sceKernelReferMbxStatus(mbx, mbxinfo); \
	if (result == 0) { \
		printf("%s: OK\n", title); \
		printMbxInfo(result, mbxinfo); \
	} else { \
		printf("%s: Failed (%X)\n", title, result); \
	} \
}

int main(int argc, char **argv) {
	SceUID mbx = sceKernelCreateMbx("refer1", 0, NULL);
	SceKernelMbxInfo mbxinfo;
	mbxinfo.size = sizeof(mbxinfo);

	// Crashes.
	//REFER_TEST("NULL info", mbx, NULL);
	REFER_TEST("Normal", mbx, &mbxinfo);

	int sizes[] = {0, 1, 2, 4, 8, 44, 48, 52, 56, 64, 128, 256, 1024, 0xFF00, 0xFF0000, 0x80000001, 0xCCCCCCCC, 0xFFFFFFFF};
	int numSizes = sizeof(sizes) / sizeof(sizes[0]);

	int i, result;
	for (i = 0; i < numSizes; ++i) {
		mbxinfo.size = sizes[i];
		result = sceKernelReferMbxStatus(mbx, &mbxinfo);
		printf("Size %08X => %08X (result=%08X)\n", sizes[i], mbxinfo.size, result);
	}

	// Evil stuff.
	TestMbxMessage *msg1 = sendMbx(mbx, 0);
	TestMbxMessage *msg2 = sendMbx(mbx, 0);
	msg2->header.next = (SceKernelMsgPacket *) msg2;
	REFER_TEST("Modified A", mbx, &mbxinfo);

	msg2->header.next = NULL;
	REFER_TEST("Modified B", mbx, &mbxinfo);

	TestMbxMessage *msg3 = sendMbx(mbx, 0);
	REFER_TEST("Modified C", mbx, &mbxinfo);

	sceKernelDeleteMbx(mbx);

	REFER_TEST("NULL", 0, &mbxinfo);
	REFER_TEST("Invalid", 0xDEADBEEF, &mbxinfo);
	REFER_TEST("Deleted", mbx, &mbxinfo);

	BASIC_SCHED_TEST("NULL",
		result = sceKernelReferMbxStatus(0, &mbxinfo);
	);
	BASIC_SCHED_TEST("Refer other",
		result = sceKernelReferMbxStatus(mbx2, &mbxinfo);
	);
	BASIC_SCHED_TEST("Refer same",
		result = sceKernelReferMbxStatus(mbx1, &mbxinfo);
	);

	return 0;
}
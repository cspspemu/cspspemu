#include <common.h>

#include <pspsdk.h>
#include <pspkernel.h>
#include <pspthreadman.h>
#include <psploadexec.h>

typedef struct _TestMbxMessage {
	SceKernelMsgPacket header;
	char text[16];
 } TestMbxMessage;

inline const char *mbxPtrStatusInfo(void *ptr, SceKernelMbxInfo *mbxinfo, void *itself) {
	if (ptr == 0) {
		return "NULL";
	} else if ((uint) ptr == 0xDEADBEEF) {
		return "DEAD";
	} else if (ptr == itself) {
		return "ITSELF";
	} else if (mbxinfo != NULL && ptr == mbxinfo->firstMessage) {
		return "FIRST";
	} else {
		return "OTHER";
	}
}

inline const char *mbxPtrStatus(void *ptr, SceUID mbx, void *itself) {
	SceKernelMbxInfo mbxinfo;
	sceKernelReferMbxStatus(mbx, &mbxinfo);

	return mbxPtrStatusInfo(ptr, &mbxinfo, itself);
}

#define NUM_MSGS 256
TestMbxMessage testMsgs[NUM_MSGS];
int testMsg = 0;

inline TestMbxMessage *nextMbxMsg() {
	if (testMsg >= NUM_MSGS) {
		testMsg = 0;
		printf("TEST FAILURE\n");
	}

	testMsgs[testMsg].header.next = (SceKernelMsgPacket *) 0xDEADBEEF;
	sprintf(testMsgs[testMsg].text, "hi %d", testMsg);

	return &testMsgs[testMsg++];
}

inline TestMbxMessage *sendMbx(SceUID mbx, char prio) {
	TestMbxMessage *msg = nextMbxMsg();

	msg->header.msgPriority = prio;
	sceKernelSendMbx(mbx, (void *) msg);

	return msg;
}

inline void printMbxInfo(int result, SceKernelMbxInfo *mbxinfo) {
	if (result == 0) {
		printf("Messagebox: OK (size=%d,name='%s',attr=%d,wait=%d,count=%d,first=%s)\n", mbxinfo->size, mbxinfo->name, mbxinfo->attr, mbxinfo->numWaitThreads, mbxinfo->numMessages, mbxPtrStatusInfo(mbxinfo->firstMessage, NULL, NULL));

		TestMbxMessage *packet = (TestMbxMessage *) mbxinfo->firstMessage;
		int recur = 0;
		while (packet != 0 && (uint) packet != 0xDEADBEEF && ++recur < 20) {
			printf("  %s prio=%02X next=%s", packet->text, (uint) packet->header.msgPriority, mbxPtrStatusInfo(packet->header.next, mbxinfo, packet));
			packet = (TestMbxMessage *) packet->header.next;

			// Seems they loop.
			if (packet == mbxinfo->firstMessage) {
				break;
			}
		}
		if (mbxinfo->firstMessage != 0) {
			printf("\n");
		}
	} else {
		printf("Messagebox: Invalid (%08X)\n", result);
	}
}

inline void PRINT_MBX(SceUID mbx) {
	if (mbx > 0) {
		SceKernelMbxInfo mbxinfo;
		mbxinfo.size = sizeof(mbxinfo);
		int result = sceKernelReferMbxStatus(mbx, &mbxinfo);
		printMbxInfo(result, &mbxinfo);
	} else {
		printf("Messagebox: Failed (%08X)\n", mbx);
	}
}

#define PSP_MBX_ATTR_PRIORITY 0x100
#define PSP_MBX_ATTR_MSG_PRIORITY 0x400

#define CREATE_PRIORITY_THREAD(func, priority) \
	sceKernelCreateThread(#func, &func, priority, 0x10000, 0, NULL)
#define CREATE_SIMPLE_THREAD(func) CREATE_PRIORITY_THREAD(func, 0x12)

// Log a checkpoint and which thread was last active.
#define SCHED_LOG(letter, placement) { \
	int old = schedulingPlacement; \
	schedulingPlacement = placement; \
	schedulingLogPos += sprintf(schedulingLog + schedulingLogPos, #letter "%d", old); \
}

// Avoid linking or other things.
#define SETUP_SCHED_TEST \
	/* Keep track of the last thread we saw here. */ \
	static volatile int schedulingPlacement = 0; \
	/* So we can log the result from the thread. */ \
	static int schedulingResult = -1; \
	/* printf() seems to reschedule, so can't use it. */ \
	static char schedulingLog[8192]; \
	static volatile int schedulingLogPos = 0; \
	\
	static int scheduleTestFunc(SceSize argSize, void* argPointer) { \
		int result = 0x800201A8; \
		void *msg; \
		SceUInt timeout; \
		schedulingResult = -1; \
		\
		SCHED_LOG(B, 2); \
		/* Constantly loop setting the placement to 2 whenever we're active. */ \
		while (result == 0x800201A8) { \
			schedulingPlacement = 2; \
			timeout = 1; \
			result = sceKernelReceiveMbxCB(*(int*) argPointer, &msg, &timeout); \
		} \
		SCHED_LOG(D, 2); \
		\
		schedulingResult = result; \
		return 0; \
	}

#define LOCKED_SCHED_TEST(title, attr1, attr2, x) { \
	SceUID thread = CREATE_SIMPLE_THREAD(scheduleTestFunc); \
	SceUID mbx1 = sceKernelCreateMbx("schedTest1", attr1, NULL); \
	SceUID mbx2 = sceKernelCreateMbx("schedTest2", attr2, NULL); \
	int result = -1; \
	\
	schedulingLogPos = 0; \
	schedulingPlacement = 1; \
	printf("%s: ", title); \
	\
	SCHED_LOG(A, 1); \
	sceKernelStartThread(thread, sizeof(int), &mbx1); \
	SCHED_LOG(C, 1); \
	x \
	SCHED_LOG(E, 1); \
	sceKernelDeleteMbx(mbx1); \
	SCHED_LOG(F, 1); \
	\
	schedulingLog[schedulingLogPos] = 0; \
	schedulingLogPos = 0; \
	printf("%s (thread=%08X, main=%08X)\n", schedulingLog, schedulingResult, result); \
	sceKernelTerminateThread(thread); \
	sceKernelDeleteMbx(mbx2); \
}
#define BASIC_SCHED_TEST(title, x) LOCKED_SCHED_TEST(title, 0, 0, x);

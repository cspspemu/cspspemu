#include "shared.h"

inline void testCancel(const char *title, SceUID msgpipe, bool doSend = true, bool doRecv = true) {
	int send = 1337, recv = 1337;
	int result = sceKernelCancelMsgPipe(msgpipe, doSend ? &send : NULL, doRecv ? &recv : NULL);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: OK (sending=%d, receiving=%d) ", title, send, recv);
		schedfMsgPipe(msgpipe);
	} else {
		checkpoint("%s: Failed (%08x, sending=%d, receiving=%d)", title, result, send, recv);
	}
}

extern "C" int main(int argc, char *argv[]) {
	SceUID msgpipe = sceKernelCreateMsgPipe("cancel", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);

	checkpointNext("Objects:");
	testCancel("  Normal", msgpipe);
	testCancel("  NULL", 0);
	testCancel("  Invalid", 0xDEADBEEF);
	testCancel("  Already canceled", msgpipe);
	sceKernelDeleteMsgPipe(msgpipe);
	testCancel("  Deleted", msgpipe);

	checkpointNext("Counts:");
	msgpipe = sceKernelCreateMsgPipe("cancel", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
	testCancel("  No send", msgpipe, false, true);
	testCancel("  No receive", msgpipe, true, false);
	testCancel("  Neither", msgpipe, false, false);
	sceKernelDeleteMsgPipe(msgpipe);

	checkpointNext("Waits:");
	{
		msgpipe = sceKernelCreateMsgPipe("cancel", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		MsgPipeReceiveWaitThread wait_r1("receiving thread 1", msgpipe, NO_TIMEOUT);
		MsgPipeReceiveWaitThread wait_r2("receiving thread 2", msgpipe, 10000);
		testCancel("  With receiving threads", msgpipe);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	{
		msgpipe = sceKernelCreateMsgPipe("cancel", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		// Send something to fill it up.
		char msg[256];
		sceKernelSendMsgPipe(msgpipe, msg, sizeof(msg), 0, NULL, NULL);
		MsgPipeSendWaitThread wait_s1("sending thread 1", msgpipe, NO_TIMEOUT);
		MsgPipeSendWaitThread wait_s2("sending thread 2", msgpipe, 10000);
		testCancel("  With sending threads", msgpipe);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	return 0;
}
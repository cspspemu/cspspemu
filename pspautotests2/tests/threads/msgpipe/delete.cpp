#include "shared.h"

inline void testDelete(const char *title, SceUID msgpipe) {
	int result = sceKernelDeleteMsgPipe(msgpipe);
	if (result == 0) {
		checkpoint("%s: OK", title);
	} else {
		checkpoint("%s: Failed (%08x)", title, result);
	}
}

extern "C" int main(int argc, char *argv[]) {
	SceUID msgpipe = sceKernelCreateMsgPipe("delete", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);

	testDelete("Normal", msgpipe);
	testDelete("NULL", 0);
	testDelete("Invalid", 0xDEADBEEF);
	testDelete("Deleted", msgpipe);

	{
		msgpipe = sceKernelCreateMsgPipe("delete", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		MsgPipeReceiveWaitThread wait_r1("receiving thread 1", msgpipe, NO_TIMEOUT);
		MsgPipeReceiveWaitThread wait_r2("receiving thread 2", msgpipe, 10000);
		checkpoint("With receiving threads: %08x", sceKernelDeleteMsgPipe(msgpipe));
	}

	{
		msgpipe = sceKernelCreateMsgPipe("delete", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		// Send something to fill it up.
		char msg[256];
		sceKernelSendMsgPipe(msgpipe, msg, sizeof(msg), 0, NULL, NULL);
		MsgPipeSendWaitThread wait_s1("sending thread 1", msgpipe, NO_TIMEOUT);
		MsgPipeSendWaitThread wait_s2("sending thread 2", msgpipe, 10000);
		checkpoint("With sending threads: %08x", sceKernelDeleteMsgPipe(msgpipe));
	}

	return 0;
}
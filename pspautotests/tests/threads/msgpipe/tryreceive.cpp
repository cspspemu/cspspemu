#include "shared.h"

inline void testReceive(const char *title, SceUID msgpipe, void *buf, SceSize bufsize, int mode, bool doBytes = true) {
	int bytes = 1337;
	int result = sceKernelTryReceiveMsgPipe(msgpipe, buf, bufsize, mode, doBytes ? &bytes : NULL);
	if (result == 0) {
		checkpoint(NULL);
		schedf("%s: OK (bytes=%d) ", title, doBytes ? bytes : -1);
		schedfMsgPipe(msgpipe);
	} else {
		checkpoint(NULL);
		schedf("%s: Failed (%08x, bytes=%d) ", title, result, doBytes ? bytes : -1);
		schedfMsgPipe(msgpipe);
	}
}

inline int fillBuffer(SceUID msgpipe, int size) {
	char *temp = new char[size];
	return sceKernelSendMsgPipe(msgpipe, temp, size, 1, NULL, NULL);
	delete [] temp;
}

extern "C" int main(int argc, char *argv[]) {
	SceUID msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
	char *temp = new char[0x1000];

	checkpointNext("Objects:");
	fillBuffer(msgpipe, 0x1000);
	testReceive("  Normal", msgpipe, temp, 0x100, 0);
	testReceive("  NULL", 0, temp, 0x100, 0);
	testReceive("  Invalid", 0xDEADBEEF, temp, 0x100, 0);
	sceKernelDeleteMsgPipe(msgpipe);
	testReceive("  Deleted", msgpipe, temp, 0x100, 0);

	msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x1000, NULL);
	fillBuffer(msgpipe, 0x1000);
	checkpointNext("Buffers:");
	testReceive("  0 length", msgpipe, temp, 0, 0);
	testReceive("  NULL and 0 length", msgpipe, NULL, 0, 0);
	// Crashes.
	//testReceive("  NULL with length", msgpipe, NULL, 0x100, 0);
	testReceive("  Unaligned", msgpipe, temp, 1, 0);
	testReceive("  Bigger than buffer", msgpipe, temp, 0x2000, 0);
	testReceive("  Larger (0x10000)", msgpipe, temp, 0x10000, 0);
	testReceive("  Larger (0x40000000)", msgpipe, temp, 0x40000000, 0);
	testReceive("  Negative", msgpipe, temp, -1, 0);
	testReceive("  Fill", msgpipe, temp, 0xFFF, 0);
	testReceive("  Timeout", msgpipe, temp, 0x100, 0);
	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	fillBuffer(msgpipe, 0x1000);
	testReceive("  Without bytes", msgpipe, temp, 0x100, 0, false);

	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	fillBuffer(msgpipe, 0x1000);
	checkpointNext("ASAP mode:");
	testReceive("  ASAP", msgpipe, temp, 0x100, 1);
	testReceive("  ASAP partial", msgpipe, temp, 0x1000, 1);
	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	fillBuffer(msgpipe, 0x1000);
	testReceive("  ASAP too big", msgpipe, temp, 0x2000, 1);
	testReceive("  ASAP no bytes", msgpipe, temp, 0x100, 1, false);
	testReceive("  ASAP partial no bytes", msgpipe, temp, 0x1000, 1, false);

	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	fillBuffer(msgpipe, 0x1000);
	checkpointNext("Other modes:");
	const static int modes[] = {-2, -1, 2, 3, 4, 5, 6, 7, 8, 9, 0x101, 0x1001, 0x10001};
	for (size_t i = 0; i < ARRAY_SIZE(modes); ++i) {
		char temp[32];
		snprintf(temp, sizeof(temp), "  Mode %d", modes[i]);
		testReceive(temp, msgpipe, temp, 0x10, modes[i]);
	}

	sceKernelDeleteMsgPipe(msgpipe);
	msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0, NULL);
	checkpointNext("Buffers (pipe size = 0):");
	testReceive("  0 length", msgpipe, temp, 0, 0);
	testReceive("  NULL and 0 length", msgpipe, NULL, 0, 0);
	// Crashes.
	//testReceive("  NULL with length", msgpipe, NULL, 0x100, 0);
	testReceive("  Unaligned", msgpipe, temp, 1, 0);
	testReceive("  Bigger than buffer", msgpipe, temp, 0x2000, 0);
	testReceive("  Negative", msgpipe, temp, -1, 0);
	testReceive("  Fill", msgpipe, temp, 0xFFF, 0);
	testReceive("  Timeout", msgpipe, temp, 0x100, 0);
	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	testReceive("  Without bytes", msgpipe, temp, 0x100, 0, false);

	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	checkpointNext("ASAP mode (pipe size = 0):");
	testReceive("  ASAP", msgpipe, temp, 0x100, 1);
	testReceive("  ASAP partial", msgpipe, temp, 0x1000, 1);
	sceKernelCancelMsgPipe(msgpipe, NULL, NULL);
	testReceive("  ASAP too big", msgpipe, temp, 0x2000, 1);
	testReceive("  ASAP no bytes", msgpipe, temp, 0x100, 1, false);
	testReceive("  ASAP partial no bytes", msgpipe, temp, 0x1000, 1, false);
	sceKernelDeleteMsgPipe(msgpipe);

	checkpointNext("With two waiting:");
	{
		msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		MsgPipeSendWaitThread wait_s1("sending thread 1", msgpipe, 10000);
		MsgPipeSendWaitThread wait_s2("sending thread 2", msgpipe, 10000);
		schedfMsgPipe(msgpipe);
		testReceive("  With sending threads", msgpipe, temp, 0x100, 0);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("With partial receive timeout:");
	{
		msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0x100, NULL);
		MsgPipeSendWaitThread wait_s1("sending thread 1", msgpipe, 10000, 0x080);
		MsgPipeSendWaitThread wait_s2("sending thread 2", msgpipe, 10000, 0x100);
		schedfMsgPipe(msgpipe);
		testReceive("  With sending threads", msgpipe, temp, 0x100, 0);
		sceKernelDelayThread(10000);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	checkpointNext("Without a buffer:");
	{
		msgpipe = sceKernelCreateMsgPipe("msgpipe", PSP_MEMORY_PARTITION_USER, 0, 0, NULL);
		MsgPipeSendWaitThread wait_s1("sending thread 1", msgpipe, 10000);
		MsgPipeSendWaitThread wait_s2("sending thread 2", msgpipe, 10000);
		schedfMsgPipe(msgpipe);
		testReceive("  Partial packet", msgpipe, temp, 0x080, 0);
		testReceive("  Complete packet", msgpipe, temp, 0x100, 0);
		sceKernelDeleteMsgPipe(msgpipe);
	}

	delete [] temp;
	return 0;
}